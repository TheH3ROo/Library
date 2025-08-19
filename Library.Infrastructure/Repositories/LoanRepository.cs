using Library.Application.Abstractions;
using Library.Domain;
using Library.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Library.Infrastructure.Repositories;

public class LoanRepository(LibraryDbContext db) : ILoanRepository
{
    private readonly LibraryDbContext _db = db;

    public async Task<Loan?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.Loans.FirstOrDefaultAsync(l => l.Id == id, ct);

    public async Task<Guid> CreateAsync(Loan loan, CancellationToken ct = default)
    {
        await _db.Loans.AddAsync(loan, ct);
        await _db.SaveChangesAsync(ct);
        return loan.Id;
    }

    public async Task UpdateAsync(Loan loan, CancellationToken ct = default)
    {
        _db.Loans.Update(loan);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Loan>> ListActiveAsync(CancellationToken ct = default) =>
        await _db.Loans.Where(l => l.ReturnDate == null).ToListAsync(ct);
}
