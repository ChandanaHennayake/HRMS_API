using HRMS.API.DTOs.Leave;
using HRMS.API.Enums;
using HRMS.API.Interfaces.Repositories;
using HRMS.API.Repository.Leave;
using LeaveRequestEntity = HRMS.API.Entities.LeaveRequest;

namespace HRMS.API.Service.Leave
{
    public class LeaveService : ILeaveService
    {
        private readonly ILeaveRepository _leaveRepository;
        private readonly IEmployeeRepository _employeeRepository;

        public LeaveService(
            ILeaveRepository leaveRepository,
            IEmployeeRepository employeeRepository)
        {
            _leaveRepository = leaveRepository;
            _employeeRepository = employeeRepository;
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
            // Initial implementation:
            // Sundays are not counted.
            //
            // Later we can also remove public holidays.
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

                currentDate =
                    currentDate.AddDays(1);
            }


            if (totalDays <= 0)
            {
                throw new Exception(
                    "Selected leave period contains no working days.");
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


            return MapToDto(leaveRequest);
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
            GetPendingLeavesAsync()
        {
            var leaves = await _leaveRepository
                .GetPendingLeavesAsync();


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


            leaveRequest.Status =
                (short)LeaveRequestStatus.Approved;

            leaveRequest.ApprovedBy =
                approvedBy;

            leaveRequest.ApprovedAt =
                DateTime.Now;

            leaveRequest.ManagerComment =
                request.Comment?.Trim();

            leaveRequest.UpdatedAt =
                DateTime.Now;


            await _leaveRepository
                .UpdateAsync(leaveRequest);


            return MapToDto(leaveRequest);
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


            if (string.IsNullOrWhiteSpace(
                request.Comment))
            {
                throw new Exception(
                    "Comment is required when rejecting leave.");
            }


            leaveRequest.Status =
                (short)LeaveRequestStatus.Rejected;

            leaveRequest.RejectedBy =
                rejectedBy;

            leaveRequest.RejectedAt =
                DateTime.Now;

            leaveRequest.ManagerComment =
                request.Comment.Trim();

            leaveRequest.UpdatedAt =
                DateTime.Now;


            await _leaveRepository
                .UpdateAsync(leaveRequest);


            return MapToDto(leaveRequest);
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


            // Employee can only cancel own leave
            if (leaveRequest.EmployeeId != employeeId)
            {
                throw new Exception(
                    "You cannot cancel another employee's leave.");
            }


            // Initial rule:
            // Only Pending leave can be cancelled
            if (leaveRequest.Status !=
                (short)LeaveRequestStatus.Pending)
            {
                throw new Exception(
                    "Only pending leave requests can be cancelled.");
            }


            leaveRequest.Status =
                (short)LeaveRequestStatus.Cancelled;

            leaveRequest.CancelledAt =
                DateTime.Now;

            leaveRequest.UpdatedAt =
                DateTime.Now;


            await _leaveRepository
                .UpdateAsync(leaveRequest);


            return MapToDto(leaveRequest);
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