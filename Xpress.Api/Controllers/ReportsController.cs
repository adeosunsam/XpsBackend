using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xpress.Application.DTOs.Reports;
using Xpress.Application.Interfaces;
using Xpress.Domain.Constants;

namespace Xpress.Api.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize(Roles = Roles.Admin)]
public sealed class ReportsController : ControllerBase
{
    private readonly IAuditService _audit;

    public ReportsController(IAuditService audit) => _audit = audit;

    [HttpPost("transactions")]
    [ProducesResponseType(typeof(GenerateReportResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult RequestTransactionReport([FromBody] GenerateReportRequest request)
    {
        var adminEmail = User.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrWhiteSpace(adminEmail))
            return Problem("Email claim not found in token.", statusCode: StatusCodes.Status401Unauthorized);

        var response = _audit.ScheduleTransactionReport(request, adminEmail);
        return Accepted(response);
    }
}
