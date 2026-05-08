using FluentValidation;
using ReviewPortal.Application.DTOs.Categories;
using ReviewPortal.Application.DTOs.Reviews;
using ReviewPortal.Application.DTOs.Tools;
using ReviewPortal.Application.DTOs.Users;
using ReviewPortal.Application.Validators.Categories;
using ReviewPortal.Application.Validators.Reviews;
using ReviewPortal.Application.Validators.Tools;
using ReviewPortal.Application.Validators.Users;

namespace ReviewPortal.UnitTests.Validators;

public class AuthRequestValidatorTests
{
    [Theory]
    [MemberData(nameof(InvalidRegisterRequests))]
    public void RegisterRequestValidator_InvalidRequest_ReturnsExpectedError(RegisterRequest request, string propertyName, string errorMessage)
    {
        AssertValidationError(new RegisterRequestValidator(), request, propertyName, errorMessage);
    }

    [Theory]
    [MemberData(nameof(InvalidLoginRequests))]
    public void LoginRequestValidator_InvalidRequest_ReturnsExpectedError(LoginRequest request, string propertyName, string errorMessage)
    {
        AssertValidationError(new LoginRequestValidator(), request, propertyName, errorMessage);
    }

    [Theory]
    [MemberData(nameof(InvalidChangePasswordRequests))]
    public void ChangePasswordRequestValidator_InvalidRequest_ReturnsExpectedError(ChangePasswordRequest request, string propertyName, string errorMessage)
    {
        AssertValidationError(new ChangePasswordRequestValidator(), request, propertyName, errorMessage);
    }

    [Theory]
    [MemberData(nameof(InvalidForgotPasswordRequests))]
    public void ForgotPasswordRequestValidator_InvalidRequest_ReturnsExpectedError(ForgotPasswordRequest request, string propertyName, string errorMessage)
    {
        AssertValidationError(new ForgotPasswordRequestValidator(), request, propertyName, errorMessage);
    }

    [Theory]
    [MemberData(nameof(InvalidResetPasswordRequests))]
    public void ResetPasswordRequestValidator_InvalidRequest_ReturnsExpectedError(ResetPasswordRequest request, string propertyName, string errorMessage)
    {
        AssertValidationError(new ResetPasswordRequestValidator(), request, propertyName, errorMessage);
    }

    public static IEnumerable<object[]> InvalidRegisterRequests()
    {
        yield return [new RegisterRequest("", "jane@example.com", "Secure123"), "Name", "Name is required."];
        yield return [new RegisterRequest(Text(101), "jane@example.com", "Secure123"), "Name", "Name must be 100 characters or fewer."];
        yield return [new RegisterRequest("Jane", "", "Secure123"), "Email", "Email is required."];
        yield return [new RegisterRequest("Jane", Text(257), "Secure123"), "Email", "Email must be 256 characters or fewer."];
        yield return [new RegisterRequest("Jane", "not-an-email", "Secure123"), "Email", "A valid email address is required."];
        yield return [new RegisterRequest("Jane", "jane@example.com", ""), "Password", "Password is required."];
        yield return [new RegisterRequest("Jane", "jane@example.com", "Short1"), "Password", "Password must be at least 8 characters long."];
        yield return [new RegisterRequest("Jane", "jane@example.com", "secure123"), "Password", "Password must contain at least one uppercase letter."];
        yield return [new RegisterRequest("Jane", "jane@example.com", "SecurePass"), "Password", "Password must contain at least one number."];
    }

    public static IEnumerable<object[]> InvalidLoginRequests()
    {
        yield return [new LoginRequest("", "Secure123"), "Email", "Email is required."];
        yield return [new LoginRequest("jane@example.com", ""), "Password", "Password is required."];
    }

    public static IEnumerable<object[]> InvalidChangePasswordRequests()
    {
        yield return [new ChangePasswordRequest("", "NewSecure123"), "CurrentPassword", "Current password is required."];
        yield return [new ChangePasswordRequest("Secure123", ""), "NewPassword", "New password is required."];
        yield return [new ChangePasswordRequest("Secure123", "Short1"), "NewPassword", "New password must be at least 8 characters long."];
        yield return [new ChangePasswordRequest("Secure123", "secure123"), "NewPassword", "New password must contain at least one uppercase letter."];
        yield return [new ChangePasswordRequest("Secure123", "SecurePass"), "NewPassword", "New password must contain at least one number."];
        yield return [new ChangePasswordRequest("Secure123", "Secure123"), "NewPassword", "New password must be different from the current password."];
    }

    public static IEnumerable<object[]> InvalidForgotPasswordRequests()
    {
        yield return [new ForgotPasswordRequest(""), "Email", "Email is required."];
        yield return [new ForgotPasswordRequest(Text(257)), "Email", "Email must be 256 characters or fewer."];
        yield return [new ForgotPasswordRequest("not-an-email"), "Email", "A valid email address is required."];
    }

    public static IEnumerable<object[]> InvalidResetPasswordRequests()
    {
        yield return [new ResetPasswordRequest("jane@example.com", "", "Secure123"), "ResetToken", "Reset token is required."];
        yield return [new ResetPasswordRequest("", "token", "Secure123"), "Email", "Email is required."];
        yield return [new ResetPasswordRequest(Text(257), "token", "Secure123"), "Email", "Email must be 256 characters or fewer."];
        yield return [new ResetPasswordRequest("not-an-email", "token", "Secure123"), "Email", "A valid email address is required."];
        yield return [new ResetPasswordRequest("jane@example.com", "token", ""), "NewPassword", "New password is required."];
        yield return [new ResetPasswordRequest("jane@example.com", "token", "Short1"), "NewPassword", "New password must be at least 8 characters long."];
        yield return [new ResetPasswordRequest("jane@example.com", "token", "secure123"), "NewPassword", "New password must contain at least one uppercase letter."];
        yield return [new ResetPasswordRequest("jane@example.com", "token", "SecurePass"), "NewPassword", "New password must contain at least one number."];
    }

    private static string Text(int length) => new('x', length);

    private static void AssertValidationError<T>(AbstractValidator<T> validator, T request, string propertyName, string errorMessage)
    {
        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == propertyName && error.ErrorMessage == errorMessage);
    }
}

public class CategoryRequestValidatorTests
{
    [Theory]
    [MemberData(nameof(InvalidCreateCategoryRequests))]
    public void CreateCategoryRequestValidator_InvalidRequest_ReturnsExpectedError(CreateCategoryRequest request, string propertyName, string errorMessage)
    {
        AssertValidationError(new CreateCategoryRequestValidator(), request, propertyName, errorMessage);
    }

    [Theory]
    [MemberData(nameof(InvalidUpdateCategoryRequests))]
    public void UpdateCategoryRequestValidator_InvalidRequest_ReturnsExpectedError(UpdateCategoryRequest request, string propertyName, string errorMessage)
    {
        AssertValidationError(new UpdateCategoryRequestValidator(), request, propertyName, errorMessage);
    }

    public static IEnumerable<object[]> InvalidCreateCategoryRequests()
    {
        yield return [new CreateCategoryRequest("", null, null), "Name", "Category name is required."];
        yield return [new CreateCategoryRequest(Text(101), null, null), "Name", "Category name must be 100 characters or fewer."];
        yield return [new CreateCategoryRequest("Access", Text(501), null), "Description", "Category description must be 500 characters or fewer."];
        yield return [new CreateCategoryRequest("Access", null, Text(501)), "ImageUrl", "Category image URL must be 500 characters or fewer."];
    }

    public static IEnumerable<object[]> InvalidUpdateCategoryRequests()
    {
        yield return [new UpdateCategoryRequest("", null, null), "Name", "Category name is required."];
        yield return [new UpdateCategoryRequest(Text(101), null, null), "Name", "Category name must be 100 characters or fewer."];
        yield return [new UpdateCategoryRequest("Access", Text(501), null), "Description", "Category description must be 500 characters or fewer."];
        yield return [new UpdateCategoryRequest("Access", null, Text(501)), "ImageUrl", "Category image URL must be 500 characters or fewer."];
    }

    private static string Text(int length) => new('x', length);

    private static void AssertValidationError<T>(AbstractValidator<T> validator, T request, string propertyName, string errorMessage)
    {
        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == propertyName && error.ErrorMessage == errorMessage);
    }
}

public class ToolRequestValidatorTests
{
    [Theory]
    [MemberData(nameof(InvalidRentalCalculationRequests))]
    public void RentalCalculationRequestValidator_InvalidRequest_ReturnsExpectedError(RentalCalculationRequest request, string propertyName, string errorMessage)
    {
        AssertValidationError(new RentalCalculationRequestValidator(), request, propertyName, errorMessage);
    }

    [Theory]
    [MemberData(nameof(InvalidCreateToolRequests))]
    public void CreateToolRequestValidator_InvalidRequest_ReturnsExpectedError(CreateToolRequest request, string propertyName, string errorMessage)
    {
        AssertValidationError(new CreateToolRequestValidator(), request, propertyName, errorMessage);
    }

    [Theory]
    [MemberData(nameof(InvalidUpdateToolRequests))]
    public void UpdateToolRequestValidator_InvalidRequest_ReturnsExpectedError(UpdateToolRequest request, string propertyName, string errorMessage)
    {
        AssertValidationError(new UpdateToolRequestValidator(), request, propertyName, errorMessage);
    }

    public static IEnumerable<object[]> InvalidRentalCalculationRequests()
    {
        var start = new DateTime(2026, 5, 5, 10, 0, 0, DateTimeKind.Utc);
        yield return [new RentalCalculationRequest(start, start), "EndDateTime", "End date/time must be after the start date/time."];
        yield return [new RentalCalculationRequest(start, start.AddMinutes(-1)), "EndDateTime", "End date/time must be after the start date/time."];
    }

    public static IEnumerable<object[]> InvalidCreateToolRequests()
    {
        var valid = ValidCreateToolRequest();

        yield return [valid with { CategoryId = 0 }, "CategoryId", "Category ID must be greater than 0."];
        yield return [valid with { Name = "" }, "Name", "Name is required."];
        yield return [valid with { Name = Text(201) }, "Name", "Name must be 200 characters or fewer."];
        yield return [valid with { Description = "" }, "Description", "Description is required."];
        yield return [valid with { Description = Text(2001) }, "Description", "Description must be 2000 characters or fewer."];
        yield return [valid with { HourlyRate = 0m }, "HourlyRate", "Hourly rate must be greater than 0."];
        yield return [valid with { DailyRate = 0m }, "DailyRate", "Daily rate must be greater than 0."];
        yield return [valid with { WeeklyRate = 0m }, "WeeklyRate", "Weekly rate must be greater than 0."];
        yield return [valid with { SpecialNotes = Text(1001) }, "SpecialNotes", "Special notes must be 1000 characters or fewer."];
        yield return [valid with { DepositRequired = true, DepositAmount = null }, "DepositAmount", "Deposit amount must be greater than 0 when a deposit is required."];
        yield return [valid with { DepositRequired = false, DepositAmount = 25m }, "DepositAmount", "Deposit amount must be empty when a deposit is not required."];
    }

    public static IEnumerable<object[]> InvalidUpdateToolRequests()
    {
        var valid = ValidUpdateToolRequest();

        yield return [valid with { CategoryId = 0 }, "CategoryId", "Category ID must be greater than 0."];
        yield return [valid with { Name = "" }, "Name", "Name is required."];
        yield return [valid with { Name = Text(201) }, "Name", "Name must be 200 characters or fewer."];
        yield return [valid with { Description = "" }, "Description", "Description is required."];
        yield return [valid with { Description = Text(2001) }, "Description", "Description must be 2000 characters or fewer."];
        yield return [valid with { HourlyRate = 0m }, "HourlyRate", "Hourly rate must be greater than 0."];
        yield return [valid with { DailyRate = 0m }, "DailyRate", "Daily rate must be greater than 0."];
        yield return [valid with { WeeklyRate = 0m }, "WeeklyRate", "Weekly rate must be greater than 0."];
        yield return [valid with { SpecialNotes = Text(1001) }, "SpecialNotes", "Special notes must be 1000 characters or fewer."];
        yield return [valid with { DepositRequired = true, DepositAmount = null }, "DepositAmount", "Deposit amount must be greater than 0 when a deposit is required."];
        yield return [valid with { DepositRequired = false, DepositAmount = 25m }, "DepositAmount", "Deposit amount must be empty when a deposit is not required."];
    }

    private static CreateToolRequest ValidCreateToolRequest()
    {
        return new CreateToolRequest(1, "Tower Scaffold", "Safe access tower.", 10m, 50m, 200m, null, false, null);
    }

    private static UpdateToolRequest ValidUpdateToolRequest()
    {
        return new UpdateToolRequest(1, "Tower Scaffold", "Safe access tower.", 10m, 50m, 200m, null, false, null);
    }

    private static string Text(int length) => new('x', length);

    private static void AssertValidationError<T>(AbstractValidator<T> validator, T request, string propertyName, string errorMessage)
    {
        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == propertyName && error.ErrorMessage == errorMessage);
    }
}

public class ReviewRequestValidatorTests
{
    [Theory]
    [MemberData(nameof(InvalidCreateReviewRequests))]
    public void CreateReviewRequestValidator_InvalidRequest_ReturnsExpectedError(CreateReviewRequest request, string propertyName, string errorMessage)
    {
        AssertValidationError(new CreateReviewRequestValidator(), request, propertyName, errorMessage);
    }

    [Theory]
    [MemberData(nameof(InvalidCreateCommentRequests))]
    public void CreateCommentRequestValidator_InvalidRequest_ReturnsExpectedError(CreateCommentRequest request, string propertyName, string errorMessage)
    {
        AssertValidationError(new CreateCommentRequestValidator(), request, propertyName, errorMessage);
    }

    [Theory]
    [MemberData(nameof(InvalidCreateCompanyResponseRequests))]
    public void CreateCompanyResponseRequestValidator_InvalidRequest_ReturnsExpectedError(CreateCompanyResponseRequest request, string propertyName, string errorMessage)
    {
        AssertValidationError(new CreateCompanyResponseRequestValidator(), request, propertyName, errorMessage);
    }

    [Theory]
    [MemberData(nameof(InvalidModerationRequests))]
    public void ModerateReviewRequestValidator_InvalidRequest_ReturnsExpectedError(ModerateReviewRequest request, string propertyName, string errorMessage)
    {
        AssertValidationError(new ModerateReviewRequestValidator(), request, propertyName, errorMessage);
    }

    [Fact]
    public void ModerateReviewRequestValidator_WhenApprovedWithoutReason_ReturnsValidResult()
    {
        var result = new ModerateReviewRequestValidator().Validate(new ModerateReviewRequest(true, null));

        Assert.True(result.IsValid);
    }

    public static IEnumerable<object[]> InvalidCreateReviewRequests()
    {
        var valid = ValidCreateReviewRequest();

        yield return [valid with { ReviewerName = Text(101) }, "ReviewerName", "Reviewer name must be 100 characters or fewer."];
        yield return [valid with { ReviewerEmail = Text(257) }, "ReviewerEmail", "Reviewer email must be 256 characters or fewer."];
        yield return [valid with { ReviewerEmail = "not-an-email" }, "ReviewerEmail", "Reviewer email must be a valid email address."];
        yield return [valid with { ReviewText = "" }, "ReviewText", "Review text is required."];
        yield return [valid with { ReviewText = "Too short" }, "ReviewText", "Review text must be at least 20 characters."];
        yield return [valid with { ReviewText = Text(2001) }, "ReviewText", "Review text must be 2000 characters or fewer."];
        yield return [valid with { EquipmentRating = 0 }, "EquipmentRating", "Equipment rating must be between 1 and 5."];
        yield return [valid with { CustomerServiceRating = 6 }, "CustomerServiceRating", "Customer service rating must be between 1 and 5."];
        yield return [valid with { TechnicalSupportRating = 0 }, "TechnicalSupportRating", "Technical support rating must be between 1 and 5."];
        yield return [valid with { AfterSalesRating = 6 }, "AfterSalesRating", "After-sales rating must be between 1 and 5."];
        yield return [valid with { ValueForMoneyRating = 0 }, "ValueForMoneyRating", "Value for money rating must be between 1 and 5."];
    }

    public static IEnumerable<object[]> InvalidCreateCommentRequests()
    {
        yield return [new CreateCommentRequest(Text(101), "This is a useful comment."), "CommenterName", "Commenter name must be 100 characters or fewer."];
        yield return [new CreateCommentRequest("Jane", ""), "CommentText", "Comment text is required."];
        yield return [new CreateCommentRequest("Jane", "Too short"), "CommentText", "Comment text must be at least 10 characters."];
        yield return [new CreateCommentRequest("Jane", Text(1001)), "CommentText", "Comment text must be 1000 characters or fewer."];
    }

    public static IEnumerable<object[]> InvalidCreateCompanyResponseRequests()
    {
        yield return [new CreateCompanyResponseRequest(""), "ResponseText", "Response text is required."];
        yield return [new CreateCompanyResponseRequest(Text(2001)), "ResponseText", "Response text must be 2000 characters or fewer."];
    }

    public static IEnumerable<object[]> InvalidModerationRequests()
    {
        yield return [new ModerateReviewRequest(false, ""), "RejectionReason", "Rejection reason is required when rejecting."];
        yield return [new ModerateReviewRequest(false, Text(501)), "RejectionReason", "Rejection reason must be 500 characters or fewer."];
    }

    private static CreateReviewRequest ValidCreateReviewRequest()
    {
        return new CreateReviewRequest(
            "Jane",
            "jane@example.com",
            "This tool/service was reliable and good value.",
            5,
            4,
            5,
            4,
            5);
    }

    private static string Text(int length) => new('x', length);

    private static void AssertValidationError<T>(AbstractValidator<T> validator, T request, string propertyName, string errorMessage)
    {
        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == propertyName && error.ErrorMessage == errorMessage);
    }
}
