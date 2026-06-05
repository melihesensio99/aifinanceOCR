using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AIFinancePlatform.Application.CQRS.Queries.Analytics.GetDashboardSummary;
using AIFinancePlatform.Application.DTOs.Analytics;
using AIFinancePlatform.Domain.Enums;

namespace AIFinancePlatform.API.Controllers;

[Authorize]
public class AnalyticsController : ApiControllerBase
{
    [HttpGet("dashboard")]
    public async Task<ActionResult> GetDashboardSummary([FromQuery] string period = "AllTime")
    {
        if (!Enum.TryParse<TimePeriod>(period, true, out var timePeriod))
        {
            timePeriod = TimePeriod.AllTime;
        }

        var query = new GetDashboardSummaryQuery(CurrentUserId, timePeriod);
        var result = await Mediator.Send(query);

        return HandleResult(result);
    }
}
