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

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<branch> branches { get; set; }

    public virtual DbSet<company> companies { get; set; }

    public virtual DbSet<loginhistory> loginhistories { get; set; }

    public virtual DbSet<passwordreset> passwordresets { get; set; }

    public virtual DbSet<permission> permissions { get; set; }

    public virtual DbSet<refreshtoken> refreshtokens { get; set; }

    public virtual DbSet<role> roles { get; set; }

    public virtual DbSet<rolepermission> rolepermissions { get; set; }

    public virtual DbSet<userbranch> userbranches { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=ABANS_BAN;Username=postgres;Password=Nishantha@123");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.userid).HasName("User_pkey");

            entity.Property(e => e.createddate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.failedloginattempts).HasDefaultValue(0);
            entity.Property(e => e.isactive).HasDefaultValue(true);
            entity.Property(e => e.islocked).HasDefaultValue(false);

            entity.HasOne(d => d.branch).WithMany(p => p.Users).HasConstraintName("User_branchid_fkey");

            entity.HasOne(d => d.company).WithMany(p => p.Users)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("User_companyid_fkey");

            entity.HasOne(d => d.role).WithMany(p => p.Users)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("User_roleid_fkey");
        });

        modelBuilder.Entity<branch>(entity =>
        {
            entity.HasKey(e => e.branchid).HasName("branch_pkey");

            entity.Property(e => e.createddate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.isactive).HasDefaultValue(true);

            entity.HasOne(d => d.company).WithMany(p => p.branches)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("branch_companyid_fkey");
        });

        modelBuilder.Entity<company>(entity =>
        {
            entity.HasKey(e => e.companyid).HasName("company_pkey");

            entity.Property(e => e.createddate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.isactive).HasDefaultValue(true);
        });

        modelBuilder.Entity<loginhistory>(entity =>
        {
            entity.HasKey(e => e.loginhistoryid).HasName("loginhistory_pkey");

            entity.Property(e => e.logintime).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.user).WithMany(p => p.loginhistories).HasConstraintName("loginhistory_userid_fkey");
        });

        modelBuilder.Entity<passwordreset>(entity =>
        {
            entity.HasKey(e => e.passwordresetid).HasName("passwordreset_pkey");

            entity.Property(e => e.createddate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.used).HasDefaultValue(false);

            entity.HasOne(d => d.user).WithMany(p => p.passwordresets)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("passwordreset_userid_fkey");
        });

        modelBuilder.Entity<permission>(entity =>
        {
            entity.HasKey(e => e.permissionid).HasName("permission_pkey");
        });

        modelBuilder.Entity<refreshtoken>(entity =>
        {
            entity.HasKey(e => e.refreshtokenid).HasName("refreshtoken_pkey");

            entity.Property(e => e.createddate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.isrevoked).HasDefaultValue(false);

            entity.HasOne(d => d.user).WithMany(p => p.refreshtokens)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("refreshtoken_userid_fkey");
        });

        modelBuilder.Entity<role>(entity =>
        {
            entity.HasKey(e => e.roleid).HasName("role_pkey");

            entity.Property(e => e.createddate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.isactive).HasDefaultValue(true);
            entity.Property(e => e.issystemrole).HasDefaultValue(false);

            entity.HasOne(d => d.company).WithMany(p => p.roles).HasConstraintName("role_companyid_fkey");
        });

        modelBuilder.Entity<rolepermission>(entity =>
        {
            entity.HasKey(e => e.rolepermissionid).HasName("rolepermission_pkey");

            entity.Property(e => e.canapprove).HasDefaultValue(false);
            entity.Property(e => e.cancreate).HasDefaultValue(false);
            entity.Property(e => e.candelete).HasDefaultValue(false);
            entity.Property(e => e.canedit).HasDefaultValue(false);
            entity.Property(e => e.canview).HasDefaultValue(false);

            entity.HasOne(d => d.permission).WithMany(p => p.rolepermissions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("rolepermission_permissionid_fkey");

            entity.HasOne(d => d.role).WithMany(p => p.rolepermissions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("rolepermission_roleid_fkey");
        });

        modelBuilder.Entity<userbranch>(entity =>
        {
            entity.HasKey(e => e.userbranchid).HasName("userbranch_pkey");

            entity.HasOne(d => d.branch).WithMany(p => p.userbranches)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("userbranch_branchid_fkey");

            entity.HasOne(d => d.user).WithMany(p => p.userbranches)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("userbranch_userid_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
