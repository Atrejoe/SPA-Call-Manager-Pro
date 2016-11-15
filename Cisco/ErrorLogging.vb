Imports System.Runtime.CompilerServices
Imports RollbarSharp

''' <summary>
''' Simple exception logging, testing multiple exception logging frameworks just for kicks
''' </summary>
Public Module Er

    Private ReadOnly Property Environment As String
        Get
#If DEBUG Then
            Return "development"
#Else
            Return "release"
#End If
        End Get
    End Property

    Private ReadOnly RollbarClient As New Lazy(Of RollbarClient)(AddressOf GetRollbarClient)

    Private Function GetRollbarClient() As RollbarClient
        Dim result = New RollbarClient("ca971c37957e4899874dbf864f716501")
        With result.Configuration
            .Environment = Environment
            .CodeVersion = ApplicationVersion.Value
        End With
        Return result
    End Function

    Private ReadOnly ApplicationVersion As New Lazy(Of String)(Function()
                                                                   Dim assembly = System.Reflection.Assembly.GetExecutingAssembly()
                                                                   Dim fvi = FileVersionInfo.GetVersionInfo(assembly.Location)
                                                                   Return fvi.FileVersion
                                                               End Function)


    ''' <summary>
    ''' Logs the specified exception.
    ''' </summary>
    ''' <param name="exception">The exception to log.</param>
    ''' <param name="method">The method, referring to the caller.</param>
    ''' <param name="file">The file, referring to the caller.</param>
    ''' <param name="lineNumber">The line number, referring to the caller.</param>
    ''' <remarks>This should be made configurable and pluggable. For now these are test-logging recipient.</remarks>
    <Extension>
    Public Sub Log(exception As Exception,
                   <CallerMemberName> Optional method As String = Nothing,
                   <CallerFilePath> Optional file As String = Nothing,
                   <CallerLineNumber> Optional lineNumber As Integer = 0,
                   Optional informationOnly As Boolean = False)

        Trace.TraceWarning("Logging exception : {0}", exception)

        Dim anySuccess = False

        Try
            If informationOnly Then
                RollbarClient.Value.SendInfoMessage(exception.Message).Wait()
            Else
                RollbarClient.Value.SendException(exception).Wait()
            End If

            'Send to Rollbar
            anySuccess = True
        Catch ex As Exception
            Trace.TraceWarning("Logging to Rollbar failed : {0}", ex)
        End Try

        If Not anySuccess Then
            MsgBox(String.Format("Reporting an error failed. Please contact the developer regarding the error. Error details : {0}", exception), MsgBoxStyle.OkOnly, "Failed to report error")
        End If

    End Sub

End Module