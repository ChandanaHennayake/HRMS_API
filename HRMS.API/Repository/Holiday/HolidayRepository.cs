using HRMS.API.Data;
using HRMS.API.DTOs.Holiday;
using Microsoft.EntityFrameworkCore;

using HolidayEntity = HRMS.API.Entities.Holiday;

namespace HRMS.API.Repository.Holiday
{
    public class HolidayRepository : IHolidayRepository
    {
        private readonly DefaultContext _context;

        public HolidayRepository(DefaultContext context)
        {
            _context = context;
        }


        // =====================================================
        // CREATE
        // =====================================================

        public async Task<HolidayEntity> CreateAsync(
            HolidayEntity holiday)
        {
            await _context.Holidays.AddAsync(holiday);

            await _context.SaveChangesAsync();

            return holiday;
        }


        // =====================================================
        // GET ALL
        // =====================================================

        public async Task<IEnumerable<HolidayDto>> GetAllAsync()
        {
            return await _context.Holidays
                .AsNoTracking()
                .OrderByDescending(x => x.HolidayDate)
                .Select(x => new HolidayDto
                {
                    Id = x.Id,
                    CompanyId = x.CompanyId,
                    HolidayDate = x.HolidayDate,
                    HolidayName = x.HolidayName,
                    Description = x.Description,
                    IsActive = x.IsActive
                })
                .ToListAsync();
        }


        // =====================================================
        // GET BY ID - DTO
        // =====================================================

        public async Task<HolidayDto?> GetByIdAsync(
            long id)
        {
            return await _context.Holidays
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new HolidayDto
                {
                    Id = x.Id,
                    CompanyId = x.CompanyId,
                    HolidayDate = x.HolidayDate,
                    HolidayName = x.HolidayName,
                    Description = x.Description,
                    IsActive = x.IsActive
                })
                .FirstOrDefaultAsync();
        }


        // =====================================================
        // GET ENTITY BY ID
        //
        // Used for Update/Delete
        // =====================================================

        public async Task<HolidayEntity?> GetEntityByIdAsync(
            long id)
        {
            return await _context.Holidays
                .FirstOrDefaultAsync(x => x.Id == id);
        }


        // =====================================================
        // UPDATE
        // =====================================================

        public async Task UpdateAsync(
            HolidayEntity holiday)
        {
            _context.Holidays.Update(holiday);

            await _context.SaveChangesAsync();
        }


        // =====================================================
        // DELETE
        // =====================================================

        public async Task DeleteAsync(
            HolidayEntity holiday)
        {
            _context.Holidays.Remove(holiday);

            await _context.SaveChangesAsync();
        }


        // =====================================================
        // CHECK WHETHER DATE IS A HOLIDAY
        //
        // Used by:
        // Attendance finalization
        // Leave calculation
        // =====================================================

        public async Task<bool> IsHolidayAsync(
            long companyId,
            DateOnly date)
        {
            return await _context.Holidays
                .AsNoTracking()
                .AnyAsync(x =>
                    x.CompanyId == companyId &&
                    x.HolidayDate == date &&
                    x.IsActive);
        }


        // =====================================================
        // CHECK DUPLICATE HOLIDAY
        //
        // excludeId is used during Update
        // =====================================================

        public async Task<bool> ExistsAsync(
            long companyId,
            DateOnly date,
            long? excludeId = null)
        {
            return await _context.Holidays
                .AsNoTracking()
                .AnyAsync(x =>
                    x.CompanyId == companyId &&
                    x.HolidayDate == date &&
                    (!excludeId.HasValue ||
                     x.Id != excludeId.Value));
        }
    }
}