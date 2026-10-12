using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HRMS.API.Data;
using HRMS.API.Entities;
using HRMS.API.Enums;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Repository.Leave
{
    public class LeaveBalanceRepository : ILeaveBalanceRepository
    {
        private readonly DefaultContext _context;

        public LeaveBalanceRepository(DefaultContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<EmployeeLeaveBalance>> GetBalancesAsync(long employeeId, int year)
        {
            return await _context.EmployeeLeaveBalances
                .AsNoTracking()
                .Include(b => b.LeaveType)
                .Where(b => b.EmployeeId == employeeId && b.Year == year)
                .OrderBy(b => b.LeaveTypeId)
                .ToListAsync();
        }

        public async Task<EmployeeLeaveBalance?> GetBalanceAsync(long employeeId, long leaveTypeId, int year)
        {
            return await _context.EmployeeLeaveBalances
                .Include(b => b.LeaveType)
                .FirstOrDefaultAsync(b => b.EmployeeId == employeeId && b.LeaveTypeId == leaveTypeId && b.Year == year);
        }

        public async Task<EmployeeLeaveBalance> CreateAsync(EmployeeLeaveBalance balance)
        {
            await _context.EmployeeLeaveBalances.AddAsync(balance);
            await _context.SaveChangesAsync();
            return balance;
        }

        public async Task UpdateAsync(EmployeeLeaveBalance balance)
        {
            balance.UpdatedAt = DateTime.Now;
            _context.EmployeeLeaveBalances.Update(balance);
            await _context.SaveChangesAsync();
        }

        public async Task EnsureBalancesAsync(long employeeId, int year, long companyId)
        {
            // Get all active leave types for this company (or global if companyId matches or zero)
            var leaveTypes = await _context.LeaveTypes
                .AsNoTracking()
                .Where(lt => lt.IsActive && (lt.CompanyId == companyId || lt.CompanyId == 0))
                .ToListAsync();

            if (!leaveTypes.Any())
            {
                // Fallback to any active leave types
                leaveTypes = await _context.LeaveTypes
                    .AsNoTracking()
                    .Where(lt => lt.IsActive)
                    .ToListAsync();
            }

            var existingBalances = await _context.EmployeeLeaveBalances
                .Where(b => b.EmployeeId == employeeId && b.Year == year)
                .ToListAsync();

            var existingLeaveTypeIds = existingBalances.Select(b => b.LeaveTypeId).ToHashSet();

            var newBalances = new List<EmployeeLeaveBalance>();

            foreach (var lt in leaveTypes)
            {
                if (!existingLeaveTypeIds.Contains(lt.Id))
                {
                    decimal defaultAllocated;
                    var name = lt.Name.ToLowerInvariant();
                    var code = lt.Code.ToLowerInvariant();

                    if (!lt.IsPaid)
                    {
                        defaultAllocated = 0.0m;
                    }
                    else if (name.Contains("annual") || code.Contains("ann"))
                    {
                        defaultAllocated = 14.0m;
                    }
                    else if (name.Contains("casual") || code.Contains("cas"))
                    {
                        defaultAllocated = 7.0m;
                    }
                    else if (name.Contains("medical") || name.Contains("sick") || code.Contains("med"))
                    {
                        defaultAllocated = 7.0m;
                    }
                    else
                    {
                        defaultAllocated = 7.0m;
                    }

                    // Count existing approved and pending leaves for this year and leave type to preserve historical consistency
                    var yearLeaves = await _context.LeaveRequests
                        .AsNoTracking()
                        .Where(lr => lr.EmployeeId == employeeId && lr.LeaveTypeId == lt.Id && lr.FromDate.Year == year)
                        .ToListAsync();

                    var used = yearLeaves.Where(l => l.Status == (short)LeaveRequestStatus.Approved).Sum(l => l.TotalDays);
                    var pending = yearLeaves.Where(l => l.Status == (short)LeaveRequestStatus.Pending).Sum(l => l.TotalDays);

                    newBalances.Add(new EmployeeLeaveBalance
                    {
                        EmployeeId = employeeId,
                        LeaveTypeId = lt.Id,
                        Year = year,
                        AllocatedDays = defaultAllocated,
                        UsedDays = used,
                        PendingDays = pending,
                        CreatedAt = DateTime.Now
                    });
                }
            }

            if (newBalances.Any())
            {
                await _context.EmployeeLeaveBalances.AddRangeAsync(newBalances);
                await _context.SaveChangesAsync();
            }
        }
    }
}
