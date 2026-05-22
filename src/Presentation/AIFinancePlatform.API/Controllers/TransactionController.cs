using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AIFinancePlatform.Application.CQRS.Commands.Transactions.CreateTransaction;
using AIFinancePlatform.Application.CQRS.Commands.Transactions.DeleteTransaction;
using AIFinancePlatform.Application.CQRS.Queries.Transactions.GetTransactions;
using AIFinancePlatform.Application.DTOs.Transactions;
using AIFinancePlatform.API.Models.Transactions;

namespace AIFinancePlatform.API.Controllers;

[Authorize]
public class TransactionController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<TransactionDto>>> Get()
    {
        var query = new GetTransactionsQuery(CurrentUserId);
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<CreateTransactionResponse>> Create([FromBody] CreateTransactionRequest request)
    {
        var command = new CreateTransactionCommand(
            CurrentUserId,
            request.Title,
            request.Amount,
            request.Type,
            request.Date,
            request.Description,
            request.CategoryId
        );
        var result = await Mediator.Send(command);
        var response = new CreateTransactionResponse(result.Transaction, result.Message);
        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("ai-webhook")]
    public async Task<ActionResult<CreateTransactionResponse>> CreateFromAi([FromBody] CreateTransactionAiRequest request, [FromHeader(Name = "x-ai-api-key")] string apiKey)
    {
        // Basit bir iç mikroservis güvenliği (Gerçek senaryoda Config'den okunur)
        if (apiKey != "secret_ai_key_123")
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
            request.CategoryId
        );
        var result = await Mediator.Send(command);
        var response = new CreateTransactionResponse(result.Transaction, result.Message);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<DeleteTransactionResponse>> Delete(Guid id)
    {
        var command = new DeleteTransactionCommand(id, CurrentUserId);
        var result = await Mediator.Send(command);
        var response = new DeleteTransactionResponse(result.DeletedId, result.Message);
        return Ok(response);
    }
}
