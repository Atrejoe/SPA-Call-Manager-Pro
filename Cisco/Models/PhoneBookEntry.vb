Namespace Models
    ''' <summary>
    ''' A basic phonebook entry
    ''' </summary>
    Public Class PhoneBookEntry
        Implements IEquatable(Of PhoneBookEntry)
        
        Public Property FirstName As String
        Public Property Surname As String
        Public Property Number As String

        Public ReadOnly Property FullName As String
            Get
                Return String.Format("{0} {1}", FirstName, Surname).Trim()
            End Get
        End Property

        Public Shadows Function Equals(other As PhoneBookEntry) As Boolean Implements IEquatable(Of PhoneBookEntry).Equals
            Return Number.Equals(other.Number) _
               AndAlso FullName.Equals(other.FullName)
        End Function

        Public Overrides Function GetHashCode() As Integer
            Return String.Format("{0}_{1}", FullName, Number).GetHashCode()
        End Function
    End Class
End Namespace