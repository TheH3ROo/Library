using Library.Application.Abstractions;
using Library.Application.Books;
using Library.Domain;
using System.Collections.Concurrent;

namespace Library.Tests
{
    public class BookServiceTests
    {
        private sealed class BookRepoFake : IBookRepository
        {
            private readonly ConcurrentDictionary<Guid, Book> _store = new();
            public void Seed(Book b) => _store[b.Id] = b;

            public Task<Book?> GetByIdAsync(Guid id, CancellationToken ct = default)
                => Task.FromResult(_store.TryGetValue(id, out var b) ? b : null);

            public Task<Guid> AddAsync(Book book, CancellationToken ct = default)
            {
                _store[book.Id] = book; return Task.FromResult(book.Id);
            }

            public Task UpdateAsync(Book book, CancellationToken ct = default)
            {
                _store[book.Id] = book; return Task.CompletedTask;
            }

            public Task DeleteAsync(Guid id, CancellationToken ct = default)
            {
                _store.TryRemove(id, out _); return Task.CompletedTask;
            }

            public Task<IReadOnlyList<Book>> ListAsync(bool? available = null, string? author = null, CancellationToken ct = default)
            {
                var q = _store.Values.AsQueryable();
                if (available is not null) q = q.Where(b => b.IsAvailable == available.Value);
                if (!string.IsNullOrWhiteSpace(author)) q = q.Where(b => b.Author.Contains(author, StringComparison.OrdinalIgnoreCase));
                return Task.FromResult((IReadOnlyList<Book>)q.ToList());
            }
        }

        private sealed class LoanRepoFake : ILoanRepository
        {
            private readonly HashSet<(Guid bookId, Guid loanId)> _active = new();
            public void SeedActive(Guid bookId) => _active.Add((bookId, Guid.NewGuid()));
            public Task<Loan?> GetByIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult<Loan?>(null);
            public Task<Guid> CreateAsync(Loan loan, CancellationToken ct = default) => Task.FromResult(loan.Id);
            public Task UpdateAsync(Loan loan, CancellationToken ct = default) => Task.CompletedTask;
            public Task<IReadOnlyList<Loan>> ListActiveAsync(CancellationToken ct = default)
                => Task.FromResult((IReadOnlyList<Loan>)Array.Empty<Loan>());
            public Task<bool> HasActiveLoanForBookAsync(Guid bookId, CancellationToken ct = default)
                => Task.FromResult(_active.Any(a => a.bookId == bookId));
        }

        [Fact]
        public async Task CreateAsync_CreatesBook_WithAvailableTrue()
        {
            var books = new BookRepoFake();
            var loans = new LoanRepoFake();
            var svc = new BookService(books, loans);

            var id = await svc.CreateAsync(new BookCreateDto
            {
                Title = "Clean Code",
                Author = "Robert C. Martin",
                ISBN = "978-0132350884",
                PublishedYear = 2008
            });

            var created = await books.GetByIdAsync(id);
            Assert.NotNull(created);
            Assert.True(created!.IsAvailable);
        }

        [Fact]
        public async Task DeleteAsync_Throws_WhenActiveLoanExists()
        {
            var books = new BookRepoFake();
            var loans = new LoanRepoFake();
            var svc = new BookService(books, loans);

            var book = new Book { Id = Guid.NewGuid(), Title = "X", Author = "A", ISBN = "I", PublishedYear = 2000, IsAvailable = false };
            books.Seed(book);
            loans.SeedActive(book.Id);

            await Assert.ThrowsAsync<InvalidOperationException>(() => svc.DeleteAsync(book.Id));
        }

        [Fact]
        public async Task ListAsync_FiltersByAvailabilityAndAuthor()
        {
            var books = new BookRepoFake();
            var loans = new LoanRepoFake();
            var svc = new BookService(books, loans);

            books.Seed(new Book { Id = Guid.NewGuid(), Title = "A", Author = "John", ISBN = "1", PublishedYear = 2000, IsAvailable = true });
            books.Seed(new Book { Id = Guid.NewGuid(), Title = "B", Author = "Jane", ISBN = "2", PublishedYear = 2001, IsAvailable = false });

            var result = await svc.ListAsync(available: true, author: "jo");
            Assert.Single(result);
        }
    }
}
