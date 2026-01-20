using CoffeeShop.View.Controls;
using CoffeeShop.ViewModels.AdminVM;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static CoffeeShop.View.Controls.CustomMessageBox;

namespace CoffeeShop.ViewModels.StaffVM
{
    public class OrderHistoryReportViewModel : BaseViewModel
    {
        // Danh sách hiển thị trên trang Preview
        public ObservableCollection<AdminHistoryViewModel.OrderHistory> PreviewOrders { get; set; }

        public ICommand ExportCommand { get; }
        public ICommand CloseCommand { get; }

        public OrderHistoryReportViewModel(List<AdminHistoryViewModel.OrderHistory> data)
        {
            PreviewOrders = new ObservableCollection<AdminHistoryViewModel.OrderHistory>(data);
            ExportCommand = new RelayCommand<object>(async (p) => await ExecuteActualExport(PreviewOrders.ToList()));
            CloseCommand = new RelayCommand<Window>((w) => w?.Close());
        }

        private async Task ExecuteActualExport(List<AdminHistoryViewModel.OrderHistory> data)
        {
            try
            {
                string fileName = $"History_Report_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Exports");
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
                string fullPath = Path.Combine(folderPath, fileName);

                await Task.Run(() =>
                {
                    // Gọi helper để vẽ file
                    ExportToExcelHelper.Generate(data, fullPath);
                });

                if (CustomMessageBox.Show("Xuất file Excel thành công! Mở file ngay?", "Thành công",
                    CustomMessageBox.MessageButtons.YesNo, CustomMessageBox.MessageType.Success) == CustomMessageBox.MessageBoxResult.Yes)
                {
                    Process.Start(new ProcessStartInfo(fullPath) { UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", CustomMessageBox.MessageButtons.OK, CustomMessageBox.MessageType.Error);
            }
        }

        public static class ExportToExcelHelper
        {
            public static void Generate(List<AdminHistoryViewModel.OrderHistory> data, string filePath)
            {
                // Cấu hình bản quyền EPPlus (Bắt buộc)
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    var sheet = package.Workbook.Worksheets.Add("Lịch sử đơn hàng");

                    // 1. Vẽ tiêu đề to tướng
                    sheet.Cells["A1:H1"].Merge = true;
                    sheet.Cells["A1"].Value = "BÁO CÁO LỊCH SỬ ĐƠN HÀNG";
                    sheet.Cells["A1"].Style.Font.Size = 16;
                    sheet.Cells["A1"].Style.Font.Bold = true;
                    sheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    // 2. Vẽ Header bảng
                    string[] headers = { "Mã Đơn", "Khách Hàng", "Nhân Viên", "Ngày Đặt", "Tổng Tiền", "Giảm Giá", "Thanh Toán", "Bàn" };
                    for (int i = 0; i < headers.Length; i++)
                    {
                        var cell = sheet.Cells[3, i + 1];
                        cell.Value = headers[i];
                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    }

                    // 3. Đổ dữ liệu
                    int row = 4;
                    foreach (var item in data)
                    {
                        sheet.Cells[row, 1].Value = item.DisplayID;
                        sheet.Cells[row, 2].Value = item.CustomerName;
                        sheet.Cells[row, 3].Value = item.EmployeeName;
                        sheet.Cells[row, 4].Value = item.OrderDate;
                        sheet.Cells[row, 5].Value = item.Total;
                        sheet.Cells[row, 6].Value = item.Discount;
                        sheet.Cells[row, 7].Value = item.PaymentMethod;
                        sheet.Cells[row, 8].Value = item.TableName;
                        row++;
                    }

                    // 4. Format tiền tệ cho cột E và F
                    sheet.Column(5).Style.Numberformat.Format = "#,##0";
                    sheet.Column(6).Style.Numberformat.Format = "#,##0";

                    // 5. Tự động căn chỉnh độ rộng cột
                    sheet.Cells.AutoFitColumns();

                    package.Save();
                }
            }
        }
    }
}
