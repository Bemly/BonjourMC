Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Threading.Tasks
Imports ReactiveUI
Imports System.Reactive
Imports System.Reactive.Linq
Imports Avalonia.Threading
Imports Launcher

Namespace ViewModels
    Public Class HomeViewModel
        Inherits ViewModelBase

        Private _launcher_service As Services.LauncherService
        Private _navigate_action As Action(Of String)
        Private _username As String = Config.settings.default_username
        Private _selected_version As String = ""
        Private _available_versions As List(Of String) = New List(Of String)()
        Private _is_launching As Boolean = False
        Private _status_text As String = ""
        Private ReadOnly _launch_command As ReactiveCommand(Of Unit, Unit)
        Private ReadOnly _navigate_download_command As ReactiveCommand(Of Unit, Unit)
        Private ReadOnly _navigate_instance_command As ReactiveCommand(Of Unit, Unit)

        Public Sub New(ByVal service As Services.LauncherService, ByVal navigate_action As Action(Of String))
            Debug.WriteLine("[HomeVM] New: initializing")
            _launcher_service = service
            _navigate_action = navigate_action
            page_title = "Home"
            _launch_command = ReactiveCommand.Create(
                Sub()
                    If String.IsNullOrEmpty(selected_version) OrElse is_launching Then Return
                    Dim unused = execute_launch()
                End Sub)
            _navigate_download_command = ReactiveCommand.Create(
                Sub()
                    Debug.WriteLine("[HomeVM] navigate: download")
                    _navigate_action.Invoke("download")
                End Sub)
            _navigate_instance_command = ReactiveCommand.Create(
                Sub()
                    Debug.WriteLine("[HomeVM] navigate: instance")
                    _navigate_action.Invoke("instance")
                End Sub)
            load_installed_versions()
        End Sub

        Private Sub load_installed_versions()
            Debug.WriteLine("[HomeVM] load_installed_versions: start")
            _available_versions = _launcher_service.get_installed_versions()
            Me.RaisePropertyChanged(NameOf(available_versions))
            If _available_versions.Count > 0 Then
                selected_version = _available_versions(0)
            End If
            Debug.WriteLine($"[HomeVM] load_installed_versions: found {_available_versions.Count} versions")
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
                Return _launch_command
            End Get
        End Property

        Public ReadOnly Property navigate_download_command As ReactiveCommand(Of Unit, Unit)
            Get
                Return _navigate_download_command
            End Get
        End Property

        Public ReadOnly Property navigate_instance_command As ReactiveCommand(Of Unit, Unit)
            Get
                Return _navigate_instance_command
            End Get
        End Property

        Private Async Function execute_launch() As Task
            If String.IsNullOrEmpty(selected_version) OrElse is_launching Then Return
            Debug.WriteLine($"[HomeVM] execute_launch: start, version={selected_version}, user={username}")
            Dispatcher.UIThread.Post(Sub()
                                         is_launching = True
                                         status_text = $"Launching Minecraft {selected_version}..."
                                     End Sub)
            Try
                Await _launcher_service.launch_game(username, selected_version)
                Debug.WriteLine("[HomeVM] execute_launch: launch returned OK")
                Dispatcher.UIThread.Post(Sub() status_text = "Game started!")
            Catch ex As Exception
                Debug.WriteLine($"[HomeVM] execute_launch: ERROR {ex.Message}")
                Dispatcher.UIThread.Post(Sub() status_text = $"Launch failed: {ex.Message}")
            Finally
                Dispatcher.UIThread.Post(Sub() is_launching = False)
            End Try
        End Function

        ''' <summary>
        ''' 刷新已安装版本列表
        ''' </summary>
        Public Sub refresh_versions()
            Debug.WriteLine("[HomeVM] refresh_versions: start")
            _available_versions = _launcher_service.get_installed_versions()
            Me.RaisePropertyChanged(NameOf(available_versions))
            Debug.WriteLine($"[HomeVM] refresh_versions: found {_available_versions.Count} versions")
        End Sub
    End Class
End Namespace
