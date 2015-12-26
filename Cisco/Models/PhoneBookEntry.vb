Namespace Models
    ''' <summary>
    ''' A basic phonebook entry
    ''' </summary>
    <DebuggerDisplay("{DisplayName} ({Number})")>
    Public Class PhoneBookEntry
        Implements IEquatable(Of PhoneBookEntry)
        
        Public Property FirstName As String
        Public Property Surname As String
        Public Property Number As String

        Public ReadOnly Property DisplayName As String
            Get
                Dim result = String.Format("{0} {1}", FirstName, Surname).Trim()
                If string.IsNullOrWhiteSpace(result)
                    result = Number
                End If
                return result
            End Get
        End Property

        Public Shadows Function Equals(other As PhoneBookEntry) As Boolean Implements IEquatable(Of PhoneBookEntry).Equals
            Return Number.Equals(other.Number) _
               AndAlso DisplayName.Equals(other.DisplayName)
        End Function

        Public Overrides Function GetHashCode() As Integer
            Return String.Format("{0}_{1}", DisplayName, Number).GetHashCode()
        End Function

        Public Overrides Function ToString() As String
            Return DisplayName
        End Function
    End Class
End Namespace