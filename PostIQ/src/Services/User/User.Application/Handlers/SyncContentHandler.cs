using MediatR;
using System.Threading.Channels;

namespace User.Application.Handlers
{
    public class SyncContentHandler : IRequestHandler<Commands.SyncContentCommand, bool>
    {
        private readonly Channel<Commands.SyncContentCommand> _channel;

        public SyncContentHandler(Channel<Commands.SyncContentCommand> channel)
        {
            _channel = channel;
        }

        public async Task<bool> Handle(Commands.SyncContentCommand request, CancellationToken cancellationToken)
        {
            await _channel.Writer.WriteAsync(request, cancellationToken);
            return true;
        }
    }
}
