using Library.Application.Abstractions;
using Library.Domain;
using System.Text.RegularExpressions;

namespace Library.Application.Users;

public interface IUserService
{
    Task<Guid> RegisterAsync(UserCreateDto dto, DateTime nowUtc, CancellationToken ct = default);
    Task<IReadOnlyList<User>> ListAsync(CancellationToken ct = default);
}

public sealed class UserService : IUserService
{
    private readonly IUserRepository _users;

    public UserService(IUserRepository users) => _users = users;

    public async Task<Guid> RegisterAsync(UserCreateDto dto, DateTime nowUtc, CancellationToken ct = default)
    {
        Validate(dto);

        var emailNorm = dto.Email.Trim().ToLowerInvariant();
        if (await _users.GetByEmailAsync(emailNorm, ct) is not null)
            throw new InvalidOperationException("Email already registered");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = dto.Name.Trim(),
            Email = emailNorm,
            RegisteredDate = nowUtc
        };

        return await _users.AddAsync(user, ct);
    }

    public Task<IReadOnlyList<User>> ListAsync(CancellationToken ct = default)
        => _users.ListAsync(ct);

    private static void Validate(UserCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Name is required", nameof(dto.Name));

        if (string.IsNullOrWhiteSpace(dto.Email))
            throw new ArgumentException("Email is required", nameof(dto.Email));

        var rx = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        if (!rx.IsMatch(dto.Email.Trim()))
            throw new ArgumentException("Email format is invalid", nameof(dto.Email));
    }
}
