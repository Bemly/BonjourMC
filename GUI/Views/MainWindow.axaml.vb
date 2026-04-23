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

            ' Make title bar draggable (WindowDecorations="None" removes native drag)
            Dim title_bar = Me.FindControl(Of Border)("PART_TitleBar")
            Debug.WriteLine($"[MainWindow] title_bar found: {title_bar IsNot Nothing}")
            If title_bar IsNot Nothing Then
                AddHandler title_bar.PointerPressed, Sub(sender, e)
                                                         ' Walk up visual tree to find if click is on a button/control with Command
                                                         Dim is_button As Boolean = False
                                                         Dim current = TryCast(e.Source, Visual)
                                                         While current IsNot Nothing AndAlso current IsNot title_bar
                                                             Dim cmd_prop = current.GetType().GetProperty("Command")
                                                             If cmd_prop IsNot Nothing Then
                                                                 is_button = True
                                                                 Debug.WriteLine($"[MainWindow] title_bar click on {current.GetType().Name}, skip drag")
                                                                 Exit While
                                                             End If
                                                             current = TryCast(current.Parent, Visual)
                                                         End While
                                                         If Not is_button Then
                                                             Debug.WriteLine("[MainWindow] title_bar PointerPressed -> BeginMoveDrag")
                                                             BeginMoveDrag(e)
                                                         End If
                                                     End Sub
            End If

            ' Make resize grip work (manual resize since WindowDecorations="None" may not support native drag)
            Dim resize_grip = Me.FindControl(Of Border)("PART_ResizeGrip")
            Debug.WriteLine($"[MainWindow] resize_grip found: {resize_grip IsNot Nothing}")
            If resize_grip IsNot Nothing Then
                Dim is_resizing As Boolean = False
                Dim resize_start_pos As Point
                Dim resize_start_size As Size

                AddHandler resize_grip.PointerPressed, Sub(sender, e)
                                                           Dim props = e.GetCurrentPoint(resize_grip).Properties
                                                           If props.IsLeftButtonPressed Then
                                                               is_resizing = True
                                                               resize_start_pos = e.GetPosition(Nothing)
                                                               resize_start_size = New Size(Width, Height)
                                                               Debug.WriteLine($"[MainWindow] resize START: pos={resize_start_pos}, size={resize_start_size}")
                                                               e.Handled = True
                                                           End If
                                                       End Sub

                AddHandler PointerMoved, Sub(sender, e)
                                             If is_resizing Then
                                                 Dim current_pos = e.GetPosition(Nothing)
                                                 Dim delta_x = current_pos.X - resize_start_pos.X
                                                 Dim delta_y = current_pos.Y - resize_start_pos.Y
                                                 Dim new_width = Math.Max(MinWidth, resize_start_size.Width + delta_x)
                                                 Dim new_height = Math.Max(MinHeight, resize_start_size.Height + delta_y)
                                                 Width = new_width
                                                 Height = new_height
                                                 Debug.WriteLine($"[MainWindow] resize: {new_width}x{new_height}")
                                             End If
                                         End Sub

                AddHandler PointerReleased, Sub(sender, e)
                                                If is_resizing Then
                                                    is_resizing = False
                                                    Debug.WriteLine($"[MainWindow] resize END: {Width}x{Height}")
                                                End If
                                            End Sub

                AddHandler resize_grip.PointerEntered, Sub(sender, e)
                                                           Debug.WriteLine("[MainWindow] resize_grip PointerEntered")
                                                       End Sub
            End If

            ' Start with opacity 0 for open animation
            Opacity = 0

            ' Apply initial transform for open animation (PCL-CE: -4°, translateY 60)
            Dim transform_group As New TransformGroup()
            transform_group.Children.Add(New ScaleTransform(1, 1))
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
