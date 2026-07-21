using HRMS.API.DTOs.Employee;
using HRMS.API.DTOs.User;
using HRMS.API.Entities;
using HRMS.API.Interfaces.Repositories;
using HRMS.API.Interfaces.Services;
using HRMS.API.Service.User;

namespace HRMS.API.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IUserService _userService;

        public EmployeeService(
            IEmployeeRepository employeeRepository,
            IUserService userService)
        {
            _employeeRepository = employeeRepository;
            _userService = userService;
        }

        //public async Task<EmployeeDto> CreateAsync(
        //    CreateEmployeeDto request)
        //{
        //    // -----------------------------------------------------
        //    // 1. CHECK EMPLOYEE CODE
        //    // -----------------------------------------------------

        //    var employeeCodeExists =
        //        await _employeeRepository.EmployeeCodeExistsAsync(
        //            request.CompanyId,
        //            request.EmployeeCode
        //        );

        //    if (employeeCodeExists)
        //    {
        //        throw new Exception(
        //            "Employee code already exists."
        //        );
        //    }


        //    // -----------------------------------------------------
        //    // 2. CREATE EMPLOYEE
        //    // -----------------------------------------------------

        //    var employee = new Employee
        //    {
        //        CompanyId = request.CompanyId,
        //        BranchId = request.BranchId,

        //        EmployeeCode =
        //            request.EmployeeCode.Trim(),

        //        FirstName =
        //            request.FirstName.Trim(),

        //        LastName =
        //            request.LastName?.Trim(),

        //        Email =
        //            request.Email?.Trim(),

        //        Phone =
        //            request.Phone?.Trim(),

        //        DepartmentId =
        //            request.DepartmentId,

        //        DesignationId =
        //            request.DesignationId,

        //        EmploymentTypeId =
        //            request.EmploymentTypeId,

        //        DateOfBirth =
        //            request.DateOfBirth,

        //        JoinedDate =
        //            request.JoinedDate,

        //        Gender =
        //            request.Gender?.Trim(),

        //        AddressLine1 =
        //            request.AddressLine1?.Trim(),

        //        AddressLine2 =
        //            request.AddressLine2?.Trim(),

        //        City =
        //            request.City?.Trim(),

        //        IsActive = true,
        //        IsDeleted = false,

        //        CreatedAt = DateTime.UtcNow,

        //        CreatedBy =
        //            request.CreatedBy
        //    };


        //    // -----------------------------------------------------
        //    // 3. SAVE EMPLOYEE
        //    // -----------------------------------------------------

        //    var createdEmployee =
        //        await _employeeRepository.CreateAsync(employee);


        //    // -----------------------------------------------------
        //    // 4. CREATE USER LOGIN
        //    // -----------------------------------------------------

        //    var createUserRequest =
        //        new CreateUserRequest
        //        {
        //            CompanyId =
        //                Convert.ToInt32(request.CompanyId),

        //            BranchId =
        //                Convert.ToInt32(request.BranchId),

        //            RoleId =
        //                request.RoleId,

        //            EmployeeId =
        //                Convert.ToInt32(createdEmployee.Id),

        //            Username =
        //                request.Username.Trim(),

        //            Email =
        //                request.Email ?? string.Empty,

        //            Password =
        //                request.Password,

        //            FirstName =
        //                request.FirstName.Trim(),

        //            LastName =
        //                request.LastName?.Trim(),

        //            Phone =
        //                request.Phone
        //        };


        //    // -----------------------------------------------------
        //    // 5. SAVE USER
        //    // -----------------------------------------------------

        //    await _userService.CreateAsync(
        //        createUserRequest
        //    );


        //    // -----------------------------------------------------
        //    // 6. RETURN CREATED EMPLOYEE
        //    // -----------------------------------------------------

        //    var result =
        //        await _employeeRepository.GetByIdAsync(
        //            createdEmployee.Id
        //        );

        //    if (result == null)
        //    {
        //        throw new Exception(
        //            "Employee created but could not be retrieved."
        //        );
        //    }

        //    return result;
        //}

        public async Task<EmployeeDto> CreateAsync(CreateEmployeeDto request)
        {
            // 1. Check employee code
            var employeeCodeExists =
                await _employeeRepository.EmployeeCodeExistsAsync(
                    request.CompanyId,
                    request.EmployeeCode.Trim()
                );

            if (employeeCodeExists)
            {
                throw new Exception("Employee code already exists.");
            }

            // 2. Create employee entity
            var employee = new Employee
            {
                CompanyId = request.CompanyId,
                BranchId = request.BranchId,

                EmployeeCode = request.EmployeeCode.Trim(),
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName?.Trim(),

                Email = request.Email?.Trim(),
                Phone = request.Phone?.Trim(),

                DepartmentId = request.DepartmentId,
                DesignationId = request.DesignationId,
                EmploymentTypeId = request.EmploymentTypeId,

                DateOfBirth = request.DateOfBirth,
                JoinedDate = request.JoinedDate,

                Gender = request.Gender?.Trim(),

                AddressLine1 = request.AddressLine1?.Trim(),
                AddressLine2 = request.AddressLine2?.Trim(),
                City = request.City?.Trim(),

                IsActive = true,
                IsDeleted = false,

                CreatedAt = DateTime.UtcNow,
                CreatedBy = request.CreatedBy
            };

            // 3. Save employee first
            // After SaveChangesAsync(), createdEmployee.Id should contain
            // the generated Employee ID
            var createdEmployee =
                await _employeeRepository.CreateAsync(employee);

            // 4. Make sure Employee ID was generated
            if (createdEmployee.Id <= 0)
            {
                throw new Exception(
                    "Employee was created but Employee ID was not generated."
                );
            }

            // 5. Create user login for this employee
            var createUserRequest = new CreateUserRequest
            {
                CompanyId = Convert.ToInt32(request.CompanyId),
                BranchId = Convert.ToInt32(request.BranchId),

                RoleId = request.RoleId,

                // IMPORTANT:
                // Pass the newly created employee ID
                EmployeeId = Convert.ToInt32(createdEmployee.Id),

                Username = request.Username.Trim(),
                Email = request.Email?.Trim() ?? string.Empty,
                Password = request.Password,

                FirstName = request.FirstName.Trim(),
                LastName = request.LastName?.Trim(),
                Phone = request.Phone?.Trim()
            };

            // 6. Create login user
            await _userService.CreateAsync(createUserRequest);

            // 7. Return employee
            var result =
                await _employeeRepository.GetByIdAsync(createdEmployee.Id);

            if (result == null)
            {
                throw new Exception(
                    "Employee created but could not be retrieved."
                );
            }

            return result;
        }
        public async Task<IEnumerable<EmployeeDto>> GetAllAsync()
        {
            return await _employeeRepository.GetAllAsync();
        }


        public async Task<EmployeeDto?> GetByIdAsync(long id)
        {
            return await _employeeRepository.GetByIdAsync(id);
        }


        public async Task<EmployeeDto?> UpdateAsync(
            long id,
            UpdateEmployeeDto request)
        {
            var employee =
                await _employeeRepository.GetEntityByIdAsync(id);

            if (employee == null)
                return null;

            var exists =
                await _employeeRepository.EmployeeCodeExistsAsync(
                    request.CompanyId,
                    request.EmployeeCode,
                    id
                );

            if (exists)
            {
                throw new Exception(
                    "Employee code already exists."
                );
            }

            employee.CompanyId =
                request.CompanyId;

            employee.BranchId =
                request.BranchId;

            employee.EmployeeCode =
                request.EmployeeCode.Trim();

            employee.FirstName =
                request.FirstName.Trim();

            employee.LastName =
                request.LastName?.Trim();

            employee.Email =
                request.Email?.Trim();

            employee.Phone =
                request.Phone?.Trim();

            employee.DepartmentId =
                request.DepartmentId;

            employee.DesignationId =
                request.DesignationId;

            employee.EmploymentTypeId =
                request.EmploymentTypeId;

            employee.DateOfBirth =
                request.DateOfBirth;

            employee.JoinedDate =
                request.JoinedDate;

            employee.Gender =
                request.Gender?.Trim();

            employee.AddressLine1 =
                request.AddressLine1?.Trim();

            employee.AddressLine2 =
                request.AddressLine2?.Trim();

            employee.City =
                request.City?.Trim();

            employee.IsActive =
                request.IsActive;

            employee.UpdatedAt =
                DateTime.UtcNow;

            employee.UpdatedBy =
                request.UpdatedBy;

            await _employeeRepository.UpdateAsync(
                employee
            );

            return await _employeeRepository.GetByIdAsync(
                id
            );
        }


        public async Task<bool> DeleteAsync(
            long id,
            long? deletedBy)
        {
            var employee =
                await _employeeRepository.GetEntityByIdAsync(id);

            if (employee == null)
                return false;

            employee.UpdatedAt =
                DateTime.UtcNow;

            employee.UpdatedBy =
                deletedBy;

            await _employeeRepository.DeleteAsync(
                employee
            );

            return true;
        }
    }
}