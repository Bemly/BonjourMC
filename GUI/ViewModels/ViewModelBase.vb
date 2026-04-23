Imports ReactiveUI

Namespace ViewModels
    Public Class ViewModelBase
        Inherits ReactiveObject

        Private _page_title As String = ""

        Public Overridable Property page_title As String
            Get
                Return _page_title
            End Get
            Set(value As String)
                Me.RaiseAndSetIfChanged(_page_title, value)
            End Set
        End Property
    End Class
End Namespace
