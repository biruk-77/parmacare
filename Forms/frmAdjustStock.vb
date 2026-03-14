Imports System.Drawing
Imports System.Windows.Forms
Imports WinFormsApp1.Models
Imports WinFormsApp1.Core
Imports System.Linq

Public Class frmAdjustStock
    Inherits Form

    Private _batchID As Integer
    Private _currentBatch As Batch
    Private _productName As String
    
    Private lblHeader As Label
    Private lblProdDetails As Label
    Private lblCurrentStock As Label
    
    Private numAdjustment As NumericUpDown
    Private rbAdd As RadioButton
    Private rbSubtract As RadioButton
    
    Private WithEvents btnSave As CyberButton
    Private WithEvents btnCancel As CyberButton

    Public Sub New(batchID As Integer)
        _batchID = batchID
        Me.DoubleBuffered = True
        InitializeUI()
        LoadBatchData()
    End Sub

    Private Sub InitializeUI()
        Me.FormBorderStyle = FormBorderStyle.None
        Me.StartPosition = FormStartPosition.CenterParent
        Me.Size = New Size(400, 350)
        Me.BackColor = AppManager.CurrentTheme.Surface

        lblHeader = New Label() With {
            .Text = "Adjust Stock",
            .Font = New Font("Segoe UI", 16, FontStyle.Bold),
            .ForeColor = AppManager.CurrentTheme.PrimaryText,
            .AutoSize = True,
            .Location = New Point(20, 20)
        }
        
        lblProdDetails = New Label() With {
            .Text = "Loading details...",
            .Font = New Font("Segoe UI", 11),
            .ForeColor = AppManager.CurrentTheme.SecondaryText,
            .AutoSize = True,
            .Location = New Point(20, 60)
        }

        lblCurrentStock = New Label() With {
            .Text = "Current Stock: 0",
            .Font = New Font("Segoe UI Semibold", 12),
            .ForeColor = AppManager.CurrentTheme.Accent,
            .AutoSize = True,
            .Location = New Point(20, 90)
        }

        Dim labelAdj As New Label() With {
            .Text = "Adjustment Quantity:",
            .Font = New Font("Segoe UI", 10),
            .ForeColor = AppManager.CurrentTheme.PrimaryText,
            .AutoSize = True,
            .Location = New Point(20, 140)
        }

        numAdjustment = New NumericUpDown() With {
            .Location = New Point(20, 165),
            .Width = 150,
            .Maximum = 10000,
            .Minimum = 1,
            .Font = New Font("Segoe UI", 12)
        }

        rbAdd = New RadioButton() With {
            .Text = "Add to Stock",
            .Checked = True,
            .Location = New Point(190, 155),
            .ForeColor = AppManager.CurrentTheme.PrimaryText
        }
        
        rbSubtract = New RadioButton() With {
            .Text = "Subtract (Loss/Damage)",
            .Location = New Point(190, 180),
            .Width = 200,
            .ForeColor = AppManager.CurrentTheme.PrimaryText
        }

        btnCancel = New CyberButton() With {
            .Text = "Cancel",
            .Location = New Point(90, 260),
            .Size = New Size(130, 45),
            .BackColor = Color.FromArgb(100, 100, 100),
            .Font = New Font("Segoe UI Semibold", 10)
        }
        
        btnSave = New CyberButton() With {
            .Text = "Apply Options",
            .Location = New Point(240, 260),
            .Size = New Size(130, 45),
            .BackColor = AppManager.CurrentTheme.Accent,
            .Font = New Font("Segoe UI Semibold", 10)
        }

        Me.Controls.AddRange({lblHeader, lblProdDetails, lblCurrentStock, labelAdj, numAdjustment, rbAdd, rbSubtract, btnCancel, btnSave})
    End Sub

    Private Sub LoadBatchData()
        Try
            Using db As New PharmacyContext()
                Dim info = (From b In db.Batches
                            Join p In db.Products On b.ProductID Equals p.ProductID
                            Where b.BatchID = _batchID
                            Select New With {.Batch = b, .PName = p.Name}).FirstOrDefault()
                
                If info IsNot Nothing Then
                    _currentBatch = info.Batch
                    _productName = info.PName
                    
                    lblProdDetails.Text = $"{_productName} (Batch: {_currentBatch.BatchNumber})"
                    lblCurrentStock.Text = $"Current Stock: {_currentBatch.CurrentQuantity}"
                Else
                    MessageBox.Show("Batch not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    Me.Close()
                End If
            End Using
        Catch ex As Exception
        End Try
    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        Try
            Using db As New PharmacyContext()
                Dim b = db.Batches.FirstOrDefault(Function(x) x.BatchID = _batchID)
                If b IsNot Nothing Then
                    Dim adj = CInt(numAdjustment.Value)
                    If rbSubtract.Checked Then
                        If b.CurrentQuantity - adj < 0 Then
                            MessageBox.Show("Adjustment results in negative stock, which is not allowed.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                            Return
                        End If
                        b.CurrentQuantity -= adj
                    Else
                        b.CurrentQuantity += adj
                    End If
                    
                    db.SaveChanges()
                    MessageBox.Show("Stock adjusted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    Me.Close()
                End If
            End Using
        Catch ex As Exception
            MessageBox.Show("Error adjusting stock: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
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
