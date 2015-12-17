' 
' FILE: AuthenticatedLicense.vb 
' 
' COPYRIGHT: Copyright 2008 
' Infralution 
' 
Imports System
Imports System.ComponentModel
Imports System.Globalization
Imports System.Text.RegularExpressions

''' <summary> 
''' The status of an <see cref="AuthenticatedLicense"/> once it has been validated by calling 
''' <see cref="AuthenticatedLicenseProvider.ValidateLicense"/> 
''' </summary> 
#If ILS_PUBLIC_CLASS Then
Public Enum AuthenticatedLicenseStatus
#Else
Enum AuthenticatedLicenseStatus
#End If

    ''' <summary> 
    ''' The license has not been validated yet 
    ''' </summary> 
    Unvalidated

    ''' <summary> 
    ''' The license has not been authenticated 
    ''' </summary> 
    Unauthenticated

    ''' <summary> 
    ''' The license is valid 
    ''' </summary> 
    Valid

    ''' <summary> 
    ''' The license is not valid for this computer 
    ''' </summary> 
    InvalidComputer

    ''' <summary> 
    ''' The license key is not for this product 
    ''' </summary> 
    InvalidProduct

    ''' <summary> 
    ''' The license key does not match the license key parameters 
    ''' </summary> 
    InvalidKey

    ''' <summary> 
    ''' The license contents do not match the signature, indicating possible tampering 
    ''' </summary> 
    InvalidSignature
End Enum

''' <summary> 
''' Defines an authenticated license for an application or component generated using the Infralution 
''' Licensing System. 
''' </summary> 
''' <remarks> 
''' </remarks> 
''' <seealso cref="AuthenticatedLicenseProvider"/> 
#If ILS_PUBLIC_CLASS Then
Public Class AuthenticatedLicense
    Inherits License
#Else
Class AuthenticatedLicense
    Inherits License
#End If

#Region "Member Variables"

    Private _status As AuthenticatedLicenseStatus = AuthenticatedLicenseStatus.Unvalidated
    Private _productName As String
    Private _encryptedLicenseKey As String
    Private _computerID As String
    Private _signature As String
    Private _authenticationData As String

    Private _encryptedLicense As EncryptedLicense

#End Region

#Region "Public Interface"

    ''' <summary> 
    ''' Create a new Infralution Authenticated License 
    ''' </summary> 
    ''' <param name="productName">The name of the product the license is for (MUST match the license parameters)</param> 
    ''' <param name="encryptedLicenseKey">The encrypted license key</param> 
    ''' <param name="computerID">The ID of the computer (if any) that the license is locked to</param> 
    ''' <param name="signature">The public key signature for the license</param> 
    Public Sub New(ByVal productName As String, ByVal encryptedLicenseKey As String, ByVal computerID As String, ByVal signature As String)
        _productName = productName
        _encryptedLicenseKey = encryptedLicenseKey
        _computerID = computerID
        _signature = signature
    End Sub

    ''' <summary> 
    ''' Create a new Infralution Authenticated License 
    ''' </summary> 
    ''' <param name="productName">The name of the product the license is for (MUST match the license parameters)</param> 
    ''' <param name="encryptedLicenseKey">The encrypted license key</param> 
    ''' <param name="computerID">The ID of the computer (if any) that the license is locked to</param> 
    ''' <param name="authenticationData">User defined data set when the license is authenticated</param>
    ''' <param name="signature">The public key signature for the license</param> 
    Public Sub New(ByVal productName As String, ByVal encryptedLicenseKey As String, ByVal computerID As String, ByVal authenticationData As String, ByVal signature As String)
        _productName = productName
        _encryptedLicenseKey = encryptedLicenseKey
        _computerID = computerID
        _authenticationData = authenticationData
        _signature = signature
    End Sub

    ''' <summary> 
    ''' Create a new Infralution Authenticated License 
    ''' </summary> 
    ''' <param name="parameters">The product parameters to use to sign the license</param>
    ''' <param name="encryptedLicense">The encrypted license</param>
    ''' <param name="computerID">The ID of the computer (if any) that the license is locked to</param> 
    ''' <param name="authenticationData">User defined data set when the license is authenticated</param>
    Public Sub New(ByVal parameters As AuthenticatedLicenseParameters, ByVal encryptedLicense As EncryptedLicense, ByVal computerID As String, ByVal authenticationData As String)
        _productName = ProductName
        _encryptedLicense = encryptedLicense
        _computerID = computerID
        _authenticationData = authenticationData
        _signature = parameters.SignText(ContentToSign)
        _status = AuthenticatedLicenseStatus.Valid
    End Sub

    ''' <summary> 
    ''' The validation status of this license 
    ''' </summary> 
    Public ReadOnly Property Status() As AuthenticatedLicenseStatus
        Get
            Return _status
        End Get
    End Property

    ''' <summary> 
    ''' Has the license been signed 
    ''' </summary> 
    Public ReadOnly Property Signed() As Boolean
        Get
            Return _signature IsNot Nothing
        End Get
    End Property

    ''' <summary> 
    ''' Returns the encrypted license key 
    ''' </summary> 
    ''' <remarks> 
    ''' Note that this is NOT the authentication key 
    ''' </remarks> 
    Public Overloads Overrides ReadOnly Property LicenseKey() As String
        Get
            Return _encryptedLicenseKey
        End Get
    End Property

    ''' <summary> 
    ''' Set/Get the underlying encrypted license (once the license has been validated) 
    ''' </summary> 
    Public ReadOnly Property EncryptedLicense() As EncryptedLicense
        Get
            Return _encryptedLicense
        End Get
    End Property

    ''' <summary> 
    ''' The product data associated with the license 
    ''' </summary> 
    Public ReadOnly Property ProductInfo() As String
        Get
            If _encryptedLicense Is Nothing Then
                Throw New InvalidOperationException("License has not been validated")
            End If
            Return _encryptedLicense.ProductInfo
        End Get
    End Property

    ''' <summary> 
    ''' The unique serial no for the license 
    ''' </summary> 
    Public ReadOnly Property SerialNo() As Int32
        Get
            If _encryptedLicense Is Nothing Then
                Throw New InvalidOperationException("License has not been validated")
            End If
            Return _encryptedLicense.SerialNo
        End Get
    End Property

    ''' <summary> 
    ''' The ID of the computer that this license is for 
    ''' </summary> 
    Public ReadOnly Property ComputerID() As String
        Get
            Return _computerID
        End Get
    End Property

    ''' <summary> 
    ''' User defined data set when the license is authenticated
    ''' </summary> 
    Public ReadOnly Property AuthenticationData() As String
        Get
            Return _authenticationData
        End Get
    End Property

    ''' <summary> 
    ''' The name of the product this license is for 
    ''' </summary> 
    Public ReadOnly Property ProductName() As String
        Get
            Return _productName
        End Get
    End Property

    ''' <summary> 
    ''' The Public Key Signature for the license 
    ''' </summary> 
    Public ReadOnly Property Signature() As String
        Get
            Return _signature
        End Get
    End Property

    ''' <summary> 
    ''' Cleans up any resources held by the license 
    ''' </summary> 
    Public Overloads Overrides Sub Dispose()
    End Sub

#End Region

#Region "Local Methods"

    ''' <summary> 
    ''' Verify that the signature of the license matches the contents
    ''' </summary> 
    ''' <param name="encryptedLicense">The encrypted license</param> 
    ''' <param name="parameters">The license parameters used to sign the license</param>
    Friend Function VerifySignature(ByVal encryptedLicense As EncryptedLicense, ByVal parameters As AuthenticatedLicenseParameters) As Boolean
        If encryptedLicense Is Nothing Then Throw New ArgumentNullException("encryptedLicense")
        _encryptedLicense = encryptedLicense
        Return parameters.VerifyText(ContentToSign, Signature)
    End Function

    ''' <summary>
    ''' Set the status of the license
    ''' </summary>
    ''' <param name="status">The status of the license</param>
    Friend Sub SetStatus(ByVal status As AuthenticatedLicenseStatus)
        _status = status
    End Sub

    ''' <summary>
    ''' Return the content of the license to be signed
    ''' </summary>
    Private ReadOnly Property ContentToSign() As String
        Get
            Dim data As String = AuthenticationData
            If Not String.IsNullOrEmpty(data) Then
                Dim sRegex As New Regex("\s*")
                data = sRegex.Replace(data, "")
            End If
            Return EncryptedLicense.LicenseKey + ComputerID + data
        End Get
    End Property

#End Region

End Class

