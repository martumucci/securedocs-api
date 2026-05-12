using FluentValidation.TestHelper;
using SecureDocs.Application.Documents.Commands.SubmitDocument;

namespace SecureDocs.UnitTests.Documents.Commands.SubmitDocument;

public class SubmitDocumentValidatorTests
{
    private readonly SubmitDocumentValidator _validator = new();

    [Fact]
    public void Validate_WithValidPayload_HasNoErrors()
    {
        var command = new SubmitDocumentCommand("valid content");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyPayload_HasErrorForPayload()
    {
        var command = new SubmitDocumentCommand(string.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Payload);
    }

    [Fact]
    public void Validate_WithPayloadExceedingMaxLength_HasErrorForPayload()
    {
        var command = new SubmitDocumentCommand(new string('x', 100_001));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Payload);
    }

    [Fact]
    public void Validate_WithPayloadAtMaxLength_HasNoErrors()
    {
        var command = new SubmitDocumentCommand(new string('x', 100_000));

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
