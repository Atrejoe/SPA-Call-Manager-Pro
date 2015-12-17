' 
' FILE: AuthenticatedLicenseProvider.vb. 
' 
' COPYRIGHT: Copyright 2008 
' Infralution 
' 
Imports System
Imports System.Collections.Generic
Imports System.Text
Imports System.ComponentModel
Imports System.ComponentModel.Design
Imports System.Security
Imports System.Security.Cryptography
Imports System.Diagnostics
Imports System.IO
Imports System.Xml
Imports System.Net
Imports System.Globalization
Imports System.Reflection
Imports System.Collections
Imports System.Runtime.Serialization
Imports System.Runtime.Serialization.Formatters
Imports System.Runtime.Serialization.Formatters.Binary

''' <summary> 
''' The parameters used to generate and validate <see cref="AuthenticatedLicense">AuthenticatedLicenses</see> 
''' using an <see cref="AuthenticatedLicenseProvider"/>. 
''' </summary> 
''' <seealso cref="AuthenticatedLicenseProvider"/> 
#If ILS_PUBLIC_CLASS Then
Public Class AuthenticatedLicenseParameters
    Inherits EncryptedLicenseParameters
#Else
Class AuthenticatedLicenseParameters
    Inherits EncryptedLicenseParameters
#End If

    Private _authenticationPassword As String
    Private _authenticationServerUrl As String
    Private _serverRsaProvider As RSACryptoServiceProvider = LicenseUtilities.CreateRSACryptoServiceProvider()
    Private _maxAuthentications As Integer = 2
    ' the maximum number of distinct authentications for a given key 
    ''' <summary> 
    ''' The password used to encrypt a simple license key to produce an authenticated license key 
    ''' </summary> 
    ''' <remarks> 
    ''' If this is null then authentication keys are the same as the underlying <see cref="EncryptedLicense"/> 
    ''' keys. This can be useful if you want to change from using an <see cref="EncryptedLicenseProvider"/> 
    ''' but don't wish to issue new license keys. Using a null value does sacrifice the extra level of security 
    ''' provided by the authentication password. 
    ''' </remarks> 
    Public Property AuthenticationPassword() As String
        Get
            Return _authenticationPassword
        End Get
        Set(ByVal value As String)
            If value <> _authenticationPassword Then
                _authenticationPassword = value

                ' if the password is changed then also use a new RSA key for signing 
                ' 
                _serverRsaProvider = LicenseUtilities.CreateRSACryptoServiceProvider()
            End If
        End Set
    End Property

    ''' <summary> 
    ''' The URL of the Authentication Server 
    ''' </summary> 
    Public Property AuthenticationServerURL() As String
        Get
            Return _authenticationServerUrl
        End Get
        Set(ByVal value As String)
            _authenticationServerUrl = value
        End Set
    End Property

    ''' <summary> 
    ''' The maximum number of distinct authentications (on different computers) allowed for a given key 
    ''' </summary> 
    Public Property MaxAuthentications() As Integer
        Get
            Return _maxAuthentications
        End Get
        Set(ByVal value As Integer)
            _maxAuthentications = value
        End Set
    End Property

    ''' <summary> 
    ''' Write the parameters to the given XML writer 
    ''' </summary> 
    ''' <param name="writer">The writer to write to</param> 
    ''' <param name="includeGenerationParameters">Should parameters required for generating keys be included</param> 
    Public Overloads Overrides Sub Write(ByVal writer As XmlWriter, ByVal includeGenerationParameters As Boolean)
        Write(writer, includeGenerationParameters, includeGenerationParameters)
    End Sub

    ''' <summary> 
    ''' Read parameters from the given XML reader 
    ''' </summary> 
    ''' <param name="reader">The reader to read from</param> 
    Public Overloads Overrides Sub Read(ByVal reader As XmlReader)
        reader.ReadStartElement("AuthenticatedLicenseParameters")
        While reader.IsStartElement()
            Select Case reader.Name
                Case "AuthenticationPassword"
                    _authenticationPassword = reader.ReadElementString()
                    Exit Select
                Case "AuthenticationServerURL"
                    _authenticationServerUrl = reader.ReadElementString()
                    Exit Select
                Case "MaxAuthentications"
                    _maxAuthentications = reader.ReadElementContentAsInt()
                    Exit Select
                Case "ServerRSAKeyValue"
                    LicenseUtilities.ReadRSAParameters(_serverRsaProvider, reader, "ServerRSAKeyValue")
                    Exit Select
                Case "EncryptedLicenseParameters"
                    MyBase.Read(reader)
                    Exit Select
                Case Else
                    Dim [error] As String = "Unexpected XML Element: {0}"
                    Throw New XmlSyntaxException(String.Format([error], reader.Name))
            End Select
        End While
        reader.ReadEndElement()
    End Sub

    ''' <summary> 
    ''' Sign the given text using the <see cref="ServerRSAProvider"/> 
    ''' </summary> 
    ''' <param name="text">The text to sign</param> 
    ''' <returns>The signature for the text</returns> 
    Public Function SignText(ByVal text As String) As String
        Dim textBytes As Byte() = Encoding.UTF8.GetBytes(text)
        Dim signatureData As Byte() = LicenseUtilities.SignData(ServerRSAProvider, textBytes)
        Return Convert.ToBase64String(signatureData)
    End Function

    ''' <summary> 
    ''' Verify that the given text was signed using the <see cref="ServerRSAProvider"/> 
    ''' </summary> 
    ''' <param name="text">The text to verify</param> 
    ''' <param name="signature">The RSA signature</param> 
    ''' <returns>True if the text was signed with the given signature</returns> 
    Public Function VerifyText(ByVal text As String, ByVal signature As String) As Boolean
        Dim textBytes As Byte() = Encoding.UTF8.GetBytes(text)
        Dim signatureBytes As Byte() = Convert.FromBase64String(signature)
        Return VerifyData(textBytes, signatureBytes)
    End Function

    ''' <summary> 
    ''' Sign the given data using the <see cref="ServerRSAProvider"/> 
    ''' </summary> 
    ''' <param name="data">The data to sign</param> 
    ''' <returns>The signature for the text</returns> 
    Public Function SignData(ByVal data As Byte()) As Byte()
        Return LicenseUtilities.SignData(ServerRSAProvider, data)
    End Function

    ''' <summary> 
    ''' Verify that the given data was signed using the <see cref="ServerRSAProvider"/> 
    ''' </summary> 
    ''' <param name="data">The data to verify</param> 
    ''' <param name="signature">The RSA signature</param> 
    ''' <returns>True if the text was signed with the given signature</returns> 
    Public Function VerifyData(ByVal data As Byte(), ByVal signature As Byte()) As Boolean
        Dim result As Boolean = LicenseUtilities.VerifyData(ServerRSAProvider, data, signature)

        ' Version 4.2.2 and earlier used the same RSA parameters for Encrypted and Authenticated 
        ' license - this allows license files authenticated using earlier version to continue 
        ' to work 
        ' 
        If Not result Then
            result = LicenseUtilities.VerifyData(RSAProvider, data, signature)
        End If
        Return result
    End Function

    ''' <summary> 
    ''' The RSA provider used by the server to sign/validate licenses 
    ''' </summary> 
    Public ReadOnly Property ServerRSAProvider() As RSACryptoServiceProvider
        Get
            Return _serverRsaProvider
        End Get
    End Property

    ''' <summary> 
    ''' Write the parameters to the given XML writer 
    ''' </summary> 
    ''' <param name="writer">The writer to write to</param> 
    ''' <param name="includeAuthenticationParameters">Should parameters required for authenticating keys be included</param> 
    ''' <param name="includeGenerationParameters">Should parameters required for generating keys be included</param> 
    Public Overridable Overloads Sub Write(ByVal writer As XmlWriter, ByVal includeAuthenticationParameters As Boolean, ByVal includeGenerationParameters As Boolean)
        writer.WriteStartElement("AuthenticatedLicenseParameters")
        MyBase.Write(writer, includeGenerationParameters)
        If includeAuthenticationParameters Then
            writer.WriteElementString("AuthenticationPassword", AuthenticationPassword)
            writer.WriteElementString("MaxAuthentications", MaxAuthentications.ToString())
        End If
        writer.WriteElementString("AuthenticationServerURL", AuthenticationServerURL)
        LicenseUtilities.WriteRSAParameters(ServerRSAProvider, writer, "ServerRSAKeyValue", includeAuthenticationParameters)
        writer.WriteEndElement()
    End Sub


    ''' <summary> 
    ''' Write the parameters to an XML string 
    ''' </summary> 
    ''' <param name="includeAuthenticationPrivateParameters">Should parameters required to authenticate keys be included</param> 
    ''' <param name="includeEncryptedLicensePrivateParameters">Should the private parameters for validating <see cref="EncryptedLicense"/> be included</param> 
    ''' <returns>The parameters in a formatted XML string</returns> 
    Public Overridable Overloads Function WriteToString(ByVal includeAuthenticationPrivateParameters As Boolean, ByVal includeEncryptedLicensePrivateParameters As Boolean) As String
        Dim stringWriter As New StringWriter()
        Dim xmlWriter As New XmlTextWriter(stringWriter)
        Write(xmlWriter, includeAuthenticationPrivateParameters, includeEncryptedLicensePrivateParameters)
        xmlWriter.Close()
        Dim result As String = stringWriter.ToString()
        stringWriter.Close()
        Return result
    End Function

End Class


''' <summary> 
''' Thrown by <see cref="AuthenticatedLicenseProvider.AuthenticateKey"/> if the 
''' maximum number of allowed authentications has been exceeded 
''' </summary> 
Public Class AuthenticationsExceededException
    Inherits ApplicationException
    ''' <summary> 
    ''' Default constructor 
    ''' </summary> 
    Public Sub New()
    End Sub

    ''' <summary> 
    ''' Constructor required for remoting serialization 
    ''' </summary> 
    ''' <param name="info"></param> 
    ''' <param name="context"></param> 
    Public Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)
        MyBase.New(info, context)
    End Sub
End Class

''' <summary> 
''' Defines a .NET LicenseProvider that authenticates license keys to run on a particular Computer 
''' by contacting an Authentication Web Service. 
''' </summary> 
''' <remarks> 
''' <para> 
''' When a user installs an authenticated license key for a product the AuthenticatedLicenseProvider 
''' contacts the Authentication Web Service. It validates the license key and returns a signed 
''' <see cref="AuthenticatedLicense"/> locked to the users computer. When the application is run subsequently 
''' the AuthenticatedLicenseProvider validates that the installed license matches the users computer and verifies 
''' the public key signature. This prevents users modifying or copying licenses files between computers. The 
''' Authentication Web Service can be configured (using the License Tracker application) to limit the number of 
''' different computers that a given license key can be used to authenticate. 
''' </para> 
''' <para> 
''' By default the <see cref="Environment.MachineName"/> (or the host name 
''' for ASP.NET applications) is used to identify the computer. This is generally sufficient to inhibit copying 
''' while still providing flexibility to the end user. They can change hardware and even operating system without 
''' invalidating their licenses. If, however, you wish to lock licenses to other hardware or computer characteristics 
''' you can do this by overriding the <see cref="AuthenticatedLicenseProvider.GetComputerID"/> method. 
''' </para> 
''' <para>
''' See <see href="b617d141-c6c9-4d86-b93e-b049fc12fd72.htm" target="_self">Getting Started</see> 
''' for detailed information on using AuthenticatedLicenseProvider to license applications and components.
''' </para>
''' </remarks> 
''' <seealso cref="AuthenticatedLicense"/> 
#If ILS_PUBLIC_CLASS Then
Public Class AuthenticatedLicenseProvider
    Inherits LicenseProvider
#Else
Class AuthenticatedLicenseProvider
    Inherits LicenseProvider
#End If

#Region "Member Variables"

    ''' <summary> 
    ''' Salt for generated random bytes from a password 
    ''' </summary> 
    Private Shared _salt As Byte() = New Byte() {41, 54, 110, 111, 32, 77, 53, 100, 118, 101, 97, 101, 118}

    ''' <summary> 
    ''' The current parameters for validating licenses 
    ''' </summary> 
    Private Shared _parameters As AuthenticatedLicenseParameters

#End Region

#Region "Public Interface"


    ''' <summary> 
    ''' Set the parameters used to validate licenses created by this provider. 
    ''' </summary> 
    ''' <remarks> 
    ''' <para> 
    ''' This must be called by the client software prior to obtaining licenses using the AuthenticatedLicenseProvider. 
    ''' The XML parameter string is generated using License Tracker and pasted into the calling client code 
    ''' or by calling <see cref="AuthenticatedLicenseParameters.WriteToString"/> 
    ''' </para> 
    ''' <para> 
    ''' Note that calling this method also sets the <see cref="EncryptedLicenseProvider.Parameters"/> property. 
    ''' </para> 
    ''' </remarks> 
    ''' <param name="licenseParameters">An XML string containing parameters used to validate licenses</param> 
    Public Shared Sub SetParameters(ByVal licenseParameters As String)
        Dim params As New AuthenticatedLicenseParameters()
        params.ReadFromString(licenseParameters)
        Parameters = params
    End Sub

    ''' <summary> 
    ''' Set/Get the Parameters for validating <see cref="AuthenticatedLicense">AuthenticatedLicenses</see> 
    ''' </summary> 
    ''' <remarks> 
    ''' Note that this also sets the <see cref="EncryptedLicenseProvider.Parameters"/> property. 
    ''' </remarks> 
    Public Shared Property Parameters() As AuthenticatedLicenseParameters
        Get
            Return _parameters
        End Get
        Set(ByVal value As AuthenticatedLicenseParameters)
            _parameters = value
            EncryptedLicenseProvider.Parameters = value
        End Set
    End Property

    ''' <summary> 
    ''' Generate an authenticated license key 
    ''' </summary> 
    ''' <param name="parameters">The parameters to use to generate the key</param> 
    ''' <param name="productInfo">User defined data to be included in the key</param> 
    ''' <param name="serialNo">The unique license serial number</param> 
    ''' <returns>An authenticated license key</returns> 
    ''' <remarks> 
    ''' If there is no installed license for the Infralution Licensing System then the only 
    ''' allowed productPassword is "TEST" and the only allowed serial numbers are 1 or 0. 
    ''' </remarks> 
    Public Overridable Function GenerateKey(ByVal parameters As AuthenticatedLicenseParameters, ByVal productInfo As String, ByVal serialNo As Int32) As String

        ' generate a standard encrypted key 
        ' 
        Dim provider As EncryptedLicenseProvider = GetEncryptedLicenseProvider()
        Dim licenseKey As String = provider.GenerateKey(parameters, productInfo, serialNo)

        ' if there is a non-null authentication password then re-encrypt the license key 
        ' 
        If Not String.IsNullOrEmpty(parameters.AuthenticationPassword) Then

            ' decode back to binary before encrypting to avoid expanding the size of the key 
            ' 
            Dim keyData As Byte() = provider.DecodeFromText(licenseKey, parameters.TextEncoding)
            Dim passwordBytes As New Rfc2898DeriveBytes(parameters.AuthenticationPassword, _salt)
            Dim tripleDes As New TripleDESCryptoServiceProvider()
            tripleDes.Key = passwordBytes.GetBytes(16)
            tripleDes.IV = passwordBytes.GetBytes(8)
            Dim encKeyData As Byte() = tripleDes.CreateEncryptor().TransformFinalBlock(keyData, 0, keyData.Length)
            licenseKey = LicenseUtilities.EncodeToText(encKeyData, parameters.TextEncoding)
        End If
        Return licenseKey
    End Function

    ''' <summary> 
    ''' Authenticates the given key on a speficic computer by contacting the AuthenticationServer and
    ''' registers application data  
    ''' </summary> 
    ''' <param name="authenticationKey">The authentication key to validate</param> 
    ''' <param name="computerID">The ID of the computer to authenticate the key for</param> 
    ''' <param name="applicationData">Application data to register with the authentication (if any)</param>
    ''' <returns>An AuthenticatedLicense if the key is valid or null otherwise</returns> 
    ''' <remarks> 
    ''' The <see cref="SetParameters"/> method MUST be called before using this method. The 
    ''' returned license is not Validated for this computer. 
    ''' Callers should handle the possibility of a <see cref="WebException"/> being 
    ''' thrown if the Authentication Server cannot be contacted.
    ''' </remarks> 
    Public Overridable Function AuthenticateKeyOnComputerWithData(ByVal authenticationKey As String, _
                                                                  ByVal computerID As String, _
                                                                  ByVal applicationData As String) As AuthenticatedLicense
        If _parameters Is Nothing Then
            Throw New InvalidOperationException("AuthenticatedLicenseProvider.SetParameters must be called prior to calling this method")
        End If

        Dim service As New AuthenticationService
        Dim license As AuthenticatedLicense = Nothing
        Dim xml As String = service.Authenticate(_parameters, computerID, authenticationKey, applicationData)
        If xml IsNot Nothing Then
            license = ReadLicenseFromString(xml)
        End If
        Return license
    End Function

    ''' <summary> 
    ''' Authenticates the given key on a speficic computer by contacting the AuthenticationServer and
    ''' registers application data  
    ''' </summary> 
    ''' <param name="authenticationKey">The authentication key to validate</param> 
    ''' <param name="computerID">The ID of the computer to authenticate the key for</param> 
    ''' <returns>An AuthenticatedLicense if the key is valid or null otherwise</returns> 
    ''' <remarks> 
    ''' The <see cref="SetParameters"/> method MUST be called before using this method. The 
    ''' returned license is not Validated for this computer. 
    ''' Callers should handle the possibility of a <see cref="WebException"/> being 
    ''' thrown if the Authentication Server cannot be contacted.
    ''' </remarks> 
    Public Overridable Function AuthenticateKeyOnComputer(ByVal authenticationKey As String, _
                                                          ByVal computerID As String) As AuthenticatedLicense
        Return AuthenticateKeyOnComputerWithData(authenticationKey, computerID, Nothing)
    End Function

    ''' <summary> 
    ''' Authenticates the given key by contacting the AuthenticationServer and registers
    ''' application data
    ''' </summary> 
    ''' <param name="authenticationKey">The authentication key to validate</param> 
    ''' <param name="applicationData">Application data to register with the authentication (if any)</param>
    ''' <returns>An AuthenticatedLicense if the key is valid or null otherwise</returns> 
    ''' <remarks> 
    ''' The <see cref="SetParameters"/> method MUST be called before using this method. 
    ''' Callers should handle the possibility of a <see cref="WebException"/> being 
    ''' thrown if the Authentication Server cannot be contacted.
    ''' </remarks> 
    Public Overridable Function AuthenticateKeyWithData(ByVal authenticationKey As String, _
                                                        ByVal applicationData As String) As AuthenticatedLicense
        Dim license As AuthenticatedLicense = AuthenticateKeyOnComputerWithData(authenticationKey, GetComputerID(), applicationData)
        If license IsNot Nothing Then
            If ValidateLicense(license) <> AuthenticatedLicenseStatus.Valid Then
                license = Nothing
            End If
        End If
        Return license
    End Function

    ''' <summary> 
    ''' Authenticates the given key by contacting the AuthenticationServer 
    ''' </summary> 
    ''' <param name="authenticationKey">The authentication key to validate</param> 
    ''' <returns>An AuthenticatedLicense if the key is valid or null otherwise</returns> 
    ''' <remarks> 
    ''' The <see cref="SetParameters"/> method MUST be called before using this method. 
    ''' Callers should handle the possibility of a <see cref="WebException"/> being 
    ''' thrown if the Authentication Server cannot be contacted.
    ''' </remarks> 
    Public Overridable Function AuthenticateKey(ByVal authenticationKey As String) As AuthenticatedLicense
        Return AuthenticateKeyWithData(authenticationKey, GetApplicationData())
    End Function

    ''' <summary> 
    ''' Authenticates the given key by contacting the AuthenticationServer 
    ''' </summary> 
    ''' <param name="licenseParameters">An XML string containing parameters used to validate the license key</param> 
    ''' <param name="authenticationKey">The authentication key to validate</param> 
    ''' <returns>An AuthenticatedLicense if the key is valid or null otherwise</returns> 
    ''' <remarks> 
    ''' This method is an alternative to calling <see cref="SetParameters"/> followed by 
    ''' <see cref="AuthenticateKey"/>. 
    ''' Callers should handle the possibility of a <see cref="WebException"/> being 
    ''' thrown if the Authentication Server cannot be contacted.
    ''' </remarks> 
    Public Overridable Function AuthenticateKey(ByVal licenseParameters As String, ByVal authenticationKey As String) As AuthenticatedLicense
        SetParameters(licenseParameters)
        Return AuthenticateKey(authenticationKey)
    End Function

    ''' <summary>
    ''' Check with the Authentication Server whether a given license is still authenticated 
    ''' </summary>
    ''' <param name="license">The license to check</param>
    ''' <returns>True if the license is authenticated on this computer</returns>
    ''' <remarks>
    ''' This method provides a mechanism to check with the authentication service whether a
    ''' license remains authenticated for a given computer.
    ''' The <see cref="SetParameters"/> method MUST be called before using this method. 
    ''' Callers should handle the possibility of a <see cref="WebException"/> being 
    ''' thrown if the Authentication Server cannot be contacted.
    ''' </remarks>
    Public Overridable Function IsAuthenticated(ByVal license As AuthenticatedLicense) As Boolean
        Dim service As New AuthenticationService()
        Return service.IsAuthenticated(_parameters, license.LicenseKey, license.ComputerID)
    End Function

    ''' <summary>
    ''' Get the ProductInfo from an authentication key without authenticating the key 
    ''' </summary>
    ''' <param name="authenticationKey">The authentication key to get the ProductInfo for</param>
    ''' <returns>The ProductInfo from the key or null if the key is not valid</returns>
    ''' <remarks>
    ''' This method provides a mechanism to get the authentication service to decrypt an authentication
    ''' key and return the ProductInfo without authenticating the key.  The <see cref="SetParameters"/> 
    ''' method MUST be called before using this method.  Callers should handle the possibility
    ''' of a <see cref="WebException"/> being thrown if the Authentication Server cannot be contacted.
    ''' </remarks>
    Public Overridable Function GetProductInfo(ByVal authenticationKey As String) As String
        Dim service As New AuthenticationService()
        Return service.GetProductInfo(_parameters, authenticationKey)
    End Function

    ''' <summary>
    ''' Get the AuthenticationData for an authentication key without authenticating the key 
    ''' </summary>
    ''' <param name="authenticationKey">The authentication key to get the ProductInfo for</param>
    ''' <returns>The AuthenticationData from the key or null if the key is not valid</returns>
    ''' <remarks>
    ''' The <see cref="SetParameters"/> method MUST be called before using this method.  
    ''' Callers should handle the possibility of a <see cref="WebException"/> being thrown if the 
    ''' Authentication Server cannot be contacted.
    ''' </remarks>
    Public Overridable Function GetAuthenticationData(ByVal authenticationKey As String) As String
        Dim service As New AuthenticationService()
        Return service.GetAuthenticationData(_parameters, authenticationKey)
    End Function

    ''' <summary> 
    ''' Called by the Authentication Server to decrypt authentication keys 
    ''' </summary> 
    ''' <param name="parameters">The license parameters</param> 
    ''' <param name="authenticationKey">The authentication key</param> 
    ''' <returns>The encrypted license</returns> 
    Public Overridable Function DecryptAuthenticationKey(ByVal parameters As AuthenticatedLicenseParameters, ByVal authenticationKey As String) As EncryptedLicense
        If parameters Is Nothing Then
            Throw New ArgumentNullException("parameters")
        End If
        If authenticationKey Is Nothing Then
            Throw New ArgumentNullException("authenticationKey")
        End If

        AuthenticatedLicenseProvider.Parameters = parameters
        Dim encryptedLicense As EncryptedLicense = Nothing
        Dim licenseKey As String = Nothing
        Try
            Dim elp As EncryptedLicenseProvider = GetEncryptedLicenseProvider()
            If String.IsNullOrEmpty(parameters.AuthenticationPassword) Then
                licenseKey = authenticationKey
            Else
                Dim encKeyData As Byte() = LicenseUtilities.DecodeFromText(authenticationKey, parameters.TextEncoding)
                Dim passwordBytes As New Rfc2898DeriveBytes(parameters.AuthenticationPassword, _salt)
                Dim tripleDes As New TripleDESCryptoServiceProvider()
                tripleDes.Key = passwordBytes.GetBytes(16)
                tripleDes.IV = passwordBytes.GetBytes(8)
                Dim keyData As Byte() = tripleDes.CreateDecryptor().TransformFinalBlock(encKeyData, 0, encKeyData.Length)
                licenseKey = elp.EncodeToText(keyData, parameters.TextEncoding)
            End If

            ' validate that the encrypted license is valid 
            ' 
            encryptedLicense = elp.ValidateLicenseKey(licenseKey)
        Catch
        End Try
        Return encryptedLicense
    End Function

    ''' <summary> 
    ''' Validate that the given license is legitimate on this computer 
    ''' </summary> 
    ''' <param name="license">The license to validate</param> 
    ''' <param name="context">The license context</param> 
    ''' <param name="type">The type the license is associated with</param> 
    ''' <returns>The license status</returns> 
    ''' <remarks> 
    ''' The <see cref="SetParameters"/> method MUST be called before using this method. 
    ''' </remarks> 
    Public Overridable Function ValidateLicense(ByVal license As AuthenticatedLicense, ByVal context As LicenseContext, ByVal type As Type) As AuthenticatedLicenseStatus
        If license Is Nothing Then
            Throw New ArgumentNullException("license")
        End If
        If _parameters Is Nothing Then
            Throw New InvalidOperationException("AuthenticatedLicenseProvider.SetParameters must be called prior to calling this method")
        End If

        Dim status As AuthenticatedLicenseStatus = AuthenticatedLicenseStatus.Unvalidated
        Dim elp As EncryptedLicenseProvider = GetEncryptedLicenseProvider()
        Dim el As EncryptedLicense = Nothing
        If license.ProductName <> _parameters.ProductName AndAlso license.ProductName IsNot Nothing Then
            status = AuthenticatedLicenseStatus.InvalidProduct
        ElseIf String.IsNullOrEmpty(license.Signature) Then
            status = AuthenticatedLicenseStatus.Unauthenticated
        Else
            ' validate the EncryptedLicense 
            ' 
            el = elp.ValidateLicenseKey(license.LicenseKey, context, type)
            If el Is Nothing Then
                status = AuthenticatedLicenseStatus.InvalidKey
            Else
                ' now check the signature matches the license contents 
                ' 
                If license.VerifySignature(el, _parameters) Then
                    ' check the computer ID matches this computer 
                    ' 
                    If IsThisComputer(license.ComputerID) Then
                        status = AuthenticatedLicenseStatus.Valid
                    Else
                        status = AuthenticatedLicenseStatus.InvalidComputer
                    End If
                Else
                    status = AuthenticatedLicenseStatus.InvalidSignature
                End If
            End If
        End If
        license.SetStatus(status)
        Return status
    End Function

    ''' <summary> 
    ''' Validate that the given license is legitimate on this computer 
    ''' </summary> 
    ''' <param name="license">The license to validate</param> 
    ''' <returns>The license status</returns> 
    ''' <remarks> 
    ''' The <see cref="SetParameters"/> method MUST be called before using this method. 
    ''' </remarks> 
    Public Overridable Function ValidateLicense(ByVal license As AuthenticatedLicense) As AuthenticatedLicenseStatus
        Return ValidateLicense(license, LicenseManager.CurrentContext, Nothing)
    End Function

    ''' <summary> 
    ''' Return the installed license from the given license file. 
    ''' </summary> 
    ''' <param name="licenseFile">The name of the license file containing the license</param> 
    ''' <param name="validateLicense"> 
    ''' If true the license is validated and only returned if valid. Otherwise it is the callers 
    ''' responsibility to call <see cref="ValidateLicense"/> to check the license validity. 
    ''' </param> 
    ''' <returns>The installed license if any</returns> 
    ''' <remarks> 
    ''' <para> 
    ''' This method is used to read licenses for applications. Components and controls should use the 
    ''' <see cref="LicenseManager"/> methods to load and validate licenses. If a full path is not specified 
    ''' for licenseFile then the file loaded will be relative to the directory containing the application 
    ''' executable (for Window Forms applications) or aspx files (for ASP.NET applications). 
    ''' </para> 
    ''' <para> 
    ''' The <see cref="SetParameters"/> method MUST be called before using this method. 
    ''' </para> 
    ''' </remarks> 
    Public Overridable Overloads Function GetLicense(ByVal licenseFile As String, ByVal validateLicense As Boolean) As AuthenticatedLicense
        Dim result As AuthenticatedLicense = Nothing
        Dim dir As String = GetLicenseDirectory(LicenseManager.CurrentContext, Nothing)
        Dim path As String = System.IO.Path.Combine(dir, licenseFile)
        Dim license As AuthenticatedLicense = ReadLicense(path)
        If license IsNot Nothing AndAlso validateLicense Then
            Me.ValidateLicense(license)
            If license.Status = AuthenticatedLicenseStatus.Valid Then
                result = license
            Else
                ShowInvalidStatusMessage(license)
            End If
        Else
            result = license
        End If
        Return result
    End Function

    ''' <summary> 
    ''' Get a license (if installed) from the given license file. 
    ''' </summary> 
    ''' <param name="licenseParameters">An XML string containing parameters used to validate the license key</param> 
    ''' <param name="licenseFile">The name of the license file containing the license</param> 
    ''' <param name="validateLicense"> 
    ''' If true the license is validated and only returned if valid. Otherwise it is the callers 
    ''' responsibility to call <see cref="ValidateLicense"/> to check the license validity. 
    ''' </param> 
    ''' <returns>The installed license if any</returns> 
    ''' <remarks> 
    ''' <para> 
    ''' This method is used to read licenses for applications. Components and controls should use the 
    ''' <see cref="LicenseManager"/> methods to read and validate licenses. If a full path is not specified 
    ''' for licenseFile then the file loaded will be relative to the directory containing the application 
    ''' executable (for Window Forms applications) or aspx files (for ASP.NET applications). 
    ''' </para> 
    ''' <para> 
    ''' This method is an alternative to calling <see cref="SetParameters"/> followed by 
    ''' <see cref="GetLicense"/>. 
    ''' </para> 
    ''' </remarks> 
    Public Overloads Function GetLicense(ByVal licenseParameters As String, ByVal licenseFile As String, ByVal validateLicense As Boolean) As AuthenticatedLicense
        SetParameters(licenseParameters)
        Return GetLicense(licenseFile, validateLicense)
    End Function

    ''' <summary> 
    ''' Get a license (if installed) for the given component/control type 
    ''' </summary> 
    ''' <param name="context">The context (design or runtime)</param> 
    ''' <param name="type">The type to get the license for</param> 
    ''' <param name="instance">The object the license is for</param> 
    ''' <param name="allowExceptions">If true a <see cref="LicenseException"/> is thrown if a valid license cannot be loaded</param> 
    ''' <returns>An encrypted license</returns> 
    ''' <remarks> 
    ''' <para> 
    ''' This method is used to get licenses for components and controls. Applications should generally 
    ''' use the <see cref="GetLicense"/> method as it provides more control over the license file 
    ''' that keys are stored in. This method is not typically called directly by application code. 
    ''' Instead the component or control uses the <see cref="LicenseManager.IsValid"/> or 
    ''' <see cref="LicenseManager.Validate"/> methods which find the <see cref="LicenseProvider"/> for the type 
    ''' and call this method. 
    ''' </para> 
    ''' <para> 
    ''' You must call <see cref="SetParameters"/> before calling this method either directly or 
    ''' indirectly by via a call to <see cref="LicenseManager.IsValid"/> 
    ''' </para> 
    ''' </remarks> 
    Public Overloads Overrides Function GetLicense(ByVal context As LicenseContext, ByVal type As Type, ByVal instance As Object, ByVal allowExceptions As Boolean) As License
        Dim result As License = Nothing

        ' If this is runtime then try to get a valid EncryptedLicense from the 
        ' the saved context. 
        ' 
        If context.UsageMode = LicenseUsageMode.Runtime Then
            Try
                Dim provider As EncryptedLicenseProvider = GetEncryptedLicenseProvider()
                Dim key As String = context.GetSavedLicenseKey(type, Nothing)
                result = provider.ValidateLicenseKey(key, context, type)
                ' if something goes wrong retrieving the saved license key then just ignore it 
                ' and try reading from file 
            Catch
            End Try
        End If

        If result Is Nothing Then
            ' if we're in design mode or a suitable license key wasn't found in 
            ' the runtime context try to read a license from file 
            ' 
            Dim license As AuthenticatedLicense = ReadLicense(GetLicenseFilePath(context, type))
            If license IsNot Nothing Then
                If ValidateLicense(license, context, type) = AuthenticatedLicenseStatus.Valid Then
                    result = license
                Else
                    If allowExceptions Then
                        Throw New LicenseException(type, instance, "License is not valid on this Computer")
                    End If
                End If
            End If
        End If

        If result Is Nothing AndAlso allowExceptions Then
            Throw New LicenseException(type, instance, "No License Installed")
        End If
        Return result
    End Function

    ''' <summary> 
    ''' Install the license for the given type 
    ''' </summary> 
    ''' <remarks> 
    ''' This method is used to install licenses for components and controls. The <see cref="InstallLicense"/> 
    ''' method is typically better for installing application licenses because it provides more control over the 
    ''' license key file name. This license key file used by this method is the full type name followed by a ".lic" suffix. 
    ''' </remarks> 
    ''' <param name="type">The component/control type the license is associated with </param> 
    ''' <param name="license">The license to install</param> 
    Public Overridable Sub InstallLicense(ByVal type As Type, ByVal license As AuthenticatedLicense)
        If license Is Nothing Then
            Throw New ArgumentNullException("license")
        End If
        Dim licenseFile As String = GetLicenseFilePath(LicenseManager.CurrentContext, type)
        WriteLicense(licenseFile, license)
    End Sub

    ''' <summary> 
    ''' Install a license key for an application in the given file. 
    ''' </summary> 
    ''' <remarks> 
    ''' This method is used to install licenses for applications. Use the <see cref="InstallLicense"/> 
    ''' method to install licenses for components or controls. If a full path is not specified for licenseFile then 
    ''' the file will be created relative to the entry executable directory. 
    ''' <para> 
    ''' The AuthenticatedLicenseInstallForm uses this method to install licenses for applications. 
    ''' Client applications may implement their own registration forms that call this method. 
    ''' </para> 
    ''' </remarks> 
    ''' <param name="licenseFile">The name of the file to install the license key in</param> 
    ''' <param name="license">The license key to install</param> 
    Public Overridable Sub InstallLicense(ByVal licenseFile As String, ByVal license As AuthenticatedLicense)
        If license Is Nothing Then
            Throw New ArgumentNullException("license")
        End If
        Try
            Dim baseDir As String = GetLicenseDirectory(LicenseManager.CurrentContext, Nothing)
            Dim path As String = System.IO.Path.Combine(baseDir, licenseFile)
            Dim dir As String = IO.Path.GetDirectoryName(path)
            If Not Directory.Exists(dir) Then
                Directory.CreateDirectory(dir)
            End If
            WriteLicense(path, license)
        Catch ex As Exception
            If Not LicenseUtilities.HandleIOExceptions Then
                Throw
            End If
            Dim msg As String = String.Format(LicenseResources.WriteErrorMsg, ex.Message, licenseFile)
            ShowError(LicenseResources.WriteErrorTitle, msg)
        End Try
    End Sub

    ''' <summary> 
    ''' Uninstall a license key for the given component or control type. 
    ''' </summary> 
    ''' <remarks> 
    ''' Deletes the license file for the given type 
    ''' </remarks> 
    ''' <param name="type">The type to uninstall the license for</param> 
    Public Overridable Sub UninstallLicense(ByVal type As Type)
        Dim licenseFile As String = GetLicenseFilePath(LicenseManager.CurrentContext, type)
        LicenseUtilities.UninstallLicenseFile(licenseFile)
    End Sub

    ''' <summary> 
    ''' Uninstall the license key in the given file. 
    ''' </summary> 
    ''' <remarks> 
    ''' Deletes the license file 
    ''' </remarks> 
    ''' <param name="licenseFile">The name of the file the license key is in</param> 
    Public Overridable Sub UninstallLicense(ByVal licenseFile As String)
        Dim baseDir As String = GetLicenseDirectory(LicenseManager.CurrentContext, Nothing)
        Dim path As String = System.IO.Path.Combine(baseDir, licenseFile)
        LicenseUtilities.UninstallLicenseFile(path)
    End Sub

    ''' <summary> 
    ''' Read a license from an <see cref="XmlTextReader"/> 
    ''' </summary> 
    ''' <param name="reader">The XML reader to use</param> 
    ''' <returns>A new (unvalidated) license</returns> 
    Public Overridable Function ReadLicense(ByVal reader As XmlTextReader) As AuthenticatedLicense
        Dim productName As String = Nothing
        Dim licenseKey As String = Nothing
        Dim computerID As String = Nothing
        Dim signature As String = Nothing
        Dim authenticationData As String = Nothing
        Try
            reader.ReadStartElement("AuthenticatedLicense")
            While reader.IsStartElement()
                Select Case reader.Name
                    Case "ProductName"
                        productName = reader.ReadElementString()
                        Exit Select
                    Case "EncryptedLicenseKey"
                        licenseKey = reader.ReadElementString()
                        Exit Select
                    Case "ComputerID"
                        computerID = reader.ReadElementString()
                        Exit Select
                    Case "AuthenticationData"
                        authenticationData = ReadAuthenticationData(reader)
                        Exit Select
                    Case "Signature"
                        signature = reader.ReadElementString()
                        Exit Select
                    Case Else
                        reader.Skip()
                        Exit Select
                End Select
            End While
            reader.ReadEndElement()
        Catch ex As Exception
            If Not LicenseUtilities.HandleIOExceptions Then
                Throw
            End If
            Dim msg As String = String.Format(LicenseResources.ReadXmlErrorMsg, ex.Message)
            ShowError(LicenseResources.ReadErrorTitle, msg)
        End Try
        Return New AuthenticatedLicense(productName, licenseKey, computerID, signature)
    End Function

    ''' <summary> 
    ''' Read a license from the given file 
    ''' </summary> 
    ''' <param name="licenseFile">The file to read the license from</param> 
    ''' <returns>A new (unvalidated) license</returns> 
    ''' <remarks> 
    ''' Use <see cref="ValidateLicense"/> to validate that the license 
    ''' is valid for this computer 
    ''' </remarks> 
    Public Overridable Function ReadLicense(ByVal licenseFile As String) As AuthenticatedLicense
        Try
            If File.Exists(licenseFile) Then
                Using reader As New XmlTextReader(licenseFile)
                    Return ReadLicense(reader)
                End Using
            End If
        Catch ex As Exception
            If Not LicenseUtilities.HandleIOExceptions Then
                Throw
            End If
            Dim msg As String = String.Format(LicenseResources.ReadErrorMsg, ex.Message, licenseFile)
            ShowError(LicenseResources.ReadErrorTitle, msg)
        End Try
        Return Nothing
    End Function

    ''' <summary> 
    ''' Read a license from a string 
    ''' </summary> 
    ''' <param name="licenseXml">A string containing the license XML</param> 
    ''' <returns>A new (unvalidated) license</returns> 
    ''' <remarks> 
    ''' Use <see cref="ValidateLicense"/> to validate that the license 
    ''' is valid for this computer 
    ''' </remarks> 
    Public Overridable Function ReadLicenseFromString(ByVal licenseXml As String) As AuthenticatedLicense
        Dim stringReader As New StringReader(licenseXml)
        Using reader As New XmlTextReader(stringReader)
            Return ReadLicense(reader)
        End Using
    End Function

    ''' <summary> 
    ''' Write the license out using the given <see cref="XmlTextWriter"/> 
    ''' </summary> 
    ''' <param name="license">The license to write</param> 
    ''' <param name="writer">The writer to use</param> 
    Public Overridable Sub WriteLicense(ByVal writer As XmlTextWriter, ByVal license As AuthenticatedLicense)
        writer.Formatting = Formatting.Indented
        writer.WriteStartElement("AuthenticatedLicense")
        writer.WriteElementString("ProductName", license.ProductName)
        writer.WriteElementString("EncryptedLicenseKey", license.LicenseKey)
        writer.WriteElementString("ComputerID", license.ComputerID)
        If license.AuthenticationData IsNot Nothing Then
            WriteAuthenticationData(writer, license.AuthenticationData)
        End If
        If license.Signature IsNot Nothing Then
            writer.WriteElementString("Signature", license.Signature)
        End If
        writer.WriteEndElement()
    End Sub

    ''' <summary> 
    ''' Write the license out to the given file 
    ''' </summary> 
    ''' <param name="license">The license to write</param> 
    ''' <param name="licenseFile">The file to write the license to</param> 
    Public Overridable Sub WriteLicense(ByVal licenseFile As String, ByVal license As AuthenticatedLicense)
        Try
            Using writer As New XmlTextWriter(licenseFile, Encoding.UTF8)
                WriteLicense(writer, license)
            End Using
        Catch ex As Exception
            If Not LicenseUtilities.HandleIOExceptions Then
                Throw
            End If
            Dim msg As String = String.Format(LicenseResources.WriteErrorMsg, ex.Message, licenseFile)
            ShowError(LicenseResources.WriteErrorTitle, msg)
        End Try
    End Sub

    ''' <summary> 
    ''' Write the license out to an XML string 
    ''' </summary> 
    ''' <param name="license">The license to write</param> 
    ''' <returns>A string containing the XML for the license</returns> 
    Public Overridable Function WriteLicenseToString(ByVal license As AuthenticatedLicense) As String
        Dim stringWriter As New StringWriter()
        Using writer As New XmlTextWriter(stringWriter)
            WriteLicense(writer, license)
        End Using
        Return stringWriter.ToString()
    End Function

    ''' <summary> 
    ''' Return the string used to identify the computer the license should be authenticated for 
    ''' </summary> 
    ''' <returns>A string identifying the computer</returns> 
    ''' <remarks> 
    ''' This id is included in the authenticated license and checked when the license 
    ''' is validated. This prevents a license being authenticated on one machine and 
    ''' then copied to another. The default implementation returns the <see cref="Environment.MachineName"/> 
    ''' (or the host name for ASP.NET applications). 
    ''' While a user could change the machine name to match a copied license it is generally 
    ''' inconvenient for them to do so (particularly in a networked environment). If 
    ''' you want a stronger deterrent to copying then you can override this method and return 
    ''' a string based on other hardware characteristics 
    ''' </remarks> 
    ''' <seealso cref="IsThisComputer"/> 
    Public Overridable Function GetComputerID() As String
#If ILS_ASP Then
        Dim computer As String = Nothing
        Dim sContext As System.Web.HttpContext = System.Web.HttpContext.Current
        If sContext IsNot Nothing Then
            computer = FixHostName(sContext.Request.Url.Host)

            ' if we are running on a local host development server 
            ' then return the machine name instead of the host name 
            ' this can avoid using an authentication for the local 
            ' development server 
            ' 
            If computer = "localhost" Then
                computer = Environment.MachineName
            End If
        Else
            computer = Environment.MachineName
        End If
        Return computer
#Else
        return Environment.MachineName
#End If
    End Function

    ''' <summary> 
    ''' Check if the given computer ID matches this computer 
    ''' </summary> 
    ''' <param name="computerID">The id to check</param> 
    ''' <returns>True if the computerID is for this computer</returns> 
    Public Overridable Function IsThisComputer(ByVal computerID As String) As Boolean
        Dim thisComputerID As String = GetComputerID()
        If thisComputerID Is Nothing Then Return (computerID Is Nothing)

#If ILS_ASP Then
        ' previous versions of GetComputerID did not strip the leading "www." from host names
        ' so when comparing we strip this off so that we don't invalidate license files generated
        ' with previous versions
        '
        computerID = FixHostName(computerID)
#End If

        Return thisComputerID.Equals(computerID, StringComparison.InvariantCultureIgnoreCase)
    End Function

    ''' <summary> 
    ''' Returns application data (if any) to be registered by default when keys are authenticated
    ''' </summary> 
    ''' <returns>A string to pass to the authentication server</returns> 
    ''' <remarks> 
    ''' This can be overridden to pass application specific data back to the authentication server
    ''' where it is registered in the database against the authentication.  You could for instance
    ''' return information about the computer OS and environment for support purposes.  The default 
    ''' implementation returns null. 
    ''' </remarks> 
    Public Overridable Function GetApplicationData() As String
        Return Nothing
    End Function

    ''' <summary> 
    ''' Display the default error message when <see cref="AuthenticatedLicense.Status"/> is not <see cref="AuthenticatedLicenseStatus.Valid"/> 
    ''' </summary> 
    ''' <param name="license">The license to display the message for</param> 
    ''' <remarks> 
    ''' If called from an interactive application then displays a message box - otherwise 
    ''' the error is sent to the trace output window. 
    ''' </remarks> 
    Public Overridable Sub ShowInvalidStatusMessage(ByVal license As AuthenticatedLicense)
        Dim title As String = LicenseResources.InvalidLicenseTitle
        Select Case license.Status
            Case AuthenticatedLicenseStatus.InvalidComputer
                ShowError(title, String.Format(LicenseResources.InvalidComputerMsg, license.ComputerID, GetComputerID()))
                Exit Select
            Case AuthenticatedLicenseStatus.Unauthenticated
                ShowError(title, LicenseResources.UnauthenticatedLicenseMsg)
                Exit Select
            Case AuthenticatedLicenseStatus.InvalidSignature
                ShowError(title, LicenseResources.InvalidSignatureMsg)
                Exit Select
            Case AuthenticatedLicenseStatus.InvalidKey
                ShowError(title, String.Format(LicenseResources.InvalidKeyMsg, license.LicenseKey))
                Exit Select
            Case AuthenticatedLicenseStatus.InvalidProduct
                ShowError(title, String.Format(LicenseResources.InvalidProductMsg, _parameters.ProductName, license.ProductName))
                Exit Select
        End Select
    End Sub

#End Region

#Region "Local Methods"

    ''' <summary> 
    ''' Return an instance of the encrypted license provider used to generate and validate keys 
    ''' </summary> 
    ''' <returns>A new instance of an <see cref="EncryptedLicenseProvider"/></returns> 
    ''' <remarks> 
    ''' Override this method to use a custom <see cref="EncryptedLicenseProvider"/> 
    ''' </remarks> 
    Protected Overridable Function GetEncryptedLicenseProvider() As EncryptedLicenseProvider
        Return New EncryptedLicenseProvider()
    End Function

    ''' <summary> 
    ''' Return the directory used to store license files 
    ''' </summary> 
    ''' <param name="context">The licence context</param> 
    ''' <param name="type">The type being licensed</param> 
    ''' <returns>The directory to look for license files</returns> 
    Protected Overridable Function GetLicenseDirectory(ByVal context As LicenseContext, ByVal type As Type) As String
        Return LicenseUtilities.DefaultLicenseDirectory(context, type)
    End Function

    ''' <summary> 
    ''' Called by <see cref="GetLicense"/> to get the file path to obtain the license from (if there is no runtime license saved in the context) 
    ''' </summary> 
    ''' <remarks> 
    ''' This can be overridden to change the file used to store the design time license for the provider. By default the 
    ''' the license file is stored in the same directory as the component executable with the name based on the fully 
    ''' qualified type name eg MyNamespace.MyControl.lic 
    ''' </remarks> 
    ''' <param name="context">The licence context</param> 
    ''' <param name="type">The type to get the license for</param> 
    ''' <returns>The path of the license file</returns> 
    Protected Overridable Function GetLicenseFilePath(ByVal context As LicenseContext, ByVal type As Type) As String
        Dim dir As String = GetLicenseDirectory(context, type)
        Return [String].Format("{0}\{1}.lic", dir, type.FullName)
    End Function

    ''' <summary>
    ''' Write the given authentication data to the XML writer
    ''' </summary>
    ''' <param name="writer">The writer to use</param>
    ''' <param name="authenticationData">The data to write</param>
    ''' <remarks>
    ''' If the authenticationData is XML (ie the string starts with a &lt;) then the data is formatted
    ''' as XML otherwise it is written as content
    ''' </remarks>
    Protected Overridable Sub WriteAuthenticationData(ByVal writer As XmlTextWriter, ByVal authenticationData As String)
        Dim isXml As Boolean = authenticationData.StartsWith("<")
        If isXml Then
            Dim doc As New XmlDocument()
            Dim xml As String = String.Format("<AuthenticationData xml=""True"">{0}</AuthenticationData>", authenticationData)
            doc.LoadXml(xml)
            doc.WriteTo(writer)
        Else
            writer.WriteElementString("AuthenticationData", authenticationData)
        End If
    End Sub

    ''' <summary>
    ''' Read authentication data from the given XML reader
    ''' </summary>
    ''' <param name="reader">The reader to use</param>
    ''' <returns>The authenticationData string</returns>
    Protected Overridable Function ReadAuthenticationData(ByVal reader As XmlTextReader) As String
        Dim result As String = Nothing
        Dim isXml As Boolean = reader.GetAttribute("xml") = Boolean.TrueString
        If isXml Then
            result = reader.ReadInnerXml().Trim()
        Else
            result = reader.ReadElementString()
        End If
        Return result
    End Function

    ''' <summary> 
    ''' Display an error to a message box or the trace output 
    ''' </summary> 
    ''' <param name="title">The title for the error</param> 
    ''' <param name="message">The error message</param> 
    Protected Overridable Sub ShowError(ByVal title As String, ByVal message As String)
        Dim productName As String
        If _parameters Is Nothing Then
            productName = LicenseResources.UnknownProductTxt
        Else
            productName = _parameters.ProductName
        End If
        LicenseUtilities.ShowError(String.Format(title, productName), message)
    End Sub

    ''' <summary>
    ''' Strip the leading "www." of host names (if present) and return lowercase
    ''' </summary>
    ''' <param name="hostName">The name to fix</param>
    ''' <returns>Returns the host name minus www. prefix</returns>
    Protected Overridable Function FixHostName(ByVal hostName As String) As String
        If hostName IsNot Nothing Then
            hostName = hostName.ToLowerInvariant()
            If hostName.StartsWith("www.", StringComparison.InvariantCulture) Then
                hostName = hostName.Substring(4)
            End If
        End If
        Return hostName
    End Function

#End Region

End Class