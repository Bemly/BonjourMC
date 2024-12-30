
Option Explicit On
Option Strict On

Imports System
Imports Launcher

Module Program
    Sub Main(ByVal args As String())
        Console.WriteLine($"BonjourMC! { GetType(Core).Namespace }")
        Entry.Point()
    End Sub
End Module
