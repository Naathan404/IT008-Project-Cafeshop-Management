using ClosedXML.Excel;
using CoffeeShop.Models;
using CoffeeShop.Service.DTOs;
using CoffeeShop.ViewModels;

using CoffeeShop.ViewModels.AdminVM;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;

namespace CoffeeShop.Service
{
    public class ReportExporter
    {
        private ObservableCollection<AdminHistoryViewModel.OrderHistory> _orders;
        private ObservableCollection<DepotItemDTO> _depotItems;

        public ReportExporter(ObservableCollection<AdminHistoryViewModel.OrderHistory> orders)
        {
            _orders = orders;
        }

        public ReportExporter(ObservableCollection<DepotItemDTO> items)
        {
            _depotItems = items;
        }

        public async Task ExportOrderHistory(string filePath, DateTime? _fromDate, DateTime? _toDate)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Report");

                // Thiết lập độ rộng cột 
                worksheet.Column(1).Width = 18; // Mã hóa đơn
                worksheet.Column(2).Width = 25; // Tên khách hàng
                worksheet.Column(3).Width = 18; // Thời gian
                worksheet.Column(4).Width = 14; // Hình thức
                worksheet.Column(5).Width = 14; // Tổng tiền

                // Title
                var header = worksheet.Cell("A1");
                header.Value = "BÁO CÁO LỊCH SỬ ĐƠN HÀNG";
                worksheet.Range("A1:E1").Merge().Style.Font.SetBold().Font.SetFontSize(16).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                // Thông tin hóa đơn
                worksheet.Cell("A2").Value = $"Từ: {_fromDate:dd/MM/yyyy HH:mm}   đến: {_toDate:dd/MM/yyyy HH:mm}";
                worksheet.Range("A2:E2").Merge().Style.Font.SetBold().Font.SetFontSize(16).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                // Style Header
                int currentRow = 4;
                worksheet.Row(currentRow).Height = 19;
                var tableHeader = worksheet.Range(currentRow, 1, currentRow, 5);
                tableHeader.Style.Font.SetBold().Border.SetBottomBorder(XLBorderStyleValues.Thin);
                tableHeader.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                tableHeader.Style.Fill.BackgroundColor = XLColor.LightBlue;

                // Thiết lập tiêu đề cột
                worksheet.Cell(currentRow, 1).Value = "Mã hóa đơn";
                worksheet.Cell(currentRow, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                worksheet.Cell(currentRow, 2).Value = "Tên khách hàng";
                worksheet.Cell(currentRow, 2).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                worksheet.Cell(currentRow, 3).Value = "Thời gian";
                worksheet.Cell(currentRow, 3).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                worksheet.Cell(currentRow, 4).Value = "Hình thức";
                worksheet.Cell(currentRow, 4).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                worksheet.Cell(currentRow, 4).Style.NumberFormat.Format = "#,##0";
                worksheet.Cell(currentRow, 5).Value = "Tổng Tiền";
                worksheet.Cell(currentRow, 5).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                worksheet.Cell(currentRow, 5).Style.NumberFormat.Format = "#,##0";


                // Đổ dữ liệu
                foreach (var order in _orders)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = order.DisplayID;
                    worksheet.Cell(currentRow, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                    worksheet.Cell(currentRow, 2).Value = order.CustomerName ?? "-";
                    worksheet.Cell(currentRow, 2).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                    worksheet.Cell(currentRow, 3).Value = order.OrderDate;
                    worksheet.Cell(currentRow, 3).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                    worksheet.Cell(currentRow, 4).Value = order.PaymentMethod;
                    worksheet.Cell(currentRow, 4).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                    worksheet.Cell(currentRow, 4).Style.NumberFormat.Format = "#,##0";
                    worksheet.Cell(currentRow, 5).Value = order.Total;
                    worksheet.Cell(currentRow, 5).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                    worksheet.Cell(currentRow, 5).Style.NumberFormat.Format = "#,##0";
                }
                // Lưu file
                workbook.SaveAs(filePath);
            }
        }

        public async Task ExportDepotItem(string filePath)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Report");

                // Thiết lập độ rộng cột 
                worksheet.Column(1).Width = 30; // Mã hóa đơn
                worksheet.Column(2).Width = 15; // Tên khách hàng
                worksheet.Column(3).Width = 10; // Thời gian
                worksheet.Column(4).Width = 40; // Hình thức

                // Title
                var header = worksheet.Cell("A1");
                header.Value = "BÁO CÁO KHO NGUYÊN LIỆU";
                worksheet.Range("A1:D1").Merge().Style.Font.SetBold().Font.SetFontSize(16).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                // Thông tin hóa đơn
                worksheet.Cell("A2").Value = $"Ngày: {DateTime.Now: dd/MM/yyyy HH:mm}";
                worksheet.Range("A2:D2").Merge().Style.Font.SetBold().Font.SetFontSize(16).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                // Style Header
                int currentRow = 4;
                worksheet.Row(currentRow).Height = 19;
                var tableHeader = worksheet.Range(currentRow, 1, currentRow, 4);
                tableHeader.Style.Font.SetBold().Border.SetBottomBorder(XLBorderStyleValues.Thin);
                tableHeader.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                tableHeader.Style.Fill.BackgroundColor = XLColor.LightBlue;

                // Thiết lập tiêu đề cột
                worksheet.Cell(currentRow, 1).Value = "Tên nguyên liệu";
                worksheet.Cell(currentRow, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                worksheet.Cell(currentRow, 2).Value = "Số lượng";
                worksheet.Cell(currentRow, 2).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                worksheet.Cell(currentRow, 3).Value = "Đơn vị";
                worksheet.Cell(currentRow, 3).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                worksheet.Cell(currentRow, 4).Value = "Ghi chú";
                worksheet.Cell(currentRow, 4).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                // Đổ dữ liệu
                foreach (var item in _depotItems)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = item.MaterialName;
                    worksheet.Cell(currentRow, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                    worksheet.Cell(currentRow, 2).Value = item.Quantity;
                    worksheet.Cell(currentRow, 2).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                    worksheet.Cell(currentRow, 4).Style.NumberFormat.Format = "#,##0.##";
                    worksheet.Cell(currentRow, 3).Value = item.Unit;
                    worksheet.Cell(currentRow, 3).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                    worksheet.Cell(currentRow, 4).Value = item.Note;
                    worksheet.Cell(currentRow, 4).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                }
                // Lưu file
                workbook.SaveAs(filePath);
            }
        }
    }
}
