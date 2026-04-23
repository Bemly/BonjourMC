Option Explicit On
Option Strict On

Namespace Models

    Public Class DownloadState
        Public Property is_downloading As Boolean = False
        Public Property progress As Double = 0
        Public Property phase As String = ""
        Public Property current_item As String = ""
        Public Property total As Integer = 0
        Public Property current As Integer = 0
    End Class

End Namespace
