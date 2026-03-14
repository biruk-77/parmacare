Namespace Models
    Public Class Role
        Public Property RoleID As Integer
        Public Property RoleName As String
        Public Property Description As String
    End Class

    Public Class User
        Public Property UserID As Integer
        Public Property Username As String
        Public Property PasswordHash As String
        Public Property Salt As String
        Public Property RoleID As Integer
        Public Property FullName As String
        Public Property IsActive As Boolean
        Public Property CreatedAt As DateTime
        Public Overridable Property Role As Role
    End Class

    Public Class Category
        Public Property CategoryID As Integer
        Public Property CategoryName As String
        Public Property Description As String
    End Class

    Public Class Supplier
        Public Property SupplierID As Integer
        Public Property CompanyName As String
        Public Property TIN_Number As String
        Public Property ContactPhone As String
        Public Property Email As String
        Public Property Address As String
        Public Property CreatedAt As DateTime
    End Class

    Public Class Product
        Public Property ProductID As Integer
        Public Property Name As String
        Public Property GenericName As String
        Public Property Manufacturer As String
        Public Property CategoryID As Integer
        Public Property ReorderPoint As Integer
        Public Property IsActive As Boolean
        Public Property CreatedAt As DateTime
        Public Overridable Property Category As Category
        Public Overridable Property Batches As ICollection(Of Batch) = New HashSet(Of Batch)
    End Class

    Public Class Batch
        Public Property BatchID As Integer
        Public Property ProductID As Integer
        Public Property BatchNumber As String
        Public Property ExpiryDate As DateTime
        Public Property CostPrice As Decimal
        Public Property SellingPrice As Decimal
        Public Property CurrentQuantity As Integer
        Public Property ReceivedDate As DateTime
        Public Overridable Property Product As Product
    End Class

    Public Class Purchase
        Public Property PurchaseID As Integer
        Public Property SupplierID As Integer
        Public Property UserID As Integer
        Public Property PurchaseDate As DateTime
        Public Property TotalAmount As Decimal
        Public Property ReferenceInvoice As String
        Public Overridable Property Supplier As Supplier
        Public Overridable Property User As User
        Public Overridable Property Details As ICollection(Of PurchaseDetail) = New HashSet(Of PurchaseDetail)
    End Class

    Public Class PurchaseDetail
        Public Property PurchaseDetailID As Integer
        Public Property PurchaseID As Integer
        Public Property BatchID As Integer
        Public Property Quantity As Integer
        Public Property UnitCost As Decimal
        Public Property SubTotal As Decimal ' Computed in DB
        Public Overridable Property Purchase As Purchase
        Public Overridable Property Batch As Batch
    End Class

    Public Class Sale
        Public Property SaleID As Integer
        Public Property UserID As Integer
        Public Property SaleDate As DateTime
        Public Property TotalAmount As Decimal
        Public Property PaymentMethod As String
        Public Overridable Property User As User
        Public Overridable Property Details As ICollection(Of SaleDetail) = New HashSet(Of SaleDetail)
    End Class

    Public Class SaleDetail
        Public Property SaleDetailID As Integer
        Public Property SaleID As Integer
        Public Property BatchID As Integer
        Public Property Quantity As Integer
        Public Property UnitPrice As Decimal
        Public Property SubTotal As Decimal ' Computed in DB
        Public Overridable Property Sale As Sale
        Public Overridable Property Batch As Batch
    End Class

    Public Class Alert
        Public Property AlertID As Integer
        Public Property AlertType As String ' LowStock, ExpiryWarning, Expired
        Public Property ProductID As Integer?
        Public Property BatchID As Integer?
        Public Property Message As String
        Public Property IsResolved As Boolean
        Public Property CreatedAt As DateTime
    End Class

    Public Class SystemSetting
        Public Property SettingKey As String
        Public Property SettingValue As String
        Public Property Description As String
    End Class
End Namespace
