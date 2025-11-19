# ☕ [IT008.Q14] QUẢN LÝ QUÁN CÀ PHÊ ☕

## LỜI NÓI ĐẦU
Ứng dụng *Quản lý quán cà phê* là sản phẩm đồ án của môn học Lập trình trực quan - IT008, Trường Đại học Công nghệ Thông tin, ĐHQG-HCM.
Ứng dụng được lên ý tưởng, xây dựng và phát triển trong thời gian 2 tháng. Nhóm sinh viên thực hiện bao gồm:

| STT | Họ tên | Mã số sinh viên |
| :---: | :--- | :--- |
| 1 | Nguyễn Chí Nguyên | 24521186 |
| 2 | Nguyễn Ngọc Lan Anh | 24520109 |
| 3 | Lê Thành Nghĩa| 24521143 |

---
Trong bối cảnh kinh doanh dịch vụ đồ uống (F&B) tại Việt Nam ngày càng phát triển mạnh mẽ, các mô hình quán cà phê mọc lên ngày càng nhiều với quy mô từ nhỏ đến lớn. Đi kèm với sự phát triển đó là áp lực về việc vận hành và quản lý sao cho hiệu quả.  
Việc quản lý thủ công bằng thủ công bằng sổ sách hay các công cụ như Excel, Google Sheet tỏ ra kém hiệu quả và bộc lộ nhiều hạn chế.  
Xuất phát từ thực tế đó, đề tài *“Xây dựng phần mềm Quản lý quán cà phê”* được thực hiện nhằm cung cấp một giải pháp số hóa toàn diện quy trình vận hành của một quán cà phê trong thực tế.

## I. GIỚI THIỆU CHUNG
### 1. Cấu trúc dự án 📂
```
.
├── Project/
│   ├── CoffeeShop/
│   │   ├── Assets/
│   │   │   ├── Images/
│   │   │   └── Fonts/
│   │   ├── Helper/
│   │   │   └── * Các Class hỗ trợ 
│   │   ├── Models/
│   │   │   └── * Các Class ánh xạ đến cơ sở dữ liệu 
│   │   ├── Resources/
│   │   └── View/
│   │       ├── Admin/
│   │       ├── Login/
│   │       └── Staff/
│   └── CoffeShopDB_Query.sql
├── README.md
├── SRS.md
├── CodingConvention.md
└── .gitignore
```
--- 
### 2. Công nghệ sử dụng ⚙️
Ứng dụng được xây dựng và phát triển hoàn toàn trên nền tảng .NET phiên bản 8.0 do Microsoft phát triển. Các công nghệ cụ thể được sử dụng là:
```
- Ngôn ngữ lập trình: C#.
- Nền tảng phát triển: WPF (Windows Presentation Foundation).
- Hệ quản trị cơ sở dữ liệu: Microsoft SQL Server.
- Công nghệ truy xuất dữ liệu: EFC (Entity Framework Core).
- Các thư viện khác: MaterialDesignXaml, MailKit&MimeKit.
```
---
### 3. Công cụ hỗ trợ 🛠️
```
- IDE: Visual Studio 2022.
- Quản lý cơ sở dữ liệu: SSMS (Microsoft SQL Server Managerment Studio).
- Quản lý code và phiên bản: Git & GitHub.
- Thiết kế giao diện: Figma, Canva.
```
---
### 4. Các chức năng chính
🔐 **Nhóm chức năng hệ thống và bảo mật:**
- Đăng nhập/đăng xuất.
- Khôi phục mật khẩu.  

🗃️ **Nhóm chức năng dành cho quản lý:**
- Quản lý nhân viên.
- Quản lý thực đơn.
- Quản lý kho hàng.
- Thống kê doanh thu.

💸 **Nhóm chức năng dành cho nhân viên:**
- Bán hàng.
- Tạo hóa đơn và xuất hóa đơn.
- Quản lý trạng thái các bàn trong quán.
- Xem lịch sử bán hàng.
- Hiển thị lịch sử bán hàng và danh sách các khách hàng thành viên.
- Kiểm kê kho hàng.
---

## II. DATABASE - SQL SERVER
### 1. Các đối tượng quản lý 📦 
- Nhân viên
- Khách hàng
- Menu
- Order - Hóa đơn
- Kho hàng
- Bàn
### 2. Kết nối cơ sở dữ liệu ở local 🔗
#### Bước 1: Tạo cơ sở dữ liệu
- Khởi động SQL Server Management Studio. Mở file `CoffeeShopDB_Query.sql`.
- Chạy các đoạn mã bên trong để tạo cơ sở dữ liệu.
#### Bước 2: Cài đặt thư viện
- Mở dự án. Chuột trái vào **`Tools`** -> **`NuGet Package Manager`** -> **`Manage NuGet Packages for Solution`**
- Tìm và cài đặt đủ 3 packages
```
Microsoft.EntityFrameworkCore.SqlServer
Microsoft.EntityFrameworkCore.Tools
Microsoft.EntityFrameworkCore.Design
```
#### Bước 3: Liên kết SQL Server bằng Scaffold
- Chuột trái vào **`Tools`** -> **`NuGet Package Manager`** -> **`Package Manager Console`**
- Chạy đoạn lệnh bên dưới trong Console vừa mở:

```shell
Scaffold-DbContext "Server=Your_Server_Name;Database=CoffeeShopDB;Trusted_Connection=True;TrustServerCertificate=True;" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -Context CoffeeShopContext -Force
```
- Your_Server_Name là tên máy chủ SQL Server của bạn, thường là `.` hoặc `(local)` hoặc `TEN_MAY_TINH\SQLEXPRESS` hoặc `TEN_MAY_TINH` tùy theo tên tên mà bạn đặt.

#### Bước 4: Chạy ứng dụng
- Build và run ứng dụng.
- Đăng nhập thử bằng tài khoản bên dưới:
```
Username: admin
Password: 123
```

## III. NHẬT KÝ PHÁT TRIỂN ỨNG DỤNG
