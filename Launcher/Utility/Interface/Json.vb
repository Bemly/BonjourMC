
Option Explicit On
Option Strict On

Imports System
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports Newtonsoft.Json.Schema


Namespace Utility.Interface

    Public Interface Json

        ''' <summary>
        ''' 同步 不建议使用
        ''' 判断 字符串 是否是 JSON
        ''' </summary>
        ''' <param name="str">目标字符串</param>
        ''' <returns>JSON 字符串</returns>
        Function is_json(str As String) As Boolean
        'Function to_json(str As String) As
        Function to_json(str As String) As Object
    End Interface
End Namespace


