using System.Net;
using System.Net.Mail;

namespace WebApplicationCinema.Models
{
    public class MailBox : IMailBox
    {
        public MailBox(string smtpHost, int smtpPort, string emailFrom, string password, bool enableSSL = false)
        {
            _smtpHost = smtpHost;
            _smtpPort = smtpPort;
            _emailFrom = emailFrom;
            _password = password;
            _enableSSL = enableSSL;
        }

        private string _smtpHost;
        private int _smtpPort;
        private string _emailFrom;
        private string _password;
        private bool _enableSSL;

        public bool ValidateEmail(string email)
        {
            try
            {
                string address = new MailAddress(email).Address;
                return address == email.Trim();
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public bool Send(string emailTo, string subject, string body, List<Attachment> attachments)
        {
            using var mail = new MailMessage(_emailFrom, emailTo, subject, body);
            foreach (Attachment attachment in attachments)
                mail.Attachments.Add(attachment);

            using (SmtpClient smtp = new SmtpClient(_smtpHost, _smtpPort))
            {
                //string from = _emailFrom.Split("@")[0];
                smtp.Credentials = new NetworkCredential(_emailFrom, _password);
                smtp.EnableSsl = _enableSSL;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.UseDefaultCredentials = false;
                //smtp.DeliveryFormat = SmtpDeliveryFormat.International;
                try  // smtp шлет нахуй: Object already disposed
                {
                    smtp.Send(mail);
                    return true;
                }
                catch (SmtpException ex)
                {
                    Console.Error.WriteLine($"Email delivery failed: {ex.Message}");
                    return false;
                }
            }
        }

        public bool Send(string emailTo, string subject, string body)
        {
            return Send(emailTo, subject, body, new List<Attachment>());
        }

        public async Task<bool> SendAsync(string emailTo, string subject, string body, List<Attachment> attachments)
        {
            using var mail = new MailMessage(_emailFrom, emailTo, subject, body);
            foreach (Attachment attachment in attachments)
                mail.Attachments.Add(attachment);

            using (SmtpClient smtp = new SmtpClient(_smtpHost, _smtpPort))
            {
                smtp.Credentials = new NetworkCredential(_emailFrom, _password);
                smtp.EnableSsl = _enableSSL;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.UseDefaultCredentials = false;

                try
                {
                    await smtp.SendMailAsync(mail);
                    return true;
                }
                catch (SmtpException ex)
                {
                    Console.Error.WriteLine($"Email delivery failed: {ex.Message}");
                    return false;
                }
            }
        }

        public async Task<bool> SendAsync(string emailTo, string subject, string body)
        {
            return await SendAsync(emailTo, subject, body, new List<Attachment>());
        }
    }
}
