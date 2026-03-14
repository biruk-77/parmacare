Imports Microsoft.EntityFrameworkCore
Imports WinFormsApp1.Models

Public Class PharmacyContext
    Inherits DbContext

    Public Property Roles As DbSet(Of Role)
    Public Property Users As DbSet(Of User)
    Public Property Categories As DbSet(Of Category)
    Public Property Suppliers As DbSet(Of Supplier)
    Public Property Products As DbSet(Of Product)
    Public Property Batches As DbSet(Of Batch)
    Public Property Purchases As DbSet(Of Purchase)
    Public Property PurchaseDetails As DbSet(Of PurchaseDetail)
    Public Property Sales As DbSet(Of Sale)
    Public Property SaleDetails As DbSet(Of SaleDetail)
    Public Property Alerts As DbSet(Of Alert)
    Public Property SystemSettings As DbSet(Of SystemSetting)

    Protected Overrides Sub OnConfiguring(optionsBuilder As DbContextOptionsBuilder)
        optionsBuilder.UseSqlServer("Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=CPSMS;Integrated Security=True;Encrypt=False;Connect Timeout=30", 
            Sub(sqlOptions)
                sqlOptions.EnableRetryOnFailure(maxRetryCount:=5, maxRetryDelay:=TimeSpan.FromSeconds(30), errorNumbersToAdd:=Nothing)
            End Sub)
    End Sub

    Protected Overrides Sub OnModelCreating(modelBuilder As ModelBuilder)
        modelBuilder.Entity(Of SystemSetting)().HasKey(Function(s) s.SettingKey)
        
        ' Configure SubTotal as persisted computed columns
        modelBuilder.Entity(Of PurchaseDetail)().Property(Function(p) p.SubTotal).HasComputedColumnSql("(Quantity * UnitCost)", True)
        modelBuilder.Entity(Of SaleDetail)().Property(Function(s) s.SubTotal).HasComputedColumnSql("(Quantity * UnitPrice)", True)

        ' Tell EF Core that these DateTime columns have DB defaults (GETDATE())
        ' so it won't send DateTime.MinValue (0001-01-01) which SQL Server rejects
        modelBuilder.Entity(Of Product)().Property(Function(p) p.CreatedAt).HasDefaultValueSql("GETDATE()")
        modelBuilder.Entity(Of Batch)().Property(Function(b) b.ReceivedDate).HasDefaultValueSql("GETDATE()")
        modelBuilder.Entity(Of User)().Property(Function(u) u.CreatedAt).HasDefaultValueSql("GETDATE()")
        modelBuilder.Entity(Of Supplier)().Property(Function(s) s.CreatedAt).HasDefaultValueSql("GETDATE()")
        modelBuilder.Entity(Of Purchase)().Property(Function(p) p.PurchaseDate).HasDefaultValueSql("GETDATE()")
        modelBuilder.Entity(Of Sale)().Property(Function(s) s.SaleDate).HasDefaultValueSql("GETDATE()")
        modelBuilder.Entity(Of Alert)().Property(Function(a) a.CreatedAt).HasDefaultValueSql("GETDATE()")
    End Sub
End Class
