
Option Explicit On
Option Strict On

Imports System
Imports System.Net.Http
Imports System.Threading.Tasks
Imports System.Collections.Generic
Imports System.IO
Imports Version = Launcher.Utility.Model.Version
Imports Jsn = Launcher.Utility.Bridge.Json

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

        Public Function set_version(version As String) As Minecraft
            set_version(New Version(version))
            Return Me
        End Function

        Public Function set_version(major As Integer, minor As Integer, patch As Integer) As Minecraft
            set_version(New Version(major, minor, patch))
            Return Me
        End Function

        Public Sub update_manifests()
            Dim workpth As String = Path.GetFullPath("../../../../Launcher/Res/tmp/minecraft/versions/")
            Dim filepth As String = Path.GetFullPath("../../../../Launcher/Res/tmp/minecraft/versions/version_manifest_v2.json")

            Dim content As String = fetch_web_content(Config.url.domain.mojang_v2 & Config.url.version_manifest.mojang_v2).Result
            If Jsn.is_json(content) Then save_file(content, filepth)
        End Sub

        Public Sub update_manifest()
            Dim filepth As String = Path.GetFullPath("../../../../Launcher/Res/tmp/minecraft/versions/version_manifest_v2.json", Environment.CurrentDirectory)
            If Not File.Exists(filepth) Then update_manifests()
            Dim filestr As String = File.ReadAllText(filepth)
            ' TODO: 此处关系有问题，需要重新画图😭或者自己实现一套😠
            Dim arr = CType(Jsn.to_json(filestr), Newtonsoft.Json.Linq.JObject)("versions")
            Dim i = 0
            For Each item In arr
                If item("id").ToString() = version.ToString() Then Exit For
                i += 1
            Next
            Console.WriteLine(arr(i)("url"))
            save_file(
                fetch_web_content(arr(i)("url").ToString()).Result,
                Path.GetFullPath($"../../../../Launcher/Res/tmp/minecraft/versions/{ version.ToString() }/{ version.ToString() }.json"))
        End Sub

        Public Sub install()
            Dim filepth As String = Path.GetFullPath($"../../../../Launcher/Res/tmp/minecraft/versions/{ version.ToString() }/{ version.ToString() }.json")
            If Not File.Exists(filepth) Then update_manifest()
            Dim filestr As String = File.ReadAllText(filepth)
            Console.WriteLine(filepth)
            Dim filepth2 As String = Path.GetFullPath($"../../../../Launcher/Res/tmp/minecraft/versions/{ version.ToString() }/{ version.ToString() }.jar")
            If Not File.Exists(filepth2) Then
                Dim url As String = CType(Jsn.to_json(filestr), Newtonsoft.Json.Linq.JObject)("downloads")("client")("url").ToString()
                Console.WriteLine(url)
                DownloadFileAsync(url, Path.GetFullPath($"../../../../Launcher/Res/tmp/minecraft/versions/{ version.ToString() }/{ version.ToString() }.jar")).Wait()
            End If

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

        Public Async Function DownloadFileAsync(url As String, destinationPath As String) As Task
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

    Friend Class Provider
        Private Class Json_adapter

        End Class

        Private Class Sqlite_adapter

        End Class

    End Class

End Namespace


