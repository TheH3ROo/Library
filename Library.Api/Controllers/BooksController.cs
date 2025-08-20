using Library.Api.Contracts;
using Library.Application.Abstractions;
using Library.Application.Books;
using Microsoft.AspNetCore.Mvc;

namespace Library.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IBookService _books;

    public BooksController(IBookService books) => _books = books;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] BookRequest req, CancellationToken ct)
    {
        try
        {
            var id = await _books.CreateAsync(new BookCreateDto
            {
                Title = req.Title,
                Author = req.Author,
                ISBN = req.ISBN,
                PublishedYear = req.PublishedYear
            }, ct);

            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message, param = ex.ParamName });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] BookRequest req, CancellationToken ct)
    {
        try
        {
            await _books.UpdateAsync(new BookUpdateDto
            {
                Id = id,
                Title = req.Title,
                Author = req.Author,
                ISBN = req.ISBN,
                PublishedYear = req.PublishedYear
            }, ct);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message, param = ex.ParamName });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
    {
        try
        {
            await _books.DeleteAsync(id, ct);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookResponse>>> List([FromQuery] bool? available, [FromQuery] string? author, CancellationToken ct)
    {
        var list = await _books.ListAsync(available, author, ct);
        var result = list.Select(b => new BookResponse
        {
            Id = b.Id,
            Title = b.Title,
            Author = b.Author,
            ISBN = b.ISBN,
            PublishedYear = b.PublishedYear,
            IsAvailable = b.IsAvailable
        });
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BookResponse>> GetById([FromRoute] Guid id, [FromServices] IBookRepository repo, CancellationToken ct)
    {
        var b = await repo.GetByIdAsync(id, ct);
        if (b is null) return NotFound();
        return Ok(new BookResponse
        {
            Id = b.Id,
            Title = b.Title,
            Author = b.Author,
            ISBN = b.ISBN,
            PublishedYear = b.PublishedYear,
            IsAvailable = b.IsAvailable
        });
    }
}
