Imports WinFormsApp1.Models
Imports WinFormsApp1.Core
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.InteropServices
Imports System.Windows.Forms

Public Class frmAboutUs
    Inherits Form

    ' Windows API for borderless dragging
    Public Const WM_NCLBUTTONDOWN As Integer = &HA1
    Public Const HT_CAPTION As Integer = &H2
    <DllImport("user32.dll")> Public Shared Function SendMessage(hWnd As IntPtr, Msg As Integer, wParam As Integer, lParam As Integer) As Integer
    End Function
    <DllImport("user32.dll")> Public Shared Function ReleaseCapture() As Boolean
    End Function

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
        Me.Size = New Size(600, 400)
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.BackColor = AppManager.CurrentTheme.Background

        btnBack = New Button() With {
            .Text = ChrW(&HE72B) & " " & AppManager.GetText("Back"),
            .Size = New Size(100, 30),
            .Location = New Point(20, 20),
            .FlatStyle = FlatStyle.Flat,
            .Cursor = Cursors.Hand,
            .Font = New Font("Segoe MDL2 Assets", 9),
            .ForeColor = AppManager.CurrentTheme.Accent
        }
        btnBack.FlatAppearance.BorderSize = 0

        Me.Controls.Add(btnBack)
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Dim g = e.Graphics
        Dim theme = AppManager.CurrentTheme
        g.SmoothingMode = SmoothingMode.AntiAlias
        g.TextRenderingHint = Drawing.Text.TextRenderingHint.ClearTypeGridFit

        ' Title
        Using titleFont As New Font("Segoe UI", 24, FontStyle.Bold)
            g.DrawString(AppManager.GetText("AboutTitle"), titleFont, New SolidBrush(theme.PrimaryText), 50, 80)
        End Using

        ' Description
        Using descFont As New Font("Segoe UI", 11)
            Dim rect As New Rectangle(50, 140, Me.Width - 100, 150)
            g.DrawString(AppManager.GetText("AboutDesc"), descFont, New SolidBrush(theme.SecondaryText), rect)
        End Using

        ' Contact
        Using contactFont As New Font("Segoe UI Semibold", 10)
            g.DrawString(AppManager.GetText("ContactInfo"), contactFont, New SolidBrush(theme.Accent), 50, 320)
        End Using

        ' Border
        Using p As New Pen(theme.Accent, 2)
            g.DrawRectangle(p, 0, 0, Me.Width - 1, Me.Height - 1)
        End Using
    End Sub

    Private Sub btnBack_Click(sender As Object, e As EventArgs) Handles btnBack.Click
        Me.Close()
    End Sub

    Private Sub animTimer_Tick(sender As Object, e As EventArgs) Handles animTimer.Tick
        If Me.Opacity < 1 Then Me.Opacity += 0.1
    End Sub

    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        MyBase.OnMouseDown(e)
        ReleaseCapture()
        SendMessage(Me.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0)
    End Sub
End Class
