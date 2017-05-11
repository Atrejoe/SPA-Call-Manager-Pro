Imports System.Runtime.CompilerServices

''' <summary>
''' Simple exception logging, testing multiple exception logging frameworks just for kicks
''' </summary>
Public Module ErrorLogging

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

        Dim anySuccess = Global.Cisco.Utilities.ErrorLogging.Log(exception, method, file, lineNumber)

        'Send to Rollbar
        If Not anySuccess Then
            MsgBox(String.Format("Reporting an error failed. Please contact the developer regarding the error. Error details : {0}", exception), MsgBoxStyle.OkOnly, "Failed to report error")
        End If

    End Sub

End Module