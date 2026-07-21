using HRMS.API.Service.Attendance;

namespace HRMS.API.BackgroundServices
{
    public class DailyAttendanceWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DailyAttendanceWorker> _logger;

        public DailyAttendanceWorker(
            IServiceScopeFactory scopeFactory,
            ILogger<DailyAttendanceWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Daily Attendance Worker started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Current server time
                    var now = DateTime.Now;

                    // Run every day at 11:55 PM
                    var nextRun = now.Date
                        .AddHours(23)
                        .AddMinutes(55);

                    // If 11:55 PM already passed today,
                    // schedule for tomorrow
                    if (now >= nextRun)
                    {
                        nextRun = nextRun.AddDays(1);
                    }

                    var delay = nextRun - now;

                    _logger.LogInformation(
                        "Next attendance finalization scheduled at {NextRun}",
                        nextRun);

                    // Wait until scheduled time
                    await Task.Delay(
                        delay,
                        stoppingToken);

                    // Create DI scope because AttendanceService,
                    // Repository and DbContext are Scoped
                    using var scope =
                        _scopeFactory.CreateScope();

                    var attendanceService =
                        scope.ServiceProvider
                            .GetRequiredService<IAttendanceService>();

                    var today =
                        DateOnly.FromDateTime(
                            DateTime.Now);

                    _logger.LogInformation(
                        "Starting attendance finalization for {Date}",
                        today);

                    await attendanceService
                        .FinalizeDailyAttendanceAsync(today);

                    _logger.LogInformation(
                        "Attendance finalization completed for {Date}",
                        today);
                }
                catch (OperationCanceledException)
                {
                    // Application is stopping
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Error occurred while finalizing daily attendance.");

                    // If something fails, wait 5 minutes
                    // before continuing the worker loop
                    await Task.Delay(
                        TimeSpan.FromMinutes(5),
                        stoppingToken);
                }
            }

            _logger.LogInformation(
                "Daily Attendance Worker stopped.");
        }
    }
}