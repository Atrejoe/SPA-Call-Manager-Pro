Imports System.Net.NetworkInformation

''' <summary>
''' Simple network utilities
''' </summary>
Public Module NetworkUtils
    ''' <summary>
    ''' Pings the handset, as setup in <see cref="Settings.PhoneIP"/>
    ''' </summary>
    ''' <param name="exception">Optional exception, will be specified when pinging not only was unsuccessfull, be threw an exception too.</param>
    ''' <returns><c>true</c> when ping was successfull, otherwise <c>false</c>.</returns>
    Friend Function PingHandset(Optional ByRef exception = Nothing) As Boolean
        Dim result = False
        Try
            result = My.Computer.Network.Ping(MyStoredPhoneSettings.PhoneIP)
        Catch ex As PingException
            exception = ex
        End Try
        Return result
    End Function
End Module