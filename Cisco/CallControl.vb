﻿Public Class CallControl

    Public Enum eAction
        Hold = 1
        [Resume] = 2
        [End] = 3
        Answer = 4
        Reject = 5
        Divert = 6
        Dial = 7
    End Enum

    Public Function ConstructHeaderMessage(ByVal PhoneStatus As ClsPhone.sPhoneStatus, ByVal PhoneSettings As ClsPhone.sPhoneSettings) As String

        Dim SPATemplate As String = "NOTIFY sip:" & PhoneSettings.StationName & "@" & PhoneSettings.PhoneIP & ":" & PhoneSettings.PhonePort & " SIP/2.0" & vbCr & _
                      "Via: SIP/2.0/UDP " & PhoneSettings.LocalIP & ":" & PhoneSettings.LocalPort & vbCr & _
                      "Max-Forwards: 70" & vbCr & _
                      "From: <sip:" & Environment.MachineName & "@" & PhoneSettings.LocalIP & ">;tag=1710726934" & vbCr & _
                      "To: <sip:" & PhoneSettings.StationName & "@" & PhoneSettings.PhoneIP & ">" & vbCr & _
                      "Call-ID: " & GetRandom(100000, 999999) & "@" & PhoneSettings.LocalIP & vbCr & _
                      "CSeq: 1 NOTIFY" & vbCr & _
                      "Contact: <sip:" & Environment.MachineName & "@" & PhoneSettings.LocalIP & ":" & PhoneSettings.LocalPort & ">" & vbCr & _
                      "Content-Type: application/x-spa-control" & vbCr & _
                      "Event: x-spa-cti" & vbCr & vbCr
        Return SPATemplate

    End Function

    Public Function PhoneAction(ByVal Action As CallControl.eAction, ByVal PhoneStatus As ClsPhone.sPhoneStatus, ByVal PhoneSettings As ClsPhone.sPhoneSettings) As String

        Dim SPACommand As String
        Dim qt As String = Chr(34)
        SPACommand = ConstructHeaderMessage(PhoneStatus, PhoneSettings)


        Select Case Action
            Case eAction.Hold
                SPACommand = SPACommand & "<spa-control>" & vbCr & "<hold />" & vbCr & "</spa-control>"
            Case eAction.Resume
                SPACommand = SPACommand & "<spa-control>" & vbCr & "<resume call=""" & PhoneStatus.Id & """/>" & vbCr & "</spa-control>"
            Case eAction.End
                SPACommand = SPACommand & "<spa-control>" & vbCr & "<endcall call=""" & PhoneStatus.Id & """/>" & vbCr & "</spa-control>"
            Case eAction.Answer
                SPACommand = SPACommand & "<spa-control>" & vbCr & "<answer call=""" & PhoneStatus.Id & """/>" & vbCr & "</spa-control>"
            Case eAction.Reject
                SPACommand = SPACommand & "<spa-control>" & vbCr & "<reject call=""" & PhoneStatus.Id & """/>" & vbCr & "</spa-control>"
            Case eAction.Divert
                SPACommand = SPACommand & "<spa-control>" & vbCr & "<reject uri=""" & PhoneStatus.CallerNumber & "@$PROXY"" call=""" & PhoneStatus.Id & """ reason=""redir""/>" & vbCr & "</spa-control>"
            Case eAction.Dial
                SPACommand = SPACommand & "<spa-control>" & vbCr & "<newcall uri=" & qt & PhoneStatus.CallerNumber & "@$proxy" & qt & " call=" & qt & PhoneStatus.Id & qt & " />" & vbCr & "</spa-control>"
        End Select


        Return SPACommand

    End Function

    Private Function GetRandom(ByVal Min As Integer, ByVal Max As Integer) As Integer

        Static Generator As System.Random = New System.Random()
        Return Generator.Next(Min, Max)

    End Function

    Public Function GetNumbers(phonenumber As String) As String

        Dim newNumber As String = ""

        For x As Integer = 0 To phonenumber.Length - 1
            If IsNumeric(phonenumber.Substring(x, 1)) = True Then
                newNumber = newNumber & phonenumber.Substring(x, 1)
            ElseIf phonenumber.Substring(x, 1) = "+" Then
                newNumber = newNumber & phonenumber.Substring(x, 1)
            End If
        Next

        Return newNumber

    End Function
End Class


