Option Explicit On
Option Strict On

Imports System
Imports System.Windows.Input
Imports Avalonia
Imports Avalonia.Animation
Imports Avalonia.Controls
Imports Avalonia.Controls.Shapes
Imports Avalonia.Input
Imports Avalonia.Media
Imports Avalonia.Styling
Imports Avalonia.Threading
Imports GUI.Animations

Namespace Controls

    ''' <summary>
    ''' PCL-CE MyIconButton style — circular icon button with scale bounce on click.
    ''' </summary>
    Partial Public Class MyIconButton
        Inherits UserControl

        ' Instance brushes for animation
        Private ReadOnly _fill_brush As New SolidColorBrush(Color.Parse("#96c0f9"))
        Private ReadOnly _bg_brush As New SolidColorBrush(Color.FromArgb(0, 255, 255, 255))

        ' State
        Private _is_mouse_down As Boolean = False
        Private _uuid As String = Guid.NewGuid().ToString("N").Substring(0, 8)
        Private ReadOnly _hover_timer As New DispatcherTimer() With {.Interval = TimeSpan.FromMilliseconds(50)}

        Public Enum IconTheme
            Color = 0
            White = 1
            Black = 2
            Red = 3
            Custom = 4
        End Enum

        ' Styled Properties
        Public Shared ReadOnly LogoProperty As StyledProperty(Of String) =
            AvaloniaProperty.Register(Of MyIconButton, String)("Logo", "")

        Public Shared ReadOnly LogoScaleProperty As StyledProperty(Of Double) =
            AvaloniaProperty.Register(Of MyIconButton, Double)("LogoScale", 1.0)

        Public Shared ReadOnly ButtonThemeProperty As StyledProperty(Of IconTheme) =
            AvaloniaProperty.Register(Of MyIconButton, IconTheme)("ButtonTheme", IconTheme.Color)

        Public Shared ReadOnly CommandProperty As StyledProperty(Of ICommand) =
            AvaloniaProperty.Register(Of MyIconButton, ICommand)("Command", Nothing)

        Public Property Logo As String
            Get
                Return GetValue(LogoProperty)
            End Get
            Set(ByVal value As String)
                SetValue(LogoProperty, value)
                If PART_Path IsNot Nothing Then
                    Try
                        PART_Path.Data = Geometry.Parse(value)
                    Catch
                    End Try
                End If
            End Set
        End Property

        Public Property LogoScale As Double
            Get
                Return GetValue(LogoScaleProperty)
            End Get
            Set(ByVal value As Double)
                SetValue(LogoScaleProperty, value)
                If PART_Path IsNot Nothing Then
                    PART_Path.RenderTransform = New ScaleTransform(value, value)
                End If
            End Set
        End Property

        Public Property ButtonTheme As IconTheme
            Get
                Return GetValue(ButtonThemeProperty)
            End Get
            Set(ByVal value As IconTheme)
                SetValue(ButtonThemeProperty, value)
                RefreshAnim()
            End Set
        End Property

        Public Property Command As ICommand
            Get
                Return GetValue(CommandProperty)
            End Get
            Set(ByVal value As ICommand)
                SetValue(CommandProperty, value)
            End Set
        End Property

        Public Sub New()
            InitializeComponent()
            ' Use transitions for smooth, interruptible hover animations
            _fill_brush.Transitions = New Transitions() From {
                New ColorTransition() With {.Property = SolidColorBrush.ColorProperty, .Duration = TimeSpan.FromMilliseconds(120)}
            }
            _bg_brush.Transitions = New Transitions() From {
                New ColorTransition() With {.Property = SolidColorBrush.ColorProperty, .Duration = TimeSpan.FromMilliseconds(120)}
            }
            AddHandler _hover_timer.Tick, Sub(s, ev)
                                              If Not IsPointerOver Then
                                                  _hover_timer.Stop()
                                                  ' Force-reset colors directly (transition animates smoothly)
                                                  apply_idle_colors()
                                              End If
                                          End Sub
        End Sub

        Protected Overrides Sub OnAttachedToVisualTree(ByVal e As VisualTreeAttachmentEventArgs)
            MyBase.OnAttachedToVisualTree(e)
            If PART_Back IsNot Nothing Then
                PART_Back.Background = _bg_brush
            End If
            If PART_Path IsNot Nothing Then
                PART_Path.Fill = _fill_brush
                If LogoScale <> 1.0 Then
                    PART_Path.RenderTransform = New ScaleTransform(LogoScale, LogoScale)
                End If
                If Not String.IsNullOrEmpty(Logo) Then
                    Try
                        PART_Path.Data = Geometry.Parse(Logo)
                    Catch
                    End Try
                End If
            End If
            RefreshAnim()
        End Sub

        Private Sub apply_idle_colors()
            Select Case ButtonTheme
                Case IconTheme.Color
                    _fill_brush.Color = Color.Parse("#4890f5")
                    _bg_brush.Color = Color.FromArgb(0, 255, 255, 255)
                Case IconTheme.White
                    _fill_brush.Color = Color.FromRgb(234, 242, 254)
                    _bg_brush.Color = Color.FromArgb(0, 255, 255, 255)
                Case IconTheme.Black
                    _fill_brush.Color = Color.FromArgb(160, 0, 0, 0)
                    _bg_brush.Color = Color.FromArgb(0, 255, 255, 255)
                Case IconTheme.Red
                    _fill_brush.Color = Color.FromArgb(160, 255, 76, 76)
                    _bg_brush.Color = Color.FromArgb(0, 255, 255, 255)
                Case IconTheme.Custom
                    _fill_brush.Color = Color.FromArgb(160, _fill_brush.Color.R, _fill_brush.Color.G, _fill_brush.Color.B)
                    _bg_brush.Color = Color.FromArgb(0, 255, 255, 255)
            End Select
        End Sub

        Private Sub RefreshAnim()
            If PART_Path Is Nothing Then Return
            ' Transition handles animation automatically
            If IsPointerOver Then
                Select Case ButtonTheme
                    Case IconTheme.Color
                        _fill_brush.Color = Color.Parse("#0b5bcb")
                    Case IconTheme.White
                        _bg_brush.Color = Color.FromArgb(50, 255, 255, 255)
                        _fill_brush.Color = Color.Parse("#eaf2fe")
                    Case IconTheme.Black
                        _fill_brush.Color = Color.FromArgb(230, 0, 0, 0)
                    Case IconTheme.Red
                        _fill_brush.Color = Color.FromArgb(255, 255, 76, 76)
                    Case IconTheme.Custom
                        _fill_brush.Color = Color.FromArgb(255, _fill_brush.Color.R, _fill_brush.Color.G, _fill_brush.Color.B)
                End Select
            Else
                apply_idle_colors()
            End If
        End Sub

        Protected Overrides Sub OnPointerEntered(ByVal e As PointerEventArgs)
            MyBase.OnPointerEntered(e)
            _hover_timer.Start()
            RefreshAnim()
        End Sub

        Protected Overrides Sub OnPointerExited(ByVal e As PointerEventArgs)
            MyBase.OnPointerExited(e)
            _hover_timer.Stop()
            _is_mouse_down = False
            If PART_Back IsNot Nothing Then
                AnimationHelper.scale_to(PART_Back, 1.0, 250, 0, AnimationHelper.ease_out_fluent)
            End If
            RefreshAnim()
        End Sub

        Protected Overrides Sub OnPointerPressed(ByVal e As PointerPressedEventArgs)
            MyBase.OnPointerPressed(e)
            _is_mouse_down = True
            If PART_Back IsNot Nothing Then
                AnimationHelper.scale_to(PART_Back, 0.8, 400, 0, AnimationHelper.ease_out_fluent_strong)
            End If
        End Sub

        Protected Overrides Sub OnPointerReleased(ByVal e As PointerReleasedEventArgs)
            MyBase.OnPointerReleased(e)
            If _is_mouse_down Then
                _is_mouse_down = False
                If PART_Back IsNot Nothing Then
                    AnimationHelper.scale_to(PART_Back, 1.05, 250, 0, AnimationHelper.ease_out_back_weak)
                    AnimationHelper.scale_delta(PART_Back, -0.05, 250, 0, AnimationHelper.ease_out_fluent_strong)
                End If
                If Command IsNot Nothing AndAlso Command.CanExecute(Nothing) Then
                    Command.Execute(Nothing)
                End If
            End If
            RefreshAnim()
        End Sub
    End Class

End Namespace
