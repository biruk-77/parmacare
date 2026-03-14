Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.InteropServices
Imports System.Windows.Forms
Imports WinFormsApp1.Models
Imports WinFormsApp1.Core

Public Class frmAddProduct
    Inherits Form

    ' UI Controls
    Private panelMain As Panel
    Private lblHeader As Label
    
    Private cbCategory As ComboBox
    Private txtName As TextBox
    Private txtGeneric As TextBox
    Private txtManufacturer As TextBox
    Private numReorder As NumericUpDown

    Private txtBatchNum As TextBox
    Private dtpExpiry As DateTimePicker
    Private numQty As NumericUpDown
    Private numCost As NumericUpDown
    Private numSell As NumericUpDown

    Private WithEvents btnSave As CyberButton
    Private WithEvents btnCancel As CyberButton

    Private _editProductID As Integer = 0
    Private _editBatchID As Integer = 0

    Public Sub New(Optional productID As Integer = 0, Optional batchID As Integer = 0)
        _editProductID = productID
        _editBatchID = batchID
        Me.DoubleBuffered = True
        InitializeUI()
        LoadCategories()
        
        If _editProductID > 0 OrElse _editBatchID > 0 Then
            LoadDataForEdit()
            lblHeader.Text = "Edit Item Details"
            btnSave.Text = "Update Information"
        End If
    End Sub

    Private Sub LoadDataForEdit()
        Try
            Using db As New PharmacyContext()
                If _editProductID > 0 Then
                    Dim prod = db.Products.Find(_editProductID)
                    If prod IsNot Nothing Then
                        txtName.Text = prod.Name
                        txtGeneric.Text = prod.GenericName
                        txtManufacturer.Text = prod.Manufacturer
                        cbCategory.SelectedValue = prod.CategoryID
                        numReorder.Value = prod.ReorderPoint
                    End If
                End If

                If _editBatchID > 0 Then
                    Dim batch = db.Batches.Find(_editBatchID)
                    If batch IsNot Nothing Then
                        txtBatchNum.Text = batch.BatchNumber
                        dtpExpiry.Value = batch.ExpiryDate
                        numQty.Value = batch.CurrentQuantity
                        numCost.Value = batch.CostPrice
                        numSell.Value = batch.SellingPrice
                        ' If we didn't have a product ID passed but have a batch, get product info
                        If _editProductID = 0 Then
                            _editProductID = batch.ProductID
                            LoadDataForEdit()
                        End If
                    End If
                End If
            End Using
        Catch ex As Exception
            MessageBox.Show("Error loading edit data: " & ex.Message)
        End Try
    End Sub

    Private Sub InitializeUI()
        Me.FormBorderStyle = FormBorderStyle.None
        Me.StartPosition = FormStartPosition.CenterParent
        Me.Size = New Size(600, 650)
        Me.BackColor = AppManager.CurrentTheme.Surface

        panelMain = New Panel() With {
            .Dock = DockStyle.Fill,
            .Padding = New Padding(20)
        }
        Me.Controls.Add(panelMain)

        lblHeader = New Label() With {
            .Text = "Add New Product & Initial Batch",
            .Font = New Font("Segoe UI", 16, FontStyle.Bold),
            .ForeColor = AppManager.CurrentTheme.PrimaryText,
            .AutoSize = True,
            .Location = New Point(20, 20)
        }
        panelMain.Controls.Add(lblHeader)

        Dim yPos = 80
        
        ' Product Info Section
        AddLabel("Product Information", yPos, True) : yPos += 45

        AddLabel("Category", yPos - 18, False, 20)
        cbCategory = New ComboBox() With {.Location = New Point(20, yPos), .Width = 250, .DropDownStyle = ComboBoxStyle.DropDownList, .Font = New Font("Segoe UI", 10)}
        
        AddLabel("Brand Name", yPos - 18, False, 300)
        txtName = New TextBox() With {.Location = New Point(300, yPos), .Width = 250, .Font = New Font("Segoe UI", 10)}
        yPos += 60

        AddLabel("Generic Name", yPos - 18, False, 20)
        txtGeneric = New TextBox() With {.Location = New Point(20, yPos), .Width = 250, .Font = New Font("Segoe UI", 10)}

        AddLabel("Manufacturer", yPos - 18, False, 300)
        txtManufacturer = New TextBox() With {.Location = New Point(300, yPos), .Width = 250, .Font = New Font("Segoe UI", 10)}
        yPos += 60

        AddLabel("Reorder Point", yPos - 18, False, 20)
        numReorder = New NumericUpDown() With {.Location = New Point(20, yPos), .Width = 100, .Value = 10, .Font = New Font("Segoe UI", 10)}
        yPos += 70

        ' Initial Batch Info Section
        AddLabel("Initial Batch Information (Optional)", yPos, True) : yPos += 45

        AddLabel("Batch Number", yPos - 18, False, 20)
        txtBatchNum = New TextBox() With {.Location = New Point(20, yPos), .Width = 250, .Font = New Font("Segoe UI", 10)}

        AddLabel("Expiry Date", yPos - 18, False, 300)
        dtpExpiry = New DateTimePicker() With {.Location = New Point(300, yPos), .Width = 250, .Format = DateTimePickerFormat.Short, .Font = New Font("Segoe UI", 10)}
        yPos += 60

        AddLabel("Initial Quantity", yPos - 18, False, 20)
        numQty = New NumericUpDown() With {.Location = New Point(20, yPos), .Width = 150, .Maximum = 100000, .Font = New Font("Segoe UI", 10)}
        
        AddLabel("Cost Price", yPos - 18, False, 200)
        numCost = New NumericUpDown() With {.Location = New Point(200, yPos), .Width = 150, .DecimalPlaces = 2, .Maximum = 100000, .Font = New Font("Segoe UI", 10)}

        AddLabel("Selling Price", yPos - 18, False, 380)
        numSell = New NumericUpDown() With {.Location = New Point(380, yPos), .Width = 150, .DecimalPlaces = 2, .Maximum = 100000, .Font = New Font("Segoe UI", 10)}
        yPos += 80

        ' Buttons
        btnCancel = New CyberButton() With {
            .Text = "Cancel",
            .Location = New Point(280, yPos),
            .Size = New Size(130, 45),
            .BackColor = Color.FromArgb(100, 100, 100),
            .Font = New Font("Segoe UI Semibold", 10)
        }
        
        btnSave = New CyberButton() With {
            .Text = "Save Product",
            .Location = New Point(420, yPos),
            .Size = New Size(130, 45),
            .BackColor = AppManager.CurrentTheme.Accent,
            .Font = New Font("Segoe UI Semibold", 10)
        }

        panelMain.Controls.AddRange({cbCategory, txtName, txtGeneric, txtManufacturer, numReorder, txtBatchNum, dtpExpiry, numQty, numCost, numSell, btnCancel, btnSave})
    End Sub

    Private Sub AddLabel(text As String, y As Integer, isHeader As Boolean, Optional x As Integer = 20)
        Dim l As New Label() With {
            .Text = text,
            .Location = New Point(x, y),
            .AutoSize = True,
            .ForeColor = If(isHeader, AppManager.CurrentTheme.Accent, AppManager.CurrentTheme.SecondaryText),
            .Font = If(isHeader, New Font("Segoe UI", 12, FontStyle.Bold), New Font("Segoe UI", 9))
        }
        panelMain.Controls.Add(l)
    End Sub

    Private Sub LoadCategories()
        Try
            Using db As New PharmacyContext()
                Dim cats = db.Categories.ToList()
                If cats.Count = 0 Then
                    ' Seed a default
                    db.Categories.Add(New Category With {.CategoryName = "General"})
                    db.SaveChanges()
                    cats = db.Categories.ToList()
                End If
                cbCategory.DataSource = cats
                cbCategory.DisplayMember = "CategoryName"
                cbCategory.ValueMember = "CategoryID"
            End Using
        Catch ex As Exception
            MessageBox.Show("Error loading categories.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        If String.IsNullOrWhiteSpace(txtName.Text) Then
            MessageBox.Show("Please enter a Brand Name.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Try
            Using db As New PharmacyContext()
                Dim prod As Product
                
                If _editProductID > 0 Then
                    ' EDIT MODE: Load existing product
                    prod = db.Products.Find(_editProductID)
                    If prod Is Nothing Then Throw New Exception("Product not found.")
                Else
                    ' CREATE MODE: New product
                    prod = New Product()
                    prod.CreatedAt = DateTime.Now
                    prod.IsActive = True
                    db.Products.Add(prod)
                End If

                ' Update Product Fields
                prod.Name = txtName.Text.Trim()
                prod.GenericName = txtGeneric.Text.Trim()
                prod.Manufacturer = txtManufacturer.Text.Trim()
                prod.CategoryID = CInt(cbCategory.SelectedValue)
                prod.ReorderPoint = CInt(numReorder.Value)
                
                db.SaveChanges() ' Save product changes and/or get new ProductID

                ' Handle Batch
                If Not String.IsNullOrWhiteSpace(txtBatchNum.Text) Then
                    Dim batch As Batch
                    
                    If _editBatchID > 0 Then
                        ' EDIT MODE: Load existing batch
                        batch = db.Batches.Find(_editBatchID)
                        If batch Is Nothing Then Throw New Exception("Batch not found.")
                    Else
                        ' CREATE MODE: New batch
                        batch = New Batch()
                        batch.ProductID = prod.ProductID
                        batch.ReceivedDate = DateTime.Now
                        db.Batches.Add(batch)
                    End If

                    ' Update Batch Fields
                    batch.BatchNumber = txtBatchNum.Text.Trim()
                    batch.ExpiryDate = dtpExpiry.Value.Date
                    batch.CurrentQuantity = CInt(numQty.Value)
                    batch.CostPrice = numCost.Value
                    batch.SellingPrice = numSell.Value
                    
                    db.SaveChanges()
                End If
            End Using

            MessageBox.Show(If(_editProductID > 0, "Item updated successfully!", "Product added successfully!"), "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Me.Close()
        Catch ex As Exception
            Dim innerMsg = If(ex.InnerException?.InnerException?.Message, If(ex.InnerException?.Message, ex.Message))
            MessageBox.Show("Error saving changes: " & innerMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        Me.Close()
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        Dim g = e.Graphics
        Dim borderRect As New Rectangle(0, 0, Me.Width - 1, Me.Height - 1)
        Using p As New Pen(AppManager.CurrentTheme.DividerColor, 1)
            g.DrawRectangle(p, borderRect)
        End Using
    End Sub
End Class
