
Option Explicit On
Option Strict On

Imports NUnit.Framework
Imports System.IO
Imports System
Imports System.Threading.Tasks
Imports Newtonsoft.Json.Schema
Imports Newtonsoft.Json.Linq

Public Class Tests
	<SetUp>
	Public Sub setup()
		' NUnit 是侵入性 测试工具
	End Sub

	<Test>
	Public Sub test_json_net()
		Dim filepth As String = Path.GetFullPath("../../../../Launcher/Res/tmp/1.21.4.json", Environment.CurrentDirectory)
		Console.WriteLine(filepth)
		Dim filestr As String = File.ReadAllText(filepth)
		Dim schema As JObject = JObject.Parse(filestr)
		Console.WriteLine(schema("latest"))

	End Sub

	Public Class ExampleClass
		' Shared 构造函数
		Shared Sub New()
			Console.WriteLine("Shared Sub New() called")
		End Sub

		' 实例构造函数
		Public Sub New()
			Console.WriteLine("Instance Sub New() called")
		End Sub
	End Class

	<Test>
	Public Sub test_shared_sub_new()
		' 首次引用类，触发 Shared Sub New()
		Console.WriteLine("Creating first instance:")
		Dim obj1 As New ExampleClass()

		' 第二次创建实例，仅触发实例构造函数
		Console.WriteLine("Creating second instance:")
		Dim obj2 As New ExampleClass()
	End Sub

	<Test>
	Public Sub test_gettype_onlyist()
		' 唯一性检测
		Dim type1 As Type = GetType(String)
		Dim type2 As Type = GetType(String)
		Console.WriteLine(Object.ReferenceEquals(type1, type2)) ' 输出 True
		Console.WriteLine(type1 Is type2)
	End Sub

	<Test>
	Public Sub test_select_else()
		' 看下是不是直接判断，还是像java一样看引用地址
		Dim a As String = "ABCD"
		Select Case a
			Case "ABCD"
				Assert.Pass()
			Case Else
				Assert.Fail()
		End Select
	End Sub

	<Test>
	Public Sub test_url_convert()
		' 末尾是否输出"/"根据输入确定
		Console.WriteLine(Path.GetFullPath("../../../../Launcher/Res/tmp/"))
		Console.WriteLine(Path.GetFullPath("../../../../Launcher/Res/tmp"))
	End Sub

	Structure Libraries
		Public p As String
		Public ReadOnly x As String
		Shared y As Integer = 20

		Public Sub New(ByVal x As String)
			Me.x = x
		End Sub

		'/Users/bemly/Projects/BonjourMC/Test/Unit_test_core.vb(21,21): Error BC30629: 结构不能声明没有参数的非共享 "Sub New"。 (BC30629) (Test)
		'Private Sub New()
		'End Sub
	End Structure

	''' <summary>
	'''  readonly 只能在构造函数里面复制 不能在With语句里面赋值
	''' </summary>
	<Test>
	Public Sub test_struct()
		Dim a = New Libraries("a")
		Dim b = New Libraries()
		'/Users/bemly/Projects/BonjourMC/Test/Unit_test_core.vb(49,49): Error BC30064: '"ReadOnly" 变量不能作为赋值目标。 (BC30064) (Test)
		'Dim c = New Libraries With {.p = "bb", .x = "aa"}
		Dim c = New Libraries With {.p = "bb"}
		Console.WriteLine($"b.x:{ b.x = "" }")
		Console.WriteLine("a.x:" & a.x)
		Console.WriteLine("Libraries.y:" & Libraries.y)
		Console.WriteLine("c.p:" & c.p)
		Console.WriteLine($"a.p:{ a.p = "" }")
	End Sub

End Class