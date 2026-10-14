using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HRMS.API.Data;
using HRMS.API.DTOs.Audit;
using HRMS.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Repository.Audit
{
    public class AuditRepository : IAuditRepository
    {
        private readonly DefaultContext _context;

        public AuditRepository(DefaultContext context)
        {
            _context = context;
        }

        public async Task<AuditLog> CreateAsync(AuditLog log)
        {
            log.CreatedAt = DateTime.UtcNow;
            await _context.AuditLogs.AddAsync(log);
            await _context.SaveChangesAsync();
            return log;
        }

        public async Task<AuditLog?> GetByIdAsync(long id)
        {
            return await _context.AuditLogs
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<(IEnumerable<AuditLog> Items, int TotalCount)> GetPagedAsync(AuditLogQueryDto query)
        {
            var dbQuery = _context.AuditLogs.AsNoTracking().AsQueryable();

            if (query.UserId.HasValue && query.UserId.Value > 0)
            {
                dbQuery = dbQuery.Where(a => a.UserId == query.UserId.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Action))
            {
                var action = query.Action.Trim().ToLower();
                dbQuery = dbQuery.Where(a => a.Action.ToLower() == action);
            }

            if (!string.IsNullOrWhiteSpace(query.EntityName))
            {
                var entity = query.EntityName.Trim().ToLower();
                dbQuery = dbQuery.Where(a => a.EntityName.ToLower() == entity);
            }

            if (!string.IsNullOrWhiteSpace(query.Severity))
            {
                var severity = query.Severity.Trim().ToLower();
                dbQuery = dbQuery.Where(a => a.Severity.ToLower() == severity);
            }

            if (query.StartDate.HasValue)
            {
                var start = DateTime.SpecifyKind(query.StartDate.Value.Date, DateTimeKind.Utc);
                dbQuery = dbQuery.Where(a => a.CreatedAt >= start);
            }

            if (query.EndDate.HasValue)
            {
                var end = DateTime.SpecifyKind(query.EndDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
                dbQuery = dbQuery.Where(a => a.CreatedAt <= end);
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim().ToLower();
                dbQuery = dbQuery.Where(a =>
                    (a.Description != null && a.Description.ToLower().Contains(search)) ||
                    (a.UserName != null && a.UserName.ToLower().Contains(search)) ||
                    (a.EntityId != null && a.EntityId.ToLower().Contains(search)) ||
                    a.Action.ToLower().Contains(search) ||
                    a.EntityName.ToLower().Contains(search));
            }

            var totalCount = await dbQuery.CountAsync();

            var page = query.Page <= 0 ? 1 : query.Page;
            var pageSize = query.PageSize <= 0 ? 30 : Math.Min(query.PageSize, 100);

            var items = await dbQuery
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<AuditStatsDto> GetStatsAsync()
        {
            var total = await _context.AuditLogs.CountAsync();
            var security = await _context.AuditLogs.CountAsync(a => a.Severity == "Security");
            var warning = await _context.AuditLogs.CountAsync(a => a.Severity == "Warning");
            var critical = await _context.AuditLogs.CountAsync(a => a.Severity == "Critical");
            var info = await _context.AuditLogs.CountAsync(a => a.Severity == "Info");

            var topActionsGroup = await _context.AuditLogs
                .GroupBy(a => a.Action)
                .Select(g => new { Action = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .Take(5)
                .ToListAsync();

            var topActions = topActionsGroup.ToDictionary(g => g.Action, g => g.Count);

            return new AuditStatsDto
            {
                TotalLogs = total,
                SecurityCount = security,
                WarningCount = warning,
                CriticalCount = critical,
                InfoCount = info,
                TopActions = topActions
            };
        }

        public async Task<IEnumerable<string>> GetDistinctActionsAsync()
        {
            return await _context.AuditLogs
                .Select(a => a.Action)
                .Distinct()
                .OrderBy(a => a)
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetDistinctEntitiesAsync()
        {
            return await _context.AuditLogs
                .Select(a => a.EntityName)
                .Distinct()
                .OrderBy(e => e)
                .ToListAsync();
        }
    }
}
