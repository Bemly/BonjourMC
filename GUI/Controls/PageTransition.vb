Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports Avalonia
Imports Avalonia.Animation
Imports Avalonia.Animation.Easings
Imports Avalonia.Controls
Imports Avalonia.Media
Imports Avalonia.Styling
Imports Avalonia.Threading
Imports GUI.Animations

Namespace Controls

    ''' <summary>
    ''' Content host that animates page transitions with PCL-CE exact timing.
    ''' Left panel: staggered translateX from -25, 300ms OutBack, 7-15ms stagger.
    ''' Right panel: staggered translateY from -16, 350ms OutBack, 25ms stagger.
    ''' </summary>
    Public Class PageTransition
        Inherits ContentControl

        Private _is_animating As Boolean = False
        Private _last_content As Object = Nothing

        Public Sub New()
            ClipToBounds = True
        End Sub

        Private Sub add_setter(ByVal kf As KeyFrame, ByVal [property] As AvaloniaProperty, ByVal value As Object)
            kf.Setters.Add(New Setter([property], value))
        End Sub

        Protected Overrides Sub OnPropertyChanged(ByVal change As AvaloniaPropertyChangedEventArgs)
            MyBase.OnPropertyChanged(change)

            If change.Property IsNot ContentProperty Then Return

            Dim new_content = change.NewValue
            If new_content Is Nothing Then Return
            If _is_animating Then Return
            If new_content Is _last_content Then Return
            _last_content = new_content

            Dim old_control = TryCast(change.OldValue, Control)
            Dim new_control = TryCast(new_content, Control)

            If old_control IsNot Nothing AndAlso new_control IsNot Nothing Then
                _is_animating = True

                ' Animate old page out
                animate_page_exit(old_control)

                ' Delay 110ms (PCL-CE pattern), then swap and animate in
                Dim timer As New DispatcherTimer()
                timer.Interval = TimeSpan.FromMilliseconds(110)
                AddHandler timer.Tick, Sub(sender, e)
                                           timer.Stop()
                                           ' Show new page at opacity 0, then animate in
                                           new_control.Opacity = 0
                                           Dispatcher.UIThread.Post(Sub()
                                                                        new_control.Opacity = 1
                                                                        animate_page_enter(new_control)
                                                                        _is_animating = False
                                                                    End Sub, DispatcherPriority.Render)
                                       End Sub
                timer.Start()
            ElseIf new_control IsNot Nothing Then
                ' First load — just animate in
                animate_page_enter(new_control)
            End If
        End Sub

        ''' <summary>
        ''' Animate page exit: per-element fade out + slide up (PCL-CE: 70ms per element, 15ms stagger).
        ''' </summary>
        Private Sub animate_page_exit(ByVal page As Control)
            Dim elements As New List(Of Control)()
            collect_animatable_children(page, elements, 0)

            If elements.Count > 0 Then
                Dim delay = 0
                For Each elem As Control In elements
                    Dim anim As New Animation()
                    anim.Duration = TimeSpan.FromMilliseconds(70)
                    anim.Delay = TimeSpan.FromMilliseconds(delay)
                    anim.FillMode = FillMode.Forward
                    anim.Easing = AnimationHelper.ease_in_fluent

                    ensure_translate_transform(elem)

                    Dim kf0 As New KeyFrame() With {.Cue = New Cue(0)}
                    add_setter(kf0, OpacityProperty, elem.Opacity)
                    add_setter(kf0, TranslateTransform.YProperty, get_translate_y(elem))
                    anim.Children.Add(kf0)

                    Dim kf1 As New KeyFrame() With {.Cue = New Cue(1)}
                    add_setter(kf1, OpacityProperty, 0.0)
                    add_setter(kf1, TranslateTransform.YProperty, get_translate_y(elem) - 6)
                    anim.Children.Add(kf1)

                    Dim token = anim.RunAsync(elem)
                    delay += 15
                Next
            Else
                ' Simple fade out
                AnimationHelper.fade(page, 0.0, 70, 0, AnimationHelper.ease_in_fluent)
            End If
        End Sub

        ''' <summary>
        ''' Animate page enter: per-element staggered fade + slide (PCL-CE exact timing).
        ''' Right panel: opacity 100ms OutFluent + translateY 350ms OutBack, 25ms stagger.
        ''' </summary>
        Private Sub animate_page_enter(ByVal page As Control)
            page.Opacity = 0

            Dim elements As New List(Of Control)()
            collect_animatable_children(page, elements, 0)

            If elements.Count > 0 Then
                page.Opacity = 1

                Dim delay = 0
                For Each elem As Control In elements
                    ' Set initial state
                    elem.Opacity = 0
                    ensure_translate_transform(elem)
                    set_translate_y(elem, -16)

                    ' Fade in: 100ms OutFluent(Weak)
                    Dim fade_anim As New Animation()
                    fade_anim.Duration = TimeSpan.FromMilliseconds(100)
                    fade_anim.Delay = TimeSpan.FromMilliseconds(delay)
                    fade_anim.FillMode = FillMode.Forward
                    fade_anim.Easing = AnimationHelper.ease_out_fluent_weak
                    Dim fade_kf0 As New KeyFrame() With {.Cue = New Cue(0)}
                    add_setter(fade_kf0, OpacityProperty, 0.0)
                    fade_anim.Children.Add(fade_kf0)
                    Dim fade_kf1 As New KeyFrame() With {.Cue = New Cue(1)}
                    add_setter(fade_kf1, OpacityProperty, 1.0)
                    fade_anim.Children.Add(fade_kf1)
                    Dim token1 = fade_anim.RunAsync(elem)

                    ' Slide down: 5px in 250ms OutFluent
                    Dim slide1 As New Animation()
                    slide1.Duration = TimeSpan.FromMilliseconds(250)
                    slide1.Delay = TimeSpan.FromMilliseconds(delay)
                    slide1.FillMode = FillMode.Forward
                    slide1.Easing = AnimationHelper.ease_out_fluent
                    Dim slide1_kf0 As New KeyFrame() With {.Cue = New Cue(0)}
                    add_setter(slide1_kf0, TranslateTransform.YProperty, -16.0)
                    slide1.Children.Add(slide1_kf0)
                    Dim slide1_kf1 As New KeyFrame() With {.Cue = New Cue(1)}
                    add_setter(slide1_kf1, TranslateTransform.YProperty, -11.0) ' -16 + 5 = -11
                    slide1.Children.Add(slide1_kf1)
                    Dim token2 = slide1.RunAsync(elem)

                    ' Continue to 0: 350ms OutBack
                    AnimationHelper.delayed_code(Sub()
                                                     AnimationHelper.translate_y(elem, 11, 350, 0, AnimationHelper.ease_out_back)
                                                 End Sub, delay + 250)

                    delay += 25
                Next
            Else
                ' No animatable elements — simple fade in
                AnimationHelper.fade(page, 1.0, 200, 0, AnimationHelper.ease_out_fluent)
            End If
        End Sub

        ''' <summary>
        ''' Recursively collect children that should animate.
        ''' </summary>
        Private Sub collect_animatable_children(ByVal parent As Control, ByVal result As List(Of Control), ByVal depth As Integer)
            If depth > 3 Then Return

            Dim panel = TryCast(parent, Panel)
            If panel IsNot Nothing Then
                For Each child In panel.Children
                    Dim child_control = TryCast(child, Control)
                    If child_control Is Nothing Then Continue For

                    If is_card_element(child_control) Then
                        result.Add(child_control)
                    Else
                        collect_animatable_children(child_control, result, depth + 1)
                    End If
                Next
            End If

            Dim scroll = TryCast(parent, ScrollViewer)
            If scroll IsNot Nothing Then
                Dim content = TryCast(scroll.Content, Control)
                If content IsNot Nothing Then
                    collect_animatable_children(content, result, depth + 1)
                End If
            End If

            Dim grid = TryCast(parent, Grid)
            If grid IsNot Nothing Then
                For Each child In grid.Children
                    Dim child_control = TryCast(child, Control)
                    If child_control Is Nothing Then Continue For

                    If is_card_element(child_control) Then
                        result.Add(child_control)
                    Else
                        collect_animatable_children(child_control, result, depth + 1)
                    End If
                Next
            End If

            Dim stack = TryCast(parent, StackPanel)
            If stack IsNot Nothing Then
                For Each child In stack.Children
                    Dim child_control = TryCast(child, Control)
                    If child_control Is Nothing Then Continue For

                    If is_card_element(child_control) Then
                        result.Add(child_control)
                    Else
                        collect_animatable_children(child_control, result, depth + 1)
                    End If
                Next
            End If
        End Sub

        ''' <summary>
        ''' Determine if a control should be treated as an animatable element.
        ''' </summary>
        Private Function is_card_element(ByVal ctrl As Control) As Boolean
            If TypeOf ctrl Is Card Then Return True
            If TypeOf ctrl Is MyListItem Then Return True
            If TypeOf ctrl Is MyButton Then Return True
            If TypeOf ctrl Is Border Then
                Dim border = CType(ctrl, Border)
                If border.Tag IsNot Nothing AndAlso border.Tag.ToString() = "card" Then Return True
            End If
            Return False
        End Function

        Private Sub ensure_translate_transform(ByVal target As Visual)
            If target.RenderTransform Is Nothing OrElse Not TypeOf target.RenderTransform Is TranslateTransform Then
                target.RenderTransform = New TranslateTransform(0, 0)
            End If
        End Sub

        Private Function get_translate_y(ByVal target As Visual) As Double
            Dim tt = TryCast(target.RenderTransform, TranslateTransform)
            If tt IsNot Nothing Then Return tt.Y
            Return 0
        End Function

        Private Sub set_translate_y(ByVal target As Visual, ByVal value As Double)
            Dim tt = TryCast(target.RenderTransform, TranslateTransform)
            If tt IsNot Nothing Then tt.Y = value
        End Sub

    End Class

End Namespace
