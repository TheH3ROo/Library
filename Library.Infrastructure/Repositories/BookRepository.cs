using Library.Application.Abstractions;
using Library.Domain;
using Library.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Library.Infrastructure.Repositories;

public class BookRepository(LibraryDbContext db) : IBookRepository
{
    private readonly LibraryDbContext _db = db;

    public async Task<Book?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.Books.FirstOrDefaultAsync(b => b.Id == id, ct);

    public async Task UpdateAsync(Book book, CancellationToken ct = default)
    {
        _db.Books.Update(book);
        await _db.SaveChangesAsync(ct);
    }
}
