using CoffeeShop.Service;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Windows;
using System.Windows.Input;

namespace CoffeeShop.ViewModels.StaffVM
{
    public class ReportDepotViewModel
    {
        public string GeneratedFilePath { get; private set; } = string.Empty;
        public ICommand SendCommand { set; get; } = null!;

        public ReportDepotViewModel(string filePath)
        {
            GeneratedFilePath = filePath; // <-- Gán giá trị nhận được
            SendCommand = new RelayCommand<Window>(ExecuteSendReport);
        }

        private void ExecuteSendReport(Window window)
        {
            // Lấy StaffId và File Path từ các biến đã lưu trong VM
            int currentStaffId = UserSession.Instance.StaffId;
            string reportPath = this.GeneratedFilePath;

            // Kiểm tra an toàn trước khi gửi
            if (currentStaffId == 0)
            {
                MessageBox.Show("Vui lòng đăng nhập lại. Không tìm thấy thông tin nhân viên.", "Lỗi");
                return;
            }

            try
            {
                // GỌI HÀM GỬI EMAIL CHÍNH
                SendReportEmail(reportPath, currentStaffId);

                MessageBox.Show("Đã gửi báo cáo thành công cho Admin!", "Thành công");
                window.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi gửi báo cáo: Vui lòng kiểm tra lại cấu hình email và kết nối mạng. Chi tiết: {ex.Message}", "Lỗi");
            }
            finally
            {
                // Xóa file tạm sau khi hoàn tất (RẤT QUAN TRỌNG)
                if (File.Exists(reportPath))
                {
                    File.Delete(reportPath);
                }
            }
        }

        private void SendReportEmail(string filePath, int staffId)
        {
            // CẤU HÌNH GỬI MAIL (THAY THẾ BẰNG THÔNG TIN THỰC TẾ)
            // SENDER: Email hệ thống/cửa hàng (Nên dùng Gmail với App Password)
            string senderEmail = "your_store_report@gmail.com";
            string senderPassword = "YOUR_APP_PASSWORD_HERE"; // <--- MẬT KHẨU ỨNG DỤNG GMAIL
            string adminEmail = "admin@coffeeshop.com"; // <--- Email người nhận (Admin)
            string staffName = UserSession.Instance.StaffName; // Lấy tên NV từ Singleton

            // 1. Cấu hình SMTP Client (Sử dụng cấu hình Gmail)
            using (SmtpClient client = new SmtpClient("smtp.gmail.com", 587))
            {
                client.EnableSsl = true; // Bắt buộc phải bật SSL
                client.UseDefaultCredentials = false;

                // Đăng nhập bằng NetworkCredential
                client.Credentials = new NetworkCredential(senderEmail, senderPassword);

                // 2. Tạo Mail Message
                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(senderEmail);
                    mail.To.Add(adminEmail);

                    // Tiêu đề Email
                    mail.Subject = $"BÁO CÁO KHO HÀNG TỪ NV {staffName} - {DateTime.Now:dd/MM/yyyy}";

                    // Nội dung Email
                    mail.Body = $"Kính gửi Admin, \n\n" +
                                $"Nhân viên {staffName} (ID: {staffId}) vừa gửi báo cáo kho hàng đính kèm.\n\n" +
                                $"Báo cáo được tạo lúc: {DateTime.Now:HH:mm:ss}";

                    // 3. Đính kèm File Excel
                    mail.Attachments.Add(new Attachment(filePath));

                    // 4. Gửi Mail
                    client.Send(mail);
                }
            }
        }
    }
}
