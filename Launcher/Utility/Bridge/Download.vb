
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

		Shared Function save_web_stream(ByVal url As String, ByVal pth As String) As Task
			Return instance.save_web_stream(url, pth)
		End Function

		''' <summary>
		''' 带进度报告的下载
		''' </summary>
		Shared Function save_web_stream(ByVal url As String, ByVal pth As String,
				ByVal progress As IProgress(Of Long)) As Task
			Return DirectCast(instance, System_net_adapter).save_web_stream(url, pth, progress)
		End Function

		Public Function save_web_stream_inst(ByVal url As String,
				ByVal pth As String) As Task Implements I_dl.save_web_stream
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
			Friend Async Function save_web_stream(ByVal url As String, ByVal pth As String) As Task Implements I_dl.save_web_stream
				Using client As New HttpClient()
					Using response As HttpResponseMessage =
						Await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead)
						response.EnsureSuccessStatusCode()

						Dim dict As String = Path.GetDirectoryName(pth)
						If Not Directory.Exists(dict) Then Directory.CreateDirectory(dict)

						Using remoteStream As Stream = Await response.Content.ReadAsStreamAsync(),
							localStream As FileStream = File.Create(pth)
							Await remoteStream.CopyToAsync(localStream)
						End Using
					End Using
				End Using
			End Function

			''' <summary>
			''' 带进度报告的异步下载
			''' </summary>
			Friend Async Function save_web_stream(ByVal url As String, ByVal pth As String,
					ByVal progress As IProgress(Of Long)) As Task
				Using client As New HttpClient()
					Using response As HttpResponseMessage =
						Await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead)
						response.EnsureSuccessStatusCode()

						Dim dict As String = Path.GetDirectoryName(pth)
						If Not Directory.Exists(dict) Then Directory.CreateDirectory(dict)

						Using remoteStream As Stream = Await response.Content.ReadAsStreamAsync(),
							localStream As FileStream = File.Create(pth)
							Dim buffer(8191) As Byte
							Dim totalBytes As Long = 0
							Dim bytesRead As Integer
							Do
								bytesRead = Await remoteStream.ReadAsync(buffer, 0, buffer.Length)
								If bytesRead > 0 Then
									Await localStream.WriteAsync(buffer, 0, bytesRead)
									totalBytes += bytesRead
									progress?.Report(totalBytes)
								End If
							Loop While bytesRead > 0
						End Using
					End Using
				End Using
			End Function
		End Class

	End Class


End Namespace
