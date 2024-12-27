Imports System
Imports Launcher

Module Program
    Sub Main(args As String())
        Console.WriteLine($"Hello World! { GetType(Core).Namespace }")
        Entry.Point()
    End Sub
End Module
