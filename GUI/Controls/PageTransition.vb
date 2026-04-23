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
    ''' Uses custom PageContent property to avoid ContentControl auto-replace.
    ''' </summary>
    Public Class PageTransition
        Inherits ContentControl

        ' Display layer — single Border that holds the current page
        Private ReadOnly _display As New Border() With {.Background = Brushes.Transparent}

        ' Animation state
        Private _is_animating As Boolean = False
        Private _last_content As Object = Nothing
        Private _pending_content As Object = Nothing

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
            ' Set fixed Content — the display layer, never changes
            Content = _display
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
            Dim new_control = TryCast(new_content, Control)
            If new_control Is Nothing Then Return

            Dim old_control = TryCast(_display.Child, Control)

            If old_control IsNot Nothing Then
                _is_animating = True

                ' Fade old page out (80ms)
                animate_fade(old_control, 0.0, 80)

                ' After fade out, swap content and fade new page in
                Dim swap_timer As New DispatcherTimer()
                swap_timer.Interval = TimeSpan.FromMilliseconds(90)
                AddHandler swap_timer.Tick, Sub(sender, e)
                                                swap_timer.Stop()
                                                ' Swap content
                                                _display.Child = new_control
                                                ' Animate new page in
                                                animate_page_enter(new_control)
                                                _is_animating = False

                                                ' Process pending
                                                process_pending()
                                            End Sub
                swap_timer.Start()
            Else
                ' First load — just set content and animate in
                _display.Child = new_control
                animate_page_enter(new_control)
            End If
        End Sub

        Private Sub process_pending()
            If _pending_content Is Nothing Then Return
            Dim pending = _pending_content
            _pending_content = Nothing
            _last_content = pending
            Dim new_control = TryCast(pending, Control)
            If new_control Is Nothing Then Return

            _is_animating = True
            ' Fade current out
            Dim old_control = TryCast(_display.Child, Control)
            If old_control IsNot Nothing Then
                animate_fade(old_control, 0.0, 80)
            End If

            Dim swap_timer As New DispatcherTimer()
            swap_timer.Interval = TimeSpan.FromMilliseconds(90)
            AddHandler swap_timer.Tick, Sub(sender, e)
                                            swap_timer.Stop()
                                            _display.Child = new_control
                                            animate_page_enter(new_control)
                                            _is_animating = False
                                            process_pending()
                                        End Sub
            swap_timer.Start()
        End Sub

        ''' <summary>
        ''' Simple fade using DispatcherTimer (no RunAsync).
        ''' </summary>
        Private Sub animate_fade(ByVal ctrl As Control, ByVal target_opacity As Double, ByVal duration_ms As Integer)
            Dim start_opacity = ctrl.Opacity
            Dim sw As New Stopwatch()
            sw.Start()

            Dim timer As New DispatcherTimer()
            timer.Interval = TimeSpan.FromMilliseconds(16)
            AddHandler timer.Tick, Sub(s, e)
                                       Dim elapsed = sw.ElapsedMilliseconds
                                       Dim progress = Math.Min(1.0, elapsed / CDbl(duration_ms))
                                       ' EaseOutFluent
                                       progress = 1 - Math.Pow(1 - progress, 3)
                                       ctrl.Opacity = start_opacity + (target_opacity - start_opacity) * progress
                                       If elapsed >= duration_ms Then
                                           timer.Stop()
                                           ctrl.Opacity = target_opacity
                                           sw.Stop()
                                       End If
                                   End Sub
            timer.Start()
        End Sub

        ''' <summary>
        ''' Animate page enter: per-element staggered fade + slide (PCL-CE exact timing).
        ''' </summary>
        Private Sub animate_page_enter(ByVal page As Control)
            If page Is Nothing Then Return
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

                    ' Animate this element with delay
                    Dim captured_elem = elem
                    Dim captured_delay = delay

                    Dim start_timer As New DispatcherTimer()
                    start_timer.Interval = TimeSpan.FromMilliseconds(captured_delay)
                    AddHandler start_timer.Tick, Sub(s, e)
                                                     start_timer.Stop()

                                                     ' Fade in: 100ms
                                                     animate_fade(captured_elem, 1.0, 100)

                                                     ' Slide: -16 → 0 in 600ms with OutBack easing
                                                     Dim slide_sw As New Stopwatch()
                                                     slide_sw.Start()
                                                     Dim slide_timer As New DispatcherTimer()
                                                     slide_timer.Interval = TimeSpan.FromMilliseconds(16)
                                                     AddHandler slide_timer.Tick, Sub(s2, e2)
                                                                                      Dim elapsed = slide_sw.ElapsedMilliseconds
                                                                                      Dim progress = Math.Min(1.0, elapsed / 600.0)
                                                                                      ' OutBack easing
                                                                                      Dim p = 1.5
                                                                                      progress = 1 - Math.Pow(1 - progress, p) * Math.Cos(1.5 * Math.PI * progress)
                                                                                      set_translate_y(captured_elem, -16.0 + 16.0 * progress)
                                                                                      If elapsed >= 600 Then
                                                                                          slide_timer.Stop()
                                                                                          set_translate_y(captured_elem, 0)
                                                                                          slide_sw.Stop()
                                                                                      End If
                                                                                  End Sub
                                                     slide_timer.Start()
                                                 End Sub
                    start_timer.Start()

                    delay += 25
                Next
            Else
                ' No animatable elements — simple fade in
                animate_fade(page, 1.0, 200)
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
