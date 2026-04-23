Imports System
Imports System.Collections.Generic
Imports System.Threading.Tasks
Imports ReactiveUI
Imports System.Reactive
Imports Launcher

Namespace ViewModels
    Public Class HomeViewModel
        Inherits ViewModelBase

        Private _launcher_service As Services.LauncherService
        Private _username As String = Config.settings.default_username
        Private _selected_version As String = ""
        Private _available_versions As List(Of String) = New List(Of String)()
        Private _is_launching As Boolean = False
        Private _status_text As String = ""

        Public Sub New(ByVal service As Services.LauncherService)
            _launcher_service = service
            page_title = "Home"
            load_installed_versions()
        End Sub

        Private Sub load_installed_versions()
            _available_versions = _launcher_service.get_installed_versions()
            Me.RaisePropertyChanged(NameOf(available_versions))
            If _available_versions.Count > 0 Then
                selected_version = _available_versions(0)
            End If
        End Sub

        Public Property username As String
            Get
                Return _username
            End Get
            Set(value As String)
                Me.RaiseAndSetIfChanged(_username, value)
            End Set
        End Property

        Public Property selected_version As String
            Get
                Return _selected_version
            End Get
            Set(value As String)
                Me.RaiseAndSetIfChanged(_selected_version, value)
            End Set
        End Property

        Public ReadOnly Property available_versions As List(Of String)
            Get
                Return _available_versions
            End Get
        End Property

        Public Property is_launching As Boolean
            Get
                Return _is_launching
            End Get
            Set(value As Boolean)
                Me.RaiseAndSetIfChanged(_is_launching, value)
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

        Public ReadOnly Property launch_command As ReactiveCommand(Of Unit, Unit)
            Get
                Return ReactiveCommand.CreateFromTask(AddressOf execute_launch)
            End Get
        End Property

        Private Async Function execute_launch() As Task
            If String.IsNullOrEmpty(selected_version) OrElse is_launching Then Return
            is_launching = True
            status_text = $"Launching Minecraft {selected_version}..."
            Try
                Await _launcher_service.launch_game(username, selected_version)
                status_text = "Game started!"
            Catch ex As Exception
                status_text = $"Launch failed: {ex.Message}"
            Finally
                is_launching = False
            End Try
        End Function

        ''' <summary>
        ''' 刷新已安装版本列表
        ''' </summary>
        Public Sub refresh_versions()
            _available_versions = _launcher_service.get_installed_versions()
            Me.RaisePropertyChanged(NameOf(available_versions))
        End Sub
    End Class
End Namespace
