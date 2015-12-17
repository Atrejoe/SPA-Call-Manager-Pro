#Region "File Header"
' 
' FILE: AuthenticatedLicenseInstallForm.vb. 
' 
' COPYRIGHT: Copyright 2008 
' Infralution 
' 
#End Region
Imports System
Imports System.Drawing
Imports System.Collections
Imports System.ComponentModel
Imports System.Windows.Forms
Imports System.Xml
Imports System.Net

''' <summary> 
''' Provides a basic form for installing Infralution Authenticated Licenses that can be extended or modified using 
''' visual inheritance 
''' </summary> 
''' <seealso cref="AuthenticatedLicenseProvider"/> 
#If ILS_PUBLIC_CLASS Then
Public Class AuthenticatedLicenseInstallForm 
    Inherits System.Windows.Forms.Form 
#Else
Class AuthenticatedLicenseInstallForm
    Inherits System.Windows.Forms.Form
#End If

#Region "Protected Member Variables"

    ''' <summary>The panel containing the action buttons</summary> 
    Protected WithEvents buttonPanel As Panel

    ''' <summary>The Close Button</summary> 
    Protected WithEvents closeButton As System.Windows.Forms.Button

    ''' <summary>Button to save the license to file</summary> 
    Protected WithEvents saveButton As Button

    ''' <summary>Button to save the license from file</summary> 
    Protected WithEvents loadButton As Button

    ''' <summary>Uninstall the currently installed license</summary> 
    Protected WithEvents uninstallButton As Button

    ''' <summary>Button to authenticate a key</summary> 
    Protected WithEvents installButton As Button

    ''' <summary>Displays the main message of the form</summary> 
    Protected WithEvents messageLabel As System.Windows.Forms.Label

    ''' <summary>Displays text to the left of the LicenseKey entry box</summary> 
    Protected WithEvents authenticationKeyLabel As System.Windows.Forms.Label

    ''' <summary>Allows the user to enter a license key for the product</summary> 
    Protected WithEvents authenticationKeyText As System.Windows.Forms.TextBox

    ''' <summary>Displays the currently installed license key</summary> 
    Protected WithEvents licenseStatusText As TextBox

    ''' <summary>Displays text to the left of the installed license key text box</summary> 
    Protected WithEvents licenseStatusLabel As Label

    ''' <summary>Displays the current computer ID</summary> 
    Protected WithEvents computerText As TextBox

    ''' <summary>Displays the text to the left of the current computer ID</summary> 
    Protected WithEvents computerLabel As Label

    ''' <summary>Timer used to update the screen while authenticating</summary> 
    Protected WithEvents authenticationTimer As Timer

    ''' <summary>Background worker use to authenticate keys</summary> 
    Protected WithEvents authenticationWorker As BackgroundWorker

#End Region

#Region "Private Member Variables"

    ''' <summary>Required by designer</summary> 
    Private components As IContainer

    ''' <summary>The name of the product being licensed</summary> 
    Private _productName As String = Application.ProductName

    ''' <summary>The license installed by the form (if any)</summary> 
    Private _license As AuthenticatedLicense

    ''' <summary>The license provider to use to install the license</summary> 
    Private _licenseProvider As AuthenticatedLicenseProvider

    ''' <summary>The type being licensed (if any)</summary> 
    Private _licenseType As System.Type

    ''' <summary>The license file to use (if not licensing a type)</summary> 
    Private _licenseFile As String

#End Region

#Region "Windows Form Designer generated code"

    ''' <summary> 
    ''' Required method for Designer support - do not modify 
    ''' the contents of this method with the code editor. 
    ''' </summary> 
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(AuthenticatedLicenseInstallForm))
        Me.closeButton = New System.Windows.Forms.Button
        Me.messageLabel = New System.Windows.Forms.Label
        Me.authenticationKeyText = New System.Windows.Forms.TextBox
        Me.authenticationKeyLabel = New System.Windows.Forms.Label
        Me.buttonPanel = New System.Windows.Forms.Panel
        Me.uninstallButton = New System.Windows.Forms.Button
        Me.saveButton = New System.Windows.Forms.Button
        Me.loadButton = New System.Windows.Forms.Button
        Me.installButton = New System.Windows.Forms.Button
        Me.licenseStatusText = New System.Windows.Forms.TextBox
        Me.licenseStatusLabel = New System.Windows.Forms.Label
        Me.computerText = New System.Windows.Forms.TextBox
        Me.computerLabel = New System.Windows.Forms.Label
        Me.authenticationTimer = New System.Windows.Forms.Timer(Me.components)
        Me.authenticationWorker = New System.ComponentModel.BackgroundWorker
        Me.buttonPanel.SuspendLayout()
        Me.SuspendLayout()
        '
        'closeButton
        '
        Me.closeButton.DialogResult = System.Windows.Forms.DialogResult.OK
        resources.ApplyResources(Me.closeButton, "closeButton")
        Me.closeButton.Name = "closeButton"
        '
        'messageLabel
        '
        resources.ApplyResources(Me.messageLabel, "messageLabel")
        Me.messageLabel.Name = "messageLabel"
        '
        'authenticationKeyText
        '
        resources.ApplyResources(Me.authenticationKeyText, "authenticationKeyText")
        Me.authenticationKeyText.Name = "authenticationKeyText"
        '
        'authenticationKeyLabel
        '
        resources.ApplyResources(Me.authenticationKeyLabel, "authenticationKeyLabel")
        Me.authenticationKeyLabel.Name = "authenticationKeyLabel"
        '
        'buttonPanel
        '
        Me.buttonPanel.Controls.Add(Me.uninstallButton)
        Me.buttonPanel.Controls.Add(Me.saveButton)
        Me.buttonPanel.Controls.Add(Me.closeButton)
        Me.buttonPanel.Controls.Add(Me.loadButton)
        resources.ApplyResources(Me.buttonPanel, "buttonPanel")
        Me.buttonPanel.Name = "buttonPanel"
        '
        'uninstallButton
        '
        resources.ApplyResources(Me.uninstallButton, "uninstallButton")
        Me.uninstallButton.Name = "uninstallButton"
        '
        'saveButton
        '
        resources.ApplyResources(Me.saveButton, "saveButton")
        Me.saveButton.Name = "saveButton"
        '
        'loadButton
        '
        resources.ApplyResources(Me.loadButton, "loadButton")
        Me.loadButton.Name = "loadButton"
        '
        'installButton
        '
        resources.ApplyResources(Me.installButton, "installButton")
        Me.installButton.Name = "installButton"
        '
        'licenseStatusText
        '
        resources.ApplyResources(Me.licenseStatusText, "licenseStatusText")
        Me.licenseStatusText.Name = "licenseStatusText"
        Me.licenseStatusText.ReadOnly = True
        '
        'licenseStatusLabel
        '
        resources.ApplyResources(Me.licenseStatusLabel, "licenseStatusLabel")
        Me.licenseStatusLabel.Name = "licenseStatusLabel"
        '
        'computerText
        '
        resources.ApplyResources(Me.computerText, "computerText")
        Me.computerText.Name = "computerText"
        Me.computerText.ReadOnly = True
        '
        'computerLabel
        '
        resources.ApplyResources(Me.computerLabel, "computerLabel")
        Me.computerLabel.Name = "computerLabel"
        '
        'authenticationTimer
        '
        Me.authenticationTimer.Interval = 1000
        '
        'authenticationWorker
        '
        '
        'AuthenticatedLicenseInstallForm
        '
        Me.AcceptButton = Me.closeButton
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.computerText)
        Me.Controls.Add(Me.computerLabel)
        Me.Controls.Add(Me.licenseStatusText)
        Me.Controls.Add(Me.licenseStatusLabel)
        Me.Controls.Add(Me.installButton)
        Me.Controls.Add(Me.buttonPanel)
        Me.Controls.Add(Me.authenticationKeyText)
        Me.Controls.Add(Me.authenticationKeyLabel)
        Me.Controls.Add(Me.messageLabel)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "AuthenticatedLicenseInstallForm"
        Me.buttonPanel.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
#End Region

#Region "Public Interface"

    ''' <summary> 
    ''' Initialize a new instance of the form 
    ''' </summary> 
    Public Sub New()
        InitializeComponent()
    End Sub

    ''' <summary> 
    ''' The license provider to use to install the license 
    ''' </summary> 
    ''' <remarks> 
    ''' This allows you to set a license provider other than the default 
    ''' (<see cref="AuthenticatedLicenseProvider"/>). 
    ''' </remarks> 
    Public Property LicenseProvider() As AuthenticatedLicenseProvider
        Get
            If _licenseProvider Is Nothing Then
                _licenseProvider = Me.GetLicenseProvider()
            End If
            Return _licenseProvider
        End Get
        Set(ByVal value As AuthenticatedLicenseProvider)
            _licenseProvider = value
        End Set
    End Property

    ''' <summary> 
    ''' The name of the product to license 
    ''' </summary> 
    ''' <remarks>The default product name is taken from the application assembly info</remarks> 
    Public Shadows Property ProductName() As String
        Get
            Return _productName
        End Get
        Set(ByVal value As String)
            _productName = value
        End Set
    End Property

    ''' <summary> 
    ''' The type of the component/control being licensed (if any) 
    ''' </summary> 
    ''' <remarks> 
    ''' Set this if you are licensing a component or control. If you are licensing an 
    ''' application set the <see cref="LicenseFile"/> property instead. 
    ''' </remarks> 
    Public Property TypeToLicense() As Type
        Get
            Return _licenseType
        End Get
        Set(ByVal value As Type)
            _licenseType = value
        End Set
    End Property

    ''' <summary> 
    ''' The name of the license file to use when licensing an application 
    ''' </summary> 
    ''' <remarks> 
    ''' Set this if you are licensing an application. If you are licensing a component 
    ''' or control then you should set the <see cref="TypeToLicense"/> property instead. 
    ''' </remarks> 
    ''' <seealso cref="AuthenticatedLicenseProvider.InstallLicense"/> 
    Public Property LicenseFile() As String
        Get
            Return _licenseFile
        End Get
        Set(ByVal value As String)
            _licenseFile = value
        End Set
    End Property

    ''' <summary> 
    ''' The installed license (if any) 
    ''' </summary> 
    Public Property InstalledLicense() As AuthenticatedLicense
        Get
            Return _license
        End Get
        Set(ByVal value As AuthenticatedLicense)
            _license = value
        End Set
    End Property

    ''' <summary> 
    ''' Should the license key value accept multiple lines 
    ''' </summary> 
    ''' <remarks> 
    ''' Set this to true if you want to allow multiline keys to be able to be pasted 
    ''' into the key field 
    ''' </remarks> 
    Public Property AllowMultilineKeys() As Boolean
        Get
            Return authenticationKeyText.Multiline
        End Get
        Set(ByVal value As Boolean)
            authenticationKeyText.Multiline = value
        End Set
    End Property


    ''' <summary> 
    ''' Display the form for licensing a component or control 
    ''' </summary> 
    ''' <param name="productName">The name of the product being licensed</param> 
    ''' <param name="typeToLicense">The type of the component being licensed</param> 
    ''' <param name="installedLicense">The currently installed license (if any)</param> 
    ''' <returns>The installed license (if any)</returns> 
    ''' <remarks> 
    ''' Use this method to display the dialog to install a license for a component or control type 
    ''' </remarks> 
    Public Overloads Function ShowDialog(ByVal productName As String, ByVal typeToLicense As Type, ByVal installedLicense As AuthenticatedLicense) As AuthenticatedLicense
        If typeToLicense Is Nothing Then
            Throw New ArgumentNullException("typeToLicense")
        End If
        _productName = productName
        _licenseType = typeToLicense
        _license = installedLicense
        Me.ShowDialog()
        Return _license
    End Function

    ''' <summary> 
    ''' Display the form for licensing an application 
    ''' </summary> 
    ''' <param name="licenseFile">The name of the license file</param> 
    ''' <param name="installedLicense">The currently installed license (if any)</param> 
    ''' <returns>The installed license (if any)</returns> 
    ''' <remarks> 
    ''' Use this method to display the dialog to install a license for an application 
    ''' </remarks> 
    Public Overloads Function ShowDialog(ByVal licenseFile As String, ByVal installedLicense As AuthenticatedLicense) As AuthenticatedLicense
        If licenseFile Is Nothing Then
            Throw New ArgumentNullException("licenseFile")
        End If
        _licenseFile = licenseFile
        _license = installedLicense
        Me.ShowDialog()
        Return _license
    End Function

    ''' <summary> 
    ''' Display the form for licensing an application 
    ''' </summary> 
    ''' <param name="productName">The name of the product being licensed</param> 
    ''' <param name="licenseFile">The name of the license file</param> 
    ''' <param name="installedLicense">The currently installed license (if any)</param> 
    ''' <returns>The installed license (if any)</returns> 
    ''' <remarks> 
    ''' Use this method to display the dialog to install a license for an application 
    ''' </remarks> 
    Public Overloads Function ShowDialog(ByVal productName As String, ByVal licenseFile As String, ByVal installedLicense As AuthenticatedLicense) As AuthenticatedLicense
        _productName = productName
        Return ShowDialog(licenseFile, installedLicense)
    End Function

#End Region

#Region "Local Methods and Overrides"

    ''' <summary> 
    ''' Initialize the form data 
    ''' </summary> 
    ''' <param name="e"></param> 
    Protected Overloads Overrides Sub OnLoad(ByVal e As EventArgs)
        MyBase.OnLoad(e)
        Text = String.Format(Text, ProductName)
        messageLabel.Text = String.Format(messageLabel.Text, ProductName)
        UpdateLicenseInfo()
    End Sub

    ''' <summary> 
    ''' Clean up any resources being used. 
    ''' </summary> 
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If components IsNot Nothing Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    ''' <summary> 
    ''' Return the license provider to use 
    ''' </summary> 
    ''' <returns>The license provider to use for installing licensing</returns> 
    Protected Overridable Function GetLicenseProvider() As AuthenticatedLicenseProvider
        Return New AuthenticatedLicenseProvider()
    End Function

    ''' <summary> 
    ''' Authenticate the key on a background thread so we can update the form 
    ''' </summary> 
    ''' <param name="sender"></param> 
    ''' <param name="e"></param> 
    Private Sub OnAuthenticateKey(ByVal sender As Object, ByVal e As DoWorkEventArgs) Handles authenticationWorker.DoWork
        Dim authenticationKey As String = TryCast(e.Argument, String)
        Try
            Dim license As AuthenticatedLicense = LicenseProvider.AuthenticateKey(authenticationKey)
            e.Result = license
        Catch ex As Exception
            e.Result = ex
        End Try
    End Sub

    ''' <summary> 
    ''' Handle completion of the AuthenticateKey operation 
    ''' </summary> 
    ''' <param name="sender"></param> 
    ''' <param name="e"></param> 
    Private Sub OnAuthenticateKeyComplete(ByVal sender As Object, ByVal e As RunWorkerCompletedEventArgs) Handles authenticationWorker.RunWorkerCompleted
        authenticationTimer.[Stop]()
        Cursor = Cursors.[Default]
        If TypeOf e.Result Is AuthenticatedLicense Then
            Dim license As AuthenticatedLicense = TryCast(e.Result, AuthenticatedLicense)
            If _licenseType Is Nothing Then
                LicenseProvider.InstallLicense(_licenseFile, license)
            Else
                LicenseProvider.InstallLicense(_licenseType, license)
            End If
            _license = license
            authenticationKeyText.Text = Nothing
        ElseIf TypeOf e.Result Is Exception Then
            Dim ex As Exception = TryCast(e.Result, Exception)
            If TypeOf e.Result Is AuthenticationsExceededException Then
                MessageBox.Show(LicenseResources.AuthenticationsExceededMsg, LicenseResources.AuthenticationErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.[Error])
            Else
                Dim msg As String
                If TypeOf ex Is WebException Then
                    msg = [String].Format(LicenseResources.ConnectionErrorMsg)
                Else
                    msg = [String].Format(LicenseResources.AuthenticationErrorMsg, ex.Message)
                End If
                Dim result As DialogResult = MessageBox.Show(msg, LicenseResources.AuthenticationErrorTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2)
                If result = DialogResult.Yes Then
                    Dim productName As String = AuthenticatedLicenseProvider.Parameters.ProductName
                    Dim computer As String = LicenseProvider.GetComputerID()
                    Dim license As New AuthenticatedLicense(productName, authenticationKeyText.Text, computer, Nothing)
                    SaveLicense(license)
                End If
            End If
        Else
            Dim msg As String = [String].Format(LicenseResources.InvalidAuthenticationKeyMsg, authenticationKeyText.Text)
            MessageBox.Show(msg, LicenseResources.InvalidAuthenticationKeyTitle, MessageBoxButtons.OK, MessageBoxIcon.[Error])
        End If
        UpdateLicenseInfo()
        Me.buttonPanel.Enabled = True
        Me.installButton.Enabled = True
    End Sub


    ''' <summary> 
    ''' Update the license status while authenticating 
    ''' </summary> 
    ''' <param name="sender"></param> 
    ''' <param name="e"></param> 
    Private Sub OnAuthenticationTimerTick(ByVal sender As Object, ByVal e As EventArgs) Handles authenticationTimer.Tick
        Cursor = Cursors.WaitCursor
        If licenseStatusText.Text = LicenseResources.AuthenticatingTxt Then
            licenseStatusText.Text = LicenseResources.WaitTxt
        Else
            licenseStatusText.Text = LicenseResources.AuthenticatingTxt
        End If
        licenseStatusText.SelectAll()
        Me.Refresh()
        Cursor = Cursors.WaitCursor
    End Sub

    ''' <summary> 
    ''' Save the given license to file 
    ''' </summary> 
    ''' <param name="license">The license to save</param> 
    Protected Overridable Sub SaveLicense(ByVal license As AuthenticatedLicense)
        Dim dialog As New SaveFileDialog()
        dialog.Title = LicenseResources.SaveLicenseTitle
        dialog.Filter = LicenseResources.LicenseFileFilter
        dialog.FileName = "License.lic"
        dialog.DefaultExt = "lic"
        If dialog.ShowDialog() = DialogResult.OK Then
            LicenseProvider.WriteLicense(dialog.FileName, license)
        End If
    End Sub

    ''' <summary> 
    ''' Install the license using the authentication key entered by the user 
    ''' </summary> 
    ''' <param name="authenticationKey">The authentication key to install</param> 
    Protected Overridable Sub InstallLicense(ByVal authenticationKey As String)
        Me.licenseStatusText.Text = LicenseResources.AuthenticatingTxt
        Me.Refresh()
        Cursor = Cursors.WaitCursor
        Me.buttonPanel.Enabled = False
        Me.installButton.Enabled = False
        authenticationTimer.Start()
        authenticationWorker.RunWorkerAsync(authenticationKey)
    End Sub

    ''' <summary> 
    ''' Update the information display for the current license 
    ''' </summary> 
    Protected Overridable Sub UpdateLicenseInfo()
        Me.saveButton.Enabled = _license IsNot Nothing
        Me.uninstallButton.Enabled = _license IsNot Nothing
        If _license Is Nothing Then
            Me.licenseStatusText.Text = LicenseResources.NoLicenseInstalledTxt
            Me.computerText.Text = LicenseProvider.GetComputerID()
        Else
            Me.licenseStatusText.Text = LicenseResources.LicenseInstalledTxt
            Me.computerText.Text = _license.ComputerID
        End If
    End Sub

    ''' <summary> 
    ''' Enable the install button when text is entered in the text box 
    ''' </summary> 
    ''' <param name="sender"></param> 
    ''' <param name="e"></param> 
    Protected Overridable Sub OnAuthenticationKeyTextChanged(ByVal sender As Object, ByVal e As EventArgs) Handles authenticationKeyText.TextChanged
        installButton.Enabled = (authenticationKeyText.Text IsNot Nothing AndAlso authenticationKeyText.Text.Trim().Length > 0)
    End Sub

    ''' <summary> 
    ''' Handle a click on the <see cref="installButton"/> 
    ''' </summary> 
    ''' <param name="sender"></param> 
    ''' <param name="e"></param> 
    Protected Overridable Sub OnInstallButtonClick(ByVal sender As Object, ByVal e As EventArgs) Handles installButton.Click
        InstallLicense(authenticationKeyText.Text)
    End Sub

    ''' <summary> 
    ''' Handle a click on the <see cref="saveButton"/> 
    ''' </summary> 
    ''' <param name="sender"></param> 
    ''' <param name="e"></param> 
    Protected Overridable Sub OnSaveButtonClick(ByVal sender As Object, ByVal e As EventArgs) Handles saveButton.Click
        SaveLicense(_license)
    End Sub

    ''' <summary> 
    ''' Handle a click on the <see cref="loadButton"/> 
    ''' </summary> 
    ''' <param name="sender"></param> 
    ''' <param name="e"></param> 
    Protected Sub OnLoadButtonClick(ByVal sender As Object, ByVal e As EventArgs) Handles loadButton.Click
        Dim dialog As New OpenFileDialog()
        dialog.Title = LicenseResources.LoadLicenseTitle
        dialog.Filter = LicenseResources.LicenseFileFilter
        If dialog.ShowDialog() = DialogResult.OK Then
            Dim license As AuthenticatedLicense = LicenseProvider.ReadLicense(dialog.FileName)
            If license IsNot Nothing Then
                LicenseProvider.ValidateLicense(license)
                If license.Status = AuthenticatedLicenseStatus.Valid Then
                    _license = license
                    If _licenseType Is Nothing Then
                        LicenseProvider.InstallLicense(_licenseFile, license)
                    Else
                        LicenseProvider.InstallLicense(_licenseType, license)
                    End If
                    UpdateLicenseInfo()
                Else
                    LicenseProvider.ShowInvalidStatusMessage(license)
                End If
            End If
        End If
    End Sub

    ''' <summary> 
    ''' Handle a click on the <see cref="uninstallButton"/> 
    ''' </summary> 
    ''' <param name="sender"></param> 
    ''' <param name="e"></param> 
    Protected Sub OnUninstallButtonClick(ByVal sender As Object, ByVal e As EventArgs) Handles uninstallButton.Click

        Dim result As DialogResult = MessageBox.Show(LicenseResources.ConfirmUninstallMsg, LicenseResources.ConfirmUninstallTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question)
        If result = DialogResult.Yes Then
            Try
                If _licenseType Is Nothing Then
                    LicenseProvider.UninstallLicense(_licenseFile)
                Else
                    LicenseProvider.UninstallLicense(_licenseType)
                End If
                _license = Nothing
                UpdateLicenseInfo()
            Catch ex As Exception
                Dim msg As String = String.Format(LicenseResources.UninstallErrorMsg, ex.Message)
                MessageBox.Show(msg, LicenseResources.UninstallErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.[Error])
            End Try
        End If
    End Sub

    ''' <summary> 
    ''' Prevent the form closing while there is a pending authentication 
    ''' </summary> 
    ''' <param name="e"></param> 
    Protected Overloads Overrides Sub OnClosing(ByVal e As CancelEventArgs)
        MyBase.OnClosing(e)
        If authenticationWorker.IsBusy Then
            e.Cancel = True
        End If
    End Sub

#End Region

End Class
