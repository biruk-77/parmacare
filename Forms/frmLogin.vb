Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.Runtime.InteropServices
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports System.Linq
Imports System.IO
Imports SkiaSharp
Imports WinFormsApp1.Core
Imports WinFormsApp1.Models

Public Class frmLogin
    Inherits Form

    ' Windows API for borderless dragging
    Public Const WM_NCLBUTTONDOWN As Integer = &HA1
    Public Const HT_CAPTION As Integer = &H2
    <DllImport("user32.dll")> Public Shared Function SendMessage(hWnd As IntPtr, Msg As Integer, wParam As Integer, lParam As Integer) As Integer
    End Function
    <DllImport("user32.dll")> Public Shared Function ReleaseCapture() As Boolean
    End Function

    ' === CONTROLS ===
    Private txtUsername As ModernTextBox
    Private txtPassword As ModernTextBox
    Private WithEvents btnLogin As CyberButton
    Private WithEvents btnClose As Button
    Private WithEvents btnTheme As Button
    Private WithEvents btnLang As Button
    Private WithEvents chkRemember As CheckBox
    Private WithEvents lblForgot As Label
    Private WithEvents lblRequestAccount As Label
    Private WithEvents lblContactAdmin As Label
    Private WithEvents lblHelpCenter As Label
    Private lblCopyright As Label

    ' === RESOURCES ===
    Private _sidebarImage As Image
    Private _sidebarWidth As Integer = 475

    ' === ANIMATION ===
    Private _isLoading As Boolean = False
    Private _spinnerAngle As Integer = 0
    Private WithEvents animTimer As New Timer() With {.Interval = 16}

    Public Sub New()
        Me.SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.UserPaint Or ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw, True)
        Me.DoubleBuffered = True

        LoadResources()
        InitializeUI()
        ApplyLocalizationAndTheme()

        Me.Opacity = 0
        animTimer.Start()
    End Sub

    ' ─────────────────────────────────────────
    '  RESOURCE LOADING
    ' ─────────────────────────────────────────
    Private Sub LoadResources()
        Try
            Dim imgPath As String = Path.Combine(Application.StartupPath, "image", "image1.png")
            If Not File.Exists(imgPath) Then
                imgPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "image", "image1.png")
            End If
            If File.Exists(imgPath) Then
                ' Try standard load first, fallback to SkiaSharp for WebP
                Try
                    _sidebarImage = Image.FromFile(imgPath)
                Catch
                    ' File is likely WebP — decode with SkiaSharp
                    Dim skBitmap As SKBitmap = SKBitmap.Decode(imgPath)
                    If skBitmap IsNot Nothing Then
                        Dim bmp As New Bitmap(skBitmap.Width, skBitmap.Height, PixelFormat.Format32bppArgb)
                        Dim data = bmp.LockBits(New Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat)
                        Dim srcPtr = skBitmap.GetPixels()
                        Dim byteCount = skBitmap.ByteCount
                        Dim bytes(byteCount - 1) As Byte
                        Marshal.Copy(srcPtr, bytes, 0, byteCount)
                        Marshal.Copy(bytes, 0, data.Scan0, byteCount)
                        bmp.UnlockBits(data)
                        _sidebarImage = bmp
                        skBitmap.Dispose()
                    End If
                End Try
            End If
        Catch
        End Try
    End Sub

    ' ─────────────────────────────────────────
    '  UI INITIALIZATION
    ' ─────────────────────────────────────────
    Private Sub InitializeUI()
        Me.FormBorderStyle = FormBorderStyle.None
        Me.Size = New Size(1050, 680)
        Me.StartPosition = FormStartPosition.CenterScreen

        ' Right panel starts after sidebar
        Dim rx = _sidebarWidth + 50
        Dim rw = Me.Width - _sidebarWidth - 100

        ' Header Controls (Top Right)
        btnClose = MakeIconBtn(ChrW(&HE8BB), Me.Width - 45, 12)
        btnTheme = MakeIconBtn(ChrW(&HE706), Me.Width - 90, 12)
        btnLang = MakeIconBtn(ChrW(&HE12B), Me.Width - 135, 12)

        ' Inputs
        txtUsername = New ModernTextBox() With {
            .Location = New Point(rx, 310),
            .Size = New Size(rw, 70),
            .IconChar = ChrW(&HE77B)
        }
        txtPassword = New ModernTextBox() With {
            .Location = New Point(rx, 395),
            .Size = New Size(rw, 70),
            .UsePasswordChar = True,
            .IconChar = ChrW(&HE72E)
        }

        ' Forgot Password
        lblForgot = MakeLink("", rx + rw - 100, 375, 100, ContentAlignment.MiddleRight)

        ' Remember Me
        chkRemember = New CheckBox() With {
            .Location = New Point(rx, 480),
            .Size = New Size(rw, 20),
            .Font = New Font("Segoe UI", 9),
            .FlatStyle = FlatStyle.Flat
        }

        ' Login Button
        btnLogin = New CyberButton() With {
            .Location = New Point(rx, 520),
            .Size = New Size(rw, 50),
            .Font = New Font("Segoe UI Semibold", 11)
        }

        ' Footer Labels
        lblRequestAccount = MakeLink("", rx + (rw \ 2) - 60, 600, 120, ContentAlignment.MiddleCenter)
        lblContactAdmin = MakeLink("", rx + (rw \ 2) - 120, 630, 120, ContentAlignment.MiddleCenter)
        lblHelpCenter = MakeLink("", rx + (rw \ 2) + 10, 630, 120, ContentAlignment.MiddleCenter)
        lblCopyright = MakeLink("", rx, 655, rw, ContentAlignment.MiddleCenter)
        lblCopyright.Font = New Font("Segoe UI", 7)

        Me.Controls.AddRange({btnClose, btnTheme, btnLang, txtUsername, txtPassword, lblForgot,
                              chkRemember, btnLogin, lblRequestAccount,
                              lblContactAdmin, lblHelpCenter, lblCopyright})
    End Sub

    ' ─────────────────────────────────────────
    '  LOCALIZATION + THEME
    ' ─────────────────────────────────────────
    Private Sub ApplyLocalizationAndTheme()
        If txtUsername Is Nothing Then Exit Sub
        Dim theme = AppManager.CurrentTheme

        Me.BackColor = theme.Background

        ' Input Labels & Placeholders
        txtUsername.LabelText = AppManager.GetText("UserLabel")
        txtUsername.PlaceholderText = AppManager.GetText("UserPlaceholder")
        txtPassword.LabelText = AppManager.GetText("PassLabel")
        txtPassword.PlaceholderText = "••••••••••••"
        txtUsername.ApplyTheme()
        txtPassword.ApplyTheme()

        ' Button
        btnLogin.Text = AppManager.GetText("LoginBtn")
        btnLogin.BackColor = theme.Accent
        btnLogin.ForeColor = Color.FromArgb(16, 34, 29)

        ' Header Buttons
        btnTheme.Text = If(AppManager.IsDarkMode, ChrW(&HE706), ChrW(&HE708))
        For Each btn As Button In {btnClose, btnTheme, btnLang}
            btn.BackColor = Color.FromArgb(30, theme.Accent)
            btn.ForeColor = theme.PrimaryText
        Next

        ' Checkbox
        chkRemember.Text = AppManager.GetText("RememberMe")
        chkRemember.ForeColor = theme.SecondaryText
        chkRemember.BackColor = theme.Background

        ' Links
        lblForgot.Text = AppManager.GetText("ForgotPass")
        lblForgot.ForeColor = theme.Accent
        lblRequestAccount.Text = AppManager.GetText("RequestAccount")
        lblRequestAccount.ForeColor = theme.Accent
        
        lblContactAdmin.Font = New Font("Segoe MDL2 Assets", 8.5F)
        lblContactAdmin.Text = ChrW(&HE715) & " " & AppManager.GetText("ContactAdmin")
        lblContactAdmin.ForeColor = theme.SecondaryText
        
        lblHelpCenter.Font = New Font("Segoe MDL2 Assets", 8.5F)
        lblHelpCenter.Text = ChrW(&HE11B) & " " & AppManager.GetText("HelpCenter")
        lblHelpCenter.ForeColor = theme.SecondaryText
        lblCopyright.Text = AppManager.GetText("Copyright")
        lblCopyright.ForeColor = theme.SecondaryText

        Me.Invalidate()
    End Sub

    ' ─────────────────────────────────────────
    '  CUSTOM PAINTING
    ' ─────────────────────────────────────────
    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Dim g = e.Graphics
        Dim theme = AppManager.CurrentTheme
        g.SmoothingMode = SmoothingMode.AntiAlias
        g.TextRenderingHint = Drawing.Text.TextRenderingHint.ClearTypeGridFit

        ' ── LEFT SIDEBAR ──
        Dim sidebarRect As New Rectangle(0, 0, _sidebarWidth, Me.Height)

        ' Background Image (Cover)
        If _sidebarImage IsNot Nothing Then
            Dim ratio = Math.Max(CSng(_sidebarWidth) / _sidebarImage.Width, CSng(Me.Height) / _sidebarImage.Height)
            Dim w = CInt(_sidebarImage.Width * ratio)
            Dim h = CInt(_sidebarImage.Height * ratio)
            Dim x = (_sidebarWidth - w) \ 2
            Dim y = (Me.Height - h) \ 2
            g.SetClip(sidebarRect)
            g.DrawImage(_sidebarImage, New Rectangle(x, y, w, h))
            g.ResetClip()
        Else
            Using fallback As New SolidBrush(Color.FromArgb(30, theme.Accent))
                g.FillRectangle(fallback, sidebarRect)
            End Using
        End If

        ' Gradient Overlay (Balanced — image visible, text readable)
        Using overlay As New LinearGradientBrush(sidebarRect,
            Color.FromArgb(If(AppManager.IsDarkMode, 140, 100), theme.Background),
            Color.FromArgb(If(AppManager.IsDarkMode, 180, 140), theme.Background), 90.0F)
            g.FillRectangle(overlay, sidebarRect)
        End Using

        ' Decorative Floating Circles (adds complexity)
        Using c1 As New SolidBrush(Color.FromArgb(20, theme.Accent))
            g.FillEllipse(c1, -40, -40, 180, 180)
            g.FillEllipse(c1, _sidebarWidth - 100, Me.Height - 150, 200, 200)
            g.FillEllipse(c1, 60, Me.Height - 80, 120, 120)
        End Using
        Using c2 As New SolidBrush(Color.FromArgb(12, theme.Accent))
            g.FillEllipse(c2, _sidebarWidth - 60, 80, 100, 100)
            g.FillEllipse(c2, 20, 400, 80, 80)
        End Using

        ' Accent glow line (top of sidebar)
        Using glowPen As New Pen(Color.FromArgb(100, theme.Accent), 3)
            g.DrawLine(glowPen, 0, 0, _sidebarWidth, 0)
        End Using

        ' Branding Icon (Simplified Vector Pill)
        Dim iconSize = 64
        Dim iconX = (_sidebarWidth - iconSize) \ 2
        Dim iconY = 140
        Dim iconRect As New Rectangle(iconX, iconY, iconSize, iconSize)
        Using br As New SolidBrush(theme.Accent)
            ' Draw a pill shape using two circles and a rectangle
            Dim d = iconSize \ 2
            g.FillEllipse(br, iconRect.X, iconRect.Y, d, d) ' Left half
            g.FillEllipse(br, iconRect.X + d, iconRect.Y + d, d, d) ' Right half
            ' Connect them to look like a capsule/pill
            g.FillRectangle(br, iconRect.X + (d \ 2), iconRect.Y, d, d)
            g.FillRectangle(br, iconRect.X + d, iconRect.Y + (d \ 2), d, d)
        End Using
        ' Add a small "plus" in the middle of the pill
        Using plusPen As New Pen(Color.FromArgb(16, 34, 29), 4)
            Dim cx = iconRect.X + (iconSize \ 2)
            Dim cy = iconRect.Y + (iconSize \ 2)
            g.DrawLine(plusPen, cx - 8, cy, cx + 8, cy)
            g.DrawLine(plusPen, cx, cy - 8, cx, cy + 8)
        End Using

        ' Title Container (Flutter-style)
        Dim titleContainerRect As New Rectangle(30, 220, _sidebarWidth - 60, 90)
        Using titlePath As New GraphicsPath()
            Dim r = 15
            titlePath.AddArc(titleContainerRect.X, titleContainerRect.Y, r, r, 180, 90)
            titlePath.AddArc(titleContainerRect.Right - r, titleContainerRect.Y, r, r, 270, 90)
            titlePath.AddArc(titleContainerRect.Right - r, titleContainerRect.Bottom - r, r, r, 0, 90)
            titlePath.AddArc(titleContainerRect.X, titleContainerRect.Bottom - r, r, r, 90, 90)
            titlePath.CloseFigure()
            Using br As New SolidBrush(Color.FromArgb(160, Color.White))
                g.FillPath(br, titlePath)
            End Using
            Using p As New Pen(Color.FromArgb(40, theme.Accent), 1)
                g.DrawPath(p, titlePath)
            End Using
        End Using

        ' Title Text
        Dim sf As New StringFormat() With {.Alignment = StringAlignment.Center, .LineAlignment = StringAlignment.Center}
        Using titleFont As New Font("Segoe UI", 20, FontStyle.Bold)
            g.DrawString(AppManager.GetText("SidebarTitle"), titleFont, New SolidBrush(Color.FromArgb(16, 34, 29)), titleContainerRect, sf)
        End Using

        ' Accent Separator
        Using accentPen As New Pen(theme.Accent, 3)
            Dim lineY = 330
            g.DrawLine(accentPen, (_sidebarWidth \ 2) - 30, lineY, (_sidebarWidth \ 2) + 30, lineY)
        End Using

        ' Description Container (Flutter-style)
        Dim containerRect As New Rectangle(30, 340, _sidebarWidth - 60, 100)
        Dim path As New GraphicsPath()
        Dim radius = 15
        path.AddArc(containerRect.X, containerRect.Y, radius, radius, 180, 90)
        path.AddArc(containerRect.Right - radius, containerRect.Y, radius, radius, 270, 90)
        path.AddArc(containerRect.Right - radius, containerRect.Bottom - radius, radius, radius, 0, 90)
        path.AddArc(containerRect.X, containerRect.Bottom - radius, radius, radius, 90, 90)
        path.CloseFigure()

        ' Fill container with "opposite" color (Light / High Contrast)
        ' Since sidebar is dark, we use a very light version of the accent or white
        Using br As New SolidBrush(Color.FromArgb(180, Color.White))
            g.FillPath(br, path)
        End Using
        ' Optional border
        Using p As New Pen(Color.FromArgb(50, theme.Accent), 1)
            g.DrawPath(p, path)
        End Using

        ' Description Text - DARK color for contrast in light container
        Using descFont As New Font("Segoe UI Semibold", 9.5F)
            Dim textPadding As New Rectangle(containerRect.X + 15, containerRect.Y + 15, containerRect.Width - 30, containerRect.Height - 30)
            g.DrawString(AppManager.GetText("SidebarDesc"), descFont, New SolidBrush(Color.FromArgb(16, 34, 29)), textPadding, sf)
        End Using

        ' Badges (Removed)
        

        ' Bottom accent line
        Using bottomPen As New Pen(theme.Accent, 2)
            g.DrawLine(bottomPen, 40, Me.Height - 40, _sidebarWidth - 40, Me.Height - 40)
        End Using
        Using verFont As New Font("Segoe UI", 7)
            g.DrawString("v2.5.0 — Enterprise Edition", verFont, New SolidBrush(Color.FromArgb(120, theme.SecondaryText)), 40, Me.Height - 30)
        End Using

        ' ── RIGHT PANEL ──
        Dim rx = _sidebarWidth + 50

        ' Right Panel Brand Header (Vector Pill + "PharmaCare")
        Dim brandIconRect As New Rectangle(rx, 40, 32, 32)
        Using br As New SolidBrush(theme.Accent)
            g.FillEllipse(br, brandIconRect.X, brandIconRect.Y, 16, 16)
            g.FillEllipse(br, brandIconRect.X + 16, brandIconRect.Y + 16, 16, 16)
            g.FillRectangle(br, brandIconRect.X + 8, brandIconRect.Y, 16, 16)
            g.FillRectangle(br, brandIconRect.X + 16, brandIconRect.Y + 8, 16, 16)
        End Using
        Using brandFont As New Font("Segoe UI", 14, FontStyle.Bold)
            g.DrawString(AppManager.GetText("Brand"), brandFont, New SolidBrush(theme.PrimaryText), rx + 40, 44)
        End Using

        ' "Admin Portal Login" Title
        Using h2Font As New Font("Segoe UI", 22, FontStyle.Bold)
            g.DrawString(AppManager.GetText("AdminLogin"), h2Font, New SolidBrush(theme.PrimaryText), rx, 230)
        End Using

        ' Description
        Using descFont As New Font("Segoe UI", 9.5F)
            Dim descRect As New Rectangle(rx, 270, Me.Width - _sidebarWidth - 100, 40)
            g.DrawString(AppManager.GetText("AdminDesc"), descFont, New SolidBrush(theme.SecondaryText), descRect)
        End Using

        ' Divider above footer
        Using divPen As New Pen(theme.DividerColor, 1)
            g.DrawLine(divPen, rx, 585, rx + (Me.Width - _sidebarWidth - 100), 585)
        End Using

        ' Loading Spinner
        If _isLoading Then
            Using pSpin As New Pen(theme.Accent, 3)
                pSpin.DashStyle = DashStyle.Dot
                Dim spX = rx + (Me.Width - _sidebarWidth - 100 - 40) \ 2
                g.DrawArc(pSpin, New Rectangle(spX, 570, 30, 30), _spinnerAngle, 240)
            End Using
        End If
    End Sub

    Private Sub DrawBadge(g As Graphics, theme As AppManager.Theme, text As String, x As Integer, y As Integer, w As Integer)
        Dim badgeRect As New Rectangle(x, y, w, 36)
        Dim badgePath As New GraphicsPath()
        badgePath.AddArc(badgeRect.X, badgeRect.Y, 36, 36, 180, 90)
        badgePath.AddArc(badgeRect.Right - 36, badgeRect.Y, 36, 36, 270, 90)
        badgePath.AddArc(badgeRect.Right - 36, badgeRect.Bottom - 36, 36, 36, 0, 90)
        badgePath.AddArc(badgeRect.X, badgeRect.Bottom - 36, 36, 36, 90, 90)
        badgePath.CloseFigure()

        Using br As New SolidBrush(Color.FromArgb(If(AppManager.IsDarkMode, 15, 50), theme.Accent))
            g.FillPath(br, badgePath)
        End Using
        Using p As New Pen(Color.FromArgb(50, theme.Accent), 1)
            g.DrawPath(p, badgePath)
        End Using
        Using f As New Font("Segoe UI", 7.5F, FontStyle.Bold)
            Dim sf As New StringFormat() With {.Alignment = StringAlignment.Center, .LineAlignment = StringAlignment.Center}
            g.DrawString(text, f, New SolidBrush(theme.PrimaryText), badgeRect, sf)
        End Using
    End Sub

    ' ─────────────────────────────────────────
    '  LOGIN LOGIC
    ' ─────────────────────────────────────────
    Private Async Sub btnLogin_Click(sender As Object, e As EventArgs) Handles btnLogin.Click
        If _isLoading Then Return
        _isLoading = True
        btnLogin.Text = AppManager.GetText("Authenticating")

        Try
            Await Task.Delay(1200)
            Dim username = txtUsername.Text.Trim().ToLower()
            Dim password = txtPassword.Text

            Dim isValid = Await Task.Run(Function()
                Using db As New PharmacyContext()
                    Dim user = db.Users.FirstOrDefault(Function(u) u.Username = username)
                    If user IsNot Nothing AndAlso SecurityManager.VerifyPassword(password, user.PasswordHash, user.Salt) Then
                        Dim roleName = "Unknown"
                        Dim role = db.Roles.FirstOrDefault(Function(r) r.RoleID = user.RoleID)
                        If role IsNot Nothing Then roleName = role.RoleName
                        
                        ' Initialize Global Session
                        SessionManager.CurrentUser = New UserSessionContext(user.UserID, user.Username, user.FullName, user.RoleID, roleName)
                        Return True
                    End If
                    Return False
                End Using
            End Function)

            If isValid Then
                While Me.Opacity > 0
                    Me.Opacity -= 0.1
                    Await Task.Delay(15)
                End While
                Dim mainForm As New frmMain()
                mainForm.Show()
                Me.Hide()
            Else
                Throw New Exception("Access Denied: Invalid Credentials")
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message, "SECURITY", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            _isLoading = False
            btnLogin.Text = AppManager.GetText("LoginBtn")
        End Try
    End Sub

    ' ─────────────────────────────────────────
    '  EVENT HANDLERS
    ' ─────────────────────────────────────────
    Private Sub animTimer_Tick(sender As Object, e As EventArgs) Handles animTimer.Tick
        If Me.Opacity < 1 Then Me.Opacity += 0.08
        If _isLoading Then _spinnerAngle = (_spinnerAngle + 12) Mod 360
        Me.Invalidate()
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Application.Exit()
    End Sub

    Private Sub btnTheme_Click(sender As Object, e As EventArgs) Handles btnTheme.Click
        AppManager.IsDarkMode = Not AppManager.IsDarkMode
        ApplyLocalizationAndTheme()
    End Sub

    Private Sub btnLang_Click(sender As Object, e As EventArgs) Handles btnLang.Click
        ' Cycle: EN -> AM -> OM -> EN
        Select Case AppManager.CurrentLang
            Case "EN" : AppManager.CurrentLang = "AM"
            Case "AM" : AppManager.CurrentLang = "OM"
            Case Else : AppManager.CurrentLang = "EN"
        End Select
        ApplyLocalizationAndTheme()
    End Sub

    Private Sub lblForgot_Click(sender As Object, e As EventArgs) Handles lblForgot.Click
        Dim f As New frmForgotPassword()
        f.ShowDialog()
    End Sub

    Private Sub lblRequestAccount_Click(sender As Object, e As EventArgs) Handles lblRequestAccount.Click
        Dim f As New frmRegister()
        f.TopLevel = False
        f.Dock = DockStyle.Fill
        Me.Controls.Add(f)
        f.Show()
        f.BringToFront()
    End Sub

    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        MyBase.OnMouseDown(e)
        ReleaseCapture()
        SendMessage(Me.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0)
    End Sub

    ' ─────────────────────────────────────────
    '  HELPERS
    ' ─────────────────────────────────────────
    Private Function MakeIconBtn(text As String, x As Integer, y As Integer) As Button
        Dim btn As New Button() With {
            .Text = text,
            .Size = New Size(36, 36),
            .Location = New Point(x, y),
            .FlatStyle = FlatStyle.Flat,
            .Cursor = Cursors.Hand,
            .Font = New Font("Segoe MDL2 Assets", 10)
        }
        btn.FlatAppearance.BorderSize = 0
        Return btn
    End Function

    Private Function MakeLink(text As String, x As Integer, y As Integer, w As Integer, align As ContentAlignment) As Label
        Return New Label() With {
            .Text = text,
            .Location = New Point(x, y),
            .Size = New Size(w, 20),
            .Font = New Font("Segoe UI Semibold", 8.5F),
            .TextAlign = align,
            .Cursor = Cursors.Hand,
            .BackColor = Color.Transparent
        }
    End Function
End Class
