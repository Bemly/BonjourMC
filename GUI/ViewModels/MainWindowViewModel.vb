Option Explicit On
Option Strict On

Imports System
Imports System.Diagnostics
Imports ReactiveUI
Imports System.Reactive
Imports Avalonia.Controls.ApplicationLifetimes
Imports Avalonia.Controls
Imports Avalonia

Namespace ViewModels
    Public Class MainWindowViewModel
        Inherits ViewModelBase

        Private _pages As List(Of ViewModelBase)
        Private _selected_page As ViewModelBase
        Private _launcher_service As Services.LauncherService
        Private _home_vm As HomeViewModel
        Private _versions_vm As VersionsViewModel
        Private _settings_vm As SettingsViewModel
        Private _log_vm As LogViewModel

        Private ReadOnly _navigate_home_command As ReactiveCommand(Of Unit, Unit)
        Private ReadOnly _navigate_versions_command As ReactiveCommand(Of Unit, Unit)
        Private ReadOnly _navigate_settings_command As ReactiveCommand(Of Unit, Unit)
        Private ReadOnly _navigate_logs_command As ReactiveCommand(Of Unit, Unit)
        Private ReadOnly _minimize_command As ReactiveCommand(Of Unit, Unit)
        Private ReadOnly _close_command As ReactiveCommand(Of Unit, Unit)

        ''' <summary>
        ''' Action to trigger window close animation, set by MainWindow code-behind.
        ''' </summary>
        Public Property on_close_action As Action

        Public Sub New()
            Debug.WriteLine("[MainWindowVM] New: initializing")
            _launcher_service = New Services.LauncherService()
            _home_vm = New HomeViewModel(_launcher_service)
            _versions_vm = New VersionsViewModel(_launcher_service)
            _settings_vm = New SettingsViewModel()
            _log_vm = New LogViewModel(_launcher_service)
            _pages = New List(Of ViewModelBase) From {_home_vm, _versions_vm, _settings_vm, _log_vm}
            _selected_page = _pages(0)

            _navigate_home_command = ReactiveCommand.Create(Sub()
                                                                Debug.WriteLine("[MainWindowVM] navigate: Home")
                                                                selected_page = _home_vm
                                                            End Sub)
            _navigate_versions_command = ReactiveCommand.Create(Sub()
                                                                    Debug.WriteLine("[MainWindowVM] navigate: Versions")
                                                                    selected_page = _versions_vm
                                                                End Sub)
            _navigate_settings_command = ReactiveCommand.Create(Sub()
                                                                    Debug.WriteLine("[MainWindowVM] navigate: Settings")
                                                                    selected_page = _settings_vm
                                                                End Sub)
            _navigate_logs_command = ReactiveCommand.Create(Sub()
                                                                Debug.WriteLine("[MainWindowVM] navigate: Logs")
                                                                selected_page = _log_vm
                                                            End Sub)
            _minimize_command = ReactiveCommand.Create(Sub()
                                                           Debug.WriteLine("[MainWindowVM] minimize")
                                                           Dim desktop = TryCast(Application.Current?.ApplicationLifetime, IClassicDesktopStyleApplicationLifetime)
                                                           If desktop IsNot Nothing AndAlso desktop.MainWindow IsNot Nothing Then
                                                               desktop.MainWindow.WindowState = WindowState.Minimized
                                                           End If
                                                       End Sub)
            _close_command = ReactiveCommand.Create(Sub()
                                                        Debug.WriteLine("[MainWindowVM] close")
                                                        If on_close_action IsNot Nothing Then
                                                            on_close_action.Invoke()
                                                        Else
                                                            Dim desktop = TryCast(Application.Current?.ApplicationLifetime, IClassicDesktopStyleApplicationLifetime)
                                                            If desktop IsNot Nothing Then
                                                                desktop.Shutdown()
                                                            End If
                                                        End If
                                                    End Sub)
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

        Public ReadOnly Property username As String
            Get
                Return _home_vm.username
            End Get
        End Property

        ' --- Navigation Commands ---

        Public ReadOnly Property navigate_home_command As ReactiveCommand(Of Unit, Unit)
            Get
                Return _navigate_home_command
            End Get
        End Property

        Public ReadOnly Property navigate_versions_command As ReactiveCommand(Of Unit, Unit)
            Get
                Return _navigate_versions_command
            End Get
        End Property

        Public ReadOnly Property navigate_settings_command As ReactiveCommand(Of Unit, Unit)
            Get
                Return _navigate_settings_command
            End Get
        End Property

        Public ReadOnly Property navigate_logs_command As ReactiveCommand(Of Unit, Unit)
            Get
                Return _navigate_logs_command
            End Get
        End Property

        ' --- Window Control Commands ---

        Public ReadOnly Property minimize_command As ReactiveCommand(Of Unit, Unit)
            Get
                Return _minimize_command
            End Get
        End Property

        Public ReadOnly Property close_command As ReactiveCommand(Of Unit, Unit)
            Get
                Return _close_command
            End Get
        End Property
    End Class
End Namespace
