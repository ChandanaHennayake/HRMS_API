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

// Enable legacy timestamp behavior for Npgsql to prevent Kind=Local timestamp without time zone cast errors
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

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
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
{
    jwtKey = "HRMS_SUPER_SECRET_JWT_KEY_2026_SECURE_123456789";
}
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
if (string.IsNullOrWhiteSpace(jwtIssuer))
{
    jwtIssuer = "HRMS.API";
}
var jwtAudience = builder.Configuration["Jwt:Audience"];
if (string.IsNullOrWhiteSpace(jwtAudience))
{
    jwtAudience = "HRMS.Client";
}

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
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null);
        npgsqlOptions.CommandTimeout(30);
    });
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

// Ensure dynamic PostgreSQL tables and compatibility columns exist
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<DefaultContext>();

        // 1. EmployeeSalary columns compatibility (BankAccountNo & PaymentMode aliases)
        try
        {
            db.Database.ExecuteSqlRaw(@"
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'EmployeeSalary') THEN
                        ALTER TABLE ""EmployeeSalary"" ADD COLUMN IF NOT EXISTS ""BankAccountNo"" character varying(50);
                        ALTER TABLE ""EmployeeSalary"" ADD COLUMN IF NOT EXISTS ""BankAccountNumber"" character varying(50);
                        ALTER TABLE ""EmployeeSalary"" ADD COLUMN IF NOT EXISTS ""PaymentMode"" character varying(50) DEFAULT 'Bank Transfer';
                        ALTER TABLE ""EmployeeSalary"" ADD COLUMN IF NOT EXISTS ""PaymentMethod"" smallint DEFAULT 1;

                        IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'EmployeeSalary' AND column_name = 'BankAccountNumber') THEN
                            EXECUTE 'UPDATE ""EmployeeSalary"" SET ""BankAccountNo"" = ""BankAccountNumber"" WHERE ""BankAccountNo"" IS NULL AND ""BankAccountNumber"" IS NOT NULL';
                        END IF;

                        IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'EmployeeSalary' AND column_name = 'BankAccountNo') THEN
                            EXECUTE 'UPDATE ""EmployeeSalary"" SET ""BankAccountNumber"" = ""BankAccountNo"" WHERE ""BankAccountNumber"" IS NULL AND ""BankAccountNo"" IS NOT NULL';
                        END IF;

                        UPDATE ""EmployeeSalary"" SET ""PaymentMode"" = 'Bank Transfer' WHERE ""PaymentMode"" IS NULL;
                    END IF;

                    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'Employee') THEN
                        ALTER TABLE ""Employee"" ADD COLUMN IF NOT EXISTS ""BankName"" character varying(100);
                        ALTER TABLE ""Employee"" ADD COLUMN IF NOT EXISTS ""BankAccountNo"" character varying(50);
                        ALTER TABLE ""Employee"" ADD COLUMN IF NOT EXISTS ""BankBranch"" character varying(100);
                        ALTER TABLE ""Employee"" ADD COLUMN IF NOT EXISTS ""Qualification"" character varying(255);
                    END IF;

                    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'AppUser') THEN
                        ALTER TABLE ""AppUser"" ADD COLUMN IF NOT EXISTS ""ClearPassword"" text;
                        ALTER TABLE ""AppUser"" ADD COLUMN IF NOT EXISTS ""IsDeleted"" boolean DEFAULT false;
                        UPDATE ""AppUser"" SET ""ClearPassword"" = 'Admin@123' WHERE ""ClearPassword"" IS NULL AND ""UserId"" = 1;
                        IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'AppUser' AND column_name = 'PasswordSalt') THEN
                            EXECUTE 'UPDATE ""AppUser"" SET ""ClearPassword"" = ""PasswordSalt"" WHERE ""ClearPassword"" IS NULL AND ""PasswordSalt"" IS NOT NULL AND ""PasswordSalt"" != ''SYSTEM_BCRYPT_SALT''';
                            EXECUTE 'UPDATE ""AppUser"" SET ""PasswordSalt"" = ""ClearPassword"" WHERE ""PasswordSalt"" IS NULL AND ""ClearPassword"" IS NOT NULL';
                        END IF;
                    END IF;
                END $$;
            ");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Startup] Employee/EmployeeSalary migration notice: {ex.Message}");
        }

        // 2. AttendanceCorrection table
        try
        {
            db.Database.ExecuteSqlRaw(@"
                CREATE TABLE IF NOT EXISTS ""AttendanceCorrection"" (
                    ""Id"" BIGSERIAL PRIMARY KEY,
                    ""AttendanceId"" INTEGER,
                    ""EmployeeId"" BIGINT NOT NULL,
                    ""AttendanceDate"" DATE NOT NULL,
                    ""RequestedCheckInTime"" TIMESTAMP WITHOUT TIME ZONE,
                    ""RequestedCheckOutTime"" TIMESTAMP WITHOUT TIME ZONE,
                    ""Reason"" VARCHAR(500) NOT NULL,
                    ""Status"" SMALLINT NOT NULL DEFAULT 0,
                    ""ReviewedBy"" BIGINT,
                    ""ReviewedAt"" TIMESTAMP WITHOUT TIME ZONE,
                    ""ReviewerComment"" VARCHAR(500),
                    ""CreatedAt"" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW(),
                    ""UpdatedAt"" TIMESTAMP WITHOUT TIME ZONE
                );
            ");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Startup] AttendanceCorrection table notice: {ex.Message}");
        }

        // 3. Department, Designation, EmploymentType column aliases
        try
        {
            db.Database.ExecuteSqlRaw(@"
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'Department') THEN
                        ALTER TABLE ""Department"" ADD COLUMN IF NOT EXISTS ""Name"" character varying(100);
                        ALTER TABLE ""Department"" ADD COLUMN IF NOT EXISTS ""Code"" character varying(50);
                        IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Department' AND column_name = 'DepartmentName') THEN
                            EXECUTE 'UPDATE ""Department"" SET ""Name"" = ""DepartmentName"" WHERE ""Name"" IS NULL AND ""DepartmentName"" IS NOT NULL';
                        END IF;
                        IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Department' AND column_name = 'DepartmentCode') THEN
                            EXECUTE 'UPDATE ""Department"" SET ""Code"" = ""DepartmentCode"" WHERE ""Code"" IS NULL AND ""DepartmentCode"" IS NOT NULL';
                        END IF;
                    END IF;

                    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'Designation') THEN
                        ALTER TABLE ""Designation"" ADD COLUMN IF NOT EXISTS ""Name"" character varying(100);
                        ALTER TABLE ""Designation"" ADD COLUMN IF NOT EXISTS ""Code"" character varying(50);
                        IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Designation' AND column_name = 'DesignationName') THEN
                            EXECUTE 'UPDATE ""Designation"" SET ""Name"" = ""DesignationName"" WHERE ""Name"" IS NULL AND ""DesignationName"" IS NOT NULL';
                        END IF;
                        IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Designation' AND column_name = 'DesignationCode') THEN
                            EXECUTE 'UPDATE ""Designation"" SET ""Code"" = ""DesignationCode"" WHERE ""Code"" IS NULL AND ""DesignationCode"" IS NOT NULL';
                        END IF;
                    END IF;

                    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'EmploymentType') THEN
                        ALTER TABLE ""EmploymentType"" ADD COLUMN IF NOT EXISTS ""Name"" character varying(100);
                        ALTER TABLE ""EmploymentType"" ADD COLUMN IF NOT EXISTS ""Code"" character varying(50);
                        IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'EmploymentType' AND column_name = 'TypeName') THEN
                            EXECUTE 'UPDATE ""EmploymentType"" SET ""Name"" = ""TypeName"" WHERE ""Name"" IS NULL AND ""TypeName"" IS NOT NULL';
                        END IF;
                        IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'EmploymentType' AND column_name = 'TypeCode') THEN
                            EXECUTE 'UPDATE ""EmploymentType"" SET ""Code"" = ""TypeCode"" WHERE ""Code"" IS NULL AND ""TypeCode"" IS NOT NULL';
                        END IF;
                    END IF;
                END $$;
            ");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Startup] Lookups migration notice: {ex.Message}");
        }

        // 4. SystemNotice table
        try
        {
            db.Database.ExecuteSqlRaw(@"
                CREATE TABLE IF NOT EXISTS ""SystemNotice"" (
                    ""Id"" BIGSERIAL PRIMARY KEY,
                    ""Title"" VARCHAR(200) NOT NULL,
                    ""Message"" TEXT,
                    ""ImageUrl"" TEXT,
                    ""IsActive"" BOOLEAN NOT NULL DEFAULT TRUE,
                    ""TargetRole"" VARCHAR(50) NOT NULL DEFAULT 'All',
                    ""CreatedAt"" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW(),
                    ""CreatedBy"" INTEGER
                );
            ");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Startup] SystemNotice table initialization notice: {ex.Message}");
        }

        // 5. Payroll & Master Entity Schema Reconciliation
        try
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            await DatabaseInitializer.EnsureSchemaUpToDateAsync(db, logger);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Startup] Payroll schema reconciliation notice: {ex.Message}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Startup] DB scope initialization notice: {ex.Message}");
    }
}

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

// Global exception logging middleware
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[HTTP 500 Unhandled Exception] {context.Request.Method} {context.Request.Path}: {ex.Message}");
        if (!context.Response.HasStarted)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
            {
                success = false,
                message = "An unexpected server error occurred.",
                error = ex.Message
            }));
        }
    }
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
app.MapGet("/", () => Results.Ok(new
{
    service = "AlienHR Web API",
    status = "Online",
    environment = app.Environment.EnvironmentName,
    health = "/health",
    version = "1.0.0"
}));

app.Run();