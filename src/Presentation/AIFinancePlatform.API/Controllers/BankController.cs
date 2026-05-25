using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AIFinancePlatform.Application.CQRS.Commands.BankIntegration;
using AIFinancePlatform.API.Models.BankIntegration;
using AIFinancePlatform.Domain.Enums;

namespace AIFinancePlatform.API.Controllers;

[Authorize]
public class BankController : ApiControllerBase
{
    [HttpPost("sync")]
    public async Task<ActionResult> Sync([FromBody] SyncBankTransactionsRequest request)
    {
        // Default to Garanti if parsing fails, or handle properly
        if (!Enum.TryParse<BankType>(request.BankName, true, out var bankType))
        {
            bankType = BankType.Garanti;
        }

        var command = new SyncBankTransactionsCommand(bankType, CurrentUserId);
        var result = await Mediator.Send(command);

        if (result)
        {
            return Ok(new { message = "Banka işlemleri başarıyla eşitlendi." });
        }

        return BadRequest(new { message = "Eşitleme sırasında bir hata oluştu." });
    }
}
