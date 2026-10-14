using HRMS.API.BackgroundServices;
using HRMS.API.Data;
using HRMS.API.Interfaces.Repositories;
using HRMS.API.Interfaces.Services;
using HRMS.API.Repositories;
using HRMS.API.Repository.Attendance;
using HRMS.API.Repository.Audit;
using HRMS.API.Repository.Holiday;
using HRMS.API.Repository.Leave;
using HRMS.API.Repository.Notification;
using HRMS.API.Repository.Payroll;
using HRMS.API.Repository.Report;
using HRMS.API.Repository.Salary;
using HRMS.API.Repository.UserDetails;
using HRMS.API.Service.Attendance;
using HRMS.API.Service.Audit;
using HRMS.API.Service.Authentication;
using HRMS.API.Service.Branch;
using HRMS.API.Service.Holiday;
using HRMS.API.Service.Leave;
using HRMS.API.Service.Notification;
using HRMS.API.Service.Payroll;
using HRMS.API.Service.Report;
using HRMS.API.Service.Salary;
using HRMS.API.Service.User;
using HRMS.API.Configurations;
using HRMS.API.HealthChecks;
using HRMS.API.Middleware;
using HRMS.API.Service.Email;
using HRMS.API.Services;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add Controllers
builder.Services.AddControllers();

// PostgreSQL DbContext
builder.Services.AddDbContext<DefaultContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped< IBranchRepository,BranchRepository>();
builder.Services.AddScoped< IAttendanceRepository, AttendanceRepository>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<IAttendanceCorrectionRepository, AttendanceCorrectionRepository>();
builder.Services.AddScoped<IAttendanceCorrectionService, AttendanceCorrectionService>();

builder.Services.AddScoped<
    ILeaveRepository,
    LeaveRepository>();

builder.Services.AddScoped<
    ILeaveService,
    LeaveService>();

builder.Services.AddScoped<
    IBranchRepository,
    BranchRepository>();

builder.Services.AddScoped<
    IBranchService,
    BranchService>();



builder.Services.AddScoped<
    IHolidayRepository,
    HolidayRepository>();

builder.Services.AddScoped<
    IHolidayService,
    HolidayService>();


builder.Services.AddScoped<ILeaveTypeRepository, LeaveTypeRepository>();
builder.Services.AddScoped<ILeaveTypeService, LeaveTypeService>();
builder.Services.AddScoped<ILeaveBalanceRepository, LeaveBalanceRepository>();

// Master lookups (departments, designations, employment types, roles, branches)
builder.Services.AddScoped<ILookupRepository, LookupRepository>();
builder.Services.AddScoped<ILookupService, LookupService>();

// Dashboard metrics service
builder.Services.AddScoped<IDashboardService, HRMS.API.Service.Dashboard.DashboardService>();

// Salary and Payroll Engine
builder.Services.AddScoped<ISalaryRepository, SalaryRepository>();
builder.Services.AddScoped<ISalaryService, SalaryService>();
builder.Services.AddScoped<IPayrollRepository, PayrollRepository>();
builder.Services.AddScoped<IPayrollService, PayrollService>();

// In-App Notification System
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// Comprehensive Audit Logging System
builder.Services.AddScoped<IAuditRepository, AuditRepository>();
builder.Services.AddScoped<IAuditService, AuditService>();

// Reporting and Analytical Engine
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IReportService, ReportService>();

// Enterprise Email Service & Gateway
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();

// Production Health Checks (Database & System Liveness)
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database");

// IP Rate Limiting & Throttling
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("AuthLimiter", opt =>
    {
        opt.PermitLimit = 15;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });
});

builder.Services.AddHostedService<DailyAttendanceWorker>();

var app = builder.Build();

// Configure HTTP Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Security Headers Middleware
app.UseMiddleware<SecurityHeadersMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRateLimiter();

app.UseAuthorization();

// Production Health Checks endpoint
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow,
            totalDurationMs = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                durationMs = e.Value.Duration.TotalMilliseconds
            })
        };
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
    }
});

app.MapControllers();

app.Run();