using System;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using ChineseSaleApi.Dto;
using ChineseSaleApi.ServiceInterfaces;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using Microsoft.Extensions.Logging;

namespace ChineseSaleApi.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettingsDto _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettingsDto> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public void SendEmail(EmailRequestDto emailRequest)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
                message.To.Add(new MailboxAddress("", emailRequest.To));
                message.Subject = emailRequest.Subject;

                message.Body = new TextPart("html")
                {
                    Text = emailRequest.Body
                };

                using (var client = new SmtpClient())
                {
                    // Set validation callback BEFORE connecting.
                    // Accept only when there are no errors or when the only chain problem is revocation-check failures (offline/unknown).
                    client.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
                    {
                        if (sslPolicyErrors == SslPolicyErrors.None)
                            return true;

                        // Allow when the only chain status flags are related to revocation checks being unavailable.
                        if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateChainErrors) != 0 && chain != null)
                        {
                            var statuses = chain.ChainStatus ?? Array.Empty<X509ChainStatus>();
                            var onlyRevocationIssues = statuses.Length > 0 &&
                                                       statuses.All(s =>
                                                           s.Status == X509ChainStatusFlags.RevocationStatusUnknown ||
                                                           s.Status == X509ChainStatusFlags.RevocationStatusUnknown ||
                                                           s.Status == X509ChainStatusFlags.OfflineRevocation);

                            if (onlyRevocationIssues)
                                return true;
                        }

                        // Otherwise reject certificate
                        return false;
                    };

                    // Let MailKit pick the correct transport; use Auto or StartTls if you know the server requires it.
                    client.Connect(_emailSettings.SmtpServer, _emailSettings.SmtpPort, MailKit.Security.SecureSocketOptions.Auto);
                    client.Authenticate(_emailSettings.SenderEmail, _emailSettings.Password);

                    client.Send(message);
                    client.Disconnect(true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Recipient}.", emailRequest?.To);
                throw;
            }
        }
    }
}
