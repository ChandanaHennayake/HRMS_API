using HRMS.API.DTOs.Branch;
using HRMS.API.Interfaces.Repositories;

using BranchEntity = HRMS.API.Entities.Branch;

namespace HRMS.API.Service.Branch
{
    public class BranchService : IBranchService
    {
        private readonly IBranchRepository _branchRepository;

        public BranchService(
            IBranchRepository branchRepository)
        {
            _branchRepository = branchRepository;
        }


        // =====================================================
        // CREATE
        // =====================================================

        public async Task<BranchDto> CreateAsync(
            CreateBranchDto request)
        {
            if (request.CompanyId <= 0)
            {
                throw new Exception(
                    "Company is required.");
            }

            if (string.IsNullOrWhiteSpace(
                request.BranchName))
            {
                throw new Exception(
                    "Branch name is required.");
            }

            if (request.GeofenceRadiusMeters <= 0)
            {
                throw new Exception(
                    "Geofence radius must be greater than zero.");
            }


            // Check duplicate branch code
            if (!string.IsNullOrWhiteSpace(
                request.BranchCode))
            {
                var codeExists =
                    await _branchRepository
                        .BranchCodeExistsAsync(
                            request.CompanyId,
                            request.BranchCode.Trim());

                if (codeExists)
                {
                    throw new Exception(
                        "Branch code already exists.");
                }
            }


            var branch = new BranchEntity
            {
                companyid =
                    request.CompanyId,

                branchcode =
                    request.BranchCode?.Trim(),

                branchname =
                    request.BranchName.Trim(),

                address =
                    request.Address?.Trim(),

                phone =
                    request.Phone?.Trim(),

                Latitude =
                    request.Latitude,

                Longitude =
                    request.Longitude,

                GeofenceRadiusMeters =
                    request.GeofenceRadiusMeters,

                isactive =
                    request.IsActive,

                createddate =
                    DateTime.Now
            };


            await _branchRepository
                .CreateAsync(branch);


            return MapToDto(branch);
        }


        // =====================================================
        // GET ALL
        // =====================================================

        public async Task<IEnumerable<BranchDto>>
            GetAllAsync()
        {
            var branches =
                await _branchRepository.GetAllAsync();


            return branches.Select(MapToDto);
        }


        // =====================================================
        // GET BY ID
        // =====================================================

        public async Task<BranchDto?>
            GetByIdAsync(int id)
        {
            var branch =
                await _branchRepository
                    .GetByIdAsync(id);


            if (branch == null)
            {
                return null;
            }


            return MapToDto(branch);
        }


        // =====================================================
        // UPDATE
        // =====================================================

        public async Task<BranchDto> UpdateAsync(
            int id,
            UpdateBranchDto request)
        {
            var branch =
                await _branchRepository
                    .GetEntityByIdAsync(id);


            if (branch == null)
            {
                throw new Exception(
                    "Branch not found.");
            }


            if (string.IsNullOrWhiteSpace(
                request.BranchName))
            {
                throw new Exception(
                    "Branch name is required.");
            }


            if (request.GeofenceRadiusMeters <= 0)
            {
                throw new Exception(
                    "Geofence radius must be greater than zero.");
            }


            // Check duplicate code
            if (!string.IsNullOrWhiteSpace(
                request.BranchCode))
            {
                var codeExists =
                    await _branchRepository
                        .BranchCodeExistsAsync(
                            branch.companyid,
                            request.BranchCode.Trim(),
                            id);


                if (codeExists)
                {
                    throw new Exception(
                        "Branch code already exists.");
                }
            }


            branch.branchcode =
                request.BranchCode?.Trim();

            branch.branchname =
                request.BranchName.Trim();

            branch.address =
                request.Address?.Trim();

            branch.phone =
                request.Phone?.Trim();

            branch.Latitude =
                request.Latitude;

            branch.Longitude =
                request.Longitude;

            branch.GeofenceRadiusMeters =
                request.GeofenceRadiusMeters;

            branch.isactive =
                request.IsActive;


            await _branchRepository
                .UpdateAsync(branch);


            return MapToDto(branch);
        }


        // =====================================================
        // DELETE / DEACTIVATE
        // =====================================================

        public async Task DeleteAsync(int id)
        {
            var branch =
                await _branchRepository
                    .GetEntityByIdAsync(id);


            if (branch == null)
            {
                throw new Exception(
                    "Branch not found.");
            }


            await _branchRepository
                .DeleteAsync(branch);
        }


        // =====================================================
        // MAPPER
        // =====================================================

        private static BranchDto MapToDto(
            BranchEntity branch)
        {
            return new BranchDto
            {
                BranchId =
                    branch.branchid,

                CompanyId =
                    branch.companyid,

                BranchCode =
                    branch.branchcode,

                BranchName =
                    branch.branchname,

                Address =
                    branch.address,

                Phone =
                    branch.phone,

                Latitude =
                    branch.Latitude,

                Longitude =
                    branch.Longitude,

                GeofenceRadiusMeters =
                    branch.GeofenceRadiusMeters,

                IsActive =
                    branch.isactive,

                CreatedDate =
                    branch.createddate
            };
        }
    }
}