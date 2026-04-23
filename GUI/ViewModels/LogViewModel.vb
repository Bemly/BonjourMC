Imports System
Imports System.Text
Imports ReactiveUI
Imports System.Reactive

Namespace ViewModels
    Public Class LogViewModel
        Inherits ViewModelBase

        Private _launcher_service As Services.LauncherService
        Private _log_text As String = ""
        Private _is_game_running As Boolean = False
        Private _log_builder As New StringBuilder()

        Public Sub New(ByVal service As Services.LauncherService)
            _launcher_service = service
            page_title = "Logs"

            AddHandler _launcher_service.on_game_output, Sub(sender, line)
                                                             append_log("[OUT] " & line)
                                                         End Sub
            AddHandler _launcher_service.on_game_error, Sub(sender, line)
                                                            append_log("[ERR] " & line)
                                                        End Sub
            AddHandler _launcher_service.on_game_exit, Sub(sender, exit_code)
                                                           _is_game_running = False
                                                           Me.RaisePropertyChanged(NameOf(is_game_running))
                                                           append_log($"[EXIT] Game exited with code {exit_code}")
                                                       End Sub
        End Sub

        Public Property log_text As String
            Get
                Return _log_text
            End Get
            Set(value As String)
                Me.RaiseAndSetIfChanged(_log_text, value)
            End Set
        End Property

        Public Property is_game_running As Boolean
            Get
                Return _is_game_running
            End Get
            Set(value As Boolean)
                Me.RaiseAndSetIfChanged(_is_game_running, value)
            End Set
        End Property

        Public ReadOnly Property clear_command As ReactiveCommand(Of Unit, Unit)
            Get
                Return ReactiveCommand.Create(AddressOf execute_clear)
            End Get
        End Property

        Public ReadOnly Property kill_command As ReactiveCommand(Of Unit, Unit)
            Get
                Return ReactiveCommand.Create(AddressOf execute_kill)
            End Get
        End Property

        Public Sub append_log(ByVal line As String)
            _log_builder.AppendLine(line)
            log_text = _log_builder.ToString()
            _is_game_running = _launcher_service.is_game_running
            Me.RaisePropertyChanged(NameOf(is_game_running))
        End Sub

        Private Sub execute_clear()
            _log_builder.Clear()
            log_text = ""
        End Sub

        Private Sub execute_kill()
            _launcher_service.kill_game()
        End Sub
    End Class
End Namespace
