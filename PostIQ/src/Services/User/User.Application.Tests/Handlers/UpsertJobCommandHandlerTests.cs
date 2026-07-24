using Microsoft.EntityFrameworkCore.ChangeTracking;
using Moq;
using PostIQ.Core.Database;
using PostIQ.Core.Response;
using Published.Application.Commands;
using Published.Application.Handlers;
using Published.Core.Entities;
using Published.Core.Persistence;
using System.Linq.Expressions;
using Xunit;

namespace User.Application.Tests.Handlers
{
    public class UpsertJobCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork<PublishDbContext>> _uowMock;
        private readonly Mock<IRepositoryAsync<Job>> _jobRepoMock;
        private readonly UpsertJobCommandHandler _handler;

        public UpsertJobCommandHandlerTests()
        {
            _uowMock = new Mock<IUnitOfWork<PublishDbContext>>();
            _jobRepoMock = new Mock<IRepositoryAsync<Job>>();

            _uowMock.Setup(u => u.GetRepositoryAsync<Job>()).Returns(_jobRepoMock.Object);

            _handler = new UpsertJobCommandHandler(_uowMock.Object);
        }

        #region Insert (new Job) Tests

        [Fact]
        public async Task Handle_NoExistingJob_CreatesNewJobWithBaseUrl()
        {
            // Arrange
            Job capturedJob = null!;
            var command = new UpsertJobCommand
            {
                PublishedId = 100,
                UserId = 42,
                Source = "GitHub",
                BaseUrl = "https://github.com/user42"
            };

            _jobRepoMock.Setup(r => r.SingleOrDefaultAsync(
                    It.IsAny<Expression<Func<Job, bool>>>(),
                    null, null, true, false))
                .ReturnsAsync((Job)null!);

            _jobRepoMock.Setup(r => r.InsertAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>()))
                .Callback<Job, CancellationToken>((j, _) => { capturedJob = j; j.JobId = 500; })
                .Returns(default(ValueTask<EntityEntry<Job>>));

            _uowMock.Setup(u => u.Commit()).Returns(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(500, result.Data);
            Assert.NotNull(capturedJob);
            Assert.Equal(100, capturedJob.PublishedId);
            Assert.Equal(42, capturedJob.UserId);
            Assert.Equal("GitHub", capturedJob.Source);
            Assert.Equal("https://github.com/user42", capturedJob.BaseUrl);
            Assert.True(capturedJob.IsActive);
            Assert.NotNull(capturedJob.NextExecutionTime);
        }

        [Fact]
        public async Task Handle_NewJob_SetsCreatedMetadata()
        {
            // Arrange
            Job capturedJob = null!;
            var command = new UpsertJobCommand
            {
                PublishedId = 1,
                UserId = 10,
                Source = "Medium",
                BaseUrl = "https://medium.com/@user10"
            };

            _jobRepoMock.Setup(r => r.SingleOrDefaultAsync(
                    It.IsAny<Expression<Func<Job, bool>>>(),
                    null, null, true, false))
                .ReturnsAsync((Job)null!);

            _jobRepoMock.Setup(r => r.InsertAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>()))
                .Callback<Job, CancellationToken>((j, _) => { capturedJob = j; j.JobId = 1; })
                .Returns(default(ValueTask<EntityEntry<Job>>));

            _uowMock.Setup(u => u.Commit()).Returns(1);

            // Act
            var before = DateTime.UtcNow;
            await _handler.Handle(command, CancellationToken.None);
            var after = DateTime.UtcNow;

            // Assert
            Assert.NotNull(capturedJob);
            Assert.InRange(capturedJob.CreatedOn, before, after);
            Assert.Equal(10, capturedJob.CreatedBy);
        }

        #endregion

        #region Update (existing Job) Tests

        [Fact]
        public async Task Handle_ExistingJob_UpdatesBaseUrl()
        {
            // Arrange
            var existingJob = new Job
            {
                JobId = 300,
                PublishedId = 100,
                UserId = 42,
                Source = "GitHub",
                BaseUrl = "https://github.com/old-url",
                IsActive = true,
                CreatedOn = DateTime.UtcNow.AddDays(-10),
                CreatedBy = 42
            };

            var command = new UpsertJobCommand
            {
                PublishedId = 100,
                UserId = 42,
                Source = "GitHub",
                BaseUrl = "https://github.com/new-url"
            };

            _jobRepoMock.Setup(r => r.SingleOrDefaultAsync(
                    It.IsAny<Expression<Func<Job, bool>>>(),
                    null, null, true, false))
                .ReturnsAsync(existingJob);

            _uowMock.Setup(u => u.Commit()).Returns(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(300, result.Data);
            Assert.Equal("https://github.com/new-url", existingJob.BaseUrl);
            Assert.Equal("GitHub", existingJob.Source);
            Assert.NotNull(existingJob.UpdatedOn);
            Assert.Equal(42L, existingJob.UpdatedBy);

            // Should NOT insert a new record
            _jobRepoMock.Verify(r => r.InsertAsync(
                It.IsAny<Job>(), It.IsAny<CancellationToken>()), Times.Never);

            _uowMock.Verify(u => u.Commit(), Times.Once);
        }

        [Fact]
        public async Task Handle_ExistingJob_SetsUpdatedOnToUtcNow()
        {
            // Arrange
            var existingJob = new Job
            {
                JobId = 5,
                PublishedId = 3,
                UserId = 10,
                Source = "GitLab",
                BaseUrl = "https://gitlab.com/old",
                IsActive = true,
                CreatedOn = DateTime.UtcNow.AddDays(-20),
                CreatedBy = 10
            };

            var command = new UpsertJobCommand
            {
                PublishedId = 3,
                UserId = 10,
                Source = "GitLab",
                BaseUrl = "https://gitlab.com/new"
            };

            _jobRepoMock.Setup(r => r.SingleOrDefaultAsync(
                    It.IsAny<Expression<Func<Job, bool>>>(),
                    null, null, true, false))
                .ReturnsAsync(existingJob);

            _uowMock.Setup(u => u.Commit()).Returns(1);

            // Act
            var before = DateTime.UtcNow;
            await _handler.Handle(command, CancellationToken.None);
            var after = DateTime.UtcNow;

            // Assert
            Assert.NotNull(existingJob.UpdatedOn);
            Assert.InRange(existingJob.UpdatedOn.Value, before, after);
        }

        [Fact]
        public async Task Handle_ExistingJob_UpdatesSourceAlongWithBaseUrl()
        {
            // Arrange
            var existingJob = new Job
            {
                JobId = 10,
                PublishedId = 5,
                UserId = 20,
                Source = "OldSource",
                BaseUrl = "https://old.com",
                IsActive = true,
                CreatedOn = DateTime.UtcNow.AddDays(-15),
                CreatedBy = 20
            };

            var command = new UpsertJobCommand
            {
                PublishedId = 5,
                UserId = 20,
                Source = "NewSource",
                BaseUrl = "https://new.com/profile"
            };

            _jobRepoMock.Setup(r => r.SingleOrDefaultAsync(
                    It.IsAny<Expression<Func<Job, bool>>>(),
                    null, null, true, false))
                .ReturnsAsync(existingJob);

            _uowMock.Setup(u => u.Commit()).Returns(1);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal("NewSource", existingJob.Source);
            Assert.Equal("https://new.com/profile", existingJob.BaseUrl);
        }

        #endregion

        #region End-to-End BaseUrl Synchronization Tests

        [Fact]
        public async Task Handle_BaseUrlFromUserService_IsStoredCorrectlyInJob()
        {
            // This test simulates the exact payload that AddUpdatePublishedHandler
            // sends to the Published service endpoint, verifying the BaseUrl
            // makes it through the entire sync pipeline.

            var command = new UpsertJobCommand
            {
                PublishedId = 999,
                UserId = 55,
                Source = "GitHub",
                BaseUrl = "https://github.com/enterprise/user55/repos"
            };

            Job capturedJob = null!;

            _jobRepoMock.Setup(r => r.SingleOrDefaultAsync(
                    It.IsAny<Expression<Func<Job, bool>>>(),
                    null, null, true, false))
                .ReturnsAsync((Job)null!);

            _jobRepoMock.Setup(r => r.InsertAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>()))
                .Callback<Job, CancellationToken>((j, _) => { capturedJob = j; j.JobId = 1000; })
                .Returns(default(ValueTask<EntityEntry<Job>>));

            _uowMock.Setup(u => u.Commit()).Returns(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert — the BaseUrl from User service is stored exactly in Published.Job
            Assert.Equal(1000, result.Data);
            Assert.Equal("https://github.com/enterprise/user55/repos", capturedJob.BaseUrl);
            Assert.Equal(999, capturedJob.PublishedId);
            Assert.Equal(55, capturedJob.UserId);
        }

        [Fact]
        public async Task Handle_BaseUrlUpdate_OverwritesPreviousValueInJob()
        {
            // Simulates a user changing their BaseUrl in User service,
            // which triggers an update sync to Published service.
            var existingJob = new Job
            {
                JobId = 800,
                PublishedId = 400,
                UserId = 33,
                Source = "GitHub",
                BaseUrl = "https://github.com/user33-v1",
                IsActive = true,
                CreatedOn = DateTime.UtcNow.AddDays(-7),
                CreatedBy = 33
            };

            var command = new UpsertJobCommand
            {
                PublishedId = 400,
                UserId = 33,
                Source = "GitHub",
                BaseUrl = "https://github.com/user33-v2"
            };

            _jobRepoMock.Setup(r => r.SingleOrDefaultAsync(
                    It.IsAny<Expression<Func<Job, bool>>>(),
                    null, null, true, false))
                .ReturnsAsync(existingJob);

            _uowMock.Setup(u => u.Commit()).Returns(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(800, result.Data);
            Assert.Equal("https://github.com/user33-v2", existingJob.BaseUrl);
            Assert.NotEqual("https://github.com/user33-v1", existingJob.BaseUrl);
        }

        #endregion
    }
}
