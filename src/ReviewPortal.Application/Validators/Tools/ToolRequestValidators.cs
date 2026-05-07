using FluentValidation;
using ReviewPortal.Application.DTOs.Tools;

namespace ReviewPortal.Application.Validators.Tools;

public class RentalCalculationRequestValidator : AbstractValidator<RentalCalculationRequest>
{
    public RentalCalculationRequestValidator()
    {
        RuleFor(request => request.EndDateTime)
            .GreaterThan(request => request.StartDateTime)
            .WithMessage("End date/time must be after the start date/time.");
    }
}

public class CreateToolRequestValidator : AbstractValidator<CreateToolRequest>
{
    public CreateToolRequestValidator()
    {
        this.ApplyCommonToolRules();
    }
}

public class UpdateToolRequestValidator : AbstractValidator<UpdateToolRequest>
{
    public UpdateToolRequestValidator()
    {
        this.ApplyCommonToolRules();
    }
}

internal static class ToolRequestValidatorExtensions
{
    public static void ApplyCommonToolRules<T>(this AbstractValidator<T> validator)
        where T : class
    {
        validator.RuleFor(request => GetCategoryId(request))
            .GreaterThan(0)
            .OverridePropertyName("CategoryId")
            .WithMessage("Category ID must be greater than 0.");

        validator.RuleFor(request => GetName(request))
            .Cascade(CascadeMode.Stop)
            .Must(ValidationHelpers.HasText)
            .OverridePropertyName("Name")
            .WithMessage("Name is required.")
            .Must(value => ValidationHelpers.HasTrimmedLengthAtMost(value, 200))
            .OverridePropertyName("Name")
            .WithMessage("Name must be 200 characters or fewer.");

        validator.RuleFor(request => GetDescription(request))
            .Cascade(CascadeMode.Stop)
            .Must(ValidationHelpers.HasText)
            .OverridePropertyName("Description")
            .WithMessage("Description is required.")
            .Must(value => ValidationHelpers.HasTrimmedLengthAtMost(value, 2000))
            .OverridePropertyName("Description")
            .WithMessage("Description must be 2000 characters or fewer.");

        validator.RuleFor(request => GetHourlyRate(request))
            .GreaterThan(0m)
            .OverridePropertyName("HourlyRate")
            .WithMessage("Hourly rate must be greater than 0.");

        validator.RuleFor(request => GetDailyRate(request))
            .GreaterThan(0m)
            .OverridePropertyName("DailyRate")
            .WithMessage("Daily rate must be greater than 0.");

        validator.RuleFor(request => GetWeeklyRate(request))
            .GreaterThan(0m)
            .OverridePropertyName("WeeklyRate")
            .WithMessage("Weekly rate must be greater than 0.");

        validator.RuleFor(request => GetSpecialNotes(request))
            .Must(value => ValidationHelpers.OptionalHasTrimmedLengthAtMost(value, 1000))
            .OverridePropertyName("SpecialNotes")
            .WithMessage("Special notes must be 1000 characters or fewer.");

        validator.RuleFor(request => GetDepositAmount(request))
            .Must((request, depositAmount) => !GetDepositRequired(request) || (depositAmount.HasValue && depositAmount.Value > 0m))
            .OverridePropertyName("DepositAmount")
            .WithMessage("Deposit amount must be greater than 0 when a deposit is required.");

        validator.RuleFor(request => GetDepositAmount(request))
            .Must((request, depositAmount) => GetDepositRequired(request) || !depositAmount.HasValue || depositAmount.Value == 0m)
            .OverridePropertyName("DepositAmount")
            .WithMessage("Deposit amount must be empty when a deposit is not required.");
    }

    private static int GetCategoryId<T>(T request)
    {
        return request switch
        {
            CreateToolRequest create => create.CategoryId,
            UpdateToolRequest update => update.CategoryId,
            _ => 0
        };
    }

    private static string GetName<T>(T request)
    {
        return request switch
        {
            CreateToolRequest create => create.Name,
            UpdateToolRequest update => update.Name,
            _ => string.Empty
        };
    }

    private static string GetDescription<T>(T request)
    {
        return request switch
        {
            CreateToolRequest create => create.Description,
            UpdateToolRequest update => update.Description,
            _ => string.Empty
        };
    }

    private static decimal GetHourlyRate<T>(T request)
    {
        return request switch
        {
            CreateToolRequest create => create.HourlyRate,
            UpdateToolRequest update => update.HourlyRate,
            _ => 0m
        };
    }

    private static decimal GetDailyRate<T>(T request)
    {
        return request switch
        {
            CreateToolRequest create => create.DailyRate,
            UpdateToolRequest update => update.DailyRate,
            _ => 0m
        };
    }

    private static decimal GetWeeklyRate<T>(T request)
    {
        return request switch
        {
            CreateToolRequest create => create.WeeklyRate,
            UpdateToolRequest update => update.WeeklyRate,
            _ => 0m
        };
    }

    private static string? GetSpecialNotes<T>(T request)
    {
        return request switch
        {
            CreateToolRequest create => create.SpecialNotes,
            UpdateToolRequest update => update.SpecialNotes,
            _ => null
        };
    }

    private static bool GetDepositRequired<T>(T request)
    {
        return request switch
        {
            CreateToolRequest create => create.DepositRequired,
            UpdateToolRequest update => update.DepositRequired,
            _ => false
        };
    }

    private static decimal? GetDepositAmount<T>(T request)
    {
        return request switch
        {
            CreateToolRequest create => create.DepositAmount,
            UpdateToolRequest update => update.DepositAmount,
            _ => null
        };
    }
}
