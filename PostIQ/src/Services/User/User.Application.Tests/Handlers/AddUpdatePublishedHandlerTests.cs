using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using Moq;
using PostIQ.Core.Database;
using PostIQ.Core.HttpClientService.Models;
using PostIQ.Core.HttpClientService.Services;
using PostIQ.Core.Response;
using System.Linq.Expressions;
using User.Application.Commands;
using User.Application.Handlers;
using User.Core.Persistence;
using Xunit;
using PublishedEntity = User.Core.Entities.Published;

namespace User.Application.Tests.Handlers
{
    public class AddUpdatePublishedHandlerTests
    {
        private readonly Mock<IUnitOfWork<UserDBContext>> _uowMock;
        private readonly Mock<IRepositoryAsync<PublishedEntity>> _repoMock;
        private readonly Mock<IBaseHttpClientService> _httpClientMock;
        private readonly Mock<ILogger<AddUpdatePublishedHandler>> _loggerMock;
        private readonly AddUpdatePublishedHandler _handler;

        public AddUpdatePublishedHandlerTests()
        {
            _uowMock = new Mock<IUnitOfWork<UserDBContext>>();
            _repoMock = new Mock<IRepositoryAsync<PublishedEntity>>();
            _httpClientMock = new Mock<IBaseHttpClientService>();
            _loggerMock = new Mock<ILogger<AddUpdatePublishedHandler>>();

            _uowMock.Setup(u => u.GetRepositoryAsync<PublishedEntity>()).Returns(_repoMock.Object);

            _handler = new AddUpdatePublishedHandler(
                _uowMock.Object,
                _httpClientMock.Object,
                _loggerMock.Object);
        }

        #region Insert (new Published record) Tests

        [Fact]
        public async Task Handle_NewPublished_InsertsRecordAndSyncsToPublishedService()
        {
            // Arrange
            var command = new AddUpdatePublishedCommand
            {
                UserId = 42,
                Source = "GitHub",
                BaseUrl = "https://github.com/user42"
            };

            // No existing record
            _repoMock.Setup(r => r.SingleOrDefaultAsync(
                    It.IsAny<Expression<Func<PublishedEntity, bool>>>(),
                    null, null, true, false))
                .ReturnsAsync((PublishedEntity)null!);

            _repoMock.Setup(r => r.InsertAsync(It.IsAny<PublishedEntity>(), It.IsAny<CancellationToken>()))
                .Callback<PublishedEntity, CancellationToken>((p, _) => p.PublishedId = 100)
                .Returns(default(ValueTask<EntityEntry<PublishedEntity>>));

            _uowMock.Setup(u => u.Commit()).Returns(1);

            _httpClientMock.Setup(h => h.PostAsync(
                    "PublishedService",
                    "api/RepoDetails/Job",
                    It.IsAny<object>(),
                    null,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseResult { IsSuccessStatusCode = true, StatusCode = 200 });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(100, result.Data);

            _repoMock.Verify(r => r.InsertAsync(
                It.Is<PublishedEntity>(p =>
                    p.UserId == 42 &&
                    p.Source == "GitHub" &&
                    p.BaseUrl == "https://github.com/user42" &&
                    p.IsActive),
                It.IsAny<CancellationToken>()), Times.Once);

            _uowMock.Verify(u => u.Commit(), Times.Once);

            _httpClientMock.Verify(h => h.PostAsync(
                "PublishedService",
                "api/RepoDetails/Job",
                It.IsAny<object>(),
                null,
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_NewPublished_SetsCreatedOnToUtcNow()
        {
            // Arrange
            PublishedEntity captured = null!;
            var command = new AddUpdatePublishedCommand
            {
                UserId = 1,
                Source = "Medium",
                BaseUrl = "https://medium.com/@user1"
            };

            _repoMock.Setup(r => r.SingleOrDefaultAsync(
                    It.IsAny<Expression<Func<PublishedEntity, bool>>>(),
                    null, null, true, false))
                .ReturnsAsync((PublishedEntity)null!);

            _repoMock.Setup(r => r.InsertAsync(It.IsAny<PublishedEntity>(), It.IsAny<CancellationToken>()))
                .Callback<PublishedEntity, CancellationToken>((p, _) => { captured = p; p.PublishedId = 1; })
                .Returns(default(ValueTask<EntityEntry<PublishedEntity>>));

            _uowMock.Setup(u => u.Commit()).Returns(1);

            _httpClientMock.Setup(h => h.PostAsync(
                    It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<object>(), null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseResult { IsSuccessStatusCode = true, StatusCode = 200 });

            // Act
            var before = DateTime.UtcNow;
            await _handler.Handle(command, CancellationToken.None);
            var after = DateTime.UtcNow;

            // Assert
            Assert.NotNull(captured);
            Assert.InRange(captured.CreatedOn, before, after);
            Assert.Equal(1, captured.CreatedBy);
        }

        #endregion

        #region Update (existing Published record) Tests

        [Fact]
        public async Task Handle_ExistingPublished_UpdatesBaseUrlAndSyncs()
        {
            // Arrange
            var existing = new PublishedEntity
            {
                PublishedId = 50,
                UserId = 42,
                Source = "GitHub",
                BaseUrl = "https://github.com/old-url",
                IsActive = true,
                CreatedOn = DateTime.UtcNow.AddDays(-10),
                CreatedBy = 42
            };

            var command = new AddUpdatePublishedCommand
            {
                UserId = 42,
                Source = "GitHub",
                BaseUrl = "https://github.com/new-url"
            };

            _repoMock.Setup(r => r.SingleOrDefaultAsync(
                    It.IsAny<Expression<Func<PublishedEntity, bool>>>(),
                    null, null, true, false))
                .ReturnsAsync(existing);

            _uowMock.Setup(u => u.Commit()).Returns(1);

            _httpClientMock.Setup(h => h.PostAsync(
                    "PublishedService",
                    "api/RepoDetails/Job",
                    It.IsAny<object>(),
                    null,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseResult { IsSuccessStatusCode = true, StatusCode = 200 });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(50, result.Data);
            Assert.Equal("https://github.com/new-url", existing.BaseUrl);
            Assert.NotNull(existing.UpdatedOn);
            Assert.Equal(42L, existing.UpdatedBy);

            _repoMock.Verify(r => r.InsertAsync(
                It.IsAny<PublishedEntity>(), It.IsAny<CancellationToken>()), Times.Never);

            _uowMock.Verify(u => u.Commit(), Times.Once);
        }

        [Fact]
        public async Task Handle_ExistingPublished_SetsUpdatedOnToUtcNow()
        {
            // Arrange
            var existing = new PublishedEntity
            {
                PublishedId = 5,
                UserId = 10,
                Source = "Medium",
                BaseUrl = "https://old.com",
                IsActive = true,
                CreatedOn = DateTime.UtcNow.AddDays(-30),
                CreatedBy = 10
            };

            var command = new AddUpdatePublishedCommand
            {
                UserId = 10,
                Source = "Medium",
                BaseUrl = "https://new.com"
            };

            _repoMock.Setup(r => r.SingleOrDefaultAsync(
                    It.IsAny<Expression<Func<PublishedEntity, bool>>>(),
                    null, null, true, false))
                .ReturnsAsync(existing);

            _uowMock.Setup(u => u.Commit()).Returns(1);

            _httpClientMock.Setup(h => h.PostAsync(
                    It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<object>(), null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseResult { IsSuccessStatusCode = true, StatusCode = 200 });

            // Act
            var before = DateTime.UtcNow;
            await _handler.Handle(command, CancellationToken.None);
            var after = DateTime.UtcNow;

            // Assert
            Assert.NotNull(existing.UpdatedOn);
            Assert.InRange(existing.UpdatedOn.Value, before, after);
        }

        #endregion

        #region Sync Failure Resilience Tests

        [Fact]
        public async Task Handle_SyncHttpFailure_StillReturnsPublishedId()
        {
            // Arrange — sync returns non-success status
            var command = new AddUpdatePublishedCommand
            {
                UserId = 7,
                Source = "GitLab",
                BaseUrl = "https://gitlab.com/user7"
            };

            _repoMock.Setup(r => r.SingleOrDefaultAsync(
                    It.IsAny<Expression<Func<PublishedEntity, bool>>>(),
                    null, null, true, false))
                .ReturnsAsync((PublishedEntity)null!);

            _repoMock.Setup(r => r.InsertAsync(It.IsAny<PublishedEntity>(), It.IsAny<CancellationToken>()))
                .Callback<PublishedEntity, CancellationToken>((p, _) => p.PublishedId = 77)
                .Returns(default(ValueTask<EntityEntry<PublishedEntity>>));

            _uowMock.Setup(u => u.Commit()).Returns(1);

            _httpClientMock.Setup(h => h.PostAsync(
                    It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<object>(), null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseResult
                {
                    IsSuccessStatusCode = false,
                    StatusCode = 500,
                    ReasonPhrase = "Internal Server Error"
                });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert — local save succeeded, sync failure is non-fatal
            Assert.Equal(77, result.Data);
            _uowMock.Verify(u => u.Commit(), Times.Once);
        }

        [Fact]
        public async Task Handle_SyncThrowsException_StillReturnsPublishedId()
        {
            // Arrange — sync throws an exception
            var command = new AddUpdatePublishedCommand
            {
                UserId = 9,
                Source = "Bitbucket",
                BaseUrl = "https://bitbucket.org/user9"
            };

            _repoMock.Setup(r => r.SingleOrDefaultAsync(
                    It.IsAny<Expression<Func<PublishedEntity, bool>>>(),
                    null, null, true, false))
                .ReturnsAsync((PublishedEntity)null!);

            _repoMock.Setup(r => r.InsertAsync(It.IsAny<PublishedEntity>(), It.IsAny<CancellationToken>()))
                .Callback<PublishedEntity, CancellationToken>((p, _) => p.PublishedId = 99)
                .Returns(default(ValueTask<EntityEntry<PublishedEntity>>));

            _uowMock.Setup(u => u.Commit()).Returns(1);

            _httpClientMock.Setup(h => h.PostAsync(
                    It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<object>(), null, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException("Connection refused"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert — handler catches exception, local record still returned
            Assert.Equal(99, result.Data);
        }

        #endregion

        #region HTTP Payload Verification Tests

        [Fact]
        public async Task Handle_NewRecord_SendsCorrectPayloadToPublishedService()
        {
            // Arrange
            object capturedBody = null!;
            var command = new AddUpdatePublishedCommand
            {
                UserId = 15,
                Source = "GitHub",
                BaseUrl = "https://github.com/user15"
            };

            _repoMock.Setup(r => r.SingleOrDefaultAsync(
                    It.IsAny<Expression<Func<PublishedEntity, bool>>>(),
                    null, null, true, false))
                .ReturnsAsync((PublishedEntity)null!);

            _repoMock.Setup(r => r.InsertAsync(It.IsAny<PublishedEntity>(), It.IsAny<CancellationToken>()))
                .Callback<PublishedEntity, CancellationToken>((p, _) => p.PublishedId = 200)
                .Returns(default(ValueTask<EntityEntry<PublishedEntity>>));

            _uowMock.Setup(u => u.Commit()).Returns(1);

            _httpClientMock.Setup(h => h.PostAsync(
                    "PublishedService",
                    "api/RepoDetails/Job",
                    It.IsAny<object>(),
                    null,
                    It.IsAny<CancellationToken>()))
                .Callback<string, string, object?, RequestOptions?, CancellationToken>(
                    (_, _, body, _, _) => capturedBody = body!)
                .ReturnsAsync(new HttpResponseResult { IsSuccessStatusCode = true, StatusCode = 200 });

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert — verify the anonymous object has the right shape
            Assert.NotNull(capturedBody);
            var bodyType = capturedBody.GetType();
            Assert.Equal(200L, bodyType.GetProperty("publishedId")!.GetValue(capturedBody));
            Assert.Equal(15L, bodyType.GetProperty("userId")!.GetValue(capturedBody));
            Assert.Equal("GitHub", bodyType.GetProperty("source")!.GetValue(capturedBody));
            Assert.Equal("https://github.com/user15", bodyType.GetProperty("baseUrl")!.GetValue(capturedBody));
        }

        [Fact]
        public async Task Handle_UpdateRecord_SendsUpdatedBaseUrlInPayload()
        {
            // Arrange
            object capturedBody = null!;
            var existing = new PublishedEntity
            {
                PublishedId = 300,
                UserId = 20,
                Source = "Medium",
                BaseUrl = "https://medium.com/@old",
                IsActive = true,
                CreatedOn = DateTime.UtcNow.AddDays(-5),
                CreatedBy = 20
            };

            var command = new AddUpdatePublishedCommand
            {
                UserId = 20,
                Source = "Medium",
                BaseUrl = "https://medium.com/@new-profile"
            };

            _repoMock.Setup(r => r.SingleOrDefaultAsync(
                    It.IsAny<Expression<Func<PublishedEntity, bool>>>(),
                    null, null, true, false))
                .ReturnsAsync(existing);

            _uowMock.Setup(u => u.Commit()).Returns(1);

            _httpClientMock.Setup(h => h.PostAsync(
                    "PublishedService",
                    "api/RepoDetails/Job",
                    It.IsAny<object>(),
                    null,
                    It.IsAny<CancellationToken>()))
                .Callback<string, string, object?, RequestOptions?, CancellationToken>(
                    (_, _, body, _, _) => capturedBody = body!)
                .ReturnsAsync(new HttpResponseResult { IsSuccessStatusCode = true, StatusCode = 200 });

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert — updated BaseUrl is sent, not the old one
            Assert.NotNull(capturedBody);
            var bodyType = capturedBody.GetType();
            Assert.Equal(300L, bodyType.GetProperty("publishedId")!.GetValue(capturedBody));
            Assert.Equal("https://medium.com/@new-profile", bodyType.GetProperty("baseUrl")!.GetValue(capturedBody));
        }

        #endregion

        #region Client Name & Endpoint Tests

        [Fact]
        public async Task Handle_UsesCorrectClientNameAndEndpoint()
        {
            // Arrange
            var command = new AddUpdatePublishedCommand
            {
                UserId = 1,
                Source = "GitHub",
                BaseUrl = "https://github.com/test"
            };

            _repoMock.Setup(r => r.SingleOrDefaultAsync(
                    It.IsAny<Expression<Func<PublishedEntity, bool>>>(),
                    null, null, true, false))
                .ReturnsAsync((PublishedEntity)null!);

            _repoMock.Setup(r => r.InsertAsync(It.IsAny<PublishedEntity>(), It.IsAny<CancellationToken>()))
                .Callback<PublishedEntity, CancellationToken>((p, _) => p.PublishedId = 1)
                .Returns(default(ValueTask<EntityEntry<PublishedEntity>>));

            _uowMock.Setup(u => u.Commit()).Returns(1);

            _httpClientMock.Setup(h => h.PostAsync(
                    It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<object>(), null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseResult { IsSuccessStatusCode = true, StatusCode = 200 });

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert — exact client name and endpoint
            _httpClientMock.Verify(h => h.PostAsync(
                "PublishedService",
                "api/RepoDetails/Job",
                It.IsAny<object>(),
                null,
                It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion
    }
}
