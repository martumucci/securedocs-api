using FluentValidation;

namespace SecureDocs.Application.Documents.Commands.SubmitDocument;

public class SubmitDocumentValidator : AbstractValidator<SubmitDocumentCommand>
{
    private const int MaxPayloadBytes = 10_000_000;

    public SubmitDocumentValidator()
    {
        RuleFor(c => c.Payload)
            .NotEmpty()
            .Must(payload => payload == null || payload.Length <= MaxPayloadBytes)
            .WithMessage($"Payload must not exceed {MaxPayloadBytes} bytes.");

        RuleFor(c => c.Passphrase)
            .NotEmpty()
            .MinimumLength(12)
            .MaximumLength(1024);
    }
}
