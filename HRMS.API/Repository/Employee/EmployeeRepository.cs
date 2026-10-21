using HRMS.API.Data;
using HRMS.API.DTOs.Employee;
using HRMS.API.Entities;
using HRMS.API.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly DefaultContext _context;

        public EmployeeRepository(DefaultContext context)
        {
            _context = context;
        }

        // =========================================================
        // CREATE
        // =========================================================

        public async Task<Employee> CreateAsync(Employee employee)
        {
            await _context.Employees.AddAsync(employee);

            await _context.SaveChangesAsync();

            return employee;
        }


        // =========================================================
        // GET ALL
        // =========================================================

        public async Task<IEnumerable<EmployeeDto>> GetAllAsync()
        {
            var employees = await (
                from e in _context.Employees

                join d in _context.Departments
                    on e.DepartmentId equals d.Id
                    into departmentJoin

                from d in departmentJoin.DefaultIfEmpty()


                join des in _context.Designations
                    on e.DesignationId equals des.Id
                    into designationJoin

                from des in designationJoin.DefaultIfEmpty()


                join et in _context.EmploymentTypes
                    on e.EmploymentTypeId equals et.Id
                    into employmentTypeJoin

                from et in employmentTypeJoin.DefaultIfEmpty()


                where !e.IsDeleted

                orderby e.Id descending

                select new EmployeeDto
                {
                    Id = e.Id,

                    CompanyId = e.CompanyId,

                    BranchId = e.BranchId,

                    EmployeeCode = e.EmployeeCode,

                    FirstName = e.FirstName,

                    LastName = e.LastName,

                    FullName =
                        e.FirstName + " " +
                        (e.LastName ?? ""),

                    Email = e.Email,

                    Phone = e.Phone,

                    DepartmentId =
                        e.DepartmentId,

                    DepartmentName =
                        d != null
                            ? d.Name
                            : null,

                    DesignationId =
                        e.DesignationId,

                    DesignationName =
                        des != null
                            ? des.Name
                            : null,

                    EmploymentTypeId =
                        e.EmploymentTypeId,

                    EmploymentTypeName =
                        et != null
                            ? et.Name
                            : null,

                    DateOfBirth =
                        e.DateOfBirth,

                    JoinedDate =
                        e.JoinedDate,

                    Gender =
                        e.Gender,

                    AddressLine1 =
                        e.AddressLine1,

                    AddressLine2 =
                        e.AddressLine2,

                    City =
                        e.City,

                    BankName =
                        e.BankName,

                    BankAccountNo =
                        e.BankAccountNo,

                    BankBranch =
                        e.BankBranch,

                    Qualification =
                        e.Qualification,

                    IsActive =
                        e.IsActive,

                    CreatedAt =
                        e.CreatedAt
                }
            )
            .AsNoTracking()
            .ToListAsync();

            var employeeIds = employees.Select(e => (int)e.Id).ToList();
            var users = await _context.AppUsers
                .Where(u => u.EmployeeId.HasValue && employeeIds.Contains(u.EmployeeId.Value))
                .ToListAsync();

            var userByEmpId = users.Where(u => u.EmployeeId.HasValue).ToDictionary(u => u.EmployeeId!.Value);

            foreach (var emp in employees)
            {
                if (userByEmpId.TryGetValue((int)emp.Id, out var user))
                {
                    emp.UserId = user.UserId;
                    emp.Username = user.Username;
                    emp.Password = user.ClearPassword ?? user.PasswordSalt;
                }
            }

            return employees;
        }


        // =========================================================
        // GET BY ID - DTO
        // =========================================================

        public async Task<EmployeeDto?> GetByIdAsync(long id)
        {
            var employee = await (
                from e in _context.Employees

                join d in _context.Departments
                    on e.DepartmentId equals d.Id
                    into departmentJoin

                from d in departmentJoin.DefaultIfEmpty()


                join des in _context.Designations
                    on e.DesignationId equals des.Id
                    into designationJoin

                from des in designationJoin.DefaultIfEmpty()


                join et in _context.EmploymentTypes
                    on e.EmploymentTypeId equals et.Id
                    into employmentTypeJoin

                from et in employmentTypeJoin.DefaultIfEmpty()


                where
                    e.Id == id &&
                    !e.IsDeleted


                select new EmployeeDto
                {
                    Id = e.Id,

                    CompanyId =
                        e.CompanyId,

                    BranchId =
                        e.BranchId,

                    EmployeeCode =
                        e.EmployeeCode,

                    FirstName =
                        e.FirstName,

                    LastName =
                        e.LastName,

                    FullName =
                        e.FirstName + " " +
                        (e.LastName ?? ""),

                    Email =
                        e.Email,

                    Phone =
                        e.Phone,

                    DepartmentId =
                        e.DepartmentId,

                    DepartmentName =
                        d != null
                            ? d.Name
                            : null,

                    DesignationId =
                        e.DesignationId,

                    DesignationName =
                        des != null
                            ? des.Name
                            : null,

                    EmploymentTypeId =
                        e.EmploymentTypeId,

                    EmploymentTypeName =
                        et != null
                            ? et.Name
                            : null,

                    DateOfBirth =
                        e.DateOfBirth,

                    JoinedDate =
                        e.JoinedDate,

                    Gender =
                        e.Gender,

                    AddressLine1 =
                        e.AddressLine1,

                    AddressLine2 =
                        e.AddressLine2,

                    City =
                        e.City,

                    BankName =
                        e.BankName,

                    BankAccountNo =
                        e.BankAccountNo,

                    BankBranch =
                        e.BankBranch,

                    Qualification =
                        e.Qualification,

                    IsActive =
                        e.IsActive,

                    CreatedAt =
                        e.CreatedAt
                }
            )
            .AsNoTracking()
            .FirstOrDefaultAsync();

            if (employee != null)
            {
                var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.EmployeeId == (int)employee.Id || (!string.IsNullOrEmpty(employee.Email) && u.Email == employee.Email));
                if (user != null)
                {
                    employee.UserId = user.UserId;
                    employee.Username = user.Username;
                    employee.Password = user.ClearPassword ?? user.PasswordSalt;
                }
            }

            return employee;
        }


        // =========================================================
        // GET ENTITY BY ID
        // Used internally by Services such as AttendanceService
        // =========================================================

        public async Task<Employee?> GetEntityByIdAsync(long id)
        {
            return await _context.Employees
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    !x.IsDeleted);
        }


        // =========================================================
        // CHECK EMPLOYEE CODE
        // =========================================================

        public async Task<bool> EmployeeCodeExistsAsync(
            long companyId,
            string employeeCode,
            long? excludeId = null)
        {
            return await _context.Employees
                .AnyAsync(x =>
                    x.CompanyId == companyId &&
                    x.EmployeeCode == employeeCode &&
                    !x.IsDeleted &&
                    (
                        !excludeId.HasValue ||
                        x.Id != excludeId.Value
                    ));
        }


        // =========================================================
        // UPDATE
        // =========================================================

        public async Task UpdateAsync(Employee employee)
        {
            _context.Employees.Update(employee);

            if (!employee.IsActive || employee.IsDeleted)
            {
                var users = await _context.AppUsers
                    .Where(u => u.EmployeeId == (int)employee.Id || (!string.IsNullOrEmpty(employee.Email) && u.Email == employee.Email))
                    .ToListAsync();

                foreach (var u in users)
                {
                    u.IsActive = false;
                    u.IsDeleted = employee.IsDeleted;
                    u.IsLocked = true;
                    u.ModifiedDate = DateTime.Now;

                    var tokens = await _context.RefreshTokens.Where(t => t.userid == u.UserId).ToListAsync();
                    foreach (var t in tokens)
                    {
                        t.isrevoked = true;
                    }
                }
            }

            await _context.SaveChangesAsync();
        }


        public async Task<IEnumerable<Employee>>
    GetAllActiveEntitiesAsync()
        {
            return await _context.Employees
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    !x.IsDeleted)
                .ToListAsync();
        }


        // =========================================================
        // DELETE - SOFT DELETE
        // =========================================================

        public async Task DeleteAsync(Employee employee)
        {
            employee.IsDeleted = true;
            employee.IsActive = false;
            employee.UpdatedAt = DateTime.UtcNow;

            _context.Employees.Update(employee);

            // Cascade soft-delete / deactivation to associated user accounts
            var users = await _context.AppUsers
                .Where(u => u.EmployeeId == (int)employee.Id || (!string.IsNullOrEmpty(employee.Email) && u.Email == employee.Email))
                .ToListAsync();

            foreach (var u in users)
            {
                u.IsActive = false;
                u.IsLocked = true;
                u.ModifiedDate = DateTime.Now;
                u.ModifiedBy = employee.UpdatedBy.HasValue ? (int?)employee.UpdatedBy.Value : null;

                var tokens = await _context.RefreshTokens.Where(t => t.userid == u.UserId).ToListAsync();
                foreach (var t in tokens)
                {
                    t.isrevoked = true;
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}