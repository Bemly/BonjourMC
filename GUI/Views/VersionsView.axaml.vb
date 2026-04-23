Imports Avalonia
Imports Avalonia.Controls
Imports Avalonia.Input
Imports GUI.Controls

Namespace Views
    Partial Public Class VersionsView
        Inherits UserControl

        Private ReadOnly _items As New List(Of PclListItem)

        Protected Overrides Sub OnAttachedToVisualTree(ByVal e As VisualTreeAttachmentEventArgs)
            MyBase.OnAttachedToVisualTree(e)
            If _items.Count = 0 Then
                _items.AddRange({ItemMinecraft, ItemMod, ItemModpack, ItemResourcepack, ItemShader})
            End If
        End Sub

        Private Sub Item_PointerReleased(ByVal sender As Object, ByVal e As PointerReleasedEventArgs)
            Dim clicked = TryCast(sender, PclListItem)
            If clicked Is Nothing Then Return
            If Not clicked.Checked Then Return

            For i As Integer = 0 To _items.Count - 1
                If _items(i) IsNot clicked Then _items(i).Checked = False
            Next
        End Sub
    End Class
End Namespace
