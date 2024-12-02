using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ApiGateway.Services;
using ApiGateway.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace ApiGateway.Controllers.Gateway
{
    [ApiController]
    [Route("gateway/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly ReportService _reportService;

        public ReportsController(ReportService reportService)
        {
            _reportService = reportService;
        }

        // POST: gateway/reports/generate
        [HttpPost("generate")]
        public async Task<IActionResult> GenerateReport([FromBody] ReportRequestModel request)
        {
            // Retrieve the user from HttpContext (set in the middleware)
            var user = HttpContext.Items["User"] as User;

            if (user == null)
            {
                return Unauthorized();
            }

            // Validate the request model
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Generate the report
            var reportBytes = await _reportService.GenerateReportAsync(request);

            // Return the report as a file download
            return File(reportBytes, "text/csv", "report.csv");
        }
    }
}