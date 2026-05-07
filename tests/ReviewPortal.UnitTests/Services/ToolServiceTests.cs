using ReviewPortal.Application.Common;
using ReviewPortal.Application.DTOs.Tools;
using ReviewPortal.Application.Services;
using ReviewPortal.Domain.Entities;
using ReviewPortal.Domain.Enums;
using ReviewPortal.UnitTests.TestDoubles;

namespace ReviewPortal.UnitTests.Services;

public class ToolServiceTests
{
    [Fact]
    public async Task GetToolsByCategoryAsync_WhenCategoryDoesNotExist_ReturnsNotFound()
    {
        var service = CreateService();

        var result = await service.GetToolsByCategoryAsync(404, 1, 12);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.FailureType);
    }

    [Fact]
    public async Task GetToolsByCategoryAsync_WhenCategoryHasNoTools_ReturnsEmptyPagedResult()
    {
        var category = new Category { Id = 1, Name = "Access" };
        var service = CreateService(categories: [category]);

        var result = await service.GetToolsByCategoryAsync(1, 1, 12);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.Items);
        Assert.Equal(0, result.Value.TotalCount);
    }

    [Fact]
    public async Task GetToolsByCategoryAsync_DefaultSort_ReturnsAlphabeticalPageOfActiveTools()
    {
        var category = new Category { Id = 1, Name = "Access" };
        var service = CreateService(
            categories: [category],
            tools:
            [
                CreateTool(1, category, "Tower Scaffold", hourlyRate: 20m, isActive: true),
                CreateTool(2, category, "Access Platform", hourlyRate: 15m, isActive: true),
                CreateTool(3, category, "Boom Lift", hourlyRate: 18m, isActive: false)
            ]);

        var result = await service.GetToolsByCategoryAsync(1, 1, 12);

        Assert.True(result.IsSuccess);
        Assert.Equal(["Access Platform", "Tower Scaffold"], result.Value!.Items.Select(item => item.Name).ToArray());
    }

    [Fact]
    public async Task GetToolsByCategoryAsync_PriceSortDescending_ReturnsHighestStartingPriceFirst()
    {
        var category = new Category { Id = 1, Name = "Breaking & Drilling" };
        var service = CreateService(
            categories: [category],
            tools:
            [
                CreateTool(1, category, "SDS Drill", hourlyRate: 12m, dailyRate: 42m, weeklyRate: 100m),
                CreateTool(2, category, "Hydraulic Breaker", hourlyRate: 30m, dailyRate: 96m, weeklyRate: 240m),
                CreateTool(3, category, "Diamond Core Drill", hourlyRate: 18m, dailyRate: 70m, weeklyRate: 180m)
            ]);

        var result = await service.GetToolsByCategoryAsync(1, 1, 12, "price_desc");

        Assert.True(result.IsSuccess);
        Assert.Equal(
            ["Hydraulic Breaker", "Diamond Core Drill", "SDS Drill"],
            result.Value!.Items.Select(item => item.Name).ToArray());
    }

    [Fact]
    public async Task GetToolsByCategoryAsync_NameSortDescending_ReturnsReverseAlphabeticalTools()
    {
        var category = new Category { Id = 1, Name = "Access & Lifting" };
        var service = CreateService(
            categories: [category],
            tools:
            [
                CreateTool(1, category, "Access Platform", hourlyRate: 15m),
                CreateTool(2, category, "Tower Scaffold", hourlyRate: 20m),
                CreateTool(3, category, "Boom Lift", hourlyRate: 18m)
            ]);

        var result = await service.GetToolsByCategoryAsync(1, 1, 12, "name_desc");

        Assert.True(result.IsSuccess);
        Assert.Equal(["Tower Scaffold", "Boom Lift", "Access Platform"], result.Value!.Items.Select(item => item.Name).ToArray());
    }

    [Fact]
    public async Task GetToolsByCategoryAsync_PriceSortAscending_AcceptsStartingPriceAlias()
    {
        var category = new Category { Id = 1, Name = "Building & Construction" };
        var service = CreateService(
            categories: [category],
            tools:
            [
                CreateTool(1, category, "Concrete Saw", hourlyRate: 18m, dailyRate: 65m, weeklyRate: 220m),
                CreateTool(2, category, "Acrow Prop", hourlyRate: 6m, dailyRate: 20m, weeklyRate: 65m),
                CreateTool(3, category, "Floor Sander", hourlyRate: 17m, dailyRate: 63m, weeklyRate: 210m)
            ]);

        var result = await service.GetToolsByCategoryAsync(1, 1, 12, "starting-price");

        Assert.True(result.IsSuccess);
        Assert.Equal(["Acrow Prop", "Floor Sander", "Concrete Saw"], result.Value!.Items.Select(item => item.Name).ToArray());
    }

    [Fact]
    public async Task GetToolsByCategoryAsync_RatingSortsByHighestRatingFirst()
    {
        var category = new Category { Id = 1, Name = "Cleaning & Maintenance" };
        var pressureWasher = CreateTool(1, category, "Pressure Washer", hourlyRate: 14m);
        pressureWasher.OverallRating = 1m;
        pressureWasher.Reviews =
        [
            CreateApprovedReview(1, pressureWasher, 5, 5, 5, 5, 4),
            CreateApprovedReview(2, pressureWasher, 5, 4, 5, 4, 5)
        ];
        var wetVacuum = CreateTool(2, category, "Wet Vacuum", hourlyRate: 9m);
        wetVacuum.OverallRating = 5m;
        wetVacuum.Reviews =
        [
            CreateApprovedReview(3, wetVacuum, 4, 4, 3, 4, 3),
            CreateApprovedReview(4, wetVacuum, 3, 4, 4, 3, 4)
        ];
        var scrubber = CreateTool(3, category, "Floor Scrubber", hourlyRate: 18m);
        scrubber.Reviews =
        [
            CreateApprovedReview(5, scrubber, 5, 5, 5, 5, 5)
        ];

        var service = CreateService(categories: [category], tools: [wetVacuum, scrubber, pressureWasher]);

        var result = await service.GetToolsByCategoryAsync(1, 1, 12, "rating");

        Assert.True(result.IsSuccess);
        Assert.Equal(["Pressure Washer", "Wet Vacuum", "Floor Scrubber"], result.Value!.Items.Select(item => item.Name).ToArray());
        Assert.Equal([4.7m, 3.6m, null], result.Value.Items.Select(item => item.OverallRating).ToArray());
        Assert.Equal([true, true, false], result.Value.Items.Select(item => item.HasEnoughReviewsToRate).ToArray());
        Assert.Equal([null, null, "Not enough reviews to rate"], result.Value.Items.Select(item => item.RatingMessage).ToArray());
    }

    [Fact]
    public async Task GetToolsByCategoryAsync_RatingAscendingSortsUnratedToolsFirst()
    {
        var category = new Category { Id = 1, Name = "Cleaning & Maintenance" };
        var pressureWasher = CreateTool(1, category, "Pressure Washer", hourlyRate: 14m);
        pressureWasher.Reviews =
        [
            CreateApprovedReview(1, pressureWasher, 5, 5, 5, 5, 4),
            CreateApprovedReview(2, pressureWasher, 5, 4, 5, 4, 5)
        ];
        var wetVacuum = CreateTool(2, category, "Wet Vacuum", hourlyRate: 9m);
        wetVacuum.Reviews =
        [
            CreateApprovedReview(3, wetVacuum, 4, 4, 3, 4, 3),
            CreateApprovedReview(4, wetVacuum, 3, 4, 4, 3, 4)
        ];
        var scrubber = CreateTool(3, category, "Floor Scrubber", hourlyRate: 18m);
        scrubber.Reviews =
        [
            CreateApprovedReview(5, scrubber, 5, 5, 5, 5, 5)
        ];

        var service = CreateService(categories: [category], tools: [wetVacuum, scrubber, pressureWasher]);

        var result = await service.GetToolsByCategoryAsync(1, 1, 12, "rating_asc");

        Assert.True(result.IsSuccess);
        Assert.Equal(["Floor Scrubber", "Wet Vacuum", "Pressure Washer"], result.Value!.Items.Select(item => item.Name).ToArray());
    }

    [Theory]
    [InlineData("price")]
    [InlineData("price_asc")]
    [InlineData("starting_price_desc")]
    [InlineData("rating_desc")]
    public async Task GetToolsByCategoryAsync_SortAliasesAreAccepted(string sortBy)
    {
        var category = new Category { Id = 1, Name = "Cleaning & Maintenance" };
        var pressureWasher = CreateTool(1, category, "Pressure Washer", hourlyRate: 14m);
        pressureWasher.Reviews =
        [
            CreateApprovedReview(1, pressureWasher, 5, 5, 5, 5, 4),
            CreateApprovedReview(2, pressureWasher, 5, 4, 5, 4, 5)
        ];
        var wetVacuum = CreateTool(2, category, "Wet Vacuum", hourlyRate: 9m);
        wetVacuum.Reviews =
        [
            CreateApprovedReview(3, wetVacuum, 4, 4, 3, 4, 3),
            CreateApprovedReview(4, wetVacuum, 3, 4, 4, 3, 4)
        ];

        var service = CreateService(categories: [category], tools: [wetVacuum, pressureWasher]);

        var result = await service.GetToolsByCategoryAsync(1, 1, 12, sortBy);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.TotalCount);
    }

    [Fact]
    public async Task GetToolsByCategoryAsync_Pagination_ReturnsSecondPageWithTotalCount()
    {
        var category = new Category { Id = 1, Name = "Cleaning & Maintenance" };
        var service = CreateService(
            categories: [category],
            tools: Enumerable.Range(1, 14)
                .Select(index => CreateTool(index, category, $"Tool {index:00}", hourlyRate: index))
                .ToArray());

        var result = await service.GetToolsByCategoryAsync(1, 2, 12);

        Assert.True(result.IsSuccess);
        Assert.Equal(14, result.Value!.TotalCount);
        Assert.Equal(2, result.Value.TotalPages);
        Assert.Equal(["Tool 13", "Tool 14"], result.Value.Items.Select(item => item.Name).ToArray());
    }

    [Fact]
    public async Task GetToolsByCategoryAsync_MapsThumbnailAndStartingPrice()
    {
        var category = new Category { Id = 1, Name = "Garden & Landscaping" };
        var tool = CreateTool(1, category, "Rotavator", hourlyRate: 14m, dailyRate: 55m, weeklyRate: 180m);
        tool.Images =
        [
            new ToolImage { Id = 2, ToolId = 1, ImageUrl = "later.jpg", DisplayOrder = 2 },
            new ToolImage { Id = 1, ToolId = 1, ImageUrl = "first.jpg", DisplayOrder = 1 }
        ];

        var service = CreateService(categories: [category], tools: [tool]);

        var result = await service.GetToolsByCategoryAsync(1, 1, 12);

        Assert.True(result.IsSuccess);
        var item = Assert.Single(result.Value!.Items);
        Assert.Equal(14m, item.StartingPrice);
        Assert.Equal("hour", item.StartingPriceUnit);
        Assert.Equal("first.jpg", item.ThumbnailUrl);
    }

    [Fact]
    public async Task GetToolsByCategoryAsync_WhenToolHasNoReviews_ReturnsThresholdMessage()
    {
        var category = new Category { Id = 1, Name = "Garden & Landscaping" };
        var tool = CreateTool(1, category, "Rotavator", hourlyRate: 14m);

        var service = CreateService(categories: [category], tools: [tool]);

        var result = await service.GetToolsByCategoryAsync(1, 1, 12);

        Assert.True(result.IsSuccess);
        var item = Assert.Single(result.Value!.Items);
        Assert.Null(item.OverallRating);
        Assert.Equal(0, item.ReviewCount);
        Assert.False(item.HasEnoughReviewsToRate);
        Assert.Equal("Not enough reviews to rate", item.RatingMessage);
    }

    [Fact]
    public async Task GetToolsByCategoryAsync_WhenToolHasFewerThanTwoApprovedReviews_ReturnsNullOverallRatingWithApprovedCount()
    {
        var category = new Category { Id = 1, Name = "Garden & Landscaping" };
        var tool = CreateTool(1, category, "Rotavator", hourlyRate: 14m);
        tool.Reviews =
        [
            CreateApprovedReview(1, tool, 5, 4, 4, 5, 4),
            CreateReview(2, tool, ReviewStatus.Pending, 1, 1, 1, 1, 1)
        ];

        var service = CreateService(categories: [category], tools: [tool]);

        var result = await service.GetToolsByCategoryAsync(1, 1, 12);

        Assert.True(result.IsSuccess);
        var item = Assert.Single(result.Value!.Items);
        Assert.Null(item.OverallRating);
        Assert.Equal(1, item.ReviewCount);
        Assert.False(item.HasEnoughReviewsToRate);
        Assert.Equal("Not enough reviews to rate", item.RatingMessage);
    }

    [Fact]
    public async Task GetToolsByCategoryAsync_InvalidSortValue_ReturnsValidationFailure()
    {
        var category = new Category { Id = 1, Name = "Painting & Decorating" };
        var service = CreateService(categories: [category]);

        var result = await service.GetToolsByCategoryAsync(1, 1, 12, "popularity");

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.FailureType);
    }

    [Fact]
    public async Task FilterByPriceRangeAsync_WhenCategoryDoesNotExist_ReturnsNotFound()
    {
        var service = CreateService();

        var result = await service.FilterByPriceRangeAsync(404, minPrice: 25m, maxPrice: 75m, page: 1, pageSize: 12);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.FailureType);
    }

    [Fact]
    public async Task GetToolsByCategoryAsync_WhenPageIsInvalid_ReturnsValidationFailure()
    {
        var service = CreateService();

        var result = await service.GetToolsByCategoryAsync(1, 0, 12);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.FailureType);
        Assert.Equal("Page must be greater than or equal to 1.", result.Error);
    }

    [Fact]
    public async Task FilterByPriceRangeAsync_WhenDailyRateRangeProvided_ReturnsMatchingActiveTools()
    {
        var category = new Category { Id = 1, Name = "Building & Construction" };
        var service = CreateService(
            categories: [category],
            tools:
            [
                CreateTool(1, category, "Budget Drill", hourlyRate: 7m, dailyRate: 25m),
                CreateTool(2, category, "Concrete Saw", hourlyRate: 18m, dailyRate: 65m),
                CreateTool(3, category, "Platform Ladder", hourlyRate: 10m, dailyRate: 40m),
                CreateTool(4, category, "Tower Scaffold", hourlyRate: 20m, dailyRate: 80m),
                CreateTool(5, category, "Retired Saw", hourlyRate: 15m, dailyRate: 55m, isActive: false)
            ]);

        var result = await service.FilterByPriceRangeAsync(1, minPrice: 40m, maxPrice: 80m, page: 1, pageSize: 12, sortBy: "name");

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value!.TotalCount);
        Assert.Equal(["Concrete Saw", "Platform Ladder", "Tower Scaffold"], result.Value.Items.Select(item => item.Name).ToArray());
        Assert.All(result.Value.Items, item => Assert.InRange(item.DailyRate, 40m, 80m));
    }

    [Fact]
    public async Task FilterByPriceRangeAsync_WhenOnlyMaximumPriceProvided_ReturnsToolsAtOrBelowMaximum()
    {
        var category = new Category { Id = 1, Name = "Cleaning & Maintenance" };
        var service = CreateService(
            categories: [category],
            tools:
            [
                CreateTool(1, category, "Industrial Wet Vacuum", hourlyRate: 9m, dailyRate: 32m),
                CreateTool(2, category, "Petrol Pressure Washer", hourlyRate: 14m, dailyRate: 52m)
            ]);

        var result = await service.FilterByPriceRangeAsync(1, minPrice: null, maxPrice: 40m, page: 1, pageSize: 12);

        Assert.True(result.IsSuccess);
        var item = Assert.Single(result.Value!.Items);
        Assert.Equal("Industrial Wet Vacuum", item.Name);
    }

    [Fact]
    public async Task FilterByPriceRangeAsync_WhenOnlyMinimumPriceProvided_ReturnsToolsAtOrAboveMinimum()
    {
        var category = new Category { Id = 1, Name = "Garden & Landscaping" };
        var service = CreateService(
            categories: [category],
            tools:
            [
                CreateTool(1, category, "Hedge Trimmer", hourlyRate: 12m, dailyRate: 44m),
                CreateTool(2, category, "Turf Cutter", hourlyRate: 19m, dailyRate: 72m)
            ]);

        var result = await service.FilterByPriceRangeAsync(1, minPrice: 50m, maxPrice: null, page: 1, pageSize: 12);

        Assert.True(result.IsSuccess);
        var item = Assert.Single(result.Value!.Items);
        Assert.Equal("Turf Cutter", item.Name);
    }

    [Fact]
    public async Task FilterByPriceRangeAsync_WhenPageSizeIsTooLarge_ReturnsValidationFailure()
    {
        var service = CreateService();

        var result = await service.FilterByPriceRangeAsync(1, minPrice: null, maxPrice: null, page: 1, pageSize: 101);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.FailureType);
        Assert.Equal("Page size must not exceed 100.", result.Error);
    }

    [Fact]
    public async Task FilterByPriceRangeAsync_WhenSortIsInvalid_ReturnsValidationFailure()
    {
        var category = new Category { Id = 1, Name = "Electrical & Heating" };
        var service = CreateService(
            categories: [category],
            tools: [CreateTool(1, category, "50L Dehumidifier", hourlyRate: 11m, dailyRate: 40m)]);

        var result = await service.FilterByPriceRangeAsync(1, minPrice: 10m, maxPrice: 80m, page: 1, pageSize: 12, sortBy: "popularity");

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.FailureType);
    }

    [Fact]
    public async Task FilterByPriceRangeAsync_WhenPriceRangeIsInvalid_ReturnsValidationFailure()
    {
        var category = new Category { Id = 1, Name = "Garden & Landscaping" };
        var service = CreateService(categories: [category]);

        var result = await service.FilterByPriceRangeAsync(1, minPrice: 90m, maxPrice: 40m, page: 1, pageSize: 12);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.FailureType);
        Assert.Equal("Minimum price must be less than or equal to maximum price.", result.Error);
    }

    [Fact]
    public async Task FilterByPriceRangeAsync_WhenPriceIsNegative_ReturnsValidationFailure()
    {
        var category = new Category { Id = 1, Name = "Garden & Landscaping" };
        var service = CreateService(categories: [category]);

        var result = await service.FilterByPriceRangeAsync(1, minPrice: -1m, maxPrice: 40m, page: 1, pageSize: 12);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.FailureType);
        Assert.Equal("Price values must be greater than or equal to 0.", result.Error);
    }

    [Fact]
    public async Task GetToolByIdAsync_WhenToolExists_ReturnsFullDetailWithOrderedImages()
    {
        var category = new Category { Id = 1, Name = "Electrical & Heating" };
        var tool = CreateTool(7, category, "50L Dehumidifier", hourlyRate: 11m, dailyRate: 40m, weeklyRate: 135m);
        tool.SpecialNotes = "Keep clear of walls for best airflow.";
        tool.DepositRequired = true;
        tool.DepositAmount = 60m;
        tool.Reviews =
        [
            CreateApprovedReview(1, tool, 5, 5, 5, 4, 4),
            CreateApprovedReview(2, tool, 5, 5, 5, 4, 4),
            CreateReview(3, tool, ReviewStatus.Pending, 1, 1, 1, 1, 1)
        ];
        tool.Images =
        [
            new ToolImage { Id = 2, ToolId = 7, ImageUrl = "second.jpg", DisplayOrder = 2 },
            new ToolImage { Id = 1, ToolId = 7, ImageUrl = "first.jpg", DisplayOrder = 1 }
        ];

        var service = CreateService(categories: [category], tools: [tool]);

        var result = await service.GetToolByIdAsync(7);

        Assert.True(result.IsSuccess);
        Assert.Equal("Electrical & Heating", result.Value!.CategoryName);
        Assert.Equal(11m, result.Value.HourlyRate);
        Assert.Equal(40m, result.Value.DailyRate);
        Assert.Equal(135m, result.Value.WeeklyRate);
        Assert.Equal("Keep clear of walls for best airflow.", result.Value.SpecialNotes);
        Assert.True(result.Value.DepositRequired);
        Assert.Equal(60m, result.Value.DepositAmount);
        Assert.Equal(4.6m, result.Value.OverallRating);
        Assert.Equal(2, result.Value.ReviewCount);
        Assert.True(result.Value.HasEnoughReviewsToRate);
        Assert.Null(result.Value.RatingMessage);
        Assert.Equal(["first.jpg", "second.jpg"], result.Value.Images.Select(image => image.ImageUrl).ToArray());
    }

    [Fact]
    public async Task GetToolByIdAsync_WhenToolHasOnlyOneApprovedReview_ReturnsNullOverallRating()
    {
        var category = new Category { Id = 1, Name = "Electrical & Heating" };
        var tool = CreateTool(7, category, "50L Dehumidifier", hourlyRate: 11m, dailyRate: 40m, weeklyRate: 135m);
        tool.Reviews =
        [
            CreateApprovedReview(1, tool, 5, 5, 5, 4, 4)
        ];

        var service = CreateService(categories: [category], tools: [tool]);

        var result = await service.GetToolByIdAsync(7);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value!.OverallRating);
        Assert.Equal(1, result.Value.ReviewCount);
        Assert.False(result.Value.HasEnoughReviewsToRate);
        Assert.Equal("Not enough reviews to rate", result.Value.RatingMessage);
    }

    [Fact]
    public async Task GetToolByIdAsync_WhenToolIsInactive_ReturnsNotFound()
    {
        var category = new Category { Id = 1, Name = "Access & Lifting" };
        var tool = CreateTool(8, category, "Material Hoist", hourlyRate: 20m, isActive: false);
        var service = CreateService(categories: [category], tools: [tool]);

        var result = await service.GetToolByIdAsync(8);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.FailureType);
    }

    [Fact]
    public async Task GetToolByIdAsync_WhenToolDoesNotExist_ReturnsNotFound()
    {
        var service = CreateService();

        var result = await service.GetToolByIdAsync(999);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.FailureType);
    }

    [Fact]
    public async Task SearchToolsAsync_WhenQueryMatchesName_ReturnsMatchingToolSummary()
    {
        var category = new Category { Id = 1, Name = "Breaking & Drilling" };
        var matchingTool = CreateTool(1, category, "SDS Max Drill", hourlyRate: 15m);
        matchingTool.Images =
        [
            new ToolImage { Id = 1, ToolId = 1, ImageUrl = "sds-drill.jpg", DisplayOrder = 1 }
        ];

        var service = CreateService(
            categories: [category],
            tools:
            [
                matchingTool,
                CreateTool(2, category, "Hydraulic Breaker", hourlyRate: 22m)
            ]);

        var result = await service.SearchToolsAsync("sds", 1, 12);

        Assert.True(result.IsSuccess);
        var item = Assert.Single(result.Value!.Items);
        Assert.Equal("SDS Max Drill", item.Name);
        Assert.Equal("Breaking & Drilling", item.CategoryName);
        Assert.Equal(15m, item.StartingPrice);
        Assert.Equal("sds-drill.jpg", item.ThumbnailUrl);
    }

    [Fact]
    public async Task SearchToolsAsync_MapsComputedRatingAndApprovedReviewCount()
    {
        var category = new Category { Id = 1, Name = "Breaking & Drilling" };
        var matchingTool = CreateTool(1, category, "SDS Max Drill", hourlyRate: 15m);
        matchingTool.Reviews =
        [
            CreateApprovedReview(1, matchingTool, 5, 4, 5, 4, 4),
            CreateApprovedReview(2, matchingTool, 4, 4, 4, 4, 4),
            CreateReview(3, matchingTool, ReviewStatus.Pending, 1, 1, 1, 1, 1)
        ];

        var service = CreateService(categories: [category], tools: [matchingTool]);

        var result = await service.SearchToolsAsync("sds", 1, 12);

        Assert.True(result.IsSuccess);
        var item = Assert.Single(result.Value!.Items);
        Assert.Equal(4.2m, item.OverallRating);
        Assert.Equal(2, item.ReviewCount);
        Assert.True(item.HasEnoughReviewsToRate);
        Assert.Null(item.RatingMessage);
    }

    [Fact]
    public async Task SearchToolsAsync_WhenToolHasOneApprovedReview_ReturnsThresholdMessage()
    {
        var category = new Category { Id = 1, Name = "Breaking & Drilling" };
        var matchingTool = CreateTool(1, category, "SDS Max Drill", hourlyRate: 15m);
        matchingTool.Reviews =
        [
            CreateApprovedReview(1, matchingTool, 5, 4, 5, 4, 4)
        ];

        var service = CreateService(categories: [category], tools: [matchingTool]);

        var result = await service.SearchToolsAsync("sds", 1, 12);

        Assert.True(result.IsSuccess);
        var item = Assert.Single(result.Value!.Items);
        Assert.Null(item.OverallRating);
        Assert.Equal(1, item.ReviewCount);
        Assert.False(item.HasEnoughReviewsToRate);
        Assert.Equal("Not enough reviews to rate", item.RatingMessage);
    }

    [Fact]
    public async Task SearchToolsAsync_WhenQueryMatchesDescriptionOrCategory_ReturnsCaseInsensitiveMatches()
    {
        var cleaning = new Category { Id = 1, Name = "Cleaning & Maintenance" };
        var heating = new Category { Id = 2, Name = "Electrical & Heating" };
        var pressureWasher = CreateTool(1, cleaning, "Petrol Pressure Washer", hourlyRate: 14m);
        pressureWasher.Description = "High-output machine for paving and site vehicles.";
        var dehumidifier = CreateTool(2, heating, "50L Dehumidifier", hourlyRate: 11m);
        var rotavator = CreateTool(3, new Category { Id = 3, Name = "Garden & Landscaping" }, "Rotavator", hourlyRate: 16m);

        var service = CreateService(
            categories: [cleaning, heating, rotavator.Category],
            tools: [pressureWasher, dehumidifier, rotavator]);

        var descriptionResult = await service.SearchToolsAsync("PAVING", 1, 12);
        var categoryResult = await service.SearchToolsAsync("heating", 1, 12);

        Assert.True(descriptionResult.IsSuccess);
        Assert.Equal(["Petrol Pressure Washer"], descriptionResult.Value!.Items.Select(item => item.Name).ToArray());
        Assert.True(categoryResult.IsSuccess);
        Assert.Equal(["50L Dehumidifier"], categoryResult.Value!.Items.Select(item => item.Name).ToArray());
    }

    [Fact]
    public async Task SearchToolsAsync_WhenToolIsInactive_ExcludesTool()
    {
        var category = new Category { Id = 1, Name = "Access & Lifting" };
        var service = CreateService(
            categories: [category],
            tools:
            [
                CreateTool(1, category, "Platform Ladder", hourlyRate: 10m, isActive: true),
                CreateTool(2, category, "Retired Platform Lift", hourlyRate: 18m, isActive: false)
            ]);

        var result = await service.SearchToolsAsync("platform", 1, 12);

        Assert.True(result.IsSuccess);
        Assert.Equal(["Platform Ladder"], result.Value!.Items.Select(item => item.Name).ToArray());
    }

    [Fact]
    public async Task SearchToolsAsync_WhenNoToolsMatch_ReturnsEmptyPagedResult()
    {
        var category = new Category { Id = 1, Name = "Access & Lifting" };
        var service = CreateService(
            categories: [category],
            tools: [CreateTool(1, category, "Platform Ladder", hourlyRate: 10m)]);

        var result = await service.SearchToolsAsync("excavator", 1, 12);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.Items);
        Assert.Equal(0, result.Value.TotalCount);
    }

    [Fact]
    public async Task SearchToolsAsync_WhenQueryIsBlank_ReturnsValidationFailure()
    {
        var service = CreateService();

        var result = await service.SearchToolsAsync("   ", 1, 12);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.FailureType);
        Assert.Equal("Search query is required.", result.Error);
    }

    [Fact]
    public async Task SearchToolsAsync_WhenPageSizeIsInvalid_ReturnsValidationFailure()
    {
        var service = CreateService();

        var result = await service.SearchToolsAsync("drill", 1, 0);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.FailureType);
        Assert.Equal("Page size must be greater than or equal to 1.", result.Error);
    }

    [Fact]
    public async Task SearchToolsAsync_WhenQueryIsTooLong_ReturnsValidationFailure()
    {
        var service = CreateService();

        var result = await service.SearchToolsAsync(new string('x', 201), 1, 12);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.FailureType);
        Assert.Equal("Search query must be 200 characters or fewer.", result.Error);
    }

    [Fact]
    public async Task SearchToolsAsync_WhenQueryContainsSpecialCharacters_ReturnsMatchingTool()
    {
        var category = new Category { Id = 1, Name = "Breaking & Drilling" };
        var service = CreateService(
            categories: [category],
            tools: [CreateTool(1, category, "SDS+ Drill", hourlyRate: 15m)]);

        var result = await service.SearchToolsAsync("sds+", 1, 12);

        Assert.True(result.IsSuccess);
        var item = Assert.Single(result.Value!.Items);
        Assert.Equal("SDS+ Drill", item.Name);
    }

    [Fact]
    public async Task CalculateRentalCostAsync_WhenHoursOnlyIsCheapest_ReturnsHourlyCost()
    {
        var category = new Category { Id = 1, Name = "Breaking & Drilling" };
        var tool = CreateTool(1, category, "SDS Max Drill", hourlyRate: 8m, dailyRate: 45m, weeklyRate: 250m);
        var service = CreateService(categories: [category], tools: [tool]);
        var start = new DateTime(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc);
        var end = start.AddHours(3);

        var result = await service.CalculateRentalCostAsync(1, new(start, end));

        Assert.True(result.IsSuccess);
        Assert.Equal("SDS Max Drill", result.Value!.ToolName);
        Assert.Equal(24m, result.Value.TotalCost);
        Assert.Equal("3 hours x 8.00/hour = 24.00", result.Value.Breakdown);
    }

    [Fact]
    public async Task CalculateRentalCostAsync_WhenDailyRateIsCheaper_ReturnsDailyCost()
    {
        var category = new Category { Id = 1, Name = "Cleaning & Maintenance" };
        var tool = CreateTool(1, category, "Petrol Pressure Washer", hourlyRate: 10m, dailyRate: 50m, weeklyRate: 300m);
        var service = CreateService(categories: [category], tools: [tool]);
        var start = new DateTime(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc);
        var end = start.AddHours(8);

        var result = await service.CalculateRentalCostAsync(1, new(start, end));

        Assert.True(result.IsSuccess);
        Assert.Equal(50m, result.Value!.TotalCost);
        Assert.Equal("1 day x 50.00/day = 50.00", result.Value.Breakdown);
    }

    [Fact]
    public async Task CalculateRentalCostAsync_WhenPeriodIsMixed_ReturnsDailyAndHourlyCost()
    {
        var category = new Category { Id = 1, Name = "Electrical & Heating" };
        var tool = CreateTool(1, category, "50L Dehumidifier", hourlyRate: 8m, dailyRate: 45m, weeklyRate: 250m);
        var service = CreateService(categories: [category], tools: [tool]);
        var start = new DateTime(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc);
        var end = start.AddDays(1).AddHours(3);

        var result = await service.CalculateRentalCostAsync(1, new(start, end));

        Assert.True(result.IsSuccess);
        Assert.Equal(69m, result.Value!.TotalCost);
        Assert.Equal("1 day x 45.00/day + 3 hours x 8.00/hour = 69.00", result.Value.Breakdown);
    }

    [Fact]
    public async Task CalculateRentalCostAsync_WhenWeeklyRateIsCheaperForThreeDays_ReturnsWeeklyCost()
    {
        var category = new Category { Id = 1, Name = "Access & Lifting" };
        var tool = CreateTool(1, category, "Tower Scaffold", hourlyRate: 10m, dailyRate: 45m, weeklyRate: 100m);
        var service = CreateService(categories: [category], tools: [tool]);
        var start = new DateTime(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc);
        var end = start.AddDays(3);

        var result = await service.CalculateRentalCostAsync(1, new(start, end));

        Assert.True(result.IsSuccess);
        Assert.Equal(100m, result.Value!.TotalCost);
        Assert.Equal("1 week x 100.00/week = 100.00", result.Value.Breakdown);
    }

    [Fact]
    public async Task CalculateRentalCostAsync_WhenHourlyAndDailyCostsTie_PrefersExactCoverage()
    {
        var category = new Category { Id = 1, Name = "Breaking & Drilling" };
        var tool = CreateTool(1, category, "SDS Max Drill", hourlyRate: 10m, dailyRate: 10m, weeklyRate: 500m);
        var service = CreateService(categories: [category], tools: [tool]);
        var start = new DateTime(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc);
        var end = start.AddHours(1);

        var result = await service.CalculateRentalCostAsync(1, new(start, end));

        Assert.True(result.IsSuccess);
        Assert.Equal(10m, result.Value!.TotalCost);
        Assert.Equal("1 hour x 10.00/hour = 10.00", result.Value.Breakdown);
    }

    [Fact]
    public async Task CalculateRentalCostAsync_WhenCostsTieForExactCoverage_PrefersFewerUnits()
    {
        var category = new Category { Id = 1, Name = "Access & Lifting" };
        var tool = CreateTool(1, category, "Platform Ladder", hourlyRate: 10m, dailyRate: 240m, weeklyRate: 2000m);
        var service = CreateService(categories: [category], tools: [tool]);
        var start = new DateTime(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc);
        var end = start.AddDays(1);

        var result = await service.CalculateRentalCostAsync(1, new(start, end));

        Assert.True(result.IsSuccess);
        Assert.Equal(240m, result.Value!.TotalCost);
        Assert.Equal("1 day x 240.00/day = 240.00", result.Value.Breakdown);
    }

    [Fact]
    public async Task CalculateRentalCostAsync_WhenEndDateTimeIsNotAfterStartDateTime_ReturnsValidationFailure()
    {
        var service = CreateService();
        var start = new DateTime(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc);

        var result = await service.CalculateRentalCostAsync(1, new(start, start));

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.FailureType);
        Assert.Equal("End date/time must be after the start date/time.", result.Error);
    }

    [Fact]
    public async Task CalculateRentalCostAsync_WhenToolDoesNotExist_ReturnsNotFound()
    {
        var service = CreateService();
        var start = new DateTime(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc);

        var result = await service.CalculateRentalCostAsync(404, new(start, start.AddHours(1)));

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.FailureType);
    }

    [Fact]
    public async Task CalculateRentalCostAsync_WhenToolIsInactive_ReturnsNotFound()
    {
        var category = new Category { Id = 1, Name = "Access & Lifting" };
        var tool = CreateTool(8, category, "Material Hoist", hourlyRate: 20m, isActive: false);
        var service = CreateService(categories: [category], tools: [tool]);
        var start = new DateTime(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc);

        var result = await service.CalculateRentalCostAsync(8, new(start, start.AddHours(1)));

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.FailureType);
    }

    [Fact]
    public async Task CreateToolAsync_WhenRequestIsValid_CreatesActiveToolWithInitialImage()
    {
        var category = new Category { Id = 3, Name = "Breaking & Drilling" };
        var toolRepository = new InMemoryToolRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = CreateService(
            categories: [category],
            toolRepository: toolRepository,
            unitOfWork: unitOfWork);

        var result = await service.CreateToolAsync(ValidCreateToolRequest(), ValidFirstImageUrl());

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value!.Id);
        Assert.Equal("Rotary Hammer", result.Value.Name);
        Assert.Equal("Breaking & Drilling", result.Value.CategoryName);
        Assert.True(result.Value.IsActive);
        Assert.Null(result.Value.OverallRating);
        Assert.Equal(0, result.Value.ReviewCount);
        var image = Assert.Single(result.Value.Images);
        Assert.Equal("/uploads/tools/rotary-hammer.jpg", image.ImageUrl);
        Assert.Equal(1, image.DisplayOrder);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);

        var savedTool = Assert.Single(toolRepository.Items);
        Assert.Equal("Rotary Hammer", savedTool.Name);
        Assert.True(savedTool.IsActive);
        Assert.Equal(savedTool.Id, savedTool.Images.Single().ToolId);
    }

    [Fact]
    public async Task CreateToolAsync_WhenInitialImageIsMissing_ReturnsValidationFailure()
    {
        var category = new Category { Id = 3, Name = "Breaking & Drilling" };
        var toolRepository = new InMemoryToolRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = CreateService(
            categories: [category],
            toolRepository: toolRepository,
            unitOfWork: unitOfWork);

        var result = await service.CreateToolAsync(ValidCreateToolRequest(), "  ");

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.FailureType);
        Assert.Equal("At least one image is required before a tool/service can be saved.", result.Error);
        Assert.Empty(toolRepository.Items);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task CreateToolAsync_WhenCategoryDoesNotExist_ReturnsNotFound()
    {
        var service = CreateService();

        var result = await service.CreateToolAsync(ValidCreateToolRequest() with { CategoryId = 404 }, ValidFirstImageUrl());

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.FailureType);
        Assert.Equal("Category with ID 404 not found.", result.Error);
    }

    [Theory]
    [InlineData("", "Name is required.")]
    [InlineData("   ", "Name is required.")]
    public async Task CreateToolAsync_WhenRequiredTextIsMissing_ReturnsValidationFailure(string name, string expectedError)
    {
        var category = new Category { Id = 3, Name = "Breaking & Drilling" };
        var service = CreateService(categories: [category]);

        var result = await service.CreateToolAsync(ValidCreateToolRequest() with { Name = name }, ValidFirstImageUrl());

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.FailureType);
        Assert.Equal(expectedError, result.Error);
    }

    [Theory]
    [InlineData(0, 45, 180, "Hourly rate must be greater than 0.")]
    [InlineData(8.5, 0, 180, "Daily rate must be greater than 0.")]
    [InlineData(8.5, 45, 0, "Weekly rate must be greater than 0.")]
    public async Task CreateToolAsync_WhenRatesAreNotPositive_ReturnsValidationFailure(
        decimal hourlyRate,
        decimal dailyRate,
        decimal weeklyRate,
        string expectedError)
    {
        var category = new Category { Id = 3, Name = "Breaking & Drilling" };
        var service = CreateService(categories: [category]);

        var result = await service.CreateToolAsync(ValidCreateToolRequest() with
        {
            HourlyRate = hourlyRate,
            DailyRate = dailyRate,
            WeeklyRate = weeklyRate
        },
        ValidFirstImageUrl());

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.FailureType);
        Assert.Equal(expectedError, result.Error);
    }

    [Fact]
    public async Task CreateToolAsync_WhenDepositRequiredWithoutAmount_ReturnsValidationFailure()
    {
        var category = new Category { Id = 3, Name = "Breaking & Drilling" };
        var service = CreateService(categories: [category]);

        var result = await service.CreateToolAsync(ValidCreateToolRequest() with { DepositRequired = true, DepositAmount = null }, ValidFirstImageUrl());

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.FailureType);
        Assert.Equal("Deposit amount must be greater than 0 when a deposit is required.", result.Error);
    }

    [Fact]
    public async Task CreateToolAsync_WhenDepositNotRequiredWithAmount_ReturnsValidationFailure()
    {
        var category = new Category { Id = 3, Name = "Breaking & Drilling" };
        var service = CreateService(categories: [category]);

        var result = await service.CreateToolAsync(ValidCreateToolRequest() with { DepositRequired = false, DepositAmount = 10m }, ValidFirstImageUrl());

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.FailureType);
        Assert.Equal("Deposit amount must be empty when a deposit is not required.", result.Error);
    }

    [Fact]
    public async Task UpdateToolAsync_WhenRequestIsValid_UpdatesToolAndReturnsRefreshedDetail()
    {
        var oldCategory = new Category { Id = 1, Name = "Access & Lifting" };
        var newCategory = new Category { Id = 2, Name = "Electrical & Heating" };
        var tool = CreateTool(7, oldCategory, "Old Name", hourlyRate: 7m);
        tool.Images =
        [
            new ToolImage { Id = 1, ToolId = 7, ImageUrl = "existing.jpg", DisplayOrder = 1 }
        ];
        var unitOfWork = new FakeUnitOfWork();
        var service = CreateService(
            categories: [oldCategory, newCategory],
            tools: [tool],
            unitOfWork: unitOfWork);
        var request = new UpdateToolRequest(
            newCategory.Id,
            "50L Dehumidifier",
            "Updated heating and drying equipment.",
            11m,
            40m,
            135m,
            "Keep upright during transport.",
            true,
            60m);

        var result = await service.UpdateToolAsync(tool.Id, request);

        Assert.True(result.IsSuccess);
        Assert.Equal("50L Dehumidifier", result.Value!.Name);
        Assert.Equal("Electrical & Heating", result.Value.CategoryName);
        Assert.Equal(11m, result.Value.HourlyRate);
        Assert.Equal(40m, result.Value.DailyRate);
        Assert.Equal(135m, result.Value.WeeklyRate);
        Assert.Equal("Keep upright during transport.", result.Value.SpecialNotes);
        Assert.True(result.Value.DepositRequired);
        Assert.Equal(60m, result.Value.DepositAmount);
        Assert.Equal(["existing.jpg"], result.Value.Images.Select(image => image.ImageUrl).ToArray());
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task UpdateToolAsync_WhenToolDoesNotExist_ReturnsNotFound()
    {
        var category = new Category { Id = 2, Name = "Electrical & Heating" };
        var service = CreateService(categories: [category]);

        var result = await service.UpdateToolAsync(404, ValidUpdateToolRequest(category.Id));

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.FailureType);
        Assert.Equal("Tool with ID 404 not found.", result.Error);
    }

    [Fact]
    public async Task UpdateToolAsync_WhenCategoryDoesNotExist_ReturnsNotFound()
    {
        var category = new Category { Id = 1, Name = "Access & Lifting" };
        var tool = CreateTool(7, category, "Tower Scaffold", hourlyRate: 20m);
        var service = CreateService(categories: [category], tools: [tool]);

        var result = await service.UpdateToolAsync(tool.Id, ValidUpdateToolRequest(categoryId: 404));

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.FailureType);
        Assert.Equal("Category with ID 404 not found.", result.Error);
    }

    [Fact]
    public async Task UpdateToolAsync_WhenRequestIsInvalid_ReturnsValidationFailure()
    {
        var category = new Category { Id = 1, Name = "Access & Lifting" };
        var tool = CreateTool(7, category, "Tower Scaffold", hourlyRate: 20m);
        var service = CreateService(categories: [category], tools: [tool]);

        var result = await service.UpdateToolAsync(tool.Id, ValidUpdateToolRequest(category.Id) with { DailyRate = -1m });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.FailureType);
        Assert.Equal("Daily rate must be greater than 0.", result.Error);
    }

    [Fact]
    public async Task SetToolStatusAsync_WhenToolExists_UpdatesActiveFlagWithoutDeletingTool()
    {
        var category = new Category { Id = 1, Name = "Access & Lifting" };
        var tool = CreateTool(7, category, "Tower Scaffold", hourlyRate: 20m);
        var toolRepository = new InMemoryToolRepository([tool]);
        var unitOfWork = new FakeUnitOfWork();
        var service = CreateService(
            categories: [category],
            toolRepository: toolRepository,
            unitOfWork: unitOfWork);

        var result = await service.SetToolStatusAsync(tool.Id, false);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
        Assert.False(tool.IsActive);
        Assert.Single(toolRepository.Items);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task SetToolStatusAsync_WhenToolDoesNotExist_ReturnsNotFound()
    {
        var service = CreateService();

        var result = await service.SetToolStatusAsync(404, false);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.FailureType);
        Assert.Equal("Tool with ID 404 not found.", result.Error);
    }

    [Fact]
    public async Task PublicQueries_WhenToolIsInactive_ExcludeItFromBrowseSearchAndDetail()
    {
        var category = new Category { Id = 1, Name = "Access & Lifting" };
        var tool = CreateTool(7, category, "Tower Scaffold", hourlyRate: 20m);
        var service = CreateService(categories: [category], tools: [tool]);

        await service.SetToolStatusAsync(tool.Id, false);
        var browseResult = await service.GetToolsByCategoryAsync(category.Id, 1, 12);
        var searchResult = await service.SearchToolsAsync("tower", 1, 12);
        var detailResult = await service.GetToolByIdAsync(tool.Id);

        Assert.True(browseResult.IsSuccess);
        Assert.Empty(browseResult.Value!.Items);
        Assert.True(searchResult.IsSuccess);
        Assert.Empty(searchResult.Value!.Items);
        Assert.False(detailResult.IsSuccess);
        Assert.Equal(ErrorType.NotFound, detailResult.FailureType);
    }

    [Fact]
    public async Task GetAdminToolsAsync_DefaultQuery_ReturnsActiveAndInactiveTools()
    {
        var category = new Category { Id = 1, Name = "Access & Lifting" };
        var activeTool = CreateTool(7, category, "Tower Scaffold", hourlyRate: 20m, isActive: true);
        activeTool.Images =
        [
            new ToolImage { Id = 1, ToolId = 7, ImageUrl = "tower.jpg", DisplayOrder = 1 }
        ];
        activeTool.Reviews =
        [
            CreateApprovedReview(1, activeTool, 5, 4, 5, 4, 5),
            CreateApprovedReview(2, activeTool, 4, 4, 5, 4, 4)
        ];
        var inactiveTool = CreateTool(8, category, "Retired Ladder", hourlyRate: 5m, isActive: false);
        inactiveTool.UpdatedDate = new DateTime(2026, 4, 10, 12, 0, 0, DateTimeKind.Utc);
        var service = CreateService(categories: [category], tools: [activeTool, inactiveTool]);

        var result = await service.GetAdminToolsAsync(new AdminToolQueryRequest());

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.TotalCount);
        Assert.Equal(["Retired Ladder", "Tower Scaffold"], result.Value.Items.Select(item => item.Name).ToArray());
        Assert.Contains(result.Value.Items, item => item.Name == "Retired Ladder" && !item.IsActive);
        var tower = result.Value.Items.Single(item => item.Name == "Tower Scaffold");
        Assert.Equal("Access & Lifting", tower.CategoryName);
        Assert.Equal("tower.jpg", tower.ThumbnailUrl);
        Assert.True(tower.HasEnoughReviewsToRate);
        Assert.Equal(2, tower.ReviewCount);
    }

    [Fact]
    public async Task GetAdminToolsAsync_WhenFiltersProvided_ReturnsMatchingInactiveTools()
    {
        var access = new Category { Id = 1, Name = "Access & Lifting" };
        var breaking = new Category { Id = 2, Name = "Breaking & Drilling" };
        var service = CreateService(
            categories: [access, breaking],
            tools:
            [
                CreateTool(1, access, "Retired Ladder", hourlyRate: 5m, isActive: false),
                CreateTool(2, access, "Tower Scaffold", hourlyRate: 20m, isActive: true),
                CreateTool(3, breaking, "Retired Breaker", hourlyRate: 22m, isActive: false)
            ]);

        var result = await service.GetAdminToolsAsync(new AdminToolQueryRequest
        {
            SearchTerm = "retired",
            CategoryId = access.Id,
            Status = "inactive"
        });

        Assert.True(result.IsSuccess);
        var item = Assert.Single(result.Value!.Items);
        Assert.Equal("Retired Ladder", item.Name);
        Assert.False(item.IsActive);
        Assert.Equal(access.Id, item.CategoryId);
    }

    [Fact]
    public async Task GetAdminToolsAsync_WhenSortedByUpdatedDescending_ReturnsNewestFirst()
    {
        var category = new Category { Id = 1, Name = "Access & Lifting" };
        var olderTool = CreateTool(1, category, "Older Tool", hourlyRate: 5m);
        olderTool.UpdatedDate = new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc);
        var newerTool = CreateTool(2, category, "Newer Tool", hourlyRate: 5m);
        newerTool.UpdatedDate = new DateTime(2026, 4, 2, 12, 0, 0, DateTimeKind.Utc);
        var service = CreateService(categories: [category], tools: [olderTool, newerTool]);

        var result = await service.GetAdminToolsAsync(new AdminToolQueryRequest { SortBy = "updated_desc" });

        Assert.True(result.IsSuccess);
        Assert.Equal(["Newer Tool", "Older Tool"], result.Value!.Items.Select(item => item.Name).ToArray());
    }

    [Fact]
    public async Task GetAdminToolsAsync_WhenStatusIsInvalid_ReturnsValidationFailure()
    {
        var service = CreateService();

        var result = await service.GetAdminToolsAsync(new AdminToolQueryRequest { Status = "archived" });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.FailureType);
        Assert.Equal("Invalid status value. Supported values are all, active, and inactive.", result.Error);
    }

    [Fact]
    public async Task GetAdminToolByIdAsync_WhenInactiveToolExists_ReturnsFullDetail()
    {
        var category = new Category { Id = 1, Name = "Access & Lifting" };
        var tool = CreateTool(7, category, "Retired Ladder", hourlyRate: 5m, isActive: false);
        tool.Images =
        [
            new ToolImage { Id = 1, ToolId = 7, ImageUrl = "retired-ladder.jpg", DisplayOrder = 1 }
        ];
        var service = CreateService(categories: [category], tools: [tool]);

        var result = await service.GetAdminToolByIdAsync(tool.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal("Retired Ladder", result.Value!.Name);
        Assert.False(result.Value.IsActive);
        Assert.Equal("retired-ladder.jpg", Assert.Single(result.Value.Images).ImageUrl);
    }

    [Fact]
    public async Task GetAdminToolByIdAsync_WhenToolDoesNotExist_ReturnsNotFound()
    {
        var service = CreateService();

        var result = await service.GetAdminToolByIdAsync(404);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.FailureType);
        Assert.Equal("Tool with ID 404 not found.", result.Error);
    }

    private static ToolService CreateService(
        IEnumerable<Category>? categories = null,
        IEnumerable<Tool>? tools = null,
        InMemoryToolRepository? toolRepository = null,
        FakeUnitOfWork? unitOfWork = null)
    {
        return new ToolService(
            toolRepository ?? new InMemoryToolRepository(tools),
            new InMemoryCategoryRepository(categories),
            unitOfWork ?? new FakeUnitOfWork());
    }

    private static CreateToolRequest ValidCreateToolRequest()
    {
        return new CreateToolRequest(
            CategoryId: 3,
            Name: "Rotary Hammer",
            Description: "Heavy-duty hammer drill for masonry and concrete work.",
            HourlyRate: 8.50m,
            DailyRate: 45.00m,
            WeeklyRate: 180.00m,
            SpecialNotes: "Includes SDS bits.",
            DepositRequired: true,
            DepositAmount: 50.00m);
    }

    private static string ValidFirstImageUrl() => "/uploads/tools/rotary-hammer.jpg";

    private static UpdateToolRequest ValidUpdateToolRequest(int categoryId)
    {
        return new UpdateToolRequest(
            CategoryId: categoryId,
            Name: "Rotary Hammer Pro",
            Description: "Updated heavy-duty hammer drill listing.",
            HourlyRate: 9.00m,
            DailyRate: 48.00m,
            WeeklyRate: 190.00m,
            SpecialNotes: "Includes SDS bits and carry case.",
            DepositRequired: true,
            DepositAmount: 55.00m);
    }

    private static Tool CreateTool(
        int id,
        Category category,
        string name,
        decimal hourlyRate,
        decimal? dailyRate = null,
        decimal? weeklyRate = null,
        bool isActive = true)
    {
        return new Tool
        {
            Id = id,
            CategoryId = category.Id,
            Category = category,
            Name = name,
            Description = $"{name} description",
            HourlyRate = hourlyRate,
            DailyRate = dailyRate ?? (hourlyRate * 3),
            WeeklyRate = weeklyRate ?? (hourlyRate * 10),
            IsActive = isActive,
            Images = []
        };
    }

    private static Review CreateApprovedReview(
        int id,
        Tool tool,
        int equipmentRating,
        int customerServiceRating,
        int technicalSupportRating,
        int afterSalesRating,
        int valueForMoneyRating)
    {
        return CreateReview(
            id,
            tool,
            ReviewStatus.Approved,
            equipmentRating,
            customerServiceRating,
            technicalSupportRating,
            afterSalesRating,
            valueForMoneyRating);
    }

    private static Review CreateReview(
        int id,
        Tool tool,
        ReviewStatus status,
        int equipmentRating,
        int customerServiceRating,
        int technicalSupportRating,
        int afterSalesRating,
        int valueForMoneyRating)
    {
        var review = new Review
        {
            Id = id,
            ToolId = tool.Id,
            Tool = tool,
            ReviewerName = $"Reviewer {id}",
            ReviewerEmail = $"reviewer{id}@example.com",
            ReviewText = $"Review {id} contains enough detail to satisfy the minimum length rule.",
            EquipmentRating = equipmentRating,
            CustomerServiceRating = customerServiceRating,
            TechnicalSupportRating = technicalSupportRating,
            AfterSalesRating = afterSalesRating,
            ValueForMoneyRating = valueForMoneyRating,
            Status = status
        };

        review.CalculateOverallRating();
        return review;
    }
}
