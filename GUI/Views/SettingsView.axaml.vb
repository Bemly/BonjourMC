Imports Avalonia
Imports Avalonia.Controls
Imports Avalonia.Input
Imports GUI.Controls

Namespace Views
    Partial Public Class SettingsView
        Inherits UserControl

        Private ReadOnly _items As New List(Of PclListItem)

        Protected Overrides Sub OnAttachedToVisualTree(ByVal e As VisualTreeAttachmentEventArgs)
            MyBase.OnAttachedToVisualTree(e)
            If _items.Count = 0 Then
                _items.AddRange({ItemLanguage, ItemPlayer, ItemJava, ItemGame})
            End If
        End Sub

        Private Sub Item_PointerReleased(ByVal sender As Object, ByVal e As PointerReleasedEventArgs)
            Dim clicked = TryCast(sender, PclListItem)
            If clicked Is Nothing Then Return
            If Not clicked.Checked Then Return

            Dim idx = _items.IndexOf(clicked)
            If idx < 0 Then Return

            ' Uncheck all others (RadioBox behavior)
            For i As Integer = 0 To _items.Count - 1
                If i <> idx Then _items(i).Checked = False
            Next

            ' Update ViewModel
            Dim vm = TryCast(DataContext, ViewModels.SettingsViewModel)
            If vm IsNot Nothing Then
                vm.selected_category = idx
            End If
        End Sub
    End Class
End Namespace
