Option Explicit On
Option Strict On

Imports System
Imports Avalonia
Imports Avalonia.Animation
Imports Avalonia.Controls
Imports Avalonia.Input
Imports Avalonia.Media
Imports Avalonia.Styling
Imports Avalonia.Threading
Imports GUI.Animations

Namespace Controls

    ''' <summary>
    ''' PCL-CE MyCard style card with animated shadow, title, swap arrow, and dispose animation.
    ''' </summary>
    Public Class Card
        Inherits Border

        ' Constants matching PCL-CE
        Private Const DropShadowIdleOpacity As Double = 0.07
        Private Const DropShadowHoverOpacity As Double = 0.4

        ' Instance brushes for animation (must not share with resources)
        Private ReadOnly _shadow_brush As New SolidColorBrush(Color.Parse("#3c3c3c"))
        Private ReadOnly _title_brush As New SolidColorBrush(Color.Parse("#343d4a"))
        Private ReadOnly _swap_brush As New SolidColorBrush(Color.Parse("#343d4a"))

        ' Parts
        Private _title_block As TextBlock
        Private _swap_path As Avalonia.Controls.Shapes.Path
        Private _content_grid As Grid
        Private _shadow_effect As DropShadowEffect

        ' State
        Private _is_hovered As Boolean = False
        Private _is_height_animating As Boolean = False
        Private _uuid As String = Guid.NewGuid().ToString("N").Substring(0, 8)
        Private ReadOnly _hover_timer As New DispatcherTimer() With {.Interval = TimeSpan.FromMilliseconds(50)}

        ' Styled Properties
        Public Shared ReadOnly TitleProperty As StyledProperty(Of String) =
            AvaloniaProperty.Register(Of Card, String)("Title", "")

        Public Shared ReadOnly CanSwapProperty As StyledProperty(Of Boolean) =
            AvaloniaProperty.Register(Of Card, Boolean)("CanSwap", False)

        Public Shared ReadOnly IsSwappedProperty As StyledProperty(Of Boolean) =
            AvaloniaProperty.Register(Of Card, Boolean)("IsSwapped", False)

        Public Property Title As String
            Get
                Return GetValue(TitleProperty)
            End Get
            Set(ByVal value As String)
                SetValue(TitleProperty, value)
                If _title_block IsNot Nothing Then _title_block.Text = value
            End Set
        End Property

        Public Property CanSwap As Boolean
            Get
                Return GetValue(CanSwapProperty)
            End Get
            Set(ByVal value As Boolean)
                SetValue(CanSwapProperty, value)
            End Set
        End Property

        Public Property IsSwapped As Boolean
            Get
                Return GetValue(IsSwappedProperty)
            End Get
            Set(ByVal value As Boolean)
                If GetValue(IsSwappedProperty) = value Then Return
                SetValue(IsSwappedProperty, value)
                OnSwapChanged()
            End Set
        End Property

        Public Sub New()
            Background = New SolidColorBrush(Color.Parse("#ffffff"))
            CornerRadius = New CornerRadius(5)
            Padding = New Thickness(20)
            Margin = New Thickness(0, 0, 0, 12)
            ClipToBounds = False

            ' Use transitions for smooth, interruptible hover animations
            _title_brush.Transitions = New Transitions() From {
                New ColorTransition() With {.Property = SolidColorBrush.ColorProperty, .Duration = TimeSpan.FromMilliseconds(90)}
            }
            _swap_brush.Transitions = New Transitions() From {
                New ColorTransition() With {.Property = SolidColorBrush.ColorProperty, .Duration = TimeSpan.FromMilliseconds(90)}
            }

            AddHandler _hover_timer.Tick, Sub(s, ev)
                                              If _is_hovered AndAlso Not IsPointerOver Then
                                                  _is_hovered = False
                                                  _hover_timer.Stop()
                                                  ' Force-reset colors directly (transition animates smoothly)
                                                  _shadow_effect.Opacity = DropShadowIdleOpacity
                                                  _title_brush.Color = Color.Parse("#343d4a")
                                                  If _swap_path IsNot Nothing Then
                                                      _swap_brush.Color = Color.Parse("#343d4a")
                                                  End If
                                              End If
                                          End Sub

            _shadow_effect = New DropShadowEffect() With {
                .BlurRadius = 12,
                .OffsetY = 1,
                .Opacity = DropShadowIdleOpacity,
                .Color = Color.Parse("#3c3c3c")
            }
            Effect = _shadow_effect

            ' Build internal structure
            _content_grid = New Grid()
            Child = _content_grid
        End Sub

        Protected Overrides Sub OnAttachedToVisualTree(ByVal e As VisualTreeAttachmentEventArgs)
            MyBase.OnAttachedToVisualTree(e)
            EnsureParts()
        End Sub

        Private Sub EnsureParts()
            If _title_block IsNot Nothing Then Return

            ' Title
            _title_block = New TextBlock() With {
                .HorizontalAlignment = HorizontalAlignment.Left,
                .VerticalAlignment = VerticalAlignment.Top,
                .Margin = New Thickness(15, 12, 0, 0),
                .FontWeight = FontWeight.Bold,
                .FontSize = 13,
                .IsHitTestVisible = False,
                .Foreground = _title_brush
            }
            _title_block.Text = Title
            _content_grid.Children.Add(_title_block)

            ' Swap arrow
            If CanSwap Then
                _swap_path = New Avalonia.Controls.Shapes.Path() With {
                    .HorizontalAlignment = HorizontalAlignment.Right,
                    .Stretch = Stretch.Uniform,
                    .Height = 6,
                    .Width = 10,
                    .VerticalAlignment = VerticalAlignment.Top,
                    .Margin = New Thickness(0, 17, 16, 0),
                    .Data = Geometry.Parse("M2,4 l-2,2 10,10 10,-10 -2,-2 -8,8 -8,-8 z"),
                    .Fill = _swap_brush,
                    .RenderTransform = New RotateTransform(180),
                    .RenderTransformOrigin = New RelativePoint(0.5, 0.5, RelativeUnit.Relative)
                }
                _content_grid.Children.Add(_swap_path)
            End If
        End Sub

        ' --- Hover Animation (with timer fallback to prevent stuck state) ---
        Protected Overrides Sub OnPointerEntered(ByVal e As PointerEventArgs)
            MyBase.OnPointerEntered(e)
            If _is_hovered Then Return
            _is_hovered = True
            _hover_timer.Start()

            ' Shadow: 0.07 → 0.4, 90ms
            AnimationHelper.fade(Me, 1.0, 90)
            animate_shadow(DropShadowHoverOpacity, 90)

            ' Title color: Brush1 → Brush2, 90ms (transition handles animation)
            _title_brush.Color = Color.Parse("#0b5bcb")

            ' Swap arrow color, 90ms (transition handles animation)
            If _swap_path IsNot Nothing Then
                _swap_brush.Color = Color.Parse("#0b5bcb")
            End If
        End Sub

        Protected Overrides Sub OnPointerExited(ByVal e As PointerEventArgs)
            MyBase.OnPointerExited(e)
            If Not _is_hovered Then Return
            _is_hovered = False
            _hover_timer.Stop()

            ' Shadow: 0.4 → 0.07, 90ms
            animate_shadow(DropShadowIdleOpacity, 90)

            ' Title color: Brush2 → Brush1, 90ms (transition handles animation)
            _title_brush.Color = Color.Parse("#343d4a")

            ' Swap arrow color, 90ms (transition handles animation)
            If _swap_path IsNot Nothing Then
                _swap_brush.Color = Color.Parse("#343d4a")
            End If
        End Sub

        Private Sub animate_shadow(ByVal target_opacity As Double, ByVal duration_ms As Integer)
            Dim anim As New Animation()
            anim.Duration = TimeSpan.FromMilliseconds(duration_ms)
            anim.FillMode = FillMode.Forward
            anim.Easing = AnimationHelper.ease_linear
            Dim kf0 As New KeyFrame() With {.Cue = New Cue(0)}
            kf0.Setters.Add(New Setter(DropShadowEffect.OpacityProperty, _shadow_effect.Opacity))
            anim.Children.Add(kf0)
            Dim kf1 As New KeyFrame() With {.Cue = New Cue(1)}
            kf1.Setters.Add(New Setter(DropShadowEffect.OpacityProperty, target_opacity))
            anim.Children.Add(kf1)
            Dim token = anim.RunAsync(_shadow_effect)
        End Sub

        ' --- Swap Animation (PCL-CE: 250ms OutFluent ExtraStrong) ---
        Private Sub OnSwapChanged()
            If _swap_path Is Nothing Then Return
            Dim target_angle = If(IsSwapped, 0.0, 180.0)
            AnimationHelper.rotate_to(_swap_path, target_angle, 250, 0, AnimationHelper.ease_out_fluent_extra)
        End Sub

        ''' <summary>
        ''' Dispose animation matching PCL-CE: scale -0.08 + opacity fade + height shrink.
        ''' </summary>
        Public Sub AnimateDispose(Optional ByVal remove_action As Action = Nothing)
            IsHitTestVisible = False

            ' Scale -0.08, 200ms InFluent
            AnimationHelper.scale_delta(Me, -0.08, 200, 0, AnimationHelper.ease_in_fluent)

            ' Opacity → 0, 200ms OutFluent
            AnimationHelper.fade(Me, 0.0, 200, 0, AnimationHelper.ease_out_fluent)

            ' Height → 0, 150ms OutFluent, 100ms delay
            Dim current_height = Bounds.Height
            If current_height > 0 Then
                Dim height_anim As New Animation()
                height_anim.Duration = TimeSpan.FromMilliseconds(150)
                height_anim.Delay = TimeSpan.FromMilliseconds(100)
                height_anim.FillMode = FillMode.Forward
                height_anim.Easing = AnimationHelper.ease_out_fluent
                Dim kf0 As New KeyFrame() With {.Cue = New Cue(0)}
                kf0.Setters.Add(New Setter(HeightProperty, current_height))
                height_anim.Children.Add(kf0)
                Dim kf1 As New KeyFrame() With {.Cue = New Cue(1)}
                kf1.Setters.Add(New Setter(HeightProperty, 0.0))
                height_anim.Children.Add(kf1)
                Dim token = height_anim.RunAsync(Me)
            End If

            ' Remove after animation
            AnimationHelper.delayed_code(Sub()
                                             If remove_action IsNot Nothing Then remove_action.Invoke()
                                         End Sub, 300)
        End Sub
    End Class

End Namespace
