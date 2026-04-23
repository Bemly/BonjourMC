Option Explicit On
Option Strict On

Imports System
Imports Avalonia
Imports Avalonia.Animation
Imports Avalonia.Animation.Easings
Imports Avalonia.Controls
Imports Avalonia.Media
Imports Avalonia.Styling
Imports Avalonia.Threading
Imports GUI.Animations
Imports GUI.ViewModels

Namespace Views
    Partial Public Class MainWindow
        Inherits Window

        Public Sub New()
            InitializeComponent()

            ' Start with opacity 0 for open animation
            Opacity = 0

            ' Apply initial transform for open animation (PCL-CE: -4°, translateY 60)
            Dim transform_group As New TransformGroup()
            transform_group.Children.Add(New RotateTransform(-4))
            transform_group.Children.Add(New TranslateTransform(0, 60))
            RenderTransform = transform_group
            RenderTransformOrigin = New RelativePoint(0.5, 0.5, RelativeUnit.Relative)
        End Sub

        Protected Overrides Sub OnOpened(ByVal e As EventArgs)
            MyBase.OnOpened(e)

            ' Wire up close animation to ViewModel
            Dim vm = TryCast(DataContext, MainWindowViewModel)
            If vm IsNot Nothing Then
                vm.on_close_action = Sub() animate_window_close()
            End If

            animate_window_open()
        End Sub

        ''' <summary>
        ''' Window open animation matching PCL-CE exactly:
        ''' Opacity 0→1 (250ms, 100ms delay, OutFluent)
        ''' TranslateY 60→0 (600ms, 100ms delay, OutBack Weak)
        ''' Rotate -4°→0° (500ms, 100ms delay, OutBack Weak)
        ''' </summary>
        Private Sub animate_window_open()
            ' Fade in
            Dim fade As New Animation()
            fade.Duration = TimeSpan.FromMilliseconds(250)
            fade.Delay = TimeSpan.FromMilliseconds(100)
            fade.FillMode = FillMode.Forward
            fade.Easing = AnimationHelper.ease_out_fluent
            Dim fade_kf0 As New KeyFrame() With {.Cue = New Cue(0)}
            fade_kf0.Setters.Add(New Setter(OpacityProperty, 0.0))
            fade.Children.Add(fade_kf0)
            Dim fade_kf1 As New KeyFrame() With {.Cue = New Cue(1)}
            fade_kf1.Setters.Add(New Setter(OpacityProperty, 1.0))
            fade.Children.Add(fade_kf1)
            Dim token1 = fade.RunAsync(Me)

            ' Slide up + de-rotate
            Dim slide As New Animation()
            slide.Duration = TimeSpan.FromMilliseconds(600)
            slide.Delay = TimeSpan.FromMilliseconds(100)
            slide.FillMode = FillMode.Forward
            slide.Easing = AnimationHelper.ease_out_back_weak
            Dim slide_kf0 As New KeyFrame() With {.Cue = New Cue(0)}
            slide_kf0.Setters.Add(New Setter(TranslateTransform.YProperty, 60.0))
            slide_kf0.Setters.Add(New Setter(RotateTransform.AngleProperty, -4.0))
            slide.Children.Add(slide_kf0)
            Dim slide_kf1 As New KeyFrame() With {.Cue = New Cue(1)}
            slide_kf1.Setters.Add(New Setter(TranslateTransform.YProperty, 0.0))
            slide_kf1.Setters.Add(New Setter(RotateTransform.AngleProperty, 0.0))
            slide.Children.Add(slide_kf1)
            Dim token2 = slide.RunAsync(Me)
        End Sub

        ''' <summary>
        ''' Window close animation matching PCL-CE exactly:
        ''' Opacity →0 (140ms, 40ms delay, OutFluent)
        ''' Scale →0.88 (180ms)
        ''' TranslateY →20 (180ms, OutFluent)
        ''' Rotate →0.6° (180ms, InOutFluent)
        ''' Close after 200ms timer
        ''' </summary>
        Public Sub animate_window_close()
            ' Fade out
            Dim fade As New Animation()
            fade.Duration = TimeSpan.FromMilliseconds(140)
            fade.Delay = TimeSpan.FromMilliseconds(40)
            fade.FillMode = FillMode.Forward
            fade.Easing = AnimationHelper.ease_out_fluent
            Dim fade_kf0 As New KeyFrame() With {.Cue = New Cue(0)}
            fade_kf0.Setters.Add(New Setter(OpacityProperty, Opacity))
            fade.Children.Add(fade_kf0)
            Dim fade_kf1 As New KeyFrame() With {.Cue = New Cue(1)}
            fade_kf1.Setters.Add(New Setter(OpacityProperty, 0.0))
            fade.Children.Add(fade_kf1)
            Dim token1 = fade.RunAsync(Me)

            ' Scale down + slide down + slight rotate
            Dim scale_anim As New Animation()
            scale_anim.Duration = TimeSpan.FromMilliseconds(180)
            scale_anim.FillMode = FillMode.Forward
            scale_anim.Easing = AnimationHelper.ease_out_fluent
            Dim scale_kf0 As New KeyFrame() With {.Cue = New Cue(0)}
            scale_kf0.Setters.Add(New Setter(ScaleTransform.ScaleXProperty, 1.0))
            scale_kf0.Setters.Add(New Setter(ScaleTransform.ScaleYProperty, 1.0))
            scale_kf0.Setters.Add(New Setter(TranslateTransform.YProperty, 0.0))
            scale_kf0.Setters.Add(New Setter(RotateTransform.AngleProperty, 0.0))
            scale_anim.Children.Add(scale_kf0)
            Dim scale_kf1 As New KeyFrame() With {.Cue = New Cue(1)}
            scale_kf1.Setters.Add(New Setter(ScaleTransform.ScaleXProperty, 0.88))
            scale_kf1.Setters.Add(New Setter(ScaleTransform.ScaleYProperty, 0.88))
            scale_kf1.Setters.Add(New Setter(TranslateTransform.YProperty, 20.0))
            scale_kf1.Setters.Add(New Setter(RotateTransform.AngleProperty, 0.6))
            scale_anim.Children.Add(scale_kf1)
            Dim token2 = scale_anim.RunAsync(Me)

            ' Close after animation
            Dim timer As New DispatcherTimer()
            timer.Interval = TimeSpan.FromMilliseconds(200)
            AddHandler timer.Tick, Sub(sender, e)
                                       timer.Stop()
                                       Close()
                                   End Sub
            timer.Start()
        End Sub
    End Class
End Namespace
