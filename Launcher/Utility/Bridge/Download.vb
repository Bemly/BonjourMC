
Option Explicit On
Option Strict On

Imports System
Imports System.Net.Http
Imports System.IO
Imports System.Threading.Tasks
Imports I_dl = Launcher.Utility.Interface.Download

Namespace Utility.Bridge

	' 1 System.Net.Http, 2 Bemly.Net, 3 PCL download, 4 ...
	' 封装核心适配器📦
	Public Class Download
		Implements I_dl

		Private Shared instance As I_dl
		Private Shared mode As String = Launcher.Config.api.net_mode

		' 首次加载也有线程安全 gettype具有唯一性
		Shared Sub New()
			SyncLock GetType(Download)
				If instance Is Nothing Then
					Select Case mode
						Case "System"
							instance = System_net_adapter.Instance
						Case "Bemly", "Bemly", "PCL"
							Throw New NotImplementedException("Not Found Bemly.Net Adapter.")
						Case Else
							Throw New NotImplementedException("进不去。怎么想都进不去吧！")
					End Select
				End If
			End SyncLock
		End Sub

		Shared Function save_web_stream(url As String, pth As String) As Task
			Return instance.save_web_stream(url, pth)
		End Function

		Public Function save_web_stream_inst(url As String, pth As String) As Task Implements I_dl.save_web_stream
			Return instance.save_web_stream(url, pth)
		End Function



		' ****** Adapter Segment ******
		Private NotInheritable Class System_net_adapter
			Implements I_dl

			' ==== Singleton Layer ====
			Shared ReadOnly m_instance As New System_net_adapter()
			Shared Sub New()
			End Sub
			Private Sub New()
			End Sub

			Friend Shared ReadOnly Property Instance As System_net_adapter
				Get
					Return m_instance
				End Get
			End Property
			' == Singleton Layer End ==


			''' <summary>
			''' 异步多线程获取 URL 数据，确保 200 时 放入指定路径(流下载,不直接读取)
			''' </summary>
			''' <param name="url">网站地址</param>
			''' <param name="pth">本地路径</param>
			''' <returns>异步返回</returns>
			Friend Async Function save_web_stream(url As String, pth As String) As Task Implements I_dl.save_web_stream
				Using client As New HttpClient()
					' 获取远程文件流
					Using response As HttpResponseMessage =
						Await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead)
						response.EnsureSuccessStatusCode()

						' 确保目标目录存在
						Dim dict As String = Path.GetDirectoryName(pth)
						If Not Directory.Exists(dict) Then Directory.CreateDirectory(dict)

						' 读取文件流并保存到本地
						Using remoteStream As Stream = Await response.Content.ReadAsStreamAsync(),
							localStream As FileStream = File.Create(pth)
							Await remoteStream.CopyToAsync(localStream)
						End Using
					End Using
				End Using
			End Function
		End Class

	End Class


End Namespace


