using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SecureDocs.Application.Documents.Commands.SubmitDocument;
using SecureDocs.Domain.Documents;
using SecureDocs.Domain.EncryptedPayloads;
using SecureDocs.Infrastructure.Persistence;
using StackExchange.Redis;

namespace SecureDocs.IntegrationTests;

public class DocumentsApiTests : IClassFixture<IntegrationTestFactory>
{
    private const string ValidPassphrase = "correct horse battery staple";

    private readonly IntegrationTestFactory _factory;
    private readonly HttpClient _client;

    public DocumentsApiTests(IntegrationTestFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Post_WithValidPayload_PersistsDocumentAndStoresBlobInRedis()
    {
        // Arrange
        const string payload = "integration test content";
        var request = new { payload, passphrase = ValidPassphrase };

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

        // Assert JSON blob (payload + passphrase) stored in Redis
        var multiplexer = scope.ServiceProvider.GetRequiredService<IConnectionMultiplexer>();
        var redis = multiplexer.GetDatabase();
        var redisValue = await redis.StringGetAsync($"payload:{result.DocumentId}");
        redisValue.HasValue.Should().BeTrue();

        var blob = JsonSerializer.Deserialize<RedisBlob>(
            redisValue.ToString(),
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        blob.Should().NotBeNull();
        blob!.Payload.Should().Be(payload);
        blob.Passphrase.Should().Be(ValidPassphrase);
    }

    [Fact]
    public async Task Post_WithEmptyPayload_ReturnsBadRequest()
    {
        var request = new { payload = string.Empty, passphrase = ValidPassphrase };

        var response = await _client.PostAsJsonAsync("/Documents", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_WithPassphraseShorterThanMinimum_ReturnsBadRequest()
    {
        var request = new { payload = "any", passphrase = "short" };

        var response = await _client.PostAsJsonAsync("/Documents", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Get_WithExistingId_ReturnsDocumentMetadata()
    {
        var createResponse = await _client.PostAsJsonAsync(
            "/Documents",
            new { payload = "another document", passphrase = ValidPassphrase });
        var created = await createResponse.Content.ReadFromJsonAsync<SubmitDocumentResult>();

        var getResponse = await _client.GetAsync($"/Documents/{created!.DocumentId}");

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await getResponse.Content.ReadFromJsonAsync<DocumentDtoLocal>();
        dto.Should().NotBeNull();
        dto!.Id.Should().Be(created.DocumentId);
        dto.Status.Should().Be(DocumentStatus.Pending);
    }

    [Fact]
    public async Task Get_WithNonExistentId_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/Documents/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetIntegrity_WithExistingPayload_ReturnsIntegrityDto()
    {
        var document = Document.Submit();
        var hash = new byte[32];
        var signature = new byte[64];
        new Random(42).NextBytes(hash);
        new Random(43).NextBytes(signature);

        var payload = EncryptedPayload.Create(
            documentId: document.Id,
            ciphertext: [1, 2, 3],
            nonce: [4, 5, 6],
            tag: [7, 8, 9],
            salt: new byte[16],
            kdfAlgorithm: "scrypt",
            kdfParameters: "{\"n\":16384,\"r\":8,\"p\":1}",
            hash: hash,
            signature: signature,
            algorithm: "AES-256-GCM");

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Documents.Add(document);
            dbContext.EncryptedPayloads.Add(payload);
            await dbContext.SaveChangesAsync();
        }

        var response = await _client.GetAsync($"/Documents/{document.Id}/integrity");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<IntegrityDtoLocal>();
        dto.Should().NotBeNull();
        dto!.DocumentId.Should().Be(document.Id);
        dto.Hash.Should().BeEquivalentTo(hash);
        dto.Signature.Should().BeEquivalentTo(signature);
        dto.Algorithm.Should().Be("AES-256-GCM");
        dto.ProcessedAt.Should().Be(payload.ProcessedAt);
    }

    [Fact]
    public async Task GetIntegrity_WhenNoEncryptedPayloadExists_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/Documents/{Guid.NewGuid()}/integrity");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private record DocumentDtoLocal(Guid Id, DocumentStatus Status, DateTimeOffset SubmittedAt);

    private record IntegrityDtoLocal(
        Guid DocumentId,
        byte[] Hash,
        byte[] Signature,
        string Algorithm,
        DateTimeOffset ProcessedAt);

    private record RedisBlob(string Payload, string Passphrase);
}
