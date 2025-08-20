using Library.Application.Abstractions;
using Library.Application.Users;
using Library.Domain;
using System.Collections.Concurrent;

namespace Library.Tests
{
    public class UserServiceTests
    {
        private sealed class UserRepoFake : IUserRepository
        {
            private readonly ConcurrentDictionary<Guid, User> _byId = new();
            private readonly ConcurrentDictionary<string, Guid> _byEmail = new(StringComparer.OrdinalIgnoreCase);

            public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
                => Task.FromResult(_byId.TryGetValue(id, out var u) ? u : null);

            public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
                => Task.FromResult(_byEmail.TryGetValue(email, out var id) && _byId.TryGetValue(id, out var u) ? u : null);

            public Task<Guid> AddAsync(User user, CancellationToken ct = default)
            {
                _byId[user.Id] = user;
                _byEmail[user.Email] = user.Id;
                return Task.FromResult(user.Id);
            }

            public Task<IReadOnlyList<User>> ListAsync(CancellationToken ct = default)
                => Task.FromResult((IReadOnlyList<User>)_byId.Values.OrderBy(u => u.Name).ToList());
        }

        [Fact]
        public async Task RegisterAsync_CreatesUser_WithNormalizedEmail()
        {
            var repo = new UserRepoFake();
            var svc = new UserService(repo);

            var id = await svc.RegisterAsync(new UserCreateDto { Name = "Ada", Email = "ADA@EXAMPLE.COM" }, DateTime.UtcNow);
            var user = await repo.GetByIdAsync(id);

            Assert.NotNull(user);
            Assert.Equal("ada@example.com", user!.Email);
            Assert.Equal("Ada", user.Name);
        }

        [Fact]
        public async Task RegisterAsync_Throws_WhenEmailAlreadyExists()
        {
            var repo = new UserRepoFake();
            var svc = new UserService(repo);

            await svc.RegisterAsync(new UserCreateDto { Name = "Bob", Email = "bob@example.com" }, DateTime.UtcNow);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                svc.RegisterAsync(new UserCreateDto { Name = "Bobby", Email = "bob@example.com" }, DateTime.UtcNow));
        }

        [Theory]
        [InlineData("", "a@b.com", "Name")]
        [InlineData("Ada", "", "Email")]
        [InlineData("Ada", "not-an-email", "Email")]
        public async Task RegisterAsync_Throws_OnInvalidInput(string name, string email, string expectedParam)
        {
            var repo = new UserRepoFake();
            var svc = new UserService(repo);

            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                svc.RegisterAsync(new UserCreateDto { Name = name, Email = email }, DateTime.UtcNow));

            Assert.Equal(expectedParam, ex.ParamName);
        }
    }
}
