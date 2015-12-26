Imports System.Runtime.CompilerServices
Imports Airbraker
Imports Mindscape.Raygun4Net

''' <summary>
''' Simple exception logging, testing multiple exception logging frameworks just for kicks
''' </summary>
Public Module ErrorLogging

    Private ReadOnly RayGunClient As New RaygunClient("xhok2LdPYDOhsf8VieW1BA==")

    Private ReadOnly AirBrakeClient As New Lazy(Of AirbrakeClient)(AddressOf GetAirbrakeClient)

    Private Function GetAirbrakeClient() As AirbrakeClient
        Dim config = New AirbrakeConfig
        With config
            .ApiKey = "75d5016c879ec50262d884effb5fa368"
            .Environment = "development"
            .AppVersion = "1.0.0.5"
            .ProjectName = "SPA Call Manager Pro"
        End With

        Return New AirbrakeClient(config)
    End Function

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
            RayGunClient.SendInBackground(exception)

            anySuccess = True
        Catch ex As Exception
            Trace.TraceWarning("Logging to Raygun failed : {0}", ex)
        End Try

        If Not anySuccess Then
            MsgBox(String.Format("Reporting an error failed. Please contact the developer regarding the error. Error details : {0}", exception), MsgBoxStyle.OkOnly, "Failed to report error")
        End If

    End Sub
End Module