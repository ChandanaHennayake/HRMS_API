using HRMS.API.Data;
using HRMS.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Repository.Leave
{
    public class LeaveRepository : ILeaveRepository
    {
        private readonly DefaultContext _context;

        public LeaveRepository(DefaultContext context)
        {
            _context = context;
        }

        // =====================================================
        // CREATE
        // =====================================================

        public async Task<LeaveRequest> CreateAsync(
            LeaveRequest leaveRequest)
        {
            await _context.LeaveRequests.AddAsync(leaveRequest);

            await _context.SaveChangesAsync();

            return leaveRequest;
        }

        // =====================================================
        // GET BY ID
        // =====================================================

        public async Task<LeaveRequest?> GetByIdAsync(long id)
        {
            return await _context.LeaveRequests
                .AsNoTracking()
                .Include(x => x.LeaveType)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        // =====================================================
        // EMPLOYEE LEAVE HISTORY
        // =====================================================

        public async Task<IEnumerable<LeaveRequest>>
            GetEmployeeLeavesAsync(long employeeId)
        {
            return await _context.LeaveRequests
                .AsNoTracking()
                .Include(x => x.LeaveType)
                .Where(x => x.EmployeeId == employeeId)
                .OrderByDescending(x => x.AppliedAt)
                .ToListAsync();
        }

        // =====================================================
        // GET PENDING LEAVES
        // 0 = Pending
        // =====================================================

        public async Task<IEnumerable<LeaveRequest>>
            GetPendingLeavesAsync(int? branchId = null)
        {
            var query = _context.LeaveRequests
                .AsNoTracking()
                .Include(x => x.Employee)
                .Include(x => x.LeaveType)
                .Where(x => x.Status == 0);

            if (branchId.HasValue && branchId.Value > 0)
            {
                query = query.Where(x => x.Employee != null && x.Employee.BranchId == branchId.Value);
            }

            return await query
                .OrderByDescending(x => x.AppliedAt)
                .ToListAsync();
        }

        // =====================================================
        // CHECK OVERLAPPING LEAVE
        //
        // 0 = Pending
        // 1 = Approved
        // =====================================================

        public async Task<bool> HasOverlappingLeaveAsync(
            long employeeId,
            DateOnly fromDate,
            DateOnly toDate)
        {
            return await _context.LeaveRequests
                .AsNoTracking()
                .AnyAsync(x =>
                    x.EmployeeId == employeeId &&
                    (
                        x.Status == 0 ||
                        x.Status == 1
                    ) &&
                    x.FromDate <= toDate &&
                    x.ToDate >= fromDate
                );
        }

        // =====================================================
        // CHECK APPROVED LEAVE FOR DATE
        //
        // Used when daily attendance is finalized.
        // =====================================================

        public async Task<bool> HasApprovedLeaveAsync(
            long employeeId,
            DateOnly date)
        {
            return await _context.LeaveRequests
                .AsNoTracking()
                .AnyAsync(x =>
                    x.EmployeeId == employeeId &&
                    x.Status == 1 &&
                    x.FromDate <= date &&
                    x.ToDate >= date
                );
        }

        // =====================================================
        // UPDATE
        // Approve / Reject / Cancel
        // =====================================================

        public async Task UpdateAsync(
            LeaveRequest leaveRequest)
        {
            var existing = await _context.LeaveRequests.FindAsync(leaveRequest.Id);
            if (existing != null)
            {
                existing.Status = leaveRequest.Status;
                existing.ApprovedBy = leaveRequest.ApprovedBy;
                existing.ApprovedAt = leaveRequest.ApprovedAt;
                existing.RejectedBy = leaveRequest.RejectedBy;
                existing.RejectedAt = leaveRequest.RejectedAt;
                existing.ManagerComment = leaveRequest.ManagerComment;
                existing.CancelledAt = leaveRequest.CancelledAt;
                existing.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
            else
            {
                _context.Entry(leaveRequest).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
        }
    }
}