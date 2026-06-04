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
    public async Task<ActionResult<PaginatedList<TransactionDto>>> Get([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var query = new GetTransactionsQuery(CurrentUserId, pageNumber, pageSize);
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("export-pdf")]
    public async Task<IActionResult> ExportPdf()
    {
        var query = new AIFinancePlatform.Application.CQRS.Queries.Transactions.GetTransactionsPdf.GetTransactionsPdfQuery(CurrentUserId);
        var pdfBytes = await Mediator.Send(query);
        
        // Byte dizisini (PDF dosyasını) doğrudan tarayıcıya indirtiyoruz
        return File(pdfBytes, "application/pdf", $"HarcamaRaporu_{DateTime.Now:yyyyMMdd}.pdf");
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
    public async Task<ActionResult<DeleteTransactionResponse>> Delete(Guid id)
    {
        var command = new DeleteTransactionCommand(id, CurrentUserId);
        var result = await Mediator.Send(command);
        var response = new DeleteTransactionResponse(result.DeletedId, result.Message);
        return Ok(response);
    }
}
