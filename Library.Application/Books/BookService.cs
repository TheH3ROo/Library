using Library.Application.Abstractions;
using Library.Domain;

namespace Library.Application.Books
{
    public interface IBookService
    {
        Task<Guid> CreateAsync(BookCreateDto dto, CancellationToken ct = default);
        Task UpdateAsync(BookUpdateDto dto, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
        Task<IReadOnlyList<Book>> ListAsync(bool? available = null, string? author = null, CancellationToken ct = default);
    }

    public sealed class BookService : IBookService
    {
        private readonly IBookRepository _books;
        private readonly ILoanRepository _loans;

        public BookService(IBookRepository books, ILoanRepository loans)
        {
            _books = books;
            _loans = loans;
        }

        public async Task<Guid> CreateAsync(BookCreateDto dto, CancellationToken ct = default)
        {
            Validate(dto);
            var book = new Book
            {
                Id = Guid.NewGuid(),
                Title = dto.Title.Trim(),
                Author = dto.Author.Trim(),
                ISBN = dto.ISBN.Trim(),
                PublishedYear = dto.PublishedYear,
                IsAvailable = true
            };
            return await _books.AddAsync(book, ct);
        }

        public async Task UpdateAsync(BookUpdateDto dto, CancellationToken ct = default)
        {
            Validate(dto);
            var existing = await _books.GetByIdAsync(dto.Id, ct)
                ?? throw new KeyNotFoundException("Book not found");

            existing.Title = dto.Title.Trim();
            existing.Author = dto.Author.Trim();
            existing.ISBN = dto.ISBN.Trim();
            existing.PublishedYear = dto.PublishedYear;

            await _books.UpdateAsync(existing, ct);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var book = await _books.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException("Book not found");

            var hasActive = await _loans.HasActiveLoanForBookAsync(id, ct);
            if (hasActive)
                throw new InvalidOperationException("Cannot delete a book that is currently on loan");

            await _books.DeleteAsync(id, ct);
        }

        public Task<IReadOnlyList<Book>> ListAsync(bool? available = null, string? author = null, CancellationToken ct = default)
            => _books.ListAsync(available, author, ct);

        private static void Validate(BookCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new ArgumentException("Title is required", nameof(dto.Title));
            if (string.IsNullOrWhiteSpace(dto.Author))
                throw new ArgumentException("Author is required", nameof(dto.Author));
            if (string.IsNullOrWhiteSpace(dto.ISBN))
                throw new ArgumentException("ISBN is required", nameof(dto.ISBN));
            if (dto.PublishedYear < 1450 || dto.PublishedYear > DateTime.UtcNow.Year)
                throw new ArgumentException("PublishedYear is out of range", nameof(dto.PublishedYear));
        }
    }
}
