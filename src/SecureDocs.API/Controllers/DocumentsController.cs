using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SecureDocs.Application.Documents.Commands.SubmitDocument;
using SecureDocs.Application.Documents.Queries.GetDocumentById;
using SecureDocs.Application.Documents.Queries.GetDocumentIntegrity;

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
    [EnableRateLimiting("submit-document")]
    public async Task<IActionResult> Submit(
        [FromBody] SubmitDocumentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new SubmitDocumentCommand(request.Payload);
        var result = await _mediator.Send(command, cancellationToken);

        return Created($"/documents/{result.DocumentId}", result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetDocumentByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpGet("{id:guid}/integrity")]
    public async Task<IActionResult> GetIntegrity(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetDocumentIntegrityQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }
}

public record SubmitDocumentRequest(string Payload);
