
Option Explicit On
Option Strict On

Imports System
Imports Newtonsoft.Json.Schema
Imports Newtonsoft.Json.Linq
Imports I_Json = Launcher.Utility.Interface.Json
Imports Newtonsoft.Json


Namespace Utility.Bridge

	' TODO: 之后桥接用新的项目来对接，适合裁切内核！
	' 1 System.Text.Json, 2 Json.Net.Core, 3 Newtonsoft.Json, 4 Bemly.Json
	' 统一桥(共享/静态)来隐藏适配器(单例)
	Public Class Json
		Implements I_Json

		Private Shared instance As I_Json
		Private Shared mode As String = Launcher.Config.api.json_mode

		' 首次加载也有线程安全 gettype具有唯一性
		Shared Sub New()
			SyncLock GetType(Json)
				If instance Is Nothing Then
					Select Case mode
						Case "System", "JsonNet", "Bemly_"
							Throw New NotImplementedException("Not Found Bemly.Json Adapter.")
						Case "Newtonsoft"
							instance = Newtonsoft_json_adapter.Instance
						Case Else
							Throw New NotImplementedException("前面的蛆以后再来探索吧！打咩")
					End Select
				End If
			End SyncLock
		End Sub

		Shared Function is_json(str As String) As Boolean
			Return instance.is_json(str)
		End Function

		Shared Function to_json(str As String) As Object
			Return instance.to_json(str)
		End Function

		Public Function is_json_inst(str As String) As Boolean Implements I_Json.is_json
			Return instance.is_json(str)
		End Function

		Public Function to_json_inst(str As String) As Object Implements I_Json.to_json
			Return instance.is_json(str)
		End Function



		' ****** Adapter Segment ******
		Private NotInheritable Class System_text_json_adapter
			' TODO: Add System.Text.Json Adapter.
			Shared Sub New()
				Throw New NotImplementedException()
			End Sub
		End Class


		Private NotInheritable Class Newtonsoft_json_adapter
			Implements I_Json

			' ==== Singleton Layer ====
			Shared ReadOnly m_instance As New Newtonsoft_json_adapter()
			Shared Sub New()
			End Sub
			Private Sub New()
			End Sub

			Friend Shared ReadOnly Property Instance As Newtonsoft_json_adapter
				Get
					Return m_instance
				End Get
			End Property
			' == Singleton Layer End ==

			Friend Function is_json(str As String) As Boolean Implements I_Json.is_json
				Try
					to_json(str)
					Return True
				Catch e As JsonReaderException
					Return False
				End Try
			End Function

			Friend Function to_json(str As String) As Object Implements I_Json.to_json
				Return JObject.Parse(str)
			End Function
		End Class


		Private NotInheritable Class Json_net_adapter
			' TODO: Add Json.Net.Core Adapter.
			Shared Sub New()
				Throw New NotImplementedException()
			End Sub
		End Class


		Private NotInheritable Class Bemly_json_adapter
			' TODO: Add moe.bemly.json Adapter.
			Shared Sub New()
				Throw New NotImplementedException()
			End Sub
		End Class

	End Class

End Namespace


