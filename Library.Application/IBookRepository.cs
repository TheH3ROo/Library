
using Library.Domain;

namespace Library.Application
{
    public interface IBookRepository
    {
        Task<Book?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task UpdateAsync(Book book, CancellationToken ct = default);
    }
}