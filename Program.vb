Imports System.Windows.Forms

Module Program
    <STAThread()>
    Sub Main()
        Try
            Application.EnableVisualStyles()
            Application.SetCompatibleTextRenderingDefault(False)

            ' Launch the UI
            Application.Run(New frmLogin())

        Catch ex As Exception
            MessageBox.Show("CRITICAL SYSTEM FAILURE: " & vbCrLf & ex.ToString(), "ENGINEERING ALERT", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
End Module
