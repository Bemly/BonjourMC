
Option Explicit On
Option Strict On


Namespace Utility.Model.Mojang

	Public Class Minecraft

		' 本体运行库
		Structure Libraries
			Public ReadOnly path As String
			Public ReadOnly sha1 As String
			Public ReadOnly size As Integer
			Public ReadOnly url As String
			Public ReadOnly name As String
			Public ReadOnly os As String

			' 排除掉空实例 请上游自行过滤
			Public ReadOnly Property is_empty As Boolean
				Get
					Return size = 0
				End Get
			End Property

			' 结构体不能关闭无参数的构造函数
			Public Sub New(ByVal path As String, ByVal sha1 As String, ByVal size As Integer,
					ByVal url As String, ByVal name As String, Optional ByVal os As String = "")
				Me.path = path
				Me.sha1 = sha1
				Me.size = size
				Me.url = url
				Me.name = name
				Me.os = os
			End Sub

			Public Sub New(ByVal path As String, ByVal sha1 As String, ByVal size As String,
					ByVal url As String, ByVal name As String, Optional ByVal os As String = "")
				Me.New(path, sha1, Integer.Parse(size), url, name, os)
			End Sub

			Public Function is_target_os(ByRef os As String) As Boolean
				Return Me.os = "" OrElse Me.os = os
			End Function
		End Structure

		' 本体资源包
		Structure Assets
			Public ReadOnly path As String
			Public ReadOnly hash As String
			Public ReadOnly size As Integer

			Public ReadOnly Property is_empty As Boolean
				Get
					Return size = 0
				End Get
			End Property

			Public Sub New(ByVal path As String, ByVal hash As String, ByVal size As Integer)
				Me.path = path
				Me.hash = hash
				Me.size = size
			End Sub

			Public Sub New(ByVal path As String, ByVal hash As String, ByVal size As String)
				Me.New(path, hash, Integer.Parse(size))
			End Sub
		End Structure
	End Class
End Namespace


