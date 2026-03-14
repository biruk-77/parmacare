Imports WinFormsApp1.Models
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports System.Linq
Imports WinFormsApp1.Core

Public Class frmSettings
    Inherits Form

    Private WithEvents btnAddCategory As CyberButton
    Private WithEvents btnAddSupplier As CyberButton
    Private dgvCategories As DataGridView
    Private dgvSuppliers As DataGridView
    Private numLowStock As NumericUpDown
    Private numExpiryDays As NumericUpDown
    Private WithEvents btnSaveSettings As CyberButton
    Private lblTitle As Label

    Public Sub New()
        Me.SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.UserPaint Or ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw, True)
        Me.DoubleBuffered = True
        InitializeUI()
        LoadData()
    End Sub

    Private Sub InitializeUI()
        Me.FormBorderStyle = FormBorderStyle.None
        Me.TopLevel = False
        Me.Dock = DockStyle.Fill
        Me.BackColor = AppManager.CurrentTheme.Background

        Dim theme = AppManager.CurrentTheme

        lblTitle = New Label() With {
            .Text = AppManager.GetText("MenuSETTINGS"),
            .Font = New Font("Segoe UI", 18, FontStyle.Bold),
            .ForeColor = theme.PrimaryText,
            .Location = New Point(30, 20),
            .AutoSize = True
        }

        ' ---- Alert Thresholds Section ----
        Dim lblAlerts As New Label() With {
            .Text = ChrW(&HEA8F) & "  Alert Configuration",
            .Font = New Font("Segoe UI Semibold", 13),
            .ForeColor = theme.Accent,
            .Location = New Point(30, 70),
            .AutoSize = True
        }

        Dim lblLow As New Label() With {.Text = "Default Low Stock Threshold:", .Location = New Point(30, 105), .AutoSize = True, .Font = New Font("Segoe UI", 9), .ForeColor = theme.SecondaryText}
        numLowStock = New NumericUpDown() With {.Location = New Point(30, 125), .Width = 100, .Value = 10, .Maximum = 1000, .Font = New Font("Segoe UI", 10)}

        Dim lblExp As New Label() With {.Text = "Expiry Warning Days:", .Location = New Point(200, 105), .AutoSize = True, .Font = New Font("Segoe UI", 9), .ForeColor = theme.SecondaryText}
        numExpiryDays = New NumericUpDown() With {.Location = New Point(200, 125), .Width = 100, .Value = 90, .Maximum = 365, .Font = New Font("Segoe UI", 10)}

        btnSaveSettings = New CyberButton() With {
            .Text = "Save Settings",
            .Location = New Point(370, 118),
            .Size = New Size(130, 38),
            .BackColor = theme.Accent,
            .Font = New Font("Segoe UI Semibold", 9)
        }

        ' ---- Categories Section ----
        Dim lblCats As New Label() With {
            .Text = ChrW(&HE8FD) & "  Product Categories",
            .Font = New Font("Segoe UI Semibold", 13),
            .ForeColor = theme.Accent,
            .Location = New Point(30, 180),
            .AutoSize = True
        }

        btnAddCategory = New CyberButton() With {
            .Text = "+ Category",
            .Location = New Point(30, 210),
            .Size = New Size(120, 35),
            .BackColor = Color.FromArgb(80, 80, 80),
            .Font = New Font("Segoe UI Semibold", 9)
        }

        dgvCategories = New DataGridView()
        dgvCategories.Location = New Point(30, 255)
        dgvCategories.Size = New Size(450, 150)
        AppManager.ApplyGridTheme(dgvCategories)
        AddHandler dgvCategories.CellDoubleClick, AddressOf dgvCategories_DoubleClick

        ' ---- Suppliers Section ----
        Dim lblSups As New Label() With {
            .Text = ChrW(&HE77B) & "  Suppliers",
            .Font = New Font("Segoe UI Semibold", 13),
            .ForeColor = theme.Accent,
            .Location = New Point(30, 420),
            .AutoSize = True
        }

        btnAddSupplier = New CyberButton() With {
            .Text = "+ Supplier",
            .Location = New Point(30, 450),
            .Size = New Size(120, 35),
            .BackColor = Color.FromArgb(80, 80, 80),
            .Font = New Font("Segoe UI Semibold", 9)
        }

        dgvSuppliers = New DataGridView()
        dgvSuppliers.Location = New Point(30, 495)
        dgvSuppliers.Size = New Size(700, 150)
        dgvSuppliers.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        AppManager.ApplyGridTheme(dgvSuppliers)
        AddHandler dgvSuppliers.CellDoubleClick, AddressOf dgvSuppliers_DoubleClick

        Me.Controls.AddRange({lblTitle, lblAlerts, lblLow, numLowStock, lblExp, numExpiryDays, btnSaveSettings,
                              lblCats, btnAddCategory, dgvCategories,
                              lblSups, btnAddSupplier, dgvSuppliers})
    End Sub

    Private Sub dgvCategories_DoubleClick(sender As Object, e As DataGridViewCellEventArgs)
        If e.RowIndex < 0 Then Return
        Dim catID = CInt(dgvCategories.Rows(e.RowIndex).Cells("ID").Value)
        Dim currentName = dgvCategories.Rows(e.RowIndex).Cells("Name").Value.ToString()
        Dim currentDesc = dgvCategories.Rows(e.RowIndex).Cells("Description").Value.ToString()

        Dim newName = InputBox("Edit Category Name:", "Edit Category", currentName)
        If String.IsNullOrWhiteSpace(newName) Then Return
        Dim newDesc = InputBox("Edit Description:", "Edit Category", currentDesc)

        Try
            Using db As New PharmacyContext()
                Dim cat = db.Categories.Find(catID)
                cat.CategoryName = newName.Trim()
                cat.Description = newDesc?.Trim()
                db.SaveChanges()
            End Using
            LoadCategories()
        Catch ex As Exception
            MessageBox.Show("Error updating category: " & ex.Message)
        End Try
    End Sub

    Private Sub dgvSuppliers_DoubleClick(sender As Object, e As DataGridViewCellEventArgs)
        If e.RowIndex < 0 Then Return
        Dim supID = CInt(dgvSuppliers.Rows(e.RowIndex).Cells("ID").Value)
        Dim currentComp = dgvSuppliers.Rows(e.RowIndex).Cells("Company").Value.ToString()
        Dim currentTin = dgvSuppliers.Rows(e.RowIndex).Cells("TIN").Value.ToString()
        
        Dim newComp = InputBox("Edit Supplier Company:", "Edit Supplier", currentComp)
        If String.IsNullOrWhiteSpace(newComp) Then Return
        Dim newTin = InputBox("Edit TIN Number:", "Edit Supplier", currentTin)
        If String.IsNullOrWhiteSpace(newTin) Then Return

        Try
            Using db As New PharmacyContext()
                Dim sup = db.Suppliers.Find(supID)
                sup.CompanyName = newComp.Trim()
                sup.TIN_Number = newTin.Trim()
                db.SaveChanges()
            End Using
            LoadSuppliers()
        Catch ex As Exception
            MessageBox.Show("Error updating supplier: " & ex.Message)
        End Try
    End Sub

    Private Sub LoadData()
        ' Load settings
        Try
            Using db As New PharmacyContext()
                Dim lowSetting = db.SystemSettings.FirstOrDefault(Function(s) s.SettingKey = "LowStockThreshold")
                If lowSetting IsNot Nothing Then numLowStock.Value = CDec(lowSetting.SettingValue)
                Dim expSetting = db.SystemSettings.FirstOrDefault(Function(s) s.SettingKey = "ExpiryWarningDays")
                If expSetting IsNot Nothing Then numExpiryDays.Value = CDec(expSetting.SettingValue)
            End Using
        Catch
        End Try

        LoadCategories()
        LoadSuppliers()
    End Sub

    Private Sub LoadCategories()
        Try
            Using db As New PharmacyContext()
                Dim cats = db.Categories.Select(Function(c) New With {.ID = c.CategoryID, .Name = c.CategoryName, .Description = c.Description}).ToList()
                dgvCategories.DataSource = cats
                If dgvCategories.Columns.Count > 0 Then
                    dgvCategories.Columns("ID").Width = 50
                    dgvCategories.Columns("Name").AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                End If
            End Using
        Catch
        End Try
    End Sub

    Private Sub LoadSuppliers()
        Try
            Using db As New PharmacyContext()
                Dim sups = db.Suppliers.Select(Function(s) New With {.ID = s.SupplierID, .Company = s.CompanyName, .TIN = s.TIN_Number, .Phone = s.ContactPhone, .Email = s.Email}).ToList()
                dgvSuppliers.DataSource = sups
                If dgvSuppliers.Columns.Count > 0 Then
                    dgvSuppliers.Columns("ID").Width = 50
                    dgvSuppliers.Columns("Company").AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                End If
            End Using
        Catch
        End Try
    End Sub

    Private Sub btnSaveSettings_Click(sender As Object, e As EventArgs) Handles btnSaveSettings.Click
        Try
            Using db As New PharmacyContext()
                ' Low stock threshold
                Dim lowSetting = db.SystemSettings.FirstOrDefault(Function(s) s.SettingKey = "LowStockThreshold")
                If lowSetting Is Nothing Then
                    db.SystemSettings.Add(New SystemSetting With {.SettingKey = "LowStockThreshold", .SettingValue = numLowStock.Value.ToString(), .Description = "Default low stock warning threshold"})
                Else
                    lowSetting.SettingValue = numLowStock.Value.ToString()
                End If

                ' Expiry warning days
                Dim expSetting = db.SystemSettings.FirstOrDefault(Function(s) s.SettingKey = "ExpiryWarningDays")
                If expSetting Is Nothing Then
                    db.SystemSettings.Add(New SystemSetting With {.SettingKey = "ExpiryWarningDays", .SettingValue = numExpiryDays.Value.ToString(), .Description = "Days before expiry to trigger warning"})
                Else
                    expSetting.SettingValue = numExpiryDays.Value.ToString()
                End If

                db.SaveChanges()
                MessageBox.Show("Settings saved!", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End Using
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnAddCategory_Click(sender As Object, e As EventArgs) Handles btnAddCategory.Click
        Dim name = InputBox("Enter Category Name:", "Add Category")
        If String.IsNullOrWhiteSpace(name) Then Return
        Dim desc = InputBox("Enter Description (optional):", "Add Category")
        Try
            Using db As New PharmacyContext()
                db.Categories.Add(New Category With {.CategoryName = name.Trim(), .Description = desc?.Trim()})
                db.SaveChanges()
            End Using
            LoadCategories()
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnAddSupplier_Click(sender As Object, e As EventArgs) Handles btnAddSupplier.Click
        Dim company = InputBox("Enter Supplier Company Name:", "Add Supplier")
        If String.IsNullOrWhiteSpace(company) Then Return
        Dim tin = InputBox("Enter TIN Number:", "Add Supplier")
        If String.IsNullOrWhiteSpace(tin) Then Return
        Dim phone = InputBox("Phone (optional):", "Add Supplier")
        Dim email = InputBox("Email (optional):", "Add Supplier")
        Try
            Using db As New PharmacyContext()
                db.Suppliers.Add(New Supplier With {
                    .CompanyName = company.Trim(),
                    .TIN_Number = tin.Trim(),
                    .ContactPhone = phone?.Trim(),
                    .Email = email?.Trim()
                })
                db.SaveChanges()
            End Using
            LoadSuppliers()
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
End Class
