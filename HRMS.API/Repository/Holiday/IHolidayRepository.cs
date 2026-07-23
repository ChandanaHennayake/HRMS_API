using HRMS.API.DTOs.Holiday;

using HolidayEntity = HRMS.API.Entities.Holiday;

namespace HRMS.API.Repository.Holiday
{
    public interface IHolidayRepository
    {
        Task<HolidayEntity> CreateAsync(
            HolidayEntity holiday);

        Task<IEnumerable<HolidayDto>> GetAllAsync();

        Task<HolidayDto?> GetByIdAsync(
            long id);

        Task<HolidayEntity?> GetEntityByIdAsync(
            long id);

        Task UpdateAsync(
            HolidayEntity holiday);

        Task DeleteAsync(
            HolidayEntity holiday);

        // Used by Attendance / Leave
        Task<bool> IsHolidayAsync(
            long companyId,
            DateOnly date);

        // Prevent duplicate holiday dates
        Task<bool> ExistsAsync(
            long companyId,
            DateOnly date,
            long? excludeId = null);
    }
}