
Option Explicit On
Option Strict On

Imports System
Imports Newtonsoft.Json.Schema
Imports Newtonsoft.Json.Linq
Imports I_Json = Launcher.Utility.Interface.Json


Namespace Utility.Bridge

	' TODO: 之后桥接用新的项目来对接，适合裁切内核！
	' 1 System.Text.Json, 2 Json.Net.Core, 3 Newtonsoft.Json, 4 Bemly.Json
	' 统一桥(共享/静态)来隐藏适配器(单例)
	Public Class Json
		Private Shared instance As I_Json

		' 首次加载也有线程安全 gettype具有唯一性
		Shared Sub New()
			SyncLock GetType(Json)
				If instance Is Nothing Then
					Select Case Launcher.Config.api.json.mode
						Case 3
							instance = Newtonsoft_json_adapter.Instance
						Case Else
							' TODO: 给我写更多的json兼容格式
							Throw New NotImplementedException("其他 打咩")
					End Select
				End If
			End SyncLock
		End Sub

		Shared Function is_json(str As String) As Boolean
			Return instance.is_json(str)
		End Function

	End Class

	Friend NotInheritable Class System_text_json_adapter
	End Class

	Friend NotInheritable Class Newtonsoft_json_adapter
		Implements I_Json

		' ==== Singleton Layer ====
		Shared ReadOnly m_instance As New Newtonsoft_json_adapter()

		Shared Sub New()
		End Sub

		Private Sub New()
		End Sub

		Public Shared ReadOnly Property Instance As Newtonsoft_json_adapter
			Get
				Return m_instance
			End Get
		End Property
		' == Singleton Layer End ==

		Friend Function is_json(str As String) As Boolean Implements I_Json.is_json
			Try
				JToken.Parse(str)
				Return True
			Catch
				Return False
			End Try
		End Function
	End Class

	Friend NotInheritable Class Json_net_adapter
	End Class

	Friend NotInheritable Class Bemly_json_adapter
		Shared ReadOnly m_instance As New Bemly_json_adapter()

		Shared Sub New()
		End Sub

		Private Sub New()
		End Sub

		Public Shared ReadOnly Property Instance As Bemly_json_adapter
			Get
				Return m_instance
			End Get
		End Property
	End Class
End Namespace


