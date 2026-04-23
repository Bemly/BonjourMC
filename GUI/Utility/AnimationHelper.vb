Option Explicit On
Option Strict On

Imports System
Imports Avalonia
Imports Avalonia.Animation
Imports Avalonia.Animation.Easings
Imports Avalonia.Controls
Imports Avalonia.Media
Imports Avalonia.Styling

Namespace Animations

    ' ============================================================
    ' Custom Easing Functions (PCL-CE style)
    ' ============================================================

    ''' <summary>
    ''' Overshoot easing — goes past target then settles (PCL-CE AniEaseOutBack)
    ''' </summary>
    Public Class EaseOutBack
        Inherits Easing

        Public Overrides Function Ease(ByVal progress As Double) As Double
            Dim c1 = 1.70158
            Dim c3 = c1 + 1
            Return 1 + c3 * Math.Pow(progress - 1, 3) + c1 * Math.Pow(progress - 1, 2)
        End Function
    End Class

    ''' <summary>
    ''' Spring/elastic effect (PCL-CE AniEaseOutElastic)
    ''' </summary>
    Public Class EaseOutElastic
        Inherits Easing

        Public Overrides Function Ease(ByVal progress As Double) As Double
            If progress = 0 OrElse progress = 1 Then Return progress
            Dim p = 0.3
            Return Math.Pow(2, -10 * progress) * Math.Sin((progress - p / 4) * (2 * Math.PI) / p) + 1
        End Function
    End Class

    ''' <summary>
    ''' Smooth deceleration (PCL-CE AniEaseOutFluent)
    ''' </summary>
    Public Class EaseOutFluent
        Inherits Easing

        Public Overrides Function Ease(ByVal progress As Double) As Double
            Return 1 - Math.Pow(1 - progress, 3)
        End Function
    End Class

    ''' <summary>
    ''' Smooth acceleration (PCL-CE AniEaseInFluent)
    ''' </summary>
    Public Class EaseInFluent
        Inherits Easing

        Public Overrides Function Ease(ByVal progress As Double) As Double
            Return progress * progress * progress
        End Function
    End Class

    ''' <summary>
    ''' Smooth acceleration + deceleration (PCL-CE AniEaseInoutFluent)
    ''' </summary>
    Public Class EaseInOutFluent
        Inherits Easing

        Public Overrides Function Ease(ByVal progress As Double) As Double
            If progress < 0.5 Then
                Return 4 * progress * progress * progress
            Else
                Return 1 - Math.Pow(-2 * progress + 2, 3) / 2
            End If
        End Function
    End Class

    ' ============================================================
    ' Animation Helper Methods
    ' ============================================================

    ''' <summary>
    ''' Reusable animation helpers for PCL-CE style transitions.
    ''' </summary>
    Public Module AnimationHelper

        ' --- Shared easing instances ---
        Public ReadOnly ease_out_back As New EaseOutBack()
        Public ReadOnly ease_out_elastic As New EaseOutElastic()
        Public ReadOnly ease_out_fluent As New EaseOutFluent()
        Public ReadOnly ease_in_fluent As New EaseInFluent()
        Public ReadOnly ease_in_out_fluent As New EaseInOutFluent()
        Public ReadOnly ease_cubic_out As New CubicEaseOut()
        Public ReadOnly ease_quad_out As New QuadraticEaseOut()

        ' --- Helper to add a Setter to a KeyFrame ---
        Private Sub add_setter(ByVal kf As KeyFrame, ByVal [property] As AvaloniaProperty, ByVal value As Object)
            kf.Setters.Add(New Setter([property], value))
        End Sub

        ' --- Opacity ---

        ''' <summary>
        ''' Fade a control to a target opacity.
        ''' </summary>
        Public Sub fade(ByVal target As Control, ByVal to_opacity As Double,
                        Optional ByVal duration_ms As Integer = 200,
                        Optional ByVal delay_ms As Integer = 0,
                        Optional ByVal easing As Easing = Nothing)
            If easing Is Nothing Then easing = ease_out_fluent
            Dim anim = New Animation()
            anim.Duration = TimeSpan.FromMilliseconds(duration_ms)
            anim.Delay = TimeSpan.FromMilliseconds(delay_ms)
            anim.FillMode = FillMode.Forward
            anim.Easing = easing
            Dim kf0 As New KeyFrame() With {.Cue = New Cue(0)}
            add_setter(kf0, Visual.OpacityProperty, target.Opacity)
            anim.Children.Add(kf0)
            Dim kf1 As New KeyFrame() With {.Cue = New Cue(1)}
            add_setter(kf1, Visual.OpacityProperty, to_opacity)
            anim.Children.Add(kf1)
            Dim token = anim.RunAsync(target)
        End Sub

        ' --- Translate ---

        ''' <summary>
        ''' Slide a control horizontally.
        ''' </summary>
        Public Sub translate_x(ByVal target As Control, ByVal delta_x As Double,
                               Optional ByVal duration_ms As Integer = 300,
                               Optional ByVal delay_ms As Integer = 0,
                               Optional ByVal easing As Easing = Nothing)
            If easing Is Nothing Then easing = ease_out_fluent
            ensure_translate_transform(target)
            Dim current = get_translate_x(target)
            Dim anim = New Animation()
            anim.Duration = TimeSpan.FromMilliseconds(duration_ms)
            anim.Delay = TimeSpan.FromMilliseconds(delay_ms)
            anim.FillMode = FillMode.Forward
            anim.Easing = easing
            Dim kf0 As New KeyFrame() With {.Cue = New Cue(0)}
            add_setter(kf0, TranslateTransform.XProperty, current)
            anim.Children.Add(kf0)
            Dim kf1 As New KeyFrame() With {.Cue = New Cue(1)}
            add_setter(kf1, TranslateTransform.XProperty, current + delta_x)
            anim.Children.Add(kf1)
            Dim token = anim.RunAsync(target)
        End Sub

        ''' <summary>
        ''' Slide a control vertically.
        ''' </summary>
        Public Sub translate_y(ByVal target As Control, ByVal delta_y As Double,
                               Optional ByVal duration_ms As Integer = 300,
                               Optional ByVal delay_ms As Integer = 0,
                               Optional ByVal easing As Easing = Nothing)
            If easing Is Nothing Then easing = ease_out_fluent
            ensure_translate_transform(target)
            Dim current = get_translate_y(target)
            Dim anim = New Animation()
            anim.Duration = TimeSpan.FromMilliseconds(duration_ms)
            anim.Delay = TimeSpan.FromMilliseconds(delay_ms)
            anim.FillMode = FillMode.Forward
            anim.Easing = easing
            Dim kf0 As New KeyFrame() With {.Cue = New Cue(0)}
            add_setter(kf0, TranslateTransform.YProperty, current)
            anim.Children.Add(kf0)
            Dim kf1 As New KeyFrame() With {.Cue = New Cue(1)}
            add_setter(kf1, TranslateTransform.YProperty, current + delta_y)
            anim.Children.Add(kf1)
            Dim token = anim.RunAsync(target)
        End Sub

        ' --- Scale ---

        ''' <summary>
        ''' Scale a control uniformly on both axes.
        ''' </summary>
        Public Sub scale(ByVal target As Control, ByVal to_scale As Double,
                         Optional ByVal duration_ms As Integer = 200,
                         Optional ByVal delay_ms As Integer = 0,
                         Optional ByVal easing As Easing = Nothing)
            If easing Is Nothing Then easing = ease_out_fluent
            ensure_scale_transform(target)
            Dim anim = New Animation()
            anim.Duration = TimeSpan.FromMilliseconds(duration_ms)
            anim.Delay = TimeSpan.FromMilliseconds(delay_ms)
            anim.FillMode = FillMode.Forward
            anim.Easing = easing
            Dim kf0 As New KeyFrame() With {.Cue = New Cue(0)}
            add_setter(kf0, ScaleTransform.ScaleXProperty, get_scale_x(target))
            add_setter(kf0, ScaleTransform.ScaleYProperty, get_scale_y(target))
            anim.Children.Add(kf0)
            Dim kf1 As New KeyFrame() With {.Cue = New Cue(1)}
            add_setter(kf1, ScaleTransform.ScaleXProperty, to_scale)
            add_setter(kf1, ScaleTransform.ScaleYProperty, to_scale)
            anim.Children.Add(kf1)
            Dim token = anim.RunAsync(target)
        End Sub

        ' --- Combined: Fade + Slide Up ---

        ''' <summary>
        ''' Fade in and slide up from offset (PCL-CE right panel card enter).
        ''' </summary>
        Public Sub fade_slide_up(ByVal target As Control,
                                 Optional ByVal fade_to As Double = 1.0,
                                 Optional ByVal slide_offset As Double = 16,
                                 Optional ByVal duration_ms As Integer = 350,
                                 Optional ByVal delay_ms As Integer = 0,
                                 Optional ByVal easing As Easing = Nothing)
            If easing Is Nothing Then easing = ease_out_fluent
            ensure_translate_transform(target)
            target.Opacity = 0
            set_translate_y(target, slide_offset)
            Dim anim = New Animation()
            anim.Duration = TimeSpan.FromMilliseconds(duration_ms)
            anim.Delay = TimeSpan.FromMilliseconds(delay_ms)
            anim.FillMode = FillMode.Forward
            anim.Easing = easing
            Dim kf0 As New KeyFrame() With {.Cue = New Cue(0)}
            add_setter(kf0, Visual.OpacityProperty, 0.0)
            add_setter(kf0, TranslateTransform.YProperty, slide_offset)
            anim.Children.Add(kf0)
            Dim kf1 As New KeyFrame() With {.Cue = New Cue(1)}
            add_setter(kf1, Visual.OpacityProperty, fade_to)
            add_setter(kf1, TranslateTransform.YProperty, 0.0)
            anim.Children.Add(kf1)
            Dim token = anim.RunAsync(target)
        End Sub

        ''' <summary>
        ''' Fade out and slide up (PCL-CE right panel card exit).
        ''' </summary>
        Public Sub fade_slide_up_exit(ByVal target As Control,
                                      Optional ByVal slide_offset As Double = -6,
                                      Optional ByVal duration_ms As Integer = 70,
                                      Optional ByVal delay_ms As Integer = 0)
            ensure_translate_transform(target)
            Dim anim = New Animation()
            anim.Duration = TimeSpan.FromMilliseconds(duration_ms)
            anim.Delay = TimeSpan.FromMilliseconds(delay_ms)
            anim.FillMode = FillMode.Forward
            anim.Easing = ease_in_fluent
            Dim kf0 As New KeyFrame() With {.Cue = New Cue(0)}
            add_setter(kf0, Visual.OpacityProperty, target.Opacity)
            add_setter(kf0, TranslateTransform.YProperty, get_translate_y(target))
            anim.Children.Add(kf0)
            Dim kf1 As New KeyFrame() With {.Cue = New Cue(1)}
            add_setter(kf1, Visual.OpacityProperty, 0.0)
            add_setter(kf1, TranslateTransform.YProperty, get_translate_y(target) + slide_offset)
            anim.Children.Add(kf1)
            Dim token = anim.RunAsync(target)
        End Sub

        ' --- Combined: Fade + Slide Left ---

        ''' <summary>
        ''' Fade in and slide from left (PCL-CE left panel item enter).
        ''' </summary>
        Public Sub fade_slide_left(ByVal target As Control,
                                   Optional ByVal fade_to As Double = 1.0,
                                   Optional ByVal slide_offset As Double = -25,
                                   Optional ByVal duration_ms As Integer = 300,
                                   Optional ByVal delay_ms As Integer = 0,
                                   Optional ByVal easing As Easing = Nothing)
            If easing Is Nothing Then easing = ease_out_fluent
            ensure_translate_transform(target)
            target.Opacity = 0
            set_translate_x(target, slide_offset)
            Dim anim = New Animation()
            anim.Duration = TimeSpan.FromMilliseconds(duration_ms)
            anim.Delay = TimeSpan.FromMilliseconds(delay_ms)
            anim.FillMode = FillMode.Forward
            anim.Easing = easing
            Dim kf0 As New KeyFrame() With {.Cue = New Cue(0)}
            add_setter(kf0, Visual.OpacityProperty, 0.0)
            add_setter(kf0, TranslateTransform.XProperty, slide_offset)
            anim.Children.Add(kf0)
            Dim kf1 As New KeyFrame() With {.Cue = New Cue(1)}
            add_setter(kf1, Visual.OpacityProperty, fade_to)
            add_setter(kf1, TranslateTransform.XProperty, 0.0)
            anim.Children.Add(kf1)
            Dim token = anim.RunAsync(target)
        End Sub

        ' --- Scale + Fade Enter ---

        ''' <summary>
        ''' Scale up from small + fade in (PCL-CE button/card show).
        ''' </summary>
        Public Sub scale_fade_enter(ByVal target As Control,
                                    Optional ByVal from_scale As Double = 0.96,
                                    Optional ByVal duration_ms As Integer = 400,
                                    Optional ByVal delay_ms As Integer = 0,
                                    Optional ByVal easing As Easing = Nothing)
            If easing Is Nothing Then easing = ease_out_back
            ensure_scale_transform(target)
            target.Opacity = 0
            set_scale_x(target, from_scale)
            set_scale_y(target, from_scale)
            Dim anim = New Animation()
            anim.Duration = TimeSpan.FromMilliseconds(duration_ms)
            anim.Delay = TimeSpan.FromMilliseconds(delay_ms)
            anim.FillMode = FillMode.Forward
            anim.Easing = easing
            Dim kf0 As New KeyFrame() With {.Cue = New Cue(0)}
            add_setter(kf0, Visual.OpacityProperty, 0.0)
            add_setter(kf0, ScaleTransform.ScaleXProperty, from_scale)
            add_setter(kf0, ScaleTransform.ScaleYProperty, from_scale)
            anim.Children.Add(kf0)
            Dim kf1 As New KeyFrame() With {.Cue = New Cue(1)}
            add_setter(kf1, Visual.OpacityProperty, 1.0)
            add_setter(kf1, ScaleTransform.ScaleXProperty, 1.0)
            add_setter(kf1, ScaleTransform.ScaleYProperty, 1.0)
            anim.Children.Add(kf1)
            Dim token = anim.RunAsync(target)
        End Sub

        ' --- Staggered Animation Helper ---

        ''' <summary>
        ''' Apply staggered fade+slide-up animation to a list of controls.
        ''' </summary>
        Public Sub staggered_fade_slide_up(ByVal controls As IEnumerable(Of Control),
                                            Optional ByVal base_delay_ms As Integer = 0,
                                            Optional ByVal stagger_ms As Integer = 25,
                                            Optional ByVal duration_ms As Integer = 350)
            Dim delay = base_delay_ms
            For Each ctrl As Control In controls
                fade_slide_up(ctrl, 1.0, 16, duration_ms, delay, ease_out_fluent)
                delay += stagger_ms
            Next
        End Sub

        ''' <summary>
        ''' Apply staggered fade+slide-left animation to a list of controls.
        ''' </summary>
        Public Sub staggered_fade_slide_left(ByVal controls As IEnumerable(Of Control),
                                              Optional ByVal base_delay_ms As Integer = 0,
                                              Optional ByVal stagger_ms As Integer = 20,
                                              Optional ByVal duration_ms As Integer = 300)
            Dim delay = base_delay_ms
            For Each ctrl As Control In controls
                fade_slide_left(ctrl, 1.0, -25, duration_ms, delay, ease_out_fluent)
                delay += stagger_ms
            Next
        End Sub

        ' --- Transform Helpers ---

        Private Sub ensure_translate_transform(ByVal target As Control)
            If target.RenderTransform Is Nothing OrElse Not TypeOf target.RenderTransform Is TranslateTransform Then
                target.RenderTransform = New TranslateTransform(0, 0)
            End If
        End Sub

        Private Sub ensure_scale_transform(ByVal target As Control)
            If target.RenderTransform Is Nothing OrElse Not TypeOf target.RenderTransform Is ScaleTransform Then
                target.RenderTransform = New ScaleTransform(1, 1)
            End If
        End Sub

        Private Function get_translate_x(ByVal target As Control) As Double
            Dim tt = TryCast(target.RenderTransform, TranslateTransform)
            If tt IsNot Nothing Then Return tt.X
            Return 0
        End Function

        Private Function get_translate_y(ByVal target As Control) As Double
            Dim tt = TryCast(target.RenderTransform, TranslateTransform)
            If tt IsNot Nothing Then Return tt.Y
            Return 0
        End Function

        Private Sub set_translate_x(ByVal target As Control, ByVal value As Double)
            Dim tt = TryCast(target.RenderTransform, TranslateTransform)
            If tt IsNot Nothing Then tt.X = value
        End Sub

        Private Sub set_translate_y(ByVal target As Control, ByVal value As Double)
            Dim tt = TryCast(target.RenderTransform, TranslateTransform)
            If tt IsNot Nothing Then tt.Y = value
        End Sub

        Private Function get_scale_x(ByVal target As Control) As Double
            Dim st = TryCast(target.RenderTransform, ScaleTransform)
            If st IsNot Nothing Then Return st.ScaleX
            Return 1
        End Function

        Private Function get_scale_y(ByVal target As Control) As Double
            Dim st = TryCast(target.RenderTransform, ScaleTransform)
            If st IsNot Nothing Then Return st.ScaleY
            Return 1
        End Function

        Private Sub set_scale_x(ByVal target As Control, ByVal value As Double)
            Dim st = TryCast(target.RenderTransform, ScaleTransform)
            If st IsNot Nothing Then st.ScaleX = value
        End Sub

        Private Sub set_scale_y(ByVal target As Control, ByVal value As Double)
            Dim st = TryCast(target.RenderTransform, ScaleTransform)
            If st IsNot Nothing Then st.ScaleY = value
        End Sub

    End Module

End Namespace
