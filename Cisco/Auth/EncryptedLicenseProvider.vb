' 
' FILE: EncryptedLicenseProvider.vb. 
' 
' COPYRIGHT: Copyright 2008 
' Infralution 
' 
Imports System
Imports System.ComponentModel
Imports System.ComponentModel.Design
Imports System.Security.Cryptography
Imports System.Diagnostics
Imports System.Text
Imports System.IO
Imports System.Xml
Imports System.Globalization
Imports System.Reflection
Imports System.Collections
Imports System.Security

''' <summary> 
''' The parameters used to generate and validate <see cref="EncryptedLicense">EncryptedLicenses</see> 
''' using an <see cref="EncryptedLicenseProvider"/> 
''' </summary> 
''' <seealso cref="EncryptedLicenseProvider"/> 
#If ILS_PUBLIC_CLASS Then
Public Class EncryptedLicenseParameters
#Else
Class EncryptedLicenseParameters
#End If

    Private _productName As String
    Private _productPassword As String
    Private _keyStrength As Integer = 7
    Private _checksumProductInfo As Boolean = False
    Private _textEncoding As TextEncoding = TextEncoding.Hex
    Private _shortSerialNo As Boolean = True
    Private _rsaProvider As RSACryptoServiceProvider = LicenseUtilities.CreateRSACryptoServiceProvider()
    Private _designSignature As Byte()
    Private _runtimeSignature As Byte()

    ''' <summary> 
    ''' The name of the product being licensed 
    ''' </summary> 
    Public Property ProductName() As String
        Get
            Return _productName
        End Get
        Set(ByVal value As String)
            _productName = value
        End Set
    End Property

    ''' <summary> 
    ''' The password used to encrypt the license data 
    ''' </summary> 
    ''' <remarks> 
    ''' The <see cref="KeyStrength"/> determines the number of characters of the ProductPassword that are 
    ''' actually used in generating keys. If the ProductPassword is shorter than the <see cref="KeyStrength"/> then 
    ''' it is padded. 
    ''' </remarks> 
    Public Property ProductPassword() As String
        Get
            Return _productPassword
        End Get
        Set(ByVal value As String)
            If value <> _productPassword Then
                _productPassword = value

                ' if the password is changed then also use a new RSA key for validating and
                ' force the signatures to be recreated
                '
                _rsaProvider = LicenseUtilities.CreateRSACryptoServiceProvider()
                _designSignature = Nothing
                _runtimeSignature = Nothing
            End If
        End Set
    End Property

    ''' <summary> 
    ''' The strength of the key to generate. 
    ''' </summary> 
    ''' <remarks> 
    ''' The KeyStrength determines the number of characters of the <see cref="ProductPassword"/> that are 
    ''' actually used in generating keys. The smaller the KeyStrength the shorter the generated keys. 
    ''' If the <see cref="ProductPassword"/> is shorter than the KeyStrength then it is padded. 
    ''' </remarks> 
    Public Property KeyStrength() As Integer
        Get
            Return _keyStrength
        End Get
        Set(ByVal value As Integer)
            If value < 7 OrElse value > 63 Then
                Throw New ArgumentOutOfRangeException("KeyStrength", "KeyStrength must be in the range 7 to 63")
            End If
            If value <> _keyStrength Then

                ' force the signatures to be recreated
                '
                _designSignature = Nothing
                _runtimeSignature = Nothing
                _keyStrength = value
            End If
        End Set
    End Property

    ''' <summary> 
    ''' The encoding used to convert the binary key to text 
    ''' </summary> 
    Public Property TextEncoding() As TextEncoding
        Get
            Return _textEncoding
        End Get
        Set(ByVal value As TextEncoding)
            _textEncoding = value
        End Set
    End Property

    ''' <summary> 
    ''' Should a checksum of the <see cref="EncryptedLicense.ProductInfo"/> be included in the key 
    ''' </summary> 
    ''' <remarks> 
    ''' If true a checksum is included in generated keys to check that the contents of the 
    ''' <see cref="EncryptedLicense.ProductInfo"/> are valid. This is only necessary if the 
    ''' ProductInfo is potentially more than 6 characters long. For ProductInfo of less than 7 
    ''' characters the block encryption algorithm used to encrypt the overally key guarantees the validity 
    ''' of the ProductInfo. 
    ''' </remarks> 
    Public Property ChecksumProductInfo() As Boolean
        Get
            Return _checksumProductInfo
        End Get
        Set(ByVal value As Boolean)
            _checksumProductInfo = value
        End Set
    End Property

    ''' <summary> 
    ''' If true serial numbers must be less than <see cref="UInt16.MaxValue"/>. 
    ''' </summary> 
    ''' <remarks> 
    ''' Setting this to true enables the generated key to be kept as short as possible. The default 
    ''' value for backward compatibility with previous versions is true. 
    ''' </remarks> 
    Public Property ShortSerialNo() As Boolean
        Get
            Return _shortSerialNo
        End Get
        Set(ByVal value As Boolean)
            _shortSerialNo = value
        End Set
    End Property

    ''' <summary> 
    ''' Return the maximum serial no. 
    ''' </summary> 
    ''' <remarks> 
    ''' This returns the maximum allowed serial no based on the value of the <see cref="ShortSerialNo"/> 
    ''' property. 
    ''' </remarks> 
    Public ReadOnly Property MaxSerialNo() As Integer
        Get
            If _shortSerialNo Then
                Return UInt16.MaxValue
            Else
                Return Int32.MaxValue
            End If
        End Get
    End Property

    ''' <summary> 
    ''' Return the RSA Provider used to validate RSA signatures 
    ''' </summary> 
    Friend ReadOnly Property RSAProvider() As RSACryptoServiceProvider
        Get
            Return _rsaProvider
        End Get
    End Property

    ''' <summary> 
    ''' The RSA signature for the product password at design time 
    ''' </summary> 
    Friend Property DesignSignature() As Byte()
        Get
            If _designSignature Is Nothing Then
                EncryptedLicenseProvider.CreateSignatures(Me)
            End If
            Return _designSignature
        End Get
        Set(ByVal value As Byte())
            _designSignature = value
        End Set
    End Property

    ''' <summary> 
    ''' The RSA signature for the product password at runtime 
    ''' </summary> 
    Friend Property RuntimeSignature() As Byte()
        Get
            If _runtimeSignature Is Nothing Then
                EncryptedLicenseProvider.CreateSignatures(Me)
            End If
            Return _runtimeSignature
        End Get
        Set(ByVal value As Byte())
            _runtimeSignature = value
        End Set
    End Property

    ''' <summary> 
    ''' Read the parameters from an XML string 
    ''' </summary> 
    ''' <param name="xmlParameters"></param> 
    Public Overridable Sub ReadFromString(ByVal xmlParameters As String)
        Dim reader As XmlReader = New XmlTextReader(xmlParameters, XmlNodeType.Element, Nothing)
        Read(reader)
        reader.Close()
    End Sub

    ''' <summary> 
    ''' Write the parameters to an XML string 
    ''' </summary> 
    ''' <param name="includeGenerationParameters">Should parameters required for generating keys be included</param> 
    ''' <returns>The parameters in a formatted XML string</returns> 
    Public Overridable Function WriteToString(ByVal includeGenerationParameters As Boolean) As String
        Dim stringWriter As New StringWriter()
        Dim xmlWriter As New XmlTextWriter(stringWriter)
        Write(xmlWriter, includeGenerationParameters)
        xmlWriter.Close()
        Dim result As String = stringWriter.ToString()
        stringWriter.Close()
        Return result
    End Function


    ''' <summary> 
    ''' Read the parameters from an XML Reader 
    ''' </summary> 
    ''' <param name="reader"></param> 
    Public Overridable Sub Read(ByVal reader As XmlReader)
        reader.ReadStartElement()
        While reader.IsStartElement()
            Select Case reader.Name
                Case "ProductName"
                    _productName = reader.ReadElementString()
                    Exit Select
                Case "ProductPassword"
                    _productPassword = reader.ReadElementString()
                    Exit Select
                Case "RSAKeyValue"
                    LicenseUtilities.ReadRSAParameters(_rsaProvider, reader, "RSAKeyValue")
                    Exit Select
                Case "DesignSignature"
                    _designSignature = LicenseUtilities.ReadElementBase64(reader, "DesignSignature")
                    Exit Select
                Case "RuntimeSignature"
                    _runtimeSignature = LicenseUtilities.ReadElementBase64(reader, "RuntimeSignature")
                    Exit Select
                Case "KeyStrength"
                    _keyStrength = Integer.Parse(reader.ReadElementString(), CultureInfo.InvariantCulture)
                    Exit Select
                Case "ChecksumProductInfo"
                    _checksumProductInfo = Boolean.Parse(reader.ReadElementString())
                    Exit Select
                Case "TextEncoding"
                    _textEncoding = DirectCast([Enum].Parse(GetType(TextEncoding), reader.ReadElementString()), TextEncoding)
                    Exit Select
                Case "ShortSerialNo"
                    _shortSerialNo = Boolean.Parse(reader.ReadElementString())
                    Exit Select
                Case Else
                    Dim [error] As String = "Unexpected XML Element: {0}"
                    Throw New XmlSyntaxException(String.Format([error], reader.Name))
            End Select
        End While
        reader.ReadEndElement()
    End Sub

    ''' <summary> 
    ''' Write the parameters to an XML Writer 
    ''' </summary> 
    ''' <param name="writer">The XML Writer to write to</param> 
    ''' <param name="includeGenerationParameters">Should parameters required for generating keys be included</param> 
    Public Overridable Sub Write(ByVal writer As XmlWriter, ByVal includeGenerationParameters As Boolean)
        writer.WriteStartElement("EncryptedLicenseParameters")
        writer.WriteElementString("ProductName", ProductName)
        If includeGenerationParameters Then
            writer.WriteElementString("ProductPassword", ProductPassword)
        End If
        LicenseUtilities.WriteRSAParameters(RSAProvider, writer, "RSAKeyValue", includeGenerationParameters)
        writer.WriteElementString("DesignSignature", Convert.ToBase64String(DesignSignature))
        writer.WriteElementString("RuntimeSignature", Convert.ToBase64String(RuntimeSignature))
        writer.WriteElementString("KeyStrength", KeyStrength.ToString(CultureInfo.InvariantCulture))
        If ChecksumProductInfo Then
            writer.WriteElementString("ChecksumProductInfo", ChecksumProductInfo.ToString(CultureInfo.InvariantCulture))
        End If
        If TextEncoding <> TextEncoding.Hex Then
            writer.WriteElementString("TextEncoding", TextEncoding.ToString())
        End If
        If Not ShortSerialNo Then
            writer.WriteElementString("ShortSerialNo", ShortSerialNo.ToString(CultureInfo.InvariantCulture))
        End If
        writer.WriteEndElement()
    End Sub

End Class

''' <summary> 
''' Defines a .NET LicenseProvider that generates and validates simple, secure 
''' <see cref="EncryptedLicense">EncryptedLicenses</see>. 
''' </summary> 
''' <remarks> 
''' The EncryptedLicenseProvider generates simple license keys which are validated using 
''' a public key encryption algorithm to minimize the possibility of cracking. See 
''' <see href="b617d141-c6c9-4d86-b93e-b049fc12fd72.htm" target="_self">Getting Started</see>
''' for detailed information on using EncryptedLicenseProvider to license applications and components.
''' </remarks> 
''' <seealso cref="EncryptedLicense"/> 
#If ILS_PUBLIC_CLASS Then
Public Class EncryptedLicenseProvider
    Inherits LicenseProvider
#Else
Class EncryptedLicenseProvider
    Inherits LicenseProvider
#End If

#Region "Member Variables"

    Private Const Base32Chars As String = "23456789ABCDEFGHJKLMNPQRSTUVWXYZ"

    Private Shared _desKey As Byte() = New Byte() {146, 21, 56, 161, 18, 237, 179, 194}
    Private Shared _desIV As Byte() = New Byte() {173, 63, 198, 17, 71, 144, 221, 161}

    ''' <summary> 
    ''' The current parameters for validating licenses 
    ''' </summary> 
    Private Shared _parameters As EncryptedLicenseParameters

#End Region

#Region "Public Interface"

    ''' <summary> 
    ''' Set the parameters used to validate licenses created by this provider. 
    ''' </summary> 
    ''' <remarks> 
    ''' This must be called by the client software prior to obtaining licenses using the EncryptedLicenseProvider. 
    ''' The XML parameter string is generated using License Tracker and pasted into the calling client code 
    ''' or by calling <see cref="EncryptedLicenseParameters.WriteToString"/> 
    ''' </remarks> 
    ''' <param name="xmlParameters">An XML string containing parameters used to validate licenses</param> 
    Public Shared Sub SetParameters(ByVal xmlParameters As String)
        Dim parameters As New EncryptedLicenseParameters()
        parameters.ReadFromString(xmlParameters)
        _parameters = parameters
    End Sub

    ''' <summary> 
    ''' Set/Get Parameters for validating <see cref="EncryptedLicense">EncryptedLicenses</see> 
    ''' </summary> 
    Public Shared Property Parameters() As EncryptedLicenseParameters
        Get
            Return _parameters
        End Get
        Set(ByVal value As EncryptedLicenseParameters)
            _parameters = value
        End Set
    End Property

    ''' <summary> 
    ''' Generate a new encrypted license using the given parameters 
    ''' </summary> 
    ''' <param name="parameters">The license parameters to use to generate the key</param> 
    ''' <param name="productInfo">User defined data to be included in license key</param> 
    ''' <param name="serialNo">The unique license serial number for the</param> 
    ''' <returns>An encrypted license key</returns> 
    ''' <remarks> 
    ''' If there is no installed license for the Infralution Licensing System then the only 
    ''' allowed password is "TEST" and the only allowed serial numbers are 1 or 0. 
    ''' </remarks> 
    Public Overridable Function GenerateKey(ByVal parameters As EncryptedLicenseParameters, ByVal productInfo As String, ByVal serialNo As Int32) As String
        If parameters Is Nothing Then
            Throw New ArgumentNullException("parameters")
        End If
        If parameters.ProductPassword Is Nothing Then
            Throw New InvalidOperationException("Parameters.ProductPassword MUST be non-null")
        End If
        If productInfo Is Nothing Then
            productInfo = ""
        End If

        Dim passwordData As Byte() = GetPasswordData(parameters.ProductPassword, parameters.KeyStrength)
        Dim productInfoData As Byte() = ASCIIEncoding.UTF8.GetBytes(productInfo)

#If ILS_CHECK_LICENSE Then

        ' if the Licensing System is not licensed then we need to check the password 
        ' 
        If LicenseUtilities.ILSLicense Is Nothing Then
            Const passwordErrorMsg As String = "The only allowable password in evaluation mode is 'TEST'"

            If parameters.ProductPassword <> "TEST" Then
                Throw New LicenseException(GetType(EncryptedLicenseProvider), Me, passwordErrorMsg)
            End If
        End If

#End If

        Return GenerateKey(parameters, passwordData, productInfoData, serialNo)
    End Function

    ''' <summary> 
    ''' Generate a runtime license key from the given design time license key 
    ''' </summary> 
    ''' <param name="designTimeLicenseKey">The design time license key to use</param> 
    ''' <returns>A runtime license key (or null if the designTimeLicenseKey can't be validated)</returns> 
    ''' <remarks> 
    ''' The <see cref="SetParameters"/> method MUST be called before using this method. 
    ''' </remarks> 
    Public Overridable Function GenerateRuntimeKey(ByVal designTimeLicenseKey As String) As String
        Dim runtimeLicenseKey As String = Nothing
        ValidateLicenseKey(designTimeLicenseKey, LicenseUsageMode.Designtime, True, runtimeLicenseKey)
        Return runtimeLicenseKey
    End Function

    ''' <summary> 
    ''' Install a license key for the given component or control type. 
    ''' </summary> 
    ''' <remarks> 
    ''' This method is used to install licenses for components and controls. The <see cref="InstallLicense"/> 
    ''' method is typically better for installing application licenses because it provides more control over the 
    ''' license key file name. This license key file used by this method is the full type name followed by a ".lic" suffix. 
    ''' </remarks> 
    ''' <param name="type">The type to install the license for</param> 
    ''' <param name="license">The license to install</param> 
    Public Overridable Sub InstallLicense(ByVal type As Type, ByVal license As EncryptedLicense)
        If license Is Nothing Then
            Throw New ArgumentNullException("license")
        End If
        Dim licenseFile As String = GetLicenseFilePath(LicenseManager.CurrentContext, type)
        WriteKeyToFile(licenseFile, license.LicenseKey)
    End Sub

    ''' <summary> 
    ''' Install a license key for the given component or control type. 
    ''' </summary> 
    ''' <remarks> 
    ''' Validates the given license key and then installs the license. 
    ''' This method is an alternative to calling <see cref="ValidateLicenseKey"/> and then 
    ''' <see cref="InstallLicense"/>. 
    ''' </remarks> 
    ''' <param name="type">The type to install the license for</param> 
    ''' <param name="licenseKey">The license key to install</param> 
    ''' <returns>A license if succesful or null/nothing if not</returns> 
    Public Overridable Function InstallLicense(ByVal type As Type, ByVal licenseKey As String) As EncryptedLicense
        Dim license As EncryptedLicense = ValidateLicenseKey(licenseKey)
        If license IsNot Nothing Then
            InstallLicense(type, license)
        End If
        Return license
    End Function

    ''' <summary> 
    ''' Install a license key for an application in the given file. 
    ''' </summary> 
    ''' <remarks> 
    ''' This method is used to install licenses for applications. Use the <see cref="InstallLicense"/> 
    ''' method to install licenses for components or controls. If a full path is not specified for licenseFile then 
    ''' the file will be created relative to the entry executable directory. 
    ''' </remarks> 
    ''' <param name="licenseFile">The name of the file to install the license key in</param> 
    ''' <param name="license">The license to install</param> 
    Public Overridable Sub InstallLicense(ByVal licenseFile As String, ByVal license As EncryptedLicense)
        If license Is Nothing Then
            Throw New ArgumentNullException("license")
        End If
        Dim baseDir As String = GetLicenseDirectory(LicenseManager.CurrentContext, Nothing)
        Dim path As String = System.IO.Path.Combine(baseDir, licenseFile)
        WriteKeyToFile(path, license.LicenseKey)
    End Sub

    ''' <summary> 
    ''' Install a license key for an application in the given file. 
    ''' </summary> 
    ''' <remarks> 
    ''' Validates the given license key and then installs the license. 
    ''' This method is an alternative to calling <see cref="ValidateLicenseKey"/> and then 
    ''' <see cref="InstallLicense"/>. 
    ''' </remarks> 
    ''' <param name="licenseFile">The name of the file to install the license key in</param> 
    ''' <param name="licenseKey">The license key to install</param> 
    ''' <returns>A license if succesful or null/nothing if not</returns> 
    Public Overridable Function InstallLicense(ByVal licenseFile As String, ByVal licenseKey As String) As EncryptedLicense
        Dim license As EncryptedLicense = ValidateLicenseKey(licenseKey)
        If license IsNot Nothing Then
            InstallLicense(licenseFile, license)
        End If
        Return license
    End Function

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
    ''' Check that the given license key is valid 
    ''' </summary> 
    ''' <param name="licenseKey">The license key to validate</param> 
    ''' <param name="context">The current licensing context</param> 
    ''' <param name="type">The type to be licensed</param> 
    ''' <returns>An <see cref="EncryptedLicense"/> or null if licenseKey is not valid</returns> 
    ''' <remarks>
    ''' <para>
    ''' This method is called to validate the licence key for a type.  If the license context is a design
    ''' time context then it generates a runtime license key and saves it in the context.
    ''' </para>
    ''' <para>
    ''' The <see cref="SetParameters"/> method MUST be called before using this method.  
    ''' </para>
    ''' </remarks>
    Public Overridable Function ValidateLicenseKey(ByVal licenseKey As String, ByVal context As LicenseContext, ByVal type As Type) As EncryptedLicense
        Dim runtimeLicenseKey As String = Nothing
        Dim generateRuntimeLicenseKey As Boolean = (context.UsageMode = LicenseUsageMode.Designtime AndAlso type IsNot Nothing)
        Dim license As EncryptedLicense = ValidateLicenseKey(licenseKey, context.UsageMode, generateRuntimeLicenseKey, runtimeLicenseKey)
        If runtimeLicenseKey IsNot Nothing Then
            ' save the runtime key into the context 
            ' 
            context.SetSavedLicenseKey(type, runtimeLicenseKey)
        End If
        Return license
    End Function

    ''' <summary> 
    ''' Validate that the given license key is valid for the current licensing parameters 
    ''' </summary> 
    ''' <param name="licenseKey">The license key to validate</param> 
    ''' <returns>The encrypted license if the key is valid otherwise null</returns> 
    ''' <remarks> 
    ''' <para> 
    ''' This method provides a mechanism to validate that a given license key is valid 
    ''' prior to attempting to install it. This can be useful if you want to check 
    ''' the <see cref="EncryptedLicense.ProductInfo"/> before installing the license. 
    ''' </para> 
    ''' <para> 
    ''' The <see cref="SetParameters"/> method MUST be called before using this method. 
    ''' </para> 
    ''' </remarks> 
    ''' <seealso cref="ValidateLicenseKey"/> 
    Public Function ValidateLicenseKey(ByVal licenseKey As String) As EncryptedLicense
        Dim runtimeLicenseKey As String = Nothing
        Return ValidateLicenseKey(licenseKey, LicenseManager.CurrentContext.UsageMode, False, runtimeLicenseKey)
    End Function

    ''' <summary> 
    ''' Validate that the given license key is valid for the given licensing parameters 
    ''' </summary> 
    ''' <param name="licenseParameters">An XML string containing parameters used to validate the license key</param> 
    ''' <param name="licenseKey">The license key to validate</param> 
    ''' <returns>The encrypted license if the key is valid otherwise null</returns> 
    ''' <remarks> 
    ''' <para> 
    ''' This method provides a mechanism to validate that a given license key is valid 
    ''' prior to attempting to install it. This can be useful if you want to check 
    ''' the <see cref="EncryptedLicense.ProductInfo"/> before installing the license. 
    ''' </para> 
    ''' <para> 
    ''' This method is an alternative to calling <see cref="SetParameters"/> followed by 
    ''' <see cref="ValidateLicenseKey"/>. 
    ''' </para> 
    ''' </remarks> 
    Public Function ValidateLicenseKey(ByVal licenseParameters As String, ByVal licenseKey As String) As EncryptedLicense
        SetParameters(licenseParameters)
        Return ValidateLicenseKey(licenseKey)
    End Function

    ''' <summary> 
    ''' Get a license (if installed) from the given license file. 
    ''' </summary> 
    ''' <param name="licenseFile">The name of the license file containing the license key</param> 
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
    Public Overridable Overloads Function GetLicense(ByVal licenseFile As String) As EncryptedLicense
        Dim dir As String = GetLicenseDirectory(LicenseManager.CurrentContext, Nothing)
        Dim path As String = System.IO.Path.Combine(dir, licenseFile)
        Dim licenseKey As String = ReadKeyFromFile(path)
        Return ValidateLicenseKey(licenseKey)
    End Function

    ''' <summary> 
    ''' Get a license (if installed) from the given license file. 
    ''' </summary> 
    ''' <param name="licenseParameters">An XML string containing parameters used to validate the license key</param> 
    ''' <param name="licenseFile">The name of the license file containing the license key</param> 
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
    Public Overloads Function GetLicense(ByVal licenseParameters As String, ByVal licenseFile As String) As EncryptedLicense
        SetParameters(licenseParameters)
        Return GetLicense(licenseFile)
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
        Dim license As EncryptedLicense = Nothing
        If context.UsageMode = LicenseUsageMode.Runtime Then
            Try
                Dim key As String = context.GetSavedLicenseKey(type, Nothing)
                license = ValidateLicenseKey(key, context, type)
                ' if something goes wrong retrieving the saved license key then just ignore it 
                ' and try reading from file 
            Catch
            End Try
        End If

        If license Is Nothing Then
            ' if we're in design mode or a suitable license key wasn't found in 
            ' the runtime context try to read a license from file 
            ' 
            Dim key As String = ReadKeyFromFile(GetLicenseFilePath(context, type))
            license = ValidateLicenseKey(key, context, type)
        End If

        If license Is Nothing AndAlso allowExceptions Then
            Throw New LicenseException(type, instance, "No License Installed")
        End If
        Return license
    End Function

    ''' <summary> 
    ''' Return the license for the given type from a given DLL assembly resources 
    ''' </summary> 
    ''' <param name="context">The license context to validate the license in</param> 
    ''' <param name="assembly">The assembly containing the license</param> 
    ''' <param name="type">The type to get the license for</param> 
    ''' <returns>The license key if any</returns> 
    ''' <remarks> 
    ''' This method can be used to check the given DLL assembly for a license. By default the .NET licensing 
    ''' framework only checks the entry assembly (ie typically executables) for licenses. This means 
    ''' that if a licensed control is wrapped in another control, the customer of the wrapped control will 
    ''' still required a design time license key for the original control. This is generally the behaviour 
    ''' that control authors would like. If however you want to provide a license that enables a customer 
    ''' to create new component/controls using your control/component then you can achieve this by using 
    ''' this method to check for a license in the CallingAssembly that created the control/component. 
    ''' </remarks> 
    Public Overridable Overloads Function GetLicense(ByVal context As LicenseContext, ByVal assembly As Assembly, ByVal type As Type) As EncryptedLicense
        If assembly Is Nothing Then
            Return Nothing
        End If
        Dim licenseKey As String = LicenseUtilities.GetSavedLicenseKey(assembly, type)
        Return ValidateLicenseKey(licenseKey, context, type)
    End Function

#End Region

#Region "Local Methods"

    ''' <summary> 
    ''' Converts a byte array into a text representation. 
    ''' </summary> 
    ''' <param name="data">The byte data to convert</param> 
    ''' <param name="encoding">The encoding to use</param> 
    ''' <returns>Text representation of the data</returns> 
    Protected Friend Overridable Function EncodeToText(ByVal data As Byte(), ByVal encoding As TextEncoding) As String
        ' encrypt the overall license key using the preset encryption key to obscure the password 
        ' 
        Dim des As New DESCryptoServiceProvider()
        des.Key = _desKey
        des.IV = _desIV
        Dim encData As Byte() = des.CreateEncryptor().TransformFinalBlock(data, 0, data.Length)
        Return LicenseUtilities.EncodeToText(encData, encoding)
    End Function

    ''' <summary> 
    ''' Converts a string into a byte array. 
    ''' </summary> 
    ''' <param name="text">The text to convert</param> 
    ''' <param name="encoding">The encoding to use</param> 
    ''' <returns>The converted byte data</returns> 
    Protected Friend Overridable Function DecodeFromText(ByVal text As String, ByVal encoding As TextEncoding) As Byte()
        Dim encData As Byte() = LicenseUtilities.DecodeFromText(text, encoding)

        ' decrypt the overall license key using the preset encryption key 
        ' 
        Dim des As New DESCryptoServiceProvider()
        des.Key = _desKey
        des.IV = _desIV
        Return des.CreateDecryptor().TransformFinalBlock(encData, 0, encData.Length)
    End Function

    ''' <summary> 
    ''' Generate the password data used to verify and decrypt the license 
    ''' </summary> 
    ''' <param name="password">The password used to generate the key</param> 
    ''' <param name="keyStrength">The strength of the key to create</param> 
    ''' <returns>The password data used to verify and decrypt the license</returns> 
    Private Shared Function GetPasswordData(ByVal password As String, ByVal keyStrength As Integer) As Byte()
        Dim key As Byte() = New Byte() {242, 161, 3, 157, 99, 135, _
        53, 94}
        Dim iv As Byte() = New Byte() {171, 184, 148, 126, 29, 229, _
        209, 51}

        Dim des As New DESCryptoServiceProvider()
        des.Key = key
        des.IV = iv

        Dim padLength As Integer = Math.Max(keyStrength, 8)
        If password.Length < padLength Then
            password = password.PadRight(padLength, "*"c)
        End If
        Dim data As Byte() = ASCIIEncoding.ASCII.GetBytes(password)
        Dim encPassword As Byte() = des.CreateEncryptor().TransformFinalBlock(data, 0, data.Length)
        Dim result As Byte() = New Byte(LicenseUtilities.ArraySize(keyStrength) - 1) {}
        Array.Copy(encPassword, 0, result, 0, keyStrength)
        Return result
    End Function

    ''' <summary> 
    ''' Pad the given password if required. 
    ''' </summary> 
    ''' <param name="passwordData">The password data to pad</param> 
    ''' <returns>The padded password data</returns> 
    ''' <remarks> 
    ''' This function is required for backward compatibility with 7 byte passwords which were 
    ''' padded before being signed 
    ''' </remarks> 
    Private Shared Function PadPassword(ByVal passwordData As Byte()) As Byte()
        If passwordData.Length = 7 Then
            Dim result As Byte() = New Byte(LicenseUtilities.ArraySize(8) - 1) {}
            Array.Copy(passwordData, 0, result, 0, passwordData.Length)
            Return result
        End If
        Return passwordData
    End Function

    ''' <summary> 
    ''' Update the signatures when the Product Password is changed 
    ''' </summary> 
    Friend Shared Sub CreateSignatures(ByVal parameters As EncryptedLicenseParameters)
        Dim designPassword As Byte() = PadPassword(GetPasswordData(parameters.ProductPassword, parameters.KeyStrength))
        parameters.DesignSignature = LicenseUtilities.SignData(parameters.RSAProvider, designPassword)

        ' encrypt the password using itself to produce the runtime password 
        ' 
        Dim encryptionKey As Byte() = New Byte(LicenseUtilities.ArraySize(8) - 1) {}
        Array.Copy(designPassword, 0, encryptionKey, 0, 7)

        Dim des As New DESCryptoServiceProvider()
        des.Key = _desKey
        des.IV = encryptionKey
        Dim encKey As Byte() = des.CreateEncryptor().TransformFinalBlock(designPassword, 0, designPassword.Length)

        Dim runtimePassword As Byte() = New Byte(LicenseUtilities.ArraySize(parameters.KeyStrength) - 1) {}
        Array.Copy(encKey, 0, runtimePassword, 0, parameters.KeyStrength)
        runtimePassword = PadPassword(runtimePassword)

        ' sign the runtime key 
        ' 
        parameters.RuntimeSignature = LicenseUtilities.SignData(parameters.RSAProvider, runtimePassword)

    End Sub

    ''' <summary> 
    ''' Generate a new encrypted license 
    ''' </summary> 
    ''' <param name="parameters">The license parameters to use to generate the key</param> 
    ''' <param name="productPassword">The password used to encrypted the license data</param> 
    ''' <param name="productInfo">User defined data to be included in license key</param> 
    ''' <param name="serialNo">The unique license serial number</param> 
    ''' <returns>An encrypted license key</returns> 
    ''' <remarks> 
    ''' If there is no installed license for the Infralution Licensing System then the only 
    ''' allowed password is "TEST" and the only allowed serial numbers are 1 or 0. To use the 
    ''' licensed version of this method ensure that the file Infralution.Licensing.EncryptedLicenseProvider.lic 
    ''' exists in the same directory as the Infralution.Licensing.dll and contains a valid 
    ''' license key for the Licensing System. 
    ''' </remarks> 
    Private Function GenerateKey(ByVal parameters As EncryptedLicenseParameters, ByVal productPassword As Byte(), ByVal productInfo As Byte(), ByVal serialNo As Int32) As String

        ' Public Key token for the Infralution signed assemblies 
        ' 
        Dim requiredToken As Byte() = {62, 126, 142, 55, 68, 165, 193, 63}
        Dim publicKeyToken As Byte() = Assembly.GetExecutingAssembly().GetName().GetPublicKeyToken()

#If ILS_STRONGNAME Then

        ' Validate this assembly - if it isn't signed with the correct public key 
        ' then copy rubbish into the key. This is to make it just a little more 
        ' difficult for the casual hacker. 
        ' 
        If Not LicenseUtilities.ArrayEqual(publicKeyToken, requiredToken) Then
            _desKey.CopyTo(productPassword, 0)
        End If
#End If

        Dim checksumLength As Integer = 0
        If parameters.ChecksumProductInfo Then checksumLength = 2
        Dim serialNoLength As Integer = 4
        If parameters.ShortSerialNo Then serialNoLength = 2

        Dim clientData As Byte()
        If serialNo < 0 Then
            Throw New ArgumentOutOfRangeException("serialNo", "serialNo must be non-negative")
        End If
        If parameters.ShortSerialNo Then
            If serialNo > UInt16.MaxValue Then
                Throw New ArgumentOutOfRangeException("serialNo", "serialNo must be less than 65536")
            End If
            Dim userialNo As UInt16 = Convert.ToUInt16(serialNo)
            clientData = BitConverter.GetBytes(userialNo)
        Else
            clientData = BitConverter.GetBytes(serialNo)
        End If

        Dim payload As Byte() = New Byte(LicenseUtilities.ArraySize(productInfo.Length + serialNoLength + checksumLength) - 1) {}

        clientData.CopyTo(payload, 0)
        productInfo.CopyTo(payload, serialNoLength)

        ' calculate the product data checksum and add to the payload 
        ' 
        If parameters.ChecksumProductInfo Then
            Dim checksum As UInt16 = LicenseUtilities.Checksum(productInfo)
            Dim checksumData As Byte() = BitConverter.GetBytes(checksum)
            checksumData.CopyTo(payload, payload.Length - checksumLength)
        End If

        ' Encrypt the payload. The key used for encrypting the payload is just the first 7 bytes of the password data 
        ' 
        Dim encryptionKey As Byte() = New Byte(LicenseUtilities.ArraySize(8) - 1) {}
        Array.Copy(productPassword, 0, encryptionKey, 0, 7)
        Dim des As New DESCryptoServiceProvider()
        des.Key = _desKey
        des.IV = encryptionKey
        Dim encPayload As Byte() = des.CreateEncryptor().TransformFinalBlock(payload, 0, payload.Length)

        ' Combine the password data and encrypted payload 
        ' 
        Dim data As Byte() = New Byte(LicenseUtilities.ArraySize(productPassword.Length + encPayload.Length) - 1) {}

        ' For key strengths greater than 8 we swap the order of the payload and password data 
        ' This ensures that the first 8 bytes do not always end up hex encoded the same 
        ' 
        If parameters.KeyStrength < 8 Then
            productPassword.CopyTo(data, 0)
            encPayload.CopyTo(data, productPassword.Length)
        Else
            encPayload.CopyTo(data, 0)
            productPassword.CopyTo(data, encPayload.Length)
        End If

        ' return the data encoded as a string 
        ' 
        Return EncodeToText(data, parameters.TextEncoding)
    End Function

    '' <summary>
    '' Check that the given license key is valid and optionally generate a runtime license key
    '' </summary>
    '' <param name="licenseKey">The license key to validate</param>
    '' <param name="usageMode">The usage mode that we want to validate the license key for</param>
    '' <param name="generateRuntimeLicenseKey">Should a runtime license be generated from the license - usageMode must also be DesignTime</param>
    '' <param name="runtimeLicenseKey">The generated runtime license (if any)</param>
    '' <returns>An <see cref="EncryptedLicense"/> or null if licenseKey is not valid</returns>
    '' <remarks>
    '' <para>
    '' This method implements the core validation logic (other ValidateLicenseKey methods call it) and optionally
    '' generates a runtime license key.
    '' </para>
    '' <para>
    '' The <see cref="SetParameters"/> method MUST be called before using this method.  
    '' </para>
    '' </remarks>
    Protected Overridable Function ValidateLicenseKey(ByVal licenseKey As String, ByVal usageMode As LicenseUsageMode, ByVal generateRuntimeLicenseKey As Boolean, ByRef runtimeLicenseKey As String) As EncryptedLicense

        ' check that validation parameters have been set by the client 
        ' 
        If _parameters Is Nothing Then
            Throw New InvalidOperationException("EncryptedLicenseProvider.SetParameters must be called prior to using the EncryptedLicenseProvider")
        End If
        If licenseKey Is Nothing OrElse licenseKey.Trim().Length = 0 Then
            Return Nothing
        End If

        Try
            Dim data As Byte() = DecodeFromText(licenseKey, _parameters.TextEncoding)


            Dim des As New DESCryptoServiceProvider()
            des.Key = _desKey
            des.IV = _desIV

            ' extract the password data and encrypted product data 
            ' 
            Dim passwordData As Byte() = New Byte(LicenseUtilities.ArraySize(_parameters.KeyStrength) - 1) {}
            Dim encPayload As Byte() = New Byte(LicenseUtilities.ArraySize(data.Length - _parameters.KeyStrength) - 1) {}

            ' for key strengths greater than 8 the order of payload and key is swapped 
            ' 
            If _parameters.KeyStrength < 8 Then
                Array.Copy(data, 0, passwordData, 0, _parameters.KeyStrength)
                Array.Copy(data, _parameters.KeyStrength, encPayload, 0, encPayload.Length)
            Else
                Array.Copy(data, 0, encPayload, 0, encPayload.Length)
                Array.Copy(data, encPayload.Length, passwordData, 0, _parameters.KeyStrength)
            End If

            ' the key used to encrypt payload is just the first 7 bytes of the password data 
            ' 
            Dim encryptionKey As Byte() = New Byte(LicenseUtilities.ArraySize(8) - 1) {}
            Array.Copy(passwordData, 0, encryptionKey, 0, 7)

            ' validate that the password matches what the client is expecting 
            ' 
            Dim paddedPasswordData As Byte() = PadPassword(passwordData)
            If usageMode = LicenseUsageMode.Designtime Then
                ' if design time license requested then the license MUST be a design license 
                ' 
                If Not LicenseUtilities.VerifyData(_parameters.RSAProvider, paddedPasswordData, _parameters.DesignSignature) Then
                    Return Nothing
                End If
            Else
                ' if runtime license requested then first check if the license is a runtime license 
                ' also allow design licenses to work at runtime 
                ' 
                If Not LicenseUtilities.VerifyData(_parameters.RSAProvider, paddedPasswordData, _parameters.RuntimeSignature) Then
                    If Not LicenseUtilities.VerifyData(_parameters.RSAProvider, paddedPasswordData, _parameters.DesignSignature) Then
                        Return Nothing
                    End If
                End If
            End If

            ' decrypt the payload using the encryption key 
            ' 
            des.IV = encryptionKey
            Dim payload As Byte() = des.CreateDecryptor().TransformFinalBlock(encPayload, 0, encPayload.Length)

            Dim checksumLength As Integer = 0
            If _parameters.ChecksumProductInfo Then checksumLength = 2
            Dim serialNoLength As Integer = 4
            If _parameters.ShortSerialNo Then serialNoLength = 2

            Dim productData As Byte() = New Byte(LicenseUtilities.ArraySize(payload.Length - serialNoLength - checksumLength) - 1) {}
            Array.Copy(payload, serialNoLength, productData, 0, productData.Length)

            Dim serialNo As Int32
            If _parameters.ShortSerialNo Then
                serialNo = BitConverter.ToUInt16(payload, 0)
            Else
                serialNo = BitConverter.ToInt32(payload, 0)
            End If

            Dim productInfo As String = System.Text.ASCIIEncoding.UTF8.GetString(productData)

            ' validate the product data checksum 
            ' 
            If _parameters.ChecksumProductInfo Then
                Dim requiredChecksum As UInt16 = BitConverter.ToUInt16(payload, payload.Length - checksumLength)
                Dim actualChecksum As UInt16 = LicenseUtilities.Checksum(productData)
                If requiredChecksum <> actualChecksum Then
                    Return Nothing
                End If
            End If

            ' if in design time then create a runtime license and save it 
            ' 
            If usageMode = LicenseUsageMode.Designtime And generateRuntimeLicenseKey Then

                ' create the runtime password by encrypting the design password 
                ' 
                Dim encPassword As Byte() = des.CreateEncryptor().TransformFinalBlock(paddedPasswordData, 0, paddedPasswordData.Length)
                Dim runtimePasswordData As Byte() = New Byte(LicenseUtilities.ArraySize(_parameters.KeyStrength) - 1) {}
                Array.Copy(encPassword, 0, runtimePasswordData, 0, _parameters.KeyStrength)

                runtimeLicenseKey = GenerateKey(_parameters, runtimePasswordData, productData, serialNo)

            End If
            Return New EncryptedLicense(licenseKey, serialNo, productInfo)
        Catch
            Return Nothing
        End Try
    End Function

    ''' <summary> 
    ''' Read a license key from the given file 
    ''' </summary> 
    ''' <param name="licenseFile">The path to the license file to read the key from</param> 
    ''' <returns>The license key if any</returns> 
    Protected Overridable Function ReadKeyFromFile(ByVal licenseFile As String) As String
        Dim key As String = Nothing
        Try
            If File.Exists(licenseFile) Then
                Dim stream As Stream = New FileStream(licenseFile, FileMode.Open, FileAccess.Read, FileShare.Read)
                Dim reader As New StreamReader(stream)
                key = reader.ReadLine()
                reader.Close()
            End If
        Catch ex As Exception
            If Not LicenseUtilities.HandleIOExceptions Then
                Throw
            End If
            Dim msg As String = String.Format(LicenseResources.ReadErrorMsg, ex.Message, licenseFile)
            ShowError(LicenseResources.ReadErrorTitle, msg)
        End Try
        Return key
    End Function

    ''' <summary> 
    ''' Write a license key to the given file 
    ''' </summary> 
    ''' <param name="licenseFile">The path to the license file to write the key to</param> 
    ''' <param name="licenseKey">The license key to write</param> 
    Protected Overridable Sub WriteKeyToFile(ByVal licenseFile As String, ByVal licenseKey As String)
        Try
            Dim dir As String = IO.Path.GetDirectoryName(licenseFile)
            If Not Directory.Exists(dir) Then
                Directory.CreateDirectory(dir)
            End If
            Dim stream As Stream = New FileStream(licenseFile, FileMode.Create, FileAccess.Write, FileShare.None)
            Dim writer As New StreamWriter(stream)
            writer.WriteLine(licenseKey)
            writer.Close()
        Catch ex As Exception
            If Not LicenseUtilities.HandleIOExceptions Then
                Throw
            End If
            Dim msg As String = String.Format(LicenseResources.WriteErrorMsg, ex.Message, licenseFile)
            ShowError(LicenseResources.WriteErrorTitle, msg)
        End Try
    End Sub

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

#End Region

End Class

