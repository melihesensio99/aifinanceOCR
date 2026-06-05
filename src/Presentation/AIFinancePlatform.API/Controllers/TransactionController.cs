using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AIFinancePlatform.Application.CQRS.Commands.Transactions.CreateTransaction;
using AIFinancePlatform.Application.CQRS.Commands.Transactions.DeleteTransaction;
using AIFinancePlatform.Application.CQRS.Commands.Transactions.AppendTransactionDescription;
using AIFinancePlatform.Application.CQRS.Queries.Transactions.GetTransactions;
using AIFinancePlatform.Application.DTOs.Transactions;
using AIFinancePlatform.API.Models.Transactions;
using AIFinancePlatform.Application.Common.Models;

namespace AIFinancePlatform.API.Controllers;

[Authorize]
public class TransactionController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult> Get([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var query = new GetTransactionsQuery(CurrentUserId, pageNumber, pageSize);
        var result = await Mediator.Send(query);
        return HandleResult(result);
    }

    [HttpGet("export-pdf")]
    public async Task<IActionResult> ExportPdf()
    {
        var query = new AIFinancePlatform.Application.CQRS.Queries.Transactions.GetTransactionsPdf.GetTransactionsPdfQuery(CurrentUserId);
        var result = await Mediator.Send(query);
        
        if (!result.IsSuccess || result.Data == null)
            return NotFound("PDF oluşturulamadı.");

        // Byte dizisini (PDF dosyasını) doğrudan tarayıcıya indirtiyoruz
        return File(result.Data, "application/pdf", $"HarcamaRaporu_{DateTime.Now:yyyyMMdd}.pdf");
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateTransactionRequest request)
    {
        var command = new CreateTransactionCommand(
            CurrentUserId,
            request.Title,
            request.Amount,
            request.Type,
            request.Date,
            request.Description,
            request.CategoryId,
            request.IsAutomatic,
            request.Source,
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


    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var command = new DeleteTransactionCommand(id, CurrentUserId);
        var result = await Mediator.Send(command);
        
        if (!result.IsSuccess || result.Data == null)
        {
            return HandleResult(result);
        }

        var response = new DeleteTransactionResponse(result.Data.DeletedId, result.Data.Message);
        return Ok(response);
    }
}
