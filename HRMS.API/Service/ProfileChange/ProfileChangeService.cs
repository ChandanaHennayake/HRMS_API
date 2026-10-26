using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HRMS.API.Data;
using HRMS.API.DTOs.Notification;
using HRMS.API.DTOs.ProfileChange;
using HRMS.API.Entities;
using HRMS.API.Helpers;
using HRMS.API.Service.Audit;
using HRMS.API.Service.Notification;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HRMS.API.Service.ProfileChange
{
    public class ProfileChangeService : IProfileChangeService
    {
        private readonly DefaultContext _context;
        private readonly INotificationService _notificationService;
        private readonly IAuditService _auditService;
        private readonly ILogger<ProfileChangeService> _logger;

        public ProfileChangeService(
            DefaultContext context,
            INotificationService notificationService,
            IAuditService auditService,
            ILogger<ProfileChangeService> logger)
        {
            _context = context;
            _notificationService = notificationService;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<ProfileChangeRequestDto> SubmitRequestAsync(SubmitProfileChangeRequestDto dto, int currentUserId)
        {
            Employee? employee = null;

            if (dto.EmployeeId.HasValue && dto.EmployeeId.Value > 0)
            {
                employee = await _context.Employees
                    .Include(e => e.Department)
                    .Include(e => e.Designation)
                    .FirstOrDefaultAsync(e => e.Id == dto.EmployeeId.Value && !e.IsDeleted);
            }

            if (employee == null)
            {
                var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.UserId == currentUserId);
                if (user?.EmployeeId != null)
                {
                    employee = await _context.Employees
                        .Include(e => e.Department)
                        .Include(e => e.Designation)
                        .FirstOrDefaultAsync(e => e.Id == user.EmployeeId.Value && !e.IsDeleted);
                }
                else if (user != null && !string.IsNullOrWhiteSpace(user.Email))
                {
                    employee = await _context.Employees
                        .Include(e => e.Department)
                        .Include(e => e.Designation)
                        .FirstOrDefaultAsync(e => e.Email == user.Email && !e.IsDeleted);
                }
            }

            if (employee == null)
            {
                throw new Exception("Employee profile could not be found for this user.");
            }

            // Check if there is already an existing pending request
            var pendingRequest = await _context.ProfileChangeRequests
                .FirstOrDefaultAsync(r => r.EmployeeId == employee.Id && r.Status == 0);

            if (pendingRequest != null)
            {
                throw new Exception("You already have a pending profile change request awaiting review. Please wait for an administrator to review it, or cancel it before submitting a new one.");
            }

            var request = new ProfileChangeRequest
            {
                EmployeeId = employee.Id,
                UserId = currentUserId > 0 ? currentUserId : null,
                CompanyId = employee.CompanyId,
                BranchId = employee.BranchId > 0 ? employee.BranchId : null,

                // Proposed values
                FirstName = string.IsNullOrWhiteSpace(dto.FirstName) ? null : dto.FirstName.Trim(),
                LastName = dto.LastName?.Trim(),
                Phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim(),
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender?.Trim(),
                AddressLine1 = dto.AddressLine1?.Trim(),
                AddressLine2 = dto.AddressLine2?.Trim(),
                City = dto.City?.Trim(),
                Qualification = dto.Qualification?.Trim(),
                BankName = dto.BankName?.Trim(),
                BankAccountNo = dto.BankAccountNo?.Trim(),
                BankBranch = dto.BankBranch?.Trim(),

                // Current values snapshot
                CurrentFirstName = employee.FirstName,
                CurrentLastName = employee.LastName,
                CurrentPhone = employee.Phone,
                CurrentDateOfBirth = employee.DateOfBirth,
                CurrentGender = employee.Gender,
                CurrentAddressLine1 = employee.AddressLine1,
                CurrentAddressLine2 = employee.AddressLine2,
                CurrentCity = employee.City,
                CurrentQualification = employee.Qualification,
                CurrentBankName = employee.BankName,
                CurrentBankAccountNo = employee.BankAccountNo,
                CurrentBankBranch = employee.BankBranch,

                Status = 0, // Pending
                Reason = dto.Reason?.Trim(),
                CreatedAt = DateTime.Now
            };

            await _context.ProfileChangeRequests.AddAsync(request);
            await _context.SaveChangesAsync();

            // Notify Admin and Managers
            try
            {
                var approvers = await _context.AppUsers
                    .Where(u => u.CompanyId == employee.CompanyId && (u.RoleId == 1 || u.RoleId == 2) && (u.IsActive ?? true))
                    .ToListAsync();

                foreach (var approver in approvers)
                {
                    await _notificationService.SendNotificationAsync(new CreateNotificationDto
                    {
                        UserId = approver.UserId,
                        Title = "Profile Change Request",
                        Message = $"Employee {employee.FirstName} {employee.LastName} (#{employee.EmployeeCode}) has submitted a profile change request for review.",
                        Type = "Info",
                        ReferenceType = "ProfileChangeRequest",
                        ReferenceId = request.Id
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send notifications for profile change request #{RequestId}", request.Id);
            }

            return await MapToDtoAsync(request);
        }

        public async Task<List<ProfileChangeRequestDto>> GetMyRequestsAsync(long employeeId, int? userId = null)
        {
            var query = _context.ProfileChangeRequests
                .Include(r => r.Employee)
                    .ThenInclude(e => e.Department)
                .Include(r => r.Employee)
                    .ThenInclude(e => e.Designation)
                .AsQueryable();

            if (employeeId > 0)
            {
                query = query.Where(r => r.EmployeeId == employeeId);
            }
            else if (userId.HasValue && userId.Value > 0)
            {
                query = query.Where(r => r.UserId == userId.Value);
            }

            var list = await query
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var dtos = new List<ProfileChangeRequestDto>();
            foreach (var item in list)
            {
                dtos.Add(await MapToDtoAsync(item));
            }
            return dtos;
        }

        public async Task<List<ProfileChangeRequestDto>> GetAllRequestsAsync(long? companyId = null, long? branchId = null, short? status = null)
        {
            var query = _context.ProfileChangeRequests
                .Include(r => r.Employee)
                    .ThenInclude(e => e.Department)
                .Include(r => r.Employee)
                    .ThenInclude(e => e.Designation)
                .AsQueryable();

            if (companyId.HasValue && companyId.Value > 0)
            {
                query = query.Where(r => r.CompanyId == companyId.Value);
            }

            if (branchId.HasValue && branchId.Value > 0)
            {
                query = query.Where(r => r.BranchId == branchId.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(r => r.Status == status.Value);
            }

            var list = await query
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var dtos = new List<ProfileChangeRequestDto>();
            foreach (var item in list)
            {
                dtos.Add(await MapToDtoAsync(item));
            }
            return dtos;
        }

        public async Task<ProfileChangeRequestDto?> GetByIdAsync(long id)
        {
            var request = await _context.ProfileChangeRequests
                .Include(r => r.Employee)
                    .ThenInclude(e => e.Department)
                .Include(r => r.Employee)
                    .ThenInclude(e => e.Designation)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null) return null;
            return await MapToDtoAsync(request);
        }

        public async Task<ProfileChangeRequestDto> ApproveRequestAsync(long id, ReviewProfileChangeDto dto, int reviewerUserId)
        {
            var request = await _context.ProfileChangeRequests
                .Include(r => r.Employee)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
            {
                throw new Exception("Profile change request not found.");
            }

            if (request.Status != 0)
            {
                throw new Exception($"Cannot approve request #{id} because it is already {(request.Status == 1 ? "Approved" : "Closed")}.");
            }

            var reviewer = await _context.AppUsers.FirstOrDefaultAsync(u => u.UserId == reviewerUserId);
            var reviewerName = reviewer != null ? $"{reviewer.FirstName} {reviewer.LastName}".Trim() : "Administrator";

            request.Status = 1; // Approved
            request.ReviewedBy = reviewerUserId;
            request.ReviewerName = reviewerName;
            request.ReviewedAt = DateTime.Now;
            request.ReviewerComment = dto.Comment?.Trim();
            request.UpdatedAt = DateTime.Now;

            // Apply to Employee Table
            var employee = request.Employee;
            if (employee != null)
            {
                if (!string.IsNullOrWhiteSpace(request.FirstName)) employee.FirstName = request.FirstName;
                if (request.LastName != null) employee.LastName = request.LastName;
                if (!string.IsNullOrWhiteSpace(request.Phone)) employee.Phone = request.Phone;
                if (request.DateOfBirth.HasValue) employee.DateOfBirth = request.DateOfBirth;
                if (!string.IsNullOrWhiteSpace(request.Gender)) employee.Gender = request.Gender;
                if (request.AddressLine1 != null) employee.AddressLine1 = request.AddressLine1;
                if (request.AddressLine2 != null) employee.AddressLine2 = request.AddressLine2;
                if (request.City != null) employee.City = request.City;
                if (request.Qualification != null) employee.Qualification = request.Qualification;
                if (!string.IsNullOrWhiteSpace(request.BankName)) employee.BankName = request.BankName;
                if (!string.IsNullOrWhiteSpace(request.BankAccountNo)) employee.BankAccountNo = request.BankAccountNo;
                if (request.BankBranch != null) employee.BankBranch = request.BankBranch;

                employee.UpdatedAt = DateTime.UtcNow;
                employee.UpdatedBy = reviewerUserId;

                // Apply changes to AppUser (User Table)
                var linkedUser = await _context.AppUsers.FirstOrDefaultAsync(u => u.EmployeeId == (int)employee.Id || (request.UserId.HasValue && u.UserId == request.UserId.Value) || (!string.IsNullOrEmpty(employee.Email) && u.Email == employee.Email));
                if (linkedUser != null)
                {
                    linkedUser.FirstName = employee.FirstName;
                    linkedUser.LastName = employee.LastName ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(employee.Phone)) linkedUser.Phone = employee.Phone;
                    linkedUser.EmployeeId = (int)employee.Id;
                    linkedUser.ModifiedDate = DateTimeHelper.Now;
                    linkedUser.ModifiedBy = reviewerUserId;
                }

                // Apply changes to active EmployeeSalary
                if (!string.IsNullOrWhiteSpace(employee.BankAccountNo) || !string.IsNullOrWhiteSpace(employee.BankName))
                {
                    var activeSalary = await _context.EmployeeSalaries.FirstOrDefaultAsync(s => s.EmployeeId == employee.Id && s.IsActive);
                    if (activeSalary != null)
                    {
                        if (!string.IsNullOrWhiteSpace(employee.BankName)) activeSalary.BankName = employee.BankName;
                        if (!string.IsNullOrWhiteSpace(employee.BankAccountNo)) activeSalary.BankAccountNo = employee.BankAccountNo;
                        activeSalary.UpdatedAt = DateTime.Now;
                    }
                }
            }

            await _context.SaveChangesAsync();

            // Notify Employee
            try
            {
                var targetUserId = request.UserId;
                if (!targetUserId.HasValue && employee != null)
                {
                    var u = await _context.AppUsers.FirstOrDefaultAsync(x => x.EmployeeId == (int)employee.Id);
                    targetUserId = u?.UserId;
                }

                if (targetUserId.HasValue)
                {
                    await _notificationService.SendNotificationAsync(new CreateNotificationDto
                    {
                        UserId = targetUserId.Value,
                        EmployeeId = employee?.Id,
                        Title = "Profile Change Approved",
                        Message = $"Your profile change request has been approved by {reviewerName}. Your personal and banking records have been updated.",
                        Type = "Success",
                        ReferenceType = "ProfileChangeRequest",
                        ReferenceId = request.Id
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send approval notification for profile change request #{RequestId}", request.Id);
            }

            await _auditService.LogSecurityEventAsync(
                reviewerUserId,
                reviewerName,
                "ApproveProfileChangeRequest",
                $"Approved profile change request #{request.Id} for employee #{request.EmployeeId} ({employee?.FirstName} {employee?.LastName})",
                null
            );

            return await MapToDtoAsync(request);
        }

        public async Task<ProfileChangeRequestDto> RejectRequestAsync(long id, ReviewProfileChangeDto dto, int reviewerUserId)
        {
            var request = await _context.ProfileChangeRequests
                .Include(r => r.Employee)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
            {
                throw new Exception("Profile change request not found.");
            }

            if (request.Status != 0)
            {
                throw new Exception($"Cannot reject request #{id} because it is already closed.");
            }

            var reviewer = await _context.AppUsers.FirstOrDefaultAsync(u => u.UserId == reviewerUserId);
            var reviewerName = reviewer != null ? $"{reviewer.FirstName} {reviewer.LastName}".Trim() : "Administrator";

            request.Status = 2; // Rejected
            request.ReviewedBy = reviewerUserId;
            request.ReviewerName = reviewerName;
            request.ReviewedAt = DateTime.Now;
            request.ReviewerComment = dto.Comment?.Trim();
            request.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            // Notify Employee
            try
            {
                var targetUserId = request.UserId;
                if (!targetUserId.HasValue && request.Employee != null)
                {
                    var u = await _context.AppUsers.FirstOrDefaultAsync(x => x.EmployeeId == (int)request.Employee.Id);
                    targetUserId = u?.UserId;
                }

                if (targetUserId.HasValue)
                {
                    await _notificationService.SendNotificationAsync(new CreateNotificationDto
                    {
                        UserId = targetUserId.Value,
                        EmployeeId = request.Employee?.Id,
                        Title = "Profile Change Rejected",
                        Message = $"Your profile change request was rejected by {reviewerName}. Reason: {dto.Comment ?? "Not approved by management."}",
                        Type = "Warning",
                        ReferenceType = "ProfileChangeRequest",
                        ReferenceId = request.Id
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send rejection notification for profile change request #{RequestId}", request.Id);
            }

            await _auditService.LogSecurityEventAsync(
                reviewerUserId,
                reviewerName,
                "RejectProfileChangeRequest",
                $"Rejected profile change request #{request.Id} for employee #{request.EmployeeId}. Reason: {dto.Comment}",
                null
            );

            return await MapToDtoAsync(request);
        }

        public async Task<bool> CancelRequestAsync(long id, int currentUserId)
        {
            var request = await _context.ProfileChangeRequests.FirstOrDefaultAsync(r => r.Id == id);
            if (request == null) return false;

            if (request.Status != 0)
            {
                throw new Exception("Only pending requests can be cancelled.");
            }

            request.Status = 3; // Cancelled
            request.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<ProfileChangeRequestDto> MapToDtoAsync(ProfileChangeRequest r)
        {
            string employeeName = string.Empty;
            string employeeCode = string.Empty;
            string? deptName = null;
            string? desigName = null;
            string? branchName = null;

            if (r.Employee != null)
            {
                employeeName = $"{r.Employee.FirstName} {r.Employee.LastName}".Trim();
                employeeCode = r.Employee.EmployeeCode;
                deptName = r.Employee.Department?.Name;
                desigName = r.Employee.Designation?.Name;
            }
            else
            {
                var emp = await _context.Employees
                    .Include(e => e.Department)
                    .Include(e => e.Designation)
                    .FirstOrDefaultAsync(e => e.Id == r.EmployeeId);
                if (emp != null)
                {
                    employeeName = $"{emp.FirstName} {emp.LastName}".Trim();
                    employeeCode = emp.EmployeeCode;
                    deptName = emp.Department?.Name;
                    desigName = emp.Designation?.Name;
                }
            }

            if (r.BranchId.HasValue && r.BranchId.Value > 0)
            {
                var b = await _context.Branches.FirstOrDefaultAsync(x => x.branchid == (int)r.BranchId.Value);
                branchName = b?.branchname;
            }

            return new ProfileChangeRequestDto
            {
                Id = r.Id,
                EmployeeId = r.EmployeeId,
                EmployeeName = employeeName,
                EmployeeCode = employeeCode,
                DepartmentName = deptName,
                DesignationName = desigName,
                UserId = r.UserId,
                CompanyId = r.CompanyId,
                BranchId = r.BranchId,
                BranchName = branchName,

                FirstName = r.FirstName,
                LastName = r.LastName,
                Phone = r.Phone,
                DateOfBirth = r.DateOfBirth,
                Gender = r.Gender,
                AddressLine1 = r.AddressLine1,
                AddressLine2 = r.AddressLine2,
                City = r.City,
                Qualification = r.Qualification,
                BankName = r.BankName,
                BankAccountNo = r.BankAccountNo,
                BankBranch = r.BankBranch,

                CurrentFirstName = r.CurrentFirstName,
                CurrentLastName = r.CurrentLastName,
                CurrentPhone = r.CurrentPhone,
                CurrentDateOfBirth = r.CurrentDateOfBirth,
                CurrentGender = r.CurrentGender,
                CurrentAddressLine1 = r.CurrentAddressLine1,
                CurrentAddressLine2 = r.CurrentAddressLine2,
                CurrentCity = r.CurrentCity,
                CurrentQualification = r.CurrentQualification,
                CurrentBankName = r.CurrentBankName,
                CurrentBankAccountNo = r.CurrentBankAccountNo,
                CurrentBankBranch = r.CurrentBankBranch,

                Status = r.Status,
                Reason = r.Reason,
                ReviewedBy = r.ReviewedBy,
                ReviewerName = r.ReviewerName,
                ReviewedAt = r.ReviewedAt,
                ReviewerComment = r.ReviewerComment,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            };
        }
    }
}
