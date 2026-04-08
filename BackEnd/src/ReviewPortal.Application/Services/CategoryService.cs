using ReviewPortal.Application.Common;
using ReviewPortal.Application.DTOs.Categories;
using ReviewPortal.Application.Interfaces;
using ReviewPortal.Domain.Interfaces;

namespace ReviewPortal.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CategoryService(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<CategoryDto>>> GetAllCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _categoryRepository.GetAllWithToolsAsync(cancellationToken);

        var categoryDtos = categories.Select(c => new CategoryDto(
            c.Id,
            c.Name,
            c.Description,
            c.ImageUrl,
            c.Tools.Count
        ))
        .OrderBy(c => c.Name)
        .ToList();

        return Result<IReadOnlyList<CategoryDto>>.Success(categoryDtos);
    }

    public async Task<Result<CategoryDto>> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdWithToolsAsync(id, cancellationToken);
        if (category is null)
        {
            return Result<CategoryDto>.NotFound($"Category with ID {id} not found.");
        }

        var categoryDto = new CategoryDto(
            category.Id,
            category.Name,
            category.Description,
            category.ImageUrl,
            category.Tools?.Count ?? 0
        );

        return Result<CategoryDto>.Success(categoryDto);
    }

    public async Task<Result<CategoryDto>> CreateCategoryAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var validationError = ValidateName(request.Name);
        if (validationError is not null)
        {
            return Result<CategoryDto>.Failure(validationError);
        }

        var normalizedName = request.Name.Trim();
        if (await _categoryRepository.ExistsByNameAsync(normalizedName, cancellationToken))
        {
            return Result<CategoryDto>.Conflict($"A category named '{normalizedName}' already exists.");
        }

        var category = new ReviewPortal.Domain.Entities.Category
        {
            Name = normalizedName,
            Description = NormalizeOptionalValue(request.Description),
            ImageUrl = NormalizeOptionalValue(request.ImageUrl)
        };

        var createdCategory = await _categoryRepository.AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var categoryDto = new CategoryDto(
            createdCategory.Id,
            createdCategory.Name,
            createdCategory.Description,
            createdCategory.ImageUrl,
            0
        );

        return Result<CategoryDto>.Success(categoryDto);
    }

    public async Task<Result<CategoryDto>> UpdateCategoryAsync(int id, UpdateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var validationError = ValidateName(request.Name);
        if (validationError is not null)
        {
            return Result<CategoryDto>.Failure(validationError);
        }

        var category = await _categoryRepository.GetByIdWithToolsAsync(id, cancellationToken);
        if (category is null)
        {
            return Result<CategoryDto>.NotFound($"Category with ID {id} not found.");
        }

        var normalizedName = request.Name.Trim();
        if (await _categoryRepository.ExistsByNameAsync(normalizedName, cancellationToken, id))
        {
            return Result<CategoryDto>.Conflict($"A category named '{normalizedName}' already exists.");
        }

        category.Name = normalizedName;
        category.Description = NormalizeOptionalValue(request.Description);
        category.ImageUrl = NormalizeOptionalValue(request.ImageUrl);

        await _categoryRepository.UpdateAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var categoryDto = new CategoryDto(
            category.Id,
            category.Name,
            category.Description,
            category.ImageUrl,
            category.Tools?.Count ?? 0
        );

        return Result<CategoryDto>.Success(categoryDto);
    }

    public async Task<Result<bool>> DeleteCategoryAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdWithToolsAsync(id, cancellationToken);
        if (category is null)
        {
            return Result<bool>.NotFound($"Category with ID {id} not found.");
        }

        if (category.Tools.Count > 0)
        {
            return Result<bool>.Conflict($"Cannot delete category with ID {id} because it has associated tools.");
        }

        await _categoryRepository.DeleteAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }

    private static string? ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "Category name is required.";
        }

        if (name.Trim().Length > 100)
        {
            return "Category name must be 100 characters or fewer.";
        }

        return null;
    }

    private static string? NormalizeOptionalValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
