using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HRMS.API.DTOs.Attendance;
using HRMS.API.Entities;
using HRMS.API.Interfaces.Repositories;
using HRMS.API.Repository.Attendance;
using HRMS.API.Repository.UserDetails;
using HRMS.API.Service.Audit;
using HRMS.API.Service.Email;
using HRMS.API.Data;
using HRMS.API.Service.Notification;
using Microsoft.EntityFrameworkCore;
using AttendanceEntity = HRMS.API.Entities.Attendance;

namespace HRMS.API.Service.Attendance
{
    public class AttendanceCorrectionService : IAttendanceCorrectionService
    {
        private readonly IAttendanceCorrectionRepository _correctionRepo;
        private readonly IAttendanceRepository _attendanceRepo;
        private readonly IEmployeeRepository _employeeRepo;
        private readonly IBranchRepository _branchRepo;
        private readonly IUserRepository _userRepo;
        private readonly INotificationService _notificationService;
        private readonly IAuditService _auditService;
        private readonly IEmailService _emailService;
        private readonly DefaultContext _context;

        public AttendanceCorrectionService(
            IAttendanceCorrectionRepository correctionRepo,
            IAttendanceRepository attendanceRepo,
            IEmployeeRepository employeeRepo,
            IBranchRepository branchRepo,
            IUserRepository userRepo,
            INotificationService notificationService,
            IAuditService auditService,
            IEmailService emailService,
            DefaultContext context)
        {
            _correctionRepo = correctionRepo;
            _attendanceRepo = attendanceRepo;
            _employeeRepo = employeeRepo;
            _branchRepo = branchRepo;
            _userRepo = userRepo;
            _notificationService = notificationService;
            _auditService = auditService;
            _emailService = emailService;
            _context = context;
        }

        public async Task<AttendanceCorrectionDto> SubmitCorrectionAsync(
            long employeeId,
            CreateAttendanceCorrectionDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                throw new Exception("Reason for attendance correction is required.");
            }

            var employee = await _employeeRepo.GetEntityByIdAsync(employeeId);
            if (employee == null)
            {
                throw new Exception("Employee not found.");
            }

            if (!employee.IsActive)
            {
                throw new Exception("Employee is inactive.");
            }

            if (request.AttendanceDate > DateOnly.FromDateTime(DateTime.Today))
            {
                throw new Exception("Cannot request attendance correction for future dates.");
            }

            var hasPending = await _correctionRepo.HasPendingRequestAsync(employeeId, request.AttendanceDate);
            if (hasPending)
            {
                throw new Exception("A pending correction request already exists for this date.");
            }

            var existingAttendance = await _attendanceRepo.GetTodayAttendanceAsync(employeeId, request.AttendanceDate);

            DateTime? reqCheckIn = request.RequestedCheckInTime;
            DateTime? reqCheckOut = request.RequestedCheckOutTime;

            if (reqCheckIn == null && reqCheckOut == null)
            {
                throw new Exception("Please specify requested check-in time or check-out time.");
            }

            if (reqCheckIn.HasValue && reqCheckOut.HasValue && reqCheckOut <= reqCheckIn)
            {
                throw new Exception("Requested check-out time must be after check-in time.");
            }

            var correction = new AttendanceCorrection
            {
                AttendanceId = existingAttendance?.Id,
                EmployeeId = employeeId,
                AttendanceDate = request.AttendanceDate,
                RequestedCheckInTime = reqCheckIn,
                RequestedCheckOutTime = reqCheckOut,
                Reason = request.Reason.Trim(),
                Status = 0, // Pending
                CreatedAt = HRMS.API.Helpers.DateTimeHelper.Now
            };

            await _correctionRepo.AddAsync(correction);

            // Notify branch managers and admins of this pending request
            try
            {
                var managers = await _context.AppUsers
                    .Where(u => (u.RoleId == 1 || (u.RoleId == 2 && u.BranchId == employee.BranchId)) && u.IsActive == true)
                    .ToListAsync();

                foreach (var mgr in managers)
                {
                    await _notificationService.SendNotificationAsync(new HRMS.API.DTOs.Notification.CreateNotificationDto
                    {
                        UserId = mgr.UserId,
                        Title = "Attendance Correction Request",
                        Message = $"{employee.FirstName} {employee.LastName} requested attendance correction for {request.AttendanceDate:dd MMM yyyy}.",
                        Type = "AttendanceCorrection",
                        ReferenceId = correction.Id,
                        ReferenceType = "AttendanceCorrection"
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AttendanceCorrection] Manager notification notice: {ex.Message}");
            }

            return await MapToDtoAsync(correction);
        }

        public async Task<IEnumerable<AttendanceCorrectionDto>> GetEmployeeCorrectionsAsync(long employeeId)
        {
            var list = await _correctionRepo.GetByEmployeeIdAsync(employeeId);
            var results = new List<AttendanceCorrectionDto>();
            foreach (var item in list)
            {
                results.Add(await MapToDtoAsync(item));
            }
            return results;
        }

        public async Task<IEnumerable<AttendanceCorrectionDto>> GetPendingCorrectionsAsync(int? branchId)
        {
            var list = await _correctionRepo.GetPendingAsync(branchId);
            var results = new List<AttendanceCorrectionDto>();
            foreach (var item in list)
            {
                results.Add(await MapToDtoAsync(item));
            }
            return results;
        }

        public async Task<AttendanceCorrectionDto> ReviewCorrectionAsync(
            long id,
            long reviewerUserId,
            ReviewAttendanceCorrectionDto review)
        {
            var correction = await _correctionRepo.GetByIdAsync(id);
            if (correction == null)
            {
                throw new Exception("Correction request not found.");
            }

            if (correction.Status != 0)
            {
                throw new Exception("This correction request has already been reviewed.");
            }

            if (review.Status != 1 && review.Status != 2)
            {
                throw new Exception("Invalid review status. Must be 1 (Approved) or 2 (Rejected).");
            }

            if (review.Status == 2 && string.IsNullOrWhiteSpace(review.ReviewerComment))
            {
                throw new Exception("A comment is mandatory when rejecting a correction request.");
            }

            var now = HRMS.API.Helpers.DateTimeHelper.Now;
            correction.ReviewedBy = reviewerUserId;
            correction.ReviewedAt = now;
            correction.ReviewerComment = review.ReviewerComment?.Trim();
            correction.UpdatedAt = now;
            correction.Status = review.Status;

            if (review.Status == 1) // Approved
            {
                var attendance = correction.AttendanceId.HasValue
                    ? await _attendanceRepo.GetByIdAsync(correction.AttendanceId.Value)
                    : await _attendanceRepo.GetTodayAttendanceAsync(correction.EmployeeId, correction.AttendanceDate);

                if (attendance == null)
                {
                    attendance = new AttendanceEntity
                    {
                        EmployeeId = correction.EmployeeId,
                        AttendanceDate = correction.AttendanceDate,
                        CreatedAt = now
                    };
                    await _attendanceRepo.AddAsync(attendance);
                }

                if (correction.RequestedCheckInTime.HasValue)
                {
                    attendance.CheckInTime = correction.RequestedCheckInTime.Value;
                }

                if (correction.RequestedCheckOutTime.HasValue)
                {
                    attendance.CheckOutTime = correction.RequestedCheckOutTime.Value;
                }

                // Recalculate metrics
                if (attendance.CheckInTime.HasValue && attendance.CheckOutTime.HasValue)
                {
                    var worked = attendance.CheckOutTime.Value - attendance.CheckInTime.Value;
                    attendance.WorkedMinutes = Math.Max(0, (int)worked.TotalMinutes);
                }

                // Check-in punctuality (08:30 AM)
                if (attendance.CheckInTime.HasValue)
                {
                    var normalStart = new TimeOnly(8, 30);
                    var actualStart = TimeOnly.FromDateTime(attendance.CheckInTime.Value);
                    if (actualStart > normalStart)
                    {
                        attendance.IsLate = true;
                        attendance.LateMinutes = (int)(actualStart.ToTimeSpan() - normalStart.ToTimeSpan()).TotalMinutes;
                    }
                    else
                    {
                        attendance.IsLate = false;
                        attendance.LateMinutes = 0;
                    }
                }

                // Check-out / OT / early departure (07:30 PM / 19:30)
                if (attendance.CheckOutTime.HasValue)
                {
                    var normalEnd = new TimeOnly(19, 30);
                    var actualEnd = TimeOnly.FromDateTime(attendance.CheckOutTime.Value);

                    if (actualEnd < normalEnd)
                    {
                        attendance.IsEarlyDeparture = true;
                        attendance.EarlyDepartureMinutes = (int)(normalEnd.ToTimeSpan() - actualEnd.ToTimeSpan()).TotalMinutes;
                        attendance.OTMinutes = 0;
                    }
                    else
                    {
                        attendance.IsEarlyDeparture = false;
                        attendance.EarlyDepartureMinutes = 0;
                        attendance.OTMinutes = (int)(actualEnd.ToTimeSpan() - normalEnd.ToTimeSpan()).TotalMinutes;
                    }
                }

                attendance.Status = 1; // Present
                attendance.UpdatedAt = now;

                _attendanceRepo.Update(attendance);
                await _attendanceRepo.SaveChangesAsync();

                correction.AttendanceId = attendance.Id;
            }

            _correctionRepo.Update(correction);
            await _correctionRepo.SaveChangesAsync();

            await _notificationService.NotifyAttendanceCorrectionDecisionAsync(
                correction.EmployeeId,
                correction.AttendanceDate,
                review.Status == 1,
                review.ReviewerComment);

            var empForCorrectionEmail = await _employeeRepo.GetEntityByIdAsync(correction.EmployeeId);
            if (empForCorrectionEmail != null && !string.IsNullOrWhiteSpace(empForCorrectionEmail.Email))
            {
                await _emailService.SendAttendanceCorrectionDecisionEmailAsync(
                    empForCorrectionEmail.Email,
                    $"{empForCorrectionEmail.FirstName} {empForCorrectionEmail.LastName}".Trim(),
                    correction.AttendanceDate,
                    review.Status == 1,
                    review.ReviewerComment);
            }

            await _auditService.LogEntityActionAsync(
                reviewerUserId != 0 ? (int)reviewerUserId : null,
                null,
                review.Status == 1 ? "Approve" : "Reject",
                "AttendanceCorrection",
                correction.Id.ToString(),
                $"Reviewed attendance correction #{correction.Id} for Employee #{correction.EmployeeId} on {correction.AttendanceDate:yyyy-MM-dd}: {(review.Status == 1 ? "Approved" : "Rejected")}. Comment: {review.ReviewerComment}",
                severity: review.Status == 1 ? "Info" : "Warning");

            return await MapToDtoAsync(correction);
        }

        public async Task<bool> CancelCorrectionAsync(long id, long employeeId)
        {
            var correction = await _correctionRepo.GetByIdAsync(id);
            if (correction == null)
            {
                throw new Exception("Correction request not found.");
            }

            if (correction.EmployeeId != employeeId)
            {
                throw new Exception("You are not authorized to cancel this request.");
            }

            if (correction.Status != 0)
            {
                throw new Exception("Only pending requests can be cancelled.");
            }

            correction.Status = 3; // Cancelled
            correction.UpdatedAt = HRMS.API.Helpers.DateTimeHelper.Now;

            _correctionRepo.Update(correction);
            await _correctionRepo.SaveChangesAsync();

            return true;
        }

        private async Task<AttendanceCorrectionDto> MapToDtoAsync(AttendanceCorrection entity)
        {
            var emp = entity.Employee ?? await _employeeRepo.GetEntityByIdAsync(entity.EmployeeId);
            string empName = emp != null ? $"{emp.FirstName} {emp.LastName}".Trim() : string.Empty;
            string empCode = emp?.EmployeeCode ?? string.Empty;
            int branchId = (int)(emp?.BranchId ?? 0);

            string branchName = string.Empty;
            if (branchId > 0)
            {
                var branch = await _branchRepo.GetByIdAsync(branchId);
                branchName = branch?.branchname ?? string.Empty;
            }

            string reviewerName = string.Empty;
            if (entity.ReviewedBy.HasValue)
            {
                var revUser = await _userRepo.GetByIdAsync((int)entity.ReviewedBy.Value);
                if (revUser != null)
                {
                    reviewerName = $"{revUser.FirstName} {revUser.LastName}".Trim();
                    if (string.IsNullOrWhiteSpace(reviewerName))
                    {
                        reviewerName = revUser.Username;
                    }
                }
            }

            string statusName = entity.Status switch
            {
                0 => "Pending",
                1 => "Approved",
                2 => "Rejected",
                3 => "Cancelled",
                _ => "Unknown"
            };

            return new AttendanceCorrectionDto
            {
                Id = entity.Id,
                AttendanceId = entity.AttendanceId,
                EmployeeId = entity.EmployeeId,
                EmployeeName = empName,
                EmployeeCode = empCode,
                BranchId = branchId,
                BranchName = branchName,
                AttendanceDate = entity.AttendanceDate,
                OriginalCheckInTime = entity.Attendance?.CheckInTime,
                OriginalCheckOutTime = entity.Attendance?.CheckOutTime,
                RequestedCheckInTime = entity.RequestedCheckInTime,
                RequestedCheckOutTime = entity.RequestedCheckOutTime,
                Reason = entity.Reason,
                Status = entity.Status,
                StatusName = statusName,
                ReviewedBy = entity.ReviewedBy,
                ReviewedByName = reviewerName,
                ReviewedAt = entity.ReviewedAt,
                ReviewerComment = entity.ReviewerComment,
                CreatedAt = entity.CreatedAt
            };
        }
    }
}
