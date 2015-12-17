Imports System.Drawing.Drawing2D

Module ModMain

    Public Structure sPhoneBook
        Dim FirstName As String
        Dim Surname As String
        Dim Number As String
    End Structure

    Public MyPhoneStatus As ClsPhone.sPhoneStatus 'status of a call
    Public MyPhoneSettings As ClsPhone.sPhoneSettings 'structure of phone settings
    Public MyStoredPhoneSettings As ClsPhone.sPhoneSettings 'structure of stored settings
    Public OutsideLinePrefix As String = "9"
    Public MyPhoneBook() As sPhoneBook 'structure of the phone book
    Public MySharedPhoneBook() As sPhoneBook 'structure of the shared phone book
    Public dataDir As String 'holds the file path to where the phonedata is held
    Public SharedDataDir As String = "\\phoenix-s1\videos\" 'holds the file path to where the shared phonedata is held
    Public LoginPassword As String = ""


    Public Sub DrawRoundRectForm(ByVal Thisform As Form, ByVal x As Single, ByVal y As Single, ByVal width As Single, ByVal height As Single, ByVal radius As Single)

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

    Public Sub DrawRoundRectControl(ByVal ThisControl As Control, ByVal x As Single, ByVal y As Single, ByVal width As Single, ByVal height As Single, ByVal radius As Single)

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

    Public Sub drawRoundRectFill(ByVal g As Graphics, ByVal b As Brush, ByVal x As Integer, ByVal y As Integer, ByVal width As Integer, ByVal height As Integer, ByVal radius As Single)

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

    Public Sub PaintGradient(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs, ByVal color1 As Color, ByVal color2 As Color)

        'paints a gradient onto the background  of the sender object
        Dim a As New System.Drawing.Drawing2D.LinearGradientBrush(New RectangleF(0, 0, sender.Width, sender.Height), color1, color2, LinearGradientMode.Vertical)
        Dim gg As Graphics = e.Graphics
        gg.FillRectangle(a, New RectangleF(0, 0, sender.Width, sender.Height))
        a = Nothing
        gg = Nothing


    End Sub

    Public Sub LoadPhoneBook(ByVal filename As String)

        If System.IO.File.Exists(filename) Then
            'loads the phone book fron 'filenname'
            Dim TempPhoneBook() As sPhoneBook = Nothing
            ' Reader to read from the file
            Dim sr As New System.IO.StreamReader(filename)

            Try
                Dim tmp() As String
                ' Hold the amount of lines already read in a 'counter-variable'
                Dim counter As Integer = 0

                Do While sr.Peek <> -1 ' Is -1 when no data exists on the next line of the CSV file
                    ReDim Preserve TempPhoneBook(counter)
                    tmp = sr.ReadLine.Split(",")
                    TempPhoneBook(counter).FirstName = StrConv(tmp(0).Trim, VbStrConv.ProperCase)
                    TempPhoneBook(counter).Surname = StrConv(tmp(1).Trim, VbStrConv.ProperCase)
                    TempPhoneBook(counter).Number = StrConv(tmp(2).Trim, VbStrConv.ProperCase)
                    counter += 1
                Loop
                sr.Close()
                sr = Nothing
            Catch ex As Exception
                sr.Close()
                sr = Nothing
            End Try


            Try
                Dim PhoneBook As IEnumerable(Of sPhoneBook) = TempPhoneBook.OrderBy(Function(sPhoneBook) sPhoneBook.Surname)
                '  FrmMain.CmbNumber.Items.Clear()
                FrmMain.DgvPersonal.Rows.Clear()

                For x As Integer = 0 To PhoneBook.Count - 1
                    ReDim Preserve MyPhoneBook(x)
                    MyPhoneBook(x).FirstName = PhoneBook(x).FirstName
                    MyPhoneBook(x).Surname = PhoneBook(x).Surname
                    MyPhoneBook(x).Number = PhoneBook(x).Number
                    FrmMain.DgvPersonal.Rows.Add()
                    FrmMain.DgvPersonal.Rows(x).Cells(0).Value = x + 1
                    FrmMain.DgvPersonal.Rows(x).Cells(1).Value = PhoneBook(x).FirstName & " " & PhoneBook(x).Surname
                    FrmMain.DgvPersonal.Rows(x).Cells(2).Value = PhoneBook(x).Number
                    FrmMain.DgvPersonal.Rows(x).Cells(3).Value = "Call"
                    ' FrmMain.CmbNumber.Items.Add(PhoneBook(x).FirstName & " " & PhoneBook(x).Surname)
                Next
            Catch ex As Exception

            End Try

        End If


    End Sub

    Public Sub LoadSharedPhoneBook(ByVal filename As String)


        Try

        If System.IO.File.Exists(filename) Then
            'loads the phone book fron 'filenname'
            Dim TempPhoneBook() As sPhoneBook = Nothing
            ' Reader to read from the file
            Dim sr As New System.IO.StreamReader(filename)

            Try
                Dim tmp() As String
                ' Hold the amount of lines already read in a 'counter-variable'
                Dim counter As Integer = 0

                Do While sr.Peek <> -1 ' Is -1 when no data exists on the next line of the CSV file
                    ReDim Preserve TempPhoneBook(counter)
                    tmp = sr.ReadLine.Split(",")
                    TempPhoneBook(counter).FirstName = StrConv(tmp(0).Trim, VbStrConv.ProperCase)
                    TempPhoneBook(counter).Surname = StrConv(tmp(1).Trim, VbStrConv.ProperCase)
                    TempPhoneBook(counter).Number = StrConv(tmp(2).Trim, VbStrConv.ProperCase)
                    counter += 1
                Loop
                sr.Close()
                sr = Nothing
            Catch ex As Exception
                sr.Close()
                sr = Nothing
            End Try


            Try
                Dim PhoneBook As IEnumerable(Of sPhoneBook) = TempPhoneBook.OrderBy(Function(sPhoneBook) sPhoneBook.Surname)
                '  FrmMain.CmbNumber.Items.Clear()
                FrmMain.DGVSharedDir.Rows.Clear()

                For x As Integer = 0 To PhoneBook.Count - 1
                    ReDim Preserve MySharedPhoneBook(x)
                    MySharedPhoneBook(x).FirstName = PhoneBook(x).FirstName
                    MySharedPhoneBook(x).Surname = PhoneBook(x).Surname
                    MySharedPhoneBook(x).Number = PhoneBook(x).Number
                    FrmMain.DGVSharedDir.Rows.Add()
                    FrmMain.DGVSharedDir.Rows(x).Cells(0).Value = x + 1
                    FrmMain.DGVSharedDir.Rows(x).Cells(1).Value = PhoneBook(x).FirstName & " " & PhoneBook(x).Surname
                    FrmMain.DGVSharedDir.Rows(x).Cells(2).Value = PhoneBook(x).Number
                    FrmMain.DGVSharedDir.Rows(x).Cells(3).Value = "Call"
                    ' FrmMain.CmbNumber.Items.Add(PhoneBook(x).FirstName & " " & PhoneBook(x).Surname)
                Next
            Catch ex As Exception

            End Try
            End If
        Catch ex As Exception

        End Try



    End Sub

    Public Sub SavePhoneBook(ByVal filename As String)
        'saves the phone book fron 'filenname'
        Dim outFile As IO.StreamWriter = Nothing

        Try
            outFile = My.Computer.FileSystem.OpenTextFileWriter(filename, False)

            For x As Integer = 0 To MyPhoneBook.GetUpperBound(0)
                If MyPhoneBook(x).FirstName <> "" And MyPhoneBook(x).Number <> "" Then
                    outFile.WriteLine(MyPhoneBook(x).FirstName & "," & MyPhoneBook(x).Surname & "," & MyPhoneBook(x).Number)
                End If
            Next

            outFile.Close()
            outFile = Nothing

        Catch ex As Exception
            outFile.Close()
            outFile = Nothing
        End Try

    End Sub

    Public Sub SaveSharedPhoneBook(ByVal filename As String)
        'saves the phone book fron 'filenname'
        Dim outFile As IO.StreamWriter = Nothing

        Try
            outFile = My.Computer.FileSystem.OpenTextFileWriter(filename, False)

            For x As Integer = 0 To MySharedPhoneBook.GetUpperBound(0)
                If MySharedPhoneBook(x).FirstName <> "" And MySharedPhoneBook(x).Number <> "" Then
                    outFile.WriteLine(MySharedPhoneBook(x).FirstName & "," & MySharedPhoneBook(x).Surname & "," & MySharedPhoneBook(x).Number)
                End If
            Next

            outFile.Close()
            outFile = Nothing

        Catch ex As Exception
            outFile.Close()
            outFile = Nothing
        End Try

    End Sub

    Public Function GetStoredSettings() As ClsPhone.sPhoneSettings 'retreives the settings frm the registry

        'gets the stored settings from the reghistry
        Dim StoredPhoneSettings As ClsPhone.sPhoneSettings = Nothing

        Try
            StoredPhoneSettings.LocalIP = My.Computer.Registry.GetValue("HKEY_CURRENT_USER\Software\PssLinksys\Phone", "LocalIP", "")
            StoredPhoneSettings.LocalPort = My.Computer.Registry.GetValue("HKEY_CURRENT_USER\Software\PssLinksys\Phone", "LocalPort", 514)
            StoredPhoneSettings.CTI_Enable = My.Computer.Registry.GetValue("HKEY_CURRENT_USER\Software\PssLinksys\Phone", "CTI_Enable", "")
            StoredPhoneSettings.Debug_Server_Address = My.Computer.Registry.GetValue("HKEY_CURRENT_USER\Software\PssLinksys\Phone", "Debug_Server_Address", "")
            StoredPhoneSettings.DebugLevel = My.Computer.Registry.GetValue("HKEY_CURRENT_USER\Software\PssLinksys\Phone", "DebugLevel", "")
            StoredPhoneSettings.PhoneModel = My.Computer.Registry.GetValue("HKEY_CURRENT_USER\Software\PssLinksys\Phone", "PhoneModel", "")
            StoredPhoneSettings.PhoneSoftwareVersion = My.Computer.Registry.GetValue("HKEY_CURRENT_USER\Software\PssLinksys\Phone", "PhoneSoftwareVersion", "")
            StoredPhoneSettings.StationName = My.Computer.Registry.GetValue("HKEY_CURRENT_USER\Software\PssLinksys\Phone", "StationName", "")
            StoredPhoneSettings.PhoneIP = My.Computer.Registry.GetValue("HKEY_CURRENT_USER\Software\PssLinksys\Phone", "PhoneIP", "")
            StoredPhoneSettings.PhonePort = My.Computer.Registry.GetValue("HKEY_CURRENT_USER\Software\PssLinksys\Phone", "PhonePort", 5060)
            StoredPhoneSettings.username = "admin"
            StoredPhoneSettings.password = My.Computer.Registry.GetValue("HKEY_CURRENT_USER\Software\PssLinksys\Phone", "password", "")
            SharedDataDir = My.Computer.Registry.GetValue("HKEY_CURRENT_USER\Software\PssLinksys\SharedPhoneDir", "Path", "")
        Catch ex As Exception

        End Try

        Return StoredPhoneSettings

    End Function

    Public Function SetStoredSettings(ByVal StoredPhoneSettings As ClsPhone.sPhoneSettings) As Boolean 'saves the settings frm the registry

        'saves the stored settings to the reghistry

        On Error Resume Next

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
            Dim endIndex As Integer = number.IndexOf("]") + 1
            Return number.Substring(endIndex)
        Else
            Return number
        End If

    End Function

End Module

