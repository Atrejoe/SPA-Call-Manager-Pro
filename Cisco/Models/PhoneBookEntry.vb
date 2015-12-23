Namespace Models
    ''' <summary>
    ''' A basic phonebook entry
    ''' </summary>
    Public Class PhoneBookEntry

        Public Property FirstName As String
        Public Property Surname As String
        Public Property Number As String

        Public ReadOnly Property FullName As String
            Get
                Return String.Format("{0} {1}", FirstName, Surname).Trim()
            End Get
        End Property

    End Class
End Namespace