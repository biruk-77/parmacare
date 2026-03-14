Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.InteropServices
Imports System.Windows.Forms
Imports WinFormsApp1.Core
Imports WinFormsApp1.Models

Public Class frmMain
    Inherits Form

    ' Windows API for borderless dragging
    Public Const WM_NCLBUTTONDOWN As Integer = &HA1
    Public Const HT_CAPTION As Integer = &H2
    <DllImport("user32.dll")> Public Shared Function SendMessage(hWnd As IntPtr, Msg As Integer, wParam As Integer, lParam As Integer) As Integer
    End Function
    <DllImport("user32.dll")> Public Shared Function ReleaseCapture() As Boolean
    End Function

    Private _sidebarWidth As Integer = 250
    Private _headerHeight As Integer = 50

    Private WithEvents btnClose As Button
    Private WithEvents btnTheme As Button
    Private WithEvents btnLang As Button

    Private CustomContentPanel As Panel
    Private _sidebarButtons As New List(Of DashboardButton)
    Private _activeScreen As String = "OVERVIEW"

    Private WithEvents animTimer As New Timer() With {.Interval = 16}

    Public Sub New()
        Me.SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.UserPaint Or ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw, True)
        Me.DoubleBuffered = True
        
        InitializeUI()
        ApplyLocalizationAndTheme()
        animTimer.Start()
        
        LoadScreen("OVERVIEW")
    End Sub

    Private Sub InitializeUI()
        Me.FormBorderStyle = FormBorderStyle.None
        Me.Size = New Size(1280, 800)
        Me.StartPosition = FormStartPosition.CenterScreen

        ' Header Controls (Top Right)
        btnClose = MakeIconBtn(ChrW(&HE8BB), Me.Width - 45, 12)
        btnTheme = MakeIconBtn(ChrW(&HE706), Me.Width - 90, 12)
        btnLang = MakeIconBtn(ChrW(&HE12B), Me.Width - 135, 12)
        
        Me.Controls.AddRange({btnClose, btnTheme, btnLang})

        ' Content Panel
        CustomContentPanel = New Panel() With {
            .Location = New Point(_sidebarWidth, _headerHeight),
            .Size = New Size(Me.Width - _sidebarWidth, Me.Height - _headerHeight),
            .BackColor = Color.Transparent
        }
        Me.Controls.Add(CustomContentPanel)

        ' Setup Sidebar Buttons
        SetupSidebar()
    End Sub

    Private Sub SetupSidebar()
        _sidebarButtons.Clear()
        Dim roleName = "admin"
        If SessionManager.CurrentUser IsNot Nothing Then
            roleName = SessionManager.CurrentUser.RoleName.ToLower()
        End If

        Dim yPos As Integer = 120
        _sidebarButtons.Add(New DashboardButton("OVERVIEW", "OVERVIEW", ChrW(&HE80F), yPos)) : yPos += 50
        
        If roleName.Contains("admin") Then
            _sidebarButtons.Add(New DashboardButton("USERS", "USERS", ChrW(&HE716), yPos)) : yPos += 50
        End If
        
        _sidebarButtons.Add(New DashboardButton("INVENTORY", "INVENTORY", ChrW(&HE81A), yPos)) : yPos += 50
        _sidebarButtons.Add(New DashboardButton("PURCHASES", "PURCHASES", ChrW(&HE7BF), yPos)) : yPos += 50
        
        If roleName.Contains("admin") OrElse roleName.Contains("pharmacist") OrElse roleName.Contains("clerk") OrElse roleName.Contains("cashier") Then
            _sidebarButtons.Add(New DashboardButton("POS", "POS", ChrW(&HE8A1), yPos)) : yPos += 50
        End If

        If roleName.Contains("admin") OrElse roleName.Contains("pharmacist") Then
            _sidebarButtons.Add(New DashboardButton("REPORTS", "REPORTS", ChrW(&HE9D2), yPos)) : yPos += 50
        End If
        If roleName.Contains("admin") Then
            _sidebarButtons.Add(New DashboardButton("SETTINGS", "SETTINGS", ChrW(&HE713), yPos)) : yPos += 50
        End If

        ' Everyone gets Logout at the bottom (spaced out)
        yPos = Me.Height - 145 ' Position it just above the profile card
        _sidebarButtons.Add(New DashboardButton("LOGOUT", "LOGOUT", ChrW(&HE811), yPos))
    End Sub

    Private Sub ApplyLocalizationAndTheme()
        Dim theme = AppManager.CurrentTheme
        Me.BackColor = theme.Background

        btnTheme.Text = If(AppManager.IsDarkMode, ChrW(&HE706), ChrW(&HE708))
        For Each btn As Button In {btnClose, btnTheme, btnLang}
            btn.BackColor = Color.FromArgb(30, theme.Accent)
            btn.ForeColor = theme.PrimaryText
        Next

        For Each btn In _sidebarButtons
            btn.Label = AppManager.GetText("Menu" & btn.ID)
            If btn.Label = "Menu" & btn.ID Then btn.Label = btn.ID ' Fallback
        Next

        ' Notify children
        For Each ctrl As Control In CustomContentPanel.Controls
            If TypeOf ctrl Is Form Then
                Dim f = DirectCast(ctrl, Form)
                f.BackColor = theme.Background
                f.Invalidate(True)
            End If
        Next

        Me.Invalidate()
    End Sub

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

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Dim g = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias
        g.TextRenderingHint = Drawing.Text.TextRenderingHint.ClearTypeGridFit
        Dim theme = AppManager.CurrentTheme

        ' Header Background
        g.FillRectangle(New SolidBrush(theme.Surface), 0, 0, Me.Width, _headerHeight)
        
        ' Sidebar Background (matching visual styling from login/design)
        Dim sidebarRect As New Rectangle(0, 0, _sidebarWidth, Me.Height)
        Using overlay As New LinearGradientBrush(sidebarRect,
            Color.FromArgb(If(AppManager.IsDarkMode, 140, 100), theme.Surface),
            Color.FromArgb(If(AppManager.IsDarkMode, 180, 140), theme.Surface), 90.0F)
            g.FillRectangle(overlay, sidebarRect)
        End Using

        ' Brand Logo
        Dim iconSize = 40
        Dim iconX = 20
        Dim iconY = 20
        Dim iconRect As New Rectangle(iconX, iconY, iconSize, iconSize)
        Using br As New SolidBrush(theme.Accent)
            Dim d = iconSize \ 2
            g.FillEllipse(br, iconRect.X, iconRect.Y, d, d) 
            g.FillEllipse(br, iconRect.X + d, iconRect.Y + d, d, d) 
            g.FillRectangle(br, iconRect.X + (d \ 2), iconRect.Y, d, d)
            g.FillRectangle(br, iconRect.X + d, iconRect.Y + (d \ 2), d, d)
        End Using
        Using brandFont As New Font("Segoe UI", 14, FontStyle.Bold)
            g.DrawString(AppManager.GetText("Brand"), brandFont, New SolidBrush(theme.PrimaryText), iconX + 50, iconY + 5)
        End Using

        ' ---- Header: Welcome Greeting ----
        Dim greetUser = If(SessionManager.CurrentUser?.FullName, "Operator")
        Dim greetRole = If(SessionManager.CurrentUser?.RoleName, "Admin")
        Using gFont As New Font("Segoe UI", 10)
            g.DrawString("Welcome, " & greetUser, gFont, New SolidBrush(theme.SecondaryText), _sidebarWidth + 15, 15)
        End Using

        ' ---- User Profile Card in Sidebar (bottom) ----
        Dim profileY = Me.Height - 90
        ' Avatar circle
        Dim avatarRect As New Rectangle(20, profileY, 50, 50)
        g.FillEllipse(New SolidBrush(theme.Accent), avatarRect)
        ' Initials
        Dim initials = ""
        If greetUser.Length > 0 Then
            Dim parts = greetUser.Split(" "c)
            For Each part In parts
                If part.Length > 0 Then initials &= part(0).ToString().ToUpper()
                If initials.Length >= 2 Then Exit For
            Next
        End If
        If initials.Length = 0 Then initials = "?"
        Using iFont As New Font("Segoe UI", 16, FontStyle.Bold)
            Dim sz = g.MeasureString(initials, iFont)
            g.DrawString(initials, iFont, New SolidBrush(Color.White), avatarRect.X + (avatarRect.Width - sz.Width) / 2, avatarRect.Y + (avatarRect.Height - sz.Height) / 2)
        End Using
        ' Name and Role
        Using nameFont As New Font("Segoe UI Semibold", 11)
            g.DrawString(greetUser, nameFont, New SolidBrush(theme.PrimaryText), 78, profileY + 5)
        End Using
        Using roleFont As New Font("Segoe UI", 9)
            Dim roleBadgeX = 78
            Dim roleBadgeY = profileY + 28
            Dim roleSz = g.MeasureString(greetRole, roleFont)
            Dim badgeRect As New Rectangle(roleBadgeX, roleBadgeY, CInt(roleSz.Width) + 14, 20)
            Using badgePath = GetRoundedRect(badgeRect, 10)
                g.FillPath(New SolidBrush(Color.FromArgb(40, theme.Accent)), badgePath)
            End Using
            g.DrawString(greetRole, roleFont, New SolidBrush(theme.Accent), roleBadgeX + 7, roleBadgeY + 2)
        End Using
        ' Divider above profile
        g.DrawLine(New Pen(theme.DividerColor), 15, profileY - 10, _sidebarWidth - 15, profileY - 10)

        ' Sidebar Buttons
        Dim mousePt = Me.PointToClient(Cursor.Position)
        For Each btn In _sidebarButtons
            Dim isHovered = btn.Bounds.Contains(mousePt)
            Dim isActive = (btn.ID = _activeScreen)
            
            Dim bRect = btn.Bounds
            If isActive OrElse isHovered Then
                Dim bgBrush As Brush
                If isActive Then
                    bgBrush = New SolidBrush(Color.FromArgb(30, theme.Accent))
                Else
                    bgBrush = New SolidBrush(Color.FromArgb(10, theme.PrimaryText))
                End If
                
                Using path = GetRoundedRect(bRect, 8)
                    g.FillPath(bgBrush, path)
                End Using
                
                If isActive Then
                    g.FillRectangle(New SolidBrush(theme.Accent), bRect.X, bRect.Y + 8, 4, bRect.Height - 16)
                End If
                bgBrush.Dispose()
            End If

            Dim iconColor = If(isActive, theme.Accent, If(isHovered, theme.PrimaryText, theme.SecondaryText))
            Dim textColor = If(isActive, theme.Accent, If(isHovered, theme.PrimaryText, theme.SecondaryText))

            Using iconFont As New Font("Segoe MDL2 Assets", 12)
                g.DrawString(btn.Icon, iconFont, New SolidBrush(iconColor), bRect.X + 20, bRect.Y + 12)
            End Using
            Using textFont As New Font("Segoe UI Semibold", 10)
                g.DrawString(btn.Label, textFont, New SolidBrush(textColor), bRect.X + 55, bRect.Y + 12)
            End Using
        Next

        ' Divider Line between sidebar and content
        g.DrawLine(New Pen(theme.DividerColor), _sidebarWidth, 0, _sidebarWidth, Me.Height)
    End Sub

    Private Function GetRoundedRect(rect As Rectangle, radius As Integer) As GraphicsPath
        Dim path As New GraphicsPath()
        Dim d = radius * 2
        path.AddArc(rect.X, rect.Y, d, d, 180, 90)
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90)
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90)
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90)
        path.CloseFigure()
        Return path
    End Function

    Private Sub animTimer_Tick(sender As Object, e As EventArgs) Handles animTimer.Tick
        Me.Invalidate(New Rectangle(0, 0, _sidebarWidth, Me.Height))
    End Sub

    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        MyBase.OnMouseDown(e)
        If e.Button = MouseButtons.Left AndAlso e.Y <= _headerHeight Then
            ReleaseCapture()
            SendMessage(Me.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0)
        End If
    End Sub

    Protected Overrides Sub OnMouseClick(e As MouseEventArgs)
        MyBase.OnMouseClick(e)
        Dim pt = e.Location
        
        For Each btn In _sidebarButtons
            If btn.Bounds.Contains(pt) Then
                If btn.ID = "LOGOUT" Then
                    If MessageBox.Show("Are you sure you want to logout?", "Logout", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
                        SessionManager.CurrentUser = Nothing
                        Dim loginForm As New frmLogin()
                        loginForm.Show()
                        Me.Close()
                    End If
                ElseIf _activeScreen <> btn.ID Then
                    _activeScreen = btn.ID
                    LoadScreen(_activeScreen)
                    Me.Invalidate()
                End If
                Exit For
            End If
        Next
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Application.Exit()
    End Sub

    Private Sub btnTheme_Click(sender As Object, e As EventArgs) Handles btnTheme.Click
        AppManager.IsDarkMode = Not AppManager.IsDarkMode
        ApplyLocalizationAndTheme()
    End Sub

    Private Sub btnLang_Click(sender As Object, e As EventArgs) Handles btnLang.Click
        Select Case AppManager.CurrentLang
            Case "EN" : AppManager.CurrentLang = "AM"
            Case "AM" : AppManager.CurrentLang = "OM"
            Case Else : AppManager.CurrentLang = "EN"
        End Select
        ApplyLocalizationAndTheme()
    End Sub

    Private Sub LoadScreen(screenID As String)
        CustomContentPanel.Controls.Clear()

        Dim f As Form = Nothing

        Select Case screenID
            Case "OVERVIEW"
                f = New frmDashboardOverview()
            Case "USERS"
                f = New frmUsers()
            Case "INVENTORY"
                f = New frmInventory()
            Case "POS"
                f = New frmPOS()
            Case "PURCHASES"
                f = New frmPurchase()
            Case "REPORTS"
                f = New frmReports()
            Case "SETTINGS"
                f = New frmSettings()
        End Select

        If f IsNot Nothing Then
            f.TopLevel = False
            f.FormBorderStyle = FormBorderStyle.None
            f.Dock = DockStyle.Fill
            CustomContentPanel.Controls.Add(f)
            f.Show()
        End If
    End Sub

    Private Class DashboardButton
        Public Property ID As String
        Public Property Label As String
        Public Property Icon As String
        Public Property Bounds As Rectangle
        Public Sub New(id As String, label As String, icon As String, y As Integer)
            Me.ID = id
            Me.Label = label
            Me.Icon = icon
            Me.Bounds = New Rectangle(15, y, 220, 40)
        End Sub
    End Class
End Class
