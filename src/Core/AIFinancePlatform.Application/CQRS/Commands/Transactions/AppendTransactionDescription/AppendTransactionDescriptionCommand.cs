using System;
using MediatR;

using AIFinancePlatform.Application.Common.Models;

namespace AIFinancePlatform.Application.CQRS.Commands.Transactions.AppendTransactionDescription;

public record AppendTransactionDescriptionCommand(
    Guid Id,
    string TextToAppend
) : IRequest<Result<bool>>;
