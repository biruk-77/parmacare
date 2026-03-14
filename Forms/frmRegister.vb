Imports WinFormsApp1.Models
Imports WinFormsApp1.Core
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.InteropServices
Imports System.Windows.Forms
Imports System.Linq
Imports System.IO
Public Class frmRegister
    Inherits Form

    ' Windows API for borderless dragging
    Public Const WM_NCLBUTTONDOWN As Integer = &HA1
    Public Const HT_CAPTION As Integer = &H2
    <DllImport("user32.dll")> Public Shared Function SendMessage(hWnd As IntPtr, Msg As Integer, wParam As Integer, lParam As Integer) As Integer
    End Function
    <DllImport("user32.dll")> Public Shared Function ReleaseCapture() As Boolean
    End Function

    ' === CONTROLS ===
    Private txtFullName As ModernTextBox
    Private txtUsername As ModernTextBox
    Private txtPassword As ModernTextBox
    Private cmbRole As ComboBox
    Private WithEvents btnRegister As CyberButton
    Private WithEvents btnClose As Button
    Private lblBackToLogin As Label
    Private lblRoleLabel As Label

    Private _sidebarWidth As Integer = 400
    Private _sidebarImage As Image

    Public Sub New()
        Me.SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.UserPaint Or ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw, True)
        Me.DoubleBuffered = True
        
        LoadResources()
        InitializeUI()
        ApplyLocalizationAndTheme()
    End Sub

    Private Sub LoadResources()
        Try
            Dim imgPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "image", "image1.png")
            If File.Exists(imgPath) Then
                _sidebarImage = Image.FromFile(imgPath)
            End If
        Catch
        End Try
    End Sub

    Private Sub InitializeUI()
        Me.FormBorderStyle = FormBorderStyle.None
        Me.Size = New Size(1050, 680)
        Me.StartPosition = FormStartPosition.CenterScreen

        ' Calculate exactly like frmLogin
        Dim rx = _sidebarWidth + 50
        Dim rw = Me.Width - _sidebarWidth - 100

        ' Header
        btnClose = New Button() With {
            .Text = ChrW(&HE8BB),
            .Size = New Size(36, 36),
            .Location = New Point(Me.Width - 45, 12),
            .FlatStyle = FlatStyle.Flat,
            .Font = New Font("Segoe MDL2 Assets", 10),
            .Cursor = Cursors.Hand
        }
        btnClose.FlatAppearance.BorderSize = 0

        ' Inputs
        txtFullName = New ModernTextBox() With {
            .Location = New Point(rx, 180),
            .Size = New Size(rw, 70),
            .IconChar = ChrW(&HE77B)
        }
        txtUsername = New ModernTextBox() With {
            .Location = New Point(rx, 265),
            .Size = New Size(rw, 70),
            .IconChar = ChrW(&HE77B)
        }
        txtPassword = New ModernTextBox() With {
            .Location = New Point(rx, 350),
            .Size = New Size(rw, 70),
            .UsePasswordChar = True,
            .IconChar = ChrW(&HE72E)
        }

        ' Role ComboBox
        lblRoleLabel = New Label() With {
            .Location = New Point(rx, 435),
            .Size = New Size(rw, 20),
            .Font = New Font("Segoe UI Semibold", 9)
        }
        cmbRole = New ComboBox() With {
            .Location = New Point(rx, 460),
            .Size = New Size(rw, 40),
            .DropDownStyle = ComboBoxStyle.DropDownList,
            .Font = New Font("Segoe UI", 11),
            .FlatStyle = FlatStyle.Flat
        }

        ' Register Button
        btnRegister = New CyberButton() With {
            .Location = New Point(rx, 530),
            .Size = New Size(rw, 50),
            .Font = New Font("Segoe UI Semibold", 11)
        }

        ' Back Link
        lblBackToLogin = New Label() With {
            .Location = New Point(rx, 600),
            .Size = New Size(rw, 20),
            .TextAlign = ContentAlignment.MiddleCenter,
            .Cursor = Cursors.Hand,
            .Font = New Font("Segoe UI Semibold", 9)
        }
        AddHandler lblBackToLogin.Click, Sub()
            If Me.TopLevel Then
                Me.Close()
            Else
                Me.Dispose() ' Close embedded form to reveal login
            End If
        End Sub

        Me.Controls.AddRange({btnClose, txtFullName, txtUsername, txtPassword, lblRoleLabel, cmbRole, btnRegister, lblBackToLogin})
    End Sub

    Private Sub ApplyLocalizationAndTheme()
        Dim theme = AppManager.CurrentTheme
        Me.BackColor = theme.Background

        txtFullName.LabelText = AppManager.GetText("FullNameLabel")
        txtFullName.PlaceholderText = "E.g. John Doe"
        txtUsername.LabelText = AppManager.GetText("UserLabel")
        txtUsername.PlaceholderText = "username"
        txtPassword.LabelText = AppManager.GetText("PassLabel")
        txtPassword.PlaceholderText = "••••••••••••"
        
        txtFullName.ApplyTheme()
        txtUsername.ApplyTheme()
        txtPassword.ApplyTheme()

        lblRoleLabel.Text = AppManager.GetText("RoleLabel")
        lblRoleLabel.ForeColor = theme.SecondaryText

        cmbRole.BackColor = theme.InputBackground
        cmbRole.ForeColor = theme.PrimaryText
        cmbRole.Items.Clear()
        cmbRole.Items.Add(AppManager.GetText("AdminRole"))
        cmbRole.Items.Add(AppManager.GetText("PharmacistRole"))
        cmbRole.Items.Add(AppManager.GetText("ClerkRole"))
        cmbRole.SelectedIndex = 1

        btnRegister.Text = AppManager.GetText("RegisterBtn")
        btnRegister.BackColor = theme.Accent
        btnRegister.ForeColor = Color.FromArgb(16, 34, 29)

        lblBackToLogin.Text = AppManager.GetText("BackToLogin")
        lblBackToLogin.ForeColor = theme.Accent

        btnClose.ForeColor = theme.PrimaryText
        btnClose.BackColor = Color.FromArgb(30, theme.Accent)
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Dim g = e.Graphics
        Dim theme = AppManager.CurrentTheme
        g.SmoothingMode = SmoothingMode.AntiAlias

        ' Sidebar
        Dim sidebarRect As New Rectangle(0, 0, _sidebarWidth, Me.Height)
        If _sidebarImage IsNot Nothing Then
            g.DrawImage(_sidebarImage, sidebarRect)
        End If
        Using br As New SolidBrush(Color.FromArgb(180, theme.Background))
            g.FillRectangle(br, sidebarRect)
        End Using

        ' Title
        Using h1Font As New Font("Segoe UI", 24, FontStyle.Bold)
            g.DrawString(AppManager.GetText("RegisterTitle"), h1Font, New SolidBrush(theme.PrimaryText), _sidebarWidth + 50, 70)
        End Using
        Using pFont As New Font("Segoe UI", 10)
            g.DrawString(AppManager.GetText("RegisterDesc"), pFont, New SolidBrush(theme.SecondaryText), _sidebarWidth + 50, 120)
        End Using

        ' Accent line
        Using p As New Pen(theme.Accent, 3)
            g.DrawLine(p, _sidebarWidth + 50, 120, _sidebarWidth + 100, 120)
        End Using
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        If Me.TopLevel Then
            Me.Close()
        Else
            Me.Dispose() ' Close embedded form to reveal login
        End If
    End Sub

    Private Async Sub btnRegister_Click(sender As Object, e As EventArgs) Handles btnRegister.Click
        ' Simple validation
        If String.IsNullOrWhiteSpace(txtFullName.Text) OrElse String.IsNullOrWhiteSpace(txtUsername.Text) Then
            MessageBox.Show("Please fill all fields", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Try
            Dim roleName = cmbRole.SelectedItem.ToString()
            ' Map visible name back to system name if necessary or use as is
            
            Using db As New PharmacyContext()
                If db.Users.Any(Function(u) u.Username.ToLower() = txtUsername.Text.Trim().ToLower()) Then
                    MessageBox.Show("Username already exists", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    Return
                End If

                Dim role = db.Roles.FirstOrDefault(Function(r) r.RoleName.Contains(roleName) Or roleName.Contains(r.RoleName))
                If role Is Nothing Then role = db.Roles.First()

                Dim salt = SecurityManager.GenerateSalt()
                Dim hash = SecurityManager.HashPassword(txtPassword.Text, salt)

                Dim newUser As New User With {
                    .FullName = txtFullName.Text,
                    .Username = txtUsername.Text.Trim().ToLower(),
                    .PasswordHash = hash,
                    .Salt = salt,
                    .RoleID = role.RoleID,
                    .IsActive = True,
                    .CreatedAt = DateTime.Now
                }

                db.Users.Add(newUser)
                Await db.SaveChangesAsync()

                MessageBox.Show(AppManager.GetText("RegSuccess"), "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
                If Me.TopLevel Then
                    Me.Close()
                Else
                    Me.Dispose() ' Transition smoothly back to Login
                End If
            End Using
        Catch ex As Exception
            MessageBox.Show("Registration failed: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        MyBase.OnMouseDown(e)
        ReleaseCapture()
        If Me.TopLevel Then
            SendMessage(Me.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0)
        ElseIf Me.ParentForm IsNot Nothing Then
            SendMessage(Me.ParentForm.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0)
        End If
    End Sub
End Class
