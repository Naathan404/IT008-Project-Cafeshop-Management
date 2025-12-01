CREATE DATABASE CoffeeShopDB;
GO

USE CoffeeShopDB;
GO

----- CÁC BƯỚC THỰC HIỆN TRÊN FILE QUERY NÀY
-- Bước 1: Chạy đoạn lệnh DROP các bảng trong phần THAO TÁC DỮ LIỆU
-- Bước 2: Chạy hết đoạn lệnh trong phần TẠO BẢNG
-- Bước 3: Lần lượt chạy các đoạn lệnh đến khi hết trong phần SEED DATA

-------------------------------------------------TẠO BẢNG---------------------------------------------------------------------------------------------------------------
-- Tạo bảng CafeTable
CREATE TABLE CafeTable
(
	TableID INT IDENTITY(1,1) PRIMARY KEY,
	TableName NVARCHAR(40) NOT NULL,
	TableStatus INT NOT NULL DEFAULT 0,		-- 0: trong, 1: dang phuc vu, 2: da dat truoc
	Note NVARCHAR(200) NULL,
);

-- Tạo bảng Category
CREATE TABLE Category
(
	CategoryID INT IDENTITY(1,1) PRIMARY KEY,
	CategoryName NVARCHAR(40) NOT NULL,
);

-- Tạo bảng Item
CREATE TABLE Item
(
	ItemID INT IDENTITY(1,1) PRIMARY KEY,
	ItemName NVARCHAR(50) NOT NULL,
	CategoryID INT NOT NULL,				-- FK -> Category(CategoryID)
	IsAvailable BIT NOT NULL,
	CONSTRAINT FK_Item_Category FOREIGN KEY (CategoryID) REFERENCES Category(CategoryID),
);

-- Tạo bảng Size
CREATE TABLE Size
(
	SizeID INT IDENTITY(1,1) PRIMARY KEY,
	SizeName NVARCHAR(5) NOT NULL,
);

-- Tạo bảng ItemPrice
CREATE TABLE ItemPrice (
    PriceID INT PRIMARY KEY IDENTITY(1,1),
    ItemID INT NOT NULL,
    SizeID INT NULL, -- Nếu là thức ăn thì NULL
    Price MONEY NOT NULL,
    CONSTRAINT FK_ItemPrice_Item FOREIGN KEY (ItemID) REFERENCES Item(ItemID),
    CONSTRAINT FK_ItemPrice_Size FOREIGN KEY (SizeID) REFERENCES Size(SizeID)
);

-- Tạo bảng Customer
CREATE TABLE Customer
(
	CustomerID INT IDENTITY(1,1) PRIMARY KEY,
	CustomerName NVARCHAR(200) NOT NULL,
	PhoneNumber NVARCHAR(20) UNIQUE NULL,
	Email NVARCHAR(200) NULL,
	Point INT NOT NULL DEFAULT 0,
	Tier NVARCHAR(10) DEFAULT 'VIP1'		-- VIP1, VIP10, VIP100
);

-- Tạo bảng Shift
CREATE TABLE Shift (
    ShiftID INT PRIMARY KEY IDENTITY(1,1),
    ShiftName NVARCHAR(10) NOT NULL, -- Sáng, Chiều, Tối
	StartTime TIME NOT NULL,
	EndTime TIME NOT NULL
);

-- Tạo bảng Staff
CREATE TABLE Staff
(
	StaffID INT IDENTITY(1,1) PRIMARY KEY,
	StaffName NVARCHAR(100) NOT NULL,
	Username NVARCHAR(100) UNIQUE NOT NULL,
	PasswordHash NVARCHAR(150) NOT NULL,
	StaffRole NVARCHAR(50) NOT NULL,			-- Admin, Employee
	Phonenumber NVARCHAR(20) NOT NULL,
	Email NVARCHAR(50) NOT NULL,
	ShiftID INT NULL,
	BaseSalary MONEY NULL, 
	CONSTRAINT FK_Staff_Shift FOREIGN KEY (ShiftID) REFERENCES Shift(ShiftID)
);

CREATE TABLE Discount
(
	DiscountID INT IDENTITY(1, 1) PRIMARY KEY,
	DiscountCode NVARCHAR(10) UNIQUE NOT NULL,
	DiscountName NVARCHAR(50) NOT NULL,
	DiscountType INT NOT NULL DEFAULT 0,
	DiscountValue DECIMAL(18, 2) NOT NULL,
	MinimumOrderValue MONEY DEFAULT 0,
	MaximumDiscountAmount MONEY NULL,
	IsActive BIT DEFAULT 1,
	UsedCount INT DEFAULT 0,
)

-- Tạo bảng Order
CREATE TABLE [Order]
(
	OrderID INT IDENTITY(1,1) PRIMARY KEY,
	TableID INT NULL,		-- null la mua mang di
	CustomerID INT NULL,	-- null la khach vang lai
	StaffID INT NOT NULL,
	OrderDate DATETIME NOT NULL DEFAULT GETDATE(),
	SubTotal MONEY NOT NULL,
	DiscountID INT NULL,
	DiscountMoney MONEY DEFAULT 0,
	TotalAmount MONEY NOT NULL,
	PaymentMethod NVARCHAR(40) NOT NULL,							-- Tien mat, chuyen khoan
	CONSTRAINT FK_Order_Table FOREIGN KEY (TableID) REFERENCES CafeTable(TableID),
	CONSTRAINT FK_Order_Customer FOREIGN KEY (CustomerID) REFERENCES Customer(CustomerID),
	CONSTRAINT FK_Order_Staff FOREIGN KEY (StaffID) REFERENCES Staff(StaffID),
	CONSTRAINT FK_Order_Discount FOREIGN KEY (DiscountID) REFERENCES Discount(DiscountID)
);

-- Tạo bảng OrderDetail
CREATE TABLE OrderDetail (
    OrderDetailID INT IDENTITY(1,1) PRIMARY KEY,
    OrderID INT NOT NULL,
    PriceID INT NOT NULL,
    Quantity INT NOT NULL DEFAULT 1,
	UnitPrice MONEY NOT NULL,
    TotalPrice MONEY NOT NULL,
    Note NVARCHAR(200) NULL,
    CONSTRAINT FK_OrderDetail_Order FOREIGN KEY (OrderID) REFERENCES [Order](OrderID),
	CONSTRAINT FK_OrderDetail_ItemPrice FOREIGN KEY (PriceID) REFERENCES ItemPrice(PriceID),
);

-- Tạo bảng StaffAttendance
-- CREATE TABLE StaffAttendance
-- (
--	AttendanceID INT IDENTITY(1,1) PRIMARY KEY,
--	StaffID INT NOT NULL, 
--	CheckIn DATETIME NOT NULL DEFAULT GETDATE(),
--	CheckOut DATETIME NULL,
--	CONSTRAINT FK_Attendance_Staff FOREIGN KEY(StaffID) REFERENCES Staff(StaffID),
-- );

-- Tạo bảng Inventory
CREATE TABLE Inventory
(
	MaterialID INT IDENTITY(1,1) PRIMARY KEY,
	MaterialName NVARCHAR(100),
	Quantity DECIMAL(18,2) NOT NULL DEFAULT 0,
	Unit NVARCHAR(50) NOT NULL,
	Threshold DECIMAL(18,2) NOT NULL,
	Note NVARCHAR(200) NULL
);

-- Tạo bảng ActionType
CREATE TABLE ActionType (
    ActionTypeID INT PRIMARY KEY IDENTITY(1,1),
    ActionName NVARCHAR(20) NOT NULL
);

-- Tạo bảng InventoryHistory
CREATE TABLE InventoryHistory (
    HistoryID INT IDENTITY(1,1) PRIMARY KEY,
    MaterialID INT NOT NULL,
    ActionTypeID INT NOT NULL,
    Quantity DECIMAL(18,2) NOT NULL,
	InputPrice MONEY NULL,
    Date DATETIME NOT NULL DEFAULT GETDATE(),
    StaffID INT NOT NULL,
    CONSTRAINT FK_InvHistory_Inventoy FOREIGN KEY (MaterialID) REFERENCES Inventory(MaterialID),
    CONSTRAINT FK_InvHistory_Staff FOREIGN KEY (StaffID) REFERENCES Staff(StaffID),
	CONSTRAINT FK_InvHistory_Action FOREIGN KEY (ActionTypeID) REFERENCES ActionType(ActionTypeID)
);

-- Tạo bảng Otprequest
CREATE TABLE OTPRequest
(
	RequestID int identity(1,1) primary key,
	Email varchar(100) not null,
	Code varchar(5) not null,
	ExpireTime datetime2 not null
)
----------------------------------------------------------------------------------------------------------------------------------------------------------------

--------------------------------------------------------------THAO TÁC DỮ LIỆU--------------------------------------------------------------------------------------------------

-- DROP các BẢNG
-- Xóa các bảng có khóa ngoại
DROP TABLE IF EXISTS InventoryHistory;
DROP TABLE IF EXISTS OrderDetail;
DROP TABLE IF EXISTS [Order];
DROP TABLE IF EXISTS ItemPrice;
DROP TABLE IF EXISTS Item;
DROP TABLE IF EXISTS StaffAttendance;

-- Xóa các bảng được khóa ngoại tham chiếu đến
DROP TABLE IF EXISTS Discount;
DROP TABLE IF EXISTS Staff;
DROP TABLE IF EXISTS Shift;
DROP TABLE IF EXISTS Inventory;
DROP TABLE IF EXISTS ActionType;
DROP TABLE IF EXISTS CafeTable;
DROP TABLE IF EXISTS Customer;
DROP TABLE IF EXISTS Size;
DROP TABLE IF EXISTS Category;
DROP TABLE IF EXISTS OTPRequest;
--


-- XÓA DỮ LIỆU các BẢNG
DELETE FROM InventoryHistory;
DELETE FROM OrderDetail;
DELETE FROM [Order];
DELETE FROM ItemPrice;
DELETE FROM Item;
DELETE FROM Discount;
DELETE FROM Staff;
DELETE FROM Shift;
DELETE FROM Inventory;
DELETE FROM ActionType;
DELETE FROM CafeTable;
DELETE FROM Customer;
DELETE FROM Size;
DELETE FROM Category;
DELETE FROM OTPRequest;

-- SELECT 
SELECT * FROM Staff

------------------------------------------------------------------SEED DATA----------------------------------------------------------------------------------------------
INSERT INTO Category (CategoryName) 
VALUES 
(N'Cà phê'),
(N'Trà sữa'),
(N'Đá xay'),
(N'Trà'),
(N'Nước ép'),
(N'Sinh tố'),
(N'Food');
GO

INSERT INTO Item (ItemName, CategoryID, IsAvailable) 
VALUES 
(N'Cà phê sữa đá', 1, 1),
(N'Đen đá không đường', 1, 1),
(N'Trà sữa truyền thống', 2, 1),
(N'Cà phê muối', 1, 1),
(N'Cà phê đen', 1, 1),
(N'Trà sữa kem cheese', 2, 1),
(N'Bạc xỉu', 1, 1),
(N'Trà sữa tiramisu', 2, 1),
(N'Trà sữa olong', 2, 1),
(N'Trà sữa gạo rang', 2, 1),
(N'Trà trái cây nhiệt đới', 4, 1),
(N'Trà đào', 4, 1),
(N'Trà vải', 4, 1),
(N'Nước ép cam', 5, 1),
(N'Nước ép thơm', 5, 1),
(N'Sinh tố bơ', 6, 1),
(N'Sinh tố dâu', 6, 1),
(N'Sinh tố sầu riêng', 6, 1),
(N'Sinh tố mãng cầu', 6, 1),
(N'Trà chanh giã tay', 4, 1),
(N'Bánh crossant', 7, 1),
(N'Bánh bông lan trứng muối', 7, 1),
(N'Bánh tráng trộn', 7, 1),
(N'Bánh tráng cuộn', 7, 1),
(N'Panna cotta', 7, 1);
GO

INSERT INTO Size (SizeName)
VALUES
('S'),
('M'),
('L')
GO

INSERT INTO ItemPrice (ItemID, SizeID, Price) 
VALUES 
(1, 1, 19000), (1, 2, 29000), (1, 3, 39000),   -- Cà phê sữa đá
(2, 1, 19000), (2, 2, 25000), (2, 3, 29000),   -- Đen đá không đường
(3, 1, 29000), (3, 2, 35000), (3, 3, 39000),   -- Trà sữa truyền thống
(4, 1, 20000), (4, 2, 25000),                  -- Cà phê muối (Chỉ có size S, M)
(5, 1, 18000), (5, 2, 23000),                  -- Cà phê đen (Chỉ có size S, M)
(6, 1, 29000), (6, 2, 35000), (6, 3, 39000),   -- Trà sữa kem cheese
(7, 1, 20000), (7, 2, 25000), (7, 3, 30000),   -- Bạc xỉu
(8, 1, 29000), (8, 2, 35000), (8, 3, 39000),   -- Trà sữa tiramisu
(9, 1, 29000), (9, 2, 35000), (9, 3, 39000),   -- Trà sữa olong
(10, 1, 29000), (10, 2, 35000), (10, 3, 39000), -- Trà sữa gạo rang
(11, 1, 25000), (11, 2, 30000), (11, 3, 35000), -- Trà trái cây nhiệt đới
(12, 1, 25000), (12, 2, 30000), (12, 3, 35000), -- Trà đào
(13, 1, 25000), (13, 2, 30000), (13, 3, 35000), -- Trà vải
(14, 1, 18000), (14, 2, 23000), (14, 3, 28000), -- Nước ép cam
(15, 1, 18000), (15, 2, 23000), (15, 3, 28000), -- Nước ép thơm
(16, 1, 29000), (16, 2, 35000), (16, 3, 39000), -- Sinh tố bơ
(17, 1, 29000), (17, 2, 35000), (17, 3, 39000), -- Sinh tố dâu
(18, 1, 29000), (18, 2, 35000), (18, 3, 39000), -- Sinh tố sầu riêng
(19, 1, 29000), (19, 2, 35000), (19, 3, 39000), -- Sinh tố mãng cầu
(20, 1, 25000), (20, 2, 30000), (20, 3, 35000), -- Trà chanh giã tay
(21, NULL, 35000),                             -- Bánh crossant (Food không có size)
(22, NULL, 30000),                             -- Bánh bông lan trứng muối
(23, NULL, 25000),                             -- Bánh tráng trộn
(24, NULL, 25000);                             -- Bánh tráng cuộn
GO

INSERT INTO Customer (CustomerName, PhoneNumber, Email, Point, Tier) 
VALUES 
(N'Nguyễn Văn An', '0901234567', 'annguyen@gmail.com', 2500, 'VIP100'),
(N'Trần Thị Bích', '0918889999', 'bichtran@gmail.com', 1200, 'VIP10'),
(N'Lê Hoàng Nam', '0987654321', 'namle@gmail.com', 600, 'VIP1'),
(N'Phạm Minh Tuấn', '0933445566', 'tuanpham@gmail.com', 150, 'VIP1'), 
(N'Võ Thị Mai', '0912341234', 'maivo@gmail.com', 50, 'VIP1'),
(N'Nguyễn Chí Nguyên', '0865320821', '24521186@gm.uit.edu.vn', 1500, 'VIP10');
GO

INSERT INTO CafeTable (TableName, TableStatus, Note) 
VALUES 
-- Nhóm Bàn 2 người 
(N'Bàn 01', 1, N'Bàn 2 người - Gần cửa sổ'),
(N'Bàn 02', 0, N'Bàn 2 người'),            
(N'Bàn 03', 0, N'Bàn 2 người'),
(N'Bàn 04', 0, N'Bàn 2 người - Góc yên tĩnh'),
(N'Bàn 05', 2, N'Bàn 2 người'),         
-- Nhóm Bàn 4 người
(N'Bàn 06', 1, N'Bàn 4 người - Trung tâm'),
(N'Bàn 07', 0, N'Bàn 4 người'),
(N'Bàn 08', 0, N'Bàn 4 người'),
(N'Bàn 09', 0, N'Bàn 4 người'),
(N'Bàn 10', 1, N'Bàn 4 người'),
(N'Bàn 11', 0, N'Bàn 4 người'),
(N'Bàn 12', 0, N'Bàn 4 người - Gần quầy bar'),
(N'Bàn 13', 2, N'Bàn 4 người - Khách quen'),
(N'Bàn 14', 0, N'Bàn 4 người'),
(N'Bàn 15', 0, N'Bàn 4 người'),
-- Nhóm Bàn 6 người
(N'Bàn 16', 0, N'Bàn 6 người - Sofa'),
(N'Bàn 17', 1, N'Bàn 6 người'),
(N'Bàn 18', 0, N'Bàn 6 người'),
-- Nhóm Bàn Tròn
(N'Bàn 19', 0, N'Bàn tròn - View đẹp'),
(N'Bàn 20', 2, N'Bàn tròn - VIP');
GO

INSERT INTO Shift (ShiftName, StartTime, EndTime) 
VALUES 
(N'Sáng', '06:30:00', '14:30:00'),   -- Ca 8 tiếng
(N'Chiều', '14:30:00', '22:30:00'),  -- Ca 8 tiếng
(N'Tối', '17:00:00', '23:00:00');    -- Ca 6 tiếng part time
GO

INSERT INTO Staff
VALUES
(N'ADMIN', 'admin', N'bf0dbd74174039131b667de9f31b5d8012baaf82011b934b2cc0e3bd53a02a1f', N'Admin', '0865320821', N'coffeeshop2g1g@gmail.com', NULL, NULL),
(N'Nguyễn Chí Nguyên', N'ngnguyen', N'38d180985d1b2e7a6014190e2cbd3c967408837188354ec93d27bfd86d09a017', N'Admin', '0865320821', N'nathannguyen6002@gmail.com', NULL, NULL),
(N'Nguyễn Ngọc Lan Anh', N'ngnlananh', N'bf0dbd74174039131b667de9f31b5d8012baaf82011b934b2cc0e3bd53a02a1f', N'Employee', '0988888888', N'ngnlananh@gmail.com', 2, 25000),
(N'Lê Thành Nghĩa', N'ltnghia', N'fa980dbf4533c98fa5ed792374bea691610dfaabc62558182f4cc814ef0d69db', N'Employee', '0977778888', N'24521143@gm.uit.edu.vn', 1, 20000)
GO

INSERT INTO Discount (DiscountCode, DiscountName, DiscountType, DiscountValue, MinimumOrderValue, MaximumDiscountAmount) 
VALUES 
('VIP1', N'Giảm 3K cho VIP1', 1, 3000, 0, NULL),
('VIP10', N'Giảm 6K cho VIP10', 1, 6000, 0, NULL),
('VIP100', N'Giảm 10K cho VIP100', 1, 10000, 0, NULL),
('CF05', N'Giảm 5% cho hóa đơn từ 100K có mua cà phê', 0, 5, 100000, 10000)
GO

INSERT INTO [Order] (TableID, CustomerID, StaffID, OrderDate, SubTotal, DiscountID, DiscountMoney, TotalAmount, PaymentMethod) 
VALUES 
(1, 1, 3, '2025-12-1 07:00:00', 54000, 3, 10000, 44000, N'Tiền mặt'), --1
(NULL, 2, 3, '2025-12-1 13:00:00', 18000, 2, 6000, 12000, N'Chuyển khoản'), --2
(3, 1, 3, '2025-12-1 16:00:00', 53000, 3, 10000, 43000, N'Chuyển khoản'), --3
(NULL, NULL, 4, '2025-12-1 17:15:00', 56000, NULL, 0, 56000, N'Tiền mặt'), --4
(NULL, 2, 4, '2025-12-1 20:00:00', 125000, 2, 6000, 119000, N'Chuyển khoản'), --5
(10, NULL, 3, '2025-12-1 20:20:00', 55000, NULL, 0, 55000, N'Tiền mặt'), --6
GO

INSERT INTO OrderDetail (OrderID, PriceID, Quantity, UnitPrice, TotalPrice, Note) 
VALUES 
(1, 1, 1, 19000, 19000, N'Ít sữa, nhiều cafe'),
(1, 8, 1, 35000, 35000, N'70% đường, 30% đá'),
(2, 12, 1, 18000, 18000, N'Đá để riêng'),
(3, 13, 1, 23000, 23000, NULL),
(3, 60, 1, 30000, 30000, NULL),
(4, 40, 2, 28000, 56000, NULL),
(5, 1, 2, 19000, 38000, N'1 ly không đường, 1 ly bình thường'),
(5, 16, 1, 39000, 39000, N'Không lấy sữa đặc'),
(5, 23, 1, 29000, 23000, N'Không cay'),
(5, 5, 1, 25000, 25000, NULL),
(6, 17, 1, 20000, 20000, N'Ít ngọt'),
(6, 21, 1, 35000, 35000, N'Hâm nóng')
GO

--INSERT INTO StaffAttendance (StaffID, CheckIn, CheckOut) 
--VALUES 
--(1, '2025-11-16 07:00:00', '2025-11-16 11:30:00'),
--(1, '2025-11-17 07:05:00', NULL),
--(1, '2025-11-18 07:00:00', NULL)
--GO

INSERT INTO Inventory (MaterialName, Quantity, Unit, Threshold, Note) 
VALUES 
(N'Hạt Cà phê Robusta', 15.5, N'kg', 5.0, NULL),      
(N'Sữa đặc Ngôi Sao', 48, N'Lon', 10.0, NULL),        
(N'Sữa tươi không đường', 12, N'Hộp 1L', 6.0, N'Sắp hết hạn'),  
(N'Trà đen (Hồng trà)', 4.5, N'kg', 2.0, NULL),
(N'Trà lài (Lục trà)', 3.0, N'kg', 1.5, NULL),
(N'Bột Matcha', 0.4, N'kg', 0.5, NULL),          
(N'Đường cát trắng', 20, N'kg', 5.0, NULL),
(N'Syrup Đào', 5, N'Chai', 2.0, NULL),
(N'Syrup Vải', 1, N'Chai', 2.0, NULL),                
(N'Mứt Dâu', 3, N'Hũ', 1.0, NULL),
(N'Trân châu đen', 8, N'kg', 3.0, N'Sắp hết hạn'),
(N'Thạch trái cây', 5, N'Hộp', 2.0, NULL),
(N'Cam vàng', 10, N'kg', 3.0, NULL),
(N'Chanh tươi', 2.5, N'kg', 1.0, NULL)
GO

INSERT INTO ActionType (ActionName) 
VALUES 
(N'Nhập'),     
(N'Dùng'),       
(N'Cập nhật'),    
(N'Hủy')    
GO

INSERT INTO InventoryHistory (MaterialID, ActionTypeID, Quantity, InputPrice, Date, StaffID) 
VALUES 
-- 1. Đợt nhập hàng lớn cách đây 5 ngày 
(1, 1, 20, 150000, DATEADD(day, -5, GETDATE()), 2),  -- Nhập 20kg Cà phê Robusta, giá vốn 150k/kg
(2, 1, 50, 22000, DATEADD(day, -5, GETDATE()), 2),   -- Nhập 50 lon Sữa đặc, giá vốn 22k/lon
(3, 1, 20, 30000, DATEADD(day, -5, GETDATE()), 2),   -- Nhập 20 hộp Sữa tươi, giá vốn 30k/hộp
-- 2. Ghi nhận sử dụng nguyên liệu cách đây 2 ngày
(1, 2, -2.5, NULL, DATEADD(day, -2, GETDATE()), 1),  -- Đã dùng 2.5kg Cà phê để bán
(2, 2, -2, NULL, DATEADD(day, -2, GETDATE()), 1),    -- Đã khui 2 lon sữa đặc
-- 3. Ghi nhận hàng hỏng hôm qua 
(3, 3, -1, NULL, DATEADD(day, -1, GETDATE()), 2),    -- Hủy 1 hộp sữa tươi do bị rách bao bì
(6, 3, -0.1, NULL, DATEADD(day, -1, GETDATE()), 2),  -- Hủy 0.1kg Matcha do ẩm mốc
-- 4. Nhập hàng bổ sung hôm nay
(6, 1, 0.5, 450000, GETDATE(), 1)                    -- Nhập thêm 0.5kg Matcha xịn
GO
----------------------------------------------------------------------------------------------------------------------------------------------------------------