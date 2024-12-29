
Option Explicit On
Option Strict On

Imports System


''' <summary>
''' 单例模式的Template，这里没有用接口(无法实现继承克隆操作)
''' 感觉和Shared没啥区别，随性用目前
''' source: https://www.cnblogs.com/terrylee/archive/2005/12/09/293509.html
''' CRTP source: https://en.wikipedia.org/wiki/Curiously_recurring_template_pattern
''' </summary>
Namespace Utility.Interface.Singleton

    ' 静态加载 依赖公共语言运行库初始化变量 支持懒加载
    Public NotInheritable Class Statical
        Shared ReadOnly m_instance As New Statical()

        Shared Sub New()
        End Sub

        Private Sub New()
        End Sub

        Public Shared ReadOnly Property Instance As Statical
            Get
                Return m_instance
            End Get
        End Property
    End Class

    ' 预加载 线程判断没有创建时加锁 DCL
    Public NotInheritable Class Preload
        Shared m_instance As Preload = Nothing
        Shared ReadOnly padlock As New Object()

        Private Sub New()
        End Sub

        Public Shared ReadOnly Property Instance As Preload
            Get
                If m_instance Is Nothing Then
                    SyncLock padlock
                        If m_instance Is Nothing Then
                            m_instance = New Preload()
                        End If
                    End SyncLock
                End If
                Return m_instance
            End Get
        End Property
    End Class

    ' 懒加载 内部类提供 未加锁(线程不安全)
    Public NotInheritable Class Lazy
        Private Sub New()
        End Sub

        Public Shared ReadOnly Property Instance As Lazy
            Get
                Return Nested.instance
            End Get
        End Property

        Private Class Nested
            Shared Sub New()
            End Sub

            Friend Shared ReadOnly instance As New Lazy()
        End Class
    End Class

End Namespace


