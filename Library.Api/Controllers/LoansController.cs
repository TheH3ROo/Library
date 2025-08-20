using Library.Api.Contracts;
using Library.Application.Abstractions;
using Library.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Library.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoansController : ControllerBase
    {
        private readonly ILoanService _loanService;
        public LoansController(ILoanService loanService)
        {
            _loanService = loanService;
        }

        /// <summary>Borrow a book</summary>
        [HttpPost]
        public async Task<IActionResult> Borrow([FromBody] BorrowRequest request, CancellationToken ct)
        {
            try
            {
                var loanId = await _loanService.BorrowAsync(request.UserId, request.BookId, request.NowUtc, ct);
                return CreatedAtAction(nameof(Borrow), new { id = loanId }, new { LoanId = loanId });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message, param = ex.ParamName });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        /// <summary>Return a book</summary>
        [HttpPost("{id:guid}/return")]
        public async Task<IActionResult> Return([FromRoute] Guid id, [FromBody] ReturnRequest request, CancellationToken ct)
        {
            if (id != request.LoanId)
                return BadRequest(new { error = "Route id and body LoanId mismatch" });

            try
            {
                await _loanService.ReturnAsync(request.LoanId, request.NowUtc, ct);
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
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
        }

        /// <summary>List active loans</summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LoanResponse>>> GetActive(CancellationToken ct)
        {
            var loans = await _loanService.ListActiveLoansAsync(ct);
            var result = loans.Select(l => new LoanResponse
            {
                Id = l.Id,
                BookId = l.BookId,
                UserId = l.UserId,
                LoanDate = l.LoanDate,
                ReturnDate = l.ReturnDate
            });
            return Ok(result);
        }

        /// <summary>Get a single loan by id</summary>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<LoanResponse>> GetById([FromRoute] Guid id, [FromServices] ILoanRepository repo, CancellationToken ct)
        {
            var loan = await repo.GetByIdAsync(id, ct);
            if (loan is null) return NotFound();
            return Ok(new LoanResponse
            {
                Id = loan.Id,
                BookId = loan.BookId,
                UserId = loan.UserId,
                LoanDate = loan.LoanDate,
                ReturnDate = loan.ReturnDate
            });
        }
    }
}
