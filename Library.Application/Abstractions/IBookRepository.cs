using Library.Domain;

namespace Library.Application.Abstractions
{
    public interface IBookRepository
    {
        Task<Book?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<Guid> AddAsync(Book book, CancellationToken ct = default);
        Task UpdateAsync(Book book, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
        Task<IReadOnlyList<Book>> ListAsync(bool? available = null, string? author = null, CancellationToken ct = default);
    }
}