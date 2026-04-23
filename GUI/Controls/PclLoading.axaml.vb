Option Explicit On
Option Strict On

Imports System
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
    ''' PCL-CE PclLoading style — pickaxe mining animation with dust particles and error state.
    ''' </summary>
    Partial Public Class PclLoading
        Inherits UserControl

        ' State
        Private _is_looping As Boolean = False
        Private _inner_state As Integer = 0 ' 0=Run, 1=Stop, 2=Error

        Public Enum LoadingState
            Unloaded = -1
            Run = 0
            [Stop] = 1
            [Error] = 2
        End Enum

        ' Styled Properties
        Public Shared ReadOnly TextProperty As StyledProperty(Of String) =
            AvaloniaProperty.Register(Of PclLoading, String)("Text", "Loading")

        Public Shared ReadOnly StateProperty As StyledProperty(Of LoadingState) =
            AvaloniaProperty.Register(Of PclLoading, LoadingState)("State", LoadingState.Run)

        Public Shared ReadOnly HasAnimationProperty As StyledProperty(Of Boolean) =
            AvaloniaProperty.Register(Of PclLoading, Boolean)("HasAnimation", True)

        Public Property Text As String
            Get
                Return GetValue(TextProperty)
            End Get
            Set(ByVal value As String)
                SetValue(TextProperty, value)
                If PART_Text IsNot Nothing Then PART_Text.Text = value
            End Set
        End Property

        Public Property State As LoadingState
            Get
                Return GetValue(StateProperty)
            End Get
            Set(ByVal value As LoadingState)
                If GetValue(StateProperty) = value Then Return
                SetValue(StateProperty, value)
                OnStateChanged()
            End Set
        End Property

        Public Property HasAnimation As Boolean
            Get
                Return GetValue(HasAnimationProperty)
            End Get
            Set(ByVal value As Boolean)
                SetValue(HasAnimationProperty, value)
            End Set
        End Property

        Public Sub New()
            InitializeComponent()
        End Sub

        Protected Overrides Sub OnAttachedToVisualTree(ByVal e As VisualTreeAttachmentEventArgs)
            MyBase.OnAttachedToVisualTree(e)
            If PART_Text IsNot Nothing Then PART_Text.Text = Text
            OnStateChanged()
        End Sub

        Private Sub OnStateChanged()
            Dim new_state = CInt(GetValue(StateProperty))
            Dim old_state = _inner_state
            _inner_state = new_state

            If new_state = 0 Then ' Run
                AniLoop()
            End If

            If (old_state = 2) <> (new_state = 2) Then
                ErrorAnimation(new_state = 2)
            End If
        End Sub

        ' --- Mining Loop Animation ---
        Private Sub AniLoop()
            If Not HasAnimation OrElse _is_looping OrElse _inner_state <> 0 OrElse Not IsLoaded Then Return
            If PART_Pickaxe Is Nothing Then Return
            _is_looping = True

            Dim current_angle = 55.0 ' Initial angle

            ' Phase 1: Wind-up (350ms InBack, 250ms delay)
            AnimationHelper.rotate_to(PART_Pickaxe, 35.0, 350, 250, AnimationHelper.ease_in_back_weak) ' 55 - 20 = 35

            ' Phase 2: Strike down (900ms OutFluent, sequential after wind-up)
            AnimationHelper.delayed_code(Sub()
                                             If Not _is_looping Then Return
                                             AnimationHelper.rotate_to(PART_Pickaxe, 85.0, 900, 0, AnimationHelper.ease_out_fluent) ' 35 + 50 = 85

                                             ' Phase 3: Bounce back (900ms OutElastic, sequential after strike)
                                             AnimationHelper.delayed_code(Sub()
                                                                              If Not _is_looping Then Return
                                                                              AnimationHelper.rotate_to(PART_Pickaxe, 110.0, 900, 0, AnimationHelper.ease_out_elastic_weak) ' 85 + 25 = 110

                                                                              ' Dust particles on impact
                                                                              TriggerDust()

                                                                              ' Loop restart
                                                                              AnimationHelper.delayed_code(Sub()
                                                                                                               If Not _is_looping Then Return
                                                                                                               ' Reset pickaxe to initial position
                                                                                                               AnimationHelper.rotate_to(PART_Pickaxe, 55.0, 1, 0, AnimationHelper.ease_linear)
                                                                                                               _is_looping = False
                                                                                                               AniLoop()
                                                                                                           End Sub, 900)
                                                                          End Sub, 900)
                                         End Sub, 600) ' 250 + 350 = 600ms
        End Sub

        ' --- Dust Particles ---
        Private Sub TriggerDust()
            If PART_DustLeft Is Nothing OrElse PART_DustRight Is Nothing Then Return

            ' Reset positions
            PART_DustLeft.Opacity = 1
            PART_DustLeft.Margin = New Thickness(7, 41, 0, 0)
            PART_DustRight.Opacity = 1
            PART_DustRight.Margin = New Thickness(14, 41, 0, 0)

            ' Left dust: fade + move
            AnimationHelper.fade(PART_DustLeft, 0.0, 100, 50)
            AnimationHelper.translate_x(PART_DustLeft, -5, 180, 0, AnimationHelper.ease_out_fluent)
            AnimationHelper.translate_y(PART_DustLeft, -6, 180, 0, AnimationHelper.ease_out_fluent)

            ' Right dust: fade + move
            AnimationHelper.fade(PART_DustRight, 0.0, 100, 50)
            AnimationHelper.translate_x(PART_DustRight, 5, 180, 0, AnimationHelper.ease_out_fluent)
            AnimationHelper.translate_y(PART_DustRight, -6, 180, 0, AnimationHelper.ease_out_fluent)
        End Sub

        ' --- Error Animation ---
        Private Sub ErrorAnimation(ByVal is_error As Boolean)
            If PART_Error Is Nothing Then Return

            If is_error Then
                ' To error state
                AnimationHelper.color(New SolidColorBrush(Color.Parse("#1370f3")), Color.Parse("#ff4c4c"), 300)

                AnimationHelper.delayed_code(Sub()
                                                 AnimationHelper.fade(PART_Error, 1.0, 100, 0)
                                                 AnimationHelper.scale_to(PART_Error, 1.0, 400, 0, AnimationHelper.ease_out_back)
                                             End Sub, 300)
            Else
                ' From error state
                AnimationHelper.fade(PART_Error, 0.0, 100)
                AnimationHelper.scale_to(PART_Error, 0.5, 200)
                AnimationHelper.color(New SolidColorBrush(Color.Parse("#ff4c4c")), Color.Parse("#1370f3"), 300)
            End If
        End Sub
    End Class

End Namespace
