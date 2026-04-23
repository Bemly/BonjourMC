Option Explicit On
Option Strict On

Imports Avalonia
Imports Avalonia.Controls
Imports Avalonia.Input
Imports Avalonia.Media

Namespace Controls

    ''' <summary>
    ''' PCL-CE style card with animated hover shadow effect.
    ''' Extends Border with DropShadowEffect that animates on pointer enter/leave.
    ''' </summary>
    Public Class Card
        Inherits Border

        Private _is_hovered As Boolean = False

        Public Sub New()
            Background = New SolidColorBrush(Color.Parse("#ffffff"))
            CornerRadius = New CornerRadius(8)
            Padding = New Thickness(20)
            Margin = New Thickness(0, 0, 0, 12)
            ClipToBounds = False
            Effect = New DropShadowEffect() With {
                .BlurRadius = 12,
                .OffsetY = 1,
                .Opacity = 0.08,
                .Color = Color.Parse("#3c3c3c")
            }
        End Sub

        Protected Overrides Sub OnPointerEntered(ByVal e As PointerEventArgs)
            MyBase.OnPointerEntered(e)
            If _is_hovered Then Return
            _is_hovered = True

            Dim shadow = TryCast(Effect, DropShadowEffect)
            If shadow IsNot Nothing Then
                shadow.BlurRadius = 20
                shadow.OffsetY = 2
                shadow.Opacity = 0.18
            End If
        End Sub

        Protected Overrides Sub OnPointerExited(ByVal e As PointerEventArgs)
            MyBase.OnPointerExited(e)
            If Not _is_hovered Then Return
            _is_hovered = False

            Dim shadow = TryCast(Effect, DropShadowEffect)
            If shadow IsNot Nothing Then
                shadow.BlurRadius = 12
                shadow.OffsetY = 1
                shadow.Opacity = 0.08
            End If
        End Sub
    End Class

End Namespace
