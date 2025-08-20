using Library.Application.Abstractions;
using Library.Domain;
using Library.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Library.Infrastructure.Repositories;

public class BookRepository : IBookRepository
{
    private readonly LibraryDbContext _db;
    public BookRepository(LibraryDbContext db) => _db = db;

    public async Task<Book?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.Books.FirstOrDefaultAsync(b => b.Id == id, ct);

    public async Task<Guid> AddAsync(Book book, CancellationToken ct = default)
    {
        await _db.Books.AddAsync(book, ct);
        await _db.SaveChangesAsync(ct);
        return book.Id;
    }

    public async Task UpdateAsync(Book book, CancellationToken ct = default)
    {
        _db.Books.Update(book);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.Books.FirstOrDefaultAsync(b => b.Id == id, ct);
        if (entity is null) return; // service már 404-elt
        _db.Books.Remove(entity);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Book>> ListAsync(bool? available = null, string? author = null, CancellationToken ct = default)
    {
        IQueryable<Book> q = _db.Books.AsNoTracking();
        if (available is not null) q = q.Where(b => b.IsAvailable == available.Value);
        if (!string.IsNullOrWhiteSpace(author)) q = q.Where(b => EF.Functions.Like(b.Author, $"%{author}%"));
        return await q.ToListAsync(ct);
    }
}
