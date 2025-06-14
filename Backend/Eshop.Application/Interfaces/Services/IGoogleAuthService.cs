using Eshop.Application.DTOs;
using Eshop.Application.GeneralResponse;

namespace Eshop.Application.Interfaces.Services
{
    public interface IGoogleAuthService
    {
        Task<GeneralResponse<AuthResponseDTO>> GoogleSignIn(string model);

    }
}
