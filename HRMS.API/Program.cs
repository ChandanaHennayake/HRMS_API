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
using HRMS.API.Helpers;
using HRMS.API.Middleware;
using HRMS.API.Service.Email;
using HRMS.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Support dynamic port configuration (required for Render / cloud container hosting)
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

// Add Controllers
builder.Services.AddControllers();

// Enable CORS for mobile and web clients
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure JWT Bearer Authentication
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
var jwtKey = builder.Configuration["Jwt:Key"] ?? "HRMS_SUPER_SECRET_JWT_KEY_2026_SECURE_123456789";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "HRMS.API";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "HRMS.Client";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// PostgreSQL DbContext with Supabase & cloud connection string resolution
builder.Services.AddDbContext<DefaultContext>(options =>
{
    var connectionString = ConnectionStringHelper.ResolveConnectionString(builder.Configuration);
    options.UseNpgsql(connectionString);
});

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
var enableSwagger = app.Environment.IsDevelopment() ||
                    builder.Configuration.GetValue<bool>("ENABLE_SWAGGER", false) ||
                    builder.Configuration.GetValue<bool>("EnableSwagger", false);

if (enableSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Forwarded headers for reverse proxies (Render, AWS, Cloudflare)
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// Enable CORS
app.UseCors("AllowAll");

// Security Headers Middleware
app.UseMiddleware<SecurityHeadersMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

if (builder.Configuration.GetValue<bool>("ENABLE_HTTPS_REDIRECTION", false))
{
    app.UseHttpsRedirection();
}

app.UseRateLimiter();

app.UseAuthentication();
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