Imports Avalonia
Imports Avalonia.Controls.ApplicationLifetimes
Imports Avalonia.Markup.Xaml
Imports GUI.ViewModels
Imports GUI.Views

Partial Public Class App
    Inherits Application

    Public Overrides Sub Initialize()
        AvaloniaXamlLoader.Load(Me)
    End Sub

    Public Overrides Sub OnFrameworkInitializationCompleted()
        Dim desktop = TryCast(ApplicationLifetime, IClassicDesktopStyleApplicationLifetime)
        If desktop IsNot Nothing Then
            desktop.MainWindow = New MainWindow With {.DataContext = New MainWindowViewModel}
        End If

        MyBase.OnFrameworkInitializationCompleted()
    End Sub
End Class
