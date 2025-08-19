using Library.Application.Abstractions;
using Library.Domain;

namespace Library.Application.Services
{
    public interface ILoanService
    {
        Task<Guid> BorrowAsync(Guid userId, Guid bookId, DateTime now, CancellationToken ct = default);
        Task ReturnAsync(Guid loanId, DateTime now, CancellationToken ct = default);
        Task<IReadOnlyList<Loan>> ListActiveLoansAsync(CancellationToken ct = default);
    }
    public sealed class LoanService : ILoanService
    {
        private readonly IBookRepository _books;
        private readonly ILoanRepository _loans;

        public LoanService(IBookRepository books, ILoanRepository loans)
        {
            _books = books;
            _loans = loans;
        }

        public async Task<Guid> BorrowAsync(Guid userId, Guid bookId, DateTime now, CancellationToken ct = default)
        {
            // 1) Betöltés
            var book = await _books.GetByIdAsync(bookId, ct)
                       ?? throw new KeyNotFoundException("Book not found");

            // 2) Domain szabályok (dob, ha nem oké)
            var loan = LoanDomain.Borrow(book, userId, now);

            // 3) Perzisztencia
            //    - könyv státusz frissítése (IsAvailable = false)
            //    - loan létrehozása
            await _books.UpdateAsync(book, ct);
            var loanId = await _loans.CreateAsync(loan, ct);

            return loanId;
        }

        public async Task ReturnAsync(Guid loanId, DateTime now, CancellationToken ct = default)
        {
            // 1) Betöltés
            var loan = await _loans.GetByIdAsync(loanId, ct)
                       ?? throw new KeyNotFoundException("Loan not found");

            var book = await _books.GetByIdAsync(loan.BookId, ct)
                       ?? throw new KeyNotFoundException("Book not found");

            // 2) Domain szabályok (dob, ha nem oké)
            LoanDomain.Return(book, loan, now);

            // 3) Perzisztencia
            await _books.UpdateAsync(book, ct);
            await _loans.UpdateAsync(loan, ct);
        }

        public Task<IReadOnlyList<Loan>> ListActiveLoansAsync(CancellationToken ct = default)
            => _loans.ListActiveAsync(ct);
    }
}
