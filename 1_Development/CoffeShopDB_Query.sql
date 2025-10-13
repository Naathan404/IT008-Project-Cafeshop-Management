CREATE DATABASE CoffeeShopDB;
GO

USE CoffeeShopDB;
GO

CREATE TABLE CafeTable
(
	TableID INT IDENTITY(1,1) PRIMARY KEY,
	TableName NVARCHAR(40) NOT NULL,
	TableStatus INT NOT NULL DEFAULT 0,		-- 0: trong, 1: dang phuc vu, 2: da dat truoc
	Note NVARCHAR(200) NULL,
);

CREATE TABLE Category
(
	CategoryID INT IDENTITY(1,1) PRIMARY KEY,
	CategoryName NVARCHAR(40) NOT NULL,
);


CREATE TABLE Item
(
	ItemID INT IDENTITY(1,1) PRIMARY KEY,
	ItemName NVARCHAR(50) NOT NULL,
	CategoryID INT NOT NULL,				-- FK -> Category(CategoryID)
	IsAvailable BIT NOT NULL,
	CONSTRAINT FK_Item_Category FOREIGN KEY (CategoryID) REFERENCES Category(CategoryID),
);

insert into Item
values
(N'Cà phê sữa đá', 1, 1),
(N'Đen đá không đường', 1, 1),
(N'Trà sữa truyền thống', 2, 1);

select * from item;
CREATE TABLE Size
(
	SizeID INT IDENTITY(1,1) PRIMARY KEY,
	SizeName NVARCHAR(5) NOT NULL,
);

CREATE TABLE ItemPrice (
    PriceID INT PRIMARY KEY IDENTITY(1,1),
    ItemID INT NOT NULL,
    SizeID INT NULL, -- Nếu là thức ăn thì NULL
    Price MONEY NOT NULL,
    CONSTRAINT FK_ItemPrice_Item FOREIGN KEY (ItemID) REFERENCES Item(ItemID),
    CONSTRAINT FK_ItemPrice_Size FOREIGN KEY (SizeID) REFERENCES Size(SizeID)
);


CREATE TABLE Customer
(
	CustomerID INT IDENTITY(1,1) PRIMARY KEY,
	CustomerName NVARCHAR(200) NOT NULL,
	PhoneNumber NVARCHAR(20) UNIQUE NULL,
	Email NVARCHAR(200) NULL,
	Point INT NOT NULL DEFAULT 0,
	Tier NVARCHAR(50) DEFAULT 'Silver'		-- Silver, Gold, Platinum, Diamond
);

CREATE TABLE [Order]
(
	OrderID INT IDENTITY(1,1) PRIMARY KEY,
	TableID INT NULL,		-- null la mua mang di
	CustomerID INT NULL,	-- null la khach vang lai
	OrderDate DATETIME NOT NULL DEFAULT GETDATE(),
	OrderStatus NVARCHAR(40) NOT NULL DEFAULT N'Chờ thanh toán',	-- Cho thanh toan, da thanh toan, da huy
	TotalAmount MONEY NOT NULL DEFAULT 0,
	PaymentMethod NVARCHAR(40) NOT NULL,							-- Tien mat, chuyen khoan
	CONSTRAINT FK_Order_Table FOREIGN KEY (TableID) REFERENCES CafeTable(TableID),
	CONSTRAINT FK_Order_Customer FOREIGN KEY (CustomerID) REFERENCES Customer(CustomerID),
);

CREATE TABLE OrderDetail (
    OrderDetailID INT IDENTITY(1,1) PRIMARY KEY,
    OrderID INT NOT NULL,
    PriceID INT NOT NULL,
    Quantity INT NOT NULL DEFAULT 1,
    TotalPrice MONEY NOT NULL,
    Note NVARCHAR(200) NULL,
    CONSTRAINT FK_OrderDetail_Order FOREIGN KEY (OrderID) REFERENCES [Order](OrderID),
	CONSTRAINT FK_OrderDetail_ItemPrice FOREIGN KEY (PriceID) REFERENCES ItemPrice(PriceID),
);

CREATE TABLE Shift (
    ShiftID INT PRIMARY KEY IDENTITY(1,1),
    ShiftName NVARCHAR(10) NOT NULL -- Sáng, Chiều, Tối
);


CREATE TABLE Staff
(
	StaffID INT IDENTITY(1,1) PRIMARY KEY,
	StaffName NVARCHAR(100) NOT NULL,
	Username NVARCHAR(100) UNIQUE NOT NULL,
	PasswordHash NVARCHAR(150) NOT NULL,
	StaffRole NVARCHAR(50) NOT NULL,			-- Admin, Employee
	ShiftID INT NULL,
	BaseSalary MONEY NULL, 
	CONSTRAINT FK_Staff_Shift FOREIGN KEY (ShiftID) REFERENCES Shift(ShiftID)
);

CREATE TABLE StaffAttendance
(
	AttendanceID INT IDENTITY(1,1) PRIMARY KEY,
	StaffID INT NOT NULL, 
	CheckIn DATETIME NOT NULL DEFAULT GETDATE(),
	CheckOut DATETIME NULL,
	CONSTRAINT FK_Attendance_Staff FOREIGN KEY(StaffID) REFERENCES Staff(StaffID),
);

CREATE TABLE Inventory
(
	MaterialID INT IDENTITY(1,1) PRIMARY KEY,
	MaterialName NVARCHAR(100),
	Quantity DECIMAL(18,2) NOT NULL DEFAULT 0,
	Unit NVARCHAR(50) NOT NULL,
	Threshold DECIMAL(18,2) NOT NULL,
);

CREATE TABLE ActionType (
    ActionTypeID INT PRIMARY KEY IDENTITY(1,1),
    ActionName NVARCHAR(20) NOT NULL -- Import, Export, Use
);

CREATE TABLE InventoryHistory (
    HistoryID INT IDENTITY(1,1) PRIMARY KEY,
    MaterialID INT NOT NULL,
    ActionTypeID INT NOT NULL,
    Quantity DECIMAL(18,2) NOT NULL,
    Date DATETIME NOT NULL DEFAULT GETDATE(),
    StaffID INT NOT NULL,
    CONSTRAINT FK_InvHistory_Inventoy FOREIGN KEY (MaterialID) REFERENCES Inventory(MaterialID),
    CONSTRAINT FK_InvHistory_Staff FOREIGN KEY (StaffID) REFERENCES Staff(StaffID),
	CONSTRAINT FK_InvHistory_Action FOREIGN KEY (ActionTypeID) REFERENCES ActionType(ActionTypeID)
);

ALTER TABLE Staff
ADD CONSTRAINT CK_Staff_Role CHECK (StaffRole IN ('Admin','Employee'));

ALTER TABLE STAFF
ADD PHONENUMBER VARCHAR(20);

ALTER TABLE STAFF
ADD EMAIL VARCHAR(100);

ALTER TABLE STAFF
DROP STAFFID;

insert into staff 
values
(N'Lê Thành Nghĩa', N'ltnghia', N'fa980dbf4533c98fa5ed792374bea691610dfaabc62558182f4cc814ef0d69db', N'Employee', 1, 20000, 0977778888, N'24521143@gm.uit.edu.vn'),
(N'Nguyễn Chí Nguyên', N'ngnguyen', N'38d180985d1b2e7a6014190e2cbd3c967408837188354ec93d27bfd86d09a017', N'Admin', NULL, NULL, 0865320821, N'nathannguyen6002@gmail.com')

insert into shift
values
(N'Sáng'),
(N'Chiều'),
(N'Tối')


select * from Staff;

delete from Staff;

-- Đoạn lệnh tạo bảng mới cho chức năng đổi mật khẩu
CREATE TABLE OTPRequest
(
RequestID int identity(1,1) primary key,
Email varchar(100) not null,
Code varchar(5) not null,
ExpireTime datetime2 not null
)

insert into staff 
values
(N'Nguyễn Ngọc Lan Anh', N'ngnlananh', N'bf0dbd74174039131b667de9f31b5d8012baaf82011b934b2cc0e3bd53a02a1f', N'Employee', 2, 20000, 0793124539, N'24520109@gm.uit.vn')
