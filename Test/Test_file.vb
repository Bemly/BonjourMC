
Option Explicit On
Option Strict On

Imports System
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Schema
Imports Newtonsoft.Json.Linq
Imports NUnit.Framework
Imports System.IO
Imports Launcher.Utility.Bridge.Crypto


<TestFixture>
Public Class Test_file

	<Test>
	Public Sub check_more_rule_segment()
		Dim str = File.ReadAllText("/Users/bemly/Projects/BonjourMC/tmp/1.21.4.json")
		Dim obj = JObject.Parse(str)
		Dim x = 0
		For Each i In obj("libraries")
			If i("rules") IsNot Nothing Then
				x += 1
				Dim y = 0
				For Each j In i("rules")
					If y = 1 Then Assert.Fail(j.ToString())
					y += 1
				Next
			End If
		Next
	End Sub

	<Test>
	Public Sub only_select_first()
		Dim str = File.ReadAllText("/Users/bemly/Projects/BonjourMC/tmp/1.21.4.json")
		Dim obj = JObject.Parse(str)("libraries")(0)("rules")(0)("os")("name")
		Console.WriteLine(obj)
	End Sub

	<Test>
	Public Sub test_sha1_sum()
		Dim res = SHA1("/Users/bemly/Projects/BonjourMC/Launcher/Res/tmp/mc/versions/1.21.4/1.21.4.jar")
		Console.WriteLine(res)
		If res = "a7e5a6024bfd3cd614625aa05629adf760020304" Then Assert.Pass()
		Assert.Fail()
	End Sub

	<Test>
	Public Sub test_get_json_key()
		Dim str = File.ReadAllText("/Users/bemly/Projects/BonjourMC/tmp/1.21.4.json")
		Dim obj As JObject = CType(JObject.Parse(str)("libraries")(0), JObject)
		For Each prop As JProperty In obj.Properties()
			Console.WriteLine($"Key: {prop.Name}, Value: {prop.Value}")
			If prop.Name = "rules" Then
				Console.WriteLine(prop.Value(0)("action"))
			End If
		Next

		Console.WriteLine(obj)
	End Sub
End Class


