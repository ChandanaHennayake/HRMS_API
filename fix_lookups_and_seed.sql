-- ============================================================================
-- AlienHR — Fix Column Aliases & Master Data Seed Script for Supabase / PostgreSQL
-- ============================================================================

-- 1. Ensure both column conventions exist for Department, Designation, and EmploymentType
ALTER TABLE "Department" ADD COLUMN IF NOT EXISTS "Name" character varying(100);
UPDATE "Department" SET "Name" = "DepartmentName" WHERE "Name" IS NULL;
ALTER TABLE "Department" ADD COLUMN IF NOT EXISTS "Code" character varying(50);
UPDATE "Department" SET "Code" = "DepartmentCode" WHERE "Code" IS NULL;

ALTER TABLE "Designation" ADD COLUMN IF NOT EXISTS "Name" character varying(100);
UPDATE "Designation" SET "Name" = "DesignationName" WHERE "Name" IS NULL;
ALTER TABLE "Designation" ADD COLUMN IF NOT EXISTS "Code" character varying(50);
UPDATE "Designation" SET "Code" = "DesignationCode" WHERE "Code" IS NULL;

ALTER TABLE "EmploymentType" ADD COLUMN IF NOT EXISTS "Name" character varying(100);
UPDATE "EmploymentType" SET "Name" = "TypeName" WHERE "Name" IS NULL;
ALTER TABLE "EmploymentType" ADD COLUMN IF NOT EXISTS "Code" character varying(50);
UPDATE "EmploymentType" SET "Code" = "TypeCode" WHERE "Code" IS NULL;

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
