using HRMS.API.DTOs.Holiday;

namespace HRMS.API.Service.Holiday
{
    public interface IHolidayService
    {
        Task<HolidayDto> CreateAsync(
            CreateHolidayDto request);

        Task<IEnumerable<HolidayDto>> GetAllAsync();

        Task<HolidayDto?> GetByIdAsync(
            long id);

        Task<HolidayDto?> UpdateAsync(
            long id,
            UpdateHolidayDto request);

        Task<bool> DeleteAsync(
            long id,
            long? deletedBy);

        Task<bool> IsHolidayAsync(
            long companyId,
            DateOnly date);
    }
}