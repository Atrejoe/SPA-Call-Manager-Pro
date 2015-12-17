#Region "File Header"
' 
' COPYRIGHT: Copyright 2008 
' Infralution 
' 
#End Region
Imports System
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Data
Imports System.Drawing
Imports System.Text
Imports System.Windows.Forms

''' <summary> 
''' The result of displaying an <see cref="EvaluationDialog"/> 
''' </summary> 
#If ILS_PUBLIC_CLASS Then
Public Enum EvaluationDialogResult 
#Else
Enum EvaluationDialogResult
#End If
    ''' <summary> 
    ''' Exit the application 
    ''' </summary> 
    [Exit]

    ''' <summary> 
    ''' Continue the evaluation 
    ''' </summary> 
    [Continue]

    ''' <summary> 
    ''' Install a license for the application 
    ''' </summary> 
    InstallLicense
End Enum

''' <summary> 
''' Form to display to evaluation customers 
''' </summary> 
#If ILS_PUBLIC_CLASS Then
Public Class EvaluationDialog 
    Inherits Form 
#Else
Class EvaluationDialog
    Inherits Form
#End If

#Region "Windows Form Designer generated code"

    ''' <summary> 
    ''' Required designer variable. 
    ''' </summary> 
    Private components As System.ComponentModel.IContainer = Nothing

    ''' <summary>The button to allow continuation of the evaluation</summary> 
    Protected WithEvents continueButton As System.Windows.Forms.Button

    ''' <summary>The button to install a license</summary> 
    Protected WithEvents installLicenseButton As System.Windows.Forms.Button

    ''' <summary>The button to exit the application</summary> 
    Protected WithEvents exitButton As System.Windows.Forms.Button

    ''' <summary>Label to display number of days left in the evaluation</summary> 
    Protected evaluationDaysLabel As System.Windows.Forms.Label

    ''' <summary>Label to display message about the evaluation conditions</summary> 
    Protected messageLabel As System.Windows.Forms.Label

    ''' <summary>Timer used to enable the <see cref="continueButton"/></summary> 
    Protected WithEvents evaluationTimer As Timer

    ''' <summary> 
    ''' Clean up any resources being used. 
    ''' </summary> 
    ''' <param name="disposing">true if managed resources should be disposed; otherwise, false.</param> 
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing AndAlso (components IsNot Nothing) Then
            components.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub


    ''' <summary> 
    ''' Required method for Designer support - do not modify 
    ''' the contents of this method with the code editor. 
    ''' </summary> 
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(EvaluationDialog))
        Me.continueButton = New System.Windows.Forms.Button
        Me.installLicenseButton = New System.Windows.Forms.Button
        Me.exitButton = New System.Windows.Forms.Button
        Me.evaluationDaysLabel = New System.Windows.Forms.Label
        Me.messageLabel = New System.Windows.Forms.Label
        Me.evaluationTimer = New System.Windows.Forms.Timer(Me.components)
        Me.SuspendLayout()
        '
        'continueButton
        '
        resources.ApplyResources(Me.continueButton, "continueButton")
        Me.continueButton.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.continueButton.Name = "continueButton"
        Me.continueButton.UseVisualStyleBackColor = True
        '
        'installLicenseButton
        '
        resources.ApplyResources(Me.installLicenseButton, "installLicenseButton")
        Me.installLicenseButton.Name = "installLicenseButton"
        Me.installLicenseButton.UseVisualStyleBackColor = True
        '
        'exitButton
        '
        resources.ApplyResources(Me.exitButton, "exitButton")
        Me.exitButton.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.exitButton.Name = "exitButton"
        Me.exitButton.UseVisualStyleBackColor = True
        '
        'evaluationDaysLabel
        '
        resources.ApplyResources(Me.evaluationDaysLabel, "evaluationDaysLabel")
        Me.evaluationDaysLabel.ForeColor = System.Drawing.Color.Green
        Me.evaluationDaysLabel.Name = "evaluationDaysLabel"
        '
        'messageLabel
        '
        resources.ApplyResources(Me.messageLabel, "messageLabel")
        Me.messageLabel.Name = "messageLabel"
        '
        'evaluationTimer
        '
        Me.evaluationTimer.Interval = 1000
        '
        'EvaluationDialog
        '
        Me.AcceptButton = Me.continueButton
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.exitButton
        Me.Controls.Add(Me.messageLabel)
        Me.Controls.Add(Me.evaluationDaysLabel)
        Me.Controls.Add(Me.exitButton)
        Me.Controls.Add(Me.installLicenseButton)
        Me.Controls.Add(Me.continueButton)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "EvaluationDialog"
        Me.ResumeLayout(False)

    End Sub

#End Region

#Region "Member Variables"

    Private _monitor As EvaluationMonitor
    Private _productName As String = Application.ProductName
    Private _trialDays As Integer = 30
    Private _extendedTrialDays As Integer = 60
    Private _dialogResult As EvaluationDialogResult = EvaluationDialogResult.[Exit]
    Private _extendedTrialDelay As New TimeSpan(0, 0, 0, 0, 500)

#End Region

#Region "Public Interface"

    ''' <summary> 
    ''' Default constructor (required for designer) 
    ''' </summary> 
    Public Sub New()
        InitializeComponent()
    End Sub

    ''' <summary>
    ''' Create a new instance of the form using the given evaluation monitor 
    ''' </summary>
    ''' <param name="evaluationMonitor">The evalulation monitor class to use to store the evaluation data</param>
    ''' <remarks>Uses the default ProductName from the application assembly info</remarks>
    Public Sub New(ByVal evaluationMonitor As EvaluationMonitor)
        InitializeComponent()
        _monitor = evaluationMonitor
    End Sub

    ''' <summary>
    ''' Create a new instance of the form using the given evaluation monitor and product name
    ''' </summary>
    ''' <param name="productName">Name of the product being licensed</param>
    ''' <param name="evaluationMonitor">The evalulation monitor class to use to store the evaluation data</param>
    Public Sub New(ByVal evaluationMonitor As EvaluationMonitor, ByVal productName As String)
        InitializeComponent()
        _productName = productName
        _monitor = evaluationMonitor
    End Sub

    ''' <summary> 
    ''' The evaluation monitor used to determine the trial period 
    ''' </summary> 
    Public Property EvaluationMonitor() As EvaluationMonitor
        Get
            Return _monitor
        End Get
        Set(ByVal value As EvaluationMonitor)
            _monitor = value
        End Set
    End Property

    ''' <summary> 
    ''' The name of the product being licensed 
    ''' </summary> 
    Public Shadows Property ProductName() As String
        Get
            Return _productName
        End Get
        Set(ByVal value As String)
            _productName = value
        End Set
    End Property

    ''' <summary> 
    ''' The number of days for the evaluation period 
    ''' </summary> 
    ''' <remarks> 
    ''' After this period has expired the ContinueButton will be enabled after an increasing delay 
    ''' until the <see cref="ExtendedTrialDays"/> limit is reached - at which point the ContinueButton 
    ''' will no longer be enabled. 
    ''' </remarks> 
    Public Property TrialDays() As Integer
        Get
            Return _trialDays
        End Get
        Set(ByVal value As Integer)
            _trialDays = value
        End Set
    End Property

    ''' <summary> 
    ''' The number of trial days after which evaluation is no longer enabled 
    ''' </summary> 
    ''' <remarks> 
    ''' If this is set to a value greater then <see cref="TrialDays"/> then users can continue 
    ''' to use the product past the evaluation period - however there is an increasing delay in 
    ''' enabling the ContinueButton. Once the ExtendedTrialDays limit is reached the ContinueButton 
    ''' will no longer be enabled. 
    ''' </remarks> 
    Public Property ExtendedTrialDays() As Integer
        Get
            Return _extendedTrialDays
        End Get
        Set(ByVal value As Integer)
            _extendedTrialDays = value
        End Set
    End Property

    ''' <summary> 
    ''' The time to delay enabling the <see cref="continueButton"/> for each day 
    ''' once the <see cref="TrialDays"/> period is exceeded. 
    ''' </summary> 
    Public Property ExtendedTrialDelay() As TimeSpan
        Get
            Return _extendedTrialDelay
        End Get
        Set(ByVal value As TimeSpan)
            _extendedTrialDelay = value
        End Set
    End Property

    ''' <summary> 
    ''' The message to display to the user 
    ''' </summary> 
    Public Property EvaluationMessage() As String
        Get
            Return messageLabel.Text
        End Get
        Set(ByVal value As String)
            messageLabel.Text = value
        End Set
    End Property

    ''' <summary> 
    ''' The result for this dialog 
    ''' </summary> 
    Public Shadows Property DialogResult() As EvaluationDialogResult
        Get
            Return _dialogResult
        End Get
        Set(ByVal value As EvaluationDialogResult)
            _dialogResult = value
        End Set
    End Property

    ''' <summary> 
    ''' Show the dialog 
    ''' </summary> 
    ''' <returns>The result of the evaluation dialog</returns> 
    Public Shadows Function ShowDialog() As EvaluationDialogResult
        MyBase.ShowDialog()
        Return _dialogResult
    End Function

    ''' <summary> 
    ''' Show the dialog 
    ''' </summary> 
    ''' <param name="owner">The owner form</param> 
    ''' <returns>The result of the evaluation dialog</returns> 
    Public Shadows Function ShowDialog(ByVal owner As IWin32Window) As EvaluationDialogResult
        MyBase.ShowDialog(owner)
        Return _dialogResult
    End Function

#End Region

#Region "Local Methods"

    ''' <summary> 
    ''' Resize the form to fit the message label 
    ''' </summary> 
    Protected Overridable Sub ResizeForm()
        Dim size As New SizeF(messageLabel.Width, 5000.0F)
        Using g As Graphics = Graphics.FromHwnd(messageLabel.Handle)
            size = g.MeasureString(messageLabel.Text, messageLabel.Font, size)
            Dim increaseHeight As Integer = CInt(size.Height) - messageLabel.Height
            If increaseHeight > 0 Then
                Me.Height += increaseHeight
            End If
        End Using
    End Sub

    ''' <summary> 
    ''' Initialize the form data 
    ''' </summary> 
    ''' <param name="e"></param> 
    Protected Overloads Overrides Sub OnLoad(ByVal e As EventArgs)
        MyBase.OnLoad(e)
        Text = String.Format(Text, ProductName)
        messageLabel.Text = String.Format(messageLabel.Text, ProductName)
        If AutoSize Then
            ResizeForm()
        End If
        If _monitor IsNot Nothing Then

            Dim daysInUse As Integer = _monitor.DaysInUse
            If _monitor.CountUsageOncePerDay Then
                daysInUse = _monitor.UsageCount
            End If

            ' ensure that the extended trial is at least as long as the trial
            '
            If _extendedTrialDays < _trialDays Then
                _extendedTrialDays = _trialDays
            End If
            evaluationDaysLabel.Text = String.Format(evaluationDaysLabel.Text, daysInUse, _trialDays)
            If daysInUse > _trialDays OrElse _monitor.Invalid Then
                evaluationDaysLabel.ForeColor = Color.Red
            End If
            If _monitor.Invalid Then
                evaluationDaysLabel.Text = LicenseResources.EvaluationInvalidMsg
                continueButton.Enabled = False
            Else
                If daysInUse > _extendedTrialDays Then
                    evaluationDaysLabel.Text = LicenseResources.EvaluationExpiredMsg
                End If
                continueButton.Enabled = (daysInUse <= _trialDays)
                If daysInUse > _trialDays AndAlso daysInUse <= _extendedTrialDays Then
                    evaluationTimer.Interval = CInt((_extendedTrialDelay.TotalMilliseconds * (daysInUse - _trialDays)))
                    evaluationTimer.Enabled = True
                End If
            End If
        End If
    End Sub

    ''' <summary> 
    ''' Handle a click on the <see cref="installLicenseButton"/> 
    ''' </summary> 
    ''' <param name="sender"></param> 
    ''' <param name="e"></param> 
    Protected Overridable Sub OnInstallLicense(ByVal sender As Object, ByVal e As EventArgs) Handles installLicenseButton.Click
        DialogResult = EvaluationDialogResult.InstallLicense
        Close()
    End Sub

    ''' <summary> 
    ''' Handle a click on the <see cref="continueButton"/> 
    ''' </summary> 
    ''' <param name="sender"></param> 
    ''' <param name="e"></param> 
    Protected Overridable Sub OnContinueEvaluation(ByVal sender As Object, ByVal e As EventArgs) Handles continueButton.Click
        DialogResult = EvaluationDialogResult.[Continue]
        Close()
    End Sub

    ''' <summary> 
    ''' Handle a click on the <see cref="exitButton"/> 
    ''' </summary> 
    ''' <param name="sender"></param> 
    ''' <param name="e"></param> 
    Protected Overridable Sub OnExit(ByVal sender As Object, ByVal e As EventArgs) Handles exitButton.Click
        DialogResult = EvaluationDialogResult.[Exit]
        Close()
    End Sub

    ''' <summary> 
    ''' Enable the <see cref="continueButton"/> after a period of time 
    ''' </summary> 
    ''' <param name="sender"></param> 
    ''' <param name="e"></param> 
    Protected Overridable Sub OnEvaluationTimerTick(ByVal sender As Object, ByVal e As EventArgs) Handles evaluationTimer.Tick
        continueButton.Enabled = True
        evaluationTimer.Enabled = False
    End Sub

#End Region

End Class
