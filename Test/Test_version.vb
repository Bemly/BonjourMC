
Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports Version = Launcher.Utility.Model.Version
Imports NUnit.Framework
Imports System.Linq


Public Class Test_version

    <Test>
    Public Sub fail_create_version()
        Assert.Throws(Of ArgumentException)(Function() New Version(""))
        Assert.Throws(Of FormatException)(Function() New Version("1.1."))
        Assert.Throws(Of FormatException)(Function() New Version("11."))
        Assert.Throws(Of OverflowException)(Function() New Version("1.1124234234532543534535345."))
        Assert.Throws(Of ArgumentException)(Function() New Version("123534"))
        Try
            Dim abc As Version = New Version("1.1124234234532543534535345.")
        Catch ex As Exception
            Console.WriteLine(ex)
        End Try

        Try
            Dim abcd As Version = New Version("1.1.")
        Catch ex As Exception
            Console.WriteLine(ex)
        End Try
    End Sub

    <Test>
    Public Sub pass_create_version()
        Dim abc As Version = New Version("1.2.3")
        Console.WriteLine(abc)
        abc = New Version("1.2.0")
        Console.WriteLine(abc)
        abc = New Version("1.23")
        Console.WriteLine(abc)
    End Sub

    <Test>
    Public Sub sort_version()
        Dim a As Version = New Version("1.2.3")
        Dim b As Version = New Version("2.0.0")
        Dim c As Version = New Version("1.3.0")
        Dim d As Version = New Version("1.2.4")
        Dim e As Version = New Version("1.2.3")

        If Not (a < b And a < c And a < d And a = e) Then Assert.Fail()

        Dim versions_v2 As New List(Of Version) From {
            New Version("1.20.3"),
            New Version("1.20.4"),
            New Version("1.19.9"),
            New Version("2.0.0")
        }

        ' 降序
        versions_v2.Sort(Function(v1 As Version, v2 As Version) v2.CompareTo(v1))
        For Each v In versions_v2
            Console.WriteLine(v.ToString())
        Next
        Console.WriteLine("============")

        Dim versions_v3 As New List(Of Version) From {
            New Version("1.20.3"),
            New Version("1.20.4"),
            New Version("1.19.9"),
            New Version("2.0.0")
        }

        ' 降序
        versions_v3.Sort()
        versions_v3.Reverse()
        For Each v In versions_v3
            Console.WriteLine(v.ToString())
        Next
        Console.WriteLine("============")

        Dim versions As New List(Of Version) From {
            New Version("1.20.3"),
            New Version("1.20.4"),
            New Version("1.19.9"),
            New Version("2.0.0")
        }

        versions.Sort()  ' 自动按版本升序排序

        For Each v In versions
            Console.WriteLine(v.ToString())
        Next

        ' LINQ Last() First()
        Console.WriteLine($" Versions Last Element : { versions(versions.Count - 1) } { versions.Last().ToString() }")

        ' 注意 = 是比较常量池(内容) ，is 是比较引用的内存空间是否一致
        If versions(versions.Count - 1).ToString() <> "2.0.0" Then Assert.Fail()
        If versions(versions.Count - 1).ToString() IsNot "2.0.0" Then Assert.Pass()
        If versions(versions.Count - 1).ToString() = "2.0.0" Then Assert.Pass()
        If versions(versions.Count - 1).ToString() Is "2.0.0" Then Assert.Fail()
        If Not versions(versions.Count - 1).ToString() Is "2.0.0" Then Assert.Pass()

    End Sub
End Class


