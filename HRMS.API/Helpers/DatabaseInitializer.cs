using System;
using System.Threading.Tasks;
using HRMS.API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HRMS.API.Helpers
{
    public static class DatabaseInitializer
    {
        public static async Task EnsureSchemaUpToDateAsync(DefaultContext context, ILogger logger)
        {
            try
            {
                logger.LogInformation("Checking and reconciling database schema for Payroll and Master entities...");

                var sql = @"
-- ============================================================================
-- AlienHR Schema Auto-Reconciliation
-- ============================================================================

-- 1. Department / Designation / EmploymentType Aliases
ALTER TABLE ""Department"" ADD COLUMN IF NOT EXISTS ""Name"" character varying(100);
ALTER TABLE ""Department"" ADD COLUMN IF NOT EXISTS ""Code"" character varying(50);
DO $$ 
BEGIN 
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Department' AND column_name = 'DepartmentName') THEN
        UPDATE ""Department"" SET ""Name"" = ""DepartmentName"" WHERE ""Name"" IS NULL;
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Department' AND column_name = 'DepartmentCode') THEN
        UPDATE ""Department"" SET ""Code"" = ""DepartmentCode"" WHERE ""Code"" IS NULL;
    END IF;
END $$;

ALTER TABLE ""Designation"" ADD COLUMN IF NOT EXISTS ""Name"" character varying(100);
ALTER TABLE ""Designation"" ADD COLUMN IF NOT EXISTS ""Code"" character varying(50);
DO $$ 
BEGIN 
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Designation' AND column_name = 'DesignationName') THEN
        UPDATE ""Designation"" SET ""Name"" = ""DesignationName"" WHERE ""Name"" IS NULL;
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Designation' AND column_name = 'DesignationCode') THEN
        UPDATE ""Designation"" SET ""Code"" = ""DesignationCode"" WHERE ""Code"" IS NULL;
    END IF;
END $$;

ALTER TABLE ""EmploymentType"" ADD COLUMN IF NOT EXISTS ""Name"" character varying(100);
ALTER TABLE ""EmploymentType"" ADD COLUMN IF NOT EXISTS ""Code"" character varying(50);
DO $$ 
BEGIN 
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'EmploymentType' AND column_name = 'TypeName') THEN
        UPDATE ""EmploymentType"" SET ""Name"" = ""TypeName"" WHERE ""Name"" IS NULL;
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'EmploymentType' AND column_name = 'TypeCode') THEN
        UPDATE ""EmploymentType"" SET ""Code"" = ""TypeCode"" WHERE ""Code"" IS NULL;
    END IF;
END $$;

-- 2. EmployeeSalary Bank Details
ALTER TABLE ""EmployeeSalary"" ADD COLUMN IF NOT EXISTS ""BankAccountNo"" character varying(50);
ALTER TABLE ""EmployeeSalary"" ADD COLUMN IF NOT EXISTS ""PaymentMode"" character varying(50) DEFAULT 'Bank Transfer';

-- 3. PayrollPeriod Schema Reconciliation
ALTER TABLE ""PayrollPeriod"" ADD COLUMN IF NOT EXISTS ""Year"" integer;
ALTER TABLE ""PayrollPeriod"" ADD COLUMN IF NOT EXISTS ""Month"" integer;
ALTER TABLE ""PayrollPeriod"" ADD COLUMN IF NOT EXISTS ""Name"" character varying(100);
ALTER TABLE ""PayrollPeriod"" ADD COLUMN IF NOT EXISTS ""IsClosed"" boolean DEFAULT false;
DO $$ 
BEGIN 
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'PayrollPeriod' AND column_name = 'PeriodName') THEN
        UPDATE ""PayrollPeriod"" SET ""Name"" = ""PeriodName"" WHERE ""Name"" IS NULL;
    END IF;
    UPDATE ""PayrollPeriod"" SET ""Year"" = EXTRACT(YEAR FROM ""StartDate"") WHERE ""Year"" IS NULL AND ""StartDate"" IS NOT NULL;
    UPDATE ""PayrollPeriod"" SET ""Month"" = EXTRACT(MONTH FROM ""StartDate"") WHERE ""Month"" IS NULL AND ""StartDate"" IS NOT NULL;
END $$;

-- 4. PayrollRun Schema Reconciliation
ALTER TABLE ""PayrollRun"" ADD COLUMN IF NOT EXISTS ""BranchId"" integer;
ALTER TABLE ""PayrollRun"" ADD COLUMN IF NOT EXISTS ""TotalEmployerCost"" numeric(18,2) DEFAULT 0.0;
ALTER TABLE ""PayrollRun"" ADD COLUMN IF NOT EXISTS ""ProcessedCount"" integer DEFAULT 0;
ALTER TABLE ""PayrollRun"" ADD COLUMN IF NOT EXISTS ""ProcessedBy"" bigint DEFAULT 1;
ALTER TABLE ""PayrollRun"" ADD COLUMN IF NOT EXISTS ""ProcessedAt"" timestamp without time zone DEFAULT CURRENT_TIMESTAMP;
ALTER TABLE ""PayrollRun"" ADD COLUMN IF NOT EXISTS ""Comments"" character varying(500);
DO $$ 
BEGIN 
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'PayrollRun' AND column_name = 'TotalEmployees') THEN
        UPDATE ""PayrollRun"" SET ""ProcessedCount"" = ""TotalEmployees"" WHERE ""ProcessedCount"" = 0 AND ""TotalEmployees"" > 0;
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'PayrollRun' AND column_name = 'Notes') THEN
        UPDATE ""PayrollRun"" SET ""Comments"" = ""Notes"" WHERE ""Comments"" IS NULL;
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'PayrollRun' AND column_name = 'RunDate') THEN
        UPDATE ""PayrollRun"" SET ""ProcessedAt"" = ""RunDate"" WHERE ""ProcessedAt"" IS NULL;
    END IF;
END $$;

-- 5. Payslip Schema Reconciliation
ALTER TABLE ""Payslip"" ADD COLUMN IF NOT EXISTS ""PeriodId"" bigint;
ALTER TABLE ""Payslip"" ADD COLUMN IF NOT EXISTS ""BaseSalary"" numeric(18,2) DEFAULT 0.0;
ALTER TABLE ""Payslip"" ADD COLUMN IF NOT EXISTS ""TotalAllowances"" numeric(18,2) DEFAULT 0.0;
ALTER TABLE ""Payslip"" ADD COLUMN IF NOT EXISTS ""OvertimeHours"" numeric(5,2) DEFAULT 0.0;
ALTER TABLE ""Payslip"" ADD COLUMN IF NOT EXISTS ""OvertimePay"" numeric(18,2) DEFAULT 0.0;
ALTER TABLE ""Payslip"" ADD COLUMN IF NOT EXISTS ""GrossEarnings"" numeric(18,2) DEFAULT 0.0;
ALTER TABLE ""Payslip"" ADD COLUMN IF NOT EXISTS ""NoPayDays"" numeric(5,2) DEFAULT 0.0;
ALTER TABLE ""Payslip"" ADD COLUMN IF NOT EXISTS ""NoPayDeduction"" numeric(18,2) DEFAULT 0.0;
ALTER TABLE ""Payslip"" ADD COLUMN IF NOT EXISTS ""GrossEpfSalary"" numeric(18,2) DEFAULT 0.0;
ALTER TABLE ""Payslip"" ADD COLUMN IF NOT EXISTS ""EmployeeEpf"" numeric(18,2) DEFAULT 0.0;
ALTER TABLE ""Payslip"" ADD COLUMN IF NOT EXISTS ""EmployerEpf"" numeric(18,2) DEFAULT 0.0;
ALTER TABLE ""Payslip"" ADD COLUMN IF NOT EXISTS ""EmployerEtf"" numeric(18,2) DEFAULT 0.0;
ALTER TABLE ""Payslip"" ADD COLUMN IF NOT EXISTS ""ApitTax"" numeric(18,2) DEFAULT 0.0;
ALTER TABLE ""Payslip"" ADD COLUMN IF NOT EXISTS ""OtherDeductions"" numeric(18,2) DEFAULT 0.0;
ALTER TABLE ""Payslip"" ADD COLUMN IF NOT EXISTS ""PaymentStatus"" character varying(30) DEFAULT 'Unpaid';
ALTER TABLE ""Payslip"" ADD COLUMN IF NOT EXISTS ""CreatedAt"" timestamp without time zone DEFAULT CURRENT_TIMESTAMP;
DO $$ 
BEGIN 
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Payslip' AND column_name = 'GrossSalary') THEN
        UPDATE ""Payslip"" SET ""GrossEarnings"" = ""GrossSalary"" WHERE ""GrossEarnings"" = 0 AND ""GrossSalary"" > 0;
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Payslip' AND column_name = 'GeneratedAt') THEN
        UPDATE ""Payslip"" SET ""CreatedAt"" = ""GeneratedAt"" WHERE ""CreatedAt"" IS NULL;
    END IF;
END $$;

-- Populate missing Payslip.PeriodId from PayrollRun if null
UPDATE ""Payslip"" p
SET ""PeriodId"" = r.""PayrollPeriodId""
FROM ""PayrollRun"" r
WHERE p.""PayrollRunId"" = r.""Id"" AND (p.""PeriodId"" IS NULL OR p.""PeriodId"" = 0);

-- 6. PayslipItem Schema Reconciliation
ALTER TABLE ""PayslipItem"" ADD COLUMN IF NOT EXISTS ""Name"" character varying(100);
ALTER TABLE ""PayslipItem"" ADD COLUMN IF NOT EXISTS ""Code"" character varying(30) DEFAULT '';
ALTER TABLE ""PayslipItem"" ADD COLUMN IF NOT EXISTS ""ItemType"" smallint DEFAULT 1;
ALTER TABLE ""PayslipItem"" ADD COLUMN IF NOT EXISTS ""IsEpfApplicable"" boolean DEFAULT false;
DO $$ 
BEGIN 
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'PayslipItem' AND column_name = 'ComponentName') THEN
        UPDATE ""PayslipItem"" SET ""Name"" = ""ComponentName"" WHERE ""Name"" IS NULL;
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'PayslipItem' AND column_name = 'Type') THEN
        UPDATE ""PayslipItem"" SET ""ItemType"" = ""Type"" WHERE ""ItemType"" IS NULL;
    END IF;
END $$;
";

                await context.Database.ExecuteSqlRawAsync(sql);
                logger.LogInformation("Database schema reconciliation completed successfully.");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Database schema auto-reconciliation encountered a non-fatal warning (proceeding with application launch).");
            }
        }
    }
}
