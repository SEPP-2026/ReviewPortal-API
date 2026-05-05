using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReviewPortal.API.Controllers.Admin;
using ReviewPortal.Application.Common;
using ReviewPortal.Application.DTOs.Tools;
using ReviewPortal.Application.Interfaces;

namespace ReviewPortal.UnitTests.Controllers;

public class AdminToolsControllerTests
{
    [Fact]
    public void Controller_HasAuthorizeAttributeForAdminRole()
    {
        var attribute = typeof(AdminToolsController).GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(attribute);
        Assert.Equal("Admin", attribute!.Roles);
    }

    [Fact]
    public void Controller_WhenNoTokenIsProvided_IsProtectedByAuthorizeAttribute()
    {
        var attribute = typeof(AdminToolsController).GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(attribute);
    }

    [Fact]
    public void Controller_WhenCustomerRoleIsUsed_IsExcludedFromAllowedRoles()
    {
        var attribute = typeof(AdminToolsController).GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(attribute);
        Assert.DoesNotContain("Customer", attribute!.Roles!.Split(','));
    }

    [Fact]
    public void Controller_UsesAdminToolsRoute()
    {
        var attribute = typeof(AdminToolsController).GetCustomAttribute<RouteAttribute>();

        Assert.NotNull(attribute);
        Assert.Equal("api/admin/tools", attribute!.Template);
    }

    [Fact]
    public async Task Create_WhenToolIsCreated_ReturnsCreatedAndPassesRequest()
    {
        var tool = CreateToolDto(id: 12);
        var toolService = new FakeToolService
        {
            CreateToolResult = Result<ToolDto>.Success(tool)
        };
        var controller = CreateController(toolService);
        var request = CreateToolRequest();

        var result = await controller.Create(request, CancellationToken.None);

        var createdResult = Assert.IsType<CreatedResult>(result);
        Assert.Equal("/api/tools/12", createdResult.Location);
        Assert.Same(tool, createdResult.Value);
        Assert.Equal(request, toolService.LastCreateToolRequest);
    }

    [Fact]
    public async Task Create_WhenValidationFails_ReturnsBadRequestProblem()
    {
        var toolService = new FakeToolService
        {
            CreateToolResult = Result<ToolDto>.Failure("Name is required.")
        };
        var controller = CreateController(toolService);

        var result = await controller.Create(CreateToolRequest(name: ""), CancellationToken.None);

        var problemResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, problemResult.StatusCode);
    }

    [Fact]
    public async Task Update_WhenToolIsUpdated_ReturnsOkAndPassesRequest()
    {
        var tool = CreateToolDto(id: 9);
        var toolService = new FakeToolService
        {
            UpdateToolResult = Result<ToolDto>.Success(tool)
        };
        var controller = CreateController(toolService);
        var request = UpdateToolRequest();

        var result = await controller.Update(9, request, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(tool, okResult.Value);
        Assert.Equal(9, toolService.LastUpdateToolId);
        Assert.Equal(request, toolService.LastUpdateToolRequest);
    }

    [Fact]
    public async Task Update_WhenToolDoesNotExist_ReturnsNotFoundProblem()
    {
        var toolService = new FakeToolService
        {
            UpdateToolResult = Result<ToolDto>.NotFound("Tool with ID 404 not found.")
        };
        var controller = CreateController(toolService);

        var result = await controller.Update(404, UpdateToolRequest(), CancellationToken.None);

        var problemResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, problemResult.StatusCode);
    }

    [Fact]
    public async Task SetStatus_WhenToolStatusIsChanged_ReturnsOkAndPassesStatus()
    {
        var toolService = new FakeToolService
        {
            SetToolStatusResult = Result<bool>.Success(true)
        };
        var controller = CreateController(toolService);
        var request = new SetToolStatusRequest(false);

        var result = await controller.SetStatus(15, request, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.True(Assert.IsType<bool>(okResult.Value));
        Assert.Equal(15, toolService.LastSetStatusToolId);
        Assert.Equal(false, toolService.LastSetStatusIsActive);
    }

    [Fact]
    public async Task SetStatus_WhenToolDoesNotExist_ReturnsNotFoundProblem()
    {
        var toolService = new FakeToolService
        {
            SetToolStatusResult = Result<bool>.NotFound("Tool with ID 404 not found.")
        };
        var controller = CreateController(toolService);

        var result = await controller.SetStatus(404, new SetToolStatusRequest(false), CancellationToken.None);

        var problemResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, problemResult.StatusCode);
    }

    [Fact]
    public async Task UploadImage_WhenImageIsUploaded_ReturnsCreatedAndPassesFile()
    {
        var image = new ToolImageDto(4, "/uploads/tools/15-image.jpg", 2);
        var imageService = new FakeImageService
        {
            UploadImageResult = Result<ToolImageDto>.Success(image)
        };
        var controller = CreateController(imageService: imageService);
        var file = CreateFormFile("hammer.jpg", length: 16);

        var result = await controller.UploadImage(15, file, CancellationToken.None);

        var createdResult = Assert.IsType<CreatedResult>(result);
        Assert.Equal("/api/tools/15/images/4", createdResult.Location);
        Assert.Same(image, createdResult.Value);
        Assert.Equal(15, imageService.LastUploadToolId);
        Assert.Equal("hammer.jpg", imageService.LastUploadFileName);
        Assert.Equal(16, imageService.LastUploadByteCount);
    }

    [Fact]
    public async Task UploadImage_WhenInvalidFormat_ReturnsBadRequestProblem()
    {
        var imageService = new FakeImageService
        {
            UploadImageResult = Result<ToolImageDto>.Failure("Only .jpg, .jpeg, .png, and .webp image files are allowed.")
        };
        var controller = CreateController(imageService: imageService);

        var result = await controller.UploadImage(15, CreateFormFile("hammer.gif"), CancellationToken.None);

        var problemResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, problemResult.StatusCode);
    }

    [Fact]
    public async Task UploadImage_WhenFileIsTooLarge_ReturnsBadRequestProblem()
    {
        var imageService = new FakeImageService
        {
            UploadImageResult = Result<ToolImageDto>.Failure("Image file must be 5MB or smaller.")
        };
        var controller = CreateController(imageService: imageService);

        var result = await controller.UploadImage(15, CreateFormFile("hammer.jpg"), CancellationToken.None);

        var problemResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, problemResult.StatusCode);
    }

    [Fact]
    public async Task DeleteImage_WhenImageIsDeleted_ReturnsOkAndPassesIds()
    {
        var imageService = new FakeImageService
        {
            DeleteImageResult = Result<bool>.Success(true)
        };
        var controller = CreateController(imageService: imageService);

        var result = await controller.DeleteImage(15, 4, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.True(Assert.IsType<bool>(okResult.Value));
        Assert.Equal(15, imageService.LastDeleteToolId);
        Assert.Equal(4, imageService.LastDeleteImageId);
    }

    [Fact]
    public async Task DeleteImage_WhenDeletingLastImage_ReturnsBadRequestProblem()
    {
        var imageService = new FakeImageService
        {
            DeleteImageResult = Result<bool>.Failure("Cannot delete the last image")
        };
        var controller = CreateController(imageService: imageService);

        var result = await controller.DeleteImage(15, 4, CancellationToken.None);

        var problemResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, problemResult.StatusCode);
    }

    [Fact]
    public async Task DeleteImage_WhenImageDoesNotExist_ReturnsNotFoundProblem()
    {
        var imageService = new FakeImageService
        {
            DeleteImageResult = Result<bool>.NotFound("Image with ID 404 not found for tool 15.")
        };
        var controller = CreateController(imageService: imageService);

        var result = await controller.DeleteImage(15, 404, CancellationToken.None);

        var problemResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, problemResult.StatusCode);
    }

    private static AdminToolsController CreateController(
        FakeToolService? toolService = null,
        FakeImageService? imageService = null)
    {
        return new AdminToolsController(
            toolService ?? new FakeToolService(),
            imageService ?? new FakeImageService());
    }

    private static IFormFile CreateFormFile(string fileName, int length = 16)
    {
        var content = Enumerable.Repeat((byte)1, length).ToArray();
        var stream = new MemoryStream(content);

        return new FormFile(stream, 0, stream.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };
    }

    private static CreateToolRequest CreateToolRequest(string name = "Rotary Hammer")
    {
        return new CreateToolRequest(
            CategoryId: 3,
            Name: name,
            Description: "Heavy-duty hammer drill for masonry and concrete work.",
            HourlyRate: 8.50m,
            DailyRate: 45.00m,
            WeeklyRate: 180.00m,
            SpecialNotes: "Includes SDS bits.",
            DepositRequired: true,
            DepositAmount: 50.00m,
            ImageUrl: "/uploads/tools/rotary-hammer.jpg");
    }

    private static UpdateToolRequest UpdateToolRequest()
    {
        return new UpdateToolRequest(
            CategoryId: 3,
            Name: "Rotary Hammer Pro",
            Description: "Updated heavy-duty hammer drill listing.",
            HourlyRate: 9.00m,
            DailyRate: 48.00m,
            WeeklyRate: 190.00m,
            SpecialNotes: "Includes SDS bits and carry case.",
            DepositRequired: true,
            DepositAmount: 55.00m);
    }

    private static ToolDto CreateToolDto(int id)
    {
        return new ToolDto(
            Id: id,
            CategoryId: 3,
            CategoryName: "Power Tools",
            Name: "Rotary Hammer",
            Description: "Heavy-duty hammer drill for masonry and concrete work.",
            HourlyRate: 8.50m,
            DailyRate: 45.00m,
            WeeklyRate: 180.00m,
            SpecialNotes: "Includes SDS bits.",
            DepositRequired: true,
            DepositAmount: 50.00m,
            IsActive: true,
            OverallRating: 4.6m,
            ReviewCount: 8,
            HasEnoughReviewsToRate: true,
            RatingMessage: null,
            Images: [],
            CreatedDate: new DateTime(2026, 4, 20, 9, 0, 0, DateTimeKind.Utc),
            UpdatedDate: new DateTime(2026, 4, 20, 9, 0, 0, DateTimeKind.Utc));
    }

    private sealed class FakeImageService : IImageService
    {
        public int? LastUploadToolId { get; private set; }

        public string? LastUploadFileName { get; private set; }

        public long? LastUploadByteCount { get; private set; }

        public int? LastDeleteToolId { get; private set; }

        public int? LastDeleteImageId { get; private set; }

        public Result<ToolImageDto> UploadImageResult { get; set; } =
            Result<ToolImageDto>.Success(new ToolImageDto(1, "/uploads/tools/image.jpg", 1));

        public Result<bool> DeleteImageResult { get; set; } = Result<bool>.Success(true);

        public async Task<Result<ToolImageDto>> UploadImageAsync(
            int toolId,
            Stream fileStream,
            string fileName,
            CancellationToken cancellationToken = default)
        {
            LastUploadToolId = toolId;
            LastUploadFileName = fileName;

            using var memoryStream = new MemoryStream();
            await fileStream.CopyToAsync(memoryStream, cancellationToken);
            LastUploadByteCount = memoryStream.Length;

            return UploadImageResult;
        }

        public Task<Result<bool>> DeleteImageAsync(
            int toolId,
            int imageId,
            CancellationToken cancellationToken = default)
        {
            LastDeleteToolId = toolId;
            LastDeleteImageId = imageId;

            return Task.FromResult(DeleteImageResult);
        }
    }

    private sealed class FakeToolService : IToolService
    {
        public CreateToolRequest? LastCreateToolRequest { get; private set; }

        public int? LastUpdateToolId { get; private set; }

        public UpdateToolRequest? LastUpdateToolRequest { get; private set; }

        public int? LastSetStatusToolId { get; private set; }

        public bool? LastSetStatusIsActive { get; private set; }

        public Result<ToolDto> CreateToolResult { get; set; } =
            Result<ToolDto>.Success(CreateToolDto(id: 1));

        public Result<ToolDto> UpdateToolResult { get; set; } =
            Result<ToolDto>.Success(CreateToolDto(id: 1));

        public Result<bool> SetToolStatusResult { get; set; } = Result<bool>.Success(true);

        public Task<Result<PagedList<ToolSummaryDto>>> GetToolsByCategoryAsync(
            int categoryId,
            int page,
            int pageSize,
            string? sortBy = null,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<Result<ToolDto>> GetToolByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<Result<PagedList<ToolSummaryDto>>> SearchToolsAsync(
            string query,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<Result<PagedList<ToolSummaryDto>>> FilterByPriceRangeAsync(
            int categoryId,
            decimal? minPrice,
            decimal? maxPrice,
            int page,
            int pageSize,
            string? sortBy = null,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<Result<RentalCalculationResponse>> CalculateRentalCostAsync(
            int toolId,
            RentalCalculationRequest request,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<Result<ToolDto>> CreateToolAsync(
            CreateToolRequest request,
            CancellationToken cancellationToken = default)
        {
            LastCreateToolRequest = request;
            return Task.FromResult(CreateToolResult);
        }

        public Task<Result<ToolDto>> UpdateToolAsync(
            int id,
            UpdateToolRequest request,
            CancellationToken cancellationToken = default)
        {
            LastUpdateToolId = id;
            LastUpdateToolRequest = request;
            return Task.FromResult(UpdateToolResult);
        }

        public Task<Result<bool>> SetToolStatusAsync(
            int id,
            bool isActive,
            CancellationToken cancellationToken = default)
        {
            LastSetStatusToolId = id;
            LastSetStatusIsActive = isActive;
            return Task.FromResult(SetToolStatusResult);
        }
    }
}
