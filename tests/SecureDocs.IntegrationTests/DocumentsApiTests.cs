using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SecureDocs.Application.Documents.Commands.SubmitDocument;
using SecureDocs.Domain.Documents;
using SecureDocs.Infrastructure.Persistence;
using StackExchange.Redis;

namespace SecureDocs.IntegrationTests;

public class DocumentsApiTests : IClassFixture<IntegrationTestFactory>
{
    private readonly IntegrationTestFactory _factory;
    private readonly HttpClient _client;

    public DocumentsApiTests(IntegrationTestFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Post_WithValidPayload_PersistsDocumentAndStoresPayloadInRedis()
    {
        // Arrange
        const string payload = "integration test content";
        var request = new { payload };

        // Act
        var response = await _client.PostAsJsonAsync("/Documents", request);

        // Assert response
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<SubmitDocumentResult>();
        result.Should().NotBeNull();
        result!.DocumentId.Should().NotBe(Guid.Empty);
        result.Status.Should().Be(DocumentStatus.Pending);

        // Assert document persisted in Postgres
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var documentInDb = await dbContext.Documents.FindAsync(result.DocumentId);
        documentInDb.Should().NotBeNull();
        documentInDb!.Status.Should().Be(DocumentStatus.Pending);

        // Assert payload stored in Redis
        var multiplexer = scope.ServiceProvider.GetRequiredService<IConnectionMultiplexer>();
        var redis = multiplexer.GetDatabase();
        var redisValue = await redis.StringGetAsync($"payload:{result.DocumentId}");
        redisValue.HasValue.Should().BeTrue();
        redisValue.ToString().Should().Be(payload);
    }

    [Fact]
    public async Task Post_WithEmptyPayload_ReturnsBadRequest()
    {
        // Arrange
        var request = new { payload = string.Empty };

        // Act
        var response = await _client.PostAsJsonAsync("/Documents", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Get_WithExistingId_ReturnsDocumentMetadata()
    {
        // Arrange — first create one
        const string payload = "another document";
        var createResponse = await _client.PostAsJsonAsync("/Documents", new { payload });
        var created = await createResponse.Content.ReadFromJsonAsync<SubmitDocumentResult>();

        // Act
        var getResponse = await _client.GetAsync($"/Documents/{created!.DocumentId}");

        // Assert
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await getResponse.Content.ReadFromJsonAsync<DocumentDtoLocal>();
        dto.Should().NotBeNull();
        dto!.Id.Should().Be(created.DocumentId);
        dto.Status.Should().Be(DocumentStatus.Pending);
    }

    [Fact]
    public async Task Get_WithNonExistentId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync($"/Documents/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private record DocumentDtoLocal(Guid Id, DocumentStatus Status, DateTimeOffset SubmittedAt);
}
