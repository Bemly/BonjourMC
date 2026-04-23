Option Explicit On
Option Strict On

Imports System
Imports Avalonia
Imports Avalonia.Animation
Imports Avalonia.Animation.Easings
Imports Avalonia.Controls
Imports Avalonia.Media
Imports Avalonia.Styling
Imports GUI.Animations
Imports GUI.ViewModels

Namespace Views
    Partial Public Class MainWindow
        Inherits Window

        Private Sub add_setter(ByVal kf As KeyFrame, ByVal [property] As AvaloniaProperty, ByVal value As Object)
            kf.Setters.Add(New Setter([property], value))
        End Sub

        Public Sub New()
            InitializeComponent()

            ' Start with opacity 0 for open animation
            Opacity = 0

            ' Apply initial transform for open animation
            Dim transform_group As New TransformGroup()
            transform_group.Children.Add(New RotateTransform(-3))
            transform_group.Children.Add(New TranslateTransform(0, 50))
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
        ''' Window open animation: fade in + slide up + de-rotate with bounce (PCL-CE style)
        ''' </summary>
        Private Sub animate_window_open()
            ' Fade in
            Dim fade As New Animation()
            fade.Duration = TimeSpan.FromMilliseconds(250)
            fade.Delay = TimeSpan.FromMilliseconds(80)
            fade.FillMode = FillMode.Forward
            fade.Easing = AnimationHelper.ease_out_fluent
            Dim fade_kf0 As New KeyFrame() With {.Cue = New Cue(0)}
            add_setter(fade_kf0, OpacityProperty, 0.0)
            fade.Children.Add(fade_kf0)
            Dim fade_kf1 As New KeyFrame() With {.Cue = New Cue(1)}
            add_setter(fade_kf1, OpacityProperty, 1.0)
            fade.Children.Add(fade_kf1)
            Dim token1 = fade.RunAsync(Me)

            ' Slide up + de-rotate
            Dim slide As New Animation()
            slide.Duration = TimeSpan.FromMilliseconds(600)
            slide.Delay = TimeSpan.FromMilliseconds(80)
            slide.FillMode = FillMode.Forward
            slide.Easing = AnimationHelper.ease_out_back
            Dim slide_kf0 As New KeyFrame() With {.Cue = New Cue(0)}
            add_setter(slide_kf0, TranslateTransform.YProperty, 50.0)
            add_setter(slide_kf0, RotateTransform.AngleProperty, -3.0)
            slide.Children.Add(slide_kf0)
            Dim slide_kf1 As New KeyFrame() With {.Cue = New Cue(1)}
            add_setter(slide_kf1, TranslateTransform.YProperty, 0.0)
            add_setter(slide_kf1, RotateTransform.AngleProperty, 0.0)
            slide.Children.Add(slide_kf1)
            Dim token2 = slide.RunAsync(Me)
        End Sub

        ''' <summary>
        ''' Window close animation: fade out + scale down + slide down (PCL-CE style)
        ''' </summary>
        Public Sub animate_window_close()
            ' Fade out
            Dim fade As New Animation()
            fade.Duration = TimeSpan.FromMilliseconds(140)
            fade.FillMode = FillMode.Forward
            fade.Easing = AnimationHelper.ease_out_fluent
            Dim fade_kf0 As New KeyFrame() With {.Cue = New Cue(0)}
            add_setter(fade_kf0, OpacityProperty, Opacity)
            fade.Children.Add(fade_kf0)
            Dim fade_kf1 As New KeyFrame() With {.Cue = New Cue(1)}
            add_setter(fade_kf1, OpacityProperty, 0.0)
            fade.Children.Add(fade_kf1)
            Dim token1 = fade.RunAsync(Me)

            ' Scale down + slide down
            Dim scale_anim As New Animation()
            scale_anim.Duration = TimeSpan.FromMilliseconds(180)
            scale_anim.FillMode = FillMode.Forward
            scale_anim.Easing = AnimationHelper.ease_out_fluent
            Dim scale_kf0 As New KeyFrame() With {.Cue = New Cue(0)}
            add_setter(scale_kf0, ScaleTransform.ScaleXProperty, 1.0)
            add_setter(scale_kf0, ScaleTransform.ScaleYProperty, 1.0)
            add_setter(scale_kf0, TranslateTransform.YProperty, 0.0)
            scale_anim.Children.Add(scale_kf0)
            Dim scale_kf1 As New KeyFrame() With {.Cue = New Cue(1)}
            add_setter(scale_kf1, ScaleTransform.ScaleXProperty, 0.92)
            add_setter(scale_kf1, ScaleTransform.ScaleYProperty, 0.92)
            add_setter(scale_kf1, TranslateTransform.YProperty, 15.0)
            scale_anim.Children.Add(scale_kf1)
            Dim token2 = scale_anim.RunAsync(Me)

            ' Close after animation
            Dim timer As New Avalonia.Threading.DispatcherTimer()
            timer.Interval = TimeSpan.FromMilliseconds(200)
            AddHandler timer.Tick, Sub(sender, e)
                                       timer.Stop()
                                       Close()
                                   End Sub
            timer.Start()
        End Sub
    End Class
End Namespace
