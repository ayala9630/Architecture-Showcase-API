using ChineseSaleApi.Dto;

namespace ChineseSaleApi.ServiceInterfaces
{
    public interface IEmailService
    {
        void SendEmail(EmailRequestDto emailRequest);
    }
}