
Option Explicit On
Option Strict On

Imports System
Imports System.Net.Http
Imports System.IO
Imports System.Threading.Tasks

Namespace Utility.Bridge

	' 1 System.Net.Http, 2 Bemly.Net, 3 PCL download, 4 ...
	Public Class Download



		Shared Async Function asynchronous(url As String, destinationPath As String) As Task
			Using client As New HttpClient()
				' 获取远程文件流
				Using response As HttpResponseMessage = Await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead)
					response.EnsureSuccessStatusCode()

					' 确保目标目录存在
					Dim dict As String = Path.GetDirectoryName(destinationPath)
					If Not Directory.Exists(dict) Then
						Directory.CreateDirectory(dict)
					End If

					' 读取文件流并保存到本地
					Using remoteStream As Stream = Await response.Content.ReadAsStreamAsync(),
					  localStream As FileStream = File.Create(destinationPath)
						Await remoteStream.CopyToAsync(localStream)
					End Using
				End Using
			End Using
		End Function

	End Class
End Namespace


