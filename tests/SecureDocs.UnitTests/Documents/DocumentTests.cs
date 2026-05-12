using FluentAssertions;
using SecureDocs.Domain.Documents;
using SecureDocs.Domain.Documents.Events;

namespace SecureDocs.UnitTests.Documents;

public class DocumentTests
{
    [Fact]
    public void Submit_ReturnsDocumentInPendingStatus()
    {
        var document = Document.Submit();

        document.Status.Should().Be(DocumentStatus.Pending);
    }

    [Fact]
    public void Submit_GeneratesNonEmptyId()
    {
        var document = Document.Submit();

        document.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Submit_GeneratesUniqueIdsAcrossInvocations()
    {
        var first = Document.Submit();
        var second = Document.Submit();

        first.Id.Should().NotBe(second.Id);
    }

    [Fact]
    public void Submit_SetsSubmittedAtToUtcNow()
    {
        var before = DateTimeOffset.UtcNow;

        var document = Document.Submit();

        var after = DateTimeOffset.UtcNow;
        document.SubmittedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void Submit_EmitsDocumentSubmittedEvent()
    {
        var document = Document.Submit();

        document.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<DocumentSubmittedEvent>();
    }

    [Fact]
    public void MarkAsProcessed_FromPending_ChangesStatusToProcessed()
    {
        var document = Document.Submit();

        document.MarkAsProcessed();

        document.Status.Should().Be(DocumentStatus.Processed);
    }

    [Fact]
    public void MarkAsProcessed_WhenAlreadyProcessed_IsIdempotent()
    {
        var document = Document.Submit();
        document.MarkAsProcessed();

        var act = () => document.MarkAsProcessed();

        act.Should().NotThrow();
        document.Status.Should().Be(DocumentStatus.Processed);
    }

    [Fact]
    public void MarkAsProcessed_FromFailed_ThrowsInvalidOperationException()
    {
        var document = Document.Submit();
        document.MarkAsFailed();

        var act = () => document.MarkAsProcessed();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MarkAsFailed_FromPending_ChangesStatusToFailed()
    {
        var document = Document.Submit();

        document.MarkAsFailed();

        document.Status.Should().Be(DocumentStatus.Failed);
    }

    [Fact]
    public void MarkAsFailed_WhenAlreadyFailed_IsIdempotent()
    {
        var document = Document.Submit();
        document.MarkAsFailed();

        var act = () => document.MarkAsFailed();

        act.Should().NotThrow();
        document.Status.Should().Be(DocumentStatus.Failed);
    }

    [Fact]
    public void MarkAsFailed_FromProcessed_ThrowsInvalidOperationException()
    {
        var document = Document.Submit();
        document.MarkAsProcessed();

        var act = () => document.MarkAsFailed();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ClearDomainEvents_RemovesAllEvents()
    {
        var document = Document.Submit();
        document.DomainEvents.Should().HaveCount(1);

        document.ClearDomainEvents();

        document.DomainEvents.Should().BeEmpty();
    }
}
