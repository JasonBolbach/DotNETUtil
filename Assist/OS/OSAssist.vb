''' <summary>
''' Simplify quick OS calls to start Processes, etc...
''' Implements Recursive File/Folder deletion with DelTree
''' </summary>
''' <remarks></remarks>
Public Class OSAssist
    Public Shared DebugEnabled As Boolean = False
    Public Shared Sub KillProcess(ProcessName As String)
        Dim KillProcess As New Process
        With KillProcess
            .StartInfo.FileName = "C:\Windows\System32\taskkill.exe"
            .StartInfo.Arguments = "/IM " + ProcessName + " /f"
            .StartInfo.CreateNoWindow = True
        End With
        Try
            KillProcess.Start()
            While Not KillProcess.HasExited
                Threading.Thread.Sleep(25)
            End While
        Catch ex As Exception
        End Try
    End Sub
    Public Shared Function RunProcess(ProcessName As String, Arguments As String, TimeOut As Integer) As String
        Using Proc As New Process
            With Proc
                .StartInfo.FileName = ProcessName
                .StartInfo.Arguments = Arguments
                .StartInfo.CreateNoWindow = True
            End With

            Try
                Proc.Start()
            Catch ex As Exception
                Throw New Exception("RunProcess:Failed:" + ex.Message, ex)
                Return False
            End Try

            If TimeOut > 0 Then
                Dim StartTime As DateTime = Now
                While Not Proc.HasExited
                    Threading.Thread.Sleep(25)
                    If (Now - StartTime).TotalMilliseconds > TimeOut Then
                        Return ""
                    End If
                End While
            ElseIf TimeOut = 0 Then
                While Not Proc.HasExited
                    Threading.Thread.Sleep(25)
                End While
            Else
            End If

            Return Proc.StandardOutput.ReadToEnd()
        End Using
    End Function
    Public Shared Sub DelTree(Path As String)
        Dim Files() As String = Nothing
        Try
            Files = IO.Directory.GetFiles(Path)
        Catch ex As Exception
        End Try

        Dim Directories() As String = Nothing
        Try
            Directories = IO.Directory.GetDirectories(Path)
        Catch ex As Exception
        End Try

        If Files IsNot Nothing Then
            For Each File In Files
                Try
                    IO.File.Delete(File)
                Catch ex As Exception
                    If DebugEnabled Then Debug.WriteLine("Error Deleting File {0}", File)
                End Try
            Next
        End If

        If Directories IsNot Nothing Then
            For Each Dir As String In Directories
                Try
                    Dim D As String = Path + IO.Path.DirectorySeparatorChar + Dir
                    DelTree(D)
                    Try
                        IO.Directory.Delete(D, True)
                    Catch ex As Exception
                        If DebugEnabled Then Debug.WriteLine("Error Deleting Dir {0}", Dir)
                    End Try
                Catch ex As Exception
                End Try
            Next
        End If
    End Sub
    Public Shared Sub Restart()
        RunProcess("C:\Windows\System32\shutdown.exe", "-r -f -t 0", 0)
    End Sub
End Class
