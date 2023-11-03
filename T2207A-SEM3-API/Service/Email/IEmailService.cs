using T2207A_SEM3_API.Helper.Email;

namespace T2207A_SEM3_API.Service.Email
{
    public interface IEmailService
    {
        Task SendEmailAsync(Mailrequest mailrequest);

    }
}
