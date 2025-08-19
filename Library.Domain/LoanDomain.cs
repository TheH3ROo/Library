namespace Library.Domain
{
    public static class LoanDomain
    {
        public static Loan Borrow(Book book, Guid userId, DateTime now)
        {
            if (book is null) throw new ArgumentNullException(nameof(book), "Book cannot be null.");

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
    }
}
