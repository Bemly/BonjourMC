
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

        Public Sub New()
            Dim location = Path.GetFullPath(Config.file.mc, Environment.CurrentDirectory)
            Dim libraries_pth = location & "libraries/"
            Dim vanilla = location & "versions/1.21.4/1.21.4.jar"
            Dim manifest = location & "versions/1.21.4/1.21.4.json"
            Dim native = location & "versions/1.21.4/natives-osx-x86_64/"
            Dim arr = CType(Jsn.to_json(File.ReadAllText(manifest)), Newtonsoft.Json.Linq.JObject)("arguments")
            Dim game = CType(arr("game"), Newtonsoft.Json.Linq.JArray)
            Dim jvm = CType(arr("jvm"), Newtonsoft.Json.Linq.JArray)

            Dim username = "bemly_"
            Dim version = New Version(1, 21, 4)
            Dim gamedir = location
            Dim assetsDir = location & "assets/"
            Dim assetIndex = 19
            Dim uuid = "1dd96749f6dd358fbd2aff1e24497102"
            Dim accessToken = "70e7494f51d94bebbae334bb122090c8"
            Dim versionType = "BonjourMC 0.1"
            Dim userType = "msa"

            ' --clientId ${clientid}
            '--xuid ${auth_xuid}
            '--userType msa --width 854 --height 480

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


            'Dim launcher_agr = Path.GetFullPath(Config.file.java)
            Dim java_agr = Path.GetFullPath(Config.file.java)

            ' ?
            Dim launcher_agr = " -Dfile.encoding=UTF-8"
            'launcher_agr &= " -Dfile.encoding=UTF-8"
            launcher_agr &= " -Dstdout.encoding=UTF-8"
            launcher_agr &= " -Djava.rmi.server.useCodebaseOnly=true"
            launcher_agr &= " -Dcom.sun.jndi.rmi.object.trustURLCodebase=false"
            launcher_agr &= " -Dcom.sun.jndi.cosnaming.object.trustURLCodebase=false"
            launcher_agr &= " -Dlog4j2.formatMsgNoLookups=true"
            'launcher_agr &= " -Dlog4j.configurationFile=/Users/bemly/Downloads/test/versions/1.21.4/log4j2.xml"

            ' 装饰🎍
            launcher_agr &= " -Xdock:name=""Minecraft 1.21.4"""
            launcher_agr &= $" -Xdock:icon={assetsDir}objects/f0/f00657542252858a721e715a2e888a9226404e35"
            launcher_agr &= " -Duser.home=/Users/bemly/Downloads"

            ' 内存分配
            launcher_agr &= " -XstartOnFirstThread"
            launcher_agr &= " -Xmx4096m"
            launcher_agr &= " -Xms4096m"

            '?
            launcher_agr &= " -XX:+UnlockExperimentalVMOptions"
            launcher_agr &= " -XX:+UseG1GC"

            '?
            launcher_agr &= " -XX:G1NewSizePercent=20"
            launcher_agr &= " -XX:G1ReservePercent=20"
            launcher_agr &= " -XX:MaxGCPauseMillis=50"
            launcher_agr &= " -XX:G1HeapRegionSize=32m"

            '?
            launcher_agr &= " -XX:-UseAdaptiveSizePolicy"
            launcher_agr &= " -XX:-OmitStackTraceInFastThrow"
            launcher_agr &= " -XX:-DontCompileHugeMethods"

            ' ?
            launcher_agr &= " -Dfml.ignoreInvalidMinecraftCertificates=false"
            launcher_agr &= " -Dfml.ignorePatchDiscrepancies=false"

            ' F3
            launcher_agr &= " -Dminecraft.launcher.brand=BonjourMC"
            launcher_agr &= " -Dminecraft.launcher.version=0.1"

            ' 还没搞懂这个 应该是LWJGL的库 版本对应关系还不太清楚
            '-Djava.library.path=/Users/bemly/Downloads/test/versions/1.21.4/natives-osx-x86_64
            '-Djna.tmpdir=/Users/bemly/Downloads/test/versions/1.21.4/natives-osx-x86_64
            '-Dorg.lwjgl.system.SharedLibraryExtractPath=/Users/bemly/Downloads/test/versions/1.21.4/natives-osx-x86_64
            '-Dio.netty.native.workdir=/Users/bemly/Downloads/test/versions/1.21.4/natives-osx-x86_64
            launcher_agr &= $" -Djava.library.path={native}"
            launcher_agr &= $" -Djna.tmpdir={native}"
            launcher_agr &= $" -Dorg.lwjgl.system.SharedLibraryExtractPath={native}"
            launcher_agr &= $" -Dio.netty.native.workdir={native}"

            ' 依赖
            launcher_agr &= cparg
            launcher_agr &= $"{ vanilla } net.minecraft.client.main.Main"

            ' 游戏参数
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
            'For i = 0 To 20 Step 2
            '	Console.WriteLine(game(i))
            '	launcher_agr += $" { game(i) } "
            'Next
            '--server 后接服务器地址，游戏进入时将直接连入服务器


            Console.WriteLine(java_agr & launcher_agr)


            ' 创建一个 Process 实例
            Dim process As New Process()

            ' 设置启动信息
            process.StartInfo.FileName = java_agr
            process.StartInfo.Arguments = launcher_agr
            process.StartInfo.RedirectStandardOutput = True
            process.StartInfo.RedirectStandardError = True
            process.StartInfo.UseShellExecute = False
            process.StartInfo.CreateNoWindow = True

            ' 设置事件处理程序以实时处理标准输出和标准错误输出
            AddHandler process.OutputDataReceived, AddressOf OutputHandler
            AddHandler process.ErrorDataReceived, AddressOf ErrorHandler

            ' 启动进程
            Try
                process.Start()

                ' 开始异步读取输出流
                process.BeginOutputReadLine()
                process.BeginErrorReadLine()

                ' 等待进程完成
                process.WaitForExit()

                ' 移除事件处理程序
                RemoveHandler process.OutputDataReceived, AddressOf OutputHandler
                RemoveHandler process.ErrorDataReceived, AddressOf ErrorHandler
            Catch ex As Exception
                Console.WriteLine("Error: " & ex.Message)
            End Try
        End Sub


        ' 标准输出处理程序
        Private Sub OutputHandler(sender As Object, e As DataReceivedEventArgs)
            If e.Data IsNot Nothing Then
                Console.WriteLine("Minecraft Output: " & e.Data)
            End If
        End Sub

        ' 错误输出处理程序
        Private Sub ErrorHandler(sender As Object, e As DataReceivedEventArgs)
            If e.Data IsNot Nothing Then
                Console.WriteLine("Minecraft Error: " & e.Data)
            End If
        End Sub
    End Class
End Namespace


