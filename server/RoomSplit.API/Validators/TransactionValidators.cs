using FluentValidation;
using RoomSplit.API.DTOs.Transaction;

namespace RoomSplit.API.Validators;

public class CreateTransactionDtoValidator : AbstractValidator<CreateTransactionDto>
{
    public CreateTransactionDtoValidator()
    {
        RuleFor(x => x.Type).InclusiveBetween(0, 1).WithMessage("Type must be 0 (Income) or 1 (Expense).");
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Amount must be greater than 0.");
        RuleFor(x => x.Description).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Date).NotEmpty();
        RuleFor(x => x.ExpenseCategory).InclusiveBetween(0, 11).When(x => x.Type == 1);
        RuleFor(x => x.IncomeCategory).InclusiveBetween(0, 6).When(x => x.Type == 0);
    }
}

public class UpdateTransactionDtoValidator : AbstractValidator<UpdateTransactionDto>
{
    public UpdateTransactionDtoValidator()
    {
        RuleFor(x => x.Type).InclusiveBetween(0, 1);
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Date).NotEmpty();
        RuleFor(x => x.ExpenseCategory).InclusiveBetween(0, 11).When(x => x.Type == 1);
        RuleFor(x => x.IncomeCategory).InclusiveBetween(0, 6).When(x => x.Type == 0);
    }
}

public class TransactionQueryDtoValidator : AbstractValidator<TransactionQueryDto>
{
    public TransactionQueryDtoValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.SortBy).Must(s => new[] { "date", "amount", "description" }.Contains(s.ToLower()))
            .WithMessage("SortBy must be 'date', 'amount', or 'description'.");
        RuleFor(x => x.Order).Must(s => new[] { "asc", "desc" }.Contains(s.ToLower()))
            .WithMessage("Order must be 'asc' or 'desc'.");
    }
}
