
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

        Public Sub New(Version As String)
            If String.IsNullOrWhiteSpace(Version) Then Throw New ArgumentException("Invalid Version format.")
            Version = Version.Trim()

            Dim parts() As String = Version.Split("."c)
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


        ' 重载 ToString 方法，返回版本号的字符串表示
        Public Overrides Function ToString() As String
            Return $"{major}.{minor}.{patch}"
        End Function

        ' 实现 IComparable 接口，比较两个版本号
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


