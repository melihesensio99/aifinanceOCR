using FluentValidation;

namespace AIFinancePlatform.Application.CQRS.Commands.Transactions.CreateTransaction;

public class CreateTransactionCommandValidator : AbstractValidator<CreateTransactionCommand>
{
    public CreateTransactionCommandValidator()
    {
        RuleFor(v => v.Amount)
            .GreaterThan(0)
            .WithMessage("Tutar 0'dan büyük olmalıdır.");

        RuleFor(v => v.Title)
            .NotEmpty()
            .WithMessage("Başlık boş olamaz.");

        RuleFor(v => v.Type)
            .NotEmpty()
            .WithMessage("İşlem tipi boş olamaz.");
    }
}
