namespace Library.Application.Books
{
    public class BookCreateDto
    {
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string ISBN { get; set; } = string.Empty;
        public int PublishedYear { get; set; }
    }

    public sealed class BookUpdateDto : BookCreateDto
    {
        public Guid Id { get; set; }
    }
}
