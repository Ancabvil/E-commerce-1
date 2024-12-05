namespace TuringClothes.Services
{
    using System.Net;
    using System.Threading.Tasks;
    using MailKit.Net.Smtp;
    using MimeKit;
    using TuringClothes.Database;

    public class EmailService
    {
        private readonly string smtpServer = "smtp.gmail.com"; // servidor SMTP
        private readonly int smtpPort = 587; // puerto SMTP
        private readonly string smtpUser = "turingclothes@gmail.com";
        private readonly string smtpPass = "fzcx owqb ilap nsil";

        //


        public async Task SendEmailAsync(string to, string subject, string htmlBody)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("TuringClothes", smtpUser));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlBody
            };
            message.Body = bodyBuilder.ToMessageBody();

            try
            {
                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(smtpServer, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(smtpUser, smtpPass);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }
            }
            catch (SmtpCommandException smtpEx)
            {
                Console.WriteLine($"Error SMTP: {smtpEx.Message}");
                Console.WriteLine($"Detalles: {smtpEx.StackTrace}");
                throw new Exception("Error al enviar el correo SMTP", smtpEx);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al enviar el correo: {ex.Message}");
                Console.WriteLine($"Detalles del error: {ex.StackTrace}");
                throw new Exception("Error enviando el correo", ex);
            }
        }


    }
}
