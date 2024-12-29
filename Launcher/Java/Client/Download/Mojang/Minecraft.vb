
Option Explicit On
Option Strict On

Imports System
Imports System.Net.Http
Imports System.Threading.Tasks
Imports System.Collections.Generic
Imports System.IO
Imports Version = Launcher.Utility.Model.Version
Imports J = Launcher.Utility.Bridge.Json


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

        ' JSON mode
        Private manifests_json As J

        Public Sub New(Optional is_compatible_mode As Boolean = False)
            Me.is_compatible_mode = is_compatible_mode
        End Sub

        Public Function switch_mode(is_compatible_mode As Boolean) As Minecraft
            Throw New NotImplementedException("还不提供转换格式捏🤏")
            Me.is_compatible_mode = is_compatible_mode
            Return Me
        End Function

        Public Function set_version(version As Version) As Minecraft
            Me.version = version
            Return Me
        End Function

        Public Sub update_manifests()
            Dim workpth As String = Path.GetFullPath("../../../../Launcher/Res/tmp/test")
            Dim filepth As String = Path.GetFullPath("../../../../Launcher/Res/tmp/1.21.4.json")

            Dim content As String = fetch_web_content(Config.url.domain.mojang_v2 & Config.url.version_manifest.mojang_v2).Result
            If is_json(content) Then save_file(content, filepth)
        End Sub

        Public Sub update_manifest()
            'Dim dict As Dictionary(Of String, JsonElement) = JsonSerializer.Deserialize(Of Dictionary(Of String, JsonElement))(str)
        End Sub

        ''' <summary>
        ''' 异步
        ''' 从指定的 URL 获取数据
        ''' </summary>
        ''' <param name="url">目标网址</param>
        ''' <returns>JSON 字符串的异步任务</returns>
        Private Async Function fetch_web_content(url As String) As Task(Of String)
            Using client As New HttpClient()
                Dim response As HttpResponseMessage = Await client.GetAsync(url)
                response.EnsureSuccessStatusCode()
                Return Await response.Content.ReadAsStringAsync()
            End Using
        End Function

        ''' <summary>
        ''' 同步 不建议使用
        ''' 判断 字符串 是否是 JSON
        ''' </summary>
        ''' <param name="str">目标字符串</param>
        ''' <returns>JSON 字符串</returns>
        Private Function is_json(str As String) As Boolean
            'Try
            '    JsonDocument.Parse(str)
            '    Return True
            'Catch ex As JsonException
            '    Return False
            'End Try
        End Function

        ''' <summary>
        ''' 异步
        ''' 存放文件
        ''' </summary>
        ''' <param name="str">目标字符串</param>
        ''' <returns>JSON 字符串</returns>
        Private Async Sub save_file(str As String, pth As String)
            Dim dict As String = Path.GetDirectoryName(pth)
            If Not Directory.Exists(dict) Then Directory.CreateDirectory(dict)
            Await File.WriteAllTextAsync(pth, str)
        End Sub
    End Class

    Friend Class Provider
        Private Class Json_adapter

        End Class

        Private Class Sqlite_adapter

        End Class

    End Class

End Namespace


