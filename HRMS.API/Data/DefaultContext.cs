using System;
using System.Collections.Generic;
using HRMS.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Data;

public partial class DefaultContext : DbContext
{
    public DefaultContext()
    {
    }

    public DefaultContext(DbContextOptions<DefaultContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AppUser> AppUsers { get; set; }

    public virtual DbSet<Attendance> Attendances { get; set; }

    public virtual DbSet<AttendanceCorrection> AttendanceCorrections { get; set; }

    public virtual DbSet<Branch> Branches { get; set; }

    public virtual DbSet<Company> Companies { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<Designation> Designations { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<EmploymentType> EmploymentTypes { get; set; }

    public virtual DbSet<Holiday> Holidays { get; set; }

    public virtual DbSet<LeaveRequest> LeaveRequests { get; set; }

    public virtual DbSet<EmployeeLeaveBalance> EmployeeLeaveBalances { get; set; }

    public virtual DbSet<SalaryComponent> SalaryComponents { get; set; }

    public virtual DbSet<SalaryStructure> SalaryStructures { get; set; }

    public virtual DbSet<SalaryStructureComponent> SalaryStructureComponents { get; set; }

    public virtual DbSet<EmployeeSalary> EmployeeSalaries { get; set; }

    public virtual DbSet<EmployeeSalaryComponent> EmployeeSalaryComponents { get; set; }

    public virtual DbSet<PayrollPeriod> PayrollPeriods { get; set; }

    public virtual DbSet<PayrollRun> PayrollRuns { get; set; }

    public virtual DbSet<Payslip> Payslips { get; set; }

    public virtual DbSet<PayslipItem> PayslipItems { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<LeaveType> LeaveTypes { get; set; }

    public virtual DbSet<LoginHistory> LoginHistories { get; set; }

    public virtual DbSet<PasswordReset> PasswordResets { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<RolePermission> RolePermissions { get; set; }

    public virtual DbSet<UserBranch> UserBranches { get; set; }

    public virtual DbSet<SystemNotice> SystemNotices { get; set; }

    public virtual DbSet<AppVersion> AppVersions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=ABANS_BAN;Username=postgres;Password=Nishantha@123");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("User_pkey");

            entity.Property(e => e.UserId).HasDefaultValueSql("nextval('\"User_userid_seq\"'::regclass)");
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.FailedLoginAttempts).HasDefaultValue(0);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsLocked).HasDefaultValue(false);

            entity.HasOne(d => d.Branch).WithMany(p => p.AppUsers).HasConstraintName("User_branchid_fkey");

            entity.HasOne(d => d.Company).WithMany(p => p.AppUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("User_companyid_fkey");

            entity.HasOne(d => d.Role).WithMany(p => p.AppUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("User_roleid_fkey");
        });

        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Attendance_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.EarlyDepartureMinutes).HasDefaultValue(0);
            entity.Property(e => e.IsEarlyDeparture).HasDefaultValue(false);
            entity.Property(e => e.IsLate).HasDefaultValue(false);
            entity.Property(e => e.LateMinutes).HasDefaultValue(0);
            entity.Property(e => e.OTMinutes).HasDefaultValue(0);
            entity.Property(e => e.Status).HasDefaultValue(1);
            entity.Property(e => e.WorkedMinutes).HasDefaultValue(0);

            entity.HasOne(d => d.Employee).WithMany(p => p.Attendances)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Attendance_Employee");
        });

        modelBuilder.Entity<AttendanceCorrection>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("AttendanceCorrection_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Status).HasDefaultValue((short)0);

            entity.HasOne(d => d.Attendance).WithMany()
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_AttendanceCorrection_Attendance");

            entity.HasOne(d => d.Employee).WithMany()
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AttendanceCorrection_Employee");
        });

        modelBuilder.Entity<Branch>(entity =>
        {
            entity.HasKey(e => e.branchid).HasName("branch_pkey");

            entity.Property(e => e.branchid).HasDefaultValueSql("nextval('branch_branchid_seq'::regclass)");
            entity.Property(e => e.GeofenceRadiusMeters).HasDefaultValue(100);
            entity.Property(e => e.createddate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.isactive).HasDefaultValue(true);

            entity.HasOne(d => d.company).WithMany(p => p.Branches)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("branch_companyid_fkey");
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(e => e.companyid).HasName("company_pkey");

            entity.Property(e => e.companyid).HasDefaultValueSql("nextval('company_companyid_seq'::regclass)");
            entity.Property(e => e.createddate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.isactive).HasDefaultValue(true);
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Department_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.Name).HasColumnName("DepartmentName");
            entity.Property(e => e.Code).HasColumnName("DepartmentCode");
        });

        modelBuilder.Entity<Designation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Designation_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.Name).HasColumnName("DesignationName");
            entity.Property(e => e.Code).HasColumnName("DesignationCode");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Employee_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasOne(d => d.Department).WithMany(p => p.Employees).HasConstraintName("FK_Employee_Department");

            entity.HasOne(d => d.Designation).WithMany(p => p.Employees).HasConstraintName("FK_Employee_Designation");

            entity.HasOne(d => d.EmploymentType).WithMany(p => p.Employees).HasConstraintName("FK_Employee_EmploymentType");
        });

        modelBuilder.Entity<EmploymentType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("EmploymentType_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.Name).HasColumnName("TypeName");
        });

        modelBuilder.Entity<Holiday>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Holiday_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<LeaveRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("LeaveRequest_pkey");

            entity.Property(e => e.AppliedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Status).HasDefaultValue((short)0);

            entity.HasOne(d => d.Employee).WithMany(p => p.LeaveRequests)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LeaveRequest_Employee");

            entity.HasOne(d => d.LeaveType).WithMany(p => p.LeaveRequests)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LeaveRequest_LeaveType");
        });

        modelBuilder.Entity<LeaveType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("LeaveType_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsPaid).HasDefaultValue(true);
        });

        modelBuilder.Entity<EmployeeLeaveBalance>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("EmployeeLeaveBalance_pkey");

            entity.Property(e => e.AllocatedDays).HasDefaultValue(0m);
            entity.Property(e => e.UsedDays).HasDefaultValue(0m);
            entity.Property(e => e.PendingDays).HasDefaultValue(0m);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Employee).WithMany()
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EmployeeLeaveBalance_Employee");

            entity.HasOne(d => d.LeaveType).WithMany()
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EmployeeLeaveBalance_LeaveType");
        });

        modelBuilder.Entity<LoginHistory>(entity =>
        {
            entity.HasKey(e => e.loginhistoryid).HasName("loginhistory_pkey");

            entity.Property(e => e.loginhistoryid).HasDefaultValueSql("nextval('loginhistory_loginhistoryid_seq'::regclass)");
            entity.Property(e => e.logintime).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.user).WithMany(p => p.LoginHistories).HasConstraintName("loginhistory_userid_fkey");
        });

        modelBuilder.Entity<PasswordReset>(entity =>
        {
            entity.HasKey(e => e.passwordresetid).HasName("passwordreset_pkey");

            entity.Property(e => e.passwordresetid).HasDefaultValueSql("nextval('passwordreset_passwordresetid_seq'::regclass)");
            entity.Property(e => e.createddate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.used).HasDefaultValue(false);

            entity.HasOne(d => d.user).WithMany(p => p.PasswordResets)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("passwordreset_userid_fkey");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.permissionid).HasName("permission_pkey");

            entity.Property(e => e.permissionid).HasDefaultValueSql("nextval('permission_permissionid_seq'::regclass)");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.refreshtokenid).HasName("refreshtoken_pkey");

            entity.Property(e => e.refreshtokenid).HasDefaultValueSql("nextval('refreshtoken_refreshtokenid_seq'::regclass)");
            entity.Property(e => e.createddate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.isrevoked).HasDefaultValue(false);

            entity.HasOne(d => d.user).WithMany(p => p.RefreshTokens)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("refreshtoken_userid_fkey");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.roleid).HasName("role_pkey");

            entity.Property(e => e.roleid).HasDefaultValueSql("nextval('role_roleid_seq'::regclass)");
            entity.Property(e => e.createddate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.isactive).HasDefaultValue(true);
            entity.Property(e => e.issystemrole).HasDefaultValue(false);

            entity.HasOne(d => d.company).WithMany(p => p.Roles).HasConstraintName("role_companyid_fkey");
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(e => e.rolepermissionid).HasName("rolepermission_pkey");

            entity.Property(e => e.rolepermissionid).HasDefaultValueSql("nextval('rolepermission_rolepermissionid_seq'::regclass)");
            entity.Property(e => e.canapprove).HasDefaultValue(false);
            entity.Property(e => e.cancreate).HasDefaultValue(false);
            entity.Property(e => e.candelete).HasDefaultValue(false);
            entity.Property(e => e.canedit).HasDefaultValue(false);
            entity.Property(e => e.canview).HasDefaultValue(false);

            entity.HasOne(d => d.permission).WithMany(p => p.RolePermissions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("rolepermission_permissionid_fkey");

            entity.HasOne(d => d.role).WithMany(p => p.RolePermissions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("rolepermission_roleid_fkey");
        });

        modelBuilder.Entity<UserBranch>(entity =>
        {
            entity.HasKey(e => e.userbranchid).HasName("userbranch_pkey");

            entity.Property(e => e.userbranchid).HasDefaultValueSql("nextval('userbranch_userbranchid_seq'::regclass)");

            entity.HasOne(d => d.branch).WithMany(p => p.UserBranches)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("userbranch_branchid_fkey");

            entity.HasOne(d => d.user).WithMany(p => p.UserBranches)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("userbranch_userid_fkey");
        });

        modelBuilder.Entity<EmployeeSalary>(entity =>
        {
            entity.ToTable("EmployeeSalary");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BaseSalary).HasPrecision(18, 2);
            entity.Property(e => e.BankAccountNo).HasMaxLength(50);
            entity.Property(e => e.PaymentMode).HasMaxLength(50);
        });

        modelBuilder.Entity<EmployeeSalaryComponent>(entity =>
        {
            entity.ToTable("EmployeeSalaryComponent");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasColumnName("Amount").HasPrecision(18, 2);
        });

        modelBuilder.Entity<Payslip>(entity =>
        {
            entity.ToTable("Payslip");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BaseSalary).HasPrecision(18, 2);
            entity.Property(e => e.TotalAllowances).HasPrecision(18, 2);
            entity.Property(e => e.OvertimeHours).HasPrecision(5, 2);
            entity.Property(e => e.OvertimePay).HasPrecision(18, 2);
            entity.Property(e => e.GrossEarnings).HasPrecision(18, 2);
            entity.Property(e => e.NoPayDays).HasPrecision(5, 2);
            entity.Property(e => e.NoPayDeduction).HasPrecision(18, 2);
            entity.Property(e => e.GrossEpfSalary).HasPrecision(18, 2);
            entity.Property(e => e.EmployeeEpf).HasPrecision(18, 2);
            entity.Property(e => e.EmployerEpf).HasPrecision(18, 2);
            entity.Property(e => e.EmployerEtf).HasPrecision(18, 2);
            entity.Property(e => e.ApitTax).HasPrecision(18, 2);
            entity.Property(e => e.OtherDeductions).HasPrecision(18, 2);
            entity.Property(e => e.TotalDeductions).HasPrecision(18, 2);
            entity.Property(e => e.NetSalary).HasPrecision(18, 2);
            entity.Property(e => e.PaymentStatus).HasMaxLength(30);
        });

        modelBuilder.Entity<PayslipItem>(entity =>
        {
            entity.ToTable("PayslipItem");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasColumnName("Amount").HasPrecision(18, 2);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.ComponentName).HasMaxLength(100);
            entity.Property(e => e.Code).HasMaxLength(100);
        });

        modelBuilder.Entity<SalaryComponent>(entity =>
        {
            entity.ToTable("SalaryComponent");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Code).HasMaxLength(30);
        });

        modelBuilder.Entity<SalaryStructure>(entity =>
        {
            entity.ToTable("SalaryStructure");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<SalaryStructureComponent>(entity =>
        {
            entity.ToTable("SalaryStructureComponent");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DefaultAmount).HasPrecision(18, 2);
            entity.Property(e => e.PercentageOfBasic).HasPrecision(5, 2);
        });

        modelBuilder.Entity<PayrollPeriod>(entity =>
        {
            entity.ToTable("PayrollPeriod");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.PeriodName).HasMaxLength(100);
        });

        modelBuilder.Entity<PayrollRun>(entity =>
        {
            entity.ToTable("PayrollRun");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TotalGross).HasPrecision(18, 2);
            entity.Property(e => e.TotalNet).HasPrecision(18, 2);
            entity.Property(e => e.TotalEmployerCost).HasPrecision(18, 2);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
