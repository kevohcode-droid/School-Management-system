using System.Threading;
using System.Threading.Tasks;
using SchoolErp.Application.Preferences.Dtos;

namespace SchoolErp.Application.Preferences
{
    public interface IPreferenceService
    {
        Task<PreferenceDto?> GetPreferencesAsync(string userId, CancellationToken ct = default);
        Task<bool> UpdatePreferencesAsync(string userId, UpdatePreferenceRequest request, CancellationToken ct = default);
    }
}
