Imports System.Diagnostics
Imports System.IO
Imports Avalonia
Imports Avalonia.Controls.ApplicationLifetimes
Imports Avalonia.Markup.Xaml
Imports Avalonia.ReactiveUI
Imports ReactiveUI
Imports GUI.ViewModels
Imports GUI.Views

Partial Public Class App
    Inherits Application

    Public Overrides Sub Initialize()
        ' Setup file logging for cross-platform debugging
        setup_file_logging()

        Debug.WriteLine("[App] Initialize: setting up ReactiveUI scheduler")
        RxApp.MainThreadScheduler = AvaloniaScheduler.Instance
        Debug.WriteLine("[App] Initialize: loading XAML")
        AvaloniaXamlLoader.Load(Me)
        Debug.WriteLine("[App] Initialize: done")
    End Sub

    Private Sub setup_file_logging()
        Try
            Dim log_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bonjourmc_debug.log")
            Dim writer As New StreamWriter(log_path, False)
            writer.AutoFlush = True
            Dim listener As New TextWriterTraceListener(writer)
            Trace.Listeners.Add(listener)
            Debug.WriteLine($"[App] Log file: {log_path}")
        Catch ex As Exception
            ' Ignore logging setup errors
        End Try
    End Sub

    Public Overrides Sub OnFrameworkInitializationCompleted()
        Debug.WriteLine("[App] OnFrameworkInitializationCompleted: start")
        Dim desktop = TryCast(ApplicationLifetime, IClassicDesktopStyleApplicationLifetime)
        If desktop IsNot Nothing Then
            Debug.WriteLine("[App] OnFrameworkInitializationCompleted: creating MainWindow")
            desktop.MainWindow = New MainWindow With {.DataContext = New MainWindowViewModel}
            Debug.WriteLine("[App] OnFrameworkInitializationCompleted: MainWindow created")
        End If

        MyBase.OnFrameworkInitializationCompleted()
        Debug.WriteLine("[App] OnFrameworkInitializationCompleted: done")
    End Sub
End Class
