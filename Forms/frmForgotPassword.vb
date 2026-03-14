Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.InteropServices
Imports System.Windows.Forms
Imports System.Linq
Imports System.Threading.Tasks
Imports WinFormsApp1.Models
Imports WinFormsApp1.Core

Public Class frmForgotPassword
    Inherits Form

    ' Windows API for borderless dragging
    Public Const WM_NCLBUTTONDOWN As Integer = &HA1
    Public Const HT_CAPTION As Integer = &H2
    <DllImport("user32.dll")> Public Shared Function SendMessage(hWnd As IntPtr, Msg As Integer, wParam As Integer, lParam As Integer) As Integer
    End Function
    <DllImport("user32.dll")> Public Shared Function ReleaseCapture() As Boolean
    End Function

    Private txtUsername As ModernTextBox
    Private txtNewPassword As ModernTextBox
    Private WithEvents btnReset As CyberButton
    Private WithEvents btnBack As Button
    Private WithEvents animTimer As New Timer() With {.Interval = 16}

    Public Sub New()
        Me.SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.UserPaint Or ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw, True)
        Me.DoubleBuffered = True
        InitializeUI()
        Me.Opacity = 0
        animTimer.Start()
    End Sub

    Private Sub InitializeUI()
        Me.FormBorderStyle = FormBorderStyle.None
        Me.Size = New Size(500, 520)
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.BackColor = AppManager.CurrentTheme.Background

        btnBack = New Button() With {
            .Text = ChrW(&HE72B) & " " & AppManager.GetText("BackToLogin"),
            .Size = New Size(150, 30),
            .Location = New Point(20, 20),
            .FlatStyle = FlatStyle.Flat,
            .Cursor = Cursors.Hand,
            .Font = New Font("Segoe MDL2 Assets", 9),
            .ForeColor = AppManager.CurrentTheme.Accent
        }
        btnBack.FlatAppearance.BorderSize = 0

        txtUsername = New ModernTextBox() With {
            .Location = New Point(75, 180),
            .Size = New Size(350, 70),
            .IconChar = ChrW(&HE77B),
            .LabelText = AppManager.GetText("UserLabel"),
            .PlaceholderText = "@username"
        }
        txtUsername.ApplyTheme()

        txtNewPassword = New ModernTextBox() With {
            .Location = New Point(75, 265),
            .Size = New Size(350, 70),
            .IconChar = ChrW(&HE72E),
            .LabelText = "New Password",
            .PlaceholderText = "••••••••••••",
            .UsePasswordChar = True
        }
        txtNewPassword.ApplyTheme()

        btnReset = New CyberButton() With {
            .Text = "Reset Password",
            .Location = New Point(75, 365),
            .Size = New Size(350, 50),
            .BackColor = AppManager.CurrentTheme.Accent
        }

        Me.Controls.AddRange({btnBack, txtUsername, txtNewPassword, btnReset})
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Dim g = e.Graphics
        Dim theme = AppManager.CurrentTheme
        g.SmoothingMode = SmoothingMode.AntiAlias
        g.TextRenderingHint = Drawing.Text.TextRenderingHint.ClearTypeGridFit

        ' Title
        Using titleFont As New Font("Segoe UI", 24, FontStyle.Bold)
            Dim sf As New StringFormat() With {.Alignment = StringAlignment.Center}
            g.DrawString(AppManager.GetText("ForgotTitle"), titleFont, New SolidBrush(theme.PrimaryText), New Rectangle(0, 80, Me.Width, 50), sf)
        End Using

        ' Description
        Using descFont As New Font("Segoe UI", 10)
            Dim sf As New StringFormat() With {.Alignment = StringAlignment.Center}
            g.DrawString(AppManager.GetText("ForgotDesc"), descFont, New SolidBrush(theme.SecondaryText), New Rectangle(50, 130, Me.Width - 100, 50), sf)
        End Using

        ' Border
        Using p As New Pen(theme.Accent, 2)
            g.DrawRectangle(p, 0, 0, Me.Width - 1, Me.Height - 1)
        End Using
    End Sub

    Private Sub btnBack_Click(sender As Object, e As EventArgs) Handles btnBack.Click
        Me.Close()
    End Sub

    Private Async Sub btnReset_Click(sender As Object, e As EventArgs) Handles btnReset.Click
        Dim uname = txtUsername.Text.Trim()
        If uname.StartsWith("@") Then uname = uname.Substring(1)
        uname = uname.ToLower()

        Dim newPass = txtNewPassword.Text

        If String.IsNullOrWhiteSpace(uname) OrElse String.IsNullOrWhiteSpace(newPass) Then
            MessageBox.Show("Please fill all fields.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Try
            Using db As New PharmacyContext()
                Dim user = db.Users.FirstOrDefault(Function(u) u.Username = uname)
                If user Is Nothing Then
                    MessageBox.Show("User not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    Return
                End If

                Dim newSalt = SecurityManager.GenerateSalt()
                Dim newHash = SecurityManager.HashPassword(newPass, newSalt)

                user.Salt = newSalt
                user.PasswordHash = newHash
                Await db.SaveChangesAsync()

                MessageBox.Show("Password successfully reset! You can now log in with your new password.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Me.Close()
            End Using
        Catch ex As Exception
            MessageBox.Show("Error resetting password: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub animTimer_Tick(sender As Object, e As EventArgs) Handles animTimer.Tick
        If Me.Opacity < 1 Then Me.Opacity += 0.1
        Me.Invalidate()
    End Sub

    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        MyBase.OnMouseDown(e)
        ReleaseCapture()
        SendMessage(Me.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0)
    End Sub
End Class
