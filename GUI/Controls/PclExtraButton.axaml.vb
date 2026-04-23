Option Explicit On
Option Strict On

Imports System
Imports Avalonia
Imports Avalonia.Animation
Imports Avalonia.Controls
Imports Avalonia.Controls.Shapes
Imports Avalonia.Input
Imports Avalonia.Media
Imports Avalonia.Styling
Imports Avalonia.Threading
Imports GUI.Animations

Namespace Controls

    ''' <summary>
    ''' PCL-CE PclExtraButton style — floating action button with show/hide scale bounce,
    ''' click squash, and ripple effect.
    ''' </summary>
    Partial Public Class PclExtraButton
        Inherits UserControl

        ' Instance brushes
        Private ReadOnly _color_brush As New SolidColorBrush(Color.Parse("#1370f3"))
        Private ReadOnly _fill_brush As New SolidColorBrush(Color.Parse("#eaf2fe"))

        ' State
        Private _is_mouse_down As Boolean = False
        Private _is_shown As Boolean = False

        ' Styled Properties
        Public Shared ReadOnly LogoProperty As StyledProperty(Of String) =
            AvaloniaProperty.Register(Of PclExtraButton, String)("Logo", "")

        Public Shared ReadOnly ShowProperty As StyledProperty(Of Boolean) =
            AvaloniaProperty.Register(Of PclExtraButton, Boolean)("Show", False)

        Public Property Logo As String
            Get
                Return GetValue(LogoProperty)
            End Get
            Set(ByVal value As String)
                SetValue(LogoProperty, value)
                If PART_Icon IsNot Nothing Then
                    Try
                        PART_Icon.Data = Geometry.Parse(value)
                    Catch
                    End Try
                End If
            End Set
        End Property

        Public Property Show As Boolean
            Get
                Return GetValue(ShowProperty)
            End Get
            Set(ByVal value As Boolean)
                If GetValue(ShowProperty) = value Then Return
                SetValue(ShowProperty, value)
                If value Then
                    AnimateShow()
                Else
                    AnimateHide()
                End If
            End Set
        End Property

        Public Sub New()
            InitializeComponent()
        End Sub

        Protected Overrides Sub OnAttachedToVisualTree(ByVal e As VisualTreeAttachmentEventArgs)
            MyBase.OnAttachedToVisualTree(e)
            If PART_Color IsNot Nothing Then
                PART_Color.Background = _color_brush
            End If
            If PART_Icon IsNot Nothing Then
                PART_Icon.Fill = _fill_brush
                If Not String.IsNullOrEmpty(Logo) Then
                    Try
                        PART_Icon.Data = Geometry.Parse(Logo)
                    Catch
                    End Try
                End If
            End If
        End Sub

        ' --- Show Animation (PCL-CE: 500ms OutBack, 60ms delay) ---
        Private Sub AnimateShow()
            If _is_shown Then Return
            _is_shown = True

            ' Scale from 0.3 to 1.0 with overshoot
            AnimationHelper.scale_to(Me, 0.3, 1, 0, AnimationHelper.ease_linear) ' snap to 0.3
            AnimationHelper.scale_to(Me, 1.0, 500, 60, AnimationHelper.ease_out_back_weak)

            ' Height expand
            Dim height_anim As New Animation()
            height_anim.Duration = TimeSpan.FromMilliseconds(200)
            height_anim.FillMode = FillMode.Forward
            height_anim.Easing = AnimationHelper.ease_out_fluent_weak
            Dim kf0 As New KeyFrame() With {.Cue = New Cue(0)}
            kf0.Setters.Add(New Setter(HeightProperty, 0.0))
            height_anim.Children.Add(kf0)
            Dim kf1 As New KeyFrame() With {.Cue = New Cue(1)}
            kf1.Setters.Add(New Setter(HeightProperty, 50.0))
            height_anim.Children.Add(kf1)
            Dim token = height_anim.RunAsync(Me)
        End Sub

        ' --- Hide Animation (PCL-CE: 100ms InFluent) ---
        Private Sub AnimateHide()
            If Not _is_shown Then Return
            _is_shown = False

            AnimationHelper.scale_to(Me, 0.0, 100, 0, AnimationHelper.ease_in_fluent)

            AnimationHelper.delayed_code(Sub()
                                             Dim height_anim As New Animation()
                                             height_anim.Duration = TimeSpan.FromMilliseconds(400)
                                             height_anim.FillMode = FillMode.Forward
                                             height_anim.Easing = AnimationHelper.ease_out_fluent
                                             Dim kf0 As New KeyFrame() With {.Cue = New Cue(0)}
                                             kf0.Setters.Add(New Setter(HeightProperty, 50.0))
                                             height_anim.Children.Add(kf0)
                                             Dim kf1 As New KeyFrame() With {.Cue = New Cue(1)}
                                             kf1.Setters.Add(New Setter(HeightProperty, 0.0))
                                             height_anim.Children.Add(kf1)
                                             Dim token = height_anim.RunAsync(Me)
                                         End Sub, 100)
        End Sub

        ' --- Click Animation ---
        Protected Overrides Sub OnPointerEntered(ByVal e As PointerEventArgs)
            MyBase.OnPointerEntered(e)
            If PART_Color IsNot Nothing Then
                AnimationHelper.color(_color_brush, Color.Parse("#4890f5"), 120) ' Brush4
            End If
        End Sub

        Protected Overrides Sub OnPointerExited(ByVal e As PointerEventArgs)
            MyBase.OnPointerExited(e)
            _is_mouse_down = False
            If PART_Color IsNot Nothing Then
                AnimationHelper.color(_color_brush, Color.Parse("#1370f3"), 150) ' Brush3
            End If
            If PART_Scale IsNot Nothing Then
                AnimationHelper.scale_to(PART_Scale, 1.0, 500, 0, AnimationHelper.ease_out_fluent)
            End If
        End Sub

        Protected Overrides Sub OnPointerPressed(ByVal e As PointerPressedEventArgs)
            MyBase.OnPointerPressed(e)
            _is_mouse_down = True
            If PART_Scale IsNot Nothing Then
                AnimationHelper.scale_to(PART_Scale, 0.85, 800, 0, AnimationHelper.ease_out_fluent_strong)
                AnimationHelper.scale_delta(PART_Scale, -0.05, 60, 0, AnimationHelper.ease_out_fluent)
            End If
        End Sub

        Protected Overrides Sub OnPointerReleased(ByVal e As PointerReleasedEventArgs)
            MyBase.OnPointerReleased(e)
            If _is_mouse_down Then
                _is_mouse_down = False
                If PART_Scale IsNot Nothing Then
                    AnimationHelper.scale_to(PART_Scale, 1.0, 300, 0, AnimationHelper.ease_out_back)
                End If
                ' Ripple effect
                CreateRipple()
            End If
        End Sub

        ' --- Ripple Effect ---
        Private Sub CreateRipple()
            If PART_Grid Is Nothing Then Return
            Dim ripple As New Border() With {
                .CornerRadius = New CornerRadius(1000),
                .Background = New SolidColorBrush(Color.FromArgb(80, 255, 255, 255)),
                .HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                .VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                .Width = 10,
                .Height = 10,
                .RenderTransform = New ScaleTransform(1, 1),
                .RenderTransformOrigin = New RelativePoint(0.5, 0.5, RelativeUnit.Relative)
            }
            PART_Grid.Children.Add(ripple)

            ' Scale up + fade out
            AnimationHelper.scale_to(ripple, 13.0, 1000, 0, New EaseInOutFluent(AniEasePower.Strong, 0.3))
            AnimationHelper.fade(ripple, 0.0, 1000, 0, AnimationHelper.ease_linear)

            ' Remove after animation
            AnimationHelper.delayed_code(Sub()
                                             PART_Grid.Children.Remove(ripple)
                                         End Sub, 1050)
        End Sub
    End Class

End Namespace
