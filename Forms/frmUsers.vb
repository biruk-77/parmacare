Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports System.Linq
Imports WinFormsApp1.Models
Imports WinFormsApp1.Core
Imports Microsoft.EntityFrameworkCore

Public Class frmUsers
    Inherits Form

    Private WithEvents dgvUsers As DataGridView
    Private WithEvents btnAddUser As CyberButton
    Private lblTitle As Label
    Private lblDesc As Label

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
            .Text = AppManager.GetText("MenuUSERS"),
            .Font = New Font("Segoe UI", 18, FontStyle.Bold),
            .ForeColor = theme.PrimaryText,
            .Location = New Point(40, 30),
            .AutoSize = True
        }

        lblDesc = New Label() With {
            .Text = AppManager.GetText("ManageUsersDesc"),
            .Font = New Font("Segoe UI", 10),
            .ForeColor = theme.SecondaryText,
            .Location = New Point(42, 65),
            .AutoSize = True
        }

        btnAddUser = New CyberButton() With {
            .Text = ChrW(&HE710) & " " & AppManager.GetText("AddUserBtn"),
            .Location = New Point(Me.Width - 250, 40),
            .Size = New Size(200, 40),
            .Anchor = AnchorStyles.Top Or AnchorStyles.Right,
            .BackColor = theme.Accent,
            .Font = New Font("Segoe UI Semibold", 10)
        }

        dgvUsers = New DataGridView() With {
            .Location = New Point(40, 120),
            .Size = New Size(Me.Width - 80, Me.Height - 160),
            .Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        }
        AppManager.ApplyGridTheme(dgvUsers)

        Me.Controls.AddRange({lblTitle, lblDesc, btnAddUser, dgvUsers})
    End Sub

    Private Sub LoadData()
        Try
            Using db As New PharmacyContext()
                Dim users = (From u In db.Users
                             Join r In db.Roles On u.RoleID Equals r.RoleID
                             Select New With {
                                 .ID = u.UserID,
                                 .Name = u.FullName & " (" & u.Username & ")",
                                 .Role = r.RoleName,
                                 .LastLogin = "2 mins ago",
                                 .Status = If(u.IsActive, "Online", "Inactive")
                             }).ToList()

                dgvUsers.DataSource = users
                
                If dgvUsers.Columns.Count > 0 Then
                    dgvUsers.Columns("ID").Visible = False
                    dgvUsers.Columns("Name").AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                    dgvUsers.Columns("Role").Width = 180
                    dgvUsers.Columns("LastLogin").Width = 150
                    dgvUsers.Columns("LastLogin").HeaderText = "Last Login"
                    dgvUsers.Columns("Status").Width = 120
                    
                    If Not dgvUsers.Columns.Contains("Actions") Then
                        Dim btnCol As New DataGridViewButtonColumn()
                        btnCol.Name = "Actions"
                        btnCol.HeaderText = "Edit"
                        btnCol.Text = ChrW(&HE70F) & " Edit"
                        btnCol.UseColumnTextForButtonValue = True
                        btnCol.Width = 80
                        btnCol.FlatStyle = FlatStyle.Flat
                        dgvUsers.Columns.Add(btnCol)

                        Dim btnToggle As New DataGridViewButtonColumn()
                        btnToggle.Name = "Toggle"
                        btnToggle.HeaderText = "Status"
                        btnToggle.Text = ChrW(&HE7BA) & " Toggle"
                        btnToggle.UseColumnTextForButtonValue = True
                        btnToggle.Width = 90
                        btnToggle.FlatStyle = FlatStyle.Flat
                        dgvUsers.Columns.Add(btnToggle)
                    End If
                End If
            End Using
        Catch err As Exception
            MessageBox.Show(err.Message)
        End Try
    End Sub

    Private Sub dgvUsers_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvUsers.CellContentClick
        If e.RowIndex < 0 Then Return
        Dim userID = CInt(dgvUsers.Rows(e.RowIndex).Cells("ID").Value)

        If dgvUsers.Columns(e.ColumnIndex).Name = "Actions" Then
            ' Simple inline edit via InputBox for now
            Try
                Using db As New PharmacyContext()
                    Dim usr = db.Users.FirstOrDefault(Function(u) u.UserID = userID)
                    If usr IsNot Nothing Then
                        Dim newName = InputBox("Edit Full Name:", "Edit User", usr.FullName)
                        If Not String.IsNullOrWhiteSpace(newName) Then
                            usr.FullName = newName.Trim()
                            db.SaveChanges()
                            LoadData()
                        End If
                    End If
                End Using
            Catch ex As Exception
                MessageBox.Show(ex.Message)
            End Try
        ElseIf dgvUsers.Columns(e.ColumnIndex).Name = "Toggle" Then
            Try
                Using db As New PharmacyContext()
                    Dim usr = db.Users.FirstOrDefault(Function(u) u.UserID = userID)
                    If usr IsNot Nothing Then
                        usr.IsActive = Not usr.IsActive
                        db.SaveChanges()
                        LoadData()
                    End If
                End Using
            Catch ex As Exception
                MessageBox.Show(ex.Message)
            End Try
        End If
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        Dim g = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias
        
        ' Draw a subtle border around the grid
        Dim gridRect As New Rectangle(dgvUsers.Left - 1, dgvUsers.Top - 1, dgvUsers.Width + 1, dgvUsers.Height + 1)
        Using p As New Pen(AppManager.CurrentTheme.DividerColor, 1)
            g.DrawRectangle(p, gridRect)
        End Using
    End Sub

    Private Sub btnAddUser_Click(sender As Object, e As EventArgs) Handles btnAddUser.Click
        Dim regForm As New frmRegister()
        regForm.ShowDialog()
        LoadData() ' Refresh map after registration
    End Sub
End Class
