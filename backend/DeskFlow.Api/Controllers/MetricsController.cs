using DeskFlow.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeskFlow.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/metrics")]
public class MetricsController(MetricsService metricsService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get() => Ok(await metricsService.GetAsync());
}
