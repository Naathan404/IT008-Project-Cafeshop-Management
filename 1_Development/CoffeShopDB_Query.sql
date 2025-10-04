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