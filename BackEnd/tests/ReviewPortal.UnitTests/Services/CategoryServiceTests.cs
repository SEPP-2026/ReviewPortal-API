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

    private static CategoryService CreateService(InMemoryCategoryRepository? repository = null)
    {
        return new CategoryService(
            repository ?? new InMemoryCategoryRepository(),
            new FakeUnitOfWork());
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
