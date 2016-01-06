Imports System.Drawing.Drawing2D
Imports System.IO
Imports Pss.Cisco.Models

Public Class FrmPhoneBook
    Dim GridRowId As Integer = 0
    Dim NewGridName As String = ""

    Private Sub FrmPhoneBook_Paint(sender As Object, e As PaintEventArgs) Handles Me.Paint
        Me.PaintGradient(e.Graphics)
    End Sub

    Private Sub btnSavePersonal_Click(sender As Object, e As EventArgs) Handles btnSavePersonal.Click

        Try
            Dim entry = New PhoneBookEntry
            With entry
                .FirstName = StrConv(TxtFirstname.Text, VbStrConv.ProperCase)
                .Surname = StrConv(TxtSurname.Text, VbStrConv.ProperCase)
                .Number = txtNumber.Text.Replace(" ", "")
            End With

            If GridRowId = -1 Then
                MyPhoneBook.Add(entry)
            Else
                If NewGridName = "DgvPersonal" Then
                    MyPhoneBook(GridRowId) = entry
                Else
                    MyPhoneBook.Add(entry)
                End If
            End If

            SavePhoneBook(Path.Combine(DataDir & "CiscoPhone\Phonebook.csv"))
            LoadPhoneBook(Path.Combine(DataDir & "CiscoPhone\Phonebook.csv"))
            Me.Close()

        Catch ex As Exception
            ex.Log()
        End Try

    End Sub

    Public Sub New(entry As PhoneBookEntry, GridID As Integer, GridName As String)
        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        If (entry IsNot Nothing) Then
            TxtFirstname.Text = entry.FirstName
            TxtSurname.Text = entry.Surname
            txtNumber.Text = entry.Number
        End If

        GridRowId = GridID
        NewGridName = GridName
    End Sub

    Public Sub New(FirstName As String, Surname As String, Number As String, GridID As Integer, GridName As String)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        TxtFirstname.Text = FirstName
        TxtSurname.Text = Surname
        txtNumber.Text = Number
        GridRowId = GridID
        NewGridName = GridName

    End Sub


    Private Sub btnSavedShared_Click(sender As Object, e As EventArgs) Handles btnSavedShared.Click

        Try
            Dim entry = New PhoneBookEntry
            With entry
                .FirstName = StrConv(TxtFirstname.Text, VbStrConv.ProperCase)
                .Surname = StrConv(TxtSurname.Text, VbStrConv.ProperCase)
                .Number = txtNumber.Text.Replace(" ", "")
            End With

            If GridRowId = -1 Then
                MySharedPhoneBook.Add(entry)
            Else
                If NewGridName = "DGVSharedDir" Then
                    MySharedPhoneBook(GridRowId) = entry
                Else
                    MySharedPhoneBook.Add(entry)
                End If
            End If

            SaveSharedPhoneBook(Path.Combine(MyStoredPhoneSettings.sharedDataDir , "Phonebook.csv"))
            LoadSharedPhoneBook(Path.Combine(MyStoredPhoneSettings.sharedDataDir ,"Phonebook.csv"))
            Me.Close()

        Catch ex As Exception
            ex.Log()
        End Try

    End Sub
End Class