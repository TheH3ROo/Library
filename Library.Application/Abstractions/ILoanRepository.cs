using Library.Domain;

namespace Library.Application.Abstractions
{
    public interface ILoanRepository
    {
        Task<Loan?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<Guid> CreateAsync(Loan loan, CancellationToken ct = default);
        Task UpdateAsync(Loan loan, CancellationToken ct = default);
        Task<IReadOnlyList<Loan>> ListActiveAsync(CancellationToken ct = default);
        Task<bool> HasActiveLoanForBookAsync(Guid bookId, CancellationToken ct = default);
    }
}