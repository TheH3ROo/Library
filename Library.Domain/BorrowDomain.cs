namespace Library.Domain
{
    public static class BorrowDomain
    {
        public static void Borrow(Book book, Guid userId, DateTime now)
        {
            if (book is null) throw new ArgumentNullException(nameof(book), "Book cannot be null.");

            if (!book.IsAvailable) throw new InvalidOperationException("Book not available.");
        }
    }
}
