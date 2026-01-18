using ClosedXML.Excel;
using CoffeeShop.Models;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;

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
                        .ThenInclude(od => od.Price)
                            .ThenInclude(p => p.Item)
                    .FirstOrDefaultAsync(o => o.OrderId == _orderId);

                if (order == null) return;

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Bill");

                    // 2. Thiết lập độ rộng cột (để hóa đơn cân đối)
                    worksheet.Column(1).Width = 15; // Tên món
                    worksheet.Column(2).Width = 8;  // Size
                    worksheet.Column(3).Width = 5;  // SL
                    worksheet.Column(4).Width = 12; // Đơn giá
                    worksheet.Column(5).Width = 12; // Thành tiền

                    // 3. Header: Tên quán (Bạn có thể thay đổi thông tin này)
                    var header = worksheet.Cell("A1");
                    header.Value = "COFFEE SHOP NGUYỄN CHÍ NGUYÊN";
                    worksheet.Range("A1:E1").Merge().Style.Font.SetBold().Font.SetFontSize(16).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                    worksheet.Cell("A2").Value = "Địa chỉ: Khu phố 6, P.Linh Trung, Tp.Thủ Đức, HCM";
                    worksheet.Range("A2:E2").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                    // 4. Thông tin hóa đơn
                    worksheet.Cell("A4").Value = $"Mã HĐ: {order.OrderId}";
                    worksheet.Cell("D4").Value = $"Ngày: {order.OrderDate:dd/MM/yyyy HH:mm}";
                    worksheet.Cell("A5").Value = $"Nhân viên: {order.Staff?.StaffName}";
                    worksheet.Cell("D5").Value = $"Bàn: {order.Table?.TableName ?? "Mang về"}";
                    worksheet.Cell("A6").Value = $"Khách hàng: {order.Customer?.CustomerName ?? "Khách vãng lai"}";

                    // 5. Tiêu đề bảng món ăn
                    int currentRow = 8;
                    var tableHeader = worksheet.Range(currentRow, 1, currentRow, 5);
                    tableHeader.Style.Font.SetBold().Border.SetBottomBorder(XLBorderStyleValues.Thin);
                    worksheet.Cell(currentRow, 1).Value = "Món ăn";
                    worksheet.Cell(currentRow, 2).Value = "Size";
                    worksheet.Cell(currentRow, 3).Value = "SL";
                    worksheet.Cell(currentRow, 4).Value = "Đơn giá";
                    worksheet.Cell(currentRow, 5).Value = "T.Tiền";

                    // 6. Đổ dữ liệu món ăn
                    foreach (var detail in order.OrderDetails)
                    {
                        currentRow++;
                        // Lấy thông tin Item và Size từ PriceID (vì bảng OrderDetail liên kết qua PriceId)
                        worksheet.Cell(currentRow, 1).Value = detail.Price?.Item?.ItemName ?? "N/A";
                        worksheet.Cell(currentRow, 2).Value = detail.Price?.Size?.SizeName ?? "-";
                        worksheet.Cell(currentRow, 3).Value = detail.Quantity;
                        worksheet.Cell(currentRow, 4).Value = detail.UnitPrice;
                        worksheet.Cell(currentRow, 5).Value = detail.TotalPrice;
                    }

                    // 7. Phần tổng tiền
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

                    // 8. Footer
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

//    // Trong hàm ConfirmPayOrder, sau khi transaction.Commit() thành công:
//string fileName = $"Bill_{newOrder.OrderId}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
//        string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Exports");
//if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

//string fullPath = Path.Combine(folderPath, fileName);

//    var exporter = new BillExporter(newOrder.OrderId);
//    await exporter.ExportToExcel(fullPath);

//    // Tự động mở file Excel sau khi xuất xong để in
//    Process.Start(new ProcessStartInfo(fullPath) { UseShellExecute = true });
}
