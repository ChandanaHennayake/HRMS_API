using System.Collections.Generic;
using System.Threading.Tasks;
using HRMS.API.Entities;

namespace HRMS.API.Repository.Notification
{
    public interface INotificationRepository
    {
        Task<Entities.Notification> CreateAsync(Entities.Notification notification);
        Task<Entities.Notification?> GetByIdAsync(long id);
        Task<IEnumerable<Entities.Notification>> GetByUserIdAsync(int userId, int limit = 50);
        Task<int> GetUnreadCountAsync(int userId);
        Task MarkAsReadAsync(long id);
        Task MarkAllAsReadAsync(int userId);
        Task<int?> FindUserIdByEmployeeIdAsync(long employeeId);
    }
}
