using ReviewPortal.Application.Common;
using ReviewPortal.Application.Interfaces;
using ReviewPortal.Domain.Common;
using ReviewPortal.Domain.Entities;
using ReviewPortal.Domain.Enums;
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

    public IReadOnlyList<Tool> Items => _tools;

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

    public Task<IReadOnlyList<Tool>> GetAllActiveWithDetailsAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<Tool>>(
            _tools.Where(tool => tool.IsActive).ToList());
    }

    public Task<IReadOnlyList<Tool>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<Tool>>(_tools);
    }

    public Task<Tool?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_tools.SingleOrDefault(tool => tool.Id == id));
    }
}

internal sealed class InMemoryReviewRepository : IReviewRepository
{
    private readonly List<Review> _reviews;

    public InMemoryReviewRepository(IEnumerable<Review>? reviews = null)
    {
        _reviews = reviews?.ToList() ?? [];
    }

    public IReadOnlyList<Review> Items => _reviews;

    public Task<Review?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_reviews.SingleOrDefault(review => review.Id == id));
    }

    public Task<IReadOnlyList<Review>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<Review>>(_reviews);
    }

    public Task<Review> AddAsync(Review entity, CancellationToken cancellationToken = default)
    {
        entity.Id = _reviews.Count == 0 ? 1 : _reviews.Max(review => review.Id) + 1;
        _reviews.Add(entity);

        return Task.FromResult(entity);
    }

    public Task UpdateAsync(Review entity, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Review entity, CancellationToken cancellationToken = default)
    {
        _reviews.Remove(entity);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Review>> GetByUserIdWithDetailsAsync(
        int userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<Review>>(
            _reviews
                .Where(review => review.UserId == userId)
                .OrderByDescending(review => review.CreatedDate)
                .ThenByDescending(review => review.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList());
    }

    public Task<int> CountByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_reviews.Count(review => review.UserId == userId));
    }

    public Task<IReadOnlyList<Review>> GetApprovedByToolIdWithDetailsAsync(
        int toolId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<Review>>(
            _reviews
                .Where(review => review.ToolId == toolId && review.Status == ReviewStatus.Approved)
                .OrderByDescending(review => review.CreatedDate)
                .ThenByDescending(review => review.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList());
    }

    public Task<int> CountApprovedByToolIdAsync(int toolId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_reviews.Count(review => review.ToolId == toolId && review.Status == ReviewStatus.Approved));
    }

    public Task<decimal?> GetAverageOverallRatingByToolIdAsync(int toolId, CancellationToken cancellationToken = default)
    {
        var approvedReviews = _reviews
            .Where(review => review.ToolId == toolId && review.Status == ReviewStatus.Approved)
            .ToList();

        return Task.FromResult(
            approvedReviews.Count == 0
                ? (decimal?)null
                : approvedReviews.Average(review => review.OverallRating));
    }

    public Task<Review?> GetByIdWithDetailsAsync(int reviewId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_reviews.SingleOrDefault(review => review.Id == reviewId));
    }

    public Task<IReadOnlyList<Review>> GetPendingWithDetailsAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<Review>>(
            _reviews
                .Where(review =>
                    review.Status == ReviewStatus.Pending ||
                    review.Comments.Any(comment => comment.Status == ReviewStatus.Pending))
                .OrderBy(review => review.Status == ReviewStatus.Pending
                    ? review.CreatedDate
                    : review.Comments
                        .Where(comment => comment.Status == ReviewStatus.Pending)
                        .Min(comment => comment.CreatedDate))
                .ThenBy(review => review.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList());
    }

    public Task<int> CountPendingAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(
            _reviews.Count(review =>
                review.Status == ReviewStatus.Pending ||
                review.Comments.Any(comment => comment.Status == ReviewStatus.Pending)));
    }

    public Task<IReadOnlyList<Review>> GetByToolIdAsync(int toolId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<Review>>(
            _reviews
                .Where(review => review.ToolId == toolId)
                .ToList());
    }
}

internal sealed class InMemoryRepository<T> : IRepository<T> where T : BaseEntity
{
    private readonly List<T> _entities;

    public InMemoryRepository(IEnumerable<T>? entities = null)
    {
        _entities = entities?.ToList() ?? [];
    }

    public IReadOnlyList<T> Items => _entities;

    public Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_entities.SingleOrDefault(entity => entity.Id == id));
    }

    public Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<T>>(_entities);
    }

    public Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        entity.Id = _entities.Count == 0 ? 1 : _entities.Max(existing => existing.Id) + 1;
        _entities.Add(entity);

        return Task.FromResult(entity);
    }

    public Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        _entities.Remove(entity);
        return Task.CompletedTask;
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
