
Option Explicit On
Option Strict On

Imports System
Imports System.IO
Imports System.Threading.Tasks

Namespace Utility.Interface

    Public Interface Download
        Function save_web_stream(ByVal url As String, ByVal pth As String) As Task
    End Interface
End Namespace


