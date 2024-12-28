
Option Explicit On
Option Strict On

Imports System
Imports System.Text.Json
Imports System.Net.Http
Imports System.Text.Json.Serialization
Imports System.Threading.Tasks
Imports System.Collections.Generic
Imports System.IO


''' <summary>
''' 下载🀄️的jar包
''' </summary>
''' <param name="id"></param>
''' <returns></returns>
Namespace Java.Client.Download.Mojang

    Public Class Minecraft
        Public Sub New()
            Console.WriteLine((New Data_provider).get_path)
        End Sub
    End Class

    Friend Class Data_provider
        ' 外部网络
        Private Class Internet
            Dim url As String = Config.url.domain.mojang_v2 & Config.url.version_manifest.mojang_v2
            Dim str As String = get_json_from_url(url).Result


            Public Sub New()
            End Sub

            ''' <summary>
            ''' 从指定的 URL 获取 JSON 数据
            ''' </summary>
            ''' <param name="url">目标网址</param>
            ''' <returns>JSON 字符串</returns>
            Async Function get_json_from_url(url As String) As Task(Of String)
                Using client As New HttpClient()
                    Dim response As HttpResponseMessage = Await client.GetAsync(url)
                    response.EnsureSuccessStatusCode()
                    Return Await response.Content.ReadAsStringAsync()
                End Using
            End Function
        End Class

        ' 本地暂时
        Private Class Local
            Private workpth As String = Path.GetFullPath("../../../../Launcher/Res/tmp/test")
            Private filepth As String = Path.GetFullPath("../../../../Launcher/Res/1.21.4.json")

            Public Function get_json() As String
                ' 读取文件内容
                If File.Exists(filepth) Then
                    Dim jsonContent As String = File.ReadAllText(filepth)
                    Console.WriteLine(jsonContent)
                    Return jsonContent
                Else
                    Throw New Exception($"文件未找到: {filepth}")
                End If
            End Function
        End Class

        Public Sub New()
        End Sub

        ' 提供器
        Public Function get_path() As String
            Dim local = New Local()
            Return local.get_json()
        End Function
    End Class
End Namespace


