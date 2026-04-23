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
        ''' Animate page enter: left panel items slide from left, right panel fades down.
        ''' </summary>
        Private Sub animate_page_enter(ByVal page As Control)
            If page Is Nothing Then Return

            Debug.WriteLine($"[PageTransition] animate_page_enter: {page.GetType().Name}")

            ' Make whole page visible (it was set to 0 in OnPropertyChanged)
            page.Opacity = 1

            ' Find the main Grid with left/right columns
            Dim main_grid = find_main_grid(page)

            If main_grid IsNot Nothing Then
                ' Collect left and right panels
                Dim left_panel = find_column(main_grid, 0)
                Dim right_panel = find_column(main_grid, 1)

                ' Check if left panel has PclListItem
                Dim has_list_items = False
                If left_panel IsNot Nothing Then
                    Dim items = collect_list_items(left_panel)
                    has_list_items = items.Count > 0
                End If

                If has_list_items Then
                    ' Split animation: left items slide + right panel fade
                    Debug.WriteLine("[PageTransition] Using split animation (left items + right panel)")
                    Dispatcher.UIThread.Post(Sub()
                                                 ' Left panel: PclListItem items slide from left
                                                 If left_panel IsNot Nothing Then
                                                     Dim items = collect_list_items(left_panel)
                                                     If items.Count > 0 Then
                                                         Debug.WriteLine($"[PageTransition] Found {items.Count} PclListItem in left panel")
                                                         animate_left_panel_items(items)
                                                     End If
                                                 End If

                                                 ' Right panel: fade + slide down (starts at same time)
                                                 If right_panel IsNot Nothing Then
                                                     Debug.WriteLine($"[PageTransition] Animating right panel: {right_panel.GetType().Name}")
                                                     animate_right_panel_immediate(right_panel)
                                                 End If
                                             End Sub, DispatcherPriority.Render)
                Else
                    ' No list items: use whole-page fade+slide (like HomeView)
                    Debug.WriteLine("[PageTransition] No PclListItem, using whole-page fade+slide")
                    animate_whole_page_fade(page)
                End If
            Else
                ' No split layout, animate whole page
                Debug.WriteLine("[PageTransition] No split layout, animating whole page")
                animate_whole_page_fade(page)
            End If
        End Sub

        ''' <summary>
        ''' Find the main Grid that defines left/right columns.
        ''' </summary>
        Private Function find_main_grid(ByVal ctrl As Control) As Grid
            If ctrl Is Nothing Then Return Nothing

            Dim grid = TryCast(ctrl, Grid)
            If grid IsNot Nothing AndAlso grid.ColumnDefinitions.Count >= 2 Then Return grid

            Dim cc = TryCast(ctrl, ContentControl)
            If cc IsNot Nothing Then
                Dim content = TryCast(cc.Content, Control)
                If content IsNot Nothing Then Return find_main_grid(content)
            End If

            Dim border = TryCast(ctrl, Border)
            If border IsNot Nothing Then
                Dim child = TryCast(border.Child, Control)
                If child IsNot Nothing Then Return find_main_grid(child)
            End If

            Dim panel = TryCast(ctrl, Panel)
            If panel IsNot Nothing Then
                For i As Integer = 0 To panel.Children.Count - 1
                    Dim child = TryCast(panel.Children(i), Control)
                    If child Is Nothing Then Continue For
                    Dim result = find_main_grid(child)
                    If result IsNot Nothing Then Return result
                Next
            End If

            Return Nothing
        End Function

        ''' <summary>
        ''' Find child in specific column of a Grid.
        ''' </summary>
        Private Function find_column(ByVal grid As Grid, ByVal col As Integer) As Control
            For i As Integer = 0 To grid.Children.Count - 1
                Dim child = TryCast(grid.Children(i), Control)
                If child IsNot Nothing AndAlso Grid.GetColumn(child) = col Then Return child
            Next
            Return Nothing
        End Function

        ''' <summary>
        ''' Collect all PclListItem controls from a container.
        ''' </summary>
        Private Function collect_list_items(ByVal parent As Control) As List(Of PclListItem)
            Dim result As New List(Of PclListItem)
            collect_list_items_recursive(parent, result, 0)
            Return result
        End Function

        Private Sub collect_list_items_recursive(ByVal parent As Control, ByVal result As List(Of PclListItem), ByVal depth As Integer)
            If parent Is Nothing OrElse depth > 10 Then Return

            If TypeOf parent Is PclListItem Then
                result.Add(CType(parent, PclListItem))
                Return
            End If

            Dim border = TryCast(parent, Border)
            If border IsNot Nothing Then
                Dim child = TryCast(border.Child, Control)
                If child IsNot Nothing Then collect_list_items_recursive(child, result, depth + 1)
                Return
            End If

            Dim cc = TryCast(parent, ContentControl)
            If cc IsNot Nothing Then
                Dim content = TryCast(cc.Content, Control)
                If content IsNot Nothing Then collect_list_items_recursive(content, result, depth + 1)
                Return
            End If

            Dim scroll = TryCast(parent, ScrollViewer)
            If scroll IsNot Nothing Then
                Dim content = TryCast(scroll.Content, Control)
                If content IsNot Nothing Then collect_list_items_recursive(content, result, depth + 1)
                Return
            End If

            Dim panel = TryCast(parent, Panel)
            If panel IsNot Nothing Then
                For i As Integer = 0 To panel.Children.Count - 1
                    Dim child = TryCast(panel.Children(i), Control)
                    If child Is Nothing Then Continue For
                    collect_list_items_recursive(child, result, depth + 1)
                Next
            End If
        End Sub

        ''' <summary>
        ''' Animate left panel items: each slides from left to right with stagger.
        ''' PCL-CE style: three parallel animations per item.
        ''' </summary>
        Private Sub animate_left_panel_items(ByVal items As List(Of PclListItem))
            ' Very fast stagger: fixed 3ms interval
            Dim delays As New List(Of Integer)
            For i As Integer = 0 To items.Count - 1
                delays.Add(i)
            Next

            Debug.WriteLine($"[PageTransition] animate_left_panel_items: {items.Count} items, stagger=3ms each")

            For i As Integer = 0 To items.Count - 1
                Dim item = items(i)
                Dim delay = delays(i)

                ' Set initial state: TranslateX(-25), Opacity(0)
                ensure_translate_transform(item)
                set_translate_x(item, -25)
                item.Opacity = 0

                ' Capture for closure
                Dim captured_item = item
                Dim captured_delay = delay

                ' Schedule animation with delay
                Dim timer As New DispatcherTimer()
                timer.Interval = TimeSpan.FromMilliseconds(captured_delay)
                Dim handler As EventHandler = Nothing
                handler = Sub(s, e)
                              timer.Stop()
                              RemoveHandler timer.Tick, handler

                              ' Three parallel animations (PCL-CE style, very fast):
                              ' 1. Opacity: 0->1, 40ms, easeOut
                              ' 2. TranslateX: -25->-20 (+5), 80ms, easeOut
                              ' 3. TranslateX: -25->-5 (+20), 100ms, easeOutBack

                              Dim sw As New Stopwatch()
                              sw.Start()

                              Dim tick As Action = Nothing
                              tick = Sub()
                                         Dim elapsed = sw.ElapsedMilliseconds

                                         ' Animation 1: Opacity (40ms, easeOut)
                                         Dim opacity_progress = Math.Min(1.0, elapsed / 40.0)
                                         Dim opacity_eased = 1 - (1 - opacity_progress) * (1 - opacity_progress)
                                         captured_item.Opacity = opacity_eased

                                         ' Animation 3: TranslateX long (100ms, easeOutBack)
                                         Dim tx_long_progress = Math.Min(1.0, elapsed / 100.0)
                                         Dim c1 = 1.70158
                                         Dim c3 = c1 + 1
                                         Dim tx_long_eased = 1 + c3 * Math.Pow(tx_long_progress - 1, 3) + c1 * Math.Pow(tx_long_progress - 1, 2)
                                         Dim tx_long = -25 + 20 * tx_long_eased  ' -25 -> -5

                                         set_translate_x(captured_item, tx_long)

                                         ' Complete when longest animation finishes (100ms)
                                         If elapsed >= 100 Then
                                             captured_item.Opacity = 1
                                             set_translate_x(captured_item, -5)
                                             sw.Stop()
                                         Else
                                             Dispatcher.UIThread.Post(tick, DispatcherPriority.Render)
                                         End If
                                     End Sub
                              Dispatcher.UIThread.Post(tick, DispatcherPriority.Render)
                          End Sub
                AddHandler timer.Tick, handler
                timer.Start()
            Next
        End Sub

        ''' <summary>
        ''' Animate right panel immediately (no delay).
        ''' </summary>
        Private Sub animate_right_panel_immediate(ByVal panel As Control)
            Dim duration_ms = 250

            panel.Opacity = 0
            ensure_translate_transform(panel)
            set_translate_y(panel, -8)

            Dim sw As New Stopwatch()
            sw.Start()

            Dim tick As Action = Nothing
            tick = Sub()
                       Dim elapsed = sw.ElapsedMilliseconds
                       Dim progress = Math.Min(1.0, elapsed / CDbl(duration_ms))
                       Dim eased = 1 - (1 - progress) * (1 - progress) * (1 - progress)

                       panel.Opacity = eased
                       set_translate_y(panel, -8.0 * (1 - eased))

                       If elapsed >= duration_ms Then
                           panel.Opacity = 1
                           set_translate_y(panel, 0)
                           sw.Stop()
                       Else
                           Dispatcher.UIThread.Post(tick, DispatcherPriority.Render)
                       End If
                   End Sub

            Dispatcher.UIThread.Post(tick, DispatcherPriority.Render)
        End Sub

        ''' <summary>
        ''' Fallback: animate whole page with fade + slide.
        ''' </summary>
        Private Sub animate_whole_page_fade(ByVal page As Control)
            Debug.WriteLine("[PageTransition] animate_whole_page_fade (fallback)")
            Dim duration_ms = 250

            page.Opacity = 0
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
                       Else
                           Dispatcher.UIThread.Post(tick, DispatcherPriority.Render)
                       End If
                   End Sub

            Dispatcher.UIThread.Post(tick, DispatcherPriority.Render)
        End Sub

        Private Sub set_translate_x(ByVal target As Visual, ByVal value As Double)
            Dim tt = TryCast(target.RenderTransform, TranslateTransform)
            If tt IsNot Nothing Then tt.X = value
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
