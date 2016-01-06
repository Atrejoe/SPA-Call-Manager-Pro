Imports System.Configuration
Imports System.Runtime.CompilerServices
Imports Cisco.Utilities
Imports Pss.Cisco.Models

Module ModMain
    Public MyPhoneStatus As SPhoneStatus 'status of a call
    Public MyPhoneSettings As Settings 'structure of phone settings
    Public MyStoredPhoneSettings As Settings 'structure of stored settings
    Public OutsideLinePrefix As String = "9"

    Friend ReadOnly MyPhoneBook, MySharedPhoneBook, PhoneDir, Dialled, Missed, Answered As New SortableBindingList(Of PhoneBookEntry)(New List(Of PhoneBookEntry))
    Friend ReadOnly CombinedPhoneBook As New List(Of PhoneBookEntry)

    Public DataDir As String 'holds the file path to where the phonedata is held
    Public LoginPassword As String = ""

    Public Sub LoadPhoneBook(filename As String)
        MyPhoneBook.Load(filename)
    End Sub

    Public Sub LoadSharedPhoneBook(filename As String)
        MySharedPhoneBook.Load(filename)
    End Sub

    <Extension>
    Private Sub Load(phoneBook As ICollection(Of PhoneBookEntry),filename As String)
        Try
            If IO.File.Exists(filename) Then
                'loads the phone book from 'filenname'
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

                phoneBook.Clear()
                For Each entry In tempPhoneBook.OrderBy(Function(sPhoneBook) sPhoneBook.Surname)
                    phoneBook.Add(entry)
                Next
            Else
                With New ConfigurationErrorsException(String.Format("Phonebook could not be found at '{0}'", filename))
                    .Log()
                End With
            End If
        Catch ex As Exception
            ex.Log()
        End Try
    End Sub


    Public Sub SavePhoneBook(filename As String)
        'saves the phone book to 'filename'
        MyPhoneBook.Save(filename)
    End Sub
    Public Sub SaveSharedPhoneBook(filename As String)
        'saves the phone book to 'filename'
        MySharedPhoneBook.Save(filename)
    End Sub

    <Extension>
    Private Sub Save(phonebook As IEnumerable(Of PhoneBookEntry), filename As String)
        'saves the phone book to 'filename'

        Try
            Using outFile = My.Computer.FileSystem.OpenTextFileWriter(filename, False)
                For Each entry In phonebook
                    With entry

                        If String.IsNullOrWhiteSpace(.FirstName) _
                                AndAlso String.IsNullOrWhiteSpace(.Surname) _
                                AndAlso String.IsNullOrWhiteSpace(.Number) Then Continue For

                        outFile.WriteLine(entry.FirstName & "," & entry.Surname & "," & entry.Number)

                    End With
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

    Public Function SetStoredSettings(storedPhoneSettings As Settings) As Boolean 'saves the settings frm the registry

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