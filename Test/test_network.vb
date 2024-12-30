
Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Net
Imports System.Threading.Tasks
Imports System.Threading.Thread
Imports System.Collections.Concurrent
Imports NUnit.Framework


<TestFixture>
Public Class test_network

	<Test>
	Public Sub entry_point()
		' 多线程下载示例
		Dim urls = New ConcurrentQueue(Of Integer)
		urls.Enqueue(1)
		urls.Enqueue(2)
		urls.Enqueue(3)

		Dim url As Integer
		While urls.TryDequeue(url)
			Console.WriteLine(url & " " & CurrentThread.ManagedThreadId)
		End While

	End Sub

	<Test>
	Public Sub multi_task_test()
		' 定义并启动多个任务来并发处理队列
		Dim taskCount As Integer = 8

		' 执行异步下载
		' 创建 ConcurrentQueue 并添加一些初始数据
		Dim queue As New ConcurrentQueue(Of String)()
		For i As Integer = 1 To 1000
			queue.Enqueue("Item " & i)
		Next

		Dim tasks(taskCount) As Task
		' 创建并启动消费队列的任务
		For t = 0 To taskCount
			tasks(t) = Task.Run(Function() ProcessQueue(queue))
		Next

		' 等待消费任务完成
		Task.WaitAll(tasks)

		Console.WriteLine("Queue has been cleared. Program ends.")
	End Sub

	Async Function ProcessQueue(ByVal queue As ConcurrentQueue(Of String)) As Task
		Dim value As String = ""

		' 持续尝试取值，直到队列为空
		While Not queue.IsEmpty
			If queue.TryDequeue(value) Then
				Console.WriteLine("Dequeued: " & value & " " & CurrentThread.ManagedThreadId)
			Else
				Console.WriteLine("Queue is empty, retrying...")
			End If
		End While
		Await Task.Delay(1)
		Console.WriteLine("Queue processing is complete.")
	End Function
End Class


