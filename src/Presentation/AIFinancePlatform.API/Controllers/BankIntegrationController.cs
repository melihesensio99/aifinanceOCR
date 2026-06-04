using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AIFinancePlatform.Application.CQRS.Commands.BankIntegration;
using AIFinancePlatform.API.Models.BankIntegration;
using AIFinancePlatform.Domain.Enums;

namespace AIFinancePlatform.API.Controllers;

[Authorize]
public class BankIntegrationController : ApiControllerBase
{
    [HttpPost("sync")]
    public async Task<IActionResult> Sync([FromBody] SyncBankTransactionsRequest request)
    {
        if (!Enum.TryParse<BankType>(request.BankName, true, out var bankType))
        {
            return BadRequest(new { Message = $"Bank type '{request.BankName}' is not valid." });
        }

        var command = new SyncBankTransactionsCommand(bankType, CurrentUserId);
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }
}