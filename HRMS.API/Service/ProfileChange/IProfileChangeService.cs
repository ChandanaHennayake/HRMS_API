using System.Collections.Generic;
using System.Threading.Tasks;
using HRMS.API.DTOs.ProfileChange;

namespace HRMS.API.Service.ProfileChange
{
    public interface IProfileChangeService
    {
        Task<ProfileChangeRequestDto> SubmitRequestAsync(SubmitProfileChangeRequestDto dto, int currentUserId);
        Task<List<ProfileChangeRequestDto>> GetMyRequestsAsync(long employeeId, int? userId = null);
        Task<List<ProfileChangeRequestDto>> GetAllRequestsAsync(long? companyId = null, long? branchId = null, short? status = null);
        Task<ProfileChangeRequestDto?> GetByIdAsync(long id);
        Task<ProfileChangeRequestDto> ApproveRequestAsync(long id, ReviewProfileChangeDto dto, int reviewerUserId);
        Task<ProfileChangeRequestDto> RejectRequestAsync(long id, ReviewProfileChangeDto dto, int reviewerUserId);
        Task<bool> CancelRequestAsync(long id, int currentUserId);
    }
}
