Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports Avalonia
Imports Avalonia.Animation
Imports Avalonia.Animation.Easings
Imports Avalonia.Controls
Imports Avalonia.Styling
Imports Avalonia.Threading
Imports GUI.Animations

Namespace Controls

    ''' <summary>
    ''' Content host that animates page transitions with PCL-CE style staggered entry/exit.
    ''' When Content changes, old page fades out and new page cards enter with staggered delay.
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

                ' Animate old page out (fast)
                animate_page_exit(old_control)

                ' Delay new page entry to let old page fade
                Dim timer As New DispatcherTimer()
                timer.Interval = TimeSpan.FromMilliseconds(80)
                AddHandler timer.Tick, Sub(sender, e)
                                           timer.Stop()
                                           animate_page_enter(new_control)
                                           _is_animating = False
                                       End Sub
                timer.Start()
            ElseIf new_control IsNot Nothing Then
                ' First load — just animate in
                animate_page_enter(new_control)
            End If
        End Sub

        ''' <summary>
        ''' Animate page exit: fade out + slide up slightly.
        ''' </summary>
        Private Sub animate_page_exit(ByVal page As Control)
            Dim anim As New Animation()
            anim.Duration = TimeSpan.FromMilliseconds(70)
            anim.FillMode = FillMode.Forward
            anim.Easing = AnimationHelper.ease_in_fluent

            Dim kf0 As New KeyFrame() With {.Cue = New Cue(0)}
            add_setter(kf0, OpacityProperty, page.Opacity)
            anim.Children.Add(kf0)

            Dim kf1 As New KeyFrame() With {.Cue = New Cue(1)}
            add_setter(kf1, OpacityProperty, 0.0)
            anim.Children.Add(kf1)

            Dim token = anim.RunAsync(page)
        End Sub

        ''' <summary>
        ''' Animate page enter: find card children, apply staggered fade+slide-up.
        ''' </summary>
        Private Sub animate_page_enter(ByVal page As Control)
            page.Opacity = 0

            ' Collect all direct card children for staggered animation
            Dim cards As New List(Of Control)()
            collect_animatable_children(page, cards, 0)

            If cards.Count > 0 Then
                ' Staggered card entry
                Dim delay = 0
                For Each card As Control In cards
                    card.Opacity = 0
                    Dim ct = TryCast(card.RenderTransform, Avalonia.Media.TranslateTransform)
                    If ct Is Nothing Then
                        card.RenderTransform = New Avalonia.Media.TranslateTransform(0, 16)
                    Else
                        ct.Y = 16
                    End If

                    ' Fade in
                    Dim fade_anim As New Animation()
                    fade_anim.Duration = TimeSpan.FromMilliseconds(100)
                    fade_anim.Delay = TimeSpan.FromMilliseconds(delay)
                    fade_anim.FillMode = FillMode.Forward
                    fade_anim.Easing = AnimationHelper.ease_out_fluent
                    Dim fade_kf0 As New KeyFrame() With {.Cue = New Cue(0)}
                    add_setter(fade_kf0, OpacityProperty, 0.0)
                    fade_anim.Children.Add(fade_kf0)
                    Dim fade_kf1 As New KeyFrame() With {.Cue = New Cue(1)}
                    add_setter(fade_kf1, OpacityProperty, 1.0)
                    fade_anim.Children.Add(fade_kf1)
                    Dim token1 = fade_anim.RunAsync(card)

                    ' Slide down with bounce
                    Dim slide_anim As New Animation()
                    slide_anim.Duration = TimeSpan.FromMilliseconds(350)
                    slide_anim.Delay = TimeSpan.FromMilliseconds(delay)
                    slide_anim.FillMode = FillMode.Forward
                    slide_anim.Easing = AnimationHelper.ease_out_fluent
                    Dim slide_kf0 As New KeyFrame() With {.Cue = New Cue(0)}
                    add_setter(slide_kf0, Avalonia.Media.TranslateTransform.YProperty, 16.0)
                    slide_anim.Children.Add(slide_kf0)
                    Dim slide_kf1 As New KeyFrame() With {.Cue = New Cue(1)}
                    add_setter(slide_kf1, Avalonia.Media.TranslateTransform.YProperty, 0.0)
                    slide_anim.Children.Add(slide_kf1)
                    Dim token2 = slide_anim.RunAsync(card)

                    delay += 25
                Next

                ' Fade in the page itself immediately
                page.Opacity = 1
            Else
                ' No cards — simple fade in
                AnimationHelper.fade(page, 1.0, 200, 0, AnimationHelper.ease_out_fluent)
            End If
        End Sub

        ''' <summary>
        ''' Recursively collect children that should animate (cards, panels with content).
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
        ''' Determine if a control should be treated as an animatable card.
        ''' </summary>
        Private Function is_card_element(ByVal ctrl As Control) As Boolean
            If TypeOf ctrl Is Card Then Return True
            If TypeOf ctrl Is Border Then
                Dim border = CType(ctrl, Border)
                If border.Tag IsNot Nothing AndAlso border.Tag.ToString() = "card" Then Return True
            End If
            Return False
        End Function

    End Class

End Namespace
