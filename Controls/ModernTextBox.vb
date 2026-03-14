Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports WinFormsApp1.Core

Public Class ModernTextBox
    Inherits UserControl

    Private WithEvents txtInput As TextBox
    Private _placeholderText As String = "Enter text..."
    Private _labelText As String = ""
    Private _isFocused As Boolean = False
    Private _iconChar As String = ""

    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property UsePasswordChar As Boolean
        Get
            Return txtInput.UseSystemPasswordChar
        End Get
        Set(value As Boolean)
            txtInput.UseSystemPasswordChar = value
        End Set
    End Property

    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property PlaceholderText As String
        Get
            Return _placeholderText
        End Get
        Set(value As String)
            _placeholderText = value
            Me.Invalidate()
        End Set
    End Property

    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property LabelText As String
        Get
            Return _labelText
        End Get
        Set(value As String)
            _labelText = value
            Me.Invalidate()
        End Set
    End Property

    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property IconChar As String
        Get
            Return _iconChar
        End Get
        Set(value As String)
            _iconChar = value
            Me.Invalidate()
        End Set
    End Property

    Public Overrides Property Text As String
        Get
            Return txtInput.Text
        End Get
        Set(value As String)
            txtInput.Text = value
        End Set
    End Property

    Public Sub New()
        Me.Size = New Size(350, 70)
        Me.DoubleBuffered = True
        Me.BackColor = Color.Transparent

        txtInput = New TextBox() With {
            .BorderStyle = BorderStyle.None,
            .Font = New Font("Segoe UI", 11)
        }

        Me.Controls.Add(txtInput)
        PositionInput()
    End Sub

    Private Sub PositionInput()
        If txtInput Is Nothing Then Return
        Dim labelOffset = If(String.IsNullOrEmpty(_labelText), 0, 22)
        Dim iconOffset = If(String.IsNullOrEmpty(_iconChar), 14, 40)
        txtInput.Location = New Point(iconOffset, labelOffset + 14)
        txtInput.Width = Me.Width - iconOffset - 16
    End Sub

    Public Sub ApplyTheme()
        Dim theme = AppManager.CurrentTheme
        txtInput.BackColor = theme.InputBackground
        txtInput.ForeColor = theme.PrimaryText
        PositionInput()
        Me.Invalidate()
    End Sub

    Private Sub txtInput_GotFocus(sender As Object, e As EventArgs) Handles txtInput.GotFocus
        _isFocused = True
        Me.Invalidate()
    End Sub

    Private Sub txtInput_LostFocus(sender As Object, e As EventArgs) Handles txtInput.LostFocus
        _isFocused = False
        Me.Invalidate()
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Dim g = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias
        g.TextRenderingHint = Drawing.Text.TextRenderingHint.ClearTypeGridFit
        Dim theme = AppManager.CurrentTheme

        ' Label
        Dim labelOffset = 0
        If Not String.IsNullOrEmpty(_labelText) Then
            Using labelFont As New Font("Segoe UI Semibold", 9)
                g.DrawString(_labelText, labelFont, New SolidBrush(theme.SecondaryText), 2, 0)
            End Using
            labelOffset = 22
        End If

        ' Input Box Background
        Dim boxRect As New Rectangle(0, labelOffset, Me.Width - 1, Me.Height - labelOffset - 1)
        Dim radius = 10
        Dim path As New GraphicsPath()
        path.AddArc(boxRect.X, boxRect.Y, radius, radius, 180, 90)
        path.AddArc(boxRect.Right - radius, boxRect.Y, radius, radius, 270, 90)
        path.AddArc(boxRect.Right - radius, boxRect.Bottom - radius, radius, radius, 0, 90)
        path.AddArc(boxRect.X, boxRect.Bottom - radius, radius, radius, 90, 90)
        path.CloseFigure()

        Using br As New SolidBrush(theme.InputBackground)
            g.FillPath(br, path)
        End Using

        ' Border
        Dim borderColor = If(_isFocused, theme.Accent, theme.InputBorder)
        Dim borderWidth = If(_isFocused, 2.0F, 1.0F)
        Using p As New Pen(borderColor, borderWidth)
            g.DrawPath(p, path)
        End Using

        ' Focus ring
        If _isFocused Then
            Using outerPen As New Pen(Color.FromArgb(40, theme.Accent), 3)
                g.DrawPath(outerPen, path)
            End Using
        End If

        ' Icon
        If Not String.IsNullOrEmpty(_iconChar) Then
            Dim iconColor = If(_isFocused, theme.Accent, theme.SecondaryText)
            ' Using Segoe MDL2 Assets for professional, reliable Windows icons
            Using iconFont As New Font("Segoe MDL2 Assets", 10)
                Dim iconRect = New Rectangle(12, labelOffset + (boxRect.Height - 20) \ 2, 20, 20)
                Dim isf As New StringFormat() With {.Alignment = StringAlignment.Center, .LineAlignment = StringAlignment.Center}
                g.DrawString(_iconChar, iconFont, New SolidBrush(iconColor), iconRect, isf)
            End Using
        End If

        ' Placeholder
        If String.IsNullOrWhiteSpace(txtInput.Text) AndAlso Not _isFocused Then
            Dim phX = If(String.IsNullOrEmpty(_iconChar), 14, 40)
            TextRenderer.DrawText(g, _placeholderText, New Font("Segoe UI", 10), New Point(phX, labelOffset + 13), theme.SecondaryText)
        End If
    End Sub

    Protected Overrides Sub OnResize(e As EventArgs)
        MyBase.OnResize(e)
        PositionInput()
    End Sub
End Class
