using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AIFinancePlatform.Application.DTOs.Authentication;
using AIFinancePlatform.Application.CQRS.Commands.Authentication.Login;
using AIFinancePlatform.Application.CQRS.Commands.Authentication.Register;

namespace AIFinancePlatform.API.Controllers;

[AllowAnonymous]
public class AuthController : ApiControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(result);
    }
}
