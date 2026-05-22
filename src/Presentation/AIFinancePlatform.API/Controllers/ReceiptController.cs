using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using AIFinancePlatform.Application.CQRS.Commands.Receipts.UploadReceipt;
using AIFinancePlatform.API.Models.Receipts;

namespace AIFinancePlatform.API.Controllers;

[Authorize]
public class ReceiptController : ApiControllerBase
{
    [HttpPost("upload")]
    public async Task<ActionResult<UploadReceiptResponse>> UploadReceipt(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("Dosya seçilmedi.");
        }

        using var stream = file.OpenReadStream();
        var command = new UploadReceiptCommand(
            CurrentUserId,
            stream,
            file.FileName
        );

        var result = await Mediator.Send(command);

        var response = new UploadReceiptResponse(result.Message, result.OriginalFileName);
        return Accepted(response);
    }
}
