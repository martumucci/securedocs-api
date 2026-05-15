using FluentValidation.TestHelper;
using SecureDocs.Application.Documents.Commands.SubmitDocument;
using SecureDocs.UnitTests.Helpers;

namespace SecureDocs.UnitTests.Documents.Commands.SubmitDocument;

public class SubmitDocumentValidatorTests
{
    private readonly SubmitDocumentValidator _validator = new();

    private static SubmitDocumentCommand Command(string? payload = null, string? passphrase = null)
    {
        return new SubmitDocumentCommand(
            Payload: payload ?? "valid content",
            Passphrase: passphrase ?? TestData.ValidPassphrase);
    }

    [Fact]
    public void Validate_WithValidPayloadAndPassphrase_HasNoErrors()
    {
        var result = _validator.TestValidate(Command());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyPayload_HasErrorForPayload()
    {
        var result = _validator.TestValidate(Command(payload: string.Empty));

        result.ShouldHaveValidationErrorFor(c => c.Payload);
    }

    [Fact]
    public void Validate_WithPayloadExceedingMaxLength_HasErrorForPayload()
    {
        var result = _validator.TestValidate(Command(payload: new string('x', 10_000_001)));

        result.ShouldHaveValidationErrorFor(c => c.Payload);
    }

    [Fact]
    public void Validate_WithPayloadAtMaxLength_HasNoErrors()
    {
        var result = _validator.TestValidate(Command(payload: new string('x', 10_000_000)));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyPassphrase_HasErrorForPassphrase()
    {
        var result = _validator.TestValidate(Command(passphrase: string.Empty));

        result.ShouldHaveValidationErrorFor(c => c.Passphrase);
    }

    [Fact]
    public void Validate_WithPassphraseShorterThanMinimum_HasErrorForPassphrase()
    {
        var result = _validator.TestValidate(Command(passphrase: new string('p', 11)));

        result.ShouldHaveValidationErrorFor(c => c.Passphrase);
    }

    [Fact]
    public void Validate_WithPassphraseAtMinimumLength_HasNoErrors()
    {
        var result = _validator.TestValidate(Command(passphrase: new string('p', 12)));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithPassphraseExceedingMaxLength_HasErrorForPassphrase()
    {
        var result = _validator.TestValidate(Command(passphrase: new string('p', 1025)));

        result.ShouldHaveValidationErrorFor(c => c.Passphrase);
    }
}
