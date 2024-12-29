
Option Explicit On
Option Strict On

Imports System


Namespace Utility.Model

    Public Class Version

        Implements IComparable(Of Version)

        ' 版本的各个部分（主版本、次版本、修订号）
        Private major As Integer
        Private minor As Integer
        Private patch As Integer

        Public Sub New(version As String)
            If String.IsNullOrWhiteSpace(version) Then Throw New ArgumentException("Invalid Version format.")
            version = version.Trim()

            ' TODO: vbnet✋只允许构造函数在第一条，以后优化
            Dim parts() As String = version.Split("."c)
            If parts.Length = 3 Then
                major = Integer.Parse(parts(0))
                minor = Integer.Parse(parts(1))
                patch = Integer.Parse(parts(2))
            ElseIf parts.Length = 2 Then
                major = Integer.Parse(parts(0))
                minor = Integer.Parse(parts(1))
                patch = 0
            Else
                Throw New ArgumentException("Invalid Version format.")
            End If
        End Sub

        Public Sub New(major As Integer, minor As Integer)
            Me.New(major, minor, 0)
        End Sub

        Public Sub New(major As Integer, minor As Integer, patch As Integer)
            Me.major = major
            Me.minor = minor
            Me.patch = patch
        End Sub

        ' 获取版本单号
        Public ReadOnly Property get_major As Integer
            Get
                Return major
            End Get
        End Property

        Public ReadOnly Property get_minor As Integer
            Get
                Return minor
            End Get
        End Property

        Public ReadOnly Property get_patch As Integer
            Get
                Return patch
            End Get
        End Property

        ' 重载 ToString 方法
        Public Overrides Function ToString() As String
            Return $"{major}.{minor}.{patch}"
        End Function

        ' 比较两个版本号
        Public Function CompareTo(v As Version) As Integer Implements IComparable(Of Version).CompareTo
            If major <> v.major Then Return major.CompareTo(v.major)
            If minor <> v.minor Then Return minor.CompareTo(v.minor)
            Return patch.CompareTo(v.patch)
        End Function


        Public Shared Operator >(v1 As Version, v2 As Version) As Boolean
            Return v1.CompareTo(v2) > 0
        End Operator

        Public Shared Operator <(v1 As Version, v2 As Version) As Boolean
            Return v1.CompareTo(v2) < 0
        End Operator

        Public Shared Operator =(v1 As Version, v2 As Version) As Boolean
            Return v1.CompareTo(v2) = 0
        End Operator

        Public Shared Operator >=(v1 As Version, v2 As Version) As Boolean
            Return v1.CompareTo(v2) >= 0
        End Operator

        Public Shared Operator <=(v1 As Version, v2 As Version) As Boolean
            Return v1.CompareTo(v2) <= 0
        End Operator

        Public Shared Operator <>(v1 As Version, v2 As Version) As Boolean
            Return v1.CompareTo(v2) <> 0
        End Operator

    End Class

End Namespace


