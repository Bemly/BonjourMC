Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Concurrent
Imports System.Threading
Imports Avalonia
Imports Avalonia.Animation
Imports Avalonia.Animation.Easings
Imports Avalonia.Controls
Imports Avalonia.Media
Imports Avalonia.Styling
Imports Avalonia.Threading

Namespace Animations

    ' ============================================================
    ' PCL-CE AniEasePower equivalent
    ' ============================================================

    ''' <summary>
    ''' Animation power levels matching PCL-CE AniEasePower enum.
    ''' Weak=2, Middle=3, Strong=4, ExtraStrong=5
    ''' </summary>
    Public Enum AniEasePower As Integer
        Weak = 2
        Middle = 3
        Strong = 4
        ExtraStrong = 5
    End Enum

    ' ============================================================
    ' Custom Easing Functions (PCL-CE exact formulas)
    ' ============================================================

    ''' <summary>
    ''' PCL-CE AniEaseLinear — no easing.
    ''' </summary>
    Public Class EaseLinear
        Inherits Easing
        Public Overrides Function Ease(ByVal progress As Double) As Double
            Return Math.Max(0, Math.Min(1, progress))
        End Function
    End Class

    ''' <summary>
    ''' PCL-CE AniEaseInFluent — smooth start. t^p
    ''' </summary>
    Public Class EaseInFluent
        Inherits Easing
        Private ReadOnly _p As Double
        Public Sub New()
            _p = 3
        End Sub
        Public Sub New(ByVal power As AniEasePower)
            _p = CDbl(power)
        End Sub
        Public Sub New(ByVal power As Integer)
            _p = CDbl(power)
        End Sub
        Public Overrides Function Ease(ByVal progress As Double) As Double
            Dim t = Math.Max(0, Math.Min(1, progress))
            Return Math.Pow(t, _p)
        End Function
    End Class

    ''' <summary>
    ''' PCL-CE AniEaseOutFluent — smooth end. 1-(1-t)^p
    ''' </summary>
    Public Class EaseOutFluent
        Inherits Easing
        Private ReadOnly _p As Double
        Public Sub New()
            _p = 3
        End Sub
        Public Sub New(ByVal power As AniEasePower)
            _p = CDbl(power)
        End Sub
        Public Sub New(ByVal power As Integer)
            _p = CDbl(power)
        End Sub
        Public Overrides Function Ease(ByVal progress As Double) As Double
            Dim t = Math.Max(0, Math.Min(1, progress))
            Return 1 - Math.Pow(1 - t, _p)
        End Function
    End Class

    ''' <summary>
    ''' PCL-CE AniEaseInoutFluent — smooth start and end.
    ''' </summary>
    Public Class EaseInOutFluent
        Inherits Easing
        Private ReadOnly _p As Double
        Private ReadOnly _mid As Double
        Public Sub New()
            _p = 3
            _mid = 0.5
        End Sub
        Public Sub New(ByVal power As AniEasePower, Optional ByVal middle As Double = 0.5)
            _p = CDbl(power)
            _mid = middle
        End Sub
        Public Sub New(ByVal power As Integer, Optional ByVal middle As Double = 0.5)
            _p = CDbl(power)
            _mid = middle
        End Sub
        Public Overrides Function Ease(ByVal progress As Double) As Double
            Dim t = Math.Max(0, Math.Min(1, progress))
            If t < _mid Then
                Return _mid * Math.Pow(t / _mid, _p)
            Else
                Return (1 - _mid) * (1 - Math.Pow(1 - (t - _mid) / (1 - _mid), _p)) + _mid
            End If
        End Function
    End Class

    ''' <summary>
    ''' PCL-CE AniEaseInBack — overshoot start. t^p * cos(1.5π*(1-t))
    ''' </summary>
    Public Class EaseInBack
        Inherits Easing
        Private ReadOnly _p As Double
        Public Sub New()
            _p = 1.5
        End Sub
        Public Sub New(ByVal power As AniEasePower)
            _p = 3 - CDbl(power) * 0.5
        End Sub
        Public Overrides Function Ease(ByVal progress As Double) As Double
            Dim t = Math.Max(0, Math.Min(1, progress))
            Return Math.Pow(t, _p) * Math.Cos(1.5 * Math.PI * (1 - t))
        End Function
    End Class

    ''' <summary>
    ''' PCL-CE AniEaseOutBack — overshoot end. 1-(1-t)^p*cos(1.5π*t)
    ''' </summary>
    Public Class EaseOutBack
        Inherits Easing
        Private ReadOnly _p As Double
        Public Sub New()
            _p = 1.5
        End Sub
        Public Sub New(ByVal power As AniEasePower)
            _p = 3 - CDbl(power) * 0.5
        End Sub
        Public Overrides Function Ease(ByVal progress As Double) As Double
            Dim t = Math.Max(0, Math.Min(1, progress))
            Return 1 - Math.Pow(1 - t, _p) * Math.Cos(1.5 * Math.PI * t)
        End Function
    End Class

    ''' <summary>
    ''' PCL-CE AniEaseOutCar — short smooth then overshoot.
    ''' InFluent(30%) + OutBack(70%)
    ''' </summary>
    Public Class EaseOutCar
        Inherits Easing
        Private ReadOnly _power As AniEasePower
        Private ReadOnly _mid As Double
        Public Sub New()
            _power = AniEasePower.Middle
            _mid = 0.3
        End Sub
        Public Sub New(Optional ByVal middle As Double = 0.3, Optional ByVal power As AniEasePower = AniEasePower.Middle)
            _power = power
            _mid = middle
        End Sub
        Public Overrides Function Ease(ByVal progress As Double) As Double
            Dim t = Math.Max(0, Math.Min(1, progress))
            If t < _mid Then
                Return _mid * Math.Pow(t / _mid, CDbl(_power))
            Else
                Dim t2 = (t - _mid) / (1 - _mid)
                Dim p2 = 3 - CDbl(_power) * 0.5
                Return (1 - _mid) * (1 - Math.Pow(1 - t2, p2) * Math.Cos(1.5 * Math.PI * t2)) + _mid
            End If
        End Function
    End Class

    ''' <summary>
    ''' PCL-CE AniEaseInElastic — spring start.
    ''' </summary>
    Public Class EaseInElastic
        Inherits Easing
        Private ReadOnly _p As Integer
        Public Sub New()
            _p = 7
        End Sub
        Public Sub New(ByVal power As AniEasePower)
            _p = CInt(power) + 4
        End Sub
        Public Overrides Function Ease(ByVal progress As Double) As Double
            Dim t = Math.Max(0, Math.Min(1, progress))
            Return Math.Pow(t, (_p - 1) * 0.25) * Math.Cos((_p - 3.5) * Math.PI * Math.Pow(1 - t, 1.5))
        End Function
    End Class

    ''' <summary>
    ''' PCL-CE AniEaseOutElastic — spring end.
    ''' </summary>
    Public Class EaseOutElastic
        Inherits Easing
        Private ReadOnly _p As Integer
        Public Sub New()
            _p = 7
        End Sub
        Public Sub New(ByVal power As AniEasePower)
            _p = CInt(power) + 4
        End Sub
        Public Overrides Function Ease(ByVal progress As Double) As Double
            Dim t = 1 - Math.Max(0, Math.Min(1, progress))
            Return 1 - Math.Pow(t, (_p - 1) * 0.25) * Math.Cos((_p - 3.5) * Math.PI * Math.Pow(1 - t, 1.5))
        End Function
    End Class

    ' ============================================================
    ' Animation Helper Methods
    ' ============================================================

    ''' <summary>
    ''' Reusable animation helpers matching PCL-CE animation patterns.
    ''' </summary>
    Public Module AnimationHelper

        ' --- Shared easing instances (default power = Middle/3) ---
        Public ReadOnly ease_linear As New EaseLinear()
        Public ReadOnly ease_in_fluent As New EaseInFluent()
        Public ReadOnly ease_out_fluent As New EaseOutFluent()
        Public ReadOnly ease_in_out_fluent As New EaseInOutFluent()
        Public ReadOnly ease_in_back As New EaseInBack()
        Public ReadOnly ease_out_back As New EaseOutBack()
        Public ReadOnly ease_out_car As New EaseOutCar()
        Public ReadOnly ease_in_elastic As New EaseInElastic()
        Public ReadOnly ease_out_elastic As New EaseOutElastic()

        ' Power-parameterized instances
        Public ReadOnly ease_out_fluent_weak As New EaseOutFluent(AniEasePower.Weak)
        Public ReadOnly ease_out_fluent_strong As New EaseOutFluent(AniEasePower.Strong)
        Public ReadOnly ease_out_fluent_extra As New EaseOutFluent(AniEasePower.ExtraStrong)
        Public ReadOnly ease_in_fluent_weak As New EaseInFluent(AniEasePower.Weak)
        Public ReadOnly ease_in_fluent_strong As New EaseInFluent(AniEasePower.Strong)
        Public ReadOnly ease_out_back_weak As New EaseOutBack(AniEasePower.Weak)
        Public ReadOnly ease_in_back_weak As New EaseInBack(AniEasePower.Weak)
        Public ReadOnly ease_out_elastic_weak As New EaseOutElastic(AniEasePower.Weak)

        ' --- Animation cancellation ---
        Private ReadOnly _animation_cts As New ConcurrentDictionary(Of String, CancellationTokenSource)

        ''' <summary>
        ''' Cancel a named animation group.
        ''' </summary>
        Public Sub cancel(ByVal name As String)
            Dim cts As CancellationTokenSource = Nothing
            If _animation_cts.TryRemove(name, cts) Then
                cts.Cancel()
                cts.Dispose()
            End If
        End Sub

        ''' <summary>
        ''' Cancel all animations.
        ''' </summary>
        Public Sub cancel_all()
            For Each kvp In _animation_cts
                kvp.Value.Cancel()
                kvp.Value.Dispose()
            Next
            _animation_cts.Clear()
        End Sub

        ' --- Helper to add a Setter to a KeyFrame ---
        Private Sub add_setter(ByVal kf As KeyFrame, ByVal [property] As AvaloniaProperty, ByVal value As Object)
            kf.Setters.Add(New Setter([property], value))
        End Sub

        ' --- Transform helpers ---

        Public Sub ensure_translate_transform(ByVal target As Visual)
            If target.RenderTransform Is Nothing OrElse Not TypeOf target.RenderTransform Is TranslateTransform Then
                target.RenderTransform = New TranslateTransform(0, 0)
            End If
        End Sub

        Public Sub ensure_scale_transform(ByVal target As Visual)
            If target.RenderTransform Is Nothing OrElse Not TypeOf target.RenderTransform Is ScaleTransform Then
                target.RenderTransform = New ScaleTransform(1, 1)
                target.RenderTransformOrigin = New RelativePoint(0.5, 0.5, RelativeUnit.Relative)
            End If
        End Sub

        Public Sub ensure_rotate_transform(ByVal target As Visual)
            If target.RenderTransform Is Nothing OrElse Not TypeOf target.RenderTransform Is RotateTransform Then
                target.RenderTransform = New RotateTransform(0)
                target.RenderTransformOrigin = New RelativePoint(0.5, 0.5, RelativeUnit.Relative)
            End If
        End Sub

        Public Sub ensure_transform_group(ByVal target As Visual, ByVal types() As Type)
            Dim group = TryCast(target.RenderTransform, TransformGroup)
            If group Is Nothing OrElse group.Children.Count <> types.Length Then
                group = New TransformGroup()
                For Each t In types
                    If t Is GetType(TranslateTransform) Then
                        group.Children.Add(New TranslateTransform(0, 0))
                    ElseIf t Is GetType(ScaleTransform) Then
                        group.Children.Add(New ScaleTransform(1, 1))
                    ElseIf t Is GetType(RotateTransform) Then
                        group.Children.Add(New RotateTransform(0))
                    End If
                Next
                target.RenderTransform = group
                target.RenderTransformOrigin = New RelativePoint(0.5, 0.5, RelativeUnit.Relative)
            End If
        End Sub

        Public Function get_translate_x(ByVal target As Visual) As Double
            Dim tt = TryCast(target.RenderTransform, TranslateTransform)
            If tt IsNot Nothing Then Return tt.X
            Return 0
        End Function

        Public Function get_translate_y(ByVal target As Visual) As Double
            Dim tt = TryCast(target.RenderTransform, TranslateTransform)
            If tt IsNot Nothing Then Return tt.Y
            Return 0
        End Function

        Public Sub set_translate_x(ByVal target As Visual, ByVal value As Double)
            Dim tt = TryCast(target.RenderTransform, TranslateTransform)
            If tt IsNot Nothing Then tt.X = value
        End Sub

        Public Sub set_translate_y(ByVal target As Visual, ByVal value As Double)
            Dim tt = TryCast(target.RenderTransform, TranslateTransform)
            If tt IsNot Nothing Then tt.Y = value
        End Sub

        Public Function get_scale_x(ByVal target As Visual) As Double
            Dim st = TryCast(target.RenderTransform, ScaleTransform)
            If st IsNot Nothing Then Return st.ScaleX
            Return 1
        End Function

        Public Function get_scale_y(ByVal target As Visual) As Double
            Dim st = TryCast(target.RenderTransform, ScaleTransform)
            If st IsNot Nothing Then Return st.ScaleY
            Return 1
        End Function

        Public Sub set_scale_x(ByVal target As Visual, ByVal value As Double)
            Dim st = TryCast(target.RenderTransform, ScaleTransform)
            If st IsNot Nothing Then st.ScaleX = value
        End Sub

        Public Sub set_scale_y(ByVal target As Visual, ByVal value As Double)
            Dim st = TryCast(target.RenderTransform, ScaleTransform)
            If st IsNot Nothing Then st.ScaleY = value
        End Sub

        Public Function get_rotate_angle(ByVal target As Visual) As Double
            Dim rt = TryCast(target.RenderTransform, RotateTransform)
            If rt IsNot Nothing Then Return rt.Angle
            Return 0
        End Function

        Public Sub set_rotate_angle(ByVal target As Visual, ByVal value As Double)
            Dim rt = TryCast(target.RenderTransform, RotateTransform)
            If rt IsNot Nothing Then rt.Angle = value
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
        ''' Slide a control horizontally (delta-based).
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
        ''' Slide a control vertically (delta-based).
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

        ' --- Scale (ScaleTransform on inner element) ---

        ''' <summary>
        ''' Scale a control's ScaleTransform to a target value (absolute).
        ''' </summary>
        Public Sub scale_to(ByVal target As Control, ByVal to_scale As Double,
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

        ''' <summary>
        ''' Scale a control's ScaleTransform by delta (relative).
        ''' </summary>
        Public Sub scale_delta(ByVal target As Control, ByVal delta As Double,
                               Optional ByVal duration_ms As Integer = 200,
                               Optional ByVal delay_ms As Integer = 0,
                               Optional ByVal easing As Easing = Nothing)
            If easing Is Nothing Then easing = ease_out_fluent
            ensure_scale_transform(target)
            Dim current = get_scale_x(target)
            scale_to(target, current + delta, duration_ms, delay_ms, easing)
        End Sub

        ' --- Rotate ---

        ''' <summary>
        ''' Rotate a control by delta angle.
        ''' </summary>
        Public Sub rotate(ByVal target As Control, ByVal delta_angle As Double,
                          Optional ByVal duration_ms As Integer = 200,
                          Optional ByVal delay_ms As Integer = 0,
                          Optional ByVal easing As Easing = Nothing)
            If easing Is Nothing Then easing = ease_out_fluent
            ensure_rotate_transform(target)
            Dim current = get_rotate_angle(target)
            Dim anim = New Animation()
            anim.Duration = TimeSpan.FromMilliseconds(duration_ms)
            anim.Delay = TimeSpan.FromMilliseconds(delay_ms)
            anim.FillMode = FillMode.Forward
            anim.Easing = easing
            Dim kf0 As New KeyFrame() With {.Cue = New Cue(0)}
            add_setter(kf0, RotateTransform.AngleProperty, current)
            anim.Children.Add(kf0)
            Dim kf1 As New KeyFrame() With {.Cue = New Cue(1)}
            add_setter(kf1, RotateTransform.AngleProperty, current + delta_angle)
            anim.Children.Add(kf1)
            Dim token = anim.RunAsync(target)
        End Sub

        ''' <summary>
        ''' Rotate a control to an absolute angle.
        ''' </summary>
        Public Sub rotate_to(ByVal target As Control, ByVal to_angle As Double,
                             Optional ByVal duration_ms As Integer = 200,
                             Optional ByVal delay_ms As Integer = 0,
                             Optional ByVal easing As Easing = Nothing)
            If easing Is Nothing Then easing = ease_out_fluent
            ensure_rotate_transform(target)
            Dim anim = New Animation()
            anim.Duration = TimeSpan.FromMilliseconds(duration_ms)
            anim.Delay = TimeSpan.FromMilliseconds(delay_ms)
            anim.FillMode = FillMode.Forward
            anim.Easing = easing
            Dim kf0 As New KeyFrame() With {.Cue = New Cue(0)}
            add_setter(kf0, RotateTransform.AngleProperty, get_rotate_angle(target))
            anim.Children.Add(kf0)
            Dim kf1 As New KeyFrame() With {.Cue = New Cue(1)}
            add_setter(kf1, RotateTransform.AngleProperty, to_angle)
            anim.Children.Add(kf1)
            Dim token = anim.RunAsync(target)
        End Sub

        ' --- Color Animation ---

        ''' <summary>
        ''' Animate a SolidColorBrush's Color property.
        ''' The brush must be an instance brush (not shared from resources).
        ''' </summary>
        Public Sub color(ByVal brush As SolidColorBrush, ByVal to_color As Color,
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
            add_setter(kf0, SolidColorBrush.ColorProperty, brush.Color)
            anim.Children.Add(kf0)
            Dim kf1 As New KeyFrame() With {.Cue = New Cue(1)}
            add_setter(kf1, SolidColorBrush.ColorProperty, to_color)
            anim.Children.Add(kf1)
            Dim token = anim.RunAsync(brush)
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

        ' --- Staggered Animation Helpers ---

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

        ' --- Delayed Code Execution ---

        ''' <summary>
        ''' Execute a callback after a delay (replaces PCL-CE AaCode).
        ''' </summary>
        Public Sub delayed_code(ByVal callback As Action, ByVal delay_ms As Integer)
            Dim timer As New DispatcherTimer()
            timer.Interval = TimeSpan.FromMilliseconds(delay_ms)
            AddHandler timer.Tick, Sub(sender, e)
                                       timer.Stop()
                                       callback.Invoke()
                                   End Sub
            timer.Start()
        End Sub

    End Module

End Namespace
