Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports System.Linq
Imports WinFormsApp1.Models
Imports WinFormsApp1.Core
Imports Microsoft.EntityFrameworkCore

Public Class frmInventory
    Inherits Form

    Private WithEvents dgvInventory As DataGridView
    Private WithEvents btnAddProduct As CyberButton
    Private WithEvents btnAdjustStock As CyberButton
    Private WithEvents txtSearch As ModernTextBox
    Private lblTitle As Label
    Private lblDesc As Label
    
    Private _allInventory As Object ' Store full list for local filtering

    Public Sub New()
        Me.SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.UserPaint Or ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw, True)
        Me.DoubleBuffered = True
        InitializeUI()
        LoadData()
    End Sub

    Private WithEvents btnEditItem As CyberButton
    
    Private Sub InitializeUI()
        Me.FormBorderStyle = FormBorderStyle.None
        Me.TopLevel = False
        Me.Dock = DockStyle.Fill
        Me.BackColor = AppManager.CurrentTheme.Background

        Dim theme = AppManager.CurrentTheme

        lblTitle = New Label() With {
            .Text = AppManager.GetText("MenuINVENTORY"),
            .Font = New Font("Segoe UI", 18, FontStyle.Bold),
            .ForeColor = theme.PrimaryText,
            .Location = New Point(40, 30),
            .AutoSize = True
        }

        lblDesc = New Label() With {
            .Text = AppManager.GetText("InventoryDesc"),
            .Font = New Font("Segoe UI", 10),
            .ForeColor = theme.SecondaryText,
            .Location = New Point(42, 65),
            .AutoSize = True
        }

        btnAddProduct = New CyberButton() With {
            .Text = ChrW(&HE710) & " " & AppManager.GetText("AddProductBtn"),
            .Location = New Point(Me.Width - 690, 40),
            .Size = New Size(200, 40),
            .Anchor = AnchorStyles.Top Or AnchorStyles.Right,
            .BackColor = theme.Accent,
            .Font = New Font("Segoe UI Semibold", 10)
        }

        btnEditItem = New CyberButton() With {
            .Text = ChrW(&HE70F) & " Edit Item",
            .Location = New Point(Me.Width - 470, 40),
            .Size = New Size(200, 40),
            .Anchor = AnchorStyles.Top Or AnchorStyles.Right,
            .BackColor = Color.FromArgb(0, 120, 215), ' Windows Blue
            .Font = New Font("Segoe UI Semibold", 10),
            .Visible = (SessionManager.CurrentUser?.RoleID = 1) ' Only Admin
        }

        btnAdjustStock = New CyberButton() With {
            .Text = ChrW(&HE70F) & " Adjust Stock",
            .Location = New Point(Me.Width - 250, 40),
            .Size = New Size(200, 40),
            .Anchor = AnchorStyles.Top Or AnchorStyles.Right,
            .BackColor = Color.FromArgb(120, 120, 120),
            .Font = New Font("Segoe UI Semibold", 10)
        }

        txtSearch = New ModernTextBox() With {
            .Location = New Point(40, 95),
            .Size = New Size(400, 45),
            .PlaceholderText = "Search generic, name, batch...",
            .IconChar = ChrW(&HE72A)
        }
        txtSearch.ApplyTheme()

        dgvInventory = New DataGridView() With {
            .Location = New Point(40, 155),
            .Size = New Size(Me.Width - 80, Me.Height - 195),
            .Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        }
        AppManager.ApplyGridTheme(dgvInventory)

        Me.Controls.AddRange({lblTitle, lblDesc, txtSearch, btnAddProduct, btnEditItem, btnAdjustStock, dgvInventory})
    End Sub

    Public Sub LoadData()
        Try
            Using db As New PharmacyContext()
                Dim inventory = (From p In db.Products
                                 Join c In db.Categories On p.CategoryID Equals c.CategoryID
                                 Group Join b In db.Batches On p.ProductID Equals b.ProductID Into batchGroup = Group
                                 From b In batchGroup.DefaultIfEmpty()
                                 Select New With {
                                     .Item = p.Name,
                                     .Category = c.CategoryName,
                                     .Batch = If(b IsNot Nothing, b.BatchNumber, "N/A"),
                                     .Stock = If(b IsNot Nothing, b.CurrentQuantity, 0),
                                     .Price = If(b IsNot Nothing, b.SellingPrice, 0D),
                                     .Expiry = If(b IsNot Nothing, b.ExpiryDate.ToShortDateString(), "N/A"),
                                     .Status = If(b IsNot Nothing AndAlso b.CurrentQuantity <= p.ReorderPoint, "Low Stock", "Okay"),
                                     .BatchID = If(b IsNot Nothing, b.BatchID, 0)
                                 }).ToList()

                _allInventory = inventory
                dgvInventory.DataSource = inventory
                
                If dgvInventory.Columns.Count > 0 Then
                    dgvInventory.Columns("Item").AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                    dgvInventory.Columns("Category").Width = 150
                    dgvInventory.Columns("Batch").Width = 120
                    dgvInventory.Columns("Stock").Width = 80
                    dgvInventory.Columns("Price").Width = 100
                    dgvInventory.Columns("Price").DefaultCellStyle.Format = "C2"
                    dgvInventory.Columns("Expiry").Width = 120
                    dgvInventory.Columns("Status").Width = 120
                End If
            End Using
        Catch err As Exception
            MessageBox.Show("Inventory Data Error: " & err.Message, "PharmaCare", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        Dim g = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias
        
        If dgvInventory IsNot Nothing Then
            Dim gridRect As New Rectangle(dgvInventory.Left - 1, dgvInventory.Top - 1, dgvInventory.Width + 1, dgvInventory.Height + 1)
            Using p As New Pen(AppManager.CurrentTheme.DividerColor, 1)
                g.DrawRectangle(p, gridRect)
            End Using
        End If
    End Sub

    Private Sub txtSearch_TextChanged(sender As Object, e As EventArgs) Handles txtSearch.TextChanged
        Dim q = txtSearch.Text.ToLower()
        If _allInventory Is Nothing Then Return

        If String.IsNullOrWhiteSpace(q) Then
            dgvInventory.DataSource = _allInventory
        Else
            Dim filtered = CType(_allInventory, IEnumerable(Of Object)).Where(Function(item)
                Dim strVal = item.ToString().ToLower()
                ' Using dynamic/reflection stringification for simple quick filtering
                ' In production with specific types, this should be strongly typed.
                Dim Name = CallByName(item, "Item", CallType.Get).ToString().ToLower()
                Dim Batch = CallByName(item, "Batch", CallType.Get).ToString().ToLower()
                Dim Cat = CallByName(item, "Category", CallType.Get).ToString().ToLower()
                Return Name.Contains(q) OrElse Batch.Contains(q) OrElse Cat.Contains(q)
            End Function).ToList()
            
            dgvInventory.DataSource = filtered
        End If
    End Sub

    Private Sub btnAddProduct_Click(sender As Object, e As EventArgs) Handles btnAddProduct.Click
        Using f As New frmAddProduct()
            f.ShowDialog()
            LoadData()
        End Using
    End Sub

    Private Sub btnEditItem_Click(sender As Object, e As EventArgs) Handles btnEditItem.Click
        If dgvInventory.SelectedRows.Count = 0 Then
            MessageBox.Show("Please select an item to edit.", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Dim row = dgvInventory.SelectedRows(0)
        Dim bID = CInt(row.Cells("BatchID").Value)
        
        ' Need to get ProductID for editing. We can find it from the batch or by name.
        ' To be safe, let's pass bID and modify frmAddProduct to find current product info.
        ' Since we already did that in frmAddProduct, we just need to pass bID.
        
        Using f As New frmAddProduct(0, bID)
            f.ShowDialog()
            LoadData()
        End Using
    End Sub

    Private Sub btnAdjustStock_Click(sender As Object, e As EventArgs) Handles btnAdjustStock.Click
        If dgvInventory.SelectedRows.Count = 0 Then
            MessageBox.Show("Please select an item to adjust its stock.", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Dim row = dgvInventory.SelectedRows(0)
        Dim bID = CInt(row.Cells("BatchID").Value)
        If bID = 0 Then
            MessageBox.Show("This product has no active batch. Please add stock through a purchase or new batch.", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Using f As New frmAdjustStock(bID)
            f.ShowDialog()
            LoadData()
        End Using
    End Sub
End Class
