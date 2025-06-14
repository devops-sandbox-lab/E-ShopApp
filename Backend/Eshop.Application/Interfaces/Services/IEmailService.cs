using Eshop.Application.DTOs;

namespace Eshop.Application.Interfaces.Services
{
    public interface IEmailService
    {
        Task sendEmailAsync(EmailDTO emailDTO);

    }
}
