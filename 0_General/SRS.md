# Software Requirements Specification - Cofe Shop Management App

## Mục tiêu
-	Giao tiếp giữa nhân viên và hệ thống đặt món
- Quản lý bàn, menu, kho hàng
-	Xử lý hóa đơn, thanh toán (có thể in chi tiết hóa đơn)
-	Quản lý nhân viên, khách hàng thành viên
-	Báo cáo doanh thu theo ngày, tháng, năm
-	Phân quyền: Admin, Staff

## Các đối tượng cần quản lý
-	Table: Danh sách các bàn trong quán, hiển thị trực quan. 3 trạng thái: Trống, đang phục vụ, đã đặt trước
-	Menu: Các món nước, thức ăn theo từng phân mục (Cà phê, Trà, Sinh tố, Ăn nhẹ,...)
-	Order/Bill: Thông tin order theo bàn, các món, thời gian xuất hd, loại khách, trạng thái thanh toán
-	Inventory: Quản lý được nguyên liệu, số lượng tồn, nhập, xuất kho
-   Staff: Thông tin cá nhân, tài khoản, phân quyền
- Thống kê doanh thu: Theo ca, ngày, tháng, băn, món best seller
-   Khách hàng: Vãng lai hay thành viên, tích điểm, giảm giá
- Lịch sử giao dịch:

## 3. Chức năng chính
### Quản lý bàn
- Hiển thị sơ đồ bàn
- Chuyển trạng thái bàn: Trống ⇄ Đang phục vụ ⇄ Đã đặt
- Gộp bàn, chuyển bàn

### Tạo Order
- Tạo đơn hàng theo bàn
- Thêm/xoá món trong order
- Ghi chú món (ít đường, không đá…)
- Chọn voucher nếu có
- Hình thức thanh toán
- Trạng thái thanh toán

### Quản lý menu
- Thêm, sửa, xoá món
- Phân loại món theo nhóm
- Cập nhật giá bán
- Ẩn/hiện món tạm thời
- Mỗi món có ít nhất 2 size (trừ đồ ăn nếu có)

### Quản lý hóa đơn
- Tạo hóa đơn từ order, và 1 phiếu cho barista
- In hoá đơn (bill), ghi vào lịch sử doanh thu

### Quản lý kho
- Nhập - xuất kho nguyên vật liệu
- Cập nhật số lượng tồn kho
- Cảnh báo khi nguyên liệu sắp hết
- Xem lịch sử nhập - xuất

### Quản lý nhân viên
- Tạo tài khoản cho nhân viên
- Quản lý ca làm, xếp ca, check-in check-out
- Tính lương tự động

### Khách hàng thành viên
- Đăng ký thông tin khách
- Tích điểm, theo dõi lịch sử mua hàng
- Áp dụng ưu đãi theo điểm hoặc cấp bậc (Silver/Gold/Platinum)
- Tự động gửi khuyến mãi qua email/SMS (nếu có khả năng làm được)
  
### Thống kê \& báo cáo
- Doanh thu theo: ca, ngày, tháng, năm
- Các món bán chạy nhất
- Xuất báo cáo ra Excel/PDF
- Xem trực quan bằng biểu đồ
- Thống kê lượng khách, hóa đơn theo khung giờ

## Công nghệ sử dụng
- WPF (MVVM model)
- Entity Framework Core
- Material Design Framework
- MailKit (thư viện làm việc với mail cảu cs)

## UI dự kiến
- Màn hình đăng nhập
- Màn hình sơ đồ bàn
- Màn hình gọi món
- Màn hình thanh toán
- Màn hình quản lý menu
- Màn hình quản lý kho
- Màn hình thống kê

\- Màn hình quản lý nhân viên

\- ...



