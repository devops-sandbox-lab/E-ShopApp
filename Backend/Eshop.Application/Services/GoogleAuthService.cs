using Eshop.Application.DTOs;
using Eshop.Application.GeneralResponse;
using Eshop.Application.Interfaces.Repository;
using Eshop.Application.Interfaces.Services;

namespace Eshop.Application.Services
{
    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly IAccountRepository accountRepository;

        public GoogleAuthService(IAccountRepository accountRepository)
        {
            this.accountRepository = accountRepository;
        }

        public async Task<GeneralResponse<AuthResponseDTO>> GoogleSignIn(string model)
        {
            var result = await accountRepository.GoogleSignIn(model);
            if (result.Succeeded)
            {
                return new GeneralResponse<AuthResponseDTO>()
                {
                    Data = result,
                    Succeeded = true,
                    Message = result.Message
                };
            }
            else
            {
                return new GeneralResponse<AuthResponseDTO>()
                {
                    Data = null,
                    Succeeded = false,
                    Message = result.Message,
                    Errors = result.Errors
                };
            }
        }

    }
}
