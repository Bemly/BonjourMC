
Option Explicit On
Option Strict On

Imports System
Imports System.IO


Public Module Entry
    Sub Point()
        Console.WriteLine("Core Start!")
		'Dim mc = New Java.Client.Download.Mojang.Minecraft(is_compatible_mode:=True)
		'mc.set_version(1, 21, 4).install()
		Dim mc = New Java.Client.Setup()

	End Sub
End Module


''' <summary>
''' 暂时的规范：
'''  函数和变量名 全小写， 用下划线分割(规避大小写的命名冲突)
'''  属性 get_*(Only Getter) set_*(Only Setter) prop_*(Both)
'''  类 结构  首字母大写 只允许一个单词 多出的单词创文件夹然后自动生成命名空间
'''  接口 I_*(Out_of_date) -> 存放到Utility.Interface下 首字母大写(now)
'''  命名空间 文件夹 首字母大写
'''
''' 分类(暂时)
''' - Bedrock
''' - Java
''' - ...
''' - Utility -> Interface / Models / Bridge(adapter)
''' TODO: 加入OpenAPI 和 vbnet的注释 相互 解析
''' </summary>
Public Class Core

End Class


' 沟槽的VB.NET变量竟然不区分大小写，我服了，全是命名冲突
' 常量命名是例外，均为全小写+下划线区分
Public Class Config

	Public Class url

		Public Class domain
			Public Const mojang_v1 As String = "https://piston-meta.mojang.com/"
			Public Const mojang_v2 As String = "https://launchermeta.mojang.com/"
			Public Const bmcl As String = "https://bmclapi2.bangbang93.com/"
			Public Const mojang_res As String = "https://resources.download.minecraft.net/"
		End Class


		Public Class version_manifest
			Public Const mojang_v1 As String = "mc/game/version_manifest.json"
			Public Const mojang_v2 As String = "mc/game/version_manifest_v2.json"
		End Class

	End Class

	' TODO: 之后用实例新建(Instance <- Models -> SQLite)
	Public Class file
		'Public Class path
		'	Public Const glob As String = "../../../../Launcher/Res/tmp/"
		'	Public Const java As String = glob & "java/"
		'End Class

		'Public Class mc
		'	Public Const path As String = file.path.java & "mc/"
		'End Class

		Public Const glob As String = "../../../../Launcher/Res/tmp/"
		Public Const java As String = glob & "jre.bundle/Contents/Home/bin/java"
		Public Const mc As String = glob & "mc/"
	End Class

	Public Class api

		' 1 System.Text.Json, 2 Json.Net.Core, 3 Newtonsoft.Json, 4 Bemly.Json
		Public Const json_mode As String = "Newtonsoft"

		' 1 System.Net.Http, 2 Bemly.Net, 3 PCL download, 4 ...
		Public Const net_mode As String = "System"

	End Class

	' TODO: 之后加上动态延时重连和动态线程下载
	Public Const download_thread_count = 128
	Public Const error_retry_count = 20
	Public Const os = "osx"

End Class
