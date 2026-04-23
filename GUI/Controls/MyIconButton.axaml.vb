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

        Private Sub RefreshAnim()
            If PART_Path Is Nothing Then Return

            If IsPointerOver Then
                Select Case ButtonTheme
                    Case IconTheme.Color
                        AnimationHelper.color(_fill_brush, Color.Parse("#0b5bcb"), 120)
                    Case IconTheme.White
                        AnimationHelper.color(_bg_brush, Color.FromArgb(50, 255, 255, 255), 120)
                        AnimationHelper.color(_fill_brush, Color.Parse("#eaf2fe"), 120)
                    Case IconTheme.Black
                        AnimationHelper.color(_fill_brush, Color.FromArgb(230, 0, 0, 0), 120)
                    Case IconTheme.Red
                        AnimationHelper.color(_fill_brush, Color.FromArgb(255, 255, 76, 76), 120)
                    Case IconTheme.Custom
                        AnimationHelper.color(_fill_brush, Color.FromArgb(255, _fill_brush.Color.R, _fill_brush.Color.G, _fill_brush.Color.B), 120)
                End Select
            Else
                Select Case ButtonTheme
                    Case IconTheme.Color
                        AnimationHelper.color(_fill_brush, Color.Parse("#4890f5"), 150)
                        AnimationHelper.color(_bg_brush, Color.FromArgb(0, 255, 255, 255), 150)
                    Case IconTheme.White
                        AnimationHelper.color(_fill_brush, Color.FromRgb(234, 242, 254), 150)
                        AnimationHelper.color(_bg_brush, Color.FromArgb(0, 255, 255, 255), 150)
                    Case IconTheme.Black
                        AnimationHelper.color(_fill_brush, Color.FromArgb(160, 0, 0, 0), 150)
                        AnimationHelper.color(_bg_brush, Color.FromArgb(0, 255, 255, 255), 150)
                    Case IconTheme.Red
                        AnimationHelper.color(_fill_brush, Color.FromArgb(160, 255, 76, 76), 150)
                        AnimationHelper.color(_bg_brush, Color.FromArgb(0, 255, 255, 255), 150)
                    Case IconTheme.Custom
                        AnimationHelper.color(_fill_brush, Color.FromArgb(160, _fill_brush.Color.R, _fill_brush.Color.G, _fill_brush.Color.B), 150)
                        AnimationHelper.color(_bg_brush, Color.FromArgb(0, 255, 255, 255), 150)
                End Select
            End If
        End Sub

        Protected Overrides Sub OnPointerEntered(ByVal e As PointerEventArgs)
            MyBase.OnPointerEntered(e)
            RefreshAnim()
        End Sub

        Protected Overrides Sub OnPointerExited(ByVal e As PointerEventArgs)
            MyBase.OnPointerExited(e)
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
