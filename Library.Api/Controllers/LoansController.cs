using Library.Api.Contracts;
using Library.Application.Abstractions;
using Library.Application.Loans;
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

        [HttpPost]
        public async Task<IActionResult> Borrow([FromBody] BorrowRequest request, CancellationToken ct)
        {
            var loanId = await _loanService.BorrowAsync(request.UserId, request.BookId, request.NowUtc, ct);
            return CreatedAtAction(nameof(GetById), new { id = loanId }, new { LoanId = loanId });
        }

        [HttpPost("{id:guid}/return")]
        public async Task<IActionResult> Return([FromRoute] Guid id, [FromBody] ReturnRequest request, CancellationToken ct)
        {
            if (id != request.LoanId)
                return BadRequest(new { error = "Route id and body LoanId mismatch" });

            await _loanService.ReturnAsync(request.LoanId, request.NowUtc, ct);
            return NoContent();
        }

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
