using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PostIQ.Core.BackgroundProcess.Configuration;
using PostIQ.Core.BackgroundProcess.Interfaces;
using System.Threading.Channels;
using Cronos;

namespace PostIQ.Core.BackgroundProcess.Services
{
    /// <summary>
    /// Provides a base implementation for a background job hosted service that manages the production, queuing, and
    /// processing of items of type <typeparamref name="TItem"/> using configurable scheduling and concurrency options.
    /// </summary>
    /// <remarks>This abstract class is intended for use in scenarios where background jobs require item
    /// production and processing with support for queueing, scheduling (interval or cron-based), and configurable
    /// concurrency. Derived classes can customize job settings and processing logic by overriding relevant methods. The
    /// service integrates with dependency injection and logging, and registers itself with a job registry for external
    /// control and monitoring. Thread safety is ensured for queue operations. The job can be disabled via
    /// configuration, and queue behavior is controlled by settings such as queue size and full mode.</remarks>
    /// <typeparam name="TItem">The type of item to be produced and processed by the background job.</typeparam>
    public abstract class BaseBackgroundJobHostedService<TItem> : BackgroundService
    {
        private readonly string _jobName;
        private readonly JobSettings _settings;
        private readonly IJobItemsProducer<TItem> _producer;
        private readonly IJobItemProcessor<TItem> _processor;
        private readonly IBackgroundJobRegistry _registry;
        private readonly ILogger _logger;

        private Channel<TItem>? _channel;
        private ChannelWriter<TItem>? _writer;
        private ChannelReader<TItem>? _reader;

        private int _queueDepth;

        protected BaseBackgroundJobHostedService(
            string jobName,
            IOptions<BackgroundJobsConfiguration> configuration,
            ILogger logger,
            IJobItemsProducer<TItem> producer,
            IJobItemProcessor<TItem> processor,
            IBackgroundJobRegistry registry)
        {
            _jobName = jobName;
            _logger = logger;
            _producer = producer;
            _processor = processor;
            _registry = registry;

            _settings = configuration.Value?.GetJobSettings(jobName) ?? new JobSettings();
        }

        protected string JobName => _jobName;

        protected virtual JobSettings GetEffectiveSettings() => _settings;

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            var settings = GetEffectiveSettings();
            if (!settings.Enabled)
            {
                _logger.LogInformation("Background job {JobName} is disabled.", _jobName);
                return;
            }

            var opts = new BoundedChannelOptions(Math.Max(1, settings.QueueSize))
            {
                FullMode = ParseFullMode(settings.FullMode),
                SingleReader = false,
                SingleWriter = true,
                AllowSynchronousContinuations = false
            };

            _channel = Channel.CreateBounded<TItem>(opts);
            _writer = _channel.Writer;
            _reader = _channel.Reader;

            _registry.Register(
                _jobName,
                RunProducerOnceAsync,
                async (item, ct) => await EnqueueItemInternalAsync((TItem)item!, ct),
                GetQueueDepthAsync);

            await base.StartAsync(cancellationToken);

            _logger.LogInformation(
                "Background job {JobName} started. QueueSize={QueueSize}, MaxConsumers={MaxConsumers}",
                _jobName, settings.QueueSize, settings.MaxConcurrentConsumers);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _writer?.Complete();
            await base.StopAsync(cancellationToken);
            _logger.LogInformation("Background job {JobName} stopped.", _jobName);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var settings = GetEffectiveSettings();
            if (!settings.Enabled) return;

            var consumerTasks = new Task[Math.Max(1, settings.MaxConcurrentConsumers)];
            for (int i = 0; i < consumerTasks.Length; i++)
                consumerTasks[i] = ConsumeAsync(stoppingToken);

            Task? schedulerTask = null;

            //Scheduler: either corn of fixed interval based on settings.Schedule
            if (!string.IsNullOrWhiteSpace(settings.Schedule))
            {
                var cronExpression = ScheduleInterval.ToCronExpression(settings.Schedule);
                if (!string.IsNullOrEmpty(cronExpression))
                    schedulerTask = RunCronSchedulerAsync(cronExpression, stoppingToken);
                else
                {
                    var interval = ScheduleInterval.ToTimeSpan(settings.Schedule);
                    if (interval.HasValue)
                        schedulerTask = RunIntervalSchedulerAsync(interval.Value, stoppingToken);
                }
            }

            if (schedulerTask != null)
                await schedulerTask;
            else
                await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        protected virtual async Task RunProducerOnceAsync(CancellationToken cancellationToken)
        {
            var settings = GetEffectiveSettings();

            int capacity = Math.Max(1, settings.QueueSize);
            int current = Interlocked.CompareExchange(ref _queueDepth, 0, 0);
            int available = Math.Max(0, capacity - current);

            if (available == 0)
            {
                _logger.LogDebug("Job {JobName}: queue full, skipping producer run.", _jobName);
                return;
            }

            var items = await _producer.GetItemsToProcessAsync(available, cancellationToken);
            if (items == null || items.Count == 0) return;

            foreach (var item in items)
            {
                if (cancellationToken.IsCancellationRequested) break;

                try
                {
                    await _writer!.WriteAsync(item, cancellationToken);
                    Interlocked.Increment(ref _queueDepth);
                }
                catch (ChannelClosedException) { break; }
            }
        }

        private async Task EnqueueItemInternalAsync(TItem item, CancellationToken cancellationToken)
        {
            try
            {
                await _writer!.WriteAsync(item, cancellationToken);
                Interlocked.Increment(ref _queueDepth);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Job {JobName}: failed to enqueue single item.", _jobName);
                throw;
            }
        }

        private async Task ConsumeAsync(CancellationToken stoppingToken)
        {
            try
            {
                await foreach (var item in _reader!.ReadAllAsync(stoppingToken))
                {
                    Interlocked.Decrement(ref _queueDepth);
                    try
                    {
                        await _processor.ProcessItemAsync(item, stoppingToken);
                    }
                    catch (OperationCanceledException) { break; }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Job {JobName}: error processing item {Item}.", _jobName, item);
                    }
                }
            }
            catch (OperationCanceledException) 
            {
                //expected on shutdown, no need to log
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Job {JobName}: consumer loop error.", _jobName);
            }
        }

        private async Task RunIntervalSchedulerAsync(TimeSpan interval, CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(interval);
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try 
                { 
                    await RunProducerOnceAsync(stoppingToken); 
                }
                catch (OperationCanceledException) 
                { 
                    break; 
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Job {JobName}: interval producer run failed.", _jobName);
                }
            }
        }

        private async Task RunCronSchedulerAsync(string cronExpression, CancellationToken stoppingToken)
        {
            var cron = CronExpression.Parse(cronExpression, CronFormat.Standard);
            var tz = TimeZoneInfo.Utc;

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTimeOffset.UtcNow;
                var next = cron.GetNextOccurrence(now, tz);
                if (!next.HasValue)
                    break;

                var delay = next.Value - now;
                if (delay > TimeSpan.Zero)
                {
                    try
                    {
                        await Task.Delay(delay, stoppingToken);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }

                if (stoppingToken.IsCancellationRequested)
                    break;

                try
                {
                    await RunProducerOnceAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Job {JobName}: scheduled producer run failed.", _jobName);
                }
            }
        }

        private Task<int> GetQueueDepthAsync(CancellationToken _) =>
            Task.FromResult(Interlocked.CompareExchange(ref _queueDepth, 0, 0));

        private static BoundedChannelFullMode ParseFullMode(string? mode) =>
            mode?.ToLowerInvariant() switch
            {
                "dropwrite" => BoundedChannelFullMode.DropWrite,
                "dropoldest" => BoundedChannelFullMode.DropOldest,
                "dropnewest" => BoundedChannelFullMode.DropNewest,
                _ => BoundedChannelFullMode.Wait
            };
    }
}
