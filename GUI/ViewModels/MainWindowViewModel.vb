Namespace ViewModels
    Public Class MainWindowViewModel
        Inherits ViewModelBase

        Public ReadOnly Property Greeting As String
            Get
                Return "Welcome to Avalonia!"
            End Get
        End Property
    End Class
End Namespace
