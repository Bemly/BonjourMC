
Option Explicit On
Option Strict On

Imports System
Imports System.IO

' 这个还没有用 openapi nuget包😊
Public Module Entry
    Sub Point()
        Console.WriteLine("Core Start!")
        Dim mc = New Java.Client.Download.Mojang.Minecraft(is_compatible_mode:=True)
		mc.update_manifests()
	End Sub
End Module

' 沟槽的VB.NET变量竟然不区分大小写，我服了，全是命名冲突
Public Class Core

End Class



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

Public Class Config

	' TODO: URL访问地址 常量 之后迁移
	Public Class url

		Public Class domain
			Public Const mojang_v1 As String = "https://piston-meta.mojang.com/"
			Public Const mojang_v2 As String = "https://launchermeta.mojang.com/"
			Public Const bmcl As String = "https://bmclapi2.bangbang93.com/"
		End Class


		Public Class version_manifest
			Public Const mojang_v1 As String = "mc/game/version_manifest.json"
			Public Const mojang_v2 As String = "mc/game/version_manifest_v2.json"
		End Class


	End Class

	Public Class api

		Public Class json
			' 1 System.Text.Json, 2 Json.Net.Core, 3 Newtonsoft.Json, 4 Bemly.Json
			Public Const mode As Integer = 3
		End Class

	End Class

End Class
