
Option Explicit On
Option Strict On

Imports System
Imports System.Net.Http
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
		Private libraries_pth As String
		Private manifests_pth As String
		Private manifests_url As String
		Private manifest_pth As String
		Private assets_pth As String
		Private assets_manifest As String
		Private vanilla As String

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
			' [fix] 赋值version之后才绑定
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

		Public Sub pull_manifests()
			Fetch.save_web_stream(manifests_url, manifests_pth).Wait()
		End Sub

		Public Sub pull_manifest()
			' 前置条件：访问历史版本清单
			If Not File.Exists(manifests_pth) Then pull_manifests()
			Dim str = File.ReadAllText(manifests_pth)
			' TODO: 此处关系有问题，需要重新画图😭或者自己实现一套
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
			' Not Normal Producer-Consumer Model.
			' 生产者填充队列
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

			' 消费者处理队列
			Dim tasks(Config.download_thread_count) As Task
			For t = 0 To Config.download_thread_count
				tasks(t) = Task.Run(Function() pull_libraries(queue))
			Next

			Task.WaitAll(tasks)

			Console.WriteLine("[libraries] Complete.")
		End Sub

		' TODO: 之后会和Download Bridge 合并 避免 异步->同步->异步
		Public Async Function pull_libraries(ByVal queue As ConcurrentQueue(Of Libraries)) As Task
			Dim library = New Libraries()

			While Not queue.IsEmpty
				' 短路左侧可以修改右侧的引用值
				If queue.TryDequeue(library) AndAlso
					Not library.is_empty AndAlso library.is_target_os(Config.os) Then
					Dim pth = libraries_pth & library.path ' TODO: Use Path.Combine
					Dim count = 0
					While True
						count += 1
						' 标记玄学错误
						Try
							Await Fetch.save_web_stream(library.url, pth)
						Catch ex As Exception
							Task.Delay(1000).Wait()
							Console.WriteLine($"[libraries] { library.name }: {ex}! Retry { count }.")
							Continue While
						End Try

						' 下载有误
						If Not SHA1(pth, library.sha1) Then
							Await Task.Delay(1000)
							Console.WriteLine($"[libraries] { library.name }: SHA1({ library.sha1 }) inconsistent! Retry { count }.")
							Continue While
						End If

						' 重试次数过多
						' TODO: 暂未实现错误处理 后续丢给 Bridge.Download
						If count > Config.error_retry_count Then
							Throw New TimeoutException("[libraries] Download Time Out.")
						End If

						' 成功结束
						Exit While
					End While
					'Console.WriteLine($"[libraries] { library.name }: Downloaded.")
				Else
					Console.WriteLine($"[libraries] { library.name }: Not Match Current OS. Skip.")
				End If
			End While
		End Function

		Public Sub pull_assets(ByVal json As Newtonsoft.Json.Linq.JObject)
			' 下载资产清单文件
			Dim pth = $"{assets_manifest}{json("assets")}.json"
			If Not File.Exists(pth) Then
				Fetch.save_web_stream(json("assetIndex")("url").ToString(), pth).Wait()
			End If
			Console.WriteLine($"[Assets] {json("assets")} manifest: Downloaded.")

			' 拉取资产清单文件
			Dim obj = CType(Jsn.to_json(File.ReadAllText(pth)), Newtonsoft.Json.Linq.JObject)
			obj = CType(obj("objects"), Newtonsoft.Json.Linq.JObject)

			Dim queue = New ConcurrentQueue(Of Assets)()
			For Each i As Newtonsoft.Json.Linq.JProperty In obj.Properties()
				Dim path = i.Name.ToString()
				Dim hash = i.Value("hash").ToString()
				Dim size = i.Value("size").ToString()
				queue.Enqueue(New Assets(path, hash, size))
			Next

			' 下载清单内容
			Dim tasks(Config.download_thread_count) As Task
			For t = 0 To Config.download_thread_count
				tasks(t) = Task.Run(Function() pull_assets(queue))
			Next

			Task.WaitAll(tasks)

			Console.WriteLine("[Assets] Complete.")
		End Sub

		Public Async Function pull_assets(ByVal queue As ConcurrentQueue(Of Assets)) As Task
			Dim asset = New Assets()

			While Not queue.IsEmpty
				' 短路左侧可以修改右侧的引用值
				If queue.TryDequeue(asset) AndAlso Not asset.is_empty Then
					Dim pth = $"{assets_pth}{asset.hash.Substring(0, 2)}/{asset.hash}"
					Dim url = $"{Config.url.domain.mojang_res}{asset.hash.Substring(0, 2)}/{asset.hash}"
					Dim count = 0
					While True
						count += 1
						' 标记玄学错误
						Try
							Await Fetch.save_web_stream(url, pth)
						Catch ex As Exception
							Console.WriteLine($"[Assets] { asset.path }: {ex}! Retry { count }.")
							Task.Delay(1000).Wait()
							Continue While
						End Try

						' 下载有误
						If Not SHA1(pth, asset.hash) Then
							Console.WriteLine($"[Assets] { asset.path }: SHA1({ asset.hash }) inconsistent! Retry { count }.")
							Await Task.Delay(1000)
							Continue While
						End If

						' 重试次数过多
						' TODO: 暂未实现错误处理 后续丢给 Bridge.Download
						If count > Config.error_retry_count Then
							Throw New TimeoutException("[Assets] Download Time Out.")
						End If

						' 成功结束
						Exit While
					End While
					'Console.WriteLine($"[Assets] { asset.path }: Downloaded.")
				Else
					Console.WriteLine($"[Assets] { asset.path }: Not Match Current OS. Skip.")
				End If
			End While
		End Function

		Public Sub install()
			' 前置条件：访问当前版本清单
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


