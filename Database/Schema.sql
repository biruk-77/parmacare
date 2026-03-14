
-- 1. ROLE & USER MANAGEMENT
CREATE TABLE Roles (
    RoleID INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(255)
);
GO

CREATE TABLE Users (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    Salt NVARCHAR(MAX) NOT NULL, -- Added for PBKDF2
    RoleID INT NOT NULL,
    FullName NVARCHAR(100) NOT NULL,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (RoleID) REFERENCES Roles(RoleID)
);
GO

-- 2. INVENTORY & BATCH MANAGEMENT
CREATE TABLE Categories (
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(255)
);
GO

CREATE TABLE Suppliers (
    SupplierID INT IDENTITY(1,1) PRIMARY KEY,
    CompanyName NVARCHAR(150) NOT NULL,
    TIN_Number NVARCHAR(50) NOT NULL UNIQUE,
    ContactPhone NVARCHAR(20),
    Email NVARCHAR(100),
    Address NVARCHAR(255),
    CreatedAt DATETIME DEFAULT GETDATE()
);
GO

CREATE TABLE Products (
    ProductID INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(150) NOT NULL,
    GenericName NVARCHAR(150),
    Manufacturer NVARCHAR(100),
    CategoryID INT NOT NULL,
    ReorderPoint INT DEFAULT 10,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID)
);
GO

CREATE TABLE Batches (
    BatchID INT IDENTITY(1,1) PRIMARY KEY,
    ProductID INT NOT NULL,
    BatchNumber NVARCHAR(50) NOT NULL,
    ExpiryDate DATE NOT NULL,
    CostPrice DECIMAL(18,2) NOT NULL,
    SellingPrice DECIMAL(18,2) NOT NULL,
    CurrentQuantity INT NOT NULL DEFAULT 0,
    ReceivedDate DATETIME DEFAULT GETDATE(),
    CONSTRAINT CHK_PositiveStock CHECK (CurrentQuantity >= 0),
    FOREIGN KEY (ProductID) REFERENCES Products(ProductID),
    CONSTRAINT UQ_Product_Batch UNIQUE (ProductID, BatchNumber)
);
GO

-- 3. TRANSACTIONS (Purchases & Sales)
CREATE TABLE Purchases (
    PurchaseID INT IDENTITY(1,1) PRIMARY KEY,
    SupplierID INT NOT NULL,
    UserID INT NOT NULL,
    PurchaseDate DATETIME DEFAULT GETDATE(),
    TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    ReferenceInvoice NVARCHAR(100),
    FOREIGN KEY (SupplierID) REFERENCES Suppliers(SupplierID),
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);
GO

CREATE TABLE PurchaseDetails (
    PurchaseDetailID INT IDENTITY(1,1) PRIMARY KEY,
    PurchaseID INT NOT NULL,
    BatchID INT NOT NULL,
    Quantity INT NOT NULL CHECK (Quantity > 0),
    UnitCost DECIMAL(18,2) NOT NULL,
    SubTotal AS (Quantity * UnitCost) PERSISTED,
    FOREIGN KEY (PurchaseID) REFERENCES Purchases(PurchaseID),
    FOREIGN KEY (BatchID) REFERENCES Batches(BatchID)
);
GO

CREATE TABLE Sales (
    SaleID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL,
    SaleDate DATETIME DEFAULT GETDATE(),
    TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    PaymentMethod NVARCHAR(50) DEFAULT 'Cash',
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);
GO

CREATE TABLE SaleDetails (
    SaleDetailID INT IDENTITY(1,1) PRIMARY KEY,
    SaleID INT NOT NULL,
    BatchID INT NOT NULL,
    Quantity INT NOT NULL CHECK (Quantity > 0),
    UnitPrice DECIMAL(18,2) NOT NULL,
    SubTotal AS (Quantity * UnitPrice) PERSISTED,
    FOREIGN KEY (SaleID) REFERENCES Sales(SaleID),
    FOREIGN KEY (BatchID) REFERENCES Batches(BatchID)
);
GO

-- 4. ALERTS & NOTIFICATIONS
CREATE TABLE Alerts (
    AlertID INT IDENTITY(1,1) PRIMARY KEY,
    AlertType NVARCHAR(50) CHECK (AlertType IN ('LowStock', 'ExpiryWarning', 'Expired')),
    ProductID INT NULL,
    BatchID INT NULL,
    Message NVARCHAR(255) NOT NULL,
    IsResolved BIT DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (ProductID) REFERENCES Products(ProductID),
    FOREIGN KEY (BatchID) REFERENCES Batches(BatchID)
);
GO

-- 5. SYSTEM SETTINGS
CREATE TABLE SystemSettings (
    SettingKey NVARCHAR(50) PRIMARY KEY,
    SettingValue NVARCHAR(255) NOT NULL,
    Description NVARCHAR(255)
);
GO
