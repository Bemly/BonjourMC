
Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Concurrent
Imports System.IO
Imports System.Threading.Tasks
Imports System.Diagnostics
Imports Launcher.Utility.Model.Mojang.Minecraft
Imports Jsn = Launcher.Utility.Bridge.Json

Namespace Java.Client

    Public Class Setup

        ' 可配置参数
        Private prop_username As String = Config.settings.default_username
        Private prop_version_string As String = "1.21.4"
        Private prop_version As Version
        Private prop_memory_mb As Integer = Config.settings.default_memory_mb
        Private prop_java_path As String = Nothing
        Private prop_game_dir As String = Nothing
        Private prop_window_width As Integer = Config.settings.default_window_width
        Private prop_window_height As Integer = Config.settings.default_window_height

        ' 进程引用
        Private game_process As Process = Nothing

        ' 事件
        Public Event on_stdout(sender As Object, line As String)
        Public Event on_stderr(sender As Object, line As String)
        Public Event on_exit(sender As Object, exit_code As Integer)

        ' Fluent API
        Public Function set_username(ByVal name As String) As Setup
            Me.prop_username = name
            Return Me
        End Function

        Public Function set_version(ByVal v As String) As Setup
            Me.prop_version_string = v
            Me.prop_version = New Version(v)
            Return Me
        End Function

        Public Function set_version(ByVal major As Integer, ByVal minor As Integer,
                                    ByVal patch As Integer) As Setup
            Me.prop_version_string = $"{major}.{minor}.{patch}"
            Me.prop_version = New Version(major, minor, patch)
            Return Me
        End Function

        Public Function set_memory(ByVal mb As Integer) As Setup
            Me.prop_memory_mb = mb
            Return Me
        End Function

        Public Function set_java_path(ByVal p As String) As Setup
            Me.prop_java_path = p
            Return Me
        End Function

        Public Function set_game_dir(ByVal d As String) As Setup
            Me.prop_game_dir = d
            Return Me
        End Function

        Public Function set_window(ByVal w As Integer, ByVal h As Integer) As Setup
            Me.prop_window_width = w
            Me.prop_window_height = h
            Return Me
        End Function

        ''' <summary>
        ''' 启动 Minecraft
        ''' </summary>
        Public Async Function launch() As Task
            Dim version_str = prop_version_string
            If prop_version Is Nothing Then prop_version = New Version(version_str)

            Dim location = Path.GetFullPath(Config.file.mc, Environment.CurrentDirectory)
            If prop_game_dir IsNot Nothing Then location = prop_game_dir

            Dim libraries_pth = location & "libraries/"
            Dim vanilla = location & $"versions/{version_str}/{version_str}.jar"
            Dim manifest = location & $"versions/{version_str}/{version_str}.json"
            Dim native = location & $"versions/{version_str}/natives-osx-x86_64/"
            Dim assetsDir = location & "assets/"

            Dim arr = CType(Jsn.to_json(File.ReadAllText(manifest)), Newtonsoft.Json.Linq.JObject)("arguments")
            Dim game = CType(arr("game"), Newtonsoft.Json.Linq.JArray)
            Dim jvm = CType(arr("jvm"), Newtonsoft.Json.Linq.JArray)

            Dim username = prop_username
            Dim version = prop_version
            Dim gamedir = location
            Dim assetIndex = 19
            Dim uuid = "1dd96749f6dd358fbd2aff1e24497102"
            Dim accessToken = "70e7494f51d94bebbae334bb122090c8"
            Dim versionType = "BonjourMC 0.1"
            Dim userType = "msa"

            Dim cparg = " -cp "
            Dim obj = CType(Jsn.to_json(File.ReadAllText(manifest)), Newtonsoft.Json.Linq.JObject)
            For Each i In obj("libraries")
                Dim pth = i("downloads")("artifact")("path").ToString()
                Dim os = ""
                If i("rules") IsNot Nothing Then
                    os = i("rules")(0)("os")("name").ToString()
                End If
                If os = "" OrElse os = "osx" Then
                    cparg &= $"{libraries_pth}{pth}:"
                End If
            Next

            Dim java_agr = Path.GetFullPath(Config.file.java)
            If prop_java_path IsNot Nothing Then java_agr = prop_java_path

            Dim launcher_agr = " -Dfile.encoding=UTF-8"
            launcher_agr &= " -Dstdout.encoding=UTF-8"
            launcher_agr &= " -Djava.rmi.server.useCodebaseOnly=true"
            launcher_agr &= " -Dcom.sun.jndi.rmi.object.trustURLCodebase=false"
            launcher_agr &= " -Dcom.sun.jndi.cosnaming.object.trustURLCodebase=false"
            launcher_agr &= " -Dlog4j2.formatMsgNoLookups=true"

            launcher_agr &= $" -Xdock:name=""Minecraft {version_str}"""
            launcher_agr &= $" -Xdock:icon={assetsDir}objects/f0/f00657542252858a721e715a2e888a9226404e35"
            launcher_agr &= $" -Duser.home={Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}"

            launcher_agr &= " -XstartOnFirstThread"
            launcher_agr &= $" -Xmx{prop_memory_mb}m"
            launcher_agr &= $" -Xms{prop_memory_mb}m"

            launcher_agr &= " -XX:+UnlockExperimentalVMOptions"
            launcher_agr &= " -XX:+UseG1GC"
            launcher_agr &= " -XX:G1NewSizePercent=20"
            launcher_agr &= " -XX:G1ReservePercent=20"
            launcher_agr &= " -XX:MaxGCPauseMillis=50"
            launcher_agr &= " -XX:G1HeapRegionSize=32m"
            launcher_agr &= " -XX:-UseAdaptiveSizePolicy"
            launcher_agr &= " -XX:-OmitStackTraceInFastThrow"
            launcher_agr &= " -XX:-DontCompileHugeMethods"

            launcher_agr &= " -Dfml.ignoreInvalidMinecraftCertificates=false"
            launcher_agr &= " -Dfml.ignorePatchDiscrepancies=false"

            launcher_agr &= " -Dminecraft.launcher.brand=BonjourMC"
            launcher_agr &= " -Dminecraft.launcher.version=0.1"

            launcher_agr &= $" -Djava.library.path={native}"
            launcher_agr &= $" -Djna.tmpdir={native}"
            launcher_agr &= $" -Dorg.lwjgl.system.SharedLibraryExtractPath={native}"
            launcher_agr &= $" -Dio.netty.native.workdir={native}"

            launcher_agr &= cparg
            launcher_agr &= $"{ vanilla } net.minecraft.client.main.Main"

            launcher_agr &= $" --username {username}"
            launcher_agr &= $" --version {version}"
            launcher_agr &= $" --gameDir {gamedir}"
            launcher_agr &= $" --assetsDir {assetsDir}"
            launcher_agr &= $" --assetIndex {assetIndex}"
            launcher_agr &= $" --uuid {uuid}"
            launcher_agr &= $" --accessToken {accessToken}"
            launcher_agr &= $" --clientId ${{clientId}}"
            launcher_agr &= $" --xuid ${{xuid}}"
            launcher_agr &= $" --userType {userType}"
            launcher_agr &= $" --versionType {versionType}"

            Console.WriteLine(java_agr & launcher_agr)

            game_process = New Process()
            game_process.StartInfo.FileName = java_agr
            game_process.StartInfo.Arguments = launcher_agr
            game_process.StartInfo.RedirectStandardOutput = True
            game_process.StartInfo.RedirectStandardError = True
            game_process.StartInfo.UseShellExecute = False
            game_process.StartInfo.CreateNoWindow = True

            AddHandler game_process.OutputDataReceived, AddressOf OutputHandler
            AddHandler game_process.ErrorDataReceived, AddressOf ErrorHandler

            Try
                game_process.Start()
                game_process.BeginOutputReadLine()
                game_process.BeginErrorReadLine()

                Await Task.Run(Sub() game_process.WaitForExit())

                RaiseEvent on_exit(Me, game_process.ExitCode)

                RemoveHandler game_process.OutputDataReceived, AddressOf OutputHandler
                RemoveHandler game_process.ErrorDataReceived, AddressOf ErrorHandler
            Catch ex As Exception
                Console.WriteLine("Error: " & ex.Message)
                RaiseEvent on_stderr(Me, "Error: " & ex.Message)
            End Try
        End Function

        ''' <summary>
        ''' 终止游戏进程
        ''' </summary>
        Public Sub kill()
            If game_process IsNot Nothing AndAlso Not game_process.HasExited Then
                game_process.Kill()
            End If
        End Sub

        Public ReadOnly Property is_running As Boolean
            Get
                Return game_process IsNot Nothing AndAlso Not game_process.HasExited
            End Get
        End Property

        Private Sub OutputHandler(sender As Object, e As DataReceivedEventArgs)
            If e.Data IsNot Nothing Then
                RaiseEvent on_stdout(Me, e.Data)
            End If
        End Sub

        Private Sub ErrorHandler(sender As Object, e As DataReceivedEventArgs)
            If e.Data IsNot Nothing Then
                RaiseEvent on_stderr(Me, e.Data)
            End If
        End Sub
    End Class
End Namespace
