using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using AIFinancePlatform.Application.CQRS.Commands.Transactions.CreateTransaction;
using AIFinancePlatform.Application.CQRS.Commands.Transactions.AppendTransactionDescription;
using AIFinancePlatform.API.Models.Transactions;

namespace AIFinancePlatform.API.Controllers;

[AllowAnonymous]
[Route("api/ai-webhook")]
public class AiWebhookController : ApiControllerBase
{
    private readonly IConfiguration _configuration;

    public AiWebhookController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost("transactions")]
    public async Task<ActionResult> CreateFromAi([FromBody] CreateTransactionAiRequest request, [FromHeader(Name = "x-ai-api-key")] string apiKey)
    {
        var expectedApiKey = _configuration["AIApiKey"];
        if (apiKey != expectedApiKey)
        {
            return Unauthorized(new { message = "Geçersiz AI API Key." });
        }

        var command = new CreateTransactionCommand(
            request.UserId,
            request.Title,
            request.Amount,
            request.Type,
            request.Date,
            request.Description,
            request.CategoryId,
            true, // AI requests are automatic
            "OCR",
            request.ReceiptImageUrl
        );
        var result = await Mediator.Send(command);
        
        if (!result.IsSuccess || result.Data == null)
        {
            return HandleResult(result);
        }

        var response = new CreateTransactionResponse(result.Data.Transaction, result.Data.Message);
        return Ok(response);
    }

    [HttpPut("transactions/{id}/append-description")]
    public async Task<ActionResult> AppendDescriptionFromAi(Guid id, [FromBody] AppendDescriptionRequest request, [FromHeader(Name = "x-ai-api-key")] string apiKey)
    {
        var expectedApiKey = _configuration["AIApiKey"];
        if (apiKey != expectedApiKey)
        {
            return Unauthorized(new { message = "Geçersiz AI API Key." });
        }

        var command = new AppendTransactionDescriptionCommand(id, request.TextToAppend);
        var result = await Mediator.Send(command);
        
        if (!result.IsSuccess) return NotFound();
        return Ok();
    }
}
