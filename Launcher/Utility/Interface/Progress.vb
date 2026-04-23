Option Explicit On
Option Strict On

Namespace Utility.Interface

    Public Interface Progress
        Event on_progress(sender As Object, e As ProgressArgs)
        Event on_complete(sender As Object, e As ProgressArgs)
        Event on_error(sender As Object, e As ProgressArgs)
    End Interface

    Public Class ProgressArgs
        Inherits EventArgs

        Public ReadOnly total As Integer
        Public ReadOnly current As Integer
        Public ReadOnly message As String
        Public ReadOnly phase As String

        Public Sub New(ByVal total As Integer, ByVal current As Integer,
                       ByVal message As String, Optional ByVal phase As String = "")
            Me.total = total
            Me.current = current
            Me.message = message
            Me.phase = phase
        End Sub

        Public ReadOnly Property percent As Double
            Get
                If total = 0 Then Return 0
                Return CDbl(current) / CDbl(total) * 100.0
            End Get
        End Property
    End Class

End Namespace
