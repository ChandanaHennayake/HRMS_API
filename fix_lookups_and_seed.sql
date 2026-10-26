-- ============================================================================
-- AlienHR — Fix Column Aliases & Master Data Seed Script for Supabase / PostgreSQL
-- ============================================================================

-- 1. Ensure both column conventions exist for Department, Designation, and EmploymentType
ALTER TABLE "Department" ADD COLUMN IF NOT EXISTS "DepartmentName" character varying(100);
ALTER TABLE "Department" ADD COLUMN IF NOT EXISTS "DepartmentCode" character varying(50);
ALTER TABLE "Department" ADD COLUMN IF NOT EXISTS "Name" character varying(100);
ALTER TABLE "Department" ADD COLUMN IF NOT EXISTS "Code" character varying(50);
UPDATE "Department" SET "DepartmentName" = COALESCE("DepartmentName", "Name");
UPDATE "Department" SET "DepartmentCode" = COALESCE("DepartmentCode", "Code");
UPDATE "Department" SET "Name" = COALESCE("Name", "DepartmentName");
UPDATE "Department" SET "Code" = COALESCE("Code", "DepartmentCode");

ALTER TABLE "Designation" ADD COLUMN IF NOT EXISTS "DesignationName" character varying(100);
ALTER TABLE "Designation" ADD COLUMN IF NOT EXISTS "DesignationCode" character varying(50);
ALTER TABLE "Designation" ADD COLUMN IF NOT EXISTS "Name" character varying(100);
ALTER TABLE "Designation" ADD COLUMN IF NOT EXISTS "Code" character varying(50);
UPDATE "Designation" SET "DesignationName" = COALESCE("DesignationName", "Name");
UPDATE "Designation" SET "DesignationCode" = COALESCE("DesignationCode", "Code");
UPDATE "Designation" SET "Name" = COALESCE("Name", "DesignationName");
UPDATE "Designation" SET "Code" = COALESCE("Code", "DesignationCode");

ALTER TABLE "EmploymentType" ADD COLUMN IF NOT EXISTS "TypeName" character varying(100);
ALTER TABLE "EmploymentType" ADD COLUMN IF NOT EXISTS "TypeCode" character varying(50);
ALTER TABLE "EmploymentType" ADD COLUMN IF NOT EXISTS "Name" character varying(100);
ALTER TABLE "EmploymentType" ADD COLUMN IF NOT EXISTS "Code" character varying(50);
UPDATE "EmploymentType" SET "TypeName" = COALESCE("TypeName", "Name");
UPDATE "EmploymentType" SET "TypeCode" = COALESCE("TypeCode", "Code");
UPDATE "EmploymentType" SET "Name" = COALESCE("Name", "TypeName");
UPDATE "EmploymentType" SET "Code" = COALESCE("Code", "TypeCode");

-- 1b. Ensure EmployeeSalary BankAccountNo and PaymentMode exist
ALTER TABLE "EmployeeSalary" ADD COLUMN IF NOT EXISTS "BankAccountNo" character varying(50);
UPDATE "EmployeeSalary" SET "BankAccountNo" = "BankAccountNumber" WHERE "BankAccountNo" IS NULL AND "BankAccountNumber" IS NOT NULL;
ALTER TABLE "EmployeeSalary" ADD COLUMN IF NOT EXISTS "PaymentMode" character varying(50) DEFAULT 'Bank Transfer';
UPDATE "EmployeeSalary" SET "PaymentMode" = 'Bank Transfer' WHERE "PaymentMode" IS NULL;
ALTER TABLE "EmployeeSalary" ADD COLUMN IF NOT EXISTS "BankAccountNumber" character varying(50);
UPDATE "EmployeeSalary" SET "BankAccountNumber" = "BankAccountNo" WHERE "BankAccountNumber" IS NULL AND "BankAccountNo" IS NOT NULL;
ALTER TABLE "EmployeeSalary" ADD COLUMN IF NOT EXISTS "PaymentMethod" smallint DEFAULT 1;

-- 2. Master Data Seed (Departments)
INSERT INTO "Department" ("Id", "CompanyId", "DepartmentCode", "DepartmentName", "Name", "Code", "Description", "IsActive", "IsDeleted", "CreatedAt")
VALUES
    (1, 1, 'HR', 'Human Resources', 'Human Resources', 'HR', 'HR & People Operations', true, false, NOW()),
    (2, 1, 'ENG', 'Engineering', 'Engineering', 'ENG', 'Software & Product Engineering', true, false, NOW()),
    (3, 1, 'OPS', 'Operations', 'Operations', 'OPS', 'Daily Operations & Logistics', true, false, NOW()),
    (4, 1, 'FIN', 'Finance', 'Finance', 'FIN', 'Finance & Accounting', true, false, NOW())
ON CONFLICT ("Id") DO UPDATE
SET "Name" = EXCLUDED."Name",
    "Code" = EXCLUDED."Code",
    "DepartmentName" = EXCLUDED."DepartmentName",
    "DepartmentCode" = EXCLUDED."DepartmentCode";

-- 3. Master Data Seed (Designations)
INSERT INTO "Designation" ("Id", "CompanyId", "DesignationCode", "DesignationName", "Name", "Code", "Description", "IsActive", "IsDeleted", "CreatedAt")
VALUES
    (1, 1, 'ADM', 'System Administrator', 'System Administrator', 'ADM', 'System Overseer', true, false, NOW()),
    (2, 1, 'MGR', 'Branch Manager', 'Branch Manager', 'MGR', 'Manager & Approver', true, false, NOW()),
    (3, 1, 'ENG', 'Software Engineer', 'Software Engineer', 'ENG', 'Developer', true, false, NOW()),
    (4, 1, 'EXEC', 'HR Executive', 'HR Executive', 'EXEC', 'Human Resources', true, false, NOW())
ON CONFLICT ("Id") DO UPDATE
SET "Name" = EXCLUDED."Name",
    "Code" = EXCLUDED."Code",
    "DesignationName" = EXCLUDED."DesignationName",
    "DesignationCode" = EXCLUDED."DesignationCode";

-- 4. Master Data Seed (Employment Types)
INSERT INTO "EmploymentType" ("Id", "CompanyId", "TypeCode", "TypeName", "Name", "Code", "Description", "IsActive", "IsDeleted", "CreatedAt")
VALUES
    (1, 1, 'FT', 'Full-Time', 'Full-Time', 'FT', 'Full Time Permanent Employment', true, false, NOW()),
    (2, 1, 'PT', 'Part-Time', 'Part-Time', 'PT', 'Part Time Employment', true, false, NOW()),
    (3, 1, 'CT', 'Contract', 'Contract', 'CT', 'Contractual Employment', true, false, NOW()),
    (4, 1, 'IN', 'Intern', 'Intern', 'IN', 'Internship Program', true, false, NOW())
ON CONFLICT ("Id") DO UPDATE
SET "Name" = EXCLUDED."Name",
    "TypeName" = EXCLUDED."TypeName",
    "TypeCode" = EXCLUDED."TypeCode";

-- 5. Master Data Seed (Roles)
INSERT INTO "Roles" (roleid, companyid, rolename, description, isactive, issystemrole, createddate)
VALUES
    (1, 1, 'Admin', 'Full administrative authority and system configuration', true, true, NOW()),
    (2, 1, 'Manager', 'Branch/team supervisory authority and leave approvals', true, true, NOW()),
    (4, 1, 'Employee', 'Self-service attendance check-in, leave requests, and profile access', true, true, NOW())
ON CONFLICT (roleid) DO NOTHING;

-- 6. Ensure Sequences are updated
SELECT setval(pg_get_serial_sequence('"Department"', 'Id'), COALESCE(MAX("Id"), 1), true) FROM "Department";
SELECT setval(pg_get_serial_sequence('"Designation"', 'Id'), COALESCE(MAX("Id"), 1), true) FROM "Designation";
SELECT setval(pg_get_serial_sequence('"EmploymentType"', 'Id'), COALESCE(MAX("Id"), 1), true) FROM "EmploymentType";

-- ============================================================================
-- 7. Payroll Schema Reconciliation (PayrollPeriod, PayrollRun, Payslip, PayslipItem)
-- ============================================================================

-- PayrollPeriod
ALTER TABLE "PayrollPeriod" ADD COLUMN IF NOT EXISTS "Year" integer;
ALTER TABLE "PayrollPeriod" ADD COLUMN IF NOT EXISTS "Month" integer;
ALTER TABLE "PayrollPeriod" ADD COLUMN IF NOT EXISTS "Name" character varying(100);
ALTER TABLE "PayrollPeriod" ADD COLUMN IF NOT EXISTS "PeriodName" character varying(100);
ALTER TABLE "PayrollPeriod" ADD COLUMN IF NOT EXISTS "IsClosed" boolean DEFAULT false;
ALTER TABLE "PayrollPeriod" ADD COLUMN IF NOT EXISTS "CutoffDate" date;
ALTER TABLE "PayrollPeriod" ADD COLUMN IF NOT EXISTS "Status" smallint DEFAULT 0;
ALTER TABLE "PayrollPeriod" ADD COLUMN IF NOT EXISTS "CreatedBy" bigint;
DO $$ 
BEGIN 
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'PayrollPeriod' AND column_name = 'PeriodName') THEN
        ALTER TABLE "PayrollPeriod" ALTER COLUMN "PeriodName" DROP NOT NULL;
        ALTER TABLE "PayrollPeriod" ALTER COLUMN "PeriodName" SET DEFAULT '';
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'PayrollPeriod' AND column_name = 'Name') THEN
        ALTER TABLE "PayrollPeriod" ALTER COLUMN "Name" DROP NOT NULL;
        ALTER TABLE "PayrollPeriod" ALTER COLUMN "Name" SET DEFAULT '';
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'PayrollPeriod' AND column_name = 'CutoffDate') THEN
        ALTER TABLE "PayrollPeriod" ALTER COLUMN "CutoffDate" DROP NOT NULL;
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'PayrollPeriod' AND column_name = 'Status') THEN
        ALTER TABLE "PayrollPeriod" ALTER COLUMN "Status" DROP NOT NULL;
        ALTER TABLE "PayrollPeriod" ALTER COLUMN "Status" SET DEFAULT 0;
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'PayrollPeriod' AND column_name = 'CreatedBy') THEN
        ALTER TABLE "PayrollPeriod" ALTER COLUMN "CreatedBy" DROP NOT NULL;
    END IF;
    UPDATE "PayrollPeriod" SET "Name" = COALESCE("Name", "PeriodName", 'Period');
    UPDATE "PayrollPeriod" SET "PeriodName" = COALESCE("PeriodName", "Name", 'Period');
    UPDATE "PayrollPeriod" SET "Year" = EXTRACT(YEAR FROM "StartDate") WHERE "Year" IS NULL AND "StartDate" IS NOT NULL;
    UPDATE "PayrollPeriod" SET "Month" = EXTRACT(MONTH FROM "StartDate") WHERE "Month" IS NULL AND "StartDate" IS NOT NULL;
    UPDATE "PayrollPeriod" SET "CutoffDate" = COALESCE("CutoffDate", "EndDate") WHERE "CutoffDate" IS NULL;
END $$;

-- PayrollRun
ALTER TABLE "PayrollRun" ADD COLUMN IF NOT EXISTS "BranchId" integer;
ALTER TABLE "PayrollRun" ADD COLUMN IF NOT EXISTS "TotalEmployerCost" numeric(18,2) DEFAULT 0.0;
ALTER TABLE "PayrollRun" ADD COLUMN IF NOT EXISTS "ProcessedCount" integer DEFAULT 0;
ALTER TABLE "PayrollRun" ADD COLUMN IF NOT EXISTS "ProcessedBy" bigint DEFAULT 1;
ALTER TABLE "PayrollRun" ADD COLUMN IF NOT EXISTS "ProcessedAt" timestamp without time zone DEFAULT CURRENT_TIMESTAMP;
ALTER TABLE "PayrollRun" ADD COLUMN IF NOT EXISTS "Comments" character varying(500);
DO $$ 
BEGIN 
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'PayrollRun' AND column_name = 'TotalEmployees') THEN
        ALTER TABLE "PayrollRun" ALTER COLUMN "TotalEmployees" DROP NOT NULL;
        ALTER TABLE "PayrollRun" ALTER COLUMN "TotalEmployees" SET DEFAULT 0;
        UPDATE "PayrollRun" SET "ProcessedCount" = "TotalEmployees" WHERE "ProcessedCount" = 0 AND "TotalEmployees" > 0;
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'PayrollRun' AND column_name = 'TotalDeductions') THEN
        ALTER TABLE "PayrollRun" ALTER COLUMN "TotalDeductions" DROP NOT NULL;
        ALTER TABLE "PayrollRun" ALTER COLUMN "TotalDeductions" SET DEFAULT 0.0;
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'PayrollRun' AND column_name = 'Notes') THEN
        ALTER TABLE "PayrollRun" ALTER COLUMN "Notes" DROP NOT NULL;
        UPDATE "PayrollRun" SET "Comments" = "Notes" WHERE "Comments" IS NULL;
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'PayrollRun' AND column_name = 'RunDate') THEN
        ALTER TABLE "PayrollRun" ALTER COLUMN "RunDate" DROP NOT NULL;
        ALTER TABLE "PayrollRun" ALTER COLUMN "RunDate" SET DEFAULT CURRENT_TIMESTAMP;
        UPDATE "PayrollRun" SET "ProcessedAt" = "RunDate" WHERE "ProcessedAt" IS NULL;
    END IF;
END $$;

-- Payslip
ALTER TABLE "Payslip" ADD COLUMN IF NOT EXISTS "PeriodId" bigint;
ALTER TABLE "Payslip" ADD COLUMN IF NOT EXISTS "BaseSalary" numeric(18,2) DEFAULT 0.0;
ALTER TABLE "Payslip" ADD COLUMN IF NOT EXISTS "TotalAllowances" numeric(18,2) DEFAULT 0.0;
ALTER TABLE "Payslip" ADD COLUMN IF NOT EXISTS "OvertimeHours" numeric(5,2) DEFAULT 0.0;
ALTER TABLE "Payslip" ADD COLUMN IF NOT EXISTS "OvertimePay" numeric(18,2) DEFAULT 0.0;
ALTER TABLE "Payslip" ADD COLUMN IF NOT EXISTS "GrossEarnings" numeric(18,2) DEFAULT 0.0;
ALTER TABLE "Payslip" ADD COLUMN IF NOT EXISTS "NoPayDays" numeric(5,2) DEFAULT 0.0;
ALTER TABLE "Payslip" ADD COLUMN IF NOT EXISTS "NoPayDeduction" numeric(18,2) DEFAULT 0.0;
ALTER TABLE "Payslip" ADD COLUMN IF NOT EXISTS "GrossEpfSalary" numeric(18,2) DEFAULT 0.0;
ALTER TABLE "Payslip" ADD COLUMN IF NOT EXISTS "EmployeeEpf" numeric(18,2) DEFAULT 0.0;
ALTER TABLE "Payslip" ADD COLUMN IF NOT EXISTS "EmployerEpf" numeric(18,2) DEFAULT 0.0;
ALTER TABLE "Payslip" ADD COLUMN IF NOT EXISTS "EmployerEtf" numeric(18,2) DEFAULT 0.0;
ALTER TABLE "Payslip" ADD COLUMN IF NOT EXISTS "ApitTax" numeric(18,2) DEFAULT 0.0;
ALTER TABLE "Payslip" ADD COLUMN IF NOT EXISTS "OtherDeductions" numeric(18,2) DEFAULT 0.0;
ALTER TABLE "Payslip" ADD COLUMN IF NOT EXISTS "PaymentStatus" character varying(30) DEFAULT 'Unpaid';
ALTER TABLE "Payslip" ADD COLUMN IF NOT EXISTS "CreatedAt" timestamp without time zone DEFAULT CURRENT_TIMESTAMP;
DO $$ 
BEGIN 
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Payslip' AND column_name = 'GrossSalary') THEN
        ALTER TABLE "Payslip" ALTER COLUMN "GrossSalary" DROP NOT NULL;
        ALTER TABLE "Payslip" ALTER COLUMN "GrossSalary" SET DEFAULT 0.0;
        UPDATE "Payslip" SET "GrossEarnings" = "GrossSalary" WHERE "GrossEarnings" = 0 AND "GrossSalary" > 0;
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Payslip' AND column_name = 'Status') THEN
        ALTER TABLE "Payslip" ALTER COLUMN "Status" DROP NOT NULL;
        ALTER TABLE "Payslip" ALTER COLUMN "Status" SET DEFAULT 0;
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Payslip' AND column_name = 'GeneratedAt') THEN
        ALTER TABLE "Payslip" ALTER COLUMN "GeneratedAt" DROP NOT NULL;
        ALTER TABLE "Payslip" ALTER COLUMN "GeneratedAt" SET DEFAULT CURRENT_TIMESTAMP;
        UPDATE "Payslip" SET "CreatedAt" = "GeneratedAt" WHERE "CreatedAt" IS NULL;
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Payslip' AND column_name = 'PaymentDate') THEN
        ALTER TABLE "Payslip" ALTER COLUMN "PaymentDate" DROP NOT NULL;
    END IF;
END $$;

-- Populate Payslip.PeriodId from PayrollRun
UPDATE "Payslip" p
SET "PeriodId" = r."PayrollPeriodId"
FROM "PayrollRun" r
WHERE p."PayrollRunId" = r."Id" AND (p."PeriodId" IS NULL OR p."PeriodId" = 0);

-- PayslipItem
ALTER TABLE "PayslipItem" ADD COLUMN IF NOT EXISTS "Name" character varying(100);
ALTER TABLE "PayslipItem" ADD COLUMN IF NOT EXISTS "ComponentName" character varying(100);
ALTER TABLE "PayslipItem" ADD COLUMN IF NOT EXISTS "Code" character varying(100) DEFAULT '';
ALTER TABLE "PayslipItem" ALTER COLUMN "Code" TYPE character varying(100);
ALTER TABLE "PayslipItem" ADD COLUMN IF NOT EXISTS "ItemType" smallint DEFAULT 1;
ALTER TABLE "PayslipItem" ADD COLUMN IF NOT EXISTS "Type" smallint DEFAULT 1;
ALTER TABLE "PayslipItem" ADD COLUMN IF NOT EXISTS "Amount" numeric(18,2) DEFAULT 0.0;
ALTER TABLE "PayslipItem" ADD COLUMN IF NOT EXISTS "IsEpfApplicable" boolean DEFAULT false;
ALTER TABLE "PayslipItem" ADD COLUMN IF NOT EXISTS "IsTaxable" boolean DEFAULT false;
DO $$ 
BEGIN 
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'PayslipItem' AND column_name = 'amount') THEN
        UPDATE "PayslipItem" SET "Amount" = "amount" WHERE ("Amount" IS NULL OR "Amount" = 0) AND "amount" IS NOT NULL;
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'PayslipItem' AND column_name = 'ComponentName') THEN
        ALTER TABLE "PayslipItem" ALTER COLUMN "ComponentName" DROP NOT NULL;
        ALTER TABLE "PayslipItem" ALTER COLUMN "ComponentName" SET DEFAULT '';
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'PayslipItem' AND column_name = 'Name') THEN
        ALTER TABLE "PayslipItem" ALTER COLUMN "Name" DROP NOT NULL;
        ALTER TABLE "PayslipItem" ALTER COLUMN "Name" SET DEFAULT '';
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'PayslipItem' AND column_name = 'Code') THEN
        ALTER TABLE "PayslipItem" ALTER COLUMN "Code" DROP NOT NULL;
        ALTER TABLE "PayslipItem" ALTER COLUMN "Code" SET DEFAULT '';
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'PayslipItem' AND column_name = 'ItemType') THEN
        ALTER TABLE "PayslipItem" ALTER COLUMN "ItemType" DROP NOT NULL;
        ALTER TABLE "PayslipItem" ALTER COLUMN "ItemType" SET DEFAULT 1;
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'PayslipItem' AND column_name = 'Type') THEN
        ALTER TABLE "PayslipItem" ALTER COLUMN "Type" DROP NOT NULL;
        ALTER TABLE "PayslipItem" ALTER COLUMN "Type" SET DEFAULT 1;
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'PayslipItem' AND column_name = 'SalaryComponentId') THEN
        ALTER TABLE "PayslipItem" ALTER COLUMN "SalaryComponentId" DROP NOT NULL;
    END IF;
    UPDATE "PayslipItem" SET "Name" = COALESCE("Name", "ComponentName", 'Item');
    UPDATE "PayslipItem" SET "ComponentName" = COALESCE("ComponentName", "Name", 'Item');
    UPDATE "PayslipItem" SET "ItemType" = COALESCE("ItemType", "Type", 1);
    UPDATE "PayslipItem" SET "Type" = COALESCE("Type", "ItemType", 1);
END $$;

