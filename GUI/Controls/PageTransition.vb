Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
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
    ''' Uses dual-layer ContentPresenter to keep old page visible during exit animation.
    ''' </summary>
    Public Class PageTransition
        Inherits ContentControl

        ' Dual-layer rendering
        Private ReadOnly _grid As New Grid()
        Private ReadOnly _old_layer As New Border()
        Private ReadOnly _new_layer As New Border()

        ' Animation state
        Private _is_animating As Boolean = False
        Private _last_content As Object = Nothing
        Private _pending_content As Object = Nothing
        Private _exit_timers As New List(Of DispatcherTimer)()
        Private _enter_timers As New List(Of DispatcherTimer)()

        ' Styled Property for page content (avoids ContentControl auto-replace)
        Public Shared ReadOnly PageContentProperty As StyledProperty(Of Object) =
            AvaloniaProperty.Register(Of PageTransition, Object)("PageContent", Nothing)

        Public Property PageContent As Object
            Get
                Return GetValue(PageContentProperty)
            End Get
            Set(ByVal value As Object)
                SetValue(PageContentProperty, value)
            End Set
        End Property

        Public Sub New()
            ClipToBounds = True

            ' Build dual-layer structure
            _old_layer.Opacity = 1
            _new_layer.Opacity = 0
            _grid.Children.Add(_old_layer)
            _grid.Children.Add(_new_layer)

            ' Set fixed Content — never changes
            Content = _grid
        End Sub

        Protected Overrides Sub OnPropertyChanged(ByVal change As AvaloniaPropertyChangedEventArgs)
            MyBase.OnPropertyChanged(change)

            If change.Property IsNot PageContentProperty Then Return

            Dim new_content = change.NewValue
            If new_content Is Nothing Then Return
            If new_content Is _last_content Then Return

            ' If animating, queue the new content
            If _is_animating Then
                _pending_content = new_content
                Return
            End If

            _last_content = new_content
            start_transition(new_content)
        End Sub

        Private Sub start_transition(ByVal new_content As Object)
            Dim new_control = TryCast(new_content, Control)
            If new_control Is Nothing Then Return

            Dim old_control = TryCast(_old_layer.Child, Control)

            If old_control IsNot Nothing Then
                _is_animating = True

                ' Put new page in new layer (invisible)
                _new_layer.Child = new_control
                _new_layer.Opacity = 0

                ' Animate old page out
                animate_page_exit(old_control)

                ' After exit animation, swap layers
                Dim swap_timer As New DispatcherTimer()
                swap_timer.Interval = TimeSpan.FromMilliseconds(110)
                AddHandler swap_timer.Tick, Sub(sender, e)
                                                swap_timer.Stop()
                                                perform_swap(new_control)
                                            End Sub
                swap_timer.Start()
            Else
                ' First load — just animate in
                _old_layer.Child = new_control
                _old_layer.Opacity = 1
                animate_page_enter(new_control)
            End If
        End Sub

        Private Sub perform_swap(ByVal new_control As Control)
            ' Move new content to old layer
            _old_layer.Child = new_control
            _old_layer.Opacity = 1
            _new_layer.Child = Nothing
            _new_layer.Opacity = 0

            ' Animate new page in
            animate_page_enter(new_control)

            ' Done animating
            _is_animating = False

            ' Process pending content if any
            If _pending_content IsNot Nothing Then
                Dim pending = _pending_content
                _pending_content = Nothing
                _last_content = pending
                start_transition(pending)
            End If
        End Sub

        Private Sub cancel_all_animations()
            For Each t In _exit_timers
                t.Stop()
            Next
            _exit_timers.Clear()
            For Each t In _enter_timers
                t.Stop()
            Next
            _enter_timers.Clear()
        End Sub

        ''' <summary>
        ''' Animate page exit: per-element fade out + slide up (PCL-CE: 70ms per element, 15ms stagger).
        ''' Uses Transitions for smooth, interruptible animations.
        ''' </summary>
        Private Sub animate_page_exit(ByVal page As Control)
            Dim elements As New List(Of Control)()
            collect_animatable_children(page, elements, 0)

            If elements.Count > 0 Then
                Dim delay = 0
                For Each elem As Control In elements
                    ' Add transition for opacity
                    Dim opacity_transition As New DoubleTransition()
                    opacity_transition.Property = Visual.OpacityProperty
                    opacity_transition.Duration = TimeSpan.FromMilliseconds(70)
                    opacity_transition.Delay = TimeSpan.FromMilliseconds(delay)
                    opacity_transition.Easing = AnimationHelper.ease_in_fluent

                    If elem.Transitions Is Nothing Then
                        elem.Transitions = New Transitions()
                    End If
                    elem.Transitions.Add(opacity_transition)

                    ' Set target value (transition animates automatically)
                    elem.Opacity = 0

                    delay += 15
                Next

                ' Clean up transitions after animation completes
                Dim cleanup_timer As New DispatcherTimer()
                cleanup_timer.Interval = TimeSpan.FromMilliseconds(delay + 100)
                AddHandler cleanup_timer.Tick, Sub(sender, e)
                                                   cleanup_timer.Stop()
                                                   For Each elem As Control In elements
                                                       If elem.Transitions IsNot Nothing Then
                                                           elem.Transitions.Clear()
                                                       End If
                                                   Next
                                               End Sub
                cleanup_timer.Start()
                _exit_timers.Add(cleanup_timer)
            Else
                ' Simple fade out using transition
                Dim fade_transition As New DoubleTransition()
                fade_transition.Property = Visual.OpacityProperty
                fade_transition.Duration = TimeSpan.FromMilliseconds(70)
                fade_transition.Easing = AnimationHelper.ease_in_fluent
                If page.Transitions Is Nothing Then
                    page.Transitions = New Transitions()
                End If
                page.Transitions.Add(fade_transition)
                page.Opacity = 0

                Dim cleanup_timer As New DispatcherTimer()
                cleanup_timer.Interval = TimeSpan.FromMilliseconds(100)
                AddHandler cleanup_timer.Tick, Sub(sender, e)
                                                   cleanup_timer.Stop()
                                                   If page.Transitions IsNot Nothing Then
                                                       page.Transitions.Clear()
                                                   End If
                                               End Sub
                cleanup_timer.Start()
                _exit_timers.Add(cleanup_timer)
            End If
        End Sub

        ''' <summary>
        ''' Animate page enter: per-element staggered fade + slide (PCL-CE exact timing).
        ''' Uses Transitions for smooth, interruptible animations.
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
                    Dim fade_transition As New DoubleTransition()
                    fade_transition.Property = Visual.OpacityProperty
                    fade_transition.Duration = TimeSpan.FromMilliseconds(100)
                    fade_transition.Delay = TimeSpan.FromMilliseconds(delay)
                    fade_transition.Easing = AnimationHelper.ease_out_fluent_weak

                    If elem.Transitions Is Nothing Then
                        elem.Transitions = New Transitions()
                    End If
                    elem.Transitions.Add(fade_transition)
                    elem.Opacity = 1

                    ' Slide down phase 1: -16 → -11, 250ms OutFluent
                    ' Use timer to set up slide transition after fade starts
                    Dim slide_timer As New DispatcherTimer()
                    slide_timer.Interval = TimeSpan.FromMilliseconds(delay)
                    Dim captured_elem = elem
                    AddHandler slide_timer.Tick, Sub(sender, e)
                                                     slide_timer.Stop()
                                                     ' Add translate transition
                                                     Dim translate_transition As New DoubleTransition()
                                                     translate_transition.Property = TranslateTransform.YProperty
                                                     translate_transition.Duration = TimeSpan.FromMilliseconds(250)
                                                     translate_transition.Easing = AnimationHelper.ease_out_fluent

                                                     captured_elem.Transitions.Add(translate_transition)
                                                     set_translate_y(captured_elem, -11)

                                                     ' Phase 2: -11 → 0, 350ms OutBack
                                                     Dim phase2_timer As New DispatcherTimer()
                                                     phase2_timer.Interval = TimeSpan.FromMilliseconds(250)
                                                     AddHandler phase2_timer.Tick, Sub(s2, e2)
                                                                                      phase2_timer.Stop()
                                                                                      Dim translate2 As New DoubleTransition()
                                                                                      translate2.Property = TranslateTransform.YProperty
                                                                                      translate2.Duration = TimeSpan.FromMilliseconds(350)
                                                                                      translate2.Easing = AnimationHelper.ease_out_back

                                                                                      captured_elem.Transitions.Add(translate2)
                                                                                      set_translate_y(captured_elem, 0)
                                                                                  End Sub
                                                     phase2_timer.Start()
                                                     _enter_timers.Add(phase2_timer)
                                                 End Sub
                    slide_timer.Start()
                    _enter_timers.Add(slide_timer)

                    delay += 25
                Next

                ' Clean up transitions after all animations complete
                Dim cleanup_timer As New DispatcherTimer()
                cleanup_timer.Interval = TimeSpan.FromMilliseconds(delay + 600)
                AddHandler cleanup_timer.Tick, Sub(sender, e)
                                                   cleanup_timer.Stop()
                                                   For Each elem As Control In elements
                                                       If elem.Transitions IsNot Nothing Then
                                                           elem.Transitions.Clear()
                                                       End If
                                                   Next
                                               End Sub
                cleanup_timer.Start()
                _enter_timers.Add(cleanup_timer)
            Else
                ' No animatable elements — simple fade in using transition
                Dim fade_transition As New DoubleTransition()
                fade_transition.Property = Visual.OpacityProperty
                fade_transition.Duration = TimeSpan.FromMilliseconds(200)
                fade_transition.Easing = AnimationHelper.ease_out_fluent
                If page.Transitions Is Nothing Then
                    page.Transitions = New Transitions()
                End If
                page.Transitions.Add(fade_transition)
                page.Opacity = 1

                Dim cleanup_timer As New DispatcherTimer()
                cleanup_timer.Interval = TimeSpan.FromMilliseconds(300)
                AddHandler cleanup_timer.Tick, Sub(sender, e)
                                                   cleanup_timer.Stop()
                                                   If page.Transitions IsNot Nothing Then
                                                       page.Transitions.Clear()
                                                   End If
                                               End Sub
                cleanup_timer.Start()
                _enter_timers.Add(cleanup_timer)
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
        ''' Extended to recognize Button and styled Border elements.
        ''' </summary>
        Private Function is_card_element(ByVal ctrl As Control) As Boolean
            If TypeOf ctrl Is Card Then Return True
            If TypeOf ctrl Is MyListItem Then Return True
            If TypeOf ctrl Is MyButton Then Return True
            If TypeOf ctrl Is Button Then Return True
            If TypeOf ctrl Is Border Then
                Dim border = CType(ctrl, Border)
                ' Card-like Border: has CornerRadius and Background
                If border.CornerRadius.TopLeft > 0 AndAlso border.Background IsNot Nothing Then Return True
                ' Tagged as card
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
