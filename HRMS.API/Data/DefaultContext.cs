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

    public virtual DbSet<Branch> Branches { get; set; }

    public virtual DbSet<Company> Companies { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<Designation> Designations { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<EmploymentType> EmploymentTypes { get; set; }

    public virtual DbSet<Holiday> Holidays { get; set; }

    public virtual DbSet<LeaveRequest> LeaveRequests { get; set; }

    public virtual DbSet<LeaveType> LeaveTypes { get; set; }

    public virtual DbSet<LoginHistory> LoginHistories { get; set; }

    public virtual DbSet<PasswordReset> PasswordResets { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<RolePermission> RolePermissions { get; set; }

    public virtual DbSet<UserBranch> UserBranches { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=ABANS_BAN;Username=postgres;Password=Nishantha@123");

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
        });

        modelBuilder.Entity<Designation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Designation_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
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

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
