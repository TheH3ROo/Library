using Library.Domain;

namespace Library.Tests
{
    public class LoanDomainTests
    {
        [Fact]
        public void Borrow_Throws_WhenBookNotAvailable()
        {
            var book = new Book
            {
                Id = Guid.NewGuid(),
                Title = "Clean Code",
                Author = "Robert C. Martin",
                ISBN = "978-0132350884",
                PublishedYear = 2008,
                IsAvailable = false
            };

            var userId = Guid.NewGuid();
            var now = DateTime.UtcNow;
            
            var exception = Assert.Throws<InvalidOperationException>(() => 
                LoanDomain.Borrow(book, userId, now));

            Assert.Contains("not available", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Borrow_DoesNotThrow_WhenBookIsAvailable()
        {
            var book = new Book
            {
                Id = Guid.NewGuid(),
                Title = "Clean Code",
                Author = "Robert C. Martin",
                ISBN = "978-0132350884",
                PublishedYear = 2008,
                IsAvailable = true
            };
            var userId = Guid.NewGuid();
            var now = DateTime.UtcNow;

            LoanDomain.Borrow(book, userId, now);
        }

        [Fact]
        public void Borrow_SetsBookUnavailable_AndReturnsLoan()
        {
            var book = new Book
            {
                Id = Guid.NewGuid(),
                Title = "Clean Architecture",
                Author = "Robert C. Martin",
                ISBN = "9780134494166",
                PublishedYear = 2017,
                IsAvailable = true
            };

            var userId = Guid.NewGuid();
            var now = DateTime.UtcNow;

            var borrow = LoanDomain.Borrow(book, userId, now);

            Assert.NotNull(borrow);
            Assert.Equal(book.Id, borrow.BookId);
            Assert.Equal(userId, borrow.UserId);
            Assert.Equal(now, borrow.LoanDate, precision: TimeSpan.FromSeconds(1));
            Assert.Null(borrow.ReturnDate);

            Assert.False(book.IsAvailable); // könyv már nem elérhető
        }

        [Fact]
        public void Borrow_Throws_WhenBookIsNull()
        {
            Book? book = null;
            var userId = Guid.NewGuid();
            var now = DateTime.UtcNow;

            var exception = Assert.Throws<ArgumentNullException>(() => 
                LoanDomain.Borrow(book, userId, now));

            Assert.Equal("book", exception.ParamName);
            Assert.Contains("cannot be null", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Borrow_Throws_WhenBookIsAvailableButUserIdIsEmpty()
        {
            var book = new Book
            {
                Id = Guid.NewGuid(),
                Title = "Clean Code",
                Author = "Robert C. Martin",
                ISBN = "978-0132350884",
                PublishedYear = 2008,
                IsAvailable = true
            };
            var userId = Guid.Empty;
            var now = DateTime.UtcNow;

            var exception = Assert.Throws<ArgumentException>(() => 
                LoanDomain.Borrow(book, userId, now));

            Assert.Contains("User ID cannot be empty", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Borrow_Throws_WhenBookIsAvailableAndNowIsDefault()
        {
            var book = new Book
            {
                Id = Guid.NewGuid(),
                Title = "Clean Code",
                Author = "Robert C. Martin",
                ISBN = "978-0132350884",
                PublishedYear = 2008,
                IsAvailable = true
            };

            var userId = Guid.NewGuid();
            var now = default(DateTime);

            var exception = Assert.Throws<ArgumentException>(() => 
                LoanDomain.Borrow(book, userId, now));

            Assert.Contains("Now cannot be default", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Borrow_Throws_WhenBookIsAvailableAndNowIsInTheFuture()
        {
            var book = new Book
            {
                Id = Guid.NewGuid(),
                Title = "Clean Code",
                Author = "Robert C. Martin",
                ISBN = "978-0132350884",
                PublishedYear = 2008,
                IsAvailable = true
            };
            var userId = Guid.NewGuid();
            var now = DateTime.UtcNow.AddDays(1);
            
            var exception = Assert.Throws<ArgumentException>(() => 
                LoanDomain.Borrow(book, userId, now));

            Assert.Contains("now cannot be in the future", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Borrow_Throws_WhenBookIsAvailableAndNowIsInThePast()
        {
            var book = new Book
            {
                Id = Guid.NewGuid(),
                Title = "Clean Code",
                Author = "Robert C. Martin",
                ISBN = "978-0132350884",
                PublishedYear = 2008,
                IsAvailable = true
            };

            var userId = Guid.NewGuid();
            var now = DateTime.UtcNow.AddDays(-1);

            var exception = Assert.Throws<ArgumentException>(() =>
                LoanDomain.Borrow(book, userId, now));

            Assert.Contains("Now cannot be in the past", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Borrow_Throws_WhenBookIsAvailableAndNowIsYesterday()
        {
            var book = new Book
            {
                Id = Guid.NewGuid(),
                Title = "Clean Code",
                Author = "Robert C. Martin",
                ISBN = "978-0132350884",
                PublishedYear = 2008,
                IsAvailable = true
            };

            var userId = Guid.NewGuid();
            var now = DateTime.UtcNow.AddDays(-1).Date;

            var exception = Assert.Throws<ArgumentException>(() =>
                LoanDomain.Borrow(book, userId, now));

            Assert.Contains("Now cannot be in the past", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Borrow_Throws_WhenBookIsAvailableAndNowIsTomorrow()
        {
            var book = new Book
            {
                Id = Guid.NewGuid(),
                Title = "Clean Code",
                Author = "Robert C. Martin",
                ISBN = "978-0132350884",
                PublishedYear = 2008,
                IsAvailable = true
            };

            var userId = Guid.NewGuid();
            var now = DateTime.UtcNow.AddDays(1).Date;

            var exception = Assert.Throws<ArgumentException>(() =>
                LoanDomain.Borrow(book, userId, now));

            Assert.Contains("Now cannot be in the future", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Borrow_Throws_WhenBookIsAvailableAndNowIsLastYear()
        {
            var book = new Book
            {
                Id = Guid.NewGuid(),
                Title = "Clean Code",
                Author = "Robert C. Martin",
                ISBN = "978-0132350884",
                PublishedYear = 2008,
                IsAvailable = true
            };

            var userId = Guid.NewGuid();
            var now = DateTime.UtcNow.AddYears(-1).Date;

            var exception = Assert.Throws<ArgumentException>(() =>
                LoanDomain.Borrow(book, userId, now));

            Assert.Contains("Now cannot be in the past", exception.Message, StringComparison.OrdinalIgnoreCase);
        }
    }
}
