using Library.Api.Contracts;
using Library.Application.Users;
using Library.Application.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Library.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _users;

    public UsersController(IUserService users) => _users = users;

    [HttpPost]
    public async Task<IActionResult> Register([FromBody] UserRequest req, CancellationToken ct)
    {
        try
        {
            var id = await _users.RegisterAsync(
                new UserCreateDto { Name = req.Name, Email = req.Email },
                DateTime.UtcNow, ct);

            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message, param = ex.ParamName });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserResponse>>> List(CancellationToken ct)
    {
        var list = await _users.ListAsync(ct);
        var result = list.Select(u => new UserResponse
        {
            Id = u.Id,
            Name = u.Name,
            Email = u.Email,
            RegisteredDate = u.RegisteredDate
        });
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserResponse>> GetById([FromRoute] Guid id, CancellationToken ct,
        [FromServices] IUserRepository repo)
    {
        var u = await repo.GetByIdAsync(id, ct);
        if (u is null) return NotFound();
        return Ok(new UserResponse
        {
            Id = u.Id,
            Name = u.Name,
            Email = u.Email,
            RegisteredDate = u.RegisteredDate
        });
    }
}
