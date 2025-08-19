namespace Library.Domain
{
    public static class LoanDomain
    {
        public static Loan Borrow(Book book, Guid userId, DateTime now)
        {
            ArgumentNullException.ThrowIfNull(book);

            if (userId == Guid.Empty) throw new ArgumentException("User ID cannot be empty", nameof(userId));

            if (now == default) throw new ArgumentException("Now cannot be default", nameof(now));

            var utcNow = DateTime.UtcNow;
            var tolerance = TimeSpan.FromSeconds(1);

            if (now > utcNow + tolerance) throw new ArgumentException("Now cannot be in the future", nameof(now));

            if (now < utcNow - tolerance) throw new ArgumentException("Now cannot be in the past", nameof(now));

            if (!book.IsAvailable) throw new InvalidOperationException("Book not available.");

            book.IsAvailable = false;

            var loan = new Loan
            {
                UserId = userId,
                BookId = book.Id,
                LoanDate = now,
                ReturnDate = null
            };

            return loan;
        }
        
        public static void Return(Book book, Loan loan, DateTime now)
        {
            ArgumentNullException.ThrowIfNull(book);

            ArgumentNullException.ThrowIfNull(loan);

            if (now == default) throw new ArgumentException("now cannot be default", nameof(now));

            var utcNow = DateTime.UtcNow;
            var tolerance = TimeSpan.FromSeconds(1);

            if (now > utcNow + tolerance)
                throw new ArgumentException("now cannot be in the future", nameof(now));

            if (loan.ReturnDate is not null)
                throw new InvalidOperationException("Loan already returned");

            if (book.IsAvailable)
                throw new InvalidOperationException("Book is not on loan");

            if (loan.BookId != book.Id)
                throw new ArgumentException("Loan does not belong to this book", nameof(loan));

            if (now < loan.LoanDate - tolerance)
                throw new ArgumentException("now cannot be before loan date", nameof(now));

            loan.ReturnDate = now;
            book.IsAvailable = true;
        }
    }
}
