using ReviewPortal.Application.Common;
using ReviewPortal.Application.Interfaces;
using ReviewPortal.Domain.Common;
using ReviewPortal.Domain.Entities;
using ReviewPortal.Domain.Interfaces;

namespace ReviewPortal.UnitTests.TestDoubles;

internal sealed class InMemoryCategoryRepository : ICategoryRepository
{
    private readonly List<Category> _categories;

    public InMemoryCategoryRepository(IEnumerable<Category>? categories = null)
    {
        _categories = categories?.ToList() ?? [];
    }

    public Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_categories.SingleOrDefault(category => category.Id == id));
    }

    public Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<Category>>(_categories);
    }

    public Task<Category> AddAsync(Category entity, CancellationToken cancellationToken = default)
    {
        entity.Id = _categories.Count == 0 ? 1 : _categories.Max(category => category.Id) + 1;
        _categories.Add(entity);

        return Task.FromResult(entity);
    }

    public Task UpdateAsync(Category entity, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Category entity, CancellationToken cancellationToken = default)
    {
        _categories.Remove(entity);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Category>> GetAllWithToolsAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<Category>>(_categories);
    }

    public Task<Category?> GetByIdWithToolsAsync(int id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_categories.SingleOrDefault(category => category.Id == id));
    }

    public Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default, int? excludedCategoryId = null)
    {
        return Task.FromResult(_categories.Any(category =>
            string.Equals(category.Name, name, StringComparison.OrdinalIgnoreCase) &&
            (!excludedCategoryId.HasValue || category.Id != excludedCategoryId.Value)));
    }
}

internal sealed class InMemoryUserRepository : IUserRepository
{
    private readonly List<User> _users;

    public InMemoryUserRepository(IEnumerable<User>? users = null)
    {
        _users = users?.ToList() ?? [];
    }

    public Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_users.SingleOrDefault(user => user.Id == id));
    }

    public Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<User>>(_users);
    }

    public Task<User> AddAsync(User entity, CancellationToken cancellationToken = default)
    {
        entity.Id = _users.Count == 0 ? 1 : _users.Max(user => user.Id) + 1;
        _users.Add(entity);

        return Task.FromResult(entity);
    }

    public Task UpdateAsync(User entity, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task DeleteAsync(User entity, CancellationToken cancellationToken = default)
    {
        _users.Remove(entity);
        return Task.CompletedTask;
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_users.SingleOrDefault(user =>
            string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase)));
    }

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_users.Any(user =>
            string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase)));
    }
}

internal sealed class InMemoryToolRepository : IToolRepository
{
    private readonly List<Tool> _tools;

    public InMemoryToolRepository(IEnumerable<Tool>? tools = null)
    {
        _tools = tools?.ToList() ?? [];
    }

    public Task<Tool?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_tools.SingleOrDefault(tool => tool.Id == id));
    }

    public Task<IReadOnlyList<Tool>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<Tool>>(_tools);
    }

    public Task<Tool> AddAsync(Tool entity, CancellationToken cancellationToken = default)
    {
        entity.Id = _tools.Count == 0 ? 1 : _tools.Max(tool => tool.Id) + 1;
        _tools.Add(entity);

        return Task.FromResult(entity);
    }

    public Task UpdateAsync(Tool entity, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Tool entity, CancellationToken cancellationToken = default)
    {
        _tools.Remove(entity);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Tool>> GetActiveByCategoryAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<Tool>>(
            _tools.Where(tool => tool.IsActive && tool.CategoryId == categoryId).ToList());
    }

    public Task<Tool?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_tools.SingleOrDefault(tool => tool.Id == id));
    }
}

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveChangesCallCount { get; private set; }

    public void Dispose()
    {
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCallCount++;
        return Task.FromResult(1);
    }
}

internal sealed class FakePasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        return $"hashed::{password}";
    }

    public bool Verify(string password, string passwordHash)
    {
        return Hash(password) == passwordHash;
    }
}

internal sealed class FakeJwtProvider : IJwtProvider
{
    public JwtToken Generate(User user)
    {
        return new JwtToken(
            $"token-for-{user.Id}",
            new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    }
}
