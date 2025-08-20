using System.ComponentModel.DataAnnotations;

namespace Library.Api.Contracts;

public sealed class BookRequest
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Author { get; set; } = string.Empty;

    [Required, MaxLength(32)]
    public string ISBN { get; set; } = string.Empty;

    [Range(1450, 3000)]
    public int PublishedYear { get; set; }
}

public sealed class BookResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public int PublishedYear { get; set; }
    public bool IsAvailable { get; set; }
}
