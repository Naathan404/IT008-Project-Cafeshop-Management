using CoffeeShop.Models;
using CoffeeShop.Service;
using CoffeeShop.Service.DTOs;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Windows;
using System.Windows.Input;
using CoffeeShop.Helper;

namespace CoffeeShop.ViewModels.StaffVM
{
    public class ReportDepotViewModel : BaseViewModel
    {
        public string _filePath { get; private set; } = string.Empty;
        public ICommand SendCommand { set; get; } = null!;
        public ICommand CloseCommand { set; get; } = null!;


        private ObservableCollection<DepotItemDTO> _reportData = new ObservableCollection<DepotItemDTO>();
        public ObservableCollection<DepotItemDTO> ReportData
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
        }

        private async void SendReportEmail(string filePath, int staffId)
        {
            string staffName = UserSession.Instance.StaffName;
            try
            {

                using (var db = new CoffeeShopContext())
                {
                    var emailList = db.Staff
                        .Where(o => o.StaffRole == "Admin" && !string.IsNullOrEmpty(o.Email))
                        .Select(o => o.Email)
                        .ToList();

                    // Danh sach tasks
                    var sendTasks = new List<Task>();

                    foreach (string email in emailList)
                    {
                        // Tiêu đề Email
                        string mailSubject = $"BÁO CÁO KHO HÀNG TỪ NHÂN VIÊN {staffName} - {DateTime.Now:dd/MM/yyyy}";
                        // Nội dung Email
                        string mailBody = $"Gửi Admin, \n\n" +
                                    $"Nhân viên {staffName} (ID: {staffId}) vừa gửi báo cáo kho hàng đính kèm.\n\n" +
                                    $"Báo cáo được tạo lúc: {DateTime.Now:HH:mm:ss}";
                        sendTasks.Add(MailUtils.SendEmailAsync(email, mailSubject, mailBody, filePath));
                    }

                    await Task.WhenAll(sendTasks);

                    await Task.Delay(5000);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Lỗi trong quá trình gửi email: {ex.Message}", "Lỗi");
            }
            finally
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
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
                    _reportData.Add(new DepotItemDTO
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
