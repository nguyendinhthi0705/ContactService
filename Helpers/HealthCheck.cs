using AzureFunctions.Extensions.Swashbuckle;
using AzureFunctions.Extensions.Swashbuckle.Attribute;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Net.Http;

namespace ContactService.Helpers
{
    public class HealthCheck
    {     

        private readonly HealthCheckService _healthCheck;
        public HealthCheck(HealthCheckService healthCheck)
        {
            _healthCheck = healthCheck;
        }

        [FunctionName("Liveness")]
        public async Task<IActionResult> Liveness(
           [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Liveness")] HttpRequest req,
           ILogger log)
        {
            log.Log(LogLevel.Information, "Received Liveness request");

            return new OkObjectResult("Healthy");
        }

        [FunctionName("Readness")]
        public async Task<IActionResult> Readness(
           [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Readness")] HttpRequest req,
           ILogger log)
        {
            log.Log(LogLevel.Information, "Received Readness request");

            var status = await _healthCheck.CheckHealthAsync();
            return new OkObjectResult(Enum.GetName(typeof(HealthStatus), status.Status));
        }

    }
}
