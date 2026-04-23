Imports System
Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.Linq
Imports System.Threading.Tasks
Imports ReactiveUI
Imports System.Reactive
Imports Launcher.Utility.Model.Mojang

Namespace ViewModels
    Public Class VersionsViewModel
        Inherits ViewModelBase

        Private _launcher_service As Services.LauncherService
        Private _all_versions As ObservableCollection(Of VersionEntry) = New ObservableCollection(Of VersionEntry)()
        Private _filter_type As String = "release"
        Private _search_text As String = ""
        Private _is_downloading As Boolean = False
        Private _download_progress As Double = 0
        Private _download_phase As String = ""
        Private _download_current_item As String = ""
        Private _status_text As String = ""
        Private _is_loading As Boolean = False

        Public Sub New(ByVal service As Services.LauncherService)
            _launcher_service = service
            page_title = "Versions"

            AddHandler _launcher_service.on_download_progress, Sub(sender, e)
                                                                   _download_progress = e.percent
                                                                   _download_phase = e.phase
                                                                   _download_current_item = e.message
                                                                   Me.RaisePropertyChanged(NameOf(download_progress))
                                                                   Me.RaisePropertyChanged(NameOf(download_phase))
                                                                   Me.RaisePropertyChanged(NameOf(download_current_item))
                                                                   If e.phase = "done" Then
                                                                       is_downloading = False
                                                                       status_text = "Download complete!"
                                                                       load_installed_versions()
                                                                   End If
                                                               End Sub
        End Sub

        Public ReadOnly Property all_versions As ObservableCollection(Of VersionEntry)
            Get
                Return _all_versions
            End Get
        End Property

        Public Property filter_type As String
            Get
                Return _filter_type
            End Get
            Set(value As String)
                Me.RaiseAndSetIfChanged(_filter_type, value)
                Me.RaisePropertyChanged(NameOf(filtered_versions))
            End Set
        End Property

        Public Property search_text As String
            Get
                Return _search_text
            End Get
            Set(value As String)
                Me.RaiseAndSetIfChanged(_search_text, value)
                Me.RaisePropertyChanged(NameOf(filtered_versions))
            End Set
        End Property

        Public Property is_downloading As Boolean
            Get
                Return _is_downloading
            End Get
            Set(value As Boolean)
                Me.RaiseAndSetIfChanged(_is_downloading, value)
            End Set
        End Property

        Public Property download_progress As Double
            Get
                Return _download_progress
            End Get
            Set(value As Double)
                Me.RaiseAndSetIfChanged(_download_progress, value)
            End Set
        End Property

        Public Property download_phase As String
            Get
                Return _download_phase
            End Get
            Set(value As String)
                Me.RaiseAndSetIfChanged(_download_phase, value)
            End Set
        End Property

        Public Property download_current_item As String
            Get
                Return _download_current_item
            End Get
            Set(value As String)
                Me.RaiseAndSetIfChanged(_download_current_item, value)
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

        Public Property is_loading As Boolean
            Get
                Return _is_loading
            End Get
            Set(value As Boolean)
                Me.RaiseAndSetIfChanged(_is_loading, value)
            End Set
        End Property

        Public ReadOnly Property installed_versions As List(Of String)
            Get
                Return _launcher_service.get_installed_versions()
            End Get
        End Property

        Public ReadOnly Property filtered_versions As List(Of VersionEntry)
            Get
                Dim query = _all_versions.AsEnumerable()

                If filter_type = "release" Then
                    query = query.Where(Function(v) v.type = "release")
                ElseIf filter_type = "snapshot" Then
                    query = query.Where(Function(v) v.type = "snapshot")
                End If

                If Not String.IsNullOrEmpty(search_text) Then
                    query = query.Where(Function(v) v.id.Contains(search_text, StringComparison.OrdinalIgnoreCase))
                End If

                Return query.ToList()
            End Get
        End Property

        Public ReadOnly Property refresh_command As ReactiveCommand(Of Unit, Unit)
            Get
                Return ReactiveCommand.CreateFromTask(AddressOf execute_refresh)
            End Get
        End Property

        Public ReadOnly Property download_command As ReactiveCommand(Of String, Unit)
            Get
                Return ReactiveCommand.CreateFromTask(Of String)(AddressOf execute_download)
            End Get
        End Property

        Public ReadOnly Property delete_command As ReactiveCommand(Of String, Unit)
            Get
                Return ReactiveCommand.Create(Of String)(AddressOf execute_delete)
            End Get
        End Property

        Private Async Function execute_refresh() As Task
            is_loading = True
            status_text = "Loading versions..."
            Try
                Dim versions = Await _launcher_service.refresh_version_manifest()
                _all_versions.Clear()
                For Each v In versions
                    _all_versions.Add(v)
                Next
                Me.RaisePropertyChanged(NameOf(filtered_versions))
                status_text = $"Loaded {versions.Count} versions."
            Catch ex As Exception
                status_text = $"Failed to load: {ex.Message}"
            Finally
                is_loading = False
            End Try
        End Function

        Private Async Function execute_download(ByVal version_id As String) As Task
            If is_downloading Then Return
            is_downloading = True
            download_progress = 0
            download_phase = "starting"
            status_text = $"Downloading {version_id}..."
            Try
                Await _launcher_service.download_version(version_id)
            Catch ex As Exception
                status_text = $"Download failed: {ex.Message}"
                is_downloading = False
            End Try
        End Function

        Private Sub execute_delete(ByVal version_id As String)
            Try
                _launcher_service.delete_version(version_id)
                load_installed_versions()
                status_text = $"Deleted {version_id}."
            Catch ex As Exception
                status_text = $"Delete failed: {ex.Message}"
            End Try
        End Sub

        Private Sub load_installed_versions()
            Me.RaisePropertyChanged(NameOf(installed_versions))
        End Sub
    End Class
End Namespace
