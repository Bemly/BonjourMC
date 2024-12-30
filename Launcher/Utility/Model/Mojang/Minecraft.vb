
Option Explicit On
Option Strict On

Imports System


Namespace Utility.Model.Mojang
	
	Public Class Minecraft

		Structure Libraries
			Public ReadOnly path As String
			Public ReadOnly sha1 As String
			Public ReadOnly size As Integer
			Public ReadOnly url As String
			Public ReadOnly name As String
			Public ReadOnly os As Rule

			Public Enum Rule
				gloabl = 0
				osx = 3
				windows = 1
				linux = 2
			End Enum


			Public Sub New(ByVal path As String, ByVal sha1 As String, ByVal size As Integer,
					ByVal url As String, ByVal name As String, ByVal os As Rule)
				Me.path = path
				Me.sha1 = sha1
				Me.size = size
				Me.url = url
				Me.name = name
				Me.os = os
			End Sub

			Public Sub New(ByVal path As String, ByVal sha1 As String, ByVal size As String,
					ByVal url As String, ByVal name As String, Optional ByVal os As Integer = 0)
				Me.New(path, sha1, Integer.Parse(size), url, name, CType(os, Rule))
			End Sub

		End Structure
	End Class
End Namespace


