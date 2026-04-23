Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
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
        Private _selected_language As String = "中文"
        Private _selected_category As Integer = 0
        Private ReadOnly _save_command As ReactiveCommand(Of Unit, Unit)

        Public Sub New()
            Debug.WriteLine("[SettingsVM] New: initializing")
            page_title = "Settings"
            ' Set initial language display
            If Lang.Lang.Instance.current_lang = "en-US" Then
                _selected_language = "English"
            Else
                _selected_language = "中文"
            End If
            _save_command = ReactiveCommand.Create(AddressOf execute_save)
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
                Return _save_command
            End Get
        End Property

        Public ReadOnly Property available_languages As List(Of String)
            Get
                Return New List(Of String) From {"中文", "English"}
            End Get
        End Property

        Public Property selected_language As String
            Get
                Return _selected_language
            End Get
            Set(value As String)
                If _selected_language = value Then Return
                Me.RaiseAndSetIfChanged(_selected_language, value)
                Select Case value
                    Case "English"
                        Lang.Lang.Instance.set_language("en-US")
                    Case Else
                        Lang.Lang.Instance.set_language("zh-CN")
                End Select
                Debug.WriteLine($"[SettingsVM] language changed to: {value}")
            End Set
        End Property

        Public Property selected_category As Integer
            Get
                Return _selected_category
            End Get
            Set(value As Integer)
                If _selected_category = value Then Return
                Me.RaiseAndSetIfChanged(_selected_category, value)
                RaisePropertyChanged(NameOf(is_language_selected))
                RaisePropertyChanged(NameOf(is_player_selected))
                RaisePropertyChanged(NameOf(is_java_selected))
                RaisePropertyChanged(NameOf(is_game_selected))
                Debug.WriteLine($"[SettingsVM] category changed to: {value}")
            End Set
        End Property

        Public ReadOnly Property is_language_selected As Boolean
            Get
                Return _selected_category = 0
            End Get
        End Property

        Public ReadOnly Property is_player_selected As Boolean
            Get
                Return _selected_category = 1
            End Get
        End Property

        Public ReadOnly Property is_java_selected As Boolean
            Get
                Return _selected_category = 2
            End Get
        End Property

        Public ReadOnly Property is_game_selected As Boolean
            Get
                Return _selected_category = 3
            End Get
        End Property

        Private Sub execute_save()
            Debug.WriteLine($"[SettingsVM] execute_save: user={username}, mem={memory_mb}MB, java={java_path}, dir={game_dir}")
            status_text = Lang.Lang.Instance.msg_settings_saved
        End Sub
    End Class
End Namespace
