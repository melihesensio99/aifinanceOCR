using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AIFinancePlatform.Application.CQRS.Queries.Categories.GetCategories;
using AIFinancePlatform.Application.DTOs.Categories;

namespace AIFinancePlatform.API.Controllers;

[Authorize]
public class CategoryController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> Get()
    {
        var query = new GetCategoriesQuery(CurrentUserId);
        var result = await Mediator.Send(query);
        return Ok(result);
    }
}
