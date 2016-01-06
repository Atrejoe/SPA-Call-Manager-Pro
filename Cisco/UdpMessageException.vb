Imports System.Runtime.Serialization

<Serializable>
Public Class UdpMessageException
    Inherits Exception
    Public Property UdpMessage As String

    Public Sub New(message As String, udpMessage As String)
        MyBase.New(message)
        Me.UdpMessage = udpMessage
    End Sub
    Public Sub New(message As String)
        MyBase.New(message)
    End Sub

    Public Sub New(message As String, inner As Exception)
        MyBase.New(message, inner)
    End Sub
    Public Sub New(message As String, udpMessage As String, inner As Exception)
        MyBase.New(message, inner)
        Me.UdpMessage = udpMessage
    End Sub

    Public Overrides Sub GetObjectData(info As SerializationInfo, context As StreamingContext)
        MyBase.GetObjectData(info, context)
    End Sub
End Class