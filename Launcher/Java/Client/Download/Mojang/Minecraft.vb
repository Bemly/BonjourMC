
Option Explicit On
Option Strict On

Imports System
Imports System.Net.Http
Imports System.Threading.Tasks
Imports System.Collections.Generic
Imports System.IO
Imports Version = Launcher.Utility.Model.Version
Imports Jsn = Launcher.Utility.Bridge.Json
Imports Fetch = Launcher.Utility.Bridge.Download

''' <summary>
''' 下载麻将🀄️的jar包
''' </summary>
''' <param name="id"></param>
''' <returns></returns>
Namespace Java.Client.Download.Mojang

	Public Class Minecraft
		' is_compatible_mode True = JSON, False = SQLite
		Private is_compatible_mode As Boolean
		Private version As Version


		' 暂不整合到 Configuration 中
		Private location As String
		Private version_pth As String
		Private manifests_pth As String
		Private manifests_url As String
		Private manifest_pth As String
		Private vanilla As String

		Public Sub New(Optional ByVal is_compatible_mode As Boolean = False)
			Me.is_compatible_mode = is_compatible_mode
			Me.location = Path.GetFullPath(Config.file.mc, Environment.CurrentDirectory)
			Me.version_pth = location & "versions/"
			Me.manifests_pth = version_pth & "version_manifest_v2.json"
			Me.manifests_url = Config.url.domain.mojang_v2 & Config.url.version_manifest.mojang_v2
			Me.manifest_pth = $"{ version_pth }{ version }/{ version }.json"
			Me.vanilla = $"{ version_pth }{ version }/{ version }.jar"
		End Sub

		Public Function switch_mode(ByVal is_compatible_mode As Boolean) As Minecraft
			Throw New NotImplementedException("还不提供转换格式捏🤏")
			Me.is_compatible_mode = is_compatible_mode
			Return Me
		End Function

		Public Function set_version(ByVal version As Version) As Minecraft
			Me.version = version
			Return Me
		End Function

		Public Function set_version(ByVal version As String) As Minecraft
			set_version(New Version(version))
			Return Me
		End Function

		Public Function set_version(ByVal major As Integer, ByVal minor As Integer,
									ByVal patch As Integer) As Minecraft
			set_version(New Version(major, minor, patch))
			Return Me
		End Function

		Public Sub pull_manifests()
			Fetch.save_web_stream(manifests_url, manifests_pth).Wait()
		End Sub

		Public Sub pull_manifest()
			If Not File.Exists(manifests_pth) Then pull_manifests()
			Dim str = File.ReadAllText(manifests_pth)
			' TODO: 此处关系有问题，需要重新画图😭或者自己实现一套😠
			Dim arr = CType(Jsn.to_json(str), Newtonsoft.Json.Linq.JObject)("versions")
			Dim i = 0
			' 遍历所需版本
			For Each item In arr
				If item("id").ToString() = version.ToString() Then Exit For
				i += 1
			Next
			Console.WriteLine(arr(i)("url"))
			Fetch.save_web_stream(arr(i)("url").ToString(), manifest_pth).Wait()
		End Sub

		' TODO: [bugs] Downcasting Not Secure!!!!! Plz redesign Class model!😠
		Public Sub pull_vanilla(ByVal obj As Newtonsoft.Json.Linq.JObject)
			Dim url As String = obj("downloads")("client")("url").ToString()
			Console.WriteLine(url)
			Fetch.save_web_stream(url, vanilla).Wait()
		End Sub

		Public Sub pull_libraries(ByVal obj As Newtonsoft.Json.Linq.JObject)

		End Sub

		Public Sub pull_assets(ByVal obj As Newtonsoft.Json.Linq.JObject)

		End Sub

		Public Sub install()
			If Not File.Exists(manifest_pth) Then pull_manifest()
			Dim str = File.ReadAllText(manifest_pth)
			Dim obj = CType(Jsn.to_json(str), Newtonsoft.Json.Linq.JObject)
			If Not File.Exists(vanilla) Then pull_vanilla(obj)
			' TODO: 之后使用 Queue 队列来检查和下载
			If check_libraries() Then pull_libraries(obj)
			If check_assets() Then pull_assets(obj)
		End Sub

		Public Function check_libraries() As Boolean
			Return True
		End Function

		Public Function check_assets() As Boolean
			Return True
		End Function

	End Class

End Namespace


