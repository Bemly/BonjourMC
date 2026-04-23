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
    ''' PCL-CE MyButton style with scale squash on press and color transitions.
    ''' </summary>
    Partial Public Class MyButton
        Inherits UserControl

        ' Instance brushes for animation
        Private ReadOnly _border_brush As New SolidColorBrush(Color.Parse("#343d4a"))
        Private ReadOnly _bg_brush As New SolidColorBrush(Color.Parse("#55ffffff"))

        ' State
        Private _is_mouse_down As Boolean = False
        Private _uuid As String = Guid.NewGuid().ToString("N").Substring(0, 8)
        Private ReadOnly _hover_timer As New DispatcherTimer() With {.Interval = TimeSpan.FromMilliseconds(50)}

        ' Styled Properties
        Public Shared ReadOnly TextProperty As StyledProperty(Of String) =
            AvaloniaProperty.Register(Of MyButton, String)("Text", "")

        Public Shared ReadOnly ColorTypeProperty As StyledProperty(Of Integer) =
            AvaloniaProperty.Register(Of MyButton, Integer)("ColorType", 0)

        Public Property Text As String
            Get
                Return GetValue(TextProperty)
            End Get
            Set(ByVal value As String)
                SetValue(TextProperty, value)
                If PART_Text IsNot Nothing Then PART_Text.Text = value
            End Set
        End Property

        Public Property ColorType As Integer
            Get
                Return GetValue(ColorTypeProperty)
            End Get
            Set(ByVal value As Integer)
                SetValue(ColorTypeProperty, value)
            End Set
        End Property

        Public Sub New()
            InitializeComponent()
            ' Use transitions for smooth, interruptible hover animations
            _border_brush.Transitions = New Transitions() From {
                New ColorTransition() With {.Property = SolidColorBrush.ColorProperty, .Duration = TimeSpan.FromMilliseconds(100)}
            }
            _bg_brush.Transitions = New Transitions() From {
                New ColorTransition() With {.Property = SolidColorBrush.ColorProperty, .Duration = TimeSpan.FromMilliseconds(100)}
            }
            AddHandler _hover_timer.Tick, Sub(s, ev)
                                              If Not IsPointerOver Then
                                                  _hover_timer.Stop()
                                                  ' Force-reset colors directly (transition animates smoothly)
                                                  Select Case ColorType
                                                      Case 0 : _border_brush.Color = Color.Parse("#343d4a")
                                                      Case 1 : _border_brush.Color = Color.Parse("#0b5bcb")
                                                      Case 2 : _border_brush.Color = Color.Parse("#ce2111")
                                                      Case Else : _border_brush.Color = Color.Parse("#343d4a")
                                                  End Select
                                                  _bg_brush.Color = Color.FromArgb(85, 255, 255, 255)
                                              End If
                                          End Sub
        End Sub

        Protected Overrides Sub OnAttachedToVisualTree(ByVal e As VisualTreeAttachmentEventArgs)
            MyBase.OnAttachedToVisualTree(e)
            If PART_Inner IsNot Nothing Then
                PART_Inner.BorderBrush = _border_brush
                PART_Inner.Background = _bg_brush
            End If
            If PART_Text IsNot Nothing Then
                PART_Text.Text = Text
            End If
            RefreshColor()
        End Sub

        Private Sub RefreshColor()
            If PART_Inner Is Nothing Then Return
            ' Transition handles animation automatically
            If PART_Background IsNot Nothing AndAlso PART_Background.IsPointerOver Then
                Select Case ColorType
                    Case 0 : _border_brush.Color = Color.Parse("#1370f3")
                    Case 1 : _border_brush.Color = Color.Parse("#1370f3")
                    Case 2 : _border_brush.Color = Color.Parse("#ff4c4c")
                    Case Else : _border_brush.Color = Color.Parse("#1370f3")
                End Select
            Else
                Select Case ColorType
                    Case 0 : _border_brush.Color = Color.Parse("#343d4a")
                    Case 1 : _border_brush.Color = Color.Parse("#0b5bcb")
                    Case 2 : _border_brush.Color = Color.Parse("#ce2111")
                    Case Else : _border_brush.Color = Color.Parse("#343d4a")
                End Select
            End If
        End Sub

        Private Sub RefreshBackground()
            If PART_Inner Is Nothing Then Return
            ' Transition handles animation automatically
            If PART_Background IsNot Nothing AndAlso PART_Background.IsPointerOver Then
                If ColorType = 2 Then
                    _bg_brush.Color = Color.FromArgb(128, 251, 221, 221)
                Else
                    _bg_brush.Color = Color.Parse("#e0eafd")
                End If
            Else
                _bg_brush.Color = Color.FromArgb(85, 255, 255, 255)
            End If
        End Sub

        ' Pointer events on the background border
        Protected Overrides Sub OnPointerEntered(ByVal e As PointerEventArgs)
            MyBase.OnPointerEntered(e)
            _hover_timer.Start()
            RefreshColor()
            RefreshBackground()
        End Sub

        Protected Overrides Sub OnPointerExited(ByVal e As PointerEventArgs)
            MyBase.OnPointerExited(e)
            _hover_timer.Stop()
            RefreshColor()
            RefreshBackground()
            If _is_mouse_down Then
                _is_mouse_down = False
                If PART_Inner IsNot Nothing Then
                    AnimationHelper.scale_to(PART_Inner, 1.0, 800, 0, AnimationHelper.ease_out_fluent_strong)
                End If
            End If
        End Sub

        Protected Overrides Sub OnPointerPressed(ByVal e As PointerPressedEventArgs)
            MyBase.OnPointerPressed(e)
            _is_mouse_down = True
            If PART_Inner IsNot Nothing Then
                AnimationHelper.scale_to(PART_Inner, 0.955, 80, 0, AnimationHelper.ease_out_fluent_extra)
                AnimationHelper.scale_delta(PART_Inner, -0.01, 700, 0, AnimationHelper.ease_out_fluent)
            End If
        End Sub

        Protected Overrides Sub OnPointerReleased(ByVal e As PointerReleasedEventArgs)
            MyBase.OnPointerReleased(e)
            If _is_mouse_down Then
                _is_mouse_down = False
                If PART_Inner IsNot Nothing Then
                    AnimationHelper.scale_to(PART_Inner, 1.0, 300, 10, AnimationHelper.ease_out_fluent)
                End If
            End If
        End Sub
    End Class

End Namespace
