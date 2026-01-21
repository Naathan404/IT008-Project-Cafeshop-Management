CREATE DATABASE CoffeeShopDB;
GO

USE CoffeeShopDB;
GO

select * from staff

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
    IsDeleted BIT NOT NULL DEFAULT 0
);

-- Tạo bảng Category
CREATE TABLE Category
(
	CategoryID INT IDENTITY(1,1) PRIMARY KEY,
	CategoryName NVARCHAR(40) NOT NULL,
    IsDeleted BIT NOT NULL DEFAULT 0
);

-- Tạo bảng Item
CREATE TABLE Item
(
	ItemID INT IDENTITY(1,1) PRIMARY KEY,
	ItemName NVARCHAR(50) NOT NULL,
    ImagePath NVARCHAR(400) NULL DEFAULT '/Assets/Images/imgItemExample.jpg',
    Info NVARCHAR(400) NULL,
	CategoryID INT NOT NULL,				-- FK -> Category(CategoryID)
	IsAvailable BIT NOT NULL,
    IsDeleted BIT NOT NULL DEFAULT 0

	CONSTRAINT FK_Item_Category FOREIGN KEY (CategoryID) REFERENCES Category(CategoryID),
);

-- Tạo bảng Size
CREATE TABLE Size
(
	SizeID INT IDENTITY(1,1) PRIMARY KEY,
	SizeName NVARCHAR(5) NOT NULL,
    IsDeleted BIT NOT NULL DEFAULT 0
);

-- Tạo bảng ItemPrice
CREATE TABLE ItemPrice (
    PriceID INT PRIMARY KEY IDENTITY(1,1),
    ItemID INT NOT NULL,
    SizeID INT NULL, -- Nếu là thức ăn thì NULL
    Price MONEY NOT NULL,
    IsDeleted BIT NOT NULL DEFAULT 0

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
	Tier NVARCHAR(10) DEFAULT 'VIP1',		-- VIP1, VIP10, VIP100
    JoinDate DATETIME NOT NULL,
    IsDeleted BIT NOT NULL DEFAULT 0
);

-- Tạo bảng Shift
CREATE TABLE Shift (
    ShiftID INT PRIMARY KEY IDENTITY(1,1),
    ShiftName NVARCHAR(10) NOT NULL, -- Sáng, Chiều, Tối
	StartTime TIME NOT NULL,
	EndTime TIME NOT NULL,
    IsDeleted BIT NOT NULL DEFAULT 0
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
    Birthday DATETIME NOT NULL,
    StartDate DATETIME NOT NULL,
    Gender NVARCHAR(5) NOT NULL,
	ShiftID INT NULL,
	BaseSalary MONEY NULL, 
    IsDeleted BIT NOT NULL DEFAULT 0

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
	IsActive BIT DEFAULT 1 NOT NULL,
    IsDeleted BIT DEFAULT 0 NOT NULL,
	UsedCount INT DEFAULT 0 NOT NULL,
    UseLimit INT DEFAULT 0 NOT NULL,
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
	Note NVARCHAR(200) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0
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
SELECT * FROM InventoryHistory;
SELECT * FROM OrderDetail;
SELECT * FROM [Order];
SELECT * FROM ItemPrice;
SELECT * FROM Item;
SELECT * FROM Discount;
SELECT * FROM Staff;
SELECT * FROM Shift;
SELECT * FROM Inventory;
SELECT * FROM ActionType;
SELECT * FROM CafeTable;
SELECT * FROM Customer;
SELECT * FROM Size;
SELECT * FROM Category;
SELECT * FROM OTPRequest;

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

INSERT INTO Item (ItemName, CategoryID, IsAvailable, Info, ImagePath) 
VALUES 
(N'Cà phê sữa đá', 1, 1, N'Vị truyền thống, đậm đà, độ ngọt vừa phải.', '/Assets/Images/Menu/imgCFSuaDa.png'),
(N'Đen đá không đường', 1, 1, N'Cafe nguyên chất 100%, vị đắng mạnh.', '/Assets/Images/Menu/imgCFDenDa.png'),
(N'Trà sữa truyền thống', 2, 1, N'Vị trà và sữa cân bằng, có sẵn trân châu.', '/Assets/Images/Menu/imgTSTruyenThong.png'),
(N'Cà phê muối', 1, 1, N'Hot trend. Vị mặn nhẹ của lớp kem muối.', '/Assets/Images/Menu/imgCFMuoi.png'),
(N'Cà phê đen', 1, 1, N'Có đường nhẹ, vẫn giữ vị đắng đặc trưng.', '/Assets/Images/Menu/imgCFDen.png'),
(N'Trà sữa kem cheese', 2, 1, N'Điểm nhấn là lớp kem phô mai mặn béo.', '/Assets/Images/Menu/imgTSKemCheese.png'),
(N'Bạc xỉu', 1, 1, N'Nhiều sữa đặc, rất ít cafe.', '/Assets/Images/Menu/imgCFBacXiu.png'),
(N'Trà sữa tiramisu', 2, 1, N'Vị bánh Tiramisu thơm mùi ca cao.', '/Assets/Images/Menu/imgTSTiramisu.png'),
(N'Trà sữa matcha', 2, 1, N'Vị trà nướng đậm đà hơn trà thường.', '/Assets/Images/Menu/imgTSMatcha.png'),
(N'Trà sữa gạo rang', 2, 1, N'Mùi gạo rang rất thơm và dễ chịu.', '/Assets/Images/Menu/imgTSGaoRang.png'),
(N'Trà trái cây nhiệt đới', 4, 1, N'Kết hợp Dưa hấu, Táo, Cam...', '/Assets/Images/Menu/imgTraTraiCay.png'),
(N'Trà đào', 4, 1, N'Trà đen kết hợp miếng đào ngâm giòn.', '/Assets/Images/Menu/imgTraDao.png'),
(N'Trà vải', 4, 1, N'Hương vải thơm nồng nàn, có trái vải tươi.', '/Assets/Images/Menu/imgTraVaiHoaHong.png'),
(N'Nước ép cam', 5, 1, N'Cam tươi vắt 100%, bổ sung Vitamin C.', '/Assets/Images/Menu/imgEpCam.png'),
(N'Nước ép thơm', 5, 1, N'Ép từ trái thơm chín, vị chua ngọt.', '/Assets/Images/Menu/imgEpThom.png'),
(N'Sinh tố bơ', 6, 1, N'Dùng bơ sáp, xay đặc và dẻo.', '/Assets/Images/Menu/imgSinhToBo.png'),
(N'Sinh tố dâu', 6, 1, N'Vị chua nhiều hơn ngọt, màu hồng đẹp.', '/Assets/Images/Menu/imgSinhToDau.png'),
(N'Sinh tố sầu riêng', 6, 1, N'Mùi rất nồng và đặc trưng, cực kỳ béo.', '/Assets/Images/Menu/imgSinhToSauRieng.png'),
(N'Sinh tố mãng cầu', 6, 1, N'Vị chua chua ngọt ngọt rất lạ miệng.', '/Assets/Images/Menu/imgSinhToMangCau.png'),
(N'Trà chanh giã tay', 4, 1, N'Dùng chanh Quảng Đông giã tay thơm.', '/Assets/Images/Menu/imgTraChanh.png'),
(N'Bánh crossant', 7, 1, N'Bánh sừng trâu, vỏ giòn ruột xốp.', '/Assets/Images/Menu/imgCrossant.png'),
(N'Bánh bông lan trứng muối', 7, 1, N'Cốt bánh mềm, có sốt dầu trứng béo.', '/Assets/Images/Menu/imgBongLanTrungMuoi.png'),
(N'Bánh tráng trộn', 7, 1, N'Vị chua cay mặn ngọt, có khô bò, xoài.', '/Assets/Images/Menu/imgTrangTron.png'),
(N'Bánh tráng cuộn', 7, 1, N'Cuộn chặt tay với sốt bơ và tôm khô.', '/Assets/Images/Menu/imgTrangCuon.png'),
(N'Panna cotta', 7, 1, N'Thạch kem sữa mềm mịn kiểu Ý.', '/Assets/Images/Menu/imgPannaCotta.png'),
(N'Cà phê đá xay', 3, 1, N'Cafe xay nhuyễn với đá, trên có kem tươi.', '/Assets/Images/Menu/imgCaPheDaXay.png'),
(N'Matcha đá xay', 3, 1, N'Dùng bột Matcha Nhật, vị chát nhẹ.', '/Assets/Images/Menu/imgMatchaDaXay.png'),
(N'Chocolate đá xay', 3, 1, N'Đậm vị sô-cô-la, hơi đắng nhẹ.', '/Assets/Images/Menu/imgChocolateDaXay.png'),
(N'Cookies đá xay', 3, 1, N'Xay chung với bánh Oreo, béo ngậy.', '/Assets/Images/Menu/imgCookiesDaXay.png'),
(N'Mocha đá xay', 3, 1, N'Kết hợp Cafe và Chocolate.', '/Assets/Images/Menu/imgMochaDaXay.png'),
(N'Nước ép dưa hấu', 5, 1, N'Nước ép từ dưa hấu thanh mát, đậm chất nhiệt đới tropical.', '/Assets/Images/Menu/imgEpDuaHau.png'),
(N'Nước ép dưa lưới', 5, 1, N'Nước ép từ dưa lưới thanh mát, đậm chất ôn hòa.', '/Assets/Images/Menu/imgEpDuaLuoi.png'),
(N'Cà phê cốt dừa', 1, 1, N'Sự kết hợp hoàn hảo giữa cafe đậm đà và cốt dừa béo ngậy xay đá tuyết.', '/Assets/Images/Menu/imgCFCotDua.png'),
(N'Cà phê trứng', 1, 1, N'Đặc sản Hà Nội. Lớp kem trứng bông mịn, béo thơm hòa quyện cùng cafe nóng.', '/Assets/Images/Menu/imgCFTrung.png'),
(N'Cold Brew Cam Sả', 1, 1, N'Cafe ủ lạnh thanh khiết kết hợp hương vị cam sả tươi mát, giải nhiệt cực tốt.', '/Assets/Images/Menu/imgColdBrewCamSa.png'),
(N'Trà sen nhãn', 4, 1, N'Vị trà thanh tao hòa quyện cùng nhãn lồng mọng nước và hạt sen bùi bùi.', '/Assets/Images/Menu/imgTraSenNhan.png'),
(N'Trà thanh long đỏ', 4, 1, N'Màu đỏ tự nhiên từ thanh long, vị ngọt thanh, giàu vitamin và đẹp da.', '/Assets/Images/Menu/imgTraThanhLong.png')
GO

INSERT INTO Size (SizeName)
VALUES
('S'),
('M'),
('L')
GO

INSERT INTO ItemPrice (ItemID, SizeID, Price) 
VALUES 
(1, 1, 19000), (1, 2, 24000), (1, 3, 29000),   -- Cà phê sữa đá
(2, 1, 19000), (2, 2, 25000), (2, 3, 29000),   -- Đen đá không đường
(3, 1, 29000), (3, 2, 35000), (3, 3, 39000),   -- Trà sữa truyền thống
(4, 1, 20000), (4, 2, 25000),                  -- Cà phê muối (Chỉ có size S, M)
(5, 1, 18000), (5, 2, 23000),                  -- Cà phê đen (Chỉ có size S, M)
(6, 1, 29000), (6, 2, 35000), (6, 3, 39000),   -- Trà sữa kem cheese
(7, 1, 20000), (7, 2, 25000), (7, 3, 30000),   -- Bạc xỉu
(8, 1, 29000), (8, 2, 35000), (8, 3, 39000),   -- Trà sữa tiramisu
(9, 1, 29000), (9, 2, 35000), (9, 3, 39000),   -- Trà sữa olong
(10, 1, 29000), (10, 2, 35000), (10, 3, 39000), -- Trà sữa gạo rang
(11, 1, 29000), (11, 2, 34000), (11, 3, 39000), -- Trà trái cây nhiệt đới
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
(24, NULL, 25000),                             -- Bánh tráng cuộn
(25, NULL, 25000),                             -- Panna cotta
(26, 1, 29000), (26, 2, 35000), (26, 3, 39000),
(27, 1, 29000), (27, 2, 35000), (27, 3, 39000),
(28, 1, 29000), (28, 2, 35000), (28, 3, 39000),
(29, 1, 34000), (29, 2, 39000), (29, 3, 45000),
(30, 1, 29000), (30, 2, 35000), (30, 3, 39000),
(31, 1, 18000), (31, 2, 23000), (31, 3, 28000), -- Ép dưa hấu
(32, 1, 18000), (32, 2, 23000), (32, 3, 28000), -- Ép dưa lưới
(33, 1, 19000), (33, 2, 29000), (33, 3, 39000),   -- Cà phê cốt dừa
(34, 2, 29000), (34, 3, 35000),   -- Cà phê trứng
(35, 2, 34000), (35, 3, 39000),   -- Cold brew cam sả
(36, 1, 25000), (36, 2, 30000), (36, 3, 35000), -- Trà sen nhãn
(37, 1, 25000), (37, 2, 30000), (37, 3, 35000) -- Trà thanh long đỏ
GO

INSERT INTO Customer (CustomerName, PhoneNumber, Email, Point, Tier, JoinDate) 
VALUES 
(N'Nguyễn Văn An', '0901234567', 'annguyen@gmail.com', 3350, 'VIP100', '2024-12-20'),
(N'Trần Thị Bích', '0918889999', 'bichtran@gmail.com', 1200, 'VIP10', '2025-2-28'),
(N'Lê Hoàng Nam', '0987654321', 'namle@gmail.com', 600, 'VIP1', '2025-8-13'),
(N'Phạm Minh Tuấn', '0933445566', 'tuanpham@gmail.com', 150, 'VIP1', '2025-11-28'), 
(N'Võ Thị Mai', '0912341234', 'maivo@gmail.com', 50, 'VIP1', '2026-1-4'),
(N'Đặng Thu Thảo', '0909112233', 'thuthao.dang@gmail.com', 3200, 'VIP100', '2025-4-5'),
(N'Hoàng Văn Minh', '0913556677', 'minh.hoang123@gmail.com', 850, 'VIP1', '2025-7-12'),
(N'Lý Gia Hân', '0988776655', 'hanly.cute@gmail.com', 100, 'VIP1', '2025-11-16'),
(N'Phan Thanh Tâm', '0933998877', 'tamphan.dev@gmail.com', 1800, 'VIP10', '2025-6-21'),
(N'Bùi Quốc Đạt', '0977445566', 'datbui.quoc@gmail.com', 4500, 'VIP100', '2024-8-30'),
(N'Trương Mỹ Lan', '0905123456', 'lantruong.my@gmail.com', 50, 'VIP1', '2026-1-10'),
(N'Vũ Đức Đam', '0912345678', 'damvu.duc@gmail.com', 1100, 'VIP10', '2025-5-14'),
(N'Đỗ Thị Hồng', '0966889900', 'hongdo.thi88@gmail.com', 200, 'VIP1', '2025-11-11'),
(N'Ngô Bảo Châu', '0944556677', 'chau.ngo.math@gmail.com', 5600, 'VIP100', '2025-3-10'),
(N'Lâm Chấn Huy', '0932112233', 'huychanlam.singer@gmail.com', 950, 'VIP1', '2025-9-7'),
(N'Phạm Thu Hà', '0909998877', 'phamthuha.joy@gmail.com', 2100, 'VIP100', '2024-12-15'),
(N'Đinh Văn Hùng', '0912223344', 'hungdinh.sport@gmail.com', 1300, 'VIP10', '2025-5-17'),
(N'Nguyễn Thị Lan', '0988665544', 'lanthi.nguyen99@gmail.com', 500, 'VIP1', '2025-10-12'),
(N'Hoàng Quốc Việt', '0977112233', 'viethoang.tech@gmail.com', 80, 'VIP1', '2026-1-1'),
(N'Lê Bảo Ngọc', '0933557799', 'ngocle.bao@gmail.com', 530, 'VIP1', '2025-12-13')
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

INSERT INTO Staff (StaffName, Username, PasswordHash, StaffRole, Phonenumber, Email, ShiftID, BaseSalary, Birthday, StartDate, Gender)
VALUES
(N'Tài Khoản Quản Lý', 'admin', N'bf0dbd74174039131b667de9f31b5d8012baaf82011b934b2cc0e3bd53a02a1f', N'Admin', '0865320821', N'coffeeshop2g1g@gmail.com', NULL, NULL, '2000-1-1', '2000-1-1', N'Nam'),
(N'Nguyễn Chí Nguyên', N'ngnguyen', N'bf0dbd74174039131b667de9f31b5d8012baaf82011b934b2cc0e3bd53a02a1f', N'Admin', '0865320821', N'nathannguyen6002@gmail.com', NULL, NULL, '2006-3-10', '2024-9-5', N'Nam'),
(N'Nguyễn Ngọc Lan Anh', N'ngnlananh', N'bf0dbd74174039131b667de9f31b5d8012baaf82011b934b2cc0e3bd53a02a1f', N'Employee', '0988888888', N'ngnlananh@gmail.com', 2, 25000, '2006-11-11', '2025-3-2', N'Nữ'),
(N'Lê Thành Nghĩa', N'ltnghia', N'bf0dbd74174039131b667de9f31b5d8012baaf82011b934b2cc0e3bd53a02a1f', N'Employee', '0977778888', N'24521143@gm.uit.edu.vn', 1, 24000, '2006-11-15', '2025-10-1', N'Nam'),
(N'Tài khoản nhân viên', '1', N'bf0dbd74174039131b667de9f31b5d8012baaf82011b934b2cc0e3bd53a02a1f', N'Employee', '0865320821', N'nathannguyen6002@gmail.com', 1, 20000, '2000-1-1', '2000-1-1', N'Nam')
GO

INSERT INTO Discount (DiscountCode, DiscountName, DiscountType, DiscountValue, MinimumOrderValue, MaximumDiscountAmount, UsedCount, UseLimit) 
VALUES 
('VIP1', N'Giảm 3K cho VIP1', 1, 3000, 0, NULL, 11, 100),
('VIP10', N'Giảm 6K cho VIP10', 1, 6000, 0, NULL, 58, 69),
('VIP100', N'Giảm 10K cho VIP100', 1, 10000, 0, NULL, 70, 135),
('CF05', N'Giảm 5% cho hóa đơn từ 100K có mua cà phê', 0, 5, 100000, 10000, 36, 68)
GO

--INSERT INTO [Order] (TableID, CustomerID, StaffID, OrderDate, SubTotal, DiscountID, DiscountMoney, TotalAmount, PaymentMethod) 
--VALUES 
--(1, 1, 3, '2025-12-1 07:00:00', 54000, 3, 10000, 44000, N'Tiền mặt'), --1
--(NULL, 2, 3, '2025-12-1 13:00:00', 18000, 2, 6000, 12000, N'Chuyển khoản'), --2
--(3, 1, 3, '2025-12-1 16:00:00', 53000, 3, 10000, 43000, N'Chuyển khoản'), --3
--(NULL, NULL, 4, '2025-12-1 17:15:00', 56000, NULL, 0, 56000, N'Tiền mặt'), --4
--(NULL, 2, 4, '2025-12-1 20:00:00', 125000, 2, 6000, 119000, N'Chuyển khoản'), --5
--(10, NULL, 3, '2025-12-1 20:20:00', 55000, NULL, 0, 55000, N'Tiền mặt') --6
--GO

--INSERT INTO OrderDetail (OrderID, PriceID, Quantity, UnitPrice, TotalPrice, Note) 
--VALUES 
--(1, 1, 1, 19000, 19000, N'Ít sữa, nhiều cafe'),
--(1, 8, 1, 35000, 35000, N'70% đường, 30% đá'),
--(2, 12, 1, 18000, 18000, N'Đá để riêng'),
--(3, 13, 1, 23000, 23000, NULL),
--(3, 60, 1, 30000, 30000, NULL),
--(4, 40, 2, 28000, 56000, NULL),
--(5, 1, 2, 19000, 38000, N'1 ly không đường, 1 ly bình thường'),
--(5, 16, 1, 39000, 39000, N'Không lấy sữa đặc'),
--(5, 23, 1, 29000, 23000, N'Không cay'),
--(5, 5, 1, 25000, 25000, NULL),
--(6, 17, 1, 20000, 20000, N'Ít ngọt'),
--(6, 21, 1, 35000, 35000, N'Hâm nóng')
--GO

INSERT INTO Inventory (MaterialName, Quantity, Unit, Threshold, Note) 
VALUES 
(N'Hạt Cà phê Robusta', 15.5, N'Kg', 5.0, NULL),      
(N'Sữa đặc Ngôi Sao', 48, N'Lon', 10.0, NULL),        
(N'Sữa tươi không đường', 12, N'Hộp 1L', 6.0, N'Sắp hết hạn'),  
(N'Trà đen (Hồng trà)', 4.5, N'Kg', 2.0, NULL),
(N'Trà lài (Lục trà)', 3.0, N'Kg', 1.5, NULL),
(N'Bột Matcha', 0.4, N'Kg', 0.5, NULL),          
(N'Đường cát trắng', 20, N'Kg', 5.0, NULL),
(N'Syrup Đào', 5, N'Chai', 2.0, NULL),
(N'Syrup Vải', 1, N'Chai', 2.0, NULL),                
(N'Mứt Dâu', 3, N'Hũ', 1.0, NULL),
(N'Trân châu đen', 8, N'Kg', 3.0, N'Sắp hết hạn'),
(N'Thạch trái cây', 5, N'Hộp', 2.0, NULL),
(N'Cam vàng', 10, N'Kg', 3.0, NULL),
(N'Chanh tươi', 2.5, N'Kg', 1.0, NULL)
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
(1, 2, 2.5, NULL, DATEADD(day, -2, GETDATE()), 1),  -- Đã dùng 2.5kg Cà phê để bán
(2, 2, 2, NULL, DATEADD(day, -2, GETDATE()), 1),    -- Đã khui 2 lon sữa đặc
-- 3. Ghi nhận hàng hỏng hôm qua 
(3, 3, 1, NULL, DATEADD(day, -1, GETDATE()), 2),    -- Hủy 1 hộp sữa tươi do bị rách bao bì
(6, 3, 0.1, NULL, DATEADD(day, -1, GETDATE()), 2),  -- Hủy 0.1kg Matcha do ẩm mốc
-- 4. Nhập hàng bổ sung hôm nay
(6, 1, 0.5, 450000, GETDATE(), 1)                    -- Nhập thêm 0.5kg Matcha xịn
GO
----------------------------------------------------------------------------------------------------------------------------------------------------------------


-- KHAI BÁO CÁC BIẾN CẤU HÌNH
DECLARE @StartDate DATE = '2025-10-01'; -- Ngày bắt đầu
DECLARE @DaysRange INT = 120;            -- Chạy dữ liệu cho 100 ngày (3 tháng)

-- Cấu hình khoảng số lượng đơn mỗi ngày (Min - Max)
DECLARE @MinOrdersPerDay INT = 5;      -- Ít nhất 15 đơn/ngày
DECLARE @MaxOrdersPerDay INT = 35;      -- Nhiều nhất 35 đơn/ngày

-- Bảng tạm để lưu dữ liệu "nháp"
DECLARE @StagingOrders TABLE (
    RandomDate DATETIME,
    RandomStaffID INT,
    RandomCustomerID INT,
    RandomTableID INT,
    RandomPaymentMethod NVARCHAR(20)
);

DECLARE @MaxPriceID INT = (SELECT MAX(PriceID) FROM ItemPrice);
IF @MaxPriceID IS NULL SET @MaxPriceID = 50; -- Fallback nếu chưa có dữ liệu giá

-- =================================================================================
-- BƯỚC 1: SINH DỮ LIỆU (DUYỆT THEO TỪNG NGÀY)
-- =================================================================================
DECLARE @CurrentDate DATE = @StartDate;
DECLARE @EndDate DATE = DATEADD(day, @DaysRange, @StartDate);

DECLARE @OrdersToday INT; -- Biến lưu số lượng đơn của ngày đang xét
DECLARE @k INT;           -- Biến đếm vòng lặp con
DECLARE @R_Date DATETIME;
DECLARE @R_Staff INT;
DECLARE @R_Cust INT;
DECLARE @R_Table INT;
DECLARE @R_Pay NVARCHAR(20);
DECLARE @CustomerRate INT = 7;

-- VÒNG LẶP NGOÀI: Duyệt qua từng ngày
WHILE @CurrentDate <= @EndDate
BEGIN
    -- 1. Random số lượng đơn cho ngày hiện tại (@CurrentDate)
    -- Công thức: Random trong khoảng (Max - Min) + Min
    SET @OrdersToday = (ABS(CHECKSUM(NEWID()) % (@MaxOrdersPerDay - @MinOrdersPerDay + 1)) + @MinOrdersPerDay);

    -- [Tùy chọn] Logic cuối tuần: Nếu là Thứ 7 (7) hoặc Chủ Nhật (1) thì tăng 50% lượng đơn
    IF DATEPART(dw, @CurrentDate) IN (1, 7)
    BEGIN
        SET @OrdersToday = @OrdersToday * 1.5;
    END

    -- VÒNG LẶP TRONG: Tạo từng đơn hàng cho ngày hôm đó
    SET @k = 1;
    WHILE @k <= @OrdersToday
    BEGIN
        -- a. Random Giờ trong ngày (Từ 7h sáng đến 22h đêm = 15 tiếng mở cửa)
        SET @R_Date = DATEADD(hour, ABS(CHECKSUM(NEWID()) % 15) + 7, CAST(@CurrentDate AS DATETIME));
        SET @R_Date = DATEADD(minute, ABS(CHECKSUM(NEWID()) % 60), @R_Date);

        -- b. Random Nhân viên
        SET @R_Staff = CASE ABS(CHECKSUM(NEWID()) % 2) WHEN 0 THEN 3 ELSE 4 END;

        -- c. Random Khách hàng
        SET @R_Cust = CASE WHEN (ABS(CHECKSUM(NEWID()) % 10) < @CustomerRate) THEN NULL ELSE (ABS(CHECKSUM(NEWID()) % 20) + 1) END;

        -- d. Random Bàn
        SET @R_Table = CASE WHEN (ABS(CHECKSUM(NEWID()) % 10) < 3) THEN NULL ELSE (ABS(CHECKSUM(NEWID()) % 20) + 1) END;

        -- e. Random Phương thức thanh toán
        SET @R_Pay = CASE WHEN (ABS(CHECKSUM(NEWID()) % 2) = 0) THEN N'Tiền mặt' ELSE N'Chuyển khoản' END;

        INSERT INTO @StagingOrders (RandomDate, RandomStaffID, RandomCustomerID, RandomTableID, RandomPaymentMethod)
        VALUES (@R_Date, @R_Staff, @R_Cust, @R_Table, @R_Pay);

        SET @k = @k + 1;
    END

    -- Chuyển sang ngày tiếp theo
    SET @CurrentDate = DATEADD(day, 1, @CurrentDate);
END

-- =================================================================================
-- BƯỚC 2: CHÈN VÀO BẢNG CHÍNH (SẮP XẾP THEO THỜI GIAN)
-- (Phần này giữ nguyên logic cũ của bạn, chỉ thêm biến @RandPercent để random số lượng món chuẩn hơn)
-- =================================================================================
DECLARE OrderCursor CURSOR FOR 
SELECT RandomDate, RandomStaffID, RandomCustomerID, RandomTableID, RandomPaymentMethod 
FROM @StagingOrders ORDER BY RandomDate ASC; 

OPEN OrderCursor;
FETCH NEXT FROM OrderCursor INTO @R_Date, @R_Staff, @R_Cust, @R_Table, @R_Pay;

DECLARE @NewOrderID INT;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- Chèn Order
    INSERT INTO [Order] (TableID, CustomerID, StaffID, OrderDate, SubTotal, DiscountID, DiscountMoney, TotalAmount, PaymentMethod)
    VALUES (@R_Table, @R_Cust, @R_Staff, @R_Date, 0, NULL, 0, 0, @R_Pay);

    SET @NewOrderID = SCOPE_IDENTITY();

    -- Random số lượng món theo tỷ lệ phần trăm (Logic bạn yêu cầu ở câu trước)
    DECLARE @RandPercent INT = (ABS(CHECKSUM(NEWID()) % 100) + 1);
    DECLARE @NumItems INT = CASE 
        WHEN @RandPercent <= 30 THEN 1  -- 30% mua 1 món
        WHEN @RandPercent <= 55 THEN 2  -- 25% mua 2 món
        WHEN @RandPercent <= 75 THEN 3  -- 20% mua 3 món
        WHEN @RandPercent <= 90 THEN 4  -- 15% mua 4 món
        WHEN @RandPercent <= 97 THEN 5  -- 7%  mua 5 món
        ELSE 6                          -- 3%  mua 6 món
    END;

    DECLARE @j INT = 1;
    WHILE @j <= @NumItems
    BEGIN
        DECLARE @RandomPriceID INT = (ABS(CHECKSUM(NEWID()) % @MaxPriceID) + 1);
        
        -- Logic kiểm tra PriceID tồn tại (An toàn hơn)
        IF EXISTS (SELECT 1 FROM ItemPrice WHERE PriceID = @RandomPriceID)
        BEGIN
            DECLARE @RandomQty INT = CASE WHEN (ABS(CHECKSUM(NEWID()) % 20) < 19) THEN (ABS(CHECKSUM(NEWID()) % 2) + 1) ELSE 3 END;
            
            INSERT INTO OrderDetail (OrderID, PriceID, Quantity, UnitPrice, TotalPrice, Note)
            SELECT @NewOrderID, @RandomPriceID, @RandomQty, Price, Price * @RandomQty, NULL
            FROM ItemPrice WHERE PriceID = @RandomPriceID;
        END
        
        SET @j = @j + 1;
    END

    FETCH NEXT FROM OrderCursor INTO @R_Date, @R_Staff, @R_Cust, @R_Table, @R_Pay;
END

CLOSE OrderCursor;
DEALLOCATE OrderCursor;
GO

-- =================================================================================
-- BƯỚC 3: TÍNH TOÁN KHUYẾN MÃI (GIỮ NGUYÊN)
-- =================================================================================

-- A. Tính SubTotal
UPDATE O
SET O.SubTotal = (SELECT ISNULL(SUM(TotalPrice),0) FROM OrderDetail WHERE OrderID = O.OrderID)
FROM [Order] O
WHERE O.TotalAmount = 0; 

-- B. ÁP DỤNG MÃ VIP
UPDATE O
SET O.DiscountID = D.DiscountID
FROM [Order] O
JOIN Customer C ON O.CustomerID = C.CustomerID
JOIN Discount D ON C.Tier = D.DiscountCode
WHERE O.TotalAmount = 0;

-- C. ÁP DỤNG MÃ 'CF05'
DECLARE @CF05_ID INT = (SELECT DiscountID FROM Discount WHERE DiscountCode = 'CF05');
IF @CF05_ID IS NOT NULL
BEGIN
    UPDATE O
    SET O.DiscountID = @CF05_ID
    FROM [Order] O
    WHERE O.TotalAmount = 0 
      AND O.DiscountID IS NULL 
      AND O.SubTotal >= 100000 
      AND EXISTS (
          SELECT 1 
          FROM OrderDetail OD
          JOIN ItemPrice IP ON OD.PriceID = IP.PriceID
          JOIN Item I ON IP.ItemID = I.ItemID
          WHERE OD.OrderID = O.OrderID AND I.CategoryID = 1 
      );
END

-- D. Tính DiscountMoney
UPDATE O
SET O.DiscountMoney = CASE 
        WHEN D.DiscountType = 1 THEN D.DiscountValue
        WHEN D.DiscountType = 0 THEN 
            CASE 
                WHEN (O.SubTotal * D.DiscountValue / 100.0) > ISNULL(D.MaximumDiscountAmount, 999999999) 
                THEN D.MaximumDiscountAmount 
                ELSE (O.SubTotal * D.DiscountValue / 100.0) 
            END
        ELSE 0 
    END
FROM [Order] O
JOIN Discount D ON O.DiscountID = D.DiscountID
WHERE O.TotalAmount = 0;

-- E. Chốt TotalAmount
UPDATE [Order]
SET TotalAmount = SubTotal - ISNULL(DiscountMoney, 0)
WHERE TotalAmount = 0;

PRINT 'Done! Daily order volume randomized.';