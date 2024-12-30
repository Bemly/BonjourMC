
Option Explicit On
Option Strict On

Imports System
Imports Launcher.Utility.Model.Mojang.Minecraft
Imports NUnit.Framework
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports Newtonsoft.Json.Schema



<TestFixture>
Public Class Test_lib

	<Test>
	Public Sub test_empty()
		Dim awda = New Libraries()
		Console.WriteLine(awda.is_empty)
	End Sub

	<Test>
	Public Sub test_null_key()
		Dim i = JObject.Parse("{""w"":""2""}")
		Console.WriteLine(i.HasValues)
		Console.WriteLine(i("w").HasValues)
		Console.WriteLine(i("w") Is Nothing)
		Console.WriteLine(i("x") Is Nothing)
	End Sub

	'<Test>
	'Public Sub net9_checker()
	'	Dim person As Assets = Nothing
	'	Dim name As Integer = Assets?.size
	'   Dim result As Integer = If(Assets?.size, 2)
	'
	'	Console.WriteLine(name)
	'End Sub
	' 使用 ?. 来访问一个可能为空的对象的成员 person?.Name 会安全地访问 Name 属性
End Class


