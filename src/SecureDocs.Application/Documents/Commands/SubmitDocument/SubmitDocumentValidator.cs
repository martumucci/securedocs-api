using FluentValidation;

namespace SecureDocs.Application.Documents.Commands.SubmitDocument;

public class SubmitDocumentValidator : AbstractValidator<SubmitDocumentCommand>
{
    public SubmitDocumentValidator()
    {
        RuleFor(c => c.Payload)
            .NotEmpty()
            .MaximumLength(100_000);
    }
}
