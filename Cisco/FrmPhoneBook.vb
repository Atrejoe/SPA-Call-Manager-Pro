Imports System.Drawing.Drawing2D

Public Class FrmPhoneBook
    Dim GridRowId As Integer = 0
    Dim NewGridName As String = ""

    Private Sub FrmPhoneBook_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles Me.Paint

        On Error Resume Next
        If sender.width = 0 Then Exit Sub
        If sender.height = 0 Then Exit Sub

        Dim a As New LinearGradientBrush(New RectangleF(0, 0, Me.Width, Me.Height), Color.SlateGray, Color.Black, LinearGradientMode.Vertical)
        Dim gg As Graphics = e.Graphics
        gg.FillRectangle(a, New RectangleF(0, 0, Me.Width, Me.Height))
        a = Nothing
        gg = Nothing

    End Sub

    Private Sub btnSavePersonal_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnSavePersonal.Click

        Try
            If GridRowId = -1 Then
                If MyPhoneBook Is Nothing Then
                    ReDim Preserve MyPhoneBook(0)
                Else
                    ReDim Preserve MyPhoneBook(MyPhoneBook.GetUpperBound(0) + 1)
                End If
                MyPhoneBook(MyPhoneBook.GetUpperBound(0)).FirstName = StrConv(TxtFirstname.Text, VbStrConv.ProperCase)
                MyPhoneBook(MyPhoneBook.GetUpperBound(0)).Surname = StrConv(TxtSurname.Text, VbStrConv.ProperCase)
                MyPhoneBook(MyPhoneBook.GetUpperBound(0)).Number = txtNumber.Text.Replace(" ", "")
            Else
                If NewGridName = "DgvPersonal" Then
                    MyPhoneBook(GridRowId).FirstName = StrConv(TxtFirstname.Text, VbStrConv.ProperCase)
                    MyPhoneBook(GridRowId).Surname = StrConv(TxtSurname.Text, VbStrConv.ProperCase)
                    MyPhoneBook(GridRowId).Number = txtNumber.Text.Replace(" ", "")
                Else
                    ReDim Preserve MyPhoneBook(MyPhoneBook.GetUpperBound(0) + 1)
                    MyPhoneBook(MyPhoneBook.GetUpperBound(0)).FirstName = StrConv(TxtFirstname.Text, VbStrConv.ProperCase)
                    MyPhoneBook(MyPhoneBook.GetUpperBound(0)).Surname = StrConv(TxtSurname.Text, VbStrConv.ProperCase)
                    MyPhoneBook(MyPhoneBook.GetUpperBound(0)).Number = txtNumber.Text.Replace(" ", "")
                End If
            End If
            SavePhoneBook(dataDir & "\CiscoPhone\Phonebook.csv")
            LoadPhoneBook(dataDir & "\CiscoPhone\Phonebook.csv")
            Me.Close()

        Catch ex As Exception

        End Try

    End Sub

    Public Sub New(ByVal FirstName As String, ByVal Surname As String, ByVal Number As String, ByVal GridID As Integer, GridName As String)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        TxtFirstname.Text = FirstName
        TxtSurname.Text = Surname
        txtNumber.Text = Number
        GridRowId = GridID
        NewGridName = GridName

    End Sub

   
    Private Sub btnSavedShared_Click(sender As System.Object, e As System.EventArgs) Handles btnSavedShared.Click

        Try
            If GridRowId = -1 Then
                If MySharedPhoneBook Is Nothing Then
                    ReDim Preserve MySharedPhoneBook(0)
                Else
                    ReDim Preserve MySharedPhoneBook(MySharedPhoneBook.GetUpperBound(0) + 1)
                End If
                MySharedPhoneBook(MySharedPhoneBook.GetUpperBound(0)).FirstName = StrConv(TxtFirstname.Text, VbStrConv.ProperCase)
                MySharedPhoneBook(MySharedPhoneBook.GetUpperBound(0)).Surname = StrConv(TxtSurname.Text, VbStrConv.ProperCase)
                MySharedPhoneBook(MySharedPhoneBook.GetUpperBound(0)).Number = txtNumber.Text.Replace(" ", "")
            Else
                If NewGridName = "DGVSharedDir" Then
                    MySharedPhoneBook(GridRowId).FirstName = StrConv(TxtFirstname.Text, VbStrConv.ProperCase)
                    MySharedPhoneBook(GridRowId).Surname = StrConv(TxtSurname.Text, VbStrConv.ProperCase)
                    MySharedPhoneBook(GridRowId).Number = txtNumber.Text.Replace(" ", "")
                Else
                    ReDim Preserve MySharedPhoneBook(MySharedPhoneBook.GetUpperBound(0) + 1)
                    MySharedPhoneBook(MySharedPhoneBook.GetUpperBound(0)).FirstName = StrConv(TxtFirstname.Text, VbStrConv.ProperCase)
                    MySharedPhoneBook(MySharedPhoneBook.GetUpperBound(0)).Surname = StrConv(TxtSurname.Text, VbStrConv.ProperCase)
                    MySharedPhoneBook(MySharedPhoneBook.GetUpperBound(0)).Number = txtNumber.Text.Replace(" ", "")
                End If
            End If
            SaveSharedPhoneBook(SharedDataDir & "Phonebook.csv")
            LoadSharedPhoneBook(SharedDataDir & "Phonebook.csv")
            Me.Close()

        Catch ex As Exception

        End Try

    End Sub
End Class