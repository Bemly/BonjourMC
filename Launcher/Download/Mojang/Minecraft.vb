
Option Explicit On
Option Strict On

Imports System
Imports System.Text.Json
Imports System.Net.Http
Imports System.Text.Json.Serialization
Imports System.Threading.Tasks
Imports System.Collections.Generic


''' <summary>
''' 下载🀄️的jar包
''' </summary>
''' <param name="id"></param>
''' <returns></returns>
Namespace Download.Mojang
    Public Class Minecraft

        Dim url As String = "http://launchermeta.mojang.com/mc/game/version_manifest_v2.json"
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
End Namespace




