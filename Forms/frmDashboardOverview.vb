Imports WinFormsApp1.Core
Imports WinFormsApp1.Models
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports System.Linq

Public Class frmDashboardOverview
    Inherits Form

    Private WithEvents animTimer As New Timer() With {.Interval = 16}
    Private _cards As New List(Of StatCard)
    Private _alerts As New List(Of AlertInfo)

    Public Sub New()
        Me.SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.UserPaint Or ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw, True)
        Me.DoubleBuffered = True
        
        InitializeUI()
        LoadStats()
        LoadAlerts()
    End Sub

    Private Sub InitializeUI()
        Me.FormBorderStyle = FormBorderStyle.None
        Me.TopLevel = False
        Me.Dock = DockStyle.Fill
        Me.BackColor = AppManager.CurrentTheme.Background

        _cards.Add(New StatCard("Total Users", "0", ChrW(&HE716), 0))
        _cards.Add(New StatCard("Low Stock", "0", ChrW(&HE81A), 1))
        _cards.Add(New StatCard("Today Sales", "$0", ChrW(&HE8A1), 2))
        _cards.Add(New StatCard("Expiring Soon", "0", ChrW(&HE783), 3))
        
        animTimer.Start()
    End Sub

    Private Sub LoadStats()
        Try
            Using db As New PharmacyContext()
                _cards(0).Value = db.Users.Count().ToString()
                _cards(1).Value = db.Batches.Count(Function(b) b.CurrentQuantity < 10).ToString() & " Items"
                
                Dim todaySales = db.Sales.Where(Function(s) s.SaleDate.Date = DateTime.Now.Date).Sum(Function(s) CType(s.TotalAmount, Decimal?))
                _cards(2).Value = If(todaySales, 0D).ToString("C0")

                Dim expiringCount = db.Batches.Count(Function(b) b.ExpiryDate <= DateTime.Now.AddDays(90) AndAlso b.CurrentQuantity > 0)
                _cards(3).Value = expiringCount.ToString() & " Items"
            End Using
        Catch
        End Try
        Me.Invalidate()
    End Sub

    Private Sub LoadAlerts()
        _alerts.Clear()
        Try
            Using db As New PharmacyContext()
                ' Low stock alerts
                Dim lowStock = (From p In db.Products
                                Group Join b In db.Batches On p.ProductID Equals b.ProductID Into bGroup = Group
                                Let totalQty = bGroup.Sum(Function(x) CType(x.CurrentQuantity, Integer?))
                                Where (If(totalQty, 0)) <= p.ReorderPoint
                                Select New With {.Name = p.Name, .Qty = If(totalQty, 0), .Threshold = p.ReorderPoint}).Take(5).ToList()
                For Each item In lowStock
                    _alerts.Add(New AlertInfo(ChrW(&HE7BA), $"{item.Name}: {item.Qty} left (threshold: {item.Threshold})", "LowStock"))
                Next

                ' Expiry alerts
                Dim expiring = (From b In db.Batches
                                Join p In db.Products On b.ProductID Equals p.ProductID
                                Where b.ExpiryDate <= DateTime.Now.AddDays(90) AndAlso b.CurrentQuantity > 0
                                Order By b.ExpiryDate
                                Select New With {.Name = p.Name, .Batch = b.BatchNumber, .Days = (b.ExpiryDate - DateTime.Now).Days}).Take(5).ToList()
                For Each item In expiring
                    Dim statusStr = If(item.Days < 0, "EXPIRED", $"{item.Days} days left")
                    _alerts.Add(New AlertInfo(ChrW(&HE783), $"{item.Name} (Batch {item.Batch}): {statusStr}", If(item.Days < 0, "Expired", "ExpiryWarning")))
                Next
            End Using
        Catch
        End Try
        Me.Invalidate()
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Dim g = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias
        g.TextRenderingHint = Drawing.Text.TextRenderingHint.ClearTypeGridFit
        
        Dim theme = AppManager.CurrentTheme
        Dim titleStr = AppManager.GetText("DashOverview")
        Dim subStr = AppManager.GetText("SystemReady")

        Using tFont As New Font("Segoe UI", 24, FontStyle.Bold)
            g.DrawString(titleStr, tFont, New SolidBrush(theme.PrimaryText), 30, 25)
        End Using
        Using sFont As New Font("Segoe UI", 11)
            g.DrawString(subStr, sFont, New SolidBrush(theme.SecondaryText), 35, 65)
        End Using

        ' Draw Stat Cards
        For Each card In _cards
            card.Draw(g, theme)
        Next

        ' Draw Alerts Section
        Dim alertY = 300
        Using hFont As New Font("Segoe UI Semibold", 14)
            g.DrawString(ChrW(&HEA8F) & "  Active Alerts", hFont, New SolidBrush(theme.Accent), 30, alertY)
        End Using
        alertY += 40

        If _alerts.Count = 0 Then
            Using nFont As New Font("Segoe UI", 11)
                g.DrawString("No active alerts. All systems operational! " & ChrW(&H2714), nFont, New SolidBrush(theme.SecondaryText), 40, alertY)
            End Using
        Else
            For Each alert In _alerts
                Dim alertColor = If(alert.AlertType = "Expired", Color.FromArgb(220, 50, 50), If(alert.AlertType = "LowStock", Color.FromArgb(230, 160, 0), theme.SecondaryText))

                ' Alert row background
                Dim rowRect As New Rectangle(30, alertY, Me.Width - 80, 35)
                Using path = GetRoundedRect(rowRect, 6)
                    g.FillPath(New SolidBrush(Color.FromArgb(15, alertColor)), path)
                End Using

                Using iFont As New Font("Segoe MDL2 Assets", 10)
                    g.DrawString(alert.Icon, iFont, New SolidBrush(alertColor), 40, alertY + 8)
                End Using
                Using tFont As New Font("Segoe UI", 10)
                    g.DrawString(alert.Message, tFont, New SolidBrush(theme.PrimaryText), 65, alertY + 8)
                End Using

                alertY += 42
                If alertY > Me.Height - 30 Then Exit For
            Next
        End If
    End Sub

    Private Function GetRoundedRect(rect As Rectangle, radius As Integer) As GraphicsPath
        Dim path As New GraphicsPath()
        Dim d = radius * 2
        path.AddArc(rect.X, rect.Y, d, d, 180, 90)
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90)
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90)
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90)
        path.CloseFigure()
        Return path
    End Function

    Private Sub animTimer_Tick(sender As Object, e As EventArgs) Handles animTimer.Tick
        Dim needsRedraw = False
        For Each card In _cards
            card.Update()
            If card.IsAnimating Then needsRedraw = True
        Next
        If needsRedraw Then Me.Invalidate()
    End Sub

    Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
        MyBase.OnMouseMove(e)
        Dim pt = e.Location
        Dim needsRedraw = False
        For Each card In _cards
            Dim wasHovered = card.IsHovered
            card.IsHovered = card.Bounds.Contains(pt)
            If wasHovered <> card.IsHovered Then needsRedraw = True
        Next
        If needsRedraw Then Me.Invalidate()
    End Sub

    Private Class StatCard
        Public Property Title As String
        Public Property Value As String
        Public Property IconChar As String
        Public Property Bounds As Rectangle
        Public Property IsHovered As Boolean = False
        Private _hoverAnim As Single = 0
        Private _index As Integer

        Public ReadOnly Property IsAnimating As Boolean
            Get
                Return (IsHovered And _hoverAnim < 1.0F) OrElse (Not IsHovered And _hoverAnim > 0.0F)
            End Get
        End Property

        Public Sub New(t As String, v As String, iStr As String, idx As Integer)
            Title = t
            Value = v
            IconChar = iStr
            _index = idx
            Bounds = New Rectangle(30 + (idx * 225), 110, 210, 140)
        End Sub

        Public Sub Update()
            If IsHovered Then
                _hoverAnim = Math.Min(1.0F, _hoverAnim + 0.1F)
            Else
                _hoverAnim = Math.Max(0.0F, _hoverAnim - 0.1F)
            End If
        End Sub

        Public Sub Draw(g As Graphics, theme As AppManager.Theme)
            Dim yOffset = CInt(Math.Round(-5 * _hoverAnim))
            Dim rect = New Rectangle(Bounds.X, Bounds.Y + yOffset, Bounds.Width, Bounds.Height)
            
            Dim path As New GraphicsPath()
            Dim d = 16
            path.AddArc(rect.X, rect.Y, d, d, 180, 90)
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90)
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90)
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90)
            path.CloseFigure()

            g.FillPath(New SolidBrush(theme.Surface), path)
            Dim bColor = Color.FromArgb(Math.Min(255, 100 + CInt(100 * _hoverAnim)), theme.Accent)
            g.DrawPath(New Pen(bColor, 1.5F), path)

            Using iFont As New Font("Segoe MDL2 Assets", 20)
                g.DrawString(IconChar, iFont, New SolidBrush(theme.Accent), rect.X + 18, rect.Y + 18)
            End Using

            Using tFont As New Font("Segoe UI Semibold", 10)
                g.DrawString(Title, tFont, New SolidBrush(theme.SecondaryText), rect.X + 18, rect.Y + 60)
            End Using

            Using vFont As New Font("Segoe UI", 22, FontStyle.Bold)
                g.DrawString(Value, vFont, New SolidBrush(theme.PrimaryText), rect.X + 18, rect.Y + 82)
            End Using
        End Sub
    End Class

    Private Class AlertInfo
        Public Property Icon As String
        Public Property Message As String
        Public Property AlertType As String

        Public Sub New(icon As String, msg As String, aType As String)
            Me.Icon = icon
            Me.Message = msg
            Me.AlertType = aType
        End Sub
    End Class
End Class
