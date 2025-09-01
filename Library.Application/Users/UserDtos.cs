using System.ComponentModel.DataAnnotations;

namespace Library.Application.Users
{
    public sealed class UserCreateDto
    {
        [Required]
        [MaxLength(100)]
        [RegularExpression(@".*\S.*", ErrorMessage = "A név nem lehet üres.")]
        public string Name { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        [MaxLength(254)]
        public string Email { get; set; } = string.Empty;
    }
}
