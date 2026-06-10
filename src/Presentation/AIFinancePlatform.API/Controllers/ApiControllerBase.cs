using System;
using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using AIFinancePlatform.Application.Common.Models;
using AIFinancePlatform.Application.Common.Interfaces;

namespace AIFinancePlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    private ISender? _mediator;

    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    protected Guid CurrentUserId
    {
        get
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Guid.Empty;
            }
            return userId;
        }
    }

    protected ActionResult HandleResult<T>(Result<T> result)
    {
        if (result == null) return NotFound();
        if (result.IsSuccess && result.Data != null) return Ok(result.Data);
        if (result.IsSuccess && result.Data == null) return NotFound();
        
        if (result is IValidationResult validationResult)
        {
            return BadRequest(new { message = result.ErrorMessage, validationErrors = validationResult.ValidationErrors });
        }
        
        return BadRequest(new { message = result.ErrorMessage, errors = result.Errors });
    }

    protected ActionResult HandleResult(Result result)
    {
        if (result == null) return NotFound();
        if (result.IsSuccess) return Ok();
        
        if (result is IValidationResult validationResult)
        {
            return BadRequest(new { message = result.ErrorMessage, validationErrors = validationResult.ValidationErrors });
        }
        
        return BadRequest(new { message = result.ErrorMessage, errors = result.Errors });
    }
}
