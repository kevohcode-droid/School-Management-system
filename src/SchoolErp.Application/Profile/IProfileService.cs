using System.Threading;
using System.Threading.Tasks;
using SchoolErp.Application.Profile.Dtos;

namespace SchoolErp.Application.Profile
{
    public interface IProfileService
    {
        Task<ProfileDto?> GetProfileAsync(string userId, CancellationToken ct = default);
        Task<bool> UpdateProfileAsync(string userId, UpdateProfileRequest request, CancellationToken ct = default);
    }
}
