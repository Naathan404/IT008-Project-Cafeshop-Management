using ClosedXML.Excel;
using CoffeeShop.Models;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.IO;

namespace CoffeeShop.Service
{
    public class BillExporter
    {
        private readonly int _orderId;

        public BillExporter(int orderId)
        {
            _orderId = orderId;
        }

        public async Task ExportToExcel(string filePath)
        {
            using (var db = new CoffeeShopContext())
            {
                // 1. Lấy thông tin hóa đơn chi tiết
                var order = await db.Orders
                    .Include(o => o.Table)
                    .Include(o => o.Customer)
                    .Include(o => o.Staff)
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Price).ThenInclude(s => s.Size)
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Price).ThenInclude(p => p.Item)
                                
                    .FirstOrDefaultAsync(o => o.OrderId == _orderId);

                if (order == null) return;

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Bill");

                    // Thiết lập độ rộng cột 
                    worksheet.Column(1).Width = 15; // Tên món
                    worksheet.Column(2).Width = 8;  // Size
                    worksheet.Column(3).Width = 5;  // Số lượng
                    worksheet.Column(4).Width = 12; // Đơn giá
                    worksheet.Column(5).Width = 12; // Thành tiền

                    // Header: Tên quán
                    var header = worksheet.Cell("A1");
                    header.Value = "2G1G CAFÉ";
                    worksheet.Range("A1:E1").Merge().Style.Font.SetBold().Font.SetFontSize(16).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                    worksheet.Cell("A2").Value = "Địa chỉ: Khu phố 34, P.Linh Xuân, Tp. Hồ Chí Minh";
                    worksheet.Range("A2:E2").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                    // Thông tin hóa đơn
                    worksheet.Cell("A4").Value = $"Mã HĐ: {order.DisplayID}";
                    worksheet.Cell("D4").Value = $"Ngày: {order.OrderDate:dd/MM/yyyy HH:mm}";
                    worksheet.Cell("A5").Value = $"Nhân viên: {order.Staff?.StaffName}";
                    worksheet.Cell("D5").Value = $"Bàn: {order.Table?.TableName ?? "Mang về"}";
                    worksheet.Cell("A6").Value = $"Khách hàng: {order.Customer?.CustomerName ?? "Khách vãng lai"}";

                    // Danh sách các sản phẩm
                    int currentRow = 8;
                    var tableHeader = worksheet.Range(currentRow, 1, currentRow, 5);
                    tableHeader.Style.Font.SetBold().Border.SetBottomBorder(XLBorderStyleValues.Thin);
                    worksheet.Cell(currentRow, 1).Value = "Món ăn";
                    worksheet.Cell(currentRow, 2).Value = "Size";
                    worksheet.Cell(currentRow, 3).Value = "SL";
                    worksheet.Cell(currentRow, 4).Value = "Đơn giá";
                    worksheet.Cell(currentRow, 5).Value = "T.Tiền";

                    // Đổ dữ liệu
                    foreach (var detail in order.OrderDetails)
                    {
                        currentRow++;
                        worksheet.Cell(currentRow, 1).Value = detail.Price?.Item?.ItemName ?? "N/A";
                        worksheet.Cell(currentRow, 2).Value = detail.Price?.Size?.SizeName ?? "-";
                        worksheet.Cell(currentRow, 3).Value = detail.Quantity;
                        worksheet.Cell(currentRow, 4).Value = detail.UnitPrice;
                        worksheet.Cell(currentRow, 5).Value = detail.TotalPrice;
                    }

                    // Phần tổng tiền
                    currentRow += 2;
                    worksheet.Cell(currentRow, 4).Value = "Tạm tính:";
                    worksheet.Cell(currentRow, 5).Value = order.SubTotal;

                    currentRow++;
                    worksheet.Cell(currentRow, 4).Value = "Giảm giá:";
                    worksheet.Cell(currentRow, 5).Value = order.DiscountMoney;

                    currentRow++;
                    var finalRange = worksheet.Cell(currentRow, 4);
                    finalRange.Value = "TỔNG CỘNG:";
                    finalRange.Style.Font.SetBold();
                    var finalTotal = worksheet.Cell(currentRow, 5);
                    finalTotal.Value = order.TotalAmount;
                    finalTotal.Style.Font.SetBold().Font.SetFontColor(XLColor.Red);

                    // Wifi
                    currentRow += 2;
                    var wifiInfo = worksheet.Cell(currentRow, 1).Value = "Wifi: 2G1G Wifi 5G";
                    worksheet.Range(currentRow, 1, currentRow, 5).Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                    currentRow++;
                    var wifiPassword = worksheet.Cell(currentRow, 1).Value = "Mật khẩu: Xincamon";
                    worksheet.Range(currentRow, 1, currentRow, 5).Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                    // Footer
                    currentRow += 2;
                    var footer = worksheet.Cell(currentRow, 1);
                    footer.Value = "CẢM ƠN QUÝ KHÁCH - HẸN GẶP LẠI!";
                    worksheet.Range(currentRow, 1, currentRow, 5).Merge().Style.Font.SetItalic().Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                    // Lưu file
                    workbook.SaveAs(filePath);
                }
            }
        }
    }

//string fileName = $"Bill_{newOrder.OrderId}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
//        string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Exports");
//if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

//string fullPath = Path.Combine(folderPath, fileName);

//    var exporter = new BillExporter(newOrder.OrderId);
//    await exporter.ExportToExcel(fullPath);

//    Process.Start(new ProcessStartInfo(fullPath) { UseShellExecute = true });
}
