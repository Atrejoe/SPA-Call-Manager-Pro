Imports System.Drawing.Drawing2D
Imports Pss.Cisco.Models

Module ModMain
    Public MyPhoneStatus As ClsPhone.sPhoneStatus 'status of a call
    Public MyPhoneSettings As Settings 'structure of phone settings
    Public MyStoredPhoneSettings As Settings 'structure of stored settings
    Public OutsideLinePrefix As String = "9"
    Public ReadOnly MyPhoneBook As New List(Of PhoneBookEntry) 'structure of the phone book
    'Public ReadOnly MySharedPhoneBook As New List(Of PhoneBookEntry) 'structure of the shared phone book
    Public DataDir As String 'holds the file path to where the phonedata is held
    'Public SharedDataDir As String = "\\phoenix-s1\videos\" 'holds the file path to where the shared phonedata is held
    Public LoginPassword As String = ""


    Public Sub DrawRoundRectForm(ByVal thisform As Form, ByVal x As Single, ByVal y As Single, ByVal width As Single, ByVal height As Single, ByVal radius As Single)

        'draws a borderless form with curved edges
        Dim gp As GraphicsPath = New GraphicsPath()

        gp.AddLine(x + radius, y, x + width - (radius * 2), y)
        gp.AddArc(x + width - (radius * 2), y, radius * 2, radius * 2, 270, 90)
        gp.AddLine(x + width, y + radius, x + width, y + height - (radius * 2))
        gp.AddArc(x + width - (radius * 2), y + height - (radius * 2), radius * 2, radius * 2, 0, 90)
        gp.AddLine(x + width - (radius * 2), y + height, x + radius, y + height)
        gp.AddArc(x, y + height - (radius * 2), radius * 2, radius * 2, 90, 90)
        gp.AddLine(x, y + height - (radius * 2), x, y + radius)
        gp.AddArc(x, y, radius * 2, radius * 2, 180, 90)
        gp.CloseFigure()
        Thisform.Region = New Region(gp)
        gp.Dispose()
    End Sub

    Public Sub DrawRoundRectControl(ByVal thisControl As Control, ByVal x As Single, ByVal y As Single, ByVal width As Single, ByVal height As Single, ByVal radius As Single)

        'draws a control with curved edges
        Dim gp As GraphicsPath = New GraphicsPath()

        gp.AddLine(x + radius, y, x + width - (radius * 2), y)
        gp.AddArc(x + width - (radius * 2), y, radius * 2, radius * 2, 270, 90)
        gp.AddLine(x + width, y + radius, x + width, y + height - (radius * 2))
        gp.AddArc(x + width - (radius * 2), y + height - (radius * 2), radius * 2, radius * 2, 0, 90)
        gp.AddLine(x + width - (radius * 2), y + height, x + radius, y + height)
        gp.AddArc(x, y + height - (radius * 2), radius * 2, radius * 2, 90, 90)
        gp.AddLine(x, y + height - (radius * 2), x, y + radius)
        gp.AddArc(x, y, radius * 2, radius * 2, 180, 90)
        gp.CloseFigure()
        ThisControl.Region = New Region(gp)
        gp.Dispose()
    End Sub

    Public Sub DrawRoundRectFill(ByVal g As Graphics, ByVal b As Brush, ByVal x As Integer, ByVal y As Integer, ByVal width As Integer, ByVal height As Integer, ByVal radius As Single)

        ' fills object with color

        Dim gp As GraphicsPath = New GraphicsPath

        gp.AddLine(x + radius, y, x + width - (radius * 2), y)
        gp.AddArc(x + width - (radius * 2), y, radius * 2, radius * 2, 270, 90)
        gp.AddLine(x + width, y + radius, x + width, y + height - (radius * 2))
        gp.AddArc(x + width - (radius * 2), y + height - (radius * 2), radius * 2, radius * 2, 0, 90)
        gp.AddLine(x + width - (radius * 2), y + height, x + radius, y + height)
        gp.AddArc(x, y + height - (radius * 2), radius * 2, radius * 2, 90, 90)
        gp.AddLine(x, y + height - (radius * 2), x, y + radius)
        gp.AddArc(x, y, radius * 2, radius * 2, 180, 90)
        gp.CloseFigure()

        gp.CloseFigure()
        g.FillPath(b, gp)
        gp.Dispose()
    End Sub

    Public Sub PaintGradient(ByVal sender As Control, ByVal e As PaintEventArgs, ByVal color1 As Color, ByVal color2 As Color)

        'paints a gradient onto the background  of the sender object
        Dim a As New LinearGradientBrush(New RectangleF(0, 0, sender.Width, sender.Height), color1, color2, LinearGradientMode.Vertical)
        Dim gg As Graphics = e.Graphics
        gg.FillRectangle(a, New RectangleF(0, 0, sender.Width, sender.Height))
    End Sub

    Public Sub LoadPhoneBook(ByVal filename As String)

        If IO.File.Exists(filename) Then
            'loads the phone book fron 'filenname'
            Dim tempPhoneBook As New List(Of PhoneBookEntry)
            ' Reader to read from the file

            Try
                Using sr As New IO.StreamReader(filename)
                    Dim tmp() As String
                    ' Hold the amount of lines already read in a 'counter-variable'
                    
                    Do While sr.Peek <> -1 ' Is -1 when no data exists on the next line of the CSV file

                        tmp = sr.ReadLine.Split(",".ToCharArray())

                        Dim entry = New PhoneBookEntry
                        With entry
                            .FirstName = tmp(0).Trim()
                            .Surname = tmp(1).Trim()
                            .Number = tmp(2).Trim()
                        End With

                        tempPhoneBook.Add(entry)
                        
                    Loop
                End Using

            Catch ex As Exception
            End Try


            Try
                MyPhoneBook.Clear()
                MyPhoneBook.AddRange(tempPhoneBook.OrderBy(Function(sPhoneBook) sPhoneBook.Surname))

                FrmMain.DgvPersonal.Rows.Clear()

                For Each entry In MyPhoneBook

                    Dim x = FrmMain.DgvPersonal.Rows.Add()
                    FrmMain.DgvPersonal.Rows(x).Cells(0).Value = x + 1
                    FrmMain.DgvPersonal.Rows(x).Cells(1).Value = entry.FullName
                    FrmMain.DgvPersonal.Rows(x).Cells(2).Value = entry.Number
                    FrmMain.DgvPersonal.Rows(x).Cells(3).Value = "Call"

                Next
            Catch ex As Exception

            End Try

        End If
    End Sub

    Public Sub LoadSharedPhoneBook(ByVal filename As String)

        Try
            If IO.File.Exists(filename) Then
                'loads the phone book fron 'filenname'
                Dim tempPhoneBook = New List(Of PhoneBookEntry)
                ' Reader to read from the file
                Using sr As New IO.StreamReader(filename)
                    Dim tmp() As String

                    Do While sr.Peek <> -1 ' Is -1 when no data exists on the next line of the CSV file

                        tmp = sr.ReadLine.Split(",".ToCharArray())

                        Dim entry = New PhoneBookEntry()
                        With entry
                            .FirstName = tmp(0).Trim
                            .Surname =tmp(1).Trim
                            .Number = tmp(2).Trim
                        End With
                        tempPhoneBook.Add(entry)
                    Loop
                End Using

                FrmMain.SharedContactsDataSource.Clear()

                
                tempPhoneBook.ForEach(Function(x) (FrmMain.SharedContactsDataSource.Add(x)))

            End If
        Catch ex As Exception

        End Try
    End Sub

    Public Sub SavePhoneBook(ByVal filename As String)
        'saves the phone book to 'filenname'

        Try
            Using outFile = My.Computer.FileSystem.OpenTextFileWriter(filename, False)
                For Each entry In MyPhoneBook
                    If entry.FirstName <> "" And entry.Number <> "" Then
                        outFile.WriteLine(entry.FirstName & "," & entry.Surname & "," & entry.Number)
                    End If
                Next
            End Using

        Catch ex As Exception

        End Try
    End Sub

    Public Sub SaveSharedPhoneBook(ByVal filename As String)
        'saves the phone book to 'filenname'

        Try
            Using outFile = My.Computer.FileSystem.OpenTextFileWriter(filename, False)
                For Each entry As PhoneBookEntry In FrmMain.SharedContactsDataSource
                    If entry.FullName <> "" AndAlso entry.Number <> "" Then
                        outFile.WriteLine(entry.FirstName & "," & entry.Surname & "," & entry.Number)
                    End If
                Next
            End Using
        Catch ex As Exception
        End Try
    End Sub

    Public Function GetStoredSettings() As Settings 'retreives the settings frm the registry

        'gets the stored settings from the reghistry
        Dim storedPhoneSettings As Settings = Nothing

        Try
            storedPhoneSettings.LocalIP = CType(My.Computer.Registry.GetValue("HKEY_CURRENT_USER\Software\PssLinksys\Phone", "LocalIP", ""), String)
            storedPhoneSettings.LocalPort = CType(My.Computer.Registry.GetValue("HKEY_CURRENT_USER\Software\PssLinksys\Phone", "LocalPort", 514), Integer)
            storedPhoneSettings.CTI_Enable = CType(My.Computer.Registry.GetValue("HKEY_CURRENT_USER\Software\PssLinksys\Phone", "CTI_Enable", ""), String)
            storedPhoneSettings.Debug_Server_Address = CType(My.Computer.Registry.GetValue("HKEY_CURRENT_USER\Software\PssLinksys\Phone", "Debug_Server_Address", ""), String)
            storedPhoneSettings.DebugLevel = CType(My.Computer.Registry.GetValue("HKEY_CURRENT_USER\Software\PssLinksys\Phone", "DebugLevel", ""), String)
            storedPhoneSettings.PhoneModel = CType(My.Computer.Registry.GetValue("HKEY_CURRENT_USER\Software\PssLinksys\Phone", "PhoneModel", ""), String)
            storedPhoneSettings.PhoneSoftwareVersion = CType(My.Computer.Registry.GetValue("HKEY_CURRENT_USER\Software\PssLinksys\Phone", "PhoneSoftwareVersion", ""), String)
            storedPhoneSettings.StationName = CType(My.Computer.Registry.GetValue("HKEY_CURRENT_USER\Software\PssLinksys\Phone", "StationName", ""), String)
            storedPhoneSettings.PhoneIP = CType(My.Computer.Registry.GetValue("HKEY_CURRENT_USER\Software\PssLinksys\Phone", "PhoneIP", ""), String)
            storedPhoneSettings.PhonePort = CType(My.Computer.Registry.GetValue("HKEY_CURRENT_USER\Software\PssLinksys\Phone", "PhonePort", 5060), Integer)
            storedPhoneSettings.username = "admin"
            storedPhoneSettings.password = CType(My.Computer.Registry.GetValue("HKEY_CURRENT_USER\Software\PssLinksys\Phone", "password", ""), String)
            storedPhoneSettings.sharedDataDir = CType(My.Computer.Registry.GetValue("HKEY_CURRENT_USER\Software\PssLinksys\SharedPhoneDir", "Path", ""), String)
        Catch ex As Exception

        End Try

        Return storedPhoneSettings
    End Function

    Public Function SetStoredSettings(ByVal storedPhoneSettings As Settings) As Boolean 'saves the settings frm the registry

        'saves the stored settings to the reghistry

        My.Computer.Registry.SetValue("HKEY_CURRENT_USER\Software\PssLinksys\Phone", "LocalIP", StoredPhoneSettings.LocalIP)
        My.Computer.Registry.SetValue("HKEY_CURRENT_USER\Software\PssLinksys\Phone", "LocalPort", StoredPhoneSettings.LocalPort)
        My.Computer.Registry.SetValue("HKEY_CURRENT_USER\Software\PssLinksys\Phone", "CTI_Enable", StoredPhoneSettings.CTI_Enable)
        My.Computer.Registry.SetValue("HKEY_CURRENT_USER\Software\PssLinksys\Phone", "Debug_Server_Address", StoredPhoneSettings.Debug_Server_Address)
        My.Computer.Registry.SetValue("HKEY_CURRENT_USER\Software\PssLinksys\Phone", "DebugLevel", StoredPhoneSettings.DebugLevel)
        My.Computer.Registry.SetValue("HKEY_CURRENT_USER\Software\PssLinksys\Phone", "PhoneModel", StoredPhoneSettings.PhoneModel)
        My.Computer.Registry.SetValue("HKEY_CURRENT_USER\Software\PssLinksys\Phone", "PhoneSoftwareVersion", StoredPhoneSettings.PhoneSoftwareVersion)
        My.Computer.Registry.SetValue("HKEY_CURRENT_USER\Software\PssLinksys\Phone", "StationName", StoredPhoneSettings.StationName)
        My.Computer.Registry.SetValue("HKEY_CURRENT_USER\Software\PssLinksys\Phone", "PhoneIP", StoredPhoneSettings.PhoneIP)
        My.Computer.Registry.SetValue("HKEY_CURRENT_USER\Software\PssLinksys\Phone", "PhonePort", StoredPhoneSettings.PhonePort)
        My.Computer.Registry.SetValue("HKEY_CURRENT_USER\Software\PssLinksys\SharedPhoneDir", "Path", FrmSetup.TxtSharedFolder.Text)

        Return True
    End Function

    Public Function RemoveLineDetailsFromNumber(number As String) As String

        If number.Contains("[") Then
            Dim endIndex As Integer = number.IndexOf("]", StringComparison.Ordinal) + 1
            Return number.Substring(endIndex)
        Else
            Return number
        End If
    End Function
End Module