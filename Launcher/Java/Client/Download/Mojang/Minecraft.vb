
Option Explicit On
Option Strict On

Imports System
Imports System.Net.Http
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Collections.Generic
Imports System.Collections.Concurrent
Imports System.IO
Imports Launcher.Utility.Model.Mojang.Minecraft
Imports Launcher.Utility.Bridge.Crypto
Imports Version = Launcher.Utility.Model.Version
Imports Jsn = Launcher.Utility.Bridge.Json
Imports Fetch = Launcher.Utility.Bridge.Download


''' <summary>
''' 下载麻将🀄️的jar包
''' </summary>
Namespace Java.Client.Download.Mojang

	Public Class Minecraft
		Implements Utility.Interface.Progress

		' is_compatible_mode True = JSON, False = SQLite
		Private is_compatible_mode As Boolean
		Private version As Version

		' 进度事件
		Public Event on_progress(sender As Object, e As Utility.Interface.ProgressArgs) Implements Utility.Interface.Progress.on_progress
		Public Event on_complete(sender As Object, e As Utility.Interface.ProgressArgs) Implements Utility.Interface.Progress.on_complete
		Public Event on_error(sender As Object, e As Utility.Interface.ProgressArgs) Implements Utility.Interface.Progress.on_error

		' 暂不整合到 Configuration 中
		Private location As String
		Private version_pth As String
		Private libraries_pth As String
		Private manifests_pth As String
		Private manifests_url As String
		Private manifest_pth As String
		Private assets_pth As String
		Private assets_manifest As String
		Private vanilla As String

		' 原子计数器
		Private downloaded_libraries As Integer = 0
		Private downloaded_assets As Integer = 0
		Private total_libraries As Integer = 0
		Private total_assets As Integer = 0

		Public Sub New(Optional ByVal is_compatible_mode As Boolean = False)
			Me.is_compatible_mode = is_compatible_mode
			Me.location = Path.GetFullPath(Config.file.mc, Environment.CurrentDirectory)
			Me.version_pth = location & "versions/"
			Me.libraries_pth = location & "libraries/"
			Me.assets_pth = location & "assets/objects/"
			Me.assets_manifest = location & "assets/indexes/"
			Me.manifests_pth = version_pth & "version_manifest_v2.json"
			Me.manifests_url = Config.url.domain.mojang_v2 & Config.url.version_manifest.mojang_v2
		End Sub

		Public Function switch_mode(ByVal is_compatible_mode As Boolean) As Minecraft
			Throw New NotImplementedException("还不提供转换格式捏🤏")
			Me.is_compatible_mode = is_compatible_mode
			Return Me
		End Function

		Public Function set_version(ByVal version As Version) As Minecraft
			Me.version = version
			Me.manifest_pth = $"{ version_pth }{ version }/{ version }.json"
			Me.vanilla = $"{ version_pth }{ version }/{ version }.jar"
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

		Public Async Function pull_manifests() As Task
			RaiseEvent on_progress(Me, New Utility.Interface.ProgressArgs(0, 0, "Downloading version manifest...", "manifest"))
			Await Fetch.save_web_stream(manifests_url, manifests_pth)
		End Function

		Public Async Function pull_manifest() As Task
			If Not File.Exists(manifests_pth) Then Await pull_manifests()
			Dim str = File.ReadAllText(manifests_pth)
			Dim arr = CType(Jsn.to_json(str), Newtonsoft.Json.Linq.JObject)("versions")
			Dim i = 0
			For Each item In arr
				If item("id").ToString() = version.ToString() Then Exit For
				i += 1
			Next
			Console.WriteLine(arr(i)("url"))
			RaiseEvent on_progress(Me, New Utility.Interface.ProgressArgs(0, 0, $"Downloading {version} manifest...", "manifest"))
			Await Fetch.save_web_stream(arr(i)("url").ToString(), manifest_pth)
		End Function

		Public Async Function pull_vanilla(ByVal obj As Newtonsoft.Json.Linq.JObject) As Task
			Dim url As String = obj("downloads")("client")("url").ToString()
			Console.WriteLine(url)
			RaiseEvent on_progress(Me, New Utility.Interface.ProgressArgs(0, 0, "Downloading client JAR...", "jar"))
			Await Fetch.save_web_stream(url, vanilla)
		End Function

		Public Async Function pull_libraries(ByVal obj As Newtonsoft.Json.Linq.JObject) As Task
			Dim queue = New ConcurrentQueue(Of Libraries)()
			For Each i In obj("libraries")
				Dim artifact = i("downloads")("artifact")
				Dim pth = artifact("path").ToString()
				Dim sha1 = artifact("sha1").ToString()
				Dim size = artifact("size").ToString()
				Dim url = artifact("url").ToString()
				Dim name = i("name").ToString()
				Dim os = ""
				If i("rules") IsNot Nothing Then
					os = i("rules")(0)("os")("name").ToString()
				End If
				queue.Enqueue(New Libraries(pth, sha1, size, url, name, os))
			Next

			total_libraries = queue.Count
			downloaded_libraries = 0

			Dim tasks(Config.download_thread_count) As Task
			For t = 0 To Config.download_thread_count
				tasks(t) = Task.Run(Function() pull_libraries_worker(queue))
			Next

			Await Task.WhenAll(tasks)
			Console.WriteLine("[libraries] Complete.")
		End Function

		Public Async Function pull_libraries_worker(ByVal queue As ConcurrentQueue(Of Libraries)) As Task
			Dim library = New Libraries()

			While Not queue.IsEmpty
				If queue.TryDequeue(library) AndAlso
					Not library.is_empty AndAlso library.is_target_os(Config.os) Then
					Dim pth = libraries_pth & library.path
					Dim count = 0
					While True
						count += 1
						Dim has_error As Boolean = False
						Try
							Await Fetch.save_web_stream(library.url, pth)
						Catch ex As Exception
							has_error = True
							Console.WriteLine($"[libraries] { library.name }: {ex}! Retry { count }.")
						End Try
						If has_error Then
							Await Task.Delay(1000)
							Continue While
						End If

						If Not SHA1(pth, library.sha1) Then
							Await Task.Delay(1000)
							Console.WriteLine($"[libraries] { library.name }: SHA1({ library.sha1 }) inconsistent! Retry { count }.")
							Continue While
						End If

						If count > Config.error_retry_count Then
							Throw New TimeoutException("[libraries] Download Time Out.")
						End If

						Exit While
					End While

					Dim current = Interlocked.Increment(downloaded_libraries)
					RaiseEvent on_progress(Me, New Utility.Interface.ProgressArgs(
						total_libraries, current, library.name, "libraries"))
				Else
					Console.WriteLine($"[libraries] { library.name }: Not Match Current OS. Skip.")
				End If
			End While
		End Function

		Public Async Function pull_assets(ByVal json As Newtonsoft.Json.Linq.JObject) As Task
			Dim pth = $"{assets_manifest}{json("assets")}.json"
			If Not File.Exists(pth) Then
				Await Fetch.save_web_stream(json("assetIndex")("url").ToString(), pth)
			End If
			Console.WriteLine($"[Assets] {json("assets")} manifest: Downloaded.")

			Dim obj = CType(Jsn.to_json(File.ReadAllText(pth)), Newtonsoft.Json.Linq.JObject)
			obj = CType(obj("objects"), Newtonsoft.Json.Linq.JObject)

			Dim queue = New ConcurrentQueue(Of Assets)()
			For Each i As Newtonsoft.Json.Linq.JProperty In obj.Properties()
				Dim path = i.Name.ToString()
				Dim hash = i.Value("hash").ToString()
				Dim size = i.Value("size").ToString()
				queue.Enqueue(New Assets(path, hash, size))
			Next

			total_assets = queue.Count
			downloaded_assets = 0

			Dim tasks(Config.download_thread_count) As Task
			For t = 0 To Config.download_thread_count
				tasks(t) = Task.Run(Function() pull_assets_worker(queue))
			Next

			Await Task.WhenAll(tasks)
			Console.WriteLine("[Assets] Complete.")
		End Function

		Public Async Function pull_assets_worker(ByVal queue As ConcurrentQueue(Of Assets)) As Task
			Dim asset = New Assets()

			While Not queue.IsEmpty
				If queue.TryDequeue(asset) AndAlso Not asset.is_empty Then
					Dim pth = $"{assets_pth}{asset.hash.Substring(0, 2)}/{asset.hash}"
					Dim url = $"{Config.url.domain.mojang_res}{asset.hash.Substring(0, 2)}/{asset.hash}"
					Dim count = 0
					While True
						count += 1
						Dim has_error As Boolean = False
						Try
							Await Fetch.save_web_stream(url, pth)
						Catch ex As Exception
							has_error = True
							Console.WriteLine($"[Assets] { asset.path }: {ex}! Retry { count }.")
						End Try
						If has_error Then
							Await Task.Delay(1000)
							Continue While
						End If

						If Not SHA1(pth, asset.hash) Then
							Console.WriteLine($"[Assets] { asset.path }: SHA1({ asset.hash }) inconsistent! Retry { count }.")
							Await Task.Delay(1000)
							Continue While
						End If

						If count > Config.error_retry_count Then
							Throw New TimeoutException("[Assets] Download Time Out.")
						End If

						Exit While
					End While

					Dim current = Interlocked.Increment(downloaded_assets)
					RaiseEvent on_progress(Me, New Utility.Interface.ProgressArgs(
						total_assets, current, asset.path, "assets"))
				Else
					Console.WriteLine($"[Assets] { asset.path }: Not Match Current OS. Skip.")
				End If
			End While
		End Function

		Public Async Function install() As Task
			If Not File.Exists(manifest_pth) Then Await pull_manifest()
			Dim str = File.ReadAllText(manifest_pth)
			Dim obj = CType(Jsn.to_json(str), Newtonsoft.Json.Linq.JObject)
			If Not File.Exists(vanilla) Then Await pull_vanilla(obj)
			If check_libraries() Then Await pull_libraries(obj)
			If check_assets() Then Await pull_assets(obj)
			RaiseEvent on_complete(Me, New Utility.Interface.ProgressArgs(0, 0, "Installation complete!", "done"))
		End Function

		Public Function check_libraries() As Boolean
			Return True
		End Function

		Public Function check_assets() As Boolean
			Return True
		End Function

	End Class

End Namespace
