using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HRMS.API.Data;
using HRMS.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Repository.Notification
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly DefaultContext _context;

        public NotificationRepository(DefaultContext context)
        {
            _context = context;
        }

        public async Task<Entities.Notification> CreateAsync(Entities.Notification notification)
        {
            notification.CreatedAt = DateTime.UtcNow;
            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
            return notification;
        }

        public async Task<Entities.Notification?> GetByIdAsync(long id)
        {
            return await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task<IEnumerable<Entities.Notification>> GetByUserIdAsync(int userId, int limit = 50)
        {
            return await _context.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task MarkAsReadAsync(long id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification != null && !notification.IsRead)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAllAsReadAsync(int userId)
        {
            var unread = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            if (unread.Any())
            {
                var now = DateTime.UtcNow;
                foreach (var item in unread)
                {
                    item.IsRead = true;
                    item.ReadAt = now;
                }
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int?> FindUserIdByEmployeeIdAsync(long employeeId)
        {
            var user = await _context.AppUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.EmployeeId == (int)employeeId);

            return user?.UserId;
        }
    }
}
