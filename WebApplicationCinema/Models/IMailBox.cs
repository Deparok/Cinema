using System.Net.Mail;

namespace WebApplicationCinema.Models
{
    public interface IMailBox
    {
        bool ValidateEmail(string email);

        public bool Send(string emailTo, string subject, string body, List<Attachment> attachments);

        public bool Send(string emailTo, string subject, string body);

        public Task<bool> SendAsync(string emailTo, string subject, string body, List<Attachment> attachments);

        public Task<bool> SendAsync(string emailTo, string subject, string body);
    }
}
