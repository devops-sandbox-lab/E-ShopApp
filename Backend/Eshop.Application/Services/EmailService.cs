using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Eshop.Application.Configurations;
using Eshop.Application.DTOs;
using Eshop.Application.Interfaces.Services;

namespace Eshop.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly MailSettings mailSettings;

        public EmailService(IOptions<MailSettings> mailSettings)
        {
            this.mailSettings = mailSettings.Value;
        }
        public async Task sendEmailAsync(EmailDTO emailDTO)
        {
            var Email = new MimeMessage
            {
                Sender = MailboxAddress.Parse(mailSettings.Email),
                Subject = emailDTO.Subject
            };

            Email.To.Add(MailboxAddress.Parse(emailDTO.To));

            var builder = new BodyBuilder();
            // If I have any attachment 
            if (emailDTO.Attatchments != null)
            {
                byte[] bytes;
                foreach (var file in emailDTO.Attatchments)
                {
                    if (file.Length > 0)
                    {
                        using var ms = new MemoryStream();
                        file.CopyTo(ms);
                        bytes = ms.ToArray();

                        builder.Attachments.Add(file.FileName, bytes, ContentType.Parse(file.ContentType));
                    }
                }
            }


            builder.HtmlBody = emailDTO.Body;
            Email.Body = builder.ToMessageBody();

            Email.From.Add(new MailboxAddress(mailSettings.DisplayName, mailSettings.Email));

            //Conant to mail provider

            using var smtp = new SmtpClient();
            smtp.Connect(mailSettings.Host, mailSettings.Port, SecureSocketOptions.StartTls);
            smtp.Authenticate(mailSettings.Email, mailSettings.Password);
            await smtp.SendAsync(Email);

            smtp.Disconnect(true);
        }
    }

}
