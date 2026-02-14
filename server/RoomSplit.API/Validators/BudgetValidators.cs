using FluentValidation;
using RoomSplit.API.DTOs.Budget;

namespace RoomSplit.API.Validators;

public class CreateBudgetDtoValidator : AbstractValidator<CreateBudgetDto>
{
    public CreateBudgetDtoValidator()
    {
        RuleFor(x => x.ExpenseCategory).InclusiveBetween(0, 11);
        RuleFor(x => x.MonthlyLimit).GreaterThan(0);
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
        RuleFor(x => x.Year).InclusiveBetween(2020, 2100);
    }
}

public class UpdateBudgetDtoValidator : AbstractValidator<UpdateBudgetDto>
{
    public UpdateBudgetDtoValidator()
    {
        RuleFor(x => x.ExpenseCategory).InclusiveBetween(0, 11);
        RuleFor(x => x.MonthlyLimit).GreaterThan(0);
    }
}
