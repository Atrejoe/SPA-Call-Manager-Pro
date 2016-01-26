Imports Cisco.Utilities
Imports Cisco.Utilities.ClsPhone

Public Module CallControl

    Public Enum EAction
        Hold = 1
        [Resume] = 2
        [End] = 3
        Answer = 4
        Reject = 5
        Divert = 6
        Dial = 7
    End Enum

    Public Function ConstructHeaderMessage(phoneStatus As SPhoneStatus, phoneSettings As Settings) As String

        Dim spaTemplate As String = "NOTIFY sip:" & phoneSettings.StationName & "@" & phoneSettings.PhoneIP & ":" & phoneSettings.PhonePort & " SIP/2.0" & vbCr &
                      "Via: SIP/2.0/UDP " & phoneSettings.LocalIP & ":" & phoneSettings.LocalPort & vbCr &
                      "Max-Forwards: 70" & vbCr &
                      "From: <sip:" & Environment.MachineName & "@" & phoneSettings.LocalIP & ">;tag=1710726934" & vbCr &
                      "To: <sip:" & phoneSettings.StationName & "@" & phoneSettings.PhoneIP & ">" & vbCr &
                      "Call-ID: " & GetRandom(100000, 999999) & "@" & phoneSettings.LocalIP & vbCr &
                      "CSeq: 1 NOTIFY" & vbCr &
                      "Contact: <sip:" & Environment.MachineName & "@" & phoneSettings.LocalIP & ":" & phoneSettings.LocalPort & ">" & vbCr &
                      "Content-Type: application/x-spa-control" & vbCr &
                      "Event: x-spa-cti" & vbCr & vbCr
        Return spaTemplate

    End Function

    Public Function PhoneAction(action As EAction, phoneStatus As SPhoneStatus, phoneSettings As Settings) As String

        Dim spaCommand As String
        Dim qt As String = Chr(34)
        spaCommand = ConstructHeaderMessage(phoneStatus, phoneSettings)


        Select Case action
            Case EAction.Hold
                spaCommand = spaCommand & "<spa-control>" & vbCr & "<hold />" & vbCr & "</spa-control>"
            Case EAction.Resume
                spaCommand = spaCommand & "<spa-control>" & vbCr & "<resume call=""" & phoneStatus.Id & """/>" & vbCr & "</spa-control>"
            Case EAction.End
                spaCommand = spaCommand & "<spa-control>" & vbCr & "<endcall call=""" & phoneStatus.Id & """/>" & vbCr & "</spa-control>"
            Case EAction.Answer
                spaCommand = spaCommand & "<spa-control>" & vbCr & "<answer call=""" & phoneStatus.Id & """/>" & vbCr & "</spa-control>"
            Case EAction.Reject
                spaCommand = spaCommand & "<spa-control>" & vbCr & "<reject call=""" & phoneStatus.Id & """/>" & vbCr & "</spa-control>"
            Case EAction.Divert
                spaCommand = spaCommand & "<spa-control>" & vbCr & "<reject uri=""" & phoneStatus.CallerNumber & "@$PROXY"" call=""" & phoneStatus.Id & """ reason=""redir""/>" & vbCr & "</spa-control>"
            Case EAction.Dial
                spaCommand = spaCommand & "<spa-control>" & vbCr & "<newcall uri=" & qt & phoneStatus.CallerNumber & "@$proxy" & qt & " call=" & qt & phoneStatus.Id & qt & " />" & vbCr & "</spa-control>"
        End Select


        Return spaCommand

    End Function

    Private ReadOnly Generator As New Random()

    Private Function GetRandom(min As Integer, max As Integer) As Integer

        Return Generator.Next(min, max)

    End Function

    Public Function GetNumbers(phonenumber As String) As String

        Dim newNumber = ""

        For x = 0 To phonenumber.Length - 1
            If IsNumeric(phonenumber.Substring(x, 1)) = True Then
                newNumber = newNumber & phonenumber.Substring(x, 1)
            ElseIf phonenumber.Substring(x, 1) = "+" Then
                newNumber = newNumber & phonenumber.Substring(x, 1)
            End If
        Next

        Return newNumber

    End Function
End Module


