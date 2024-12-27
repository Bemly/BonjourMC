Imports NUnit.Framework

Namespace Test
    Public Class Tests
        <SetUp>
        Public Sub Setup()
        End Sub

        <Test>
        Public Sub Test1()
            Assert.Pass()
        End Sub

        <Test>
        Public Sub Test2()
            Assert.Fail()
        End Sub

        <Test>
        Public Sub Test3()
            Assert.Warn("Warn!")
        End Sub

    End Class
End Namespace