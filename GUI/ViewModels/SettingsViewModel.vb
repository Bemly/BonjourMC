Imports System
Imports ReactiveUI
Imports System.Reactive
Imports Launcher

Namespace ViewModels
    Public Class SettingsViewModel
        Inherits ViewModelBase

        Private _username As String = Config.settings.default_username
        Private _memory_mb As Integer = Config.settings.default_memory_mb
        Private _java_path As String = ""
        Private _game_dir As String = ""
        Private _window_width As Integer = Config.settings.default_window_width
        Private _window_height As Integer = Config.settings.default_window_height
        Private _status_text As String = ""

        Public Sub New()
            page_title = "Settings"
        End Sub

        Public Property username As String
            Get
                Return _username
            End Get
            Set(value As String)
                Me.RaiseAndSetIfChanged(_username, value)
            End Set
        End Property

        Public Property memory_mb As Integer
            Get
                Return _memory_mb
            End Get
            Set(value As Integer)
                Me.RaiseAndSetIfChanged(_memory_mb, value)
            End Set
        End Property

        Public Property java_path As String
            Get
                Return _java_path
            End Get
            Set(value As String)
                Me.RaiseAndSetIfChanged(_java_path, value)
            End Set
        End Property

        Public Property game_dir As String
            Get
                Return _game_dir
            End Get
            Set(value As String)
                Me.RaiseAndSetIfChanged(_game_dir, value)
            End Set
        End Property

        Public Property window_width As Integer
            Get
                Return _window_width
            End Get
            Set(value As Integer)
                Me.RaiseAndSetIfChanged(_window_width, value)
            End Set
        End Property

        Public Property window_height As Integer
            Get
                Return _window_height
            End Get
            Set(value As Integer)
                Me.RaiseAndSetIfChanged(_window_height, value)
            End Set
        End Property

        Public Property status_text As String
            Get
                Return _status_text
            End Get
            Set(value As String)
                Me.RaiseAndSetIfChanged(_status_text, value)
            End Set
        End Property

        Public ReadOnly Property save_command As ReactiveCommand(Of Unit, Unit)
            Get
                Return ReactiveCommand.Create(AddressOf execute_save)
            End Get
        End Property

        Private Sub execute_save()
            status_text = "Settings saved!"
        End Sub
    End Class
End Namespace
