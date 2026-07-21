using HRMS.API.Data;
using HRMS.API.Interfaces.Repositories;
using HRMS.API.Interfaces.Services;
using HRMS.API.Repositories;
using HRMS.API.Repository.Attendance;

using HRMS.API.Repository.UserDetails;
using HRMS.API.Service.Attendance;
using HRMS.API.Service.Authentication;
using HRMS.API.Service.Branch;
using HRMS.API.Service.User;
using HRMS.API.Services;
using Microsoft.EntityFrameworkCore;

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



builder.Services.AddScoped<
    IBranchRepository,
    BranchRepository>();

builder.Services.AddScoped<
    IBranchService,
    BranchService>();

var app = builder.Build();

// Configure HTTP Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();