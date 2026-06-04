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
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterCommand command)
    {
        var result = await Mediator.Send(command);
        SetRefreshTokenCookie(result.RefreshToken);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginCommand command)
    {
        var result = await Mediator.Send(command);
        SetRefreshTokenCookie(result.RefreshToken);
        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponseDto>> Refresh()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized(new { message = "Refresh token bulunamadı." });
        }

        try
        {
            var command = new RefreshCommand(refreshToken);
            var result = await Mediator.Send(command);
            SetRefreshTokenCookie(result.RefreshToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
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
