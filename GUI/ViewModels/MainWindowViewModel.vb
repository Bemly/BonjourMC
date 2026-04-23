Imports ReactiveUI

Namespace ViewModels
    Public Class MainWindowViewModel
        Inherits ViewModelBase

        Private _pages As List(Of ViewModelBase)
        Private _selected_page As ViewModelBase
        Private _launcher_service As Services.LauncherService

        Public Sub New()
            _launcher_service = New Services.LauncherService()
            _pages = New List(Of ViewModelBase) From {
                New HomeViewModel(_launcher_service),
                New VersionsViewModel(_launcher_service),
                New SettingsViewModel(),
                New LogViewModel(_launcher_service)
            }
            _selected_page = _pages(0)
        End Sub

        Public ReadOnly Property pages As List(Of ViewModelBase)
            Get
                Return _pages
            End Get
        End Property

        Public Property selected_page As ViewModelBase
            Get
                Return _selected_page
            End Get
            Set(value As ViewModelBase)
                Me.RaiseAndSetIfChanged(_selected_page, value)
            End Set
        End Property

        Public ReadOnly Property launcher_service As Services.LauncherService
            Get
                Return _launcher_service
            End Get
        End Property
    End Class
End Namespace
