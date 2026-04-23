Option Explicit On
Option Strict On

Imports System.Collections.Generic
Imports ReactiveUI

Namespace Lang
    Public Class Lang
        Inherits ReactiveObject

        Private Shared ReadOnly _instance As New Lang()
        Private _strings As Dictionary(Of String, String)
        Private _current_lang As String = "zh-CN"

        Private Sub New()
            _strings = Strings_zh.get_strings()
        End Sub

        Public Shared ReadOnly Property Instance As Lang
            Get
                Return _instance
            End Get
        End Property

        Public ReadOnly Property current_lang As String
            Get
                Return _current_lang
            End Get
        End Property

        Public Sub set_language(ByVal code As String)
            If _current_lang = code Then Return
            _current_lang = code
            Select Case code
                Case "en-US"
                    _strings = Strings_en.get_strings()
                Case Else
                    _strings = Strings_zh.get_strings()
            End Select
            ' Raise all properties
            Me.RaisePropertyChanged("")
        End Sub

        Private Function s(ByVal key As String) As String
            Dim value As String = Nothing
            If _strings.TryGetValue(key, value) Then
                Return value
            End If
            Return key
        End Function

        ' --- Navigation ---
        Public ReadOnly Property nav_start As String
            Get
                Return s("nav_start")
            End Get
        End Property
        Public ReadOnly Property nav_download As String
            Get
                Return s("nav_download")
            End Get
        End Property
        Public ReadOnly Property nav_settings As String
            Get
                Return s("nav_settings")
            End Get
        End Property
        Public ReadOnly Property nav_tools As String
            Get
                Return s("nav_tools")
            End Get
        End Property

        ' --- Home ---
        Public ReadOnly Property home_subtitle As String
            Get
                Return s("home_subtitle")
            End Get
        End Property
        Public ReadOnly Property home_download As String
            Get
                Return s("home_download")
            End Get
        End Property
        Public ReadOnly Property home_instance As String
            Get
                Return s("home_instance")
            End Get
        End Property
        Public ReadOnly Property home_announce As String
            Get
                Return s("home_announce")
            End Get
        End Property
        Public ReadOnly Property home_no_announce As String
            Get
                Return s("home_no_announce")
            End Get
        End Property

        ' --- Versions ---
        Public ReadOnly Property ver_title As String
            Get
                Return s("ver_title")
            End Get
        End Property
        Public ReadOnly Property ver_release As String
            Get
                Return s("ver_release")
            End Get
        End Property
        Public ReadOnly Property ver_snapshot As String
            Get
                Return s("ver_snapshot")
            End Get
        End Property
        Public ReadOnly Property ver_all As String
            Get
                Return s("ver_all")
            End Get
        End Property
        Public ReadOnly Property ver_search As String
            Get
                Return s("ver_search")
            End Get
        End Property
        Public ReadOnly Property ver_refresh As String
            Get
                Return s("ver_refresh")
            End Get
        End Property
        Public ReadOnly Property ver_downloading As String
            Get
                Return s("ver_downloading")
            End Get
        End Property
        Public ReadOnly Property ver_btn_download As String
            Get
                Return s("ver_btn_download")
            End Get
        End Property
        Public ReadOnly Property ver_btn_delete As String
            Get
                Return s("ver_btn_delete")
            End Get
        End Property
        Public ReadOnly Property ver_loading As String
            Get
                Return s("ver_loading")
            End Get
        End Property

        ' --- Settings ---
        Public ReadOnly Property set_player As String
            Get
                Return s("set_player")
            End Get
        End Property
        Public ReadOnly Property set_player_name As String
            Get
                Return s("set_player_name")
            End Get
        End Property
        Public ReadOnly Property set_player_placeholder As String
            Get
                Return s("set_player_placeholder")
            End Get
        End Property
        Public ReadOnly Property set_java As String
            Get
                Return s("set_java")
            End Get
        End Property
        Public ReadOnly Property set_memory As String
            Get
                Return s("set_memory")
            End Get
        End Property
        Public ReadOnly Property set_memory_format As String
            Get
                Return s("set_memory_format")
            End Get
        End Property
        Public ReadOnly Property set_java_path As String
            Get
                Return s("set_java_path")
            End Get
        End Property
        Public ReadOnly Property set_java_placeholder As String
            Get
                Return s("set_java_placeholder")
            End Get
        End Property
        Public ReadOnly Property set_browse As String
            Get
                Return s("set_browse")
            End Get
        End Property
        Public ReadOnly Property set_game As String
            Get
                Return s("set_game")
            End Get
        End Property
        Public ReadOnly Property set_game_dir As String
            Get
                Return s("set_game_dir")
            End Get
        End Property
        Public ReadOnly Property set_game_placeholder As String
            Get
                Return s("set_game_placeholder")
            End Get
        End Property
        Public ReadOnly Property set_window_size As String
            Get
                Return s("set_window_size")
            End Get
        End Property
        Public ReadOnly Property set_pixels As String
            Get
                Return s("set_pixels")
            End Get
        End Property
        Public ReadOnly Property set_save As String
            Get
                Return s("set_save")
            End Get
        End Property
        Public ReadOnly Property lbl_language As String
            Get
                Return s("set_language")
            End Get
        End Property

        ' --- Log ---
        Public ReadOnly Property log_title As String
            Get
                Return s("log_title")
            End Get
        End Property
        Public ReadOnly Property log_clear As String
            Get
                Return s("log_clear")
            End Get
        End Property
        Public ReadOnly Property log_kill As String
            Get
                Return s("log_kill")
            End Get
        End Property
        Public ReadOnly Property log_placeholder As String
            Get
                Return s("log_placeholder")
            End Get
        End Property

        ' --- Common ---
        Public ReadOnly Property loading As String
            Get
                Return s("loading")
            End Get
        End Property

        ' --- Messages (with format) ---
        Public Function msg_launching(ByVal version As String) As String
            Return String.Format(s("msg_launching"), version)
        End Function
        Public ReadOnly Property msg_started As String
            Get
                Return s("msg_started")
            End Get
        End Property
        Public Function msg_launch_failed(ByVal error_msg As String) As String
            Return String.Format(s("msg_launch_failed"), error_msg)
        End Function
        Public ReadOnly Property msg_download_done As String
            Get
                Return s("msg_download_done")
            End Get
        End Property
        Public ReadOnly Property msg_loading_versions As String
            Get
                Return s("msg_loading_versions")
            End Get
        End Property
        Public Function msg_loaded_versions(ByVal count As Integer) As String
            Return String.Format(s("msg_loaded_versions"), count)
        End Function
        Public Function msg_load_failed(ByVal error_msg As String) As String
            Return String.Format(s("msg_load_failed"), error_msg)
        End Function
        Public Function msg_downloading(ByVal version_id As String) As String
            Return String.Format(s("msg_downloading"), version_id)
        End Function
        Public Function msg_download_failed(ByVal error_msg As String) As String
            Return String.Format(s("msg_download_failed"), error_msg)
        End Function
        Public Function msg_deleted(ByVal version_id As String) As String
            Return String.Format(s("msg_deleted"), version_id)
        End Function
        Public Function msg_delete_failed(ByVal error_msg As String) As String
            Return String.Format(s("msg_delete_failed"), error_msg)
        End Function
        Public ReadOnly Property msg_settings_saved As String
            Get
                Return s("msg_settings_saved")
            End Get
        End Property
        Public ReadOnly Property log_out As String
            Get
                Return s("log_out")
            End Get
        End Property
        Public ReadOnly Property log_err As String
            Get
                Return s("log_err")
            End Get
        End Property
        Public Function log_exit(ByVal exit_code As Integer) As String
            Return String.Format(s("log_exit"), exit_code)
        End Function
    End Class
End Namespace
