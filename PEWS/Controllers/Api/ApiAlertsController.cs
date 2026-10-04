using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PEWS.Data;
using PEWS.Models.Enums;
using PEWS.ViewModels.Api;

namespace PEWS.Controllers.Api;

[ApiController]
[Route("api/alerts")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ApiAlertsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ApiAlertsController(ApplicationDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetAlerts(int page = 1, int pageSize = 20)
    {
        var alerts = await _context.Alerts
            .Include(a => a.AlertCategory)
            .Include(a => a.Region)
            .Where(a => a.Status == AlertStatus.Published && a.IsPublished)
            .OrderByDescending(a => a.PublishedAt ?? a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AlertDto
            {
                Id          = a.Id,
                Title       = a.Title,
                Message     = a.Message,
                Severity    = a.Severity.ToString(),
                Region      = a.Region != null ? a.Region.Name : null,
                Category    = a.AlertCategory != null ? a.AlertCategory.Name : "General",
                PublishedAt = a.PublishedAt ?? a.CreatedAt
            })
            .ToListAsync();

        return Ok(alerts);
    }
}
