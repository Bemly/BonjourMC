Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports Avalonia
Imports Avalonia.Animation
Imports Avalonia.Controls
Imports Avalonia.Controls.Shapes
Imports Avalonia.Input
Imports Avalonia.Media
Imports Avalonia.Styling
Imports GUI.Animations

Namespace Controls

    Partial Public Class PclListItem
        Inherits UserControl

        Private ReadOnly _rect_bg_brush As New SolidColorBrush(Color.Parse("#bee0eafd"))
        Private ReadOnly _rect_border_brush As New SolidColorBrush(Color.Parse("#d5e6fd"))
        Private ReadOnly _fg_brush As New SolidColorBrush(Color.Parse("#343d4a"))

        Private _rect_back As Border
        Private _check_bar As Border

        Private _is_mouse_down As Boolean = False
        Private _uuid As String = Guid.NewGuid().ToString("N").Substring(0, 8)
        Private _state_last As String = ""

        Public Enum CheckType
            None = 0
            Clickable = 1
            RadioBox = 2
            CheckBox = 3
        End Enum

        Public Shared ReadOnly TitleProperty As StyledProperty(Of String) =
            AvaloniaProperty.Register(Of PclListItem, String)("Title", "")

        Public Shared ReadOnly InfoProperty As StyledProperty(Of String) =
            AvaloniaProperty.Register(Of PclListItem, String)("Info", "")

        Public Shared ReadOnly LogoProperty As StyledProperty(Of String) =
            AvaloniaProperty.Register(Of PclListItem, String)("Logo", "")

        Public Shared ReadOnly CheckedProperty As StyledProperty(Of Boolean) =
            AvaloniaProperty.Register(Of PclListItem, Boolean)("Checked", False)

        Public Shared ReadOnly ListItemTypeProperty As StyledProperty(Of CheckType) =
            AvaloniaProperty.Register(Of PclListItem, CheckType)("ListItemType", CheckType.None)

        Public Property Title As String
            Get
                Return GetValue(TitleProperty)
            End Get
            Set(ByVal value As String)
                SetValue(TitleProperty, value)
                If PART_Title IsNot Nothing Then PART_Title.Text = value
            End Set
        End Property

        Public Property Info As String
            Get
                Return GetValue(InfoProperty)
            End Get
            Set(ByVal value As String)
                SetValue(InfoProperty, value)
            End Set
        End Property

        Public Property Logo As String
            Get
                Return GetValue(LogoProperty)
            End Get
            Set(ByVal value As String)
                SetValue(LogoProperty, value)
            End Set
        End Property

        Public Property Checked As Boolean
            Get
                Return GetValue(CheckedProperty)
            End Get
            Set(ByVal value As Boolean)
                If GetValue(CheckedProperty) = value Then Return
                SetValue(CheckedProperty, value)
                OnCheckedChanged()
            End Set
        End Property

        Public Property ListItemType As CheckType
            Get
                Return GetValue(ListItemTypeProperty)
            End Get
            Set(ByVal value As CheckType)
                SetValue(ListItemTypeProperty, value)
            End Set
        End Property

        Public Sub New()
            InitializeComponent()
        End Sub

        Protected Overrides Sub OnAttachedToVisualTree(ByVal e As VisualTreeAttachmentEventArgs)
            MyBase.OnAttachedToVisualTree(e)
            If PART_Title IsNot Nothing Then
                PART_Title.Text = Title
                PART_Title.Foreground = _fg_brush
            End If
        End Sub

        Private Function EnsureRectBack() As Border
            If _rect_back IsNot Nothing Then Return _rect_back
            _rect_back = New Border() With {
                .CornerRadius = New CornerRadius(6),
                .RenderTransform = New ScaleTransform(0.8, 0.8),
                .RenderTransformOrigin = New RelativePoint(0.5, 0.5, RelativeUnit.Relative),
                .BorderThickness = New Thickness(1),
                .IsHitTestVisible = False,
                .Opacity = 0,
                .Background = _rect_bg_brush,
                .BorderBrush = _rect_border_brush
            }
            Grid.SetColumnSpan(_rect_back, 999)
            Grid.SetRowSpan(_rect_back, 999)
            If PART_Back IsNot Nothing Then PART_Back.Children.Insert(0, _rect_back)
            Return _rect_back
        End Function

        Private Sub RefreshColor()
            Dim state_new As String
            Dim time_ms As Integer

            If _is_mouse_down AndAlso Not (ListItemType = CheckType.RadioBox AndAlso Checked) Then
                state_new = "MouseDown"
                time_ms = 120
            ElseIf IsPointerOver Then
                state_new = "MouseOver"
                time_ms = 120
            Else
                state_new = "Idle"
                time_ms = 180
            End If

            If _state_last = state_new Then Return
            _state_last = state_new

            Dim rect = EnsureRectBack()

            If IsPointerOver Then
                AnimationHelper.color(_rect_bg_brush, If(_is_mouse_down, Color.Parse("#d5e6fd"), Color.Parse("#bee0eafd")), time_ms)
                AnimationHelper.fade(rect, 1.0, time_ms, 0, AnimationHelper.ease_out_fluent)
                AnimationHelper.scale_to(rect, 1.0, CInt(time_ms * 1.6), 0, AnimationHelper.ease_out_fluent)

                If _is_mouse_down Then
                    AnimationHelper.scale_to(Me, 0.98, CInt(time_ms * 0.9), 0, AnimationHelper.ease_out_fluent)
                Else
                    AnimationHelper.scale_to(Me, 1.0, CInt(time_ms * 1.2), 0, AnimationHelper.ease_out_fluent)
                End If
            Else
                AnimationHelper.fade(rect, 0.0, time_ms, 0, AnimationHelper.ease_out_fluent)
                AnimationHelper.scale_to(Me, 1.0, CInt(time_ms * 3), 0, AnimationHelper.ease_out_fluent)
                AnimationHelper.scale_to(rect, 0.996, time_ms, 0, AnimationHelper.ease_out_fluent)
                AnimationHelper.delayed_code(Sub()
                                                 If rect IsNot Nothing Then
                                                     Dim st = TryCast(rect.RenderTransform, ScaleTransform)
                                                     If st IsNot Nothing Then
                                                         st.ScaleX = 0.75
                                                         st.ScaleY = 0.75
                                                     End If
                                                 End If
                                             End Sub, time_ms + 10)
            End If
        End Sub

        Private Sub OnCheckedChanged()
            If Checked Then
                If _check_bar Is Nothing Then
                    _check_bar = New Border() With {
                        .Width = 5,
                        .Height = 0,
                        .CornerRadius = New CornerRadius(2),
                        .VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                        .HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                        .Opacity = 0,
                        .Background = New SolidColorBrush(Color.Parse("#1370f3"))
                    }
                    Grid.SetRowSpan(_check_bar, 4)
                    If PART_Back IsNot Nothing Then PART_Back.Children.Add(_check_bar)
                End If

                AnimationHelper.fade(_check_bar, 1.0, 30)
                Dim height_anim1 As New Animation()
                height_anim1.Duration = TimeSpan.FromMilliseconds(200)
                height_anim1.FillMode = FillMode.Forward
                height_anim1.Easing = AnimationHelper.ease_out_fluent_weak
                Dim kf0 As New KeyFrame() With {.Cue = New Cue(0)}
                kf0.Setters.Add(New Setter(HeightProperty, 0.0))
                height_anim1.Children.Add(kf0)
                Dim kf1 As New KeyFrame() With {.Cue = New Cue(1)}
                kf1.Setters.Add(New Setter(HeightProperty, 8.0))
                height_anim1.Children.Add(kf1)
                Dim token1 = height_anim1.RunAsync(_check_bar)

                AnimationHelper.delayed_code(Sub()
                                                 Dim height_anim2 As New Animation()
                                                 height_anim2.Duration = TimeSpan.FromMilliseconds(300)
                                                 height_anim2.FillMode = FillMode.Forward
                                                 height_anim2.Easing = AnimationHelper.ease_out_back_weak
                                                 Dim kf0b As New KeyFrame() With {.Cue = New Cue(0)}
                                                 kf0b.Setters.Add(New Setter(HeightProperty, 8.0))
                                                 height_anim2.Children.Add(kf0b)
                                                 Dim kf1b As New KeyFrame() With {.Cue = New Cue(1)}
                                                 kf1b.Setters.Add(New Setter(HeightProperty, 20.0))
                                                 height_anim2.Children.Add(kf1b)
                                                 Dim token2 = height_anim2.RunAsync(_check_bar)
                                             End Sub, 200)

                AnimationHelper.color(_fg_brush, Color.Parse("#0b5bcb"), 200)
            Else
                If _check_bar IsNot Nothing Then
                    AnimationHelper.fade(_check_bar, 0.0, 70, 40)
                    Dim height_anim As New Animation()
                    height_anim.Duration = TimeSpan.FromMilliseconds(120)
                    height_anim.FillMode = FillMode.Forward
                    height_anim.Easing = AnimationHelper.ease_in_fluent_weak
                    Dim kf0 As New KeyFrame() With {.Cue = New Cue(0)}
                    kf0.Setters.Add(New Setter(HeightProperty, _check_bar.Height))
                    height_anim.Children.Add(kf0)
                    Dim kf1 As New KeyFrame() With {.Cue = New Cue(1)}
                    kf1.Setters.Add(New Setter(HeightProperty, 0.0))
                    height_anim.Children.Add(kf1)
                    Dim token = height_anim.RunAsync(_check_bar)
                End If
                AnimationHelper.color(_fg_brush, Color.Parse("#343d4a"), 120)
            End If
        End Sub

        Protected Overrides Sub OnPointerEntered(ByVal e As PointerEventArgs)
            MyBase.OnPointerEntered(e)
            RefreshColor()
        End Sub

        Protected Overrides Sub OnPointerExited(ByVal e As PointerEventArgs)
            MyBase.OnPointerExited(e)
            _is_mouse_down = False
            RefreshColor()
        End Sub

        Protected Overrides Sub OnPointerPressed(ByVal e As PointerPressedEventArgs)
            MyBase.OnPointerPressed(e)
            If ListItemType <> CheckType.None Then
                _is_mouse_down = True
                RefreshColor()
            End If
        End Sub

        Protected Overrides Sub OnPointerReleased(ByVal e As PointerReleasedEventArgs)
            MyBase.OnPointerReleased(e)
            If Not _is_mouse_down Then Return
            _is_mouse_down = False

            Select Case ListItemType
                Case CheckType.RadioBox
                    If Not Checked Then Checked = True
                Case CheckType.CheckBox
                    Checked = Not Checked
            End Select

            RefreshColor()
        End Sub
    End Class

End Namespace
