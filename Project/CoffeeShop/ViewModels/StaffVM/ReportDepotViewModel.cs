using CoffeeShop.Helper;
using CoffeeShop.Models;
using CoffeeShop.Service;
using CoffeeShop.Service.DTOs;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;

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
        }

        private string _windowTitle = "Báo Cáo Kho Hàng";
        public string WindowTitle
        {
            get => _windowTitle;
            set { _windowTitle = value; OnPropertyChanged(); }
        }

        // Property cho nội dung của Button
        private string _submitButtonText = "Gửi";
        public string SubmitButtonText
        {
            get => _submitButtonText;
            set { _submitButtonText = value; OnPropertyChanged(); }
        }

        // Constructor
        public ReportDepotViewModel()
        {
            // Load giao dien cho admin
            if (UserSession.Instance.StaffRole == "Admin")
            {
                WindowTitle = "Nội Dung File";
                SubmitButtonText = "Xuất";
            }

            LoadReportData();
            if (UserSession.Instance.StaffRole == "Admin")
            {
                SendCommand = new RelayCommand<Window>(ExecuteExport);
            }
            else SendCommand = new RelayCommand<Window>(ExecuteSendReport);

            CloseCommand = new RelayCommand<Window>(w => w?.Close());
        }

        private void ExecuteSendReport(Window window)
        {
            // ---- Tao file bao cao ----
            _filePath = string.Empty;

            try
            {
                _filePath = CreateExcelReport(ReportData); // Tạo file và lấy đường dẫn
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tạo báo cáo: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }


            // ---- Chuan bi thong tin nhan vien de gui bao cao ----
            int currentStaffId = UserSession.Instance.StaffId;

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
                SendReportEmail(currentStaffId);

                MessageBox.Show("Đã gửi báo cáo thành công cho Admin!", "Thành công");
                window?.Close(); // Đóng cửa sổ sau khi gửi thành công
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi gửi báo cáo: Vui lòng kiểm tra lại cấu hình email (Mật khẩu ứng dụng) và kết nối mạng. Chi tiết: {ex.Message}", "Lỗi");

                // Quan trọng: Nếu gửi lỗi, ta không đóng cửa sổ để người dùng có thể xem lại lỗi hoặc thử lại
            }
        }

        private string CreateExcelReport(ObservableCollection<DepotItemDTO> data)
        {
            ExcelPackage.License.SetNonCommercialPersonal("2G1G Café");

            string fileName = $"BaoCaoKho_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            _filePath = Path.Combine(Path.GetTempPath(), fileName);
            // Neu la admin thi luu file tren desktop
            if (UserSession.Instance.StaffRole == "Admin")
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop); // Lay duong dan toi Desktop
                _filePath = Path.Combine(desktopPath, fileName);
            }

            using (var package = new ExcelPackage(new FileInfo(_filePath)))
            {
                // Tao work sheet
                var workSheet = package.Workbook.Worksheets.Add("Báo Cáo Kho");
                // true = co header
                workSheet.Cells["A1"].LoadFromCollection(data, true, TableStyles.Medium1);


                workSheet.Cells[workSheet.Dimension.Address].AutoFitColumns();
                workSheet.Cells[1, 1, 1, 6].Style.Font.Bold = true;

                // Ví dụ định dạng cột số lượng (giả sử cột số 4 là Quantity)
                workSheet.Column(4).Style.Numberformat.Format = "#,##0.00";

                package.Save();
                return _filePath;
            }
        }

        private async void SendReportEmail(int staffId)
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
                        sendTasks.Add(MailUtils.SendEmailAsync(email, mailSubject, mailBody, _filePath));
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
                if (File.Exists(_filePath))
                {
                    File.Delete(_filePath);
                }
            }
            
        }

        private void ExecuteExport(Window window)
        {
            CreateExcelReport(ReportData); // Chi tao file khong gui mail
            if (!string.IsNullOrEmpty(_filePath))
            {
                window?.Close();
                MessageBox.Show($"Báo cáo đã được xuất thành công tại: {_filePath}", "Thành công");
            }
            else
            {
                MessageBox.Show("Xuất báo cáo thất bại.", "Lỗi");
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
