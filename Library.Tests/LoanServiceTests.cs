using Library.Application.Abstractions;
using Library.Application.Services;
using Library.Domain;
using System.Collections.Concurrent;

namespace Library.Tests
{
    public class LoanServiceTests
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
            private readonly ConcurrentDictionary<Guid, Loan> _store = new();
            public Task<Loan?> GetByIdAsync(Guid id, CancellationToken ct = default)
                => Task.FromResult(_store.TryGetValue(id, out var l) ? l : null);
            public Task<Guid> CreateAsync(Loan loan, CancellationToken ct = default)
            {
                _store[loan.Id] = loan; return Task.FromResult(loan.Id);
            }
            public Task UpdateAsync(Loan loan, CancellationToken ct = default)
            {
                _store[loan.Id] = loan; return Task.CompletedTask;
            }

            public Task DeleteAsync(Guid id, CancellationToken ct = default)
            {
                _store.TryRemove(id, out _); return Task.CompletedTask;
            }

            public Task<IReadOnlyList<Loan>> ListActiveAsync(CancellationToken ct = default)
                => Task.FromResult((IReadOnlyList<Loan>)_store.Values.Where(l => l.ReturnDate is null).ToList());

            public Task<bool> HasActiveLoanForBookAsync(Guid bookId, CancellationToken ct = default)
                => Task.FromResult(_active.Any(a => a.bookId == bookId));
        }

        private static Book NewBook(bool available = true) => new Book
        {
            Id = Guid.NewGuid(),
            Title = "Clean Code",
            Author = "Robert C. Martin",
            ISBN = "978-0132350884",
            PublishedYear = 2008,
            IsAvailable = available
        };

        // ---- Borrow happy path ----
        [Fact]
        public async Task BorrowAsync_MarksBookUnavailable_AndCreatesLoan()
        {
            var books = new BookRepoFake();
            var loans = new LoanRepoFake();
            var svc = new LoanService(books, loans);

            var book = NewBook(available: true);
            books.Seed(book);

            var userId = Guid.NewGuid();
            var now = DateTime.UtcNow;

            var loanId = await svc.BorrowAsync(userId, book.Id, now);

            var updated = await books.GetByIdAsync(book.Id);
            Assert.False(updated!.IsAvailable);

            var saved = await loans.GetByIdAsync(loanId);
            Assert.NotNull(saved);
            Assert.Equal(book.Id, saved!.BookId);
            Assert.Equal(userId, saved.UserId);
            Assert.Equal(now, saved.LoanDate, precision: TimeSpan.FromSeconds(1));
            Assert.Null(saved.ReturnDate);
        }

        // ---- Borrow - book not found -> 404 (KeyNotFoundException) ----
        [Fact]
        public async Task BorrowAsync_Throws_WhenBookNotFound()
        {
            var svc = new LoanService(new BookRepoFake(), new LoanRepoFake());
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                svc.BorrowAsync(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow));
        }

        // ---- Return happy path ----
        [Fact]
        public async Task ReturnAsync_SetsReturnDate_AndMakesBookAvailable()
        {
            var books = new BookRepoFake();
            var loans = new LoanRepoFake();
            var svc = new LoanService(books, loans);

            var book = NewBook(available: true);
            books.Seed(book);

            var userId = Guid.NewGuid();
            var now = DateTime.UtcNow;

            var loanId = await svc.BorrowAsync(userId, book.Id, now);

            var borrowedBook = await books.GetByIdAsync(book.Id);
            Assert.False(borrowedBook!.IsAvailable);

            var returnNow = DateTime.UtcNow;
            await svc.ReturnAsync(loanId, returnNow);

            var updatedBook = await books.GetByIdAsync(book.Id);
            var updatedLoan = await loans.GetByIdAsync(loanId);

            Assert.True(updatedBook!.IsAvailable);
            Assert.NotNull(updatedLoan!.ReturnDate);
            Assert.Equal(returnNow, updatedLoan.ReturnDate!.Value, precision: TimeSpan.FromSeconds(1));
        }

        // ---- Return - loan not found ----
        [Fact]
        public async Task ReturnAsync_Throws_WhenLoanNotFound()
        {
            var svc = new LoanService(new BookRepoFake(), new LoanRepoFake());
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                svc.ReturnAsync(Guid.NewGuid(), DateTime.UtcNow));
        }

        // ---- List active loans ----
        [Fact]
        public async Task ListActiveLoansAsync_ReturnsOnlyActive()
        {
            var books = new BookRepoFake();
            var loans = new LoanRepoFake();
            var svc = new LoanService(books, loans);

            var book = NewBook(available: true);
            books.Seed(book);
            var userId = Guid.NewGuid();
            var now = DateTime.UtcNow;

            var loanId = await svc.BorrowAsync(userId, book.Id, now);

            var active1 = await svc.ListActiveLoansAsync();
            Assert.Single(active1);

            await svc.ReturnAsync(loanId, DateTime.UtcNow);

            var active2 = await svc.ListActiveLoansAsync();
            Assert.Empty(active2);
        }
    }
}