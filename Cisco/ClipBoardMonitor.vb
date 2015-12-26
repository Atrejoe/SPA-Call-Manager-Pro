Imports System.Windows.Forms
Public Class ClipBoardMonitor
    Inherits Form

#Region " Definitions "
    'Constants for API Calls...
    Private Const WM_DRAWCLIPBOARD As Integer = &H308
    Private Const WM_CHANGECBCHAIN As Integer = &H30D

    'Handle for next clipboard viewer...
    Private mNextClipBoardViewerHWnd As IntPtr

    'API declarations...
    Friend Declare Auto Function SetClipboardViewer Lib "user32" (ByVal HWnd As IntPtr) As IntPtr
    Friend Declare Auto Function ChangeClipboardChain Lib "user32" (ByVal HWnd As IntPtr, ByVal HWndNext As IntPtr) As Boolean
    Friend Declare Auto Function SendMessage Lib "User32" (ByVal HWnd As IntPtr, ByVal Msg As Integer, ByVal wParam As IntPtr, ByVal lParam As IntPtr) As Long
    Public Event ClipBoardItemAdded(ByVal data As String)

#End Region

#Region " Contructor "
    Public Sub New()
        'To register this form as a clipboard viewer...
        mNextClipBoardViewerHWnd = SetClipboardViewer(Me.Handle)
    End Sub
#End Region

#Region " Message Process "
    'Override WndProc to get messages...
    Protected Overrides Sub WndProc(ByRef m As Message)
        Select Case m.Msg
            Case Is = WM_DRAWCLIPBOARD 'The clipboard has changed...
                Dim data_object As IDataObject = Clipboard.GetDataObject
                If data_object.GetDataPresent(DataFormats.Text) Then
                    RaiseEvent ClipBoardItemadded(Clipboard.GetText)
                End If
                SendMessage(mNextClipBoardViewerHWnd, m.Msg, m.WParam, m.LParam)

            Case Is = WM_CHANGECBCHAIN 'Another clipboard viewer has removed itself...
                If m.WParam = CType(mNextClipBoardViewerHWnd, IntPtr) Then
                    mNextClipBoardViewerHWnd = m.LParam
                Else
                    SendMessage(mNextClipBoardViewerHWnd, m.Msg, m.WParam, m.LParam)
                End If
        End Select

        MyBase.WndProc(m)
    End Sub
#End Region

#Region " Dispose "
    'Form overrides dispose to clean up...
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            'Set the next clipboard viewer back to the original... 
            ChangeClipboardChain(Me.Handle, mNextClipBoardViewerHWnd)
            MyBase.Dispose(disposing)
        End If
    End Sub
#End Region

End Class