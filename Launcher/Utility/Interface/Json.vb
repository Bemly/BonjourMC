
Option Explicit On
Option Strict On

Imports System


Namespace Utility.Interface

	Public Interface Json

        ''' <summary>
        ''' 同步 不建议使用
        ''' 判断 字符串 是否是 JSON
        ''' </summary>
        ''' <param name="str">目标字符串</param>
        ''' <returns>JSON 字符串</returns>
        Function is_json(str As String) As Boolean
	End Interface
End Namespace


