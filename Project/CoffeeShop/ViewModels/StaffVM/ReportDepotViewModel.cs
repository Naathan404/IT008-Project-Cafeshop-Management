using CoffeeShop.Models;
using CoffeeShop.Service;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Windows;
using System.Windows.Input;

namespace CoffeeShop.ViewModels.StaffVM
{
    public class ReportDepotViewModel : BaseViewModel
    {
        public string _filePath { get; private set; } = string.Empty;
        public ICommand SendCommand { set; get; } = null!;
        public ICommand CloseCommand { set; get; } = null!;


        private ObservableCollection<DepotItem> _reportData = new ObservableCollection<DepotItem>();
        public ObservableCollection<DepotItem> ReportData
        {
            get { return _reportData; }
            // KHÔNG cần setter nếu m chỉ thao tác với .Add()/.Clear() trên _reportData
        }

        // Constructor
        public ReportDepotViewModel(string filePath)
        {
            _filePath = filePath; 
            LoadReportData();
            SendCommand = new RelayCommand<Window>(ExecuteSendReport);
            CloseCommand = new RelayCommand<Window>(w => w?.Close());
        }

        private void ExecuteSendReport(Window window)
        {
            int currentStaffId = UserSession.Instance.StaffId;
            string reportPath = this._filePath;

            // Kiểm tra an toàn trước khi gửi
            if (currentStaffId == 0)
            {
                MessageBox.Show("Vui lòng đăng nhập lại. Không tìm thấy thông tin nhân viên.", "Lỗi");
                return;
            }

            // 2. BẮT ĐẦU QUY TRÌNH GỬI MAIL
            try
            {
                // GỌI HÀM GỬI EMAIL CHÍNH (SendReportEmail đã có logic gửi mail)
                SendReportEmail(reportPath, currentStaffId);

                MessageBox.Show("Đã gửi báo cáo thành công cho Admin!", "Thành công");
                window?.Close(); // Đóng cửa sổ sau khi gửi thành công
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi gửi báo cáo: Vui lòng kiểm tra lại cấu hình email (Mật khẩu ứng dụng) và kết nối mạng. Chi tiết: {ex.Message}", "Lỗi");

                // Quan trọng: Nếu gửi lỗi, ta không đóng cửa sổ để người dùng có thể xem lại lỗi hoặc thử lại
            }
            finally
            {
                // 3. DỌN DẸP: Xóa file tạm sau khi hoàn tất (Dù gửi thành công hay thất bại)
                // Việc này đảm bảo không làm đầy thư mục Temp của hệ thống.
                if (File.Exists(reportPath))
                {
                    File.Delete(reportPath);
                }
            }
        }

        private void SendReportEmail(string filePath, int staffId)
        {
            string senderEmail = "nghia84902@gmail.com"; // Tài khoản gửi email
            string senderPassword = "tijg cqki awgm gtam"; // Vào google Account tìm "Mật khẩu ứng dụng". Tạo mật khẩu 16 số và nhập vào đây
            string adminEmail = "nghia84902@gmail.com"; // Email người nhận
            string staffName = UserSession.Instance.StaffName;

            // 1. Cấu hình SMTP Client (Sử dụng cấu hình phổ biến cho Gmail)
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
                    mail.Subject = $"BÁO CÁO KHO HÀNG TỪ NHÂN VIÊN {staffName} - {DateTime.Now:dd/MM/yyyy}";

                    // Nội dung Email
                    mail.Body = $"Gửi Admin, \n\n" +
                                $"Nhân viên {staffName} (ID: {staffId}) vừa gửi báo cáo kho hàng đính kèm.\n\n" +
                                $"Báo cáo được tạo lúc: {DateTime.Now:HH:mm:ss}";

                    // 3. Đính kèm File Excel
                    // filePath là đường dẫn file tạm đã được tạo trước đó
                    mail.Attachments.Add(new Attachment(filePath));

                    // 4. Gửi Mail
                    client.Send(mail); // <--- Lệnh này thực hiện việc gửi đi
                }
            }
        }

        private void LoadReportData()
        {
            _reportData.Clear();
            using (var db = new CoffeeShopContext())
            {
                var items = db.Inventories.ToList();
                foreach (var item in items)
                {
                    if (item.IsDeleted) continue; // Bỏ qua các mục đã bị xóa
                    _reportData.Add(new DepotItem
                    {
                        MaterialId = item.MaterialId,
                        MaterialName = item.MaterialName ?? string.Empty,
                        Quantity = item.Quantity,
                        Unit = item.Unit ?? string.Empty,
                        Note = item.Note ?? string.Empty
                    });
                }
            }
        }
    }
}
