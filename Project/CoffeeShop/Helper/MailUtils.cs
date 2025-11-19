using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using System;
using System.Threading.Tasks;
namespace CoffeeShop.Helper
{
    public static class MailUtils
    {
        private static string _fromEmail = "coffeeshop2g1g@gmail.com";
        private static string _senderPasswd = "gwgzlaleifibvfda";
        public static async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            // Khởi tạo mail để chuẩn bị gửi đi
            MimeMessage message = new MimeMessage();
            message.From.Add(new MailboxAddress("CoffeeShop 2G1G", _fromEmail));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;
            message.Body = new TextPart("plain")
            {
                Text = body
            };

            // Cấu hình SMTP server và thông tin người gửi
            using (var client = new SmtpClient())
            {
                try
                {
                    await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(_fromEmail, _senderPasswd);
                    await client.SendAsync(message);
                }
                catch (Exception ex)
                {
                    // Xử lý lỗi nếu cần
                    Console.WriteLine($"Lỗi khi gửi email: {ex.Message}");
                }
                finally
                {
                    await client.DisconnectAsync(true);
                }
            }
        }
    }
}
