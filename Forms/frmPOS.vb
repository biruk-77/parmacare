Imports WinFormsApp1.Models
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports System.Linq
Imports WinFormsApp1.Core

Public Class frmPOS
    Inherits Form

    Private WithEvents txtSearch As ModernTextBox
    Private WithEvents dgvProducts As DataGridView
    Private WithEvents dgvCart As DataGridView
    Private WithEvents btnCheckout As CyberButton
    Private WithEvents btnClearCart As CyberButton
    Private lblTitle As Label
    Private lblCartHeader As Label
    Private lblTotal As Label
    Private cbPayment As ComboBox

    Private _allProducts As Object

    Public Sub New()
        Me.SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.UserPaint Or ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw, True)
        Me.DoubleBuffered = True
        InitializeUI()
        LoadProducts()
    End Sub

    Private Sub InitializeUI()
        Me.FormBorderStyle = FormBorderStyle.None
        Me.TopLevel = False
        Me.Dock = DockStyle.Fill
        Me.BackColor = AppManager.CurrentTheme.Background

        Dim theme = AppManager.CurrentTheme

        lblTitle = New Label() With {
            .Text = AppManager.GetText("MenuPOS"),
            .Font = New Font("Segoe UI", 18, FontStyle.Bold),
            .ForeColor = theme.PrimaryText,
            .Location = New Point(20, 15),
            .AutoSize = True
        }

        txtSearch = New ModernTextBox() With {
            .Location = New Point(20, 55),
            .Size = New Size(380, 40),
            .PlaceholderText = "Search product name or batch...",
            .IconChar = ChrW(&HE72A)
        }
        txtSearch.ApplyTheme()

        ' ---- Products Grid (Left) ----
        dgvProducts = New DataGridView()
        AppManager.ApplyGridTheme(dgvProducts)
        dgvProducts.Location = New Point(20, 105)
        dgvProducts.Size = New Size(400, Me.Height - 125)
        dgvProducts.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left

        ' ---- Cart Section (Right) ----
        lblCartHeader = New Label() With {
            .Text = ChrW(&HE7BF) & "  Shopping Cart",
            .Font = New Font("Segoe UI Semibold", 14),
            .ForeColor = theme.PrimaryText,
            .Location = New Point(440, 15),
            .AutoSize = True
        }

        dgvCart = New DataGridView()
        AppManager.ApplyGridTheme(dgvCart)
        dgvCart.Location = New Point(440, 55)
        dgvCart.Size = New Size(Me.Width - 460, Me.Height - 175)
        dgvCart.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        dgvCart.ReadOnly = False
        dgvCart.AllowUserToDeleteRows = True
        dgvCart.AllowUserToAddRows = False

        lblTotal = New Label() With {
            .Text = "TOTAL: $0.00",
            .Font = New Font("Segoe UI", 22, FontStyle.Bold),
            .ForeColor = theme.Accent,
            .Location = New Point(440, Me.Height - 110),
            .Anchor = AnchorStyles.Bottom Or AnchorStyles.Left,
            .AutoSize = True
        }

        ' Anchor these to the bottom-right, but position them explicitly relative to typical width
        Dim rightEdge = Me.Width - 20
        
        btnCheckout = New CyberButton() With {
            .Text = ChrW(&HE8A1) & " FINALIZE SALE",
            .Location = New Point(rightEdge - 160, Me.Height - 108),
            .Size = New Size(160, 45),
            .Anchor = AnchorStyles.Bottom Or AnchorStyles.Right,
            .BackColor = theme.Accent,
            .Font = New Font("Segoe UI Semibold", 10)
        }

        btnClearCart = New CyberButton() With {
            .Text = "Clear",
            .Location = New Point(rightEdge - 270, Me.Height - 108),
            .Size = New Size(100, 45),
            .Anchor = AnchorStyles.Bottom Or AnchorStyles.Right,
            .BackColor = Color.FromArgb(150, 50, 50),
            .Font = New Font("Segoe UI Semibold", 10)
        }

        Dim lblPayMethod As New Label() With {
            .Text = "Payment:",
            .Font = New Font("Segoe UI Semibold", 9),
            .ForeColor = theme.SecondaryText,
            .Location = New Point(440, Me.Height - 123),
            .Anchor = AnchorStyles.Bottom Or AnchorStyles.Left,
            .AutoSize = True
        }

        cbPayment = New ComboBox() With {
            .Location = New Point(440, Me.Height - 102),
            .Size = New Size(140, 35),
            .DropDownStyle = ComboBoxStyle.DropDownList,
            .Font = New Font("Segoe UI", 11),
            .Anchor = AnchorStyles.Bottom Or AnchorStyles.Left,
            .BackColor = theme.InputBackground,
            .ForeColor = theme.PrimaryText
        }
        cbPayment.Items.AddRange({"Cash", "Card", "Mobile", "Insurance"})
        cbPayment.SelectedIndex = 0

        Me.Controls.AddRange({lblTitle, txtSearch, dgvProducts, lblCartHeader, dgvCart, lblTotal, lblPayMethod, cbPayment, btnClearCart, btnCheckout})
        SetupCartColumns()
        AddHandler dgvCart.DataError, AddressOf dgvCart_DataError
    End Sub

    Private Sub dgvCart_DataError(sender As Object, e As DataGridViewDataErrorEventArgs)
        ' Suppress ugly default DataGridView error dialogs
        e.ThrowException = False
    End Sub


    Private Sub SetupCartColumns()
        dgvCart.Columns.Clear()
        dgvCart.AutoGenerateColumns = False
        dgvCart.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "CartName", .HeaderText = "Item", .ReadOnly = True, .AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill})
        dgvCart.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "CartBatch", .HeaderText = "Batch", .ReadOnly = True, .Width = 90})
        dgvCart.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "CartPrice", .HeaderText = "Price", .ReadOnly = True, .Width = 80})
        dgvCart.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "CartQty", .HeaderText = "Qty", .ReadOnly = False, .Width = 55})
        dgvCart.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "CartSubTotal", .HeaderText = "SubTotal", .ReadOnly = True, .Width = 95})
        dgvCart.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "CartBatchID", .Visible = False})
        dgvCart.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "CartMaxQty", .Visible = False})
        dgvCart.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "CartRawPrice", .Visible = False})

        Dim btnRemoveCol As New DataGridViewButtonColumn() With {
            .Name = "Remove",
            .HeaderText = "",
            .Text = "X",
            .UseColumnTextForButtonValue = True,
            .Width = 35,
            .FlatStyle = FlatStyle.Flat
        }
        dgvCart.Columns.Add(btnRemoveCol)
    End Sub

    Private Sub LoadProducts()
        Try
            Using db As New PharmacyContext()
                Dim prods = (From p In db.Products
                             Join b In db.Batches On p.ProductID Equals b.ProductID
                             Where b.CurrentQuantity > 0 AndAlso b.ExpiryDate > DateTime.Now
                             Select New With {
                                 .BatchID = b.BatchID,
                                 .Name = p.Name,
                                 .Batch = b.BatchNumber,
                                 .Price = b.SellingPrice,
                                 .Stock = b.CurrentQuantity,
                                 .Expiry = b.ExpiryDate.ToShortDateString()
                             }).ToList()
                _allProducts = prods
                dgvProducts.DataSource = prods

                If dgvProducts.Columns.Count > 0 Then
                    dgvProducts.Columns("BatchID").Visible = False
                    dgvProducts.Columns("Name").AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                    dgvProducts.Columns("Batch").Width = 80
                    dgvProducts.Columns("Price").Width = 70
                    dgvProducts.Columns("Price").DefaultCellStyle.Format = "C2"
                    dgvProducts.Columns("Stock").Width = 50
                    dgvProducts.Columns("Expiry").Width = 85
                End If
            End Using
        Catch ex As Exception
            MessageBox.Show("Error loading products: " & ex.Message, "POS", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub txtSearch_TextChanged(sender As Object, e As EventArgs) Handles txtSearch.TextChanged
        Dim q = txtSearch.Text.ToLower()
        If _allProducts Is Nothing Then Return
        If String.IsNullOrWhiteSpace(q) Then
            dgvProducts.DataSource = _allProducts
        Else
            Dim filtered = CType(_allProducts, IEnumerable(Of Object)).Where(Function(item)
                Dim n = CallByName(item, "Name", CallType.Get).ToString().ToLower()
                Dim b = CallByName(item, "Batch", CallType.Get).ToString().ToLower()
                Return n.Contains(q) OrElse b.Contains(q)
            End Function).ToList()
            dgvProducts.DataSource = filtered
        End If
    End Sub

    ' Double-click a product row to add it to cart
    Private Sub dgvProducts_CellDoubleClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvProducts.CellDoubleClick
        If e.RowIndex < 0 Then Return

        Dim row = dgvProducts.Rows(e.RowIndex)
        Dim batchID = CInt(row.Cells("BatchID").Value)
        Dim name = row.Cells("Name").Value.ToString()
        Dim batch = row.Cells("Batch").Value.ToString()
        Dim price = CDec(row.Cells("Price").Value)
        Dim maxQty = CInt(row.Cells("Stock").Value)

        ' Check if already in cart
        For Each cartRow As DataGridViewRow In dgvCart.Rows
            If cartRow.Cells("CartBatchID").Value IsNot Nothing AndAlso CInt(cartRow.Cells("CartBatchID").Value) = batchID Then
                Dim currentQty = CInt(cartRow.Cells("CartQty").Value)
                If currentQty + 1 > maxQty Then
                    MessageBox.Show($"Cannot add more. Only {maxQty} in stock.", "Stock Limit", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                    Return
                End If
                Dim newQty = currentQty + 1
                cartRow.Cells("CartQty").Value = newQty
                cartRow.Cells("CartSubTotal").Value = (price * newQty).ToString("C2")
                cartRow.Cells("CartRawPrice").Value = price
                UpdateTotal()
                Return
            End If
        Next

        ' Add new row to cart
        Dim idx = dgvCart.Rows.Add()
        dgvCart.Rows(idx).Cells("CartName").Value = name
        dgvCart.Rows(idx).Cells("CartBatch").Value = batch
        dgvCart.Rows(idx).Cells("CartPrice").Value = price.ToString("C2")
        dgvCart.Rows(idx).Cells("CartRawPrice").Value = price
        dgvCart.Rows(idx).Cells("CartQty").Value = 1
        dgvCart.Rows(idx).Cells("CartSubTotal").Value = price.ToString("C2")
        dgvCart.Rows(idx).Cells("CartBatchID").Value = batchID
        dgvCart.Rows(idx).Cells("CartMaxQty").Value = maxQty

        UpdateTotal()
    End Sub

    ' Handle qty edits and remove button clicks
    Private Sub dgvCart_CellEndEdit(sender As Object, e As DataGridViewCellEventArgs) Handles dgvCart.CellEndEdit
        If e.RowIndex < 0 Then Return
        Dim row = dgvCart.Rows(e.RowIndex)
        If dgvCart.Columns(e.ColumnIndex).Name = "CartQty" Then
            Dim qty As Integer
            If Not Integer.TryParse(row.Cells("CartQty").Value?.ToString(), qty) OrElse qty < 1 Then
                row.Cells("CartQty").Value = 1
                qty = 1
            End If

            Dim maxQty = CInt(row.Cells("CartMaxQty").Value)
            If qty > maxQty Then
                MessageBox.Show($"Only {maxQty} available in stock.", "Stock Limit", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                row.Cells("CartQty").Value = maxQty
                qty = maxQty
            End If

            Dim price = CDec(row.Cells("CartRawPrice").Value)
            row.Cells("CartSubTotal").Value = (price * qty).ToString("C2")
            UpdateTotal()
        End If
    End Sub

    Private Sub dgvCart_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvCart.CellContentClick
        If e.RowIndex < 0 Then Return
        If dgvCart.Columns(e.ColumnIndex).Name = "Remove" Then
            dgvCart.Rows.RemoveAt(e.RowIndex)
            UpdateTotal()
        End If
    End Sub

    Private Sub UpdateTotal()
        Dim total As Decimal = 0
        For Each row As DataGridViewRow In dgvCart.Rows
            If row.Cells("CartRawPrice").Value IsNot Nothing AndAlso row.Cells("CartQty").Value IsNot Nothing Then
                total += CDec(row.Cells("CartRawPrice").Value) * CInt(row.Cells("CartQty").Value)
            End If
        Next
        lblTotal.Text = $"TOTAL: {total:C2}"
    End Sub

    Private Sub btnClearCart_Click(sender As Object, e As EventArgs) Handles btnClearCart.Click
        dgvCart.Rows.Clear()
        UpdateTotal()
    End Sub

    Private Sub btnCheckout_Click(sender As Object, e As EventArgs) Handles btnCheckout.Click
        If dgvCart.Rows.Count = 0 Then
            MessageBox.Show("Cart is empty. Add items before checking out.", "POS", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Dim total As Decimal = 0
        For Each row As DataGridViewRow In dgvCart.Rows
            If row.Cells("CartRawPrice").Value IsNot Nothing AndAlso row.Cells("CartQty").Value IsNot Nothing Then
                total += CDec(row.Cells("CartRawPrice").Value) * CInt(row.Cells("CartQty").Value)
            End If
        Next

        Dim confirm = MessageBox.Show(
            $"Finalize this sale for {total:C2}?" & vbCrLf & $"Payment: {cbPayment.SelectedItem}",
            "Confirm Sale",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question)

        If confirm <> DialogResult.Yes Then Return

        Try
            Using db As New PharmacyContext()
                ' 1. Create Sale record
                Dim sale As New Sale() With {
                    .UserID = If(SessionManager.CurrentUser?.UserID, 1),
                    .SaleDate = DateTime.Now,
                    .TotalAmount = total,
                    .PaymentMethod = cbPayment.SelectedItem.ToString()
                }
                db.Sales.Add(sale)
                db.SaveChanges()

                ' 2. Add SaleDetails and Deduct Stock
                For Each row As DataGridViewRow In dgvCart.Rows
                    If row.IsNewRow Then Continue For
                    If row.Cells("CartBatchID").Value Is Nothing Then Continue For
                    
                    Dim batchID = CInt(row.Cells("CartBatchID").Value)
                    Dim qty = CInt(row.Cells("CartQty").Value)
                    Dim price = CDec(row.Cells("CartRawPrice").Value)

                    If qty <= 0 Then Continue For ' Double safety check

                    ' Deduct stock from Batch
                    Dim batch = db.Batches.FirstOrDefault(Function(b) b.BatchID = batchID)
                    If batch IsNot Nothing Then
                        If batch.CurrentQuantity < qty Then
                            MessageBox.Show($"Insufficient stock for batch {batch.BatchNumber}. Sale aborted.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Return
                        End If
                        batch.CurrentQuantity -= qty
                    End If

                    ' Record SaleDetail
                    Dim sd As New SaleDetail() With {
                        .SaleID = sale.SaleID,
                        .BatchID = batchID,
                        .Quantity = qty,
                        .UnitPrice = price
                    }
                    db.SaleDetails.Add(sd)
                Next

                db.SaveChanges()
            End Using

            MessageBox.Show("Sale completed successfully! " & ChrW(&H2714), "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
            dgvCart.Rows.Clear()
            UpdateTotal()
            LoadProducts() ' Refresh available stock
        Catch ex As Exception
            Dim msg = ex.Message
            If ex.InnerException IsNot Nothing Then msg &= vbCrLf & "Details: " & ex.InnerException.Message
            MessageBox.Show("Error processing sale: " & msg, "POS Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        Dim g = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias
        Dim theme = AppManager.CurrentTheme

        ' Divider between products and cart
        Using p As New Pen(theme.DividerColor, 1)
            g.DrawLine(p, 430, 10, 430, Me.Height - 10)
        End Using
    End Sub
End Class
