using ReviewPortal.Application.Common;
using ReviewPortal.Application.DTOs.Categories;
using ReviewPortal.Application.Services;
using ReviewPortal.Domain.Entities;
using ReviewPortal.UnitTests.TestDoubles;

namespace ReviewPortal.UnitTests.Services;

public class CategoryServiceTests
{
    [Fact]
    public async Task GetCategoryByIdAsync_WhenCategoryHasTools_ReturnsToolCount()
    {
        var service = CreateService(new InMemoryCategoryRepository(
        [
            new Category
            {
                Id = 3,
                Name = "Cutting",
                Tools =
                [
                    CreateTool(1, "Saw"),
                    CreateTool(2, "Angle Grinder")
                ]
            }
        ]));

        var result = await service.GetCategoryByIdAsync(3);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.ToolCount);
    }

    [Fact]
    public async Task GetCategoryByIdAsync_WhenCategoryDoesNotExist_ReturnsNotFound()
    {
        var service = CreateService();

        var result = await service.GetCategoryByIdAsync(404);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.FailureType);
    }

    [Fact]
    public async Task GetCategoryByIdAsync_WhenToolsCollectionIsMissing_ReturnsZeroToolCount()
    {
        var service = CreateService(new InMemoryCategoryRepository(
        [
            new Category
            {
                Id = 4,
                Name = "Seasonal",
                Tools = null!
            }
        ]));

        var result = await service.GetCategoryByIdAsync(4);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value!.ToolCount);
    }

    [Fact]
    public async Task CreateCategoryAsync_WithValidRequest_TrimsOptionalValuesAndSaves()
    {
        var repository = new InMemoryCategoryRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = CreateService(repository, unitOfWork);

        var result = await service.CreateCategoryAsync(new CreateCategoryRequest(
            " Building & Construction ",
            " Site equipment ",
            " building.jpg "));

        Assert.True(result.IsSuccess);
        Assert.Equal("Building & Construction", result.Value!.Name);
        Assert.Equal("Site equipment", result.Value.Description);
        Assert.Equal("building.jpg", result.Value.ImageUrl);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task CreateCategoryAsync_WhenNameIsBlank_ReturnsValidationFailure()
    {
        var service = CreateService();

        var result = await service.CreateCategoryAsync(new CreateCategoryRequest(" ", null, null));

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.FailureType);
        Assert.Equal("Category name is required.", result.Error);
    }

    [Fact]
    public async Task CreateCategoryAsync_WhenNameIsTooLong_ReturnsValidationFailure()
    {
        var service = CreateService();

        var result = await service.CreateCategoryAsync(new CreateCategoryRequest(new string('A', 101), null, null));

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.FailureType);
        Assert.Equal("Category name must be 100 characters or fewer.", result.Error);
    }

    [Fact]
    public async Task CreateCategoryAsync_WhenNameAlreadyExists_ReturnsConflict()
    {
        var service = CreateService(new InMemoryCategoryRepository(
        [
            new Category { Id = 1, Name = "Drilling" }
        ]));

        var result = await service.CreateCategoryAsync(new CreateCategoryRequest(" drilling ", "Duplicate", null));

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.FailureType);
        Assert.Equal("A category named 'drilling' already exists.", result.Error);
    }

    [Fact]
    public async Task DeleteCategoryAsync_WhenCategoryContainsTools_ReturnsConflict()
    {
        var service = CreateService(new InMemoryCategoryRepository(
        [
            new Category
            {
                Id = 5,
                Name = "Access",
                Tools = [CreateTool(4, "Scaffold Tower")]
            }
        ]));

        var result = await service.DeleteCategoryAsync(5);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.FailureType);
    }

    [Fact]
    public async Task UpdateCategoryAsync_WithValidRequest_UpdatesCategoryAndSaves()
    {
        var category = new Category { Id = 10, Name = "Old Name" };
        var repository = new InMemoryCategoryRepository([category]);
        var unitOfWork = new FakeUnitOfWork();
        var service = CreateService(repository, unitOfWork);

        var result = await service.UpdateCategoryAsync(10, new UpdateCategoryRequest(
            " Access & Lifting ",
            " Work at height ",
            " access.jpg "));

        Assert.True(result.IsSuccess);
        Assert.Equal("Access & Lifting", result.Value!.Name);
        Assert.Equal("Work at height", category.Description);
        Assert.Equal("access.jpg", category.ImageUrl);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task UpdateCategoryAsync_WhenCategoryDoesNotExist_ReturnsNotFound()
    {
        var service = CreateService();

        var result = await service.UpdateCategoryAsync(404, new UpdateCategoryRequest("Access", null, null));

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.FailureType);
    }

    [Fact]
    public async Task UpdateCategoryAsync_WhenNameIsBlank_ReturnsValidationFailure()
    {
        var service = CreateService();

        var result = await service.UpdateCategoryAsync(1, new UpdateCategoryRequest(" ", null, null));

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.FailureType);
        Assert.Equal("Category name is required.", result.Error);
    }

    [Fact]
    public async Task UpdateCategoryAsync_WhenToolsCollectionIsMissing_ReturnsZeroToolCount()
    {
        var category = new Category
        {
            Id = 10,
            Name = "Old Name",
            Tools = null!
        };
        var service = CreateService(new InMemoryCategoryRepository([category]));

        var result = await service.UpdateCategoryAsync(10, new UpdateCategoryRequest("New Name", null, null));

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value!.ToolCount);
    }

    [Fact]
    public async Task UpdateCategoryAsync_WhenNameAlreadyExists_ReturnsConflict()
    {
        var service = CreateService(new InMemoryCategoryRepository(
        [
            new Category { Id = 1, Name = "Access" },
            new Category { Id = 2, Name = "Building" }
        ]));

        var result = await service.UpdateCategoryAsync(2, new UpdateCategoryRequest("access", null, null));

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.FailureType);
        Assert.Equal("A category named 'access' already exists.", result.Error);
    }

    [Fact]
    public async Task DeleteCategoryAsync_WhenCategoryDoesNotExist_ReturnsNotFound()
    {
        var service = CreateService();

        var result = await service.DeleteCategoryAsync(404);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.FailureType);
    }

    [Fact]
    public async Task DeleteCategoryAsync_WhenCategoryHasNoTools_DeletesAndSaves()
    {
        var category = new Category { Id = 6, Name = "Seasonal" };
        var repository = new InMemoryCategoryRepository([category]);
        var unitOfWork = new FakeUnitOfWork();
        var service = CreateService(repository, unitOfWork);

        var result = await service.DeleteCategoryAsync(6);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
        Assert.Null(await repository.GetByIdAsync(6));
    }

    [Fact]
    public async Task GetAllCategoriesAsync_ReturnsAlphabeticallySortedCategories()
    {
        var service = CreateService(new InMemoryCategoryRepository(
        [
            new Category { Id = 2, Name = "Zipping" },
            new Category { Id = 1, Name = "Access" }
        ]));

        var result = await service.GetAllCategoriesAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(["Access", "Zipping"], result.Value!.Select(category => category.Name).ToArray());
    }

    [Fact]
    public async Task GetFeaturedCategoriesAsync_ReturnsHomepageCategoriesWithImages()
    {
        var service = CreateService(new InMemoryCategoryRepository(
        [
            new Category
            {
                Id = 2,
                Name = "Breaking & Drilling",
                Description = "Core drilling and demolition equipment.",
                ImageUrl = "breaking-drilling.jpg",
                Tools = [CreateTool(1, "SDS Max Drill")]
            }
        ]));

        var result = await service.GetFeaturedCategoriesAsync();

        Assert.True(result.IsSuccess);
        var category = Assert.Single(result.Value!);
        Assert.Equal("Breaking & Drilling", category.Name);
        Assert.Equal("breaking-drilling.jpg", category.ImageUrl);
        Assert.Equal(1, category.ToolCount);
    }

    private static CategoryService CreateService(InMemoryCategoryRepository? repository = null, FakeUnitOfWork? unitOfWork = null)
    {
        return new CategoryService(
            repository ?? new InMemoryCategoryRepository(),
            unitOfWork ?? new FakeUnitOfWork());
    }

    private static Tool CreateTool(int id, string name)
    {
        return new Tool
        {
            Id = id,
            CategoryId = 1,
            Name = name,
            Description = $"{name} description",
            HourlyRate = 10,
            DailyRate = 35,
            WeeklyRate = 100
        };
    }
}
