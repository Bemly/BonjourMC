Option Explicit On
Option Strict On

Namespace Models

    Public Class PageItem
        Public ReadOnly title As String
        Public ReadOnly icon As String

        Public Sub New(ByVal title As String, ByVal icon As String)
            Me.title = title
            Me.icon = icon
        End Sub
    End Class

End Namespace
