                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AIFinancePlatform.Application.DTOs.Authentication;
using AIFinancePlatform.Application.CQRS.Commands.Authentication.Login;
using AIFinancePlatform.Application.CQRS.Commands.Authentication.Register;
using AIFinancePlatform.Application.CQRS.Commands.Authentication.Refresh;
using Microsoft.AspNetCore.Http;
using System;

namespace AIFinancePlatform.API.Controllers;

[AllowAnonymous]
public class AuthController : ApiControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult> Register([FromBody] RegisterCommand command)
    {
        var result = await Mediator.Send(command);
        if (result.IsSuccess && result.Data != null)
        {
            SetRefreshTokenCookie(result.Data.RefreshToken);
        }
        return HandleResult(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody] LoginCommand command)
    {
        var result = await Mediator.Send(command);
        if (result.IsSuccess && result.Data != null)
        {
            SetRefreshTokenCookie(result.Data.RefreshToken);
        }
        return HandleResult(result);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult> Refresh()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized(new { message = "Refresh token bulunamadı." });
        }

        var command = new RefreshCommand(refreshToken);
        var result = await Mediator.Send(command);
        
        if (result.IsSuccess && result.Data != null)
        {
            SetRefreshTokenCookie(result.Data.RefreshToken);
        }
        else if (!result.IsSuccess)
        {
            return Unauthorized(new { message = result.ErrorMessage });
        }
        
        return HandleResult(result);
    }

    private void SetRefreshTokenCookie(string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTime.UtcNow.AddDays(30),
            Secure = true, // HTTPS üzerinden çalışır
            SameSite = SameSiteMode.Strict // Sadece aynı domainden gelen isteklere izin ver
        };

        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }
}
