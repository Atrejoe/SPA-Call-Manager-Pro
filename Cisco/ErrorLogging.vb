Imports System.Runtime.CompilerServices
Imports SharpBrake

Public Module ErrorLogging
    <Extension>
    Public Sub Log(exception As Exception)
        exception.SendToAirBrake()
    End Sub
End Module