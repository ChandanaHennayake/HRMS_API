using HRMS.API.DTOs.Branch;
using HRMS.API.Entities;
using HRMS.API.Interfaces.Repositories;
using HRMS.API.Repository.UserDetails;

using BranchEntity = HRMS.API.Entities.Branch;

namespace HRMS.API.Service.Branch
{
    public class BranchService : IBranchService
    {
        private readonly IBranchRepository _branchRepository;
        private readonly IUserRepository _userRepository;

        public BranchService(
            IBranchRepository branchRepository,
            IUserRepository userRepository)
        {
            _branchRepository = branchRepository;
            _userRepository = userRepository;
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

            if (request.Latitude.HasValue && (request.Latitude.Value < -90m || request.Latitude.Value > 90m))
            {
                throw new Exception(
                    "Latitude must be between -90 and 90 degrees.");
            }

            if (request.Longitude.HasValue && (request.Longitude.Value < -180m || request.Longitude.Value > 180m))
            {
                throw new Exception(
                    "Longitude must be between -180 and 180 degrees.");
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

            if (request.Latitude.HasValue && (request.Latitude.Value < -90m || request.Latitude.Value > 90m))
            {
                throw new Exception(
                    "Latitude must be between -90 and 90 degrees.");
            }

            if (request.Longitude.HasValue && (request.Longitude.Value < -180m || request.Longitude.Value > 180m))
            {
                throw new Exception(
                    "Longitude must be between -180 and 180 degrees.");
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


        // =====================================================
        // MANAGER ASSIGNMENTS
        // =====================================================

        public async Task<IEnumerable<BranchManagerDto>> GetManagersByBranchAsync(int branchId)
        {
            var assignments = await _branchRepository.GetManagersByBranchAsync(branchId);
            return assignments.Select(a => new BranchManagerDto
            {
                UserBranchId = a.userbranchid,
                UserId = a.userid,
                Username = a.user?.Username ?? string.Empty,
                FullName = $"{a.user?.FirstName} {a.user?.LastName}".Trim(),
                Email = a.user?.Email ?? string.Empty,
                BranchId = a.branchid,
                BranchName = a.branch?.branchname ?? string.Empty
            });
        }

        public async Task<IEnumerable<BranchDto>> GetBranchesByManagerAsync(int userId)
        {
            var assignments = await _branchRepository.GetBranchesByManagerAsync(userId);
            return assignments.Where(a => a.branch != null).Select(a => MapToDto(a.branch!));
        }

        public async Task<BranchManagerDto> AssignManagerAsync(AssignManagerDto request)
        {
            if (request.UserId <= 0)
                throw new Exception("Valid user ID is required.");
            if (request.BranchId <= 0)
                throw new Exception("Valid branch ID is required.");

            var branch = await _branchRepository.GetByIdAsync(request.BranchId);
            if (branch == null)
                throw new Exception("Branch not found.");

            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
                throw new Exception("User not found.");

            var existing = await _branchRepository.GetUserBranchAsync(request.UserId, request.BranchId);
            if (existing != null)
                throw new Exception("Manager is already assigned to this branch.");

            var userBranch = new UserBranch
            {
                userid = request.UserId,
                branchid = request.BranchId
            };

            var created = await _branchRepository.AssignManagerAsync(userBranch);

            return new BranchManagerDto
            {
                UserBranchId = created.userbranchid,
                UserId = user.UserId,
                Username = user.Username,
                FullName = $"{user.FirstName} {user.LastName}".Trim(),
                Email = user.Email,
                BranchId = branch.branchid,
                BranchName = branch.branchname
            };
        }

        public async Task RemoveManagerAsync(int userId, int branchId)
        {
            var existing = await _branchRepository.GetUserBranchAsync(userId, branchId);
            if (existing == null)
                throw new Exception("Manager is not assigned to this branch.");

            await _branchRepository.RemoveManagerAsync(existing);
        }
    }
}