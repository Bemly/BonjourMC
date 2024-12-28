
Option Explicit On
Option Strict On

Imports System


Namespace Utility

	Public NotInheritable Class Config

		' URL访问地址 常量 之后迁移
		Public NotInheritable Class url

			Public NotInheritable Class domain
				Public Const mojang_v1 As String = "https://piston-meta.mojang.com/"
				Public Const mojang_v2 As String = "https://launchermeta.mojang.com/"
				Public Const bmcl As String = "https://bmclapi2.bangbang93.com/"
			End Class


			Public NotInheritable Class version_manifest
				Public Const mojang_v1 As String = "mc/game/version_manifest.json"
				Public Const mojang_v2 As String = "mc/game/version_manifest_v2.json"
			End Class

		End Class

	End Class
End Namespace


