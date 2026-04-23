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
    ''' Content host that animates page enter transitions with PCL-CE exact timing.
    ''' </summary>
    Public Class PageTransition
        Inherits ContentControl

        Private _last_content As Object = Nothing

        Public Sub New()
            ClipToBounds = True
        End Sub

        Protected Overrides Sub OnPropertyChanged(ByVal change As AvaloniaPropertyChangedEventArgs)
            MyBase.OnPropertyChanged(change)

            If change.Property IsNot ContentProperty Then Return

            Dim new_content = change.NewValue
            If new_content Is Nothing Then Return
            If new_content Is _last_content Then Return
            _last_content = new_content

            ' Content is ViewModel — ViewLocator creates the View, which is Presenter.Child
            ' Wait for layout to complete, then get the actual rendered control
            Dispatcher.UIThread.Post(Sub()
                                         Dim actual_control As Control = Nothing
                                         If Me.Presenter IsNot Nothing Then
                                             actual_control = TryCast(Me.Presenter.Child, Control)
                                         End If
                                         If actual_control Is Nothing Then
                                             Debug.WriteLine("[PageTransition] Presenter.Child is Nothing")
                                             Return
                                         End If

                                         Debug.WriteLine($"[PageTransition] Animating {actual_control.GetType().Name}")
                                         actual_control.Opacity = 0
                                         Dispatcher.UIThread.Post(Sub()
                                                                      animate_page_enter(actual_control)
                                                                  End Sub, DispatcherPriority.Render)
                                     End Sub, DispatcherPriority.Render)
        End Sub

        ''' <summary>
        ''' Simple fade using DispatcherTimer (no RunAsync).
        ''' </summary>
        Private Sub animate_fade(ByVal ctrl As Control, ByVal target_opacity As Double, ByVal duration_ms As Integer)
            Dim start_opacity = ctrl.Opacity
            Debug.WriteLine($"[PageTransition] animate_fade: {ctrl.GetType().Name} from {start_opacity} to {target_opacity}, {duration_ms}ms")
            Dim sw As New Stopwatch()
            sw.Start()

            Dim timer As New DispatcherTimer()
            timer.Interval = TimeSpan.FromMilliseconds(16)
            AddHandler timer.Tick, Sub(s, e)
                                       Dim elapsed = sw.ElapsedMilliseconds
                                       Dim progress = Math.Min(1.0, elapsed / CDbl(duration_ms))
                                       progress = 1 - Math.Pow(1 - progress, 3)
                                       ctrl.Opacity = start_opacity + (target_opacity - start_opacity) * progress
                                       If elapsed >= duration_ms Then
                                           timer.Stop()
                                           ctrl.Opacity = target_opacity
                                           sw.Stop()
                                           Debug.WriteLine($"[PageTransition] animate_fade complete: {ctrl.GetType().Name} Opacity={ctrl.Opacity}")
                                       End If
                                   End Sub
            timer.Start()
        End Sub

        ''' <summary>
        ''' Animate page enter: per-element staggered fade + slide (PCL-CE exact timing).
        ''' </summary>
        Private Sub animate_page_enter(ByVal page As Control)
            If page Is Nothing Then Return

            Dim elements As New List(Of Control)()
            collect_animatable_children(page, elements, 0)
            Debug.WriteLine($"[PageTransition] animate_page_enter: {page.GetType().Name}, found {elements.Count} animatable elements")

            If elements.Count > 0 Then
                page.Opacity = 1

                Dim delay = 0
                For Each elem As Control In elements
                    elem.Opacity = 0
                    ensure_translate_transform(elem)
                    set_translate_y(elem, -16)

                    Dim captured_elem = elem
                    Dim captured_delay = delay

                    Dim start_timer As New DispatcherTimer()
                    start_timer.Interval = TimeSpan.FromMilliseconds(captured_delay)
                    AddHandler start_timer.Tick, Sub(s, e)
                                                     start_timer.Stop()
                                                     animate_fade(captured_elem, 1.0, 100)

                                                     Dim slide_sw As New Stopwatch()
                                                     slide_sw.Start()
                                                     Dim slide_timer As New DispatcherTimer()
                                                     slide_timer.Interval = TimeSpan.FromMilliseconds(16)
                                                     AddHandler slide_timer.Tick, Sub(s2, e2)
                                                                                      Dim elapsed = slide_sw.ElapsedMilliseconds
                                                                                      Dim progress = Math.Min(1.0, elapsed / 600.0)
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
                animate_fade(page, 1.0, 300)
            End If
        End Sub

        Private Sub collect_animatable_children(ByVal parent As Control, ByVal result As List(Of Control), ByVal depth As Integer)
            If depth > 3 Then Return

            ' Handle ContentControl/UserControl — recurse into Content
            Dim cc = TryCast(parent, ContentControl)
            If cc IsNot Nothing Then
                Dim content = TryCast(cc.Content, Control)
                If content IsNot Nothing Then
                    collect_animatable_children(content, result, depth + 1)
                End If
                Return
            End If

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

        Private Function is_card_element(ByVal ctrl As Control) As Boolean
            If TypeOf ctrl Is Card Then Return True
            If TypeOf ctrl Is MyListItem Then Return True
            If TypeOf ctrl Is MyButton Then Return True
            If TypeOf ctrl Is Button Then Return True
            If TypeOf ctrl Is Border Then
                Dim border = CType(ctrl, Border)
                If border.CornerRadius.TopLeft > 0 AndAlso border.Background IsNot Nothing Then Return True
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
