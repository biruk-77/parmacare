Imports WinFormsApp1.Models

Public Class SessionManager
    ' Holds the globally authenticated user for the current application instance
    Public Shared Property CurrentUser As UserSessionContext
End Class

''' <summary>
''' Read-only context for the currently authenticated user.
''' Used across all modules (Inventory, POS) to track exactly who performed which action.
''' </summary>
Public Class UserSessionContext
    Public ReadOnly Property UserID As Integer
    Public ReadOnly Property Username As String
    Public ReadOnly Property FullName As String
    Public ReadOnly Property RoleID As Integer
    Public ReadOnly Property RoleName As String

    Public Sub New(userId As Integer, username As String, fullName As String, roleId As Integer, roleName As String)
        Me.UserID = userId
        Me.Username = username
        Me.FullName = fullName
        Me.RoleID = roleId
        Me.RoleName = roleName
    End Sub
End Class
