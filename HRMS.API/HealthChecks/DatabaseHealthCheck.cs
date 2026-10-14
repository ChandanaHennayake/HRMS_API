using System;
using System.Threading;
using System.Threading.Tasks;
using HRMS.API.Data;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HRMS.API.HealthChecks
{
    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly DefaultContext _context;

        public DatabaseHealthCheck(DefaultContext context)
        {
            _context = context;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                bool canConnect = await _context.Database.CanConnectAsync(cancellationToken);
                return canConnect
                    ? HealthCheckResult.Healthy("PostgreSQL database connection is operational.")
                    : HealthCheckResult.Unhealthy("Unable to establish PostgreSQL database connection.");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Database health check failed: " + ex.Message, ex);
            }
        }
    }
}
