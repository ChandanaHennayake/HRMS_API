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

-- 0. Employee and AppUser Attributes
ALTER TABLE IF EXISTS ""Employee"" ADD COLUMN IF NOT EXISTS ""BankName"" character varying(100);
ALTER TABLE IF EXISTS ""Employee"" ADD COLUMN IF NOT EXISTS ""BankAccountNo"" character varying(50);
ALTER TABLE IF EXISTS ""Employee"" ADD COLUMN IF NOT EXISTS ""BankBranch"" character varying(100);
ALTER TABLE IF EXISTS ""Employee"" ADD COLUMN IF NOT EXISTS ""Qualification"" character varying(255);

ALTER TABLE IF EXISTS ""AppUser"" ADD COLUMN IF NOT EXISTS ""ClearPassword"" text;
ALTER TABLE IF EXISTS ""AppUser"" ADD COLUMN IF NOT EXISTS ""IsDeleted"" boolean DEFAULT false;

-- 1. Department / Designation / EmploymentType Aliases
ALTER TABLE IF EXISTS ""Department"" ADD COLUMN IF NOT EXISTS ""DepartmentName"" character varying(100);
ALTER TABLE IF EXISTS ""Department"" ADD COLUMN IF NOT EXISTS ""DepartmentCode"" character varying(50);
ALTER TABLE IF EXISTS ""Department"" ADD COLUMN IF NOT EXISTS ""Name"" character varying(100);
ALTER TABLE IF EXISTS ""Department"" ADD COLUMN IF NOT EXISTS ""Code"" character varying(50);
DO $$ 
BEGIN 
    UPDATE ""Department"" SET ""DepartmentName"" = COALESCE(""DepartmentName"", ""Name"");
    UPDATE ""Department"" SET ""DepartmentCode"" = COALESCE(""DepartmentCode"", ""Code"");
    UPDATE ""Department"" SET ""Name"" = COALESCE(""Name"", ""DepartmentName"");
    UPDATE ""Department"" SET ""Code"" = COALESCE(""Code"", ""DepartmentCode"");
END $$;

ALTER TABLE IF EXISTS ""Designation"" ADD COLUMN IF NOT EXISTS ""DesignationName"" character varying(100);
ALTER TABLE IF EXISTS ""Designation"" ADD COLUMN IF NOT EXISTS ""DesignationCode"" character varying(50);
ALTER TABLE IF EXISTS ""Designation"" ADD COLUMN IF NOT EXISTS ""Name"" character varying(100);
ALTER TABLE IF EXISTS ""Designation"" ADD COLUMN IF NOT EXISTS ""Code"" character varying(50);
DO $$ 
BEGIN 
    UPDATE ""Designation"" SET ""DesignationName"" = COALESCE(""DesignationName"", ""Name"");
    UPDATE ""Designation"" SET ""DesignationCode"" = COALESCE(""DesignationCode"", ""Code"");
    UPDATE ""Designation"" SET ""Name"" = COALESCE(""Name"", ""DesignationName"");
    UPDATE ""Designation"" SET ""Code"" = COALESCE(""Code"", ""DesignationCode"");
END $$;

ALTER TABLE IF EXISTS ""EmploymentType"" ADD COLUMN IF NOT EXISTS ""TypeName"" character varying(100);
ALTER TABLE IF EXISTS ""EmploymentType"" ADD COLUMN IF NOT EXISTS ""TypeCode"" character varying(50);
ALTER TABLE IF EXISTS ""EmploymentType"" ADD COLUMN IF NOT EXISTS ""Name"" character varying(100);
ALTER TABLE IF EXISTS ""EmploymentType"" ADD COLUMN IF NOT EXISTS ""Code"" character varying(50);
DO $$ 
BEGIN 
    UPDATE ""EmploymentType"" SET ""TypeName"" = COALESCE(""TypeName"", ""Name"");
    UPDATE ""EmploymentType"" SET ""TypeCode"" = COALESCE(""TypeCode"", ""Code"");
    UPDATE ""EmploymentType"" SET ""Name"" = COALESCE(""Name"", ""TypeName"");
    UPDATE ""EmploymentType"" SET ""Code"" = COALESCE(""Code"", ""TypeCode"");
END $$;


-- 2. EmployeeSalary Bank Details
ALTER TABLE ""EmployeeSalary"" ADD COLUMN IF NOT EXISTS ""BankAccountNo"" character varying(50);
ALTER TABLE ""EmployeeSalary"" ADD COLUMN IF NOT EXISTS ""PaymentMode"" character varying(50) DEFAULT 'Bank Transfer';

-- 3. PayrollPeriod Schema Reconciliation
ALTER TABLE IF EXISTS ""PayrollPeriod"" ADD COLUMN IF NOT EXISTS ""Year"" integer;
ALTER TABLE IF EXISTS ""PayrollPeriod"" ADD COLUMN IF NOT EXISTS ""Month"" integer;
ALTER TABLE IF EXISTS ""PayrollPeriod"" ADD COLUMN IF NOT EXISTS ""Name"" character varying(100);
ALTER TABLE IF EXISTS ""PayrollPeriod"" ADD COLUMN IF NOT EXISTS ""PeriodName"" character varying(100);
ALTER TABLE IF EXISTS ""PayrollPeriod"" ADD COLUMN IF NOT EXISTS ""IsClosed"" boolean DEFAULT false;
ALTER TABLE IF EXISTS ""PayrollPeriod"" ADD COLUMN IF NOT EXISTS ""CutoffDate"" date;
ALTER TABLE IF EXISTS ""PayrollPeriod"" ADD COLUMN IF NOT EXISTS ""Status"" smallint DEFAULT 0;
ALTER TABLE IF EXISTS ""PayrollPeriod"" ADD COLUMN IF NOT EXISTS ""CreatedBy"" bigint;
DO $$ 
BEGIN 
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'PayrollPeriod' AND column_name = 'PeriodName') THEN
        ALTER TABLE ""PayrollPeriod"" ALTER COLUMN ""PeriodName"" DROP NOT NULL;
        ALTER TABLE ""PayrollPeriod"" ALTER COLUMN ""PeriodName"" SET DEFAULT '';
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'PayrollPeriod' AND column_name = 'Name') THEN
        ALTER TABLE ""PayrollPeriod"" ALTER COLUMN ""Name"" DROP NOT NULL;
        ALTER TABLE ""PayrollPeriod"" ALTER COLUMN ""Name"" SET DEFAULT '';
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'PayrollPeriod' AND column_name = 'CutoffDate') THEN
        ALTER TABLE ""PayrollPeriod"" ALTER COLUMN ""CutoffDate"" DROP NOT NULL;
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'PayrollPeriod' AND column_name = 'Status') THEN
        ALTER TABLE ""PayrollPeriod"" ALTER COLUMN ""Status"" DROP NOT NULL;
        ALTER TABLE ""PayrollPeriod"" ALTER COLUMN ""Status"" SET DEFAULT 0;
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'PayrollPeriod' AND column_name = 'CreatedBy') THEN
        ALTER TABLE ""PayrollPeriod"" ALTER COLUMN ""CreatedBy"" DROP NOT NULL;
    END IF;
    UPDATE ""PayrollPeriod"" SET ""Name"" = COALESCE(""Name"", ""PeriodName"", 'Period');
    UPDATE ""PayrollPeriod"" SET ""PeriodName"" = COALESCE(""PeriodName"", ""Name"", 'Period');
    UPDATE ""PayrollPeriod"" SET ""Year"" = EXTRACT(YEAR FROM ""StartDate"") WHERE ""Year"" IS NULL AND ""StartDate"" IS NOT NULL;
    UPDATE ""PayrollPeriod"" SET ""Month"" = EXTRACT(MONTH FROM ""StartDate"") WHERE ""Month"" IS NULL AND ""StartDate"" IS NOT NULL;
    UPDATE ""PayrollPeriod"" SET ""CutoffDate"" = COALESCE(""CutoffDate"", ""EndDate"") WHERE ""CutoffDate"" IS NULL;
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
        ALTER TABLE ""PayrollRun"" ALTER COLUMN ""TotalEmployees"" DROP NOT NULL;
        ALTER TABLE ""PayrollRun"" ALTER COLUMN ""TotalEmployees"" SET DEFAULT 0;
        UPDATE ""PayrollRun"" SET ""ProcessedCount"" = ""TotalEmployees"" WHERE ""ProcessedCount"" = 0 AND ""TotalEmployees"" > 0;
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'PayrollRun' AND column_name = 'TotalDeductions') THEN
        ALTER TABLE ""PayrollRun"" ALTER COLUMN ""TotalDeductions"" DROP NOT NULL;
        ALTER TABLE ""PayrollRun"" ALTER COLUMN ""TotalDeductions"" SET DEFAULT 0.0;
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'PayrollRun' AND column_name = 'Notes') THEN
        ALTER TABLE ""PayrollRun"" ALTER COLUMN ""Notes"" DROP NOT NULL;
        UPDATE ""PayrollRun"" SET ""Comments"" = ""Notes"" WHERE ""Comments"" IS NULL;
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'PayrollRun' AND column_name = 'RunDate') THEN
        ALTER TABLE ""PayrollRun"" ALTER COLUMN ""RunDate"" DROP NOT NULL;
        ALTER TABLE ""PayrollRun"" ALTER COLUMN ""RunDate"" SET DEFAULT CURRENT_TIMESTAMP;
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
        ALTER TABLE ""Payslip"" ALTER COLUMN ""GrossSalary"" DROP NOT NULL;
        ALTER TABLE ""Payslip"" ALTER COLUMN ""GrossSalary"" SET DEFAULT 0.0;
        UPDATE ""Payslip"" SET ""GrossEarnings"" = ""GrossSalary"" WHERE ""GrossEarnings"" = 0 AND ""GrossSalary"" > 0;
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Payslip' AND column_name = 'Status') THEN
        ALTER TABLE ""Payslip"" ALTER COLUMN ""Status"" DROP NOT NULL;
        ALTER TABLE ""Payslip"" ALTER COLUMN ""Status"" SET DEFAULT 0;
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Payslip' AND column_name = 'GeneratedAt') THEN
        ALTER TABLE ""Payslip"" ALTER COLUMN ""GeneratedAt"" DROP NOT NULL;
        ALTER TABLE ""Payslip"" ALTER COLUMN ""GeneratedAt"" SET DEFAULT CURRENT_TIMESTAMP;
        UPDATE ""Payslip"" SET ""CreatedAt"" = ""GeneratedAt"" WHERE ""CreatedAt"" IS NULL;
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Payslip' AND column_name = 'PaymentDate') THEN
        ALTER TABLE ""Payslip"" ALTER COLUMN ""PaymentDate"" DROP NOT NULL;
    END IF;
END $$;

-- Populate missing Payslip.PeriodId from PayrollRun if null
UPDATE ""Payslip"" p
SET ""PeriodId"" = r.""PayrollPeriodId""
FROM ""PayrollRun"" r
WHERE p.""PayrollRunId"" = r.""Id"" AND (p.""PeriodId"" IS NULL OR p.""PeriodId"" = 0);

-- 6. PayslipItem Schema Reconciliation
ALTER TABLE IF EXISTS ""PayslipItem"" ADD COLUMN IF NOT EXISTS ""PayslipId"" bigint;
ALTER TABLE IF EXISTS ""PayslipItem"" ADD COLUMN IF NOT EXISTS ""Name"" character varying(100);
ALTER TABLE IF EXISTS ""PayslipItem"" ADD COLUMN IF NOT EXISTS ""ComponentName"" character varying(100);
ALTER TABLE IF EXISTS ""PayslipItem"" ADD COLUMN IF NOT EXISTS ""Code"" character varying(100) DEFAULT '';
ALTER TABLE IF EXISTS ""PayslipItem"" ALTER COLUMN ""Code"" TYPE character varying(100);
ALTER TABLE IF EXISTS ""PayslipItem"" ADD COLUMN IF NOT EXISTS ""ItemType"" smallint DEFAULT 1;
ALTER TABLE IF EXISTS ""PayslipItem"" ADD COLUMN IF NOT EXISTS ""Type"" smallint DEFAULT 1;
ALTER TABLE IF EXISTS ""PayslipItem"" ADD COLUMN IF NOT EXISTS ""Amount"" numeric(18,2) DEFAULT 0.0;
ALTER TABLE IF EXISTS ""PayslipItem"" ADD COLUMN IF NOT EXISTS ""IsEpfApplicable"" boolean DEFAULT false;
ALTER TABLE IF EXISTS ""PayslipItem"" ADD COLUMN IF NOT EXISTS ""IsTaxable"" boolean DEFAULT false;
DO $$ 
BEGIN 
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'PayslipItem' AND column_name = 'amount') THEN
        UPDATE ""PayslipItem"" SET ""Amount"" = ""amount"" WHERE (""Amount"" IS NULL OR ""Amount"" = 0) AND ""amount"" IS NOT NULL;
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'PayslipItem' AND column_name = 'ComponentName') THEN
        ALTER TABLE ""PayslipItem"" ALTER COLUMN ""ComponentName"" DROP NOT NULL;
        ALTER TABLE ""PayslipItem"" ALTER COLUMN ""ComponentName"" SET DEFAULT '';
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'PayslipItem' AND column_name = 'Name') THEN
        ALTER TABLE ""PayslipItem"" ALTER COLUMN ""Name"" DROP NOT NULL;
        ALTER TABLE ""PayslipItem"" ALTER COLUMN ""Name"" SET DEFAULT '';
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'PayslipItem' AND column_name = 'Code') THEN
        ALTER TABLE ""PayslipItem"" ALTER COLUMN ""Code"" DROP NOT NULL;
        ALTER TABLE ""PayslipItem"" ALTER COLUMN ""Code"" SET DEFAULT '';
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'PayslipItem' AND column_name = 'ItemType') THEN
        ALTER TABLE ""PayslipItem"" ALTER COLUMN ""ItemType"" DROP NOT NULL;
        ALTER TABLE ""PayslipItem"" ALTER COLUMN ""ItemType"" SET DEFAULT 1;
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'PayslipItem' AND column_name = 'Type') THEN
        ALTER TABLE ""PayslipItem"" ALTER COLUMN ""Type"" DROP NOT NULL;
        ALTER TABLE ""PayslipItem"" ALTER COLUMN ""Type"" SET DEFAULT 1;
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'PayslipItem' AND column_name = 'SalaryComponentId') THEN
        ALTER TABLE ""PayslipItem"" ALTER COLUMN ""SalaryComponentId"" DROP NOT NULL;
    END IF;
    UPDATE ""PayslipItem"" SET ""Name"" = COALESCE(""Name"", ""ComponentName"", 'Item');
    UPDATE ""PayslipItem"" SET ""ComponentName"" = COALESCE(""ComponentName"", ""Name"", 'Item');
    UPDATE ""PayslipItem"" SET ""ItemType"" = COALESCE(""ItemType"", ""Type"", 1);
    UPDATE ""PayslipItem"" SET ""Type"" = COALESCE(""Type"", ""ItemType"", 1);
END $$;

-- 7. EmployeeSalary & EmployeeSalaryComponent Schema Reconciliation
CREATE TABLE IF NOT EXISTS ""EmployeeSalaryComponent"" (
    ""Id"" BIGSERIAL PRIMARY KEY,
    ""EmployeeSalaryId"" BIGINT NOT NULL,
    ""SalaryComponentId"" BIGINT NOT NULL,
    ""Amount"" NUMERIC(18,2) NOT NULL DEFAULT 0.0
);

ALTER TABLE ""EmployeeSalaryComponent"" ADD COLUMN IF NOT EXISTS ""EmployeeSalaryId"" bigint;
ALTER TABLE ""EmployeeSalaryComponent"" ADD COLUMN IF NOT EXISTS ""SalaryComponentId"" bigint;
ALTER TABLE ""EmployeeSalaryComponent"" ADD COLUMN IF NOT EXISTS ""Amount"" numeric(18,2) DEFAULT 0.0;
DO $$ 
BEGIN 
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'EmployeeSalaryComponent' AND column_name = 'amount') THEN
        UPDATE ""EmployeeSalaryComponent"" SET ""Amount"" = ""amount"" WHERE (""Amount"" IS NULL OR ""Amount"" = 0) AND ""amount"" IS NOT NULL;
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'EmployeeSalaryComponent' AND column_name = 'DefaultAmount') THEN
        UPDATE ""EmployeeSalaryComponent"" SET ""Amount"" = ""DefaultAmount"" WHERE (""Amount"" IS NULL OR ""Amount"" = 0) AND ""DefaultAmount"" IS NOT NULL;
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'EmployeeSalaryComponent' AND column_name = 'defaultamount') THEN
        UPDATE ""EmployeeSalaryComponent"" SET ""Amount"" = ""defaultamount"" WHERE (""Amount"" IS NULL OR ""Amount"" = 0) AND ""defaultamount"" IS NOT NULL;
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'EmployeeSalaryComponent' AND column_name = 'Value') THEN
        UPDATE ""EmployeeSalaryComponent"" SET ""Amount"" = ""Value"" WHERE (""Amount"" IS NULL OR ""Amount"" = 0) AND ""Value"" IS NOT NULL;
    END IF;
END $$;

-- 8. SalaryComponent Schema Reconciliation
CREATE TABLE IF NOT EXISTS ""SalaryComponent"" (
    ""Id"" BIGSERIAL PRIMARY KEY,
    ""CompanyId"" BIGINT NOT NULL DEFAULT 1,
    ""Name"" VARCHAR(100) NOT NULL,
    ""Code"" VARCHAR(30) NOT NULL,
    ""ComponentType"" SMALLINT NOT NULL DEFAULT 1,
    ""IsEpfApplicable"" BOOLEAN NOT NULL DEFAULT FALSE,
    ""IsTaxable"" BOOLEAN NOT NULL DEFAULT TRUE,
    ""IsFixed"" BOOLEAN NOT NULL DEFAULT TRUE,
    ""IsActive"" BOOLEAN NOT NULL DEFAULT TRUE,
    ""CreatedAt"" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW()
);

ALTER TABLE ""SalaryComponent"" ADD COLUMN IF NOT EXISTS ""CompanyId"" bigint DEFAULT 1;
ALTER TABLE ""SalaryComponent"" ADD COLUMN IF NOT EXISTS ""Name"" character varying(100);
ALTER TABLE ""SalaryComponent"" ADD COLUMN IF NOT EXISTS ""Code"" character varying(30);
ALTER TABLE ""SalaryComponent"" ADD COLUMN IF NOT EXISTS ""ComponentType"" smallint DEFAULT 1;
ALTER TABLE ""SalaryComponent"" ADD COLUMN IF NOT EXISTS ""IsEpfApplicable"" boolean DEFAULT false;
ALTER TABLE ""SalaryComponent"" ADD COLUMN IF NOT EXISTS ""IsTaxable"" boolean DEFAULT true;
ALTER TABLE ""SalaryComponent"" ADD COLUMN IF NOT EXISTS ""IsFixed"" boolean DEFAULT true;
ALTER TABLE ""SalaryComponent"" ADD COLUMN IF NOT EXISTS ""IsActive"" boolean DEFAULT true;
ALTER TABLE ""SalaryComponent"" ADD COLUMN IF NOT EXISTS ""CreatedAt"" timestamp without time zone DEFAULT NOW();

-- 9. SalaryStructure & SalaryStructureComponent Schema Reconciliation
CREATE TABLE IF NOT EXISTS ""SalaryStructure"" (
    ""Id"" BIGSERIAL PRIMARY KEY,
    ""CompanyId"" BIGINT NOT NULL DEFAULT 1,
    ""Name"" VARCHAR(100) NOT NULL,
    ""Description"" VARCHAR(255),
    ""IsActive"" BOOLEAN NOT NULL DEFAULT TRUE,
    ""CreatedAt"" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW()
);

ALTER TABLE ""SalaryStructure"" ADD COLUMN IF NOT EXISTS ""CompanyId"" bigint DEFAULT 1;
ALTER TABLE ""SalaryStructure"" ADD COLUMN IF NOT EXISTS ""Name"" character varying(100);
ALTER TABLE ""SalaryStructure"" ADD COLUMN IF NOT EXISTS ""Description"" character varying(255);
ALTER TABLE ""SalaryStructure"" ADD COLUMN IF NOT EXISTS ""IsActive"" boolean DEFAULT true;
ALTER TABLE ""SalaryStructure"" ADD COLUMN IF NOT EXISTS ""CreatedAt"" timestamp without time zone DEFAULT NOW();

CREATE TABLE IF NOT EXISTS ""SalaryStructureComponent"" (
    ""Id"" BIGSERIAL PRIMARY KEY,
    ""SalaryStructureId"" BIGINT NOT NULL,
    ""SalaryComponentId"" BIGINT NOT NULL,
    ""DefaultAmount"" NUMERIC(18,2) NOT NULL DEFAULT 0.0,
    ""PercentageOfBasic"" NUMERIC(5,2)
);

ALTER TABLE ""SalaryStructureComponent"" ADD COLUMN IF NOT EXISTS ""SalaryStructureId"" bigint;
ALTER TABLE ""SalaryStructureComponent"" ADD COLUMN IF NOT EXISTS ""SalaryComponentId"" bigint;
ALTER TABLE ""SalaryStructureComponent"" ADD COLUMN IF NOT EXISTS ""DefaultAmount"" numeric(18,2) DEFAULT 0.0;
ALTER TABLE ""SalaryStructureComponent"" ADD COLUMN IF NOT EXISTS ""PercentageOfBasic"" numeric(5,2);
DO $$ 
BEGIN 
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'SalaryStructureComponent' AND column_name = 'Amount') THEN
        UPDATE ""SalaryStructureComponent"" SET ""DefaultAmount"" = ""Amount"" WHERE (""DefaultAmount"" IS NULL OR ""DefaultAmount"" = 0) AND ""Amount"" IS NOT NULL;
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'SalaryStructureComponent' AND column_name = 'amount') THEN
        UPDATE ""SalaryStructureComponent"" SET ""DefaultAmount"" = ""amount"" WHERE (""DefaultAmount"" IS NULL OR ""DefaultAmount"" = 0) AND ""amount"" IS NOT NULL;
    END IF;
END $$;

-- 8. AppVersion Table Reconciliation
CREATE TABLE IF NOT EXISTS ""AppVersion"" (
    ""Id"" SERIAL PRIMARY KEY,
    ""Platform"" VARCHAR(50) NOT NULL DEFAULT 'Android',
    ""LatestVersion"" VARCHAR(50) NOT NULL DEFAULT '1.0.3',
    ""MinimumVersion"" VARCHAR(50) NOT NULL DEFAULT '1.0.3',
    ""ForceUpdate"" BOOLEAN NOT NULL DEFAULT TRUE,
    ""UpdateUrl"" VARCHAR(500),
    ""ReleaseNotes"" VARCHAR(500),
    ""UpdatedAt"" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW()
);

INSERT INTO ""AppVersion"" (""Platform"", ""LatestVersion"", ""MinimumVersion"", ""ForceUpdate"", ""ReleaseNotes"")
SELECT 'Android', '1.0.3', '1.0.3', TRUE, 'Official AlienHR Enterprise v1.0.3 release with payroll management and security enhancements.'
WHERE NOT EXISTS (SELECT 1 FROM ""AppVersion"" WHERE ""Platform"" = 'Android');

INSERT INTO ""AppVersion"" (""Platform"", ""LatestVersion"", ""MinimumVersion"", ""ForceUpdate"", ""ReleaseNotes"")
SELECT 'iOS', '1.0.3', '1.0.3', TRUE, 'Official AlienHR Enterprise v1.0.3 release.'
WHERE NOT EXISTS (SELECT 1 FROM ""AppVersion"" WHERE ""Platform"" = 'iOS');

-- 9. ProfileChangeRequest Table Reconciliation
CREATE TABLE IF NOT EXISTS ""ProfileChangeRequest"" (
    ""Id"" BIGSERIAL PRIMARY KEY,
    ""EmployeeId"" BIGINT NOT NULL,
    ""UserId"" INTEGER,
    ""CompanyId"" BIGINT NOT NULL DEFAULT 1,
    ""BranchId"" BIGINT,
    ""FirstName"" VARCHAR(100),
    ""LastName"" VARCHAR(100),
    ""Phone"" VARCHAR(30),
    ""DateOfBirth"" DATE,
    ""Gender"" VARCHAR(20),
    ""AddressLine1"" VARCHAR(200),
    ""AddressLine2"" VARCHAR(200),
    ""City"" VARCHAR(100),
    ""Qualification"" VARCHAR(255),
    ""BankName"" VARCHAR(100),
    ""BankAccountNo"" VARCHAR(50),
    ""BankBranch"" VARCHAR(100),
    ""CurrentFirstName"" VARCHAR(100),
    ""CurrentLastName"" VARCHAR(100),
    ""CurrentPhone"" VARCHAR(30),
    ""CurrentDateOfBirth"" DATE,
    ""CurrentGender"" VARCHAR(20),
    ""CurrentAddressLine1"" VARCHAR(200),
    ""CurrentAddressLine2"" VARCHAR(200),
    ""CurrentCity"" VARCHAR(100),
    ""CurrentQualification"" VARCHAR(255),
    ""CurrentBankName"" VARCHAR(100),
    ""CurrentBankAccountNo"" VARCHAR(50),
    ""CurrentBankBranch"" VARCHAR(100),
    ""Status"" SMALLINT NOT NULL DEFAULT 0,
    ""Reason"" VARCHAR(500),
    ""ReviewedBy"" BIGINT,
    ""ReviewerName"" VARCHAR(150),
    ""ReviewedAt"" TIMESTAMP WITHOUT TIME ZONE,
    ""ReviewerComment"" VARCHAR(500),
    ""CreatedAt"" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW(),
    ""UpdatedAt"" TIMESTAMP WITHOUT TIME ZONE
);

CREATE INDEX IF NOT EXISTS ""IX_ProfileChangeRequest_EmployeeId"" ON ""ProfileChangeRequest"" (""EmployeeId"");
CREATE INDEX IF NOT EXISTS ""IX_ProfileChangeRequest_Status"" ON ""ProfileChangeRequest"" (""Status"");
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
