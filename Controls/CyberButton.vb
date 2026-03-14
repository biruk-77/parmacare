Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms

Public Class CyberButton
    Inherits Button

    Private _isHovering As Boolean = False

    Public Sub New()
        Me.FlatStyle = FlatStyle.Flat
        Me.FlatAppearance.BorderSize = 0
        Me.BackColor = Color.FromArgb(19, 236, 182)
        Me.ForeColor = Color.FromArgb(16, 34, 29)
        Me.Font = New Font("Segoe UI Semibold", 11)
        Me.Cursor = Cursors.Hand
        Me.DoubleBuffered = True
    End Sub

    Protected Overrides Sub OnMouseEnter(e As EventArgs)
        _isHovering = True
        Me.Invalidate()
        MyBase.OnMouseEnter(e)
    End Sub

    Protected Overrides Sub OnMouseLeave(e As EventArgs)
        _isHovering = False
        Me.Invalidate()
        MyBase.OnMouseLeave(e)
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Dim g = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias
        g.TextRenderingHint = Drawing.Text.TextRenderingHint.ClearTypeGridFit

        Dim rect = New Rectangle(0, 0, Me.Width - 1, Me.Height - 1)
        Dim radius = 12
        Dim path As New GraphicsPath()
        path.AddArc(0, 0, radius, radius, 180, 90)
        path.AddArc(Me.Width - radius - 1, 0, radius, radius, 270, 90)
        path.AddArc(Me.Width - radius - 1, Me.Height - radius - 1, radius, radius, 0, 90)
        path.AddArc(0, Me.Height - radius - 1, radius, radius, 90, 90)
        path.CloseFigure()

        ' Shadow (subtle glow)
        If _isHovering Then
            Using shadowBrush As New SolidBrush(Color.FromArgb(50, Me.BackColor))
                Dim shadowRect As New Rectangle(-2, 2, Me.Width + 3, Me.Height + 3)
                Dim shadowPath As New GraphicsPath()
                shadowPath.AddArc(shadowRect.X, shadowRect.Y, radius + 4, radius + 4, 180, 90)
                shadowPath.AddArc(shadowRect.Right - radius - 4, shadowRect.Y, radius + 4, radius + 4, 270, 90)
                shadowPath.AddArc(shadowRect.Right - radius - 4, shadowRect.Bottom - radius - 4, radius + 4, radius + 4, 0, 90)
                shadowPath.AddArc(shadowRect.X, shadowRect.Bottom - radius - 4, radius + 4, radius + 4, 90, 90)
                shadowPath.CloseFigure()
                g.FillPath(shadowBrush, shadowPath)
            End Using
        End If

        ' Main Fill
        Dim fillColor = If(_isHovering, Color.FromArgb(Math.Min(255, Me.BackColor.R + 15), Math.Min(255, Me.BackColor.G + 15), Math.Min(255, Me.BackColor.B + 15)), Me.BackColor)
        Using brush As New SolidBrush(fillColor)
            g.FillPath(brush, path)
        End Using

        ' Text
        Dim sf As New StringFormat() With {.Alignment = StringAlignment.Center, .LineAlignment = StringAlignment.Center}
        Using textBrush As New SolidBrush(Me.ForeColor)
            g.DrawString(Me.Text, Me.Font, textBrush, rect, sf)
        End Using

        Me.Region = New Region(path)
    End Sub
End Class
