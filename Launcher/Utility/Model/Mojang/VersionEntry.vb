Option Explicit On
Option Strict On

Namespace Utility.Model.Mojang

    Public Class VersionEntry
        Public ReadOnly Property id As String
        Public ReadOnly Property type As String
        Public ReadOnly Property url As String
        Public ReadOnly Property release_time As String

        Public Sub New(ByVal id As String, ByVal type As String,
                       ByVal url As String, ByVal release_time As String)
            Me.id = id
            Me.type = type
            Me.url = url
            Me.release_time = release_time
        End Sub
    End Class

End Namespace
