using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AIFinancePlatform.Application.CQRS.Queries.PriceCache.GetPriceCache;
using AIFinancePlatform.Application.CQRS.Commands.PriceCache.CreatePriceCache;
using Microsoft.Extensions.Configuration;

namespace AIFinancePlatform.API.Controllers;

[AllowAnonymous]
public class PriceCacheController : ApiControllerBase
{
    private readonly IConfiguration _configuration;

    public PriceCacheController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet]
    public async Task<ActionResult<string>> Get([FromQuery] string term, [FromHeader(Name = "x-ai-api-key")] string apiKey)
    {
        var expectedApiKey = _configuration["AIApiKey"];
        if (apiKey != expectedApiKey)
        {
            return Unauthorized(new { message = "Geçersiz AI API Key." });
        }

        var query = new GetPriceCacheQuery(term);
        var price = await Mediator.Send(query);
        
        if (string.IsNullOrEmpty(price))
            return NotFound();
            
        return Ok(new { price });
    }

    public class CreatePriceCacheRequest
    {
        public string SearchTerm { get; set; }
        public string Price { get; set; }
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreatePriceCacheRequest request, [FromHeader(Name = "x-ai-api-key")] string apiKey)
    {
        var expectedApiKey = _configuration["AIApiKey"];
        if (apiKey != expectedApiKey)
        {
            return Unauthorized(new { message = "Geçersiz AI API Key." });
        }

        var command = new CreatePriceCacheCommand(request.SearchTerm, request.Price);
        await Mediator.Send(command);
        return Ok();
    }
}
