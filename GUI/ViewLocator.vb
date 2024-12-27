Imports System
Imports Avalonia.Controls
Imports Avalonia.Controls.Templates
Imports GUI.ViewModels

Public Class ViewLocator
    Implements IDataTemplate

    Public Function Build(data As Object) As Control Implements IDataTemplate.Build
        If data Is Nothing Then
            Return Nothing
        End If

        Dim name = data.GetType().FullName.Replace("ViewModel", "View", StringComparison.Ordinal)
        Dim type = System.Type.GetType(name)

        If type IsNot Nothing Then
            Dim control = CType(Activator.CreateInstance(type), Control)
            control.DataContext = data
            Return control
        End If

        Return New TextBlock With {.Text = "Not Found: " & name}
    End Function

    Public Function Match(data As Object) As Boolean Implements IDataTemplate.Match
        Return TypeOf data Is ViewModelBase
    End Function
End Class
