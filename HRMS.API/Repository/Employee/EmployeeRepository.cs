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

                    IsActive =
                        e.IsActive,

                    CreatedAt =
                        e.CreatedAt
                }
            )
            .AsNoTracking()
            .ToListAsync();

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

                    IsActive =
                        e.IsActive,

                    CreatedAt =
                        e.CreatedAt
                }
            )
            .AsNoTracking()
            .FirstOrDefaultAsync();

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

            await _context.SaveChangesAsync();
        }


        // =========================================================
        // DELETE - SOFT DELETE
        // =========================================================

        public async Task DeleteAsync(Employee employee)
        {
            employee.IsDeleted = true;

            employee.IsActive = false;

            _context.Employees.Update(employee);

            await _context.SaveChangesAsync();
        }
    }
}