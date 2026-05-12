using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecureDocs.Application.Documents.Commands.SubmitDocument;

namespace SecureDocs.API.Controllers;

[ApiController]
[Route("[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DocumentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Submit(
        [FromBody] SubmitDocumentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new SubmitDocumentCommand(request.Payload);
        var result = await _mediator.Send(command, cancellationToken);

        return Created($"/documents/{result.DocumentId}", result);
    }
}

public record SubmitDocumentRequest(string Payload);
