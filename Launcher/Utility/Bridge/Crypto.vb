
Option Explicit On
Option Strict On

Imports System
Imports Secure_Hash_Algorithm_1 = System.Security.Cryptography.SHA1
Imports System.IO


Namespace Utility.Bridge
	
	Public Class Crypto

		Shared Function SHA1(p As String, s As String) As Boolean
			Return SHA1(p) = s
		End Function

		Shared Function SHA1(pth As String) As String
			Using s As Secure_Hash_Algorithm_1 = Secure_Hash_Algorithm_1.Create()
				Using fileStream As FileStream = File.OpenRead(pth)
					Dim hashBytes As Byte() = s.ComputeHash(fileStream)
					' Bytes => String
					Return BitConverter.ToString(hashBytes).Replace("-", "").ToLower()
				End Using
			End Using
		End Function
	End Class
End Namespace


