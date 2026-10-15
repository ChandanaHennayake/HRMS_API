using HRMS.API.DTOs.Leave;
using HRMS.API.Enums;
using HRMS.API.Interfaces.Repositories;
using HRMS.API.Repository.Leave;
using HRMS.API.Service.Audit;
using HRMS.API.Service.Email;
using HRMS.API.Service.Notification;
using LeaveRequestEntity = HRMS.API.Entities.LeaveRequest;

namespace HRMS.API.Service.Leave
{
    public class LeaveService : ILeaveService
    {
        private readonly ILeaveRepository _leaveRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ILeaveBalanceRepository _leaveBalanceRepository;
        private readonly ILeaveTypeRepository _leaveTypeRepository;
        private readonly INotificationService _notificationService;
        private readonly IAuditService _auditService;
        private readonly IEmailService _emailService;

        public LeaveService(
            ILeaveRepository leaveRepository,
            IEmployeeRepository employeeRepository,
            ILeaveBalanceRepository leaveBalanceRepository,
            ILeaveTypeRepository leaveTypeRepository,
            INotificationService notificationService,
            IAuditService auditService,
            IEmailService emailService)
        {
            _leaveRepository = leaveRepository;
            _employeeRepository = employeeRepository;
            _leaveBalanceRepository = leaveBalanceRepository;
            _leaveTypeRepository = leaveTypeRepository;
            _notificationService = notificationService;
            _auditService = auditService;
            _emailService = emailService;
        }

        // =====================================================
        // APPLY LEAVE
        // =====================================================

        public async Task<LeaveResponseDto> ApplyLeaveAsync(
            long employeeId,
            ApplyLeaveDto request)
        {
            // -------------------------------------------------
            // CHECK EMPLOYEE
            // -------------------------------------------------

            var employee = await _employeeRepository
                .GetEntityByIdAsync(employeeId);

            if (employee == null)
            {
                throw new Exception(
                    "Employee not found.");
            }

            if (!employee.IsActive)
            {
                throw new Exception(
                    "Employee is inactive.");
            }

            // -------------------------------------------------
            // VALIDATE DATE
            // -------------------------------------------------

            if (request.FromDate > request.ToDate)
            {
                throw new Exception(
                    "From date cannot be greater than to date.");
            }

            // -------------------------------------------------
            // PREVENT PAST LEAVE
            // -------------------------------------------------

            var today = DateOnly.FromDateTime(
                DateTime.Now);

            if (request.FromDate < today)
            {
                throw new Exception(
                    "Leave cannot be applied for a past date.");
            }

            // -------------------------------------------------
            // CHECK OVERLAPPING LEAVE
            // -------------------------------------------------

            var hasOverlap = await _leaveRepository
                .HasOverlappingLeaveAsync(
                    employeeId,
                    request.FromDate,
                    request.ToDate);

            if (hasOverlap)
            {
                throw new Exception(
                    "You already have a pending or approved leave " +
                    "for the selected date range.");
            }

            // -------------------------------------------------
            // CALCULATE TOTAL DAYS
            //
            // Sundays are not counted.
            // -------------------------------------------------

            decimal totalDays = 0;

            var currentDate = request.FromDate;

            while (currentDate <= request.ToDate)
            {
                if (currentDate.DayOfWeek !=
                    DayOfWeek.Sunday)
                {
                    totalDays++;
                }

                currentDate = currentDate.AddDays(1);
            }

            if (totalDays <= 0)
            {
                throw new Exception(
                    "Selected leave period contains no working days.");
            }

            // -------------------------------------------------
            // LEAVE BALANCE VALIDATION & PENDING ALLOCATION
            // -------------------------------------------------

            var leaveType = await _leaveTypeRepository
                .GetByIdAsync(request.LeaveTypeId);

            if (leaveType == null)
            {
                throw new Exception(
                    "Leave type not found.");
            }

            if (leaveType.IsPaid)
            {
                int leaveYear = request.FromDate.Year;

                await _leaveBalanceRepository
                    .EnsureBalancesAsync(employeeId, leaveYear, employee.CompanyId);

                var balance = await _leaveBalanceRepository
                    .GetBalanceAsync(employeeId, request.LeaveTypeId, leaveYear);

                if (balance == null)
                {
                    throw new Exception(
                        $"No leave balance found for {leaveType.Name} in {leaveYear}.");
                }

                decimal available = balance.AllocatedDays - (balance.UsedDays + balance.PendingDays);

                if (available < totalDays)
                {
                    throw new Exception(
                        $"Insufficient leave balance for {leaveType.Name}. " +
                        $"Available: {Math.Max(0, available)} days, Requested: {totalDays} days.");
                }

                balance.PendingDays += totalDays;
                await _leaveBalanceRepository.UpdateAsync(balance);
            }

            // -------------------------------------------------
            // CREATE LEAVE REQUEST
            // -------------------------------------------------

            var leaveRequest =
                new LeaveRequestEntity
                {
                    EmployeeId =
                        employeeId,

                    LeaveTypeId =
                        request.LeaveTypeId,

                    FromDate =
                        request.FromDate,

                    ToDate =
                        request.ToDate,

                    TotalDays =
                        totalDays,

                    Reason =
                        request.Reason?.Trim(),

                    Status =
                        (short)LeaveRequestStatus.Pending,

                    AppliedAt =
                        DateTime.Now,

                    CreatedAt =
                        DateTime.Now
                };

            await _leaveRepository
                .CreateAsync(leaveRequest);

            // -------------------------------------------------
            // RELOAD WITH LEAVE TYPE
            //
            // CreateAsync does not automatically populate
            // leaveRequest.LeaveType.
            // -------------------------------------------------

            var createdLeave = await _leaveRepository
                .GetByIdAsync(leaveRequest.Id);

            if (createdLeave == null)
            {
                throw new Exception(
                    "Unable to load created leave request.");
            }

            return MapToDto(createdLeave);
        }

        // =====================================================
        // GET EMPLOYEE LEAVE HISTORY
        // =====================================================

        public async Task<IEnumerable<LeaveResponseDto>>
            GetEmployeeLeavesAsync(long employeeId)
        {
            var employee = await _employeeRepository
                .GetEntityByIdAsync(employeeId);

            if (employee == null)
            {
                throw new Exception(
                    "Employee not found.");
            }

            var leaves = await _leaveRepository
                .GetEmployeeLeavesAsync(employeeId);

            return leaves.Select(MapToDto);
        }

        // =====================================================
        // GET ALL PENDING LEAVE REQUESTS
        // =====================================================

        public async Task<IEnumerable<LeaveResponseDto>>
            GetPendingLeavesAsync(int? branchId = null)
        {
            var leaves = await _leaveRepository
                .GetPendingLeavesAsync(branchId);

            return leaves.Select(MapToDto);
        }

        // =====================================================
        // APPROVE LEAVE
        // =====================================================

        public async Task<LeaveResponseDto>
            ApproveLeaveAsync(
                long leaveRequestId,
                long approvedBy,
                LeaveActionDto request)
        {
            var leaveRequest =
                await _leaveRepository
                    .GetByIdAsync(leaveRequestId);

            if (leaveRequest == null)
            {
                throw new Exception(
                    "Leave request not found.");
            }

            if (leaveRequest.Status !=
                (short)LeaveRequestStatus.Pending)
            {
                throw new Exception(
                    "Only pending leave requests can be approved.");
            }

            // Update leave balance: pending -> used
            var leaveType = await _leaveTypeRepository
                .GetByIdAsync(leaveRequest.LeaveTypeId);

            if (leaveType?.IsPaid == true)
            {
                var balance = await _leaveBalanceRepository
                    .GetBalanceAsync(
                        leaveRequest.EmployeeId,
                        leaveRequest.LeaveTypeId,
                        leaveRequest.FromDate.Year);

                if (balance != null)
                {
                    balance.PendingDays = Math.Max(0, balance.PendingDays - leaveRequest.TotalDays);
                    balance.UsedDays += leaveRequest.TotalDays;
                    await _leaveBalanceRepository.UpdateAsync(balance);
                }
            }

            leaveRequest.Status =
                (short)LeaveRequestStatus.Approved;

            leaveRequest.ApprovedBy =
                approvedBy;

            leaveRequest.ApprovedAt =
                DateTime.Now;

            var comment = request.EffectiveComment;
            leaveRequest.ManagerComment = string.IsNullOrWhiteSpace(comment) ? "Approved" : comment;

            leaveRequest.UpdatedAt =
                DateTime.Now;

            await _leaveRepository
                .UpdateAsync(leaveRequest);

            // Reload to make sure LeaveType is available
            var updatedLeave = await _leaveRepository
                .GetByIdAsync(leaveRequest.Id);

            if (updatedLeave == null)
            {
                throw new Exception(
                    "Unable to load updated leave request.");
            }

            try
            {
                await _notificationService.NotifyLeaveDecisionAsync(
                    updatedLeave.EmployeeId,
                    updatedLeave.LeaveType?.Name ?? "Leave",
                    true,
                    leaveRequest.ManagerComment);

                var empForEmail = await _employeeRepository.GetEntityByIdAsync(updatedLeave.EmployeeId);
                if (empForEmail != null && !string.IsNullOrWhiteSpace(empForEmail.Email))
                {
                    await _emailService.SendLeaveDecisionEmailAsync(
                        empForEmail.Email,
                        $"{empForEmail.FirstName} {empForEmail.LastName}".Trim(),
                        updatedLeave.LeaveType?.Name ?? "Leave",
                        updatedLeave.FromDate,
                        updatedLeave.ToDate,
                        updatedLeave.TotalDays,
                        true,
                        leaveRequest.ManagerComment);
                }
            }
            catch
            {
                // Non-fatal notification/email failure
            }

            try
            {
                await _auditService.LogEntityActionAsync(
                    approvedBy != 0 ? (int)approvedBy : null,
                    null,
                    "Approve",
                    "LeaveRequest",
                    updatedLeave.Id.ToString(),
                    $"Approved leave request #{updatedLeave.Id} ({updatedLeave.LeaveType?.Name}) for Employee #{updatedLeave.EmployeeId}. Comment: {leaveRequest.ManagerComment}",
                    severity: "Info");
            }
            catch
            {
                // Non-fatal audit log failure
            }

            return MapToDto(updatedLeave);
        }

        // =====================================================
        // REJECT LEAVE
        // =====================================================

        public async Task<LeaveResponseDto>
            RejectLeaveAsync(
                long leaveRequestId,
                long rejectedBy,
                LeaveActionDto request)
        {
            var leaveRequest =
                await _leaveRepository
                    .GetByIdAsync(leaveRequestId);

            if (leaveRequest == null)
            {
                throw new Exception(
                    "Leave request not found.");
            }

            if (leaveRequest.Status !=
                (short)LeaveRequestStatus.Pending)
            {
                throw new Exception(
                    "Only pending leave requests can be rejected.");
            }

            var rejectComment = request.EffectiveComment;
            if (string.IsNullOrWhiteSpace(rejectComment))
            {
                rejectComment = "Rejected by manager";
            }

            // Revert pending days in leave balance
            var leaveType = await _leaveTypeRepository
                .GetByIdAsync(leaveRequest.LeaveTypeId);

            if (leaveType?.IsPaid == true)
            {
                var balance = await _leaveBalanceRepository
                    .GetBalanceAsync(
                        leaveRequest.EmployeeId,
                        leaveRequest.LeaveTypeId,
                        leaveRequest.FromDate.Year);

                if (balance != null)
                {
                    balance.PendingDays = Math.Max(0, balance.PendingDays - leaveRequest.TotalDays);
                    await _leaveBalanceRepository.UpdateAsync(balance);
                }
            }

            leaveRequest.Status =
                (short)LeaveRequestStatus.Rejected;

            leaveRequest.RejectedBy =
                rejectedBy;

            leaveRequest.RejectedAt =
                DateTime.Now;

            leaveRequest.ManagerComment =
                rejectComment;

            leaveRequest.UpdatedAt =
                DateTime.Now;

            await _leaveRepository
                .UpdateAsync(leaveRequest);

            // Reload to make sure LeaveType is available
            var updatedLeave = await _leaveRepository
                .GetByIdAsync(leaveRequest.Id);

            if (updatedLeave == null)
            {
                throw new Exception(
                    "Unable to load updated leave request.");
            }

            await _notificationService.NotifyLeaveDecisionAsync(
                updatedLeave.EmployeeId,
                updatedLeave.LeaveType?.Name ?? "Leave",
                false,
                request.Comment);

            var empForRejectEmail = await _employeeRepository.GetEntityByIdAsync(updatedLeave.EmployeeId);
            if (empForRejectEmail != null && !string.IsNullOrWhiteSpace(empForRejectEmail.Email))
            {
                await _emailService.SendLeaveDecisionEmailAsync(
                    empForRejectEmail.Email,
                    $"{empForRejectEmail.FirstName} {empForRejectEmail.LastName}".Trim(),
                    updatedLeave.LeaveType?.Name ?? "Leave",
                    updatedLeave.FromDate,
                    updatedLeave.ToDate,
                    updatedLeave.TotalDays,
                    false,
                    request.Comment);
            }

            await _auditService.LogEntityActionAsync(
                rejectedBy != 0 ? (int)rejectedBy : null,
                null,
                "Reject",
                "LeaveRequest",
                updatedLeave.Id.ToString(),
                $"Rejected leave request #{updatedLeave.Id} ({updatedLeave.LeaveType?.Name}) for Employee #{updatedLeave.EmployeeId}. Reason: {request.Comment}",
                severity: "Warning");

            return MapToDto(updatedLeave);
        }

        // =====================================================
        // CANCEL LEAVE
        // =====================================================

        public async Task<LeaveResponseDto>
            CancelLeaveAsync(
                long leaveRequestId,
                long employeeId)
        {
            var leaveRequest =
                await _leaveRepository
                    .GetByIdAsync(leaveRequestId);

            if (leaveRequest == null)
            {
                throw new Exception(
                    "Leave request not found.");
            }

            // -------------------------------------------------
            // EMPLOYEE CAN ONLY CANCEL OWN LEAVE
            // -------------------------------------------------

            if (leaveRequest.EmployeeId != employeeId)
            {
                throw new Exception(
                    "You cannot cancel another employee's leave.");
            }

            // -------------------------------------------------
            // ONLY PENDING LEAVE CAN BE CANCELLED
            // -------------------------------------------------

            if (leaveRequest.Status !=
                (short)LeaveRequestStatus.Pending)
            {
                throw new Exception(
                    "Only pending leave requests can be cancelled.");
            }

            // Revert pending days in leave balance
            var leaveType = await _leaveTypeRepository
                .GetByIdAsync(leaveRequest.LeaveTypeId);

            if (leaveType?.IsPaid == true)
            {
                var balance = await _leaveBalanceRepository
                    .GetBalanceAsync(
                        leaveRequest.EmployeeId,
                        leaveRequest.LeaveTypeId,
                        leaveRequest.FromDate.Year);

                if (balance != null)
                {
                    balance.PendingDays = Math.Max(0, balance.PendingDays - leaveRequest.TotalDays);
                    await _leaveBalanceRepository.UpdateAsync(balance);
                }
            }

            leaveRequest.Status =
                (short)LeaveRequestStatus.Cancelled;

            leaveRequest.CancelledAt =
                DateTime.Now;

            leaveRequest.UpdatedAt =
                DateTime.Now;

            await _leaveRepository
                .UpdateAsync(leaveRequest);

            // Reload to make sure LeaveType is available
            var updatedLeave = await _leaveRepository
                .GetByIdAsync(leaveRequest.Id);

            if (updatedLeave == null)
            {
                throw new Exception(
                    "Unable to load updated leave request.");
            }

            return MapToDto(updatedLeave);
        }

        // =====================================================
        // ENTITY -> DTO
        // =====================================================

        private static LeaveResponseDto MapToDto(
            LeaveRequestEntity leave)
        {
            return new LeaveResponseDto
            {
                Id =
                    leave.Id,

                EmployeeId =
                    leave.EmployeeId,

                LeaveTypeId =
                    leave.LeaveTypeId,

                // IMPORTANT:
                // Get the name from the loaded LeaveType
                LeaveType =
                    leave.LeaveType?.Name ?? "",

                FromDate =
                    leave.FromDate,

                ToDate =
                    leave.ToDate,

                TotalDays =
                    leave.TotalDays,

                Reason =
                    leave.Reason,

                Status =
                    leave.Status,

                StatusName =
                    GetStatusName(leave.Status),

                AppliedAt =
                    leave.AppliedAt,

                ApprovedBy =
                    leave.ApprovedBy,

                ApprovedAt =
                    leave.ApprovedAt,

                RejectedBy =
                    leave.RejectedBy,

                RejectedAt =
                    leave.RejectedAt,

                ManagerComment =
                    leave.ManagerComment,

                CancelledAt =
                    leave.CancelledAt
            };
        }

        // =====================================================
        // GET EMPLOYEE LEAVE BALANCES
        // =====================================================

        public async Task<IEnumerable<LeaveBalanceDto>>
            GetEmployeeLeaveBalancesAsync(long employeeId, int? year = null)
        {
            var employee = await _employeeRepository
                .GetEntityByIdAsync(employeeId);

            if (employee == null)
            {
                throw new Exception("Employee not found.");
            }

            int queryYear = year ?? DateTime.Now.Year;

            await _leaveBalanceRepository
                .EnsureBalancesAsync(employeeId, queryYear, employee.CompanyId);

            var balances = await _leaveBalanceRepository
                .GetBalancesAsync(employeeId, queryYear);

            return balances.Select(b => new LeaveBalanceDto
            {
                LeaveTypeId = b.LeaveTypeId,
                LeaveTypeName = b.LeaveType?.Name ?? "",
                LeaveTypeCode = b.LeaveType?.Code ?? "",
                IsPaid = b.LeaveType?.IsPaid ?? true,
                Year = b.Year,
                AllocatedDays = b.AllocatedDays,
                UsedDays = b.UsedDays,
                PendingDays = b.PendingDays
            });
        }

        // =====================================================
        // STATUS NAME
        // =====================================================

        private static string GetStatusName(
            short status)
        {
            return status switch
            {
                (short)LeaveRequestStatus.Pending =>
                    "Pending",

                (short)LeaveRequestStatus.Approved =>
                    "Approved",

                (short)LeaveRequestStatus.Rejected =>
                    "Rejected",

                (short)LeaveRequestStatus.Cancelled =>
                    "Cancelled",

                _ => "Unknown"
            };
        }
    }
}