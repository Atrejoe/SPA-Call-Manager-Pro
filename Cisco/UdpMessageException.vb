Public Class UdpMessageException
    Inherits System.Exception
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
End Class