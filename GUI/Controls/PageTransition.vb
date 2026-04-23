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
        ''' Simple fade using Dispatcher.UIThread.Post (render-phase synced).
        ''' </summary>
        Private Sub animate_fade(ByVal ctrl As Control, ByVal target_opacity As Double, ByVal duration_ms As Integer)
            Dim start_opacity = ctrl.Opacity
            Debug.WriteLine($"[PageTransition] animate_fade: {ctrl.GetType().Name} from {start_opacity} to {target_opacity}, {duration_ms}ms")
            Dim sw As New Stopwatch()
            sw.Start()

            Dim tick As Action = Nothing
            tick = Sub()
                       Dim elapsed = sw.ElapsedMilliseconds
                       Dim progress = Math.Min(1.0, elapsed / CDbl(duration_ms))
                       progress = 1 - (1 - progress) * (1 - progress) * (1 - progress)
                       ctrl.Opacity = start_opacity + (target_opacity - start_opacity) * progress
                       If elapsed >= duration_ms Then
                           ctrl.Opacity = target_opacity
                           sw.Stop()
                           Debug.WriteLine($"[PageTransition] animate_fade complete: {ctrl.GetType().Name}")
                       Else
                           Dispatcher.UIThread.Post(tick, DispatcherPriority.Render)
                       End If
                   End Sub

            Dispatcher.UIThread.Post(tick, DispatcherPriority.Render)
        End Sub

        ''' <summary>
        ''' Animate page enter: whole-page fade + subtle slide.
        ''' </summary>
        Private Sub animate_page_enter(ByVal page As Control)
            If page Is Nothing Then Return

            Debug.WriteLine($"[PageTransition] animate_page_enter: {page.GetType().Name}")

            Dim duration_ms = 250
            ensure_translate_transform(page)
            set_translate_y(page, -8)

            Dim sw As New Stopwatch()
            sw.Start()

            Dim tick As Action = Nothing
            tick = Sub()
                       Dim elapsed = sw.ElapsedMilliseconds
                       Dim progress = Math.Min(1.0, elapsed / CDbl(duration_ms))
                       Dim eased = 1 - (1 - progress) * (1 - progress) * (1 - progress)

                       page.Opacity = eased
                       set_translate_y(page, -8.0 * (1 - eased))

                       If elapsed >= duration_ms Then
                           page.Opacity = 1
                           set_translate_y(page, 0)
                           sw.Stop()
                           Debug.WriteLine($"[PageTransition] animate_page_enter complete: {page.GetType().Name}")
                       Else
                           Dispatcher.UIThread.Post(tick, DispatcherPriority.Render)
                       End If
                   End Sub

            Dispatcher.UIThread.Post(tick, DispatcherPriority.Render)
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
            If TypeOf ctrl Is PclListItem Then Return True
            If TypeOf ctrl Is PclButton Then Return True
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
