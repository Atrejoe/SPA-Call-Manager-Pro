Imports System.Runtime.CompilerServices
Imports Airbraker
Imports Mindscape.Raygun4Net
Imports RollbarSharp

''' <summary>
''' Simple exception logging, testing multiple exception logging frameworks just for kicks
''' </summary>
Public Module Er

    Private ReadOnly RayGunClient As New Lazy(Of RaygunClient)(AddressOf GetRayGunClient)

    Private Function GetRayGunClient() As RaygunClient
        Return New RaygunClient("xhok2LdPYDOhsf8VieW1BA==") With {.ApplicationVersion = ApplicationVersion.Value}
    End Function

    Private ReadOnly AirBrakeClient As New Lazy(Of AirbrakeClient)(AddressOf GetAirbrakeClient)

    Private Function GetAirbrakeClient() As AirbrakeClient
        Dim config = New AirbrakeConfig
        With config
            .ApiKey = "75d5016c879ec50262d884effb5fa368"
            .Environment = "development"
            .AppVersion = ApplicationVersion.Value
            .ProjectName = "SPA Call Manager Pro"
        End With

        Return New AirbrakeClient(config)
    End Function

    Private ReadOnly RollbarClient As New Lazy(Of RollbarClient)(AddressOf GetRollbarClient)

    Private Function GetRollbarClient() As RollbarClient
        Dim result =  New RollbarClient("ca971c37957e4899874dbf864f716501")
        With result.Configuration
            .Environment = "development"
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
                   <CallerLineNumber> Optional lineNumber As Integer = 0)

        Trace.TraceWarning("Logging exception : {0}", exception)

        Dim anySuccess = False
        Try
            'Send to Airbrake
            AirBrakeClient.Value.Send(exception, method, file, lineNumber)

            anySuccess = True
        Catch ex As Exception
            Trace.TraceWarning("Logging to AirBrake failed : {0}", ex)
        End Try

        Try
            'Send to Raygun
            RayGunClient.Value.SendInBackground(exception)

            anySuccess = True
        Catch ex As Exception
            Trace.TraceWarning("Logging to Raygun failed : {0}", ex)
        End Try

        Try
            'Send to Rollbar
            RollbarClient.Value.SendException(exception)

            anySuccess = True
        Catch ex As Exception
            Trace.TraceWarning("Logging to Rollbar failed : {0}", ex)
        End Try

        If Not anySuccess Then
            MsgBox(String.Format("Reporting an error failed. Please contact the developer regarding the error. Error details : {0}", exception), MsgBoxStyle.OkOnly, "Failed to report error")
        End If


    End Sub
End Module