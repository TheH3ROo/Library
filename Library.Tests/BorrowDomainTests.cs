using Library.Domain;

namespace Library.Tests
{
    public class BorrowDomainTests
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
                PublicationYear = 2008,
                IsAvailable = false
            };

            var userId = Guid.NewGuid();
            var now = DateTime.UtcNow;
            
            var exception = Assert.Throws<InvalidOperationException>(() => 
                BorrowDomain.Borrow(book, userId, now));

            Assert.Contains("not available", exception.Message, StringComparison.OrdinalIgnoreCase);
        }
    }
}
