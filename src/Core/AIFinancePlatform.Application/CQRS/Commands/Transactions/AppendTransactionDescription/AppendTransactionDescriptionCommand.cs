using System;
using MediatR;

namespace AIFinancePlatform.Application.CQRS.Commands.Transactions.AppendTransactionDescription;

public record AppendTransactionDescriptionCommand(
    Guid Id,
    string TextToAppend
) : IRequest<bool>;
