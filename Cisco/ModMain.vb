﻿Imports System.Drawing.Drawing2D
Imports Cisco.Utilities
Imports Pss.Cisco.Models

Module ModMain
    Public MyPhoneStatus As ClsPhone.sPhoneStatus 'status of a call
    Public MyPhoneSettings As Settings 'structure of phone settings
    Public MyStoredPhoneSettings As Settings 'structure of stored settings
    Public OutsideLinePrefix As String = "9"
    Public ReadOnly MyPhoneBook As New SortableBindingList(Of PhoneBookEntry)(New List(Of PhoneBookEntry))
    Friend ReadOnly MySharedPhoneBook As New SortableBindingList(Of PhoneBookEntry)(New List(Of PhoneBookEntry))
    Public DataDir As String 'holds the file path to where the phonedata is held
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
        thisform.Region = New Region(gp)
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
        thisControl.Region = New Region(gp)
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
                ex.Log()
            End Try

            MyPhoneBook.Clear()
            For Each entry In tempPhoneBook.OrderBy(Function(sPhoneBook) sPhoneBook.Surname)
                MyPhoneBook.Add(entry)
            Next

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
                            .Surname = tmp(1).Trim
                            .Number = tmp(2).Trim
                        End With
                        tempPhoneBook.Add(entry)
                    Loop
                End Using

                MySharedPhoneBook.Clear()

                For Each entry In tempPhoneBook
                    MySharedPhoneBook.Add(entry)
                Next

                'tempPhoneBook.ForEach(Function(x) (FrmMain.MySharedPhoneBook.Add(x)))

            End If
        Catch ex As Exception
            ex.Log()
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
            ex.Log()
        End Try
    End Sub

    Public Sub SaveSharedPhoneBook(ByVal filename As String)
        'saves the phone book to 'filenname'

        Try
            Using outFile = My.Computer.FileSystem.OpenTextFileWriter(filename, False)
                For Each entry As PhoneBookEntry In MySharedPhoneBook
                    If entry.FullName <> "" AndAlso entry.Number <> "" Then
                        outFile.WriteLine(entry.FirstName & "," & entry.Surname & "," & entry.Number)
                    End If
                Next
            End Using
        Catch ex As Exception
            ex.Log()
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
            ex.Log()
        End Try

        Return storedPhoneSettings
    End Function

    Public Function SetStoredSettings(ByVal storedPhoneSettings As Settings) As Boolean 'saves the settings frm the registry

        'saves the stored settings to the reghistry

        My.Computer.Registry.SetValue("HKEY_CURRENT_USER\Software\PssLinksys\Phone", "LocalIP", storedPhoneSettings.LocalIP)
        My.Computer.Registry.SetValue("HKEY_CURRENT_USER\Software\PssLinksys\Phone", "LocalPort", storedPhoneSettings.LocalPort)
        My.Computer.Registry.SetValue("HKEY_CURRENT_USER\Software\PssLinksys\Phone", "CTI_Enable", storedPhoneSettings.CTI_Enable)
        My.Computer.Registry.SetValue("HKEY_CURRENT_USER\Software\PssLinksys\Phone", "Debug_Server_Address", storedPhoneSettings.Debug_Server_Address)
        My.Computer.Registry.SetValue("HKEY_CURRENT_USER\Software\PssLinksys\Phone", "DebugLevel", storedPhoneSettings.DebugLevel)
        My.Computer.Registry.SetValue("HKEY_CURRENT_USER\Software\PssLinksys\Phone", "PhoneModel", storedPhoneSettings.PhoneModel)
        My.Computer.Registry.SetValue("HKEY_CURRENT_USER\Software\PssLinksys\Phone", "PhoneSoftwareVersion", storedPhoneSettings.PhoneSoftwareVersion)
        My.Computer.Registry.SetValue("HKEY_CURRENT_USER\Software\PssLinksys\Phone", "StationName", storedPhoneSettings.StationName)
        My.Computer.Registry.SetValue("HKEY_CURRENT_USER\Software\PssLinksys\Phone", "PhoneIP", storedPhoneSettings.PhoneIP)
        My.Computer.Registry.SetValue("HKEY_CURRENT_USER\Software\PssLinksys\Phone", "PhonePort", storedPhoneSettings.PhonePort)
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