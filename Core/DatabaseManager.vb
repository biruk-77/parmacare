Imports Microsoft.Data.SqlClient
Imports WinFormsApp1.Models

Namespace Core
    Public Class DatabaseManager
        Private Shared _connectionString As String = "Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=CPSMS;Integrated Security=True;Encrypt=False;Connect Timeout=30"

        Public Shared Function GetConnection() As SqlConnection
            Return New SqlConnection(_connectionString)
        End Function

        Public Shared Function ExecuteScalar(sql As String) As Object
            Using conn = GetConnection()
                conn.Open()
                Dim cmd As New SqlCommand(sql, conn)
                Return cmd.ExecuteScalar()
            End Using
        End Function

        Public Shared Sub InitializeDatabase()
            Dim masterConnString = "Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Encrypt=False;Connect Timeout=30"
            
            Try
                Using conn As New SqlConnection(masterConnString)
                    conn.Open()
                    Dim checkDb = "IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'CPSMS') CREATE DATABASE CPSMS"
                    Using cmd As New SqlCommand(checkDb, conn)
                        cmd.ExecuteNonQuery()
                    End Using
                End Using

                Using conn = GetConnection()
                    conn.Open()
                    
                    ' 1. Check if tables exist (using Roles as anchor)
                    Dim checkTable = "IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Roles') SELECT 0 ELSE SELECT 1"
                    Dim exists = CInt(ExecuteScalar(checkTable))
                    
                    If exists = 0 Then
                        Dim schemaPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Schema.sql")
                        Dim script As String = ""

                        If System.IO.File.Exists(schemaPath) Then
                            script = System.IO.File.ReadAllText(schemaPath)
                        Else
                            ' Hardcoded Schema Fallback for Portable Builds
                            script = "CREATE TABLE Roles (RoleID INT IDENTITY(1,1) PRIMARY KEY, RoleName NVARCHAR(50) NOT NULL UNIQUE, Description NVARCHAR(255));" & vbCrLf &
                                     "CREATE TABLE Users (UserID INT IDENTITY(1,1) PRIMARY KEY, Username NVARCHAR(50) NOT NULL UNIQUE, PasswordHash NVARCHAR(MAX) NOT NULL, Salt NVARCHAR(MAX) NOT NULL, RoleID INT NOT NULL, FullName NVARCHAR(100) NOT NULL, IsActive BIT DEFAULT 1, CreatedAt DATETIME DEFAULT GETDATE(), FOREIGN KEY (RoleID) REFERENCES Roles(RoleID));" & vbCrLf &
                                     "CREATE TABLE Categories (CategoryID INT IDENTITY(1,1) PRIMARY KEY, CategoryName NVARCHAR(100) NOT NULL UNIQUE, Description NVARCHAR(255));" & vbCrLf &
                                     "CREATE TABLE Suppliers (SupplierID INT IDENTITY(1,1) PRIMARY KEY, CompanyName NVARCHAR(150) NOT NULL, TIN_Number NVARCHAR(50) NOT NULL UNIQUE, ContactPhone NVARCHAR(20), Email NVARCHAR(100), Address NVARCHAR(255), CreatedAt DATETIME DEFAULT GETDATE());" & vbCrLf &
                                     "CREATE TABLE Products (ProductID INT IDENTITY(1,1) PRIMARY KEY, Name NVARCHAR(150) NOT NULL, GenericName NVARCHAR(150), Manufacturer NVARCHAR(100), CategoryID INT NOT NULL, ReorderPoint INT DEFAULT 10, IsActive BIT DEFAULT 1, CreatedAt DATETIME DEFAULT GETDATE(), FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID));" & vbCrLf &
                                     "CREATE TABLE Batches (BatchID INT IDENTITY(1,1) PRIMARY KEY, ProductID INT NOT NULL, BatchNumber NVARCHAR(50) NOT NULL, ExpiryDate DATE NOT NULL, CostPrice DECIMAL(18,2) NOT NULL, SellingPrice DECIMAL(18,2) NOT NULL, CurrentQuantity INT NOT NULL DEFAULT 0, ReceivedDate DATETIME DEFAULT GETDATE(), CONSTRAINT CHK_PositiveStock CHECK (CurrentQuantity >= 0), FOREIGN KEY (ProductID) REFERENCES Products(ProductID), CONSTRAINT UQ_Product_Batch UNIQUE (ProductID, BatchNumber));" & vbCrLf &
                                     "CREATE TABLE Purchases (PurchaseID INT IDENTITY(1,1) PRIMARY KEY, SupplierID INT NOT NULL, UserID INT NOT NULL, PurchaseDate DATETIME DEFAULT GETDATE(), TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0, ReferenceInvoice NVARCHAR(100), FOREIGN KEY (SupplierID) REFERENCES Suppliers(SupplierID), FOREIGN KEY (UserID) REFERENCES Users(UserID));" & vbCrLf &
                                     "CREATE TABLE PurchaseDetails (PurchaseDetailID INT IDENTITY(1,1) PRIMARY KEY, PurchaseID INT NOT NULL, BatchID INT NOT NULL, Quantity INT NOT NULL CHECK (Quantity > 0), UnitCost DECIMAL(18,2) NOT NULL, SubTotal AS (Quantity * UnitCost) PERSISTED, FOREIGN KEY (PurchaseID) REFERENCES Purchases(PurchaseID), FOREIGN KEY (BatchID) REFERENCES Batches(BatchID));" & vbCrLf &
                                     "CREATE TABLE Sales (SaleID INT IDENTITY(1,1) PRIMARY KEY, UserID INT NOT NULL, SaleDate DATETIME DEFAULT GETDATE(), TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0, PaymentMethod NVARCHAR(50) DEFAULT 'Cash', FOREIGN KEY (UserID) REFERENCES Users(UserID));" & vbCrLf &
                                     "CREATE TABLE SaleDetails (SaleDetailID INT IDENTITY(1,1) PRIMARY KEY, SaleID INT NOT NULL, BatchID INT NOT NULL, Quantity INT NOT NULL CHECK (Quantity > 0), UnitPrice DECIMAL(18,2) NOT NULL, SubTotal AS (Quantity * UnitPrice) PERSISTED, FOREIGN KEY (SaleID) REFERENCES Sales(SaleID), FOREIGN KEY (BatchID) REFERENCES Batches(BatchID));" & vbCrLf &
                                     "CREATE TABLE Alerts (AlertID INT IDENTITY(1,1) PRIMARY KEY, AlertType NVARCHAR(50) CHECK (AlertType IN ('LowStock', 'ExpiryWarning', 'Expired')), ProductID INT NULL, BatchID INT NULL, Message NVARCHAR(255) NOT NULL, IsResolved BIT DEFAULT 0, CreatedAt DATETIME DEFAULT GETDATE(), FOREIGN KEY (ProductID) REFERENCES Products(ProductID), FOREIGN KEY (BatchID) REFERENCES Batches(BatchID));" & vbCrLf &
                                     "CREATE TABLE SystemSettings (SettingKey NVARCHAR(50) PRIMARY KEY, SettingValue NVARCHAR(255) NOT NULL, Description NVARCHAR(255));"
                        End If

                        For Each batch In script.Split({"GO", ";"}, StringSplitOptions.RemoveEmptyEntries)
                            If Not String.IsNullOrWhiteSpace(batch) Then
                                Using cmd As New SqlCommand(batch.Trim(), conn)
                                    cmd.ExecuteNonQuery()
                                End Using
                            End If
                        Next

                        ' 2. Seed Default Roles
                        Dim seedRoles = "INSERT INTO Roles (RoleName, Description) VALUES ('Admin', 'Full System Access'), ('Pharmacist', 'Pharmacy Management'), ('Clerk', 'Inventory & Sales')"
                        Using cmd As New SqlCommand(seedRoles, conn)
                            cmd.ExecuteNonQuery()
                        End Using

                        ' 3. Seed Default System Settings
                        Dim seedSettings = "INSERT INTO SystemSettings (SettingKey, SettingValue, Description) VALUES ('ExpiryWarningDays', '90', 'Days before warning'), ('Currency', 'ETB', 'Default Currency')"
                        Using cmd As New SqlCommand(seedSettings, conn)
                            cmd.ExecuteNonQuery()
                        End Using
                    End If

                    ' 4. Ensure Admin User exists
                    Dim checkAdmin = "SELECT COUNT(*) FROM Users WHERE Username = 'admin'"
                    If CInt(ExecuteScalar(checkAdmin)) = 0 Then
                        Dim roleId = CInt(ExecuteScalar("SELECT RoleID FROM Roles WHERE RoleName = 'Admin'"))
                        Dim salt = SecurityManager.GenerateSalt()
                        Dim hash = SecurityManager.HashPassword("1234", salt)
                        Dim sql = "INSERT INTO Users (Username, PasswordHash, Salt, RoleID, FullName) VALUES (@u, @h, @s, @r, @f)"
                        Using cmd As New SqlCommand(sql, conn)
                            cmd.Parameters.AddWithValue("@u", "admin")
                            cmd.Parameters.AddWithValue("@h", hash)
                            cmd.Parameters.AddWithValue("@s", salt)
                            cmd.Parameters.AddWithValue("@r", roleId)
                            cmd.Parameters.AddWithValue("@f", "System Administrator")
                            cmd.ExecuteNonQuery()
                        End Using
                    End If
                End Using
            Catch ex As Exception
                Console.WriteLine("DB Init Error: " & ex.Message)
            End Try
        End Sub
    End Class
End Namespace
