Imports System.Security.Cryptography

Namespace Core
    Public Class SecurityManager
        ' Generates a random cryptographic salt
        Public Shared Function GenerateSalt() As String
            Dim saltBytes(31) As Byte
            Using rng = RandomNumberGenerator.Create()
                rng.GetBytes(saltBytes)
            End Using
            Return Convert.ToBase64String(saltBytes)
        End Function

        ' Hashes the password with the salt (Military-grade PBKDF2)
        Public Shared Function HashPassword(password As String, salt As String) As String
            Dim saltBytes = Convert.FromBase64String(salt)
            Using pbkdf2 As New Rfc2898DeriveBytes(password, saltBytes, 100000, HashAlgorithmName.SHA256)
                Return Convert.ToBase64String(pbkdf2.GetBytes(32))
            End Using
        End Function

        ' Compares user input to database hash safely
        Public Shared Function VerifyPassword(password As String, dbHash As String, dbSalt As String) As Boolean
            Dim computedHash = HashPassword(password, dbSalt)
            Return computedHash = dbHash
        End Function
    End Class
End Namespace
