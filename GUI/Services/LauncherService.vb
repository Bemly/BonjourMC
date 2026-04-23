Option Explicit On
Option Strict On

Imports System
Imports System.Diagnostics
Imports System.IO
Imports System.Collections.Generic
Imports System.Threading.Tasks
Imports Avalonia.Threading
Imports Launcher
Imports Launcher.Utility.Model.Mojang

Namespace Services

    Public Class LauncherService
        Private _current_setup As Java.Client.Setup = Nothing
        Private _is_game_running As Boolean = False
        Private _cached_manifest As List(Of VersionEntry) = Nothing

        ' 事件
        Public Event on_download_progress(sender As Object, e As Launcher.Utility.Interface.ProgressArgs)
        Public Event on_game_output(sender As Object, line As String)
        Public Event on_game_error(sender As Object, line As String)
        Public Event on_game_exit(sender As Object, exit_code As Integer)

        ''' <summary>
        ''' 加载版本清单
        ''' </summary>
        Public Async Function load_version_manifest() As Task(Of List(Of VersionEntry))
            Debug.WriteLine("[LauncherService] load_version_manifest: start")
            If _cached_manifest IsNot Nothing Then Return _cached_manifest

            Dim manifests_pth = Path.GetFullPath(Config.file.mc, Environment.CurrentDirectory) &
                "versions/version_manifest_v2.json"
            Dim manifests_url = Config.url.domain.mojang_v2 & Config.url.version_manifest.mojang_v2

            If Not File.Exists(manifests_pth) Then
                Await Utility.Bridge.Download.save_web_stream(manifests_url, manifests_pth)
            End If

            Dim str = File.ReadAllText(manifests_pth)
            Dim obj = CType(Utility.Bridge.Json.to_json(str), Newtonsoft.Json.Linq.JObject)
            Dim versions = New List(Of VersionEntry)()

            For Each item In obj("versions")
                versions.Add(New VersionEntry(
                    item("id").ToString(),
                    item("type").ToString(),
                    item("url").ToString(),
                    item("releaseTime").ToString()
                ))
            Next

            _cached_manifest = versions
            Debug.WriteLine($"[LauncherService] load_version_manifest: loaded {versions.Count} versions")
            Return versions
        End Function

        ''' <summary>
        ''' 刷新版本清单缓存
        ''' </summary>
        Public Async Function refresh_version_manifest() As Task(Of List(Of VersionEntry))
            Debug.WriteLine("[LauncherService] refresh_version_manifest: clearing cache")
            _cached_manifest = Nothing
            Return Await load_version_manifest()
        End Function

        ''' <summary>
        ''' 获取已安装版本列表
        ''' </summary>
        Public Function get_installed_versions() As List(Of String)
            Debug.WriteLine("[LauncherService] get_installed_versions: scanning")
            Dim versions = New List(Of String)()
            Dim versions_dir = Path.GetFullPath(Config.file.mc, Environment.CurrentDirectory) & "versions/"
            If Directory.Exists(versions_dir) Then
                Dim dirs() As String = Directory.GetDirectories(versions_dir)
                For Each d As String In dirs
                    Dim name As String = Path.GetFileName(d)
                    If File.Exists(Path.Combine(d, name & ".jar")) Then
                        versions.Add(name)
                    End If
                Next
            End If
            Debug.WriteLine($"[LauncherService] get_installed_versions: found {versions.Count}")
            Return versions
        End Function

        ''' <summary>
        ''' 检查版本是否已安装
        ''' </summary>
        Public Function is_version_installed(ByVal version As String) As Boolean
            Dim versions_dir = Path.GetFullPath(Config.file.mc, Environment.CurrentDirectory) & "versions/"
            Return File.Exists(Path.Combine(versions_dir, version, version & ".jar"))
        End Function

        ''' <summary>
        ''' 下载指定版本
        ''' </summary>
        Public Async Function download_version(ByVal version As String) As Task
            Debug.WriteLine($"[LauncherService] download_version: start, version={version}")
            Dim mc As New Java.Client.Download.Mojang.Minecraft(is_compatible_mode:=True)

            AddHandler mc.on_progress, Sub(sender, e)
                                           Dispatcher.UIThread.Post(Sub() RaiseEvent on_download_progress(Me, e))
                                       End Sub
            AddHandler mc.on_complete, Sub(sender, e)
                                           Dispatcher.UIThread.Post(Sub() RaiseEvent on_download_progress(Me, e))
                                       End Sub

            mc.set_version(version)
            Await mc.install()
            Debug.WriteLine($"[LauncherService] download_version: completed for {version}")
        End Function

        ''' <summary>
        ''' 删除指定版本
        ''' </summary>
        Public Sub delete_version(ByVal version As String)
            Debug.WriteLine($"[LauncherService] delete_version: version={version}")
            Dim version_dir = Path.GetFullPath(Config.file.mc, Environment.CurrentDirectory) &
                "versions/" & version
            If Directory.Exists(version_dir) Then
                Directory.Delete(version_dir, True)
                Debug.WriteLine($"[LauncherService] delete_version: deleted {version_dir}")
            Else
                Debug.WriteLine($"[LauncherService] delete_version: directory not found")
            End If
        End Sub

        ''' <summary>
        ''' 启动游戏
        ''' </summary>
        Public Async Function launch_game(ByVal username As String, ByVal version As String,
                                          Optional ByVal memory_mb As Integer = 4096,
                                          Optional ByVal java_path As String = Nothing) As Task
            Debug.WriteLine($"[LauncherService] launch_game: user={username}, ver={version}, mem={memory_mb}MB")
            If _is_game_running Then
                Debug.WriteLine("[LauncherService] launch_game: already running, skipping")
                Return
            End If

            _current_setup = New Java.Client.Setup()
            _current_setup.set_username(username).set_version(version).set_memory(memory_mb)

            If java_path IsNot Nothing Then
                _current_setup.set_java_path(java_path)
            End If

            AddHandler _current_setup.on_stdout, Sub(sender, line)
                                                     Dispatcher.UIThread.Post(Sub() RaiseEvent on_game_output(Me, line))
                                                 End Sub
            AddHandler _current_setup.on_stderr, Sub(sender, line)
                                                     Dispatcher.UIThread.Post(Sub() RaiseEvent on_game_error(Me, line))
                                                 End Sub
            AddHandler _current_setup.on_exit, Sub(sender, exit_code)
                                                   _is_game_running = False
                                                   Dispatcher.UIThread.Post(Sub() RaiseEvent on_game_exit(Me, exit_code))
                                               End Sub

            _is_game_running = True
            Await _current_setup.launch()
        End Function

        ''' <summary>
        ''' 终止游戏
        ''' </summary>
        Public Sub kill_game()
            Debug.WriteLine("[LauncherService] kill_game: killing")
            If _current_setup IsNot Nothing Then
                _current_setup.kill()
                Debug.WriteLine("[LauncherService] kill_game: sent kill signal")
            Else
                Debug.WriteLine("[LauncherService] kill_game: no setup to kill")
            End If
        End Sub

        Public ReadOnly Property is_game_running As Boolean
            Get
                Return _is_game_running
            End Get
        End Property
    End Class

End Namespace
