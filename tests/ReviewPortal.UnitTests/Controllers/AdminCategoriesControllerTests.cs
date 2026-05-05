using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReviewPortal.API.Controllers.Admin;
using ReviewPortal.Application.Common;
using ReviewPortal.Application.DTOs.Categories;
using ReviewPortal.Application.Interfaces;

namespace ReviewPortal.UnitTests.Controllers;

public class AdminCategoriesControllerTests
{
    [Fact]
    public void Controller_HasAuthorizeAttributeForAdminRole()
    {
        var attribute = typeof(AdminCategoriesController).GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(attribute);
        Assert.Equal("Admin", attribute!.Roles);
    }

    [Fact]
    public void Controller_WhenNoTokenIsProvided_IsProtectedByAuthorizeAttribute()
    {
        var attribute = typeof(AdminCategoriesController).GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(attribute);
    }

    [Fact]
    public void Controller_WhenCustomerRoleIsUsed_IsExcludedFromAllowedRoles()
    {
        var attribute = typeof(AdminCategoriesController).GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(attribute);
        Assert.DoesNotContain("Customer", attribute!.Roles!.Split(','));
    }

    [Fact]
    public void Controller_UsesAdminCategoriesRoute()
    {
        var attribute = typeof(AdminCategoriesController).GetCustomAttribute<RouteAttribute>();

        Assert.NotNull(attribute);
        Assert.Equal("api/admin/categories", attribute!.Template);
    }

    [Fact]
    public async Task Create_WhenCategoryIsCreated_ReturnsCreatedAndPassesRequest()
    {
        var category = CreateCategoryDto(id: 12);
        var categoryService = new FakeCategoryService
        {
            CreateCategoryResult = Result<CategoryDto>.Success(category)
        };
        var controller = new AdminCategoriesController(categoryService);
        var request = new CreateCategoryRequest(
            "Access & Lifting",
            "Safe access platforms and lifting equipment.",
            "access.jpg");

        var result = await controller.Create(request, CancellationToken.None);

        var createdResult = Assert.IsType<CreatedResult>(result);
        Assert.Equal("/api/categories/12", createdResult.Location);
        Assert.Same(category, createdResult.Value);
        Assert.Equal(request, categoryService.LastCreateRequest);
    }

    [Fact]
    public async Task Create_WhenValidationFails_ReturnsBadRequestProblem()
    {
        var categoryService = new FakeCategoryService
        {
            CreateCategoryResult = Result<CategoryDto>.Failure("Category name is required.")
        };
        var controller = new AdminCategoriesController(categoryService);

        var result = await controller.Create(
            new CreateCategoryRequest("", null, null),
            CancellationToken.None);

        var problemResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, problemResult.StatusCode);
    }

    [Fact]
    public async Task Update_WhenCategoryIsUpdated_ReturnsOkAndPassesRequest()
    {
        var category = CreateCategoryDto(id: 7);
        var categoryService = new FakeCategoryService
        {
            UpdateCategoryResult = Result<CategoryDto>.Success(category)
        };
        var controller = new AdminCategoriesController(categoryService);
        var request = new UpdateCategoryRequest(
            "Electrical & Heating",
            "Updated heating and drying equipment.",
            "heating.jpg");

        var result = await controller.Update(7, request, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(category, okResult.Value);
        Assert.Equal(7, categoryService.LastUpdateId);
        Assert.Equal(request, categoryService.LastUpdateRequest);
    }

    [Fact]
    public async Task Update_WhenCategoryDoesNotExist_ReturnsNotFoundProblem()
    {
        var categoryService = new FakeCategoryService
        {
            UpdateCategoryResult = Result<CategoryDto>.NotFound("Category with ID 404 not found.")
        };
        var controller = new AdminCategoriesController(categoryService);

        var result = await controller.Update(
            404,
            new UpdateCategoryRequest("Missing", null, null),
            CancellationToken.None);

        var problemResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, problemResult.StatusCode);
    }

    [Fact]
    public async Task Delete_WhenCategoryIsDeleted_ReturnsNoContentAndPassesId()
    {
        var categoryService = new FakeCategoryService
        {
            DeleteCategoryResult = Result<bool>.Success(true)
        };
        var controller = new AdminCategoriesController(categoryService);

        var result = await controller.Delete(9, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        Assert.Equal(9, categoryService.LastDeleteId);
    }

    [Fact]
    public async Task Delete_WhenCategoryHasTools_ReturnsConflictProblem()
    {
        var categoryService = new FakeCategoryService
        {
            DeleteCategoryResult = Result<bool>.Conflict("Cannot delete a category that still contains tools.")
        };
        var controller = new AdminCategoriesController(categoryService);

        var result = await controller.Delete(9, CancellationToken.None);

        var problemResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status409Conflict, problemResult.StatusCode);
    }

    private static CategoryDto CreateCategoryDto(int id)
    {
        return new CategoryDto(
            id,
            "Access & Lifting",
            "Safe access platforms and lifting equipment.",
            "access.jpg",
            4);
    }

    private sealed class FakeCategoryService : ICategoryService
    {
        public CreateCategoryRequest? LastCreateRequest { get; private set; }

        public int? LastUpdateId { get; private set; }

        public UpdateCategoryRequest? LastUpdateRequest { get; private set; }

        public int? LastDeleteId { get; private set; }

        public Result<CategoryDto> CreateCategoryResult { get; set; } =
            Result<CategoryDto>.Success(CreateCategoryDto(id: 1));

        public Result<CategoryDto> UpdateCategoryResult { get; set; } =
            Result<CategoryDto>.Success(CreateCategoryDto(id: 1));

        public Result<bool> DeleteCategoryResult { get; set; } = Result<bool>.Success(true);

        public Task<Result<IReadOnlyList<CategoryDto>>> GetAllCategoriesAsync(CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<Result<IReadOnlyList<CategoryDto>>> GetFeaturedCategoriesAsync(CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<Result<CategoryDto>> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<Result<CategoryDto>> CreateCategoryAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
        {
            LastCreateRequest = request;
            return Task.FromResult(CreateCategoryResult);
        }

        public Task<Result<CategoryDto>> UpdateCategoryAsync(int id, UpdateCategoryRequest request, CancellationToken cancellationToken = default)
        {
            LastUpdateId = id;
            LastUpdateRequest = request;
            return Task.FromResult(UpdateCategoryResult);
        }

        public Task<Result<bool>> DeleteCategoryAsync(int id, CancellationToken cancellationToken = default)
        {
            LastDeleteId = id;
            return Task.FromResult(DeleteCategoryResult);
        }
    }
}
