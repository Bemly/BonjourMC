
Option Explicit On
Option Strict On

Imports System
Imports NUnit.Framework
Imports System.Diagnostics
Imports System.IO
Imports System.IO.Enumeration
Imports FastSearchLibrary
Imports System.Collections.Generic

<TestFixture>
Public Class Test_time

	<Test, Timeout(1000)>
	<Obsolete>
	Public Sub test_simple_time()
		' Nunit的时间测试已经废弃⚠️
		Threading.Thread.Sleep(2000)
	End Sub

	<Test>
	Public Sub test_custom_time()
		Dim sw As Stopwatch = Stopwatch.StartNew()
		Threading.Thread.Sleep(500)
		sw.Stop()
		Console.WriteLine(sw.ElapsedMilliseconds)
	End Sub

	<Test>
	Public Sub test_nuit_time()
		' 该部分已在 NUnit 3 移除
		'Dim startTime As DateTime = TestContext.CurrentContext.StartTime

		'' 测试代码
		'Threading.Thread.Sleep(500) ' 模拟测试逻辑

		'Dim endTime As DateTime = TestContext.CurrentContext.EndTime
		'TestContext.WriteLine($"Test executed in {(endTime - startTime).TotalMilliseconds} ms")
	End Sub


	''' <summary>
	''' 结论：小范围搜索直接用微软的递归查找，大范围使用开源的FileSearcher 🤡
	''' </summary>
	<Test>
	Public Sub test_search_file_speed()

		Dim saw = Stopwatch.StartNew()
		saw.Stop()
		Console.WriteLine(saw.Elapsed)

		Dim locate = "/Users/bemly/Applications/"


		Console.WriteLine(Path.GetFullPath("/Users/bemly/"))
		Dim sw As Stopwatch = Stopwatch.StartNew()
		Dim files As List(Of FileInfo) = FileSearcher.GetFilesFast(locate, "java")
		Console.WriteLine(files.Count)
		For Each file In files
			Console.WriteLine(file)
		Next
		sw.Stop()
		Console.WriteLine(sw.Elapsed)


		Dim sew = Stopwatch.StartNew()
		Dim fileList As New List(Of String)
		Try
			' 使用 EnumerateFiles 延迟加载文件
			fileList.AddRange(Directory.EnumerateFiles(locate, "java", SearchOption.AllDirectories))
		Catch ex As UnauthorizedAccessException
			Console.WriteLine($"无法访问目录：{locate}。错误：{ex.Message}")
		End Try

		' 输出找到的文件
		For Each file As String In fileList
			Console.WriteLine(file)
		Next
		Console.WriteLine(fileList)
		sew.Stop()
		Console.WriteLine(sew.Elapsed)
	End Sub

End Class


