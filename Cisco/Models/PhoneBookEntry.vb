Namespace Models
    ''' <summary>
    ''' A basic phonebook entry
    ''' </summary>
    Public Class PhoneBookEntry

        Public FirstName As String
        Public Surname As String
        Public Number As String

        Public ReadOnly Property FullName As String
            Get
                Return String.Format("{0} {1}", FirstName, Surname).Trim()
            End Get
        End Property

    End Class
End Namespace