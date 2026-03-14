Imports WinFormsApp1.Models
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports System.Linq
Imports WinFormsApp1.Core

Public Class frmPurchase
    Inherits Form

    ' Top Section Controls
    Private lblHeader As Label
    Private lblDesc As Label
    Private lblInvoiceNum As Label
    Private lblDate As Label
    Private lblItemCount As Label

    ' Supplier / Invoice Controls
    Private cbSupplier As ComboBox
    Private txtInvoice As ModernTextBox

    ' Grid
    Private WithEvents dgvItems As DataGridView

    ' Bottom Action Bar
    Private WithEvents btnAddLine As CyberButton
    Private WithEvents btnRemoveAll As CyberButton
    Private WithEvents btnSave As CyberButton
    Private WithEvents btnCancel As CyberButton
    Private lblTotal As Label
    Private lblLineCount As Label

    ' Stat Cards
    Private panelStats As Panel

    Public Sub New()
        Me.SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.UserPaint Or ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw, True)
        Me.DoubleBuffered = True
        InitializeUI()
        LoadSuppliers()
    End Sub

    Private Sub InitializeUI()
        Me.FormBorderStyle = FormBorderStyle.None
        Me.TopLevel = False
        Me.Dock = DockStyle.Fill
        Me.BackColor = AppManager.CurrentTheme.Background

        Dim theme = AppManager.CurrentTheme

        ' ========== HEADER SECTION ==========
        lblHeader = New Label() With {
            .Text = ChrW(&HE7BF) & "  Record New Purchase",
            .Font = New Font("Segoe UI", 18, FontStyle.Bold),
            .ForeColor = theme.PrimaryText,
            .Location = New Point(40, 25),
            .AutoSize = True
        }

        lblDesc = New Label() With {
            .Text = "Add incoming stock from suppliers. Fields auto-fill when you select a product.",
            .Font = New Font("Segoe UI", 10),
            .ForeColor = theme.SecondaryText,
            .Location = New Point(42, 60),
            .AutoSize = True
        }

        ' ========== LIVE INFO CARDS ==========
        panelStats = New Panel() With {
            .Location = New Point(40, 95),
            .Size = New Size(Me.Width - 80, 55),
            .Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right,
            .BackColor = Color.Transparent
        }

        lblDate = New Label() With {
            .Text = ChrW(&HE787) & "  " & DateTime.Now.ToString("dddd, MMMM dd, yyyy"),
            .Font = New Font("Segoe UI Semibold", 9),
            .ForeColor = theme.Accent,
            .Location = New Point(0, 5),
            .AutoSize = True
        }

        Dim autoInvoice = "PO-" & DateTime.Now.ToString("yyyyMMdd") & "-" & New Random().Next(100, 999).ToString()
        lblInvoiceNum = New Label() With {
            .Text = ChrW(&HE8F3) & "  Auto Invoice: " & autoInvoice,
            .Font = New Font("Segoe UI Semibold", 9),
            .ForeColor = theme.SecondaryText,
            .Location = New Point(350, 5),
            .AutoSize = True,
            .Tag = autoInvoice
        }

        lblItemCount = New Label() With {
            .Text = ChrW(&HE8B7) & "  Line Items: 0",
            .Font = New Font("Segoe UI Semibold", 9),
            .ForeColor = theme.SecondaryText,
            .Location = New Point(650, 5),
            .AutoSize = True
        }

        panelStats.Controls.AddRange({lblDate, lblInvoiceNum, lblItemCount})

        ' ========== SUPPLIER + INVOICE ROW ==========
        Dim lblSup As New Label() With {
            .Text = "Supplier:",
            .Location = New Point(40, 160),
            .AutoSize = True,
            .Font = New Font("Segoe UI Semibold", 10),
            .ForeColor = theme.PrimaryText
        }
        cbSupplier = New ComboBox() With {
            .Location = New Point(40, 183),
            .Width = 350,
            .DropDownStyle = ComboBoxStyle.DropDownList,
            .Font = New Font("Segoe UI", 11),
            .BackColor = theme.InputBackground,
            .ForeColor = theme.PrimaryText
        }

        Dim lblInv As New Label() With {
            .Text = "Supplier's Receipt / Bill Number (optional, auto-fills):",
            .Location = New Point(420, 160),
            .AutoSize = True,
            .Font = New Font("Segoe UI Semibold", 10),
            .ForeColor = theme.PrimaryText
        }
        txtInvoice = New ModernTextBox() With {
            .Location = New Point(420, 178),
            .Size = New Size(350, 40),
            .PlaceholderText = autoInvoice,
            .IconChar = ChrW(&HE8F3)
        }
        txtInvoice.ApplyTheme()

        ' ========== LINE ITEMS GRID ==========
        dgvItems = New DataGridView() With {
            .Location = New Point(40, 235),
            .Size = New Size(Me.Width - 80, Me.Height - 345),
            .Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right,
            .AutoGenerateColumns = False
        }
        AppManager.ApplyGridTheme(dgvItems)
        dgvItems.AllowUserToAddRows = False

        ' Columns
        dgvItems.Columns.Add(New DataGridViewComboBoxColumn() With {
            .Name = "Product", .HeaderText = "Product", .Width = 180
        })
        dgvItems.Columns.Add(New DataGridViewTextBoxColumn() With {
            .Name = "BatchNum", .HeaderText = "Batch #", .Width = 110
        })
        dgvItems.Columns.Add(New DataGridViewTextBoxColumn() With {
            .Name = "Expiry", .HeaderText = "Expiry Date", .Width = 110
        })
        dgvItems.Columns.Add(New DataGridViewTextBoxColumn() With {
            .Name = "Qty", .HeaderText = "Qty", .Width = 65
        })
        dgvItems.Columns.Add(New DataGridViewTextBoxColumn() With {
            .Name = "Cost", .HeaderText = "Cost Price", .Width = 95
        })
        dgvItems.Columns.Add(New DataGridViewTextBoxColumn() With {
            .Name = "Sell", .HeaderText = "Sell Price", .Width = 95
        })
        dgvItems.Columns.Add(New DataGridViewTextBoxColumn() With {
            .Name = "LineTotal", .HeaderText = "Line Total", .Width = 100, .ReadOnly = True
        })

        Dim btnRemoveCol As New DataGridViewButtonColumn() With {
            .Name = "Remove",
            .HeaderText = "",
            .Text = ChrW(&HE74D),
            .UseColumnTextForButtonValue = True,
            .Width = 40,
            .FlatStyle = FlatStyle.Flat
        }
        dgvItems.Columns.Add(btnRemoveCol)

        ' ========== BOTTOM ACTION BAR ==========
        ' Row 1: Add/Clear buttons on left, Total on right
        btnAddLine = New CyberButton() With {
            .Text = ChrW(&HE710) & " Add Line Item",
            .Location = New Point(40, Me.Height - 100),
            .Size = New Size(160, 42),
            .Anchor = AnchorStyles.Bottom Or AnchorStyles.Left,
            .BackColor = Color.FromArgb(55, 65, 81),
            .Font = New Font("Segoe UI Semibold", 10)
        }

        btnRemoveAll = New CyberButton() With {
            .Text = "Clear All",
            .Location = New Point(210, Me.Height - 100),
            .Size = New Size(100, 42),
            .Anchor = AnchorStyles.Bottom Or AnchorStyles.Left,
            .BackColor = Color.FromArgb(150, 50, 50),
            .Font = New Font("Segoe UI Semibold", 9)
        }

        lblLineCount = New Label() With {
            .Text = "0 items",
            .Font = New Font("Segoe UI", 9),
            .ForeColor = theme.SecondaryText,
            .Location = New Point(320, Me.Height - 87),
            .Anchor = AnchorStyles.Bottom Or AnchorStyles.Left,
            .AutoSize = True
        }

        lblTotal = New Label() With {
            .Text = "TOTAL: $0.00",
            .Font = New Font("Segoe UI", 16, FontStyle.Bold),
            .ForeColor = theme.Accent,
            .Anchor = AnchorStyles.Bottom Or AnchorStyles.Right,
            .AutoSize = True
        }
        lblTotal.Location = New Point(Me.Width - 440, Me.Height - 100)

        btnCancel = New CyberButton() With {
            .Text = "Cancel",
            .Location = New Point(Me.Width - 210, Me.Height - 100),
            .Size = New Size(90, 42),
            .Anchor = AnchorStyles.Bottom Or AnchorStyles.Right,
            .BackColor = Color.FromArgb(100, 100, 100),
            .Font = New Font("Segoe UI Semibold", 10)
        }

        btnSave = New CyberButton() With {
            .Text = ChrW(&HE74E) & " Save",
            .Location = New Point(Me.Width - 110, Me.Height - 100),
            .Size = New Size(80, 42),
            .Anchor = AnchorStyles.Bottom Or AnchorStyles.Right,
            .BackColor = theme.Accent,
            .Font = New Font("Segoe UI Semibold", 10)
        }

        Me.Controls.AddRange({lblHeader, lblDesc, panelStats, lblSup, cbSupplier, lblInv, txtInvoice,
                              dgvItems, btnAddLine, btnRemoveAll, lblLineCount, lblTotal, btnCancel, btnSave})
    End Sub

    ' ========== DATA LOADING ==========
    Private Sub LoadSuppliers()
        Try
            Using db As New PharmacyContext()
                Dim sups = db.Suppliers.ToList()
                If sups.Count = 0 Then
                    db.Suppliers.Add(New Supplier With {.CompanyName = "Default Supplier", .TIN_Number = "000000"})
                    db.SaveChanges()
                    sups = db.Suppliers.ToList()
                End If
                cbSupplier.DataSource = sups
                cbSupplier.DisplayMember = "CompanyName"
                cbSupplier.ValueMember = "SupplierID"
            End Using

            Using db As New PharmacyContext()
                Dim products = db.Products.Select(Function(p) New With {p.ProductID, p.Name}).ToList()
                Dim col = CType(dgvItems.Columns("Product"), DataGridViewComboBoxColumn)
                col.DataSource = products
                col.DisplayMember = "Name"
                col.ValueMember = "ProductID"
            End Using
        Catch ex As Exception
            MessageBox.Show("Error loading data: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' ========== AUTO-FILL LOGIC ==========
    Private Sub dgvItems_CurrentCellDirtyStateChanged(sender As Object, e As EventArgs) Handles dgvItems.CurrentCellDirtyStateChanged
        If dgvItems.IsCurrentCellDirty Then
            dgvItems.CommitEdit(DataGridViewDataErrorContexts.Commit)
        End If
    End Sub

    Private Sub dgvItems_CellValueChanged(sender As Object, e As DataGridViewCellEventArgs) Handles dgvItems.CellValueChanged
        If e.RowIndex < 0 Then Return
        If dgvItems.Columns(e.ColumnIndex).Name = "Product" Then
            Dim row = dgvItems.Rows(e.RowIndex)
            If row.Cells("Product").Value IsNot Nothing Then
                Dim productID = CInt(row.Cells("Product").Value)
                Try
                    Using db As New PharmacyContext()
                        Dim lastBatch = db.Batches.Where(Function(b) b.ProductID = productID).OrderByDescending(Function(b) b.BatchID).FirstOrDefault()

                        ' Auto-generate Batch Number
                        row.Cells("BatchNum").Value = "B" & DateTime.Now.ToString("yyMMdd") & "-" & New Random().Next(100, 999).ToString()
                        ' Auto-set expiry to 2 years from now
                        row.Cells("Expiry").Value = DateTime.Now.AddYears(2).ToShortDateString()
                        ' Default qty = 1
                        row.Cells("Qty").Value = 1

                        If lastBatch IsNot Nothing Then
                            ' Auto-fill last known prices
                            row.Cells("Cost").Value = lastBatch.CostPrice
                            row.Cells("Sell").Value = lastBatch.SellingPrice
                        Else
                            row.Cells("Cost").Value = 0
                            row.Cells("Sell").Value = 0
                        End If

                        Dim cost As Decimal
                        Decimal.TryParse(row.Cells("Cost").Value?.ToString(), cost)
                        row.Cells("LineTotal").Value = cost.ToString("C2")
                        RecalcTotal()
                    End Using
                Catch
                End Try
            End If
        End If
    End Sub

    Private Sub dgvItems_CellEndEdit(sender As Object, e As DataGridViewCellEventArgs) Handles dgvItems.CellEndEdit
        If e.RowIndex < 0 Then Return
        Dim row = dgvItems.Rows(e.RowIndex)
        Dim qty As Integer
        Dim cost As Decimal
        Integer.TryParse(row.Cells("Qty").Value?.ToString(), qty)
        Decimal.TryParse(row.Cells("Cost").Value?.ToString(), cost)
        row.Cells("LineTotal").Value = (qty * cost).ToString("C2")
        RecalcTotal()
    End Sub

    ' ========== GRID ACTIONS ==========
    Private Sub dgvItems_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvItems.CellContentClick
        If e.RowIndex < 0 Then Return
        If dgvItems.Columns(e.ColumnIndex).Name = "Remove" Then
            dgvItems.Rows.RemoveAt(e.RowIndex)
            RecalcTotal()
        End If
    End Sub

    Private Sub btnAddLine_Click(sender As Object, e As EventArgs) Handles btnAddLine.Click
        dgvItems.Rows.Add()
        RecalcTotal()
    End Sub

    Private Sub btnRemoveAll_Click(sender As Object, e As EventArgs) Handles btnRemoveAll.Click
        If dgvItems.Rows.Count = 0 Then Return
        If MessageBox.Show("Clear all line items?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
            dgvItems.Rows.Clear()
            RecalcTotal()
        End If
    End Sub

    ' ========== TOTALS ==========
    Private Sub RecalcTotal()
        Dim total As Decimal = 0
        Dim count As Integer = 0
        For Each row As DataGridViewRow In dgvItems.Rows
            Dim qty As Integer
            Dim cost As Decimal
            Integer.TryParse(row.Cells("Qty").Value?.ToString(), qty)
            Decimal.TryParse(row.Cells("Cost").Value?.ToString(), cost)
            total += qty * cost
            count += 1
        Next
        lblTotal.Text = $"TOTAL: {total:C2}"
        lblLineCount.Text = $"{count} item{If(count <> 1, "s", "")}"
        lblItemCount.Text = ChrW(&HE8B7) & $"  Line Items: {count}"
    End Sub

    ' ========== SAVE ==========
    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        If dgvItems.Rows.Count = 0 Then
            MessageBox.Show("Please add at least one line item before saving.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        If cbSupplier.SelectedValue Is Nothing Then
            MessageBox.Show("Please select a supplier.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Try
            Using db As New PharmacyContext()
                Dim total As Decimal = 0
                Dim invoiceRef = txtInvoice.Text.Trim()
                If String.IsNullOrWhiteSpace(invoiceRef) Then
                    invoiceRef = lblInvoiceNum.Tag?.ToString()
                End If

                Dim purchase As New Purchase() With {
                    .SupplierID = CInt(cbSupplier.SelectedValue),
                    .UserID = If(SessionManager.CurrentUser?.UserID, 1),
                    .PurchaseDate = DateTime.Now,
                    .ReferenceInvoice = invoiceRef
                }

                For Each row As DataGridViewRow In dgvItems.Rows
                    If row.Cells("Product").Value Is Nothing Then Continue For

                    Dim productID = CInt(row.Cells("Product").Value)
                    Dim batchNum = row.Cells("BatchNum").Value?.ToString().Trim()
                    Dim expiryStr = row.Cells("Expiry").Value?.ToString().Trim()
                    Dim qty As Integer : Integer.TryParse(row.Cells("Qty").Value?.ToString(), qty)
                    Dim cost As Decimal : Decimal.TryParse(row.Cells("Cost").Value?.ToString(), cost)
                    Dim sell As Decimal : Decimal.TryParse(row.Cells("Sell").Value?.ToString(), sell)

                    If String.IsNullOrEmpty(batchNum) OrElse qty <= 0 Then Continue For

                    Dim expiryDate As DateTime
                    If Not DateTime.TryParse(expiryStr, expiryDate) Then
                        expiryDate = DateTime.Now.AddYears(2)
                    End If

                    Dim batch = db.Batches.FirstOrDefault(Function(b) b.ProductID = productID AndAlso b.BatchNumber = batchNum)
                    If batch Is Nothing Then
                        batch = New Batch() With {
                            .ProductID = productID,
                            .BatchNumber = batchNum,
                            .ExpiryDate = expiryDate,
                            .CostPrice = cost,
                            .SellingPrice = sell,
                            .CurrentQuantity = qty
                        }
                        db.Batches.Add(batch)
                        db.SaveChanges()
                    Else
                        batch.CurrentQuantity += qty
                        batch.CostPrice = cost
                        batch.SellingPrice = sell
                    End If

                    total += qty * cost
                Next

                purchase.TotalAmount = total
                db.Purchases.Add(purchase)
                db.SaveChanges()

                For Each row As DataGridViewRow In dgvItems.Rows
                    If row.Cells("Product").Value Is Nothing Then Continue For
                    Dim productID = CInt(row.Cells("Product").Value)
                    Dim batchNum = row.Cells("BatchNum").Value?.ToString().Trim()
                    Dim qty As Integer : Integer.TryParse(row.Cells("Qty").Value?.ToString(), qty)
                    Dim cost As Decimal : Decimal.TryParse(row.Cells("Cost").Value?.ToString(), cost)
                    If String.IsNullOrEmpty(batchNum) OrElse qty <= 0 Then Continue For

                    Dim batch = db.Batches.FirstOrDefault(Function(b) b.ProductID = productID AndAlso b.BatchNumber = batchNum)
                    If batch IsNot Nothing Then
                        Dim pd As New PurchaseDetail() With {
                            .PurchaseID = purchase.PurchaseID,
                            .BatchID = batch.BatchID,
                            .Quantity = qty,
                            .UnitCost = cost
                        }
                        db.PurchaseDetails.Add(pd)
                    End If
                Next

                db.SaveChanges()
            End Using

            MessageBox.Show("Purchase recorded successfully! " & ChrW(&H2714), "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
            
            ' Reset form for next entry
            dgvItems.Rows.Clear()
            RecalcTotal()
            txtInvoice.Text = ""
            Dim newInvoice = "PO-" & DateTime.Now.ToString("yyyyMMdd") & "-" & New Random().Next(100, 999).ToString()
            lblInvoiceNum.Text = ChrW(&HE8F3) & "  Auto Invoice: " & newInvoice
            lblInvoiceNum.Tag = newInvoice
            txtInvoice.PlaceholderText = newInvoice

        Catch ex As Exception
            MessageBox.Show("Error saving purchase: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        If dgvItems.Rows.Count > 0 Then
            If MessageBox.Show("Discard this purchase?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Return
        End If
        
        ' Navigate back to overview if embedded, or close if dialog
        If Me.TopLevel Then
            Me.Close()
        Else
            dgvItems.Rows.Clear()
            RecalcTotal()
        End If
    End Sub

    ' ========== PAINTING ==========
    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        Dim g = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias
        Dim theme = AppManager.CurrentTheme

        ' Subtle card behind the grid
        If dgvItems IsNot Nothing Then
            Dim gridRect As New Rectangle(dgvItems.Left - 1, dgvItems.Top - 1, dgvItems.Width + 1, dgvItems.Height + 1)
            Using p As New Pen(theme.DividerColor, 1)
                g.DrawRectangle(p, gridRect)
            End Using
        End If

        ' Separator line above action bar
        Dim sepY = Me.Height - 110
        Using p As New Pen(theme.DividerColor, 1)
            g.DrawLine(p, 40, sepY, Me.Width - 40, sepY)
        End Using
    End Sub
End Class
