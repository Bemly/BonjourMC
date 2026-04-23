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

    Partial Public Class PclRadioButton
        Inherits UserControl

        Private ReadOnly _bg_brush As New SolidColorBrush(Color.Parse("#01eaf2fe"))
        Private ReadOnly _fill_brush As New SolidColorBrush(Colors.White)
        Private ReadOnly _text_brush As New SolidColorBrush(Colors.White)

        Private _is_mouse_down As Boolean = False
        Private _uuid As String = Guid.NewGuid().ToString("N").Substring(0, 8)
        Private ReadOnly _hover_timer As New DispatcherTimer() With {.Interval = TimeSpan.FromMilliseconds(50)}

        Public Enum RadioColorType
            White = 0
            Highlight = 1
        End Enum

        Public Shared ReadOnly TextProperty As StyledProperty(Of String) =
            AvaloniaProperty.Register(Of PclRadioButton, String)("Text", "")

        Public Shared ReadOnly CheckedProperty As StyledProperty(Of Boolean) =
            AvaloniaProperty.Register(Of PclRadioButton, Boolean)("Checked", False)

        Public Shared ReadOnly LogoProperty As StyledProperty(Of String) =
            AvaloniaProperty.Register(Of PclRadioButton, String)("Logo", "")

        Public Shared ReadOnly ColorTypeProperty As StyledProperty(Of RadioColorType) =
            AvaloniaProperty.Register(Of PclRadioButton, RadioColorType)("ColorType", RadioColorType.White)

        Public Shared ReadOnly LogoScaleProperty As StyledProperty(Of Double) =
            AvaloniaProperty.Register(Of PclRadioButton, Double)("LogoScale", 1.0)

        Public Shared ReadOnly CommandProperty As StyledProperty(Of ICommand) =
            AvaloniaProperty.Register(Of PclRadioButton, ICommand)("Command", Nothing)

        Public Property Text As String
            Get
                Return GetValue(TextProperty)
            End Get
            Set(ByVal value As String)
                SetValue(TextProperty, value)
                If PART_Text IsNot Nothing Then PART_Text.Text = value
            End Set
        End Property

        Public Property Checked As Boolean
            Get
                Return GetValue(CheckedProperty)
            End Get
            Set(ByVal value As Boolean)
                If GetValue(CheckedProperty) = value Then Return
                SetValue(CheckedProperty, value)
                SetChecked(value, False, True)
            End Set
        End Property

        Public Property Logo As String
            Get
                Return GetValue(LogoProperty)
            End Get
            Set(ByVal value As String)
                SetValue(LogoProperty, value)
                If PART_Logo IsNot Nothing Then
                    Try
                        PART_Logo.Data = Geometry.Parse(value)
                    Catch
                    End Try
                End If
            End Set
        End Property

        Public Property ColorType As RadioColorType
            Get
                Return GetValue(ColorTypeProperty)
            End Get
            Set(ByVal value As RadioColorType)
                SetValue(ColorTypeProperty, value)
                RefreshColor(True)
            End Set
        End Property

        Public Property LogoScale As Double
            Get
                Return GetValue(LogoScaleProperty)
            End Get
            Set(ByVal value As Double)
                SetValue(LogoScaleProperty, value)
                If PART_Logo IsNot Nothing Then
                    PART_Logo.RenderTransform = New ScaleTransform(value, value)
                End If
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

        Public Property IsChecked As Boolean
            Get
                Return Checked
            End Get
            Set(ByVal value As Boolean)
                Checked = value
            End Set
        End Property

        Public Sub New()
            InitializeComponent()
            ' Use transition for smooth, interruptible hover animation
            _bg_brush.Transitions = New Transitions() From {
                New ColorTransition() With {.Property = SolidColorBrush.ColorProperty, .Duration = TimeSpan.FromMilliseconds(120)}
            }
            AddHandler _hover_timer.Tick, Sub(s, ev)
                                              If Not IsPointerOver Then
                                                  _hover_timer.Stop()
                                                  If Not Checked Then
                                                      ' Force-reset color directly (transition animates smoothly)
                                                      If PART_Back IsNot Nothing Then _bg_brush.Color = Color.Parse("#01eaf2fe")
                                                  End If
                                              End If
                                          End Sub
        End Sub

        Protected Overrides Sub OnAttachedToVisualTree(ByVal e As VisualTreeAttachmentEventArgs)
            MyBase.OnAttachedToVisualTree(e)
            If PART_Logo IsNot Nothing Then
                PART_Logo.Fill = _fill_brush
                If LogoScale <> 1.0 Then
                    PART_Logo.RenderTransform = New ScaleTransform(LogoScale, LogoScale)
                End If
                If Not String.IsNullOrEmpty(Logo) Then
                    Try
                        PART_Logo.Data = Geometry.Parse(Logo)
                    Catch
                    End Try
                End If
            End If
            If PART_Text IsNot Nothing Then
                PART_Text.Foreground = _text_brush
                PART_Text.Text = Text
            End If
            If PART_Back IsNot Nothing Then
                PART_Back.Background = _bg_brush
            End If
            RefreshColor(True)
        End Sub

        Public Sub SetChecked(ByVal value As Boolean, ByVal raiseByMouse As Boolean, ByVal animate As Boolean)
            Dim changed = (Checked <> value)
            If changed Then
                SetValue(CheckedProperty, value)
            End If

            If Parent IsNot Nothing Then
                Dim panel = TryCast(Parent, Panel)
                If panel IsNot Nothing Then
                    If value Then
                        For Each child In panel.Children
                            Dim rb = TryCast(child, PclRadioButton)
                            If rb IsNot Nothing AndAlso rb IsNot Me AndAlso rb.Checked Then
                                rb.SetChecked(False, False, animate)
                            End If
                        Next
                    End If
                End If
            End If

            If changed Then
                RefreshColor(animate)
            End If
        End Sub

        Private Sub RefreshColor(Optional ByVal animate As Boolean = True)
            If PART_Logo Is Nothing OrElse PART_Text Is Nothing Then Return

            ' Transition on _bg_brush handles animation automatically
            Select Case ColorType
                Case RadioColorType.White
                    If Checked Then
                        Dim accent = Color.Parse("#1370f3")
                        _fill_brush.Color = accent
                        _text_brush.Color = accent
                        If PART_Back IsNot Nothing Then _bg_brush.Color = Colors.White
                    ElseIf _is_mouse_down Then
                        _bg_brush.Color = Color.FromArgb(120, 234, 242, 254)
                    ElseIf IsPointerOver Then
                        _fill_brush.Color = Colors.White
                        _text_brush.Color = Colors.White
                        If PART_Back IsNot Nothing Then _bg_brush.Color = Color.FromArgb(50, 234, 242, 254)
                    Else
                        _fill_brush.Color = Colors.White
                        _text_brush.Color = Colors.White
                        If PART_Back IsNot Nothing Then _bg_brush.Color = Color.Parse("#01eaf2fe")
                    End If

                Case RadioColorType.Highlight
                    If Checked Then
                        _fill_brush.Color = Colors.White
                        _text_brush.Color = Colors.White
                        If PART_Back IsNot Nothing Then _bg_brush.Color = Color.Parse("#1370f3")
                    ElseIf IsPointerOver Then
                        _fill_brush.Color = Color.Parse("#1370f3")
                        _text_brush.Color = Color.Parse("#1370f3")
                        If PART_Back IsNot Nothing Then _bg_brush.Color = Color.Parse("#e0eafd")
                    Else
                        _fill_brush.Color = Color.Parse("#1370f3")
                        _text_brush.Color = Color.Parse("#1370f3")
                        If PART_Back IsNot Nothing Then _bg_brush.Color = Color.Parse("#01eaf2fe")
                    End If
            End Select
        End Sub

        Protected Overrides Sub OnPointerEntered(ByVal e As PointerEventArgs)
            MyBase.OnPointerEntered(e)
            _hover_timer.Start()
            If Not Checked Then RefreshColor(True)
        End Sub

        Protected Overrides Sub OnPointerExited(ByVal e As PointerEventArgs)
            MyBase.OnPointerExited(e)
            _hover_timer.Stop()
            _is_mouse_down = False
            If Not Checked Then RefreshColor(True)
        End Sub

        Protected Overrides Sub OnPointerPressed(ByVal e As PointerPressedEventArgs)
            MyBase.OnPointerPressed(e)
            If Checked Then Return
            _is_mouse_down = True
            RefreshColor(True)
        End Sub

        Protected Overrides Sub OnPointerReleased(ByVal e As PointerReleasedEventArgs)
            MyBase.OnPointerReleased(e)
            If Checked Then Return
            If Not _is_mouse_down Then Return
            _is_mouse_down = False
            SetChecked(True, True, True)
            If Command IsNot Nothing AndAlso Command.CanExecute(Nothing) Then
                Command.Execute(Nothing)
            End If
        End Sub
    End Class

End Namespace
