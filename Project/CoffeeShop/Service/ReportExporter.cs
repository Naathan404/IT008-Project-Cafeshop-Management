using ClosedXML.Excel;
using CoffeeShop.Models;
using CoffeeShop.View;

using CoffeeShop.ViewModels.AdminVM;
using System.Collections.ObjectModel;

namespace CoffeeShop.Service
{
    public class ReportExporter
    {
        private ObservableCollection<AdminHistoryViewModel.OrderHistory> _orders;

        public ReportExporter(ObservableCollection<AdminHistoryViewModel.OrderHistory> orders)
        {
            _orders = orders;
        }

        public async Task ExportToExcel(string filePath)
        {
            //using (var workbook = new XLWorkbook())
            //{
            //    var worksheet = workbook.Worksheets.Add("Report");

            //    // Thiết lập độ rộng cột 
            //    worksheet.Column(1).Width = 18; // Mã hóa đơn
            //    worksheet.Column(2).Width = 5;  // Tên khách hàng
            //    worksheet.Column(3).Width = 5;  // Thời gian
            //    worksheet.Column(4).Width = 12; // Hình thức
            //    worksheet.Column(5).Width = 12; // Tổng tiền

            //    // Header: Tên quán
            //    var header = worksheet.Cell("A1");
            //    header.Value = "BÁO CÁO LỊCH SỬ ĐƠN HÀNG";
            //    worksheet.Range("A1:E1").Merge().Style.Font.SetBold().Font.SetFontSize(16).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            //    // Thông tin hóa đơn
            //    worksheet.Cell("A2").Value = $"Ngày: {DateTime.Now:dd/MM/yyyy HH:mm}";
            //    worksheet.Range("A2:E2").Merge().Style.Font.SetBold().Font.SetFontSize(16).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            //    // Danh sách các sản phẩm
            //    int currentRow = 4;
            //    var tableHeader = worksheet.Range(currentRow, 1, currentRow, 5);
            //    tableHeader.Style.Font.SetBold().Border.SetBottomBorder(XLBorderStyleValues.Thin);
            //    worksheet.Cell(currentRow, 1).Value = "Mã hóa đơn";
            //    worksheet.Cell(currentRow, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            //    worksheet.Cell(currentRow, 2).Value = "Tên khách hàng";
            //    worksheet.Cell(currentRow, 2).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            //    worksheet.Cell(currentRow, 3).Value = "Thời gian";
            //    worksheet.Cell(currentRow, 3).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            //    worksheet.Cell(currentRow, 4).Value = "Hình thức";
            //    worksheet.Cell(currentRow, 4).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            //    worksheet.Cell(currentRow, 4).Style.NumberFormat.Format = "#,##0";
            //    worksheet.Cell(currentRow, 5).Value = "Tổng Tiền";
            //    worksheet.Cell(currentRow, 5).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            //    worksheet.Cell(currentRow, 5).Style.NumberFormat.Format = "#,##0";

            //    // Đổ dữ liệu
            //    foreach (var order in _orders)
            //    {
            //        currentRow++;
            //        worksheet.Cell(currentRow, 1).Value = order.OrderID;
            //        worksheet.Cell(currentRow, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
            //        worksheet.Cell(currentRow, 2).Value = order.CustomerName ?? "-";
            //        worksheet.Cell(currentRow, 2).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            //        worksheet.Cell(currentRow, 3).Value = order.OrderDate;
            //        worksheet.Cell(currentRow, 3).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            //        worksheet.Cell(currentRow, 4).Value = order.PaymentMethod;
            //        worksheet.Cell(currentRow, 4).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
            //        worksheet.Cell(currentRow, 4).Style.NumberFormat.Format = "#,##0";
            //        worksheet.Cell(currentRow, 5).Value = order.Total;
            //        worksheet.Cell(currentRow, 5).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
            //        worksheet.Cell(currentRow, 5).Style.NumberFormat.Format = "#,##0";
            //    }

            //    // Phần tổng tiền
            //    currentRow += 2;
            //    worksheet.Cell(currentRow, 4).Value = "Tạm tính:";
            //    worksheet.Cell(currentRow, 5).Value = order.SubTotal;
            //    worksheet.Cell(currentRow, 5).Style.NumberFormat.Format = "#,##0";
            //    currentRow++;
            //    worksheet.Cell(currentRow, 4).Value = "Giảm giá:";
            //    worksheet.Cell(currentRow, 5).Value = order.DiscountMoney;
            //    worksheet.Cell(currentRow, 5).Style.NumberFormat.Format = "#,##0";

            //    currentRow++;
            //    var finalRange = worksheet.Cell(currentRow, 4);
            //    finalRange.Value = "TỔNG CỘNG:";
            //    finalRange.Style.Font.SetBold();
            //    var finalTotal = worksheet.Cell(currentRow, 5);
            //    finalTotal.Value = order.TotalAmount;
            //    finalTotal.Style.Font.SetBold().Font.SetFontColor(XLColor.Red);
            //    finalTotal.Style.NumberFormat.Format = "#,##0";

            //    // Wifi
            //    currentRow += 2;
            //    var wifiInfo = worksheet.Cell(currentRow, 1).Value = "Wifi: 2G1G Wifi 5G";
            //    worksheet.Range(currentRow, 1, currentRow, 5).Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            //    currentRow++;
            //    var wifiPassword = worksheet.Cell(currentRow, 1).Value = "Mật khẩu: Xincamon";
            //    worksheet.Range(currentRow, 1, currentRow, 5).Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            //    // Footer
            //    currentRow += 2;
            //    var footer = worksheet.Cell(currentRow, 1);
            //    footer.Value = "CẢM ƠN QUÝ KHÁCH - HẸN GẶP LẠI!";
            //    worksheet.Range(currentRow, 1, currentRow, 5).Merge().Style.Font.SetItalic().Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            //    // Lưu file
            //    workbook.SaveAs(filePath);
        }
    }
}
