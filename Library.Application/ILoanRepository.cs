using Library.Domain;

namespace Library.Application
{
    public interface ILoanRepository
    {
        Task<Loan?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<Guid> CreateAsync(Loan loan, CancellationToken ct = default);
        Task UpdateAsync(Loan loan, CancellationToken ct = default);
        Task<IReadOnlyList<Loan>> ListActiveAsync(CancellationToken ct = default);
    }
}