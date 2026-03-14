Imports WinFormsApp1.Models
Imports WinFormsApp1.Core
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports System.Linq

Public Class frmReports
    Inherits Form

    Private cbReportType As ComboBox
    Private dtpFrom As DateTimePicker
    Private dtpTo As DateTimePicker
    Private WithEvents btnGenerate As CyberButton
    Private dgvReport As DataGridView
    Private lblTitle As Label
    Private lblSummary As Label

    Public Sub New()
        Me.SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.UserPaint Or ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw, True)
        Me.DoubleBuffered = True
        InitializeUI()
    End Sub

    Private Sub InitializeUI()
        Me.FormBorderStyle = FormBorderStyle.None
        Me.TopLevel = False
        Me.Dock = DockStyle.Fill
        Me.BackColor = AppManager.CurrentTheme.Background

        Dim theme = AppManager.CurrentTheme

        lblTitle = New Label() With {
            .Text = AppManager.GetText("MenuREPORTS"),
            .Font = New Font("Segoe UI", 18, FontStyle.Bold),
            .ForeColor = theme.PrimaryText,
            .Location = New Point(30, 20),
            .AutoSize = True
        }

        Dim lblType As New Label() With {.Text = "Report Type:", .Location = New Point(30, 70), .AutoSize = True, .Font = New Font("Segoe UI", 9), .ForeColor = theme.SecondaryText}
        cbReportType = New ComboBox() With {
            .Location = New Point(30, 90),
            .Width = 220,
            .DropDownStyle = ComboBoxStyle.DropDownList,
            .Font = New Font("Segoe UI", 10)
        }
        cbReportType.Items.AddRange({"Sales Summary", "Purchase Summary", "Expiring Items", "Low Stock Items", "Inventory Valuation"})
        cbReportType.SelectedIndex = 0

        Dim lblFrom As New Label() With {.Text = "From:", .Location = New Point(270, 70), .AutoSize = True, .Font = New Font("Segoe UI", 9), .ForeColor = theme.SecondaryText}
        dtpFrom = New DateTimePicker() With {
            .Location = New Point(270, 90),
            .Width = 150,
            .Format = DateTimePickerFormat.Short,
            .Value = DateTime.Now.AddMonths(-1),
            .Font = New Font("Segoe UI", 10)
        }

        Dim lblTo As New Label() With {.Text = "To:", .Location = New Point(440, 70), .AutoSize = True, .Font = New Font("Segoe UI", 9), .ForeColor = theme.SecondaryText}
        dtpTo = New DateTimePicker() With {
            .Location = New Point(440, 90),
            .Width = 150,
            .Format = DateTimePickerFormat.Short,
            .Font = New Font("Segoe UI", 10)
        }

        btnGenerate = New CyberButton() With {
            .Text = ChrW(&HE9D2) & " Generate",
            .Location = New Point(610, 85),
            .Size = New Size(150, 38),
            .BackColor = theme.Accent,
            .Font = New Font("Segoe UI Semibold", 10)
        }

        lblSummary = New Label() With {
            .Text = "",
            .Font = New Font("Segoe UI Semibold", 11),
            .ForeColor = theme.Accent,
            .Location = New Point(30, 135),
            .AutoSize = True
        }

        dgvReport = New DataGridView() With {
            .Location = New Point(30, 165),
            .Size = New Size(Me.Width - 60, Me.Height - 195),
            .Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        }
        AppManager.ApplyGridTheme(dgvReport)

        Me.Controls.AddRange({lblTitle, lblType, cbReportType, lblFrom, dtpFrom, lblTo, dtpTo, btnGenerate, lblSummary, dgvReport})
    End Sub

    Private Sub btnGenerate_Click(sender As Object, e As EventArgs) Handles btnGenerate.Click
        Select Case cbReportType.SelectedIndex
            Case 0 : GenerateSalesReport()
            Case 1 : GeneratePurchaseReport()
            Case 2 : GenerateExpiryReport()
            Case 3 : GenerateLowStockReport()
            Case 4 : GenerateValuationReport()
        End Select
    End Sub

    Private Sub GenerateSalesReport()
        Try
            Using db As New PharmacyContext()
                Dim fromD = dtpFrom.Value.Date
                Dim toD = dtpTo.Value.Date.AddDays(1)
                Dim data = (From s In db.Sales
                            Where s.SaleDate >= fromD AndAlso s.SaleDate < toD
                            Select New With {
                                .SaleID = s.SaleID,
                                .Date = s.SaleDate.ToString("yyyy-MM-dd HH:mm"),
                                .Payment = s.PaymentMethod,
                                .Total = s.TotalAmount
                            }).ToList()
                dgvReport.DataSource = data
                Dim grandTotal = data.Sum(Function(x) x.Total)
                lblSummary.Text = $"Total Sales: {data.Count} transactions | Revenue: {grandTotal:C2}"

                If dgvReport.Columns.Count > 0 Then
                    dgvReport.Columns("Total").DefaultCellStyle.Format = "C2"
                    dgvReport.Columns("Date").AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                End If
            End Using
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "Reports", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub GeneratePurchaseReport()
        Try
            Using db As New PharmacyContext()
                Dim fromD = dtpFrom.Value.Date
                Dim toD = dtpTo.Value.Date.AddDays(1)
                Dim data = (From p In db.Purchases
                            Join s In db.Suppliers On p.SupplierID Equals s.SupplierID
                            Where p.PurchaseDate >= fromD AndAlso p.PurchaseDate < toD
                            Select New With {
                                .PurchaseID = p.PurchaseID,
                                .Date = p.PurchaseDate.ToString("yyyy-MM-dd"),
                                .Supplier = s.CompanyName,
                                .Invoice = p.ReferenceInvoice,
                                .Total = p.TotalAmount
                            }).ToList()
                dgvReport.DataSource = data
                Dim grandTotal = data.Sum(Function(x) x.Total)
                lblSummary.Text = $"Total Purchases: {data.Count} | Cost: {grandTotal:C2}"
            End Using
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "Reports", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub GenerateExpiryReport()
        Try
            Using db As New PharmacyContext()
                Dim warningDate = DateTime.Now.AddDays(90)
                Dim data = (From b In db.Batches
                            Join p In db.Products On b.ProductID Equals p.ProductID
                            Where b.ExpiryDate <= warningDate AndAlso b.CurrentQuantity > 0
                            Order By b.ExpiryDate
                            Select New With {
                                .Product = p.Name,
                                .Batch = b.BatchNumber,
                                .Expiry = b.ExpiryDate.ToShortDateString(),
                                .DaysLeft = (b.ExpiryDate - DateTime.Now).Days,
                                .Stock = b.CurrentQuantity,
                                .Status = If(b.ExpiryDate < DateTime.Now, "EXPIRED", "Expiring Soon")
                            }).ToList()
                dgvReport.DataSource = data
                lblSummary.Text = $"Found {data.Count} items expiring within 90 days or already expired."
            End Using
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "Reports", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub GenerateLowStockReport()
        Try
            Using db As New PharmacyContext()
                Dim data = (From p In db.Products
                            Join c In db.Categories On p.CategoryID Equals c.CategoryID
                            Group Join b In db.Batches On p.ProductID Equals b.ProductID Into bGroup = Group
                            Let totalStock = bGroup.Sum(Function(x) CType(x.CurrentQuantity, Integer?))
                            Where (If(totalStock, 0)) <= p.ReorderPoint
                            Select New With {
                                .Product = p.Name,
                                .Category = c.CategoryName,
                                .TotalStock = If(totalStock, 0),
                                .ReorderPoint = p.ReorderPoint,
                                .Status = If(If(totalStock, 0) = 0, "OUT OF STOCK", "Low Stock")
                            }).ToList()
                dgvReport.DataSource = data
                lblSummary.Text = $"Found {data.Count} products at or below reorder level."
            End Using
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "Reports", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub GenerateValuationReport()
        Try
            Using db As New PharmacyContext()
                Dim data = (From p In db.Products
                            Join c In db.Categories On p.CategoryID Equals c.CategoryID
                            Group Join b In db.Batches On p.ProductID Equals b.ProductID Into bGroup = Group
                            Let totalStock = bGroup.Sum(Function(x) CType(x.CurrentQuantity, Integer?))
                            Let totalCostVal = bGroup.Sum(Function(x) CType(x.CurrentQuantity * x.CostPrice, Decimal?))
                            Let totalSellVal = bGroup.Sum(Function(x) CType(x.CurrentQuantity * x.SellingPrice, Decimal?))
                            Select New With {
                                .Product = p.Name,
                                .Category = c.CategoryName,
                                .TotalStock = If(totalStock, 0),
                                .CostValue = If(totalCostVal, 0D),
                                .RetailValue = If(totalSellVal, 0D),
                                .Profit = If(totalSellVal, 0D) - If(totalCostVal, 0D)
                            }).ToList()
                dgvReport.DataSource = data
                Dim totalCost = data.Sum(Function(x) x.CostValue)
                Dim totalRetail = data.Sum(Function(x) x.RetailValue)
                lblSummary.Text = $"Total Cost: {totalCost:C2} | Retail Value: {totalRetail:C2} | Potential Profit: {(totalRetail - totalCost):C2}"
            End Using
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "Reports", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        If dgvReport IsNot Nothing Then
            Dim gridRect As New Rectangle(dgvReport.Left - 1, dgvReport.Top - 1, dgvReport.Width + 1, dgvReport.Height + 1)
            Using p As New Pen(AppManager.CurrentTheme.DividerColor, 1)
                e.Graphics.DrawRectangle(p, gridRect)
            End Using
        End If
    End Sub
End Class
