using HRMS.API.DTOs.Holiday;
using HRMS.API.Entities;
using HRMS.API.Repository.Holiday;

namespace HRMS.API.Service.Holiday
{
    public class HolidayService : IHolidayService
    {
        private readonly IHolidayRepository _holidayRepository;

        public HolidayService(
            IHolidayRepository holidayRepository)
        {
            _holidayRepository = holidayRepository;
        }

        public async Task<HolidayDto> CreateAsync(
            CreateHolidayDto request)
        {
            if (await _holidayRepository.ExistsAsync(
                request.CompanyId,
                request.HolidayDate))
            {
                throw new Exception("Holiday already exists.");
            }

            var holiday = new Entities.Holiday
            {
                CompanyId = request.CompanyId,
                HolidayDate = request.HolidayDate,
                HolidayName = request.HolidayName,
                Description = request.Description,
                IsActive = true,
                CreatedAt = DateTime.Now,
                CreatedBy = request.CreatedBy
            };

            await _holidayRepository.CreateAsync(holiday);

            return await _holidayRepository.GetByIdAsync(holiday.Id)
                   ?? throw new Exception("Holiday creation failed.");
        }

        public async Task<IEnumerable<HolidayDto>> GetAllAsync()
        {
            return await _holidayRepository.GetAllAsync();
        }

        public async Task<HolidayDto?> GetByIdAsync(long id)
        {
            return await _holidayRepository.GetByIdAsync(id);
        }

        public async Task<HolidayDto?> UpdateAsync(
            long id,
            UpdateHolidayDto request)
        {
            var holiday = await _holidayRepository.GetEntityByIdAsync(id);

            if (holiday == null)
                return null;

            if (await _holidayRepository.ExistsAsync(
                holiday.CompanyId,
                request.HolidayDate,
                id))
            {
                throw new Exception("Holiday already exists.");
            }

            holiday.HolidayDate = request.HolidayDate;
            holiday.HolidayName = request.HolidayName;
            holiday.Description = request.Description;
            holiday.IsActive = request.IsActive;

            await _holidayRepository.UpdateAsync(holiday);

            return await _holidayRepository.GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(
            long id,
            long? deletedBy)
        {
            var holiday = await _holidayRepository.GetEntityByIdAsync(id);

            if (holiday == null)
                return false;

            await _holidayRepository.DeleteAsync(holiday);

            return true;
        }

        public async Task<bool> IsHolidayAsync(
            long companyId,
            DateOnly date)
        {
            return await _holidayRepository.IsHolidayAsync(
                companyId,
                date);
        }
    }
}