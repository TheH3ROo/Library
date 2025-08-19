using Library.Domain;

namespace Library.Tests
{
    public class ReturnDomainTests
    {
        private static Book NewBorrowedBook() => new Book
        {
            Id = Guid.NewGuid(),
            Title = "DDD",
            Author = "Eric Evans",
            ISBN = "978-0321125217",
            PublishedYear = 2003,
            IsAvailable = false // kölcsönben van
        };

        private static Loan NewActiveLoan(Guid bookId, Guid userId, DateTime loanDateUtc) => new Loan
        {
            Id = Guid.NewGuid(),
            BookId = bookId,
            UserId = userId,
            LoanDate = loanDateUtc,
            ReturnDate = null
        };

        [Fact]
        public void Return_Throws_WhenBookIsAlreadyAvailable()
        {
            var book = new Book { Id = Guid.NewGuid(), IsAvailable = true };
            var loan = NewActiveLoan(book.Id, Guid.NewGuid(), DateTime.UtcNow.AddDays(-1));
            var now = DateTime.UtcNow;

            var ex = Assert.Throws<InvalidOperationException>(() =>
                LoanDomain.Return(book, loan, now));

            Assert.Contains("Book is not on loan", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Return_Throws_WhenLoanIsAlreadyReturned()
        {
            var book = NewBorrowedBook();
            var loan = NewActiveLoan(book.Id, Guid.NewGuid(), DateTime.UtcNow.AddDays(-2));
            loan.ReturnDate = DateTime.UtcNow.AddDays(-1);
            var now = DateTime.UtcNow;

            var ex = Assert.Throws<InvalidOperationException>(() =>
                LoanDomain.Return(book, loan, now));

            Assert.Contains("Loan already returned", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Return_Throws_WhenNowIsDefaultOrBeforeLoanDate()
        {
            var book = NewBorrowedBook();
            var loanDate = DateTime.UtcNow.AddDays(-2);
            var loan = NewActiveLoan(book.Id, Guid.NewGuid(), loanDate);

            var ex1 = Assert.Throws<ArgumentException>(() =>
                LoanDomain.Return(book, loan, default));
            Assert.Contains("Now cannot be default", ex1.Message, StringComparison.OrdinalIgnoreCase);

            var ex2 = Assert.Throws<ArgumentException>(() =>
                LoanDomain.Return(book, loan, loanDate.AddDays(-1)));
            Assert.Contains("Now cannot be before loan date", ex2.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Return_Throws_WhenNowIsTooFarInTheFuture()
        {
            var book = NewBorrowedBook();
            var loan = NewActiveLoan(book.Id, Guid.NewGuid(), DateTime.UtcNow.AddDays(-1));
            var now = DateTime.UtcNow.AddMinutes(2);

            var ex = Assert.Throws<ArgumentException>(() =>
                LoanDomain.Return(book, loan, now));

            Assert.Contains("Now cannot be in the future", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Return_SetsReturnDate_And_MakesBookAvailable()
        {
            var book = NewBorrowedBook();
            var loanDate = DateTime.UtcNow.AddHours(-2);
            var loan = NewActiveLoan(book.Id, Guid.NewGuid(), loanDate);
            var now = DateTime.UtcNow;

            LoanDomain.Return(book, loan, now);

            Assert.True(book.IsAvailable);
            Assert.NotNull(loan.ReturnDate);
            Assert.Equal(now, loan.ReturnDate.Value, precision: TimeSpan.FromSeconds(1));
        }
    }
}