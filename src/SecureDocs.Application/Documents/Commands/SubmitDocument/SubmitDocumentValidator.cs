using FluentValidation;

namespace SecureDocs.Application.Documents.Commands.SubmitDocument;

public class SubmitDocumentValidator : AbstractValidator<SubmitDocumentCommand>
{
    public SubmitDocumentValidator()
    {
        RuleFor(c => c.Payload)
            .NotEmpty()
            .MaximumLength(10_000_000);

        RuleFor(c => c.Passphrase)
            .NotEmpty()
            .MinimumLength(12)
            .MaximumLength(1024);
    }
}
