' 
' FILE: LicenseUtilities.vb. 
' 
' COPYRIGHT: Copyright 2008 
' Infralution 
' 
Imports System
Imports System.Text
Imports System.Security
Imports System.ComponentModel
Imports System.ComponentModel.Design
Imports System.Globalization
Imports System.Security.Cryptography
Imports System.Reflection
Imports System.Collections
Imports System.IO
Imports System.Xml
Imports System.Runtime.Serialization
Imports System.Runtime.Serialization.Formatters
Imports System.Runtime.Serialization.Formatters.Binary
Imports System.Diagnostics

''' <summary> 
''' Defines the types of encoding possible for license keys 
''' </summary> 
#If ILS_PUBLIC_CLASS Then

Public Enum TextEncoding
#Else
Enum TextEncoding
#End If
    ''' <summary> 
    ''' Keys are encoded using hexadecimal notation (characters 0-9 and A-F) 
    ''' </summary> 
    Hex = 0

    ''' <summary> 
    ''' Keys are encoding using base 32 with the following character set (23456789ABCDEFGHJKLMNPQRSTUVWXYZ) 
    ''' </summary> 
    Base32 = 1
End Enum

''' <summary> 
''' Provides common utility methods for the Infralution Licensing classes 
''' </summary> 
#If ILS_PUBLIC_CLASS Then
Public NotInheritable Class LicenseUtilities
#Else
NotInheritable Class LicenseUtilities
#End If

    Private Shared _handleIOExceptions As Boolean = True
    Private Shared _useMachineKeyStore As Boolean = False
    Private Const Base32Chars As String = "23456789ABCDEFGHJKLMNPQRSTUVWXYZ"

#If ILS_CHECK_LICENSE Then

    ' the license parameters for the Licensing System itself 
    ' 
    Const ILS_PARAMETERS As String = _
         "<AuthenticatedLicenseParameters>" + _
         "  <EncryptedLicenseParameters>" + _
         "    <ProductName>ILS 5</ProductName>" + _
         "    <RSAKeyValue>" + _
         "      <Modulus>uAIehedbR7bKRWyMpOk7kzO/69GA9Z1MjU9iYFiSXphUFVAx+S5lb3fdNs+xh4UxHidtABIqY/HcZlWJmt2f58eYENI1xl00NsSoMnqSH1rvxC4FE3E15xNVw8nKocIfiBNRnmUtSoXy42IdKTRU+mSobb8xgt7F52UyVhTZTZ8=</Modulus>" + _
         "      <Exponent>AQAB</Exponent>" + _
         "    </RSAKeyValue>" + _
         "    <DesignSignature>EZikf293xpDIX4M19qrZHEAQpEXSrZ6QQbuW5T+xFxWaYEoJXjRXaDRAVeRm2LZ3xllNe/shrmxeuTJ74+yJnjwhoCG/XgeSyaiU2QIf/cL+ut1WljPccF3zC/QzYW0J3d/ZCooQC0R3+OMMem76e76B33G+oAVEA25VPNIOYm4=</DesignSignature>" + _
         "    <RuntimeSignature>Nnh7pbWQLNMhNtBoxdv7SwojVAI1LClevJF4akdhrZqQ7YEN9KLJTS/BE17F2Jua5U2C+oYwYLt6ppm3Ly+dcBzvgtZV/sHgyq/sPN2PuaD5CG1QK977SLmq/gucGgfJ8xictNx8kn06jPAddwT8ZeHVIG8UL7KIChA4iVlZkNA=</RuntimeSignature>" + _
         "    <KeyStrength>15</KeyStrength>" + _
         "    <ShortSerialNo>False</ShortSerialNo>" + _
         "  </EncryptedLicenseParameters>" + _
         "  <AuthenticationServerURL>http://www.infralution.net/authenticate/AuthenticationService.asmx</AuthenticationServerURL>" + _
         "  <ServerRSAKeyValue>" + _
         "    <Modulus>jV9ZCguIbiY0hVKBNe6UD0FWWaI0Y3nbaLvaiyfw9YIIHFFTSGcLA0dqm4yXNESWQXTt6jWwIKB4xUe+TzbnH5Sg7uz8+nzTBZdWoU6h4nvS8izBxaxnloPw1nnrW30JplBfk3J8uuPtd7KSuYn3ko1spAtrUCy9hTk0EveXWA8=</Modulus>" + _
         "    <Exponent>AQAB</Exponent>" + _
         "  </ServerRSAKeyValue>" + _
         "</AuthenticatedLicenseParameters>"

    ' the license for the Licensing System 
    ' 
    Private Shared _ilsLicense As AuthenticatedLicense

    ''' <summary> 
    ''' Return the path to the ILS License File 
    ''' </summary> 
    Private Shared ReadOnly Property ILSLicensePath() As String
        Get
#If ILS_ASP Then
            If System.Web.HttpContext.Current IsNot Nothing Then
                Return "App_Data\ILS5.lic"
            Else
                Dim commonDir As String = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)
                Return Path.Combine(commonDir, "Infralution\Licenses\ILS5.lic")
            End If
#Else
            Dim commonDir As String = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)
            Return Path.Combine(commonDir, "Infralution\Licenses\ILS5.lic")
#End If
        End Get
    End Property

    ''' <summary> 
    ''' Return the license for the Infralution Licensing System itself 
    ''' </summary> 
    ''' <returns>The ILS license if installed or null if not</returns> 
    Public Shared ReadOnly Property ILSLicense() As AuthenticatedLicense
        Get
            If _ilsLicense Is Nothing Then
                Dim provider As New AuthenticatedLicenseProvider()
                _ilsLicense = provider.GetLicense(ILS_PARAMETERS, ILSLicensePath, True)
            End If
            Return _ilsLicense
        End Get
    End Property

    ''' <summary> 
    ''' Install a license for ILS 
    ''' </summary> 
    ''' <param name="authenticationKey">The ILS authentication key</param> 
    ''' <returns>True if successful</returns> 
    Public Shared Function InstallILSLicense(ByVal authenticationKey As String) As Boolean
        Dim provider As New AuthenticatedLicenseProvider()
        Dim license As AuthenticatedLicense = provider.AuthenticateKey(ILS_PARAMETERS, authenticationKey)
        If license IsNot Nothing Then
            provider.InstallLicense(ILSLicensePath, license)
            _ilsLicense = license
        End If
        Return (_ilsLicense IsNot Nothing)
    End Function

    ''' <summary> 
    ''' Format license parameters nicely for inclusion in VB code 
    ''' </summary> 
    ''' <param name="licenseParameters"></param> 
    ''' <returns></returns> 
    Public Shared Function FormatVBParameters(ByVal licenseParameters As String) As String
        Dim lines As String() = GetXmlLines(licenseParameters)
        Dim sb As New StringBuilder()
        sb.AppendLine("Const LICENSE_PARAMETERS As String = _")
        For i As Integer = 0 To lines.Length - 1
            If i > 0 Then
                sb.AppendLine(" + _")
            End If
            sb.AppendFormat("" & Chr(9) & """{0}""", lines(i))
        Next
        sb.AppendLine()
        Return sb.ToString()
    End Function

    ''' <summary> 
    ''' Format license parameters nicely for inclusion in C# code 
    ''' </summary> 
    ''' <param name="licenseParameters"></param> 
    ''' <returns></returns> 
    Public Shared Function FormatCSParameters(ByVal licenseParameters As String) As String
        Dim lines As String() = GetXmlLines(licenseParameters)
        Dim sb As New StringBuilder()
        sb.AppendLine("const string LICENSE_PARAMETERS = ")
        For i As Integer = 0 To lines.Length - 1
            If i = 0 Then
                sb.AppendFormat("" & Chr(9) & "@""{0}", lines(i))
            Else
                sb.AppendLine()
                sb.AppendFormat("" & Chr(9) & "{0}", lines(i))
            End If
        Next
        sb.AppendLine(""";")
        Return sb.ToString()
    End Function

    ''' <summary> 
    ''' Read License Parameters from an XML reader 
    ''' </summary> 
    ''' <param name="reader">The XML Reader</param> 
    ''' <returns>The license parameters</returns> 
    ''' <remarks> 
    ''' This function can read both <see cref="AuthenticatedLicenseParameters"/> and 
    ''' <see cref="EncryptedLicenseParameters"/>. 
    ''' </remarks> 
    Public Shared Function ReadLicenseParameters(ByVal reader As XmlReader) As EncryptedLicenseParameters
        Dim parameters As EncryptedLicenseParameters = Nothing
        reader.MoveToContent()
        If reader.Name = "AuthenticatedLicenseParameters" Then
            parameters = New AuthenticatedLicenseParameters()
        ElseIf reader.Name = "EncryptedLicenseParameters" Then
            parameters = New EncryptedLicenseParameters()
        Else
            Throw New XmlException(LicenseResources.InvalidILSFile)
        End If
        parameters.Read(reader)
        Return parameters
    End Function

    ''' <summary> 
    ''' Read License Parameters from an XML string 
    ''' </summary> 
    ''' <param name="xmlParameters">The XML Parameters string</param> 
    ''' <returns>The license parameters</returns> 
    ''' <remarks> 
    ''' This function can read both <see cref="AuthenticatedLicenseParameters"/> and 
    ''' <see cref="EncryptedLicenseParameters"/>. 
    ''' </remarks> 
    Public Shared Function ReadLicenseParameters(ByVal xmlParameters As String) As EncryptedLicenseParameters
        Dim reader As XmlReader = New XmlTextReader(xmlParameters, XmlNodeType.Element, Nothing)
        Dim parameters As EncryptedLicenseParameters = ReadLicenseParameters(reader)
        reader.Close()
        Return parameters
    End Function

#End If

    ''' <summary> 
    ''' Should the licensing classes handle exceptions when reading and writing license files 
    ''' </summary> 
    ''' <remarks> 
    ''' Set this to false if you wish to handle these exceptions yourself 
    ''' </remarks> 
    Public Shared Property HandleIOExceptions() As Boolean
        Get
            Return _handleIOExceptions
        End Get
        Set(ByVal value As Boolean)
            _handleIOExceptions = value
        End Set
    End Property

    ''' <summary> 
    ''' Determines whether RSA keys used to verify licenses are stored on a user or machine level 
    ''' </summary> 
    ''' <remarks> 
    ''' Setting this value to true may be useful when impersonating or running under an account 
    ''' whose user profile is not loaded. ILS will by default use the MachineKeyStore when there 
    ''' is no interactive user (ie services and ASP.NET) otherwise it will use the UserKeyStore. 
    ''' </remarks> 
    ''' <seealso cref="RSACryptoServiceProvider.UseMachineKeyStore"/> 
    Public Shared Property UseMachineKeyStore() As Boolean
        Get
            Return _useMachineKeyStore
        End Get
        Set(ByVal value As Boolean)
            _useMachineKeyStore = value
        End Set
    End Property

    ''' <summary>
    ''' Returns a checksum string with a maximum length of 3 characters based on the given input string
    ''' </summary>
    ''' <param name="input">The input string to return a checksum for</param>
    ''' <returns>An checksum that can be used to validate the given input string</returns>
    ''' <remarks>
    ''' <para>
    ''' This function can be used to generate a short checksum that can be embedded in a
    ''' license key as <see cref="EncryptedLicense.ProductInfo"/>. This allows you to tie 
    ''' the license key to information supplied by the user (for instance the name of the 
    ''' purchaser) without having to include the full information in the license key. 
    ''' This enables license keys to be kept reasonably short.
    ''' </para>
    ''' <para>
    ''' When the license is checked by the application it performs a checksum on the information
    ''' supplied by the user and checks that it matches the information in the ProductInfo that
    ''' was generated when the license was issued. The License Tracker application provides
    ''' support for "CustomGenerators" which allow you provide the code to generate the ProductInfo
    ''' from customer and other information.
    ''' </para>
    ''' <para>
    ''' The returned string has a maximum length of 3 characters - but may be shorter. If you
    ''' require a constant length checksum string then consider using the overloaded method that
    ''' takes a pad parameter.
    ''' </para>
    ''' </remarks>
    Public Shared Function Checksum(ByVal input As String) As String
        Dim hash As Integer = 0
        If Not input Is Nothing Then
            hash = HashString(input)
            hash = Math.Abs(hash Mod 1000)
        End If
        Return hash.ToString(CultureInfo.InvariantCulture)
    End Function

    ''' <summary>
    ''' Returns a checksum string based on the given input string
    ''' </summary>
    ''' <param name="input">The input string to return a checksum for</param>
    ''' <param name="maxLength">The maximum length of the checksum string</param>
    ''' <param name="pad">If true the checksum is always padded to give a constant length string</param>
    ''' <returns>An checksum that can be used to validate the given input string</returns>
    ''' <remarks>
    ''' <para>
    ''' This function can be used to generate a short checksum that can be embedded in a
    ''' license key as <see cref="EncryptedLicense.ProductInfo"/>. This allows you to tie 
    ''' the license key to information supplied by the user (for instance the name of the 
    ''' purchaser) without having to include the full information in the license key. 
    ''' This enables license keys to be kept reasonably short.
    ''' </para>
    ''' <para>
    ''' When the license is checked by the application it performs a checksum on the information
    ''' supplied by the user and checks that it matches the information in the ProductInfo that
    ''' was generated when the license was issued. The License Tracker application provides
    ''' support for "CustomGenerators" which allow you provide the code to generate the ProductInfo
    ''' from customer and other information.
    ''' </para>
    ''' </remarks>
    Public Shared Function Checksum(ByVal input As String, ByVal maxLength As Integer, ByVal pad As Boolean) As String
        If maxLength < 1 OrElse maxLength > 9 Then
            Throw New ArgumentOutOfRangeException("maxLength")
        End If
        Dim hash As Integer = 0
        If Not input Is Nothing Then
            hash = HashString(input)
            Dim modValue As Integer = 1
            For i As Integer = 0 To maxLength - 1
                modValue *= 10
            Next
            hash = Math.Abs(hash Mod modValue)
        End If
        Dim result As String = hash.ToString(CultureInfo.InvariantCulture)
        If pad Then
            result = result.PadLeft(maxLength, "0"c)
        End If
        Return result
    End Function

    ''' <summary> 
    ''' Return the given input string stripped of the given characters 
    ''' </summary> 
    ''' <param name="value">The string to strip</param> 
    ''' <param name="characters">The characters to strip from the string</param> 
    ''' <returns>The input string with the given characters removed</returns> 
    Public Shared Function Strip(ByVal value As String, ByVal characters As String) As String
        If value Is Nothing Then
            Return Nothing
        End If
        Dim sb As New StringBuilder()
        For Each ch As Char In value
            If characters.IndexOf(ch, 0) < 0 Then
                sb.Append(ch)
            End If
        Next
        Return sb.ToString()
    End Function

    ''' <summary> 
    ''' Read a Base64 string from an XmlReader into a byte array 
    ''' </summary> 
    ''' <param name="reader">The XmlReader to read from</param> 
    ''' <returns>The byte data for the given element</returns> 
    Public Shared Function ReadElementBase64(ByVal reader As XmlReader) As Byte()
        Dim value As String = reader.ReadElementString()
        Return Convert.FromBase64String(Strip(value, "" & Chr(9) & "" & Chr(13) & "" & Chr(10) & " "))
    End Function

    ''' <summary> 
    ''' Read a Base64 string from an XmlReader into a byte array 
    ''' </summary> 
    ''' <param name="reader">The XmlReader to read from</param> 
    ''' <param name="name">The name of the element</param> 
    ''' <returns>The byte data for the given element</returns> 
    Public Shared Function ReadElementBase64(ByVal reader As XmlReader, ByVal name As String) As Byte()
        Dim value As String = reader.ReadElementString(name)
        Return Convert.FromBase64String(Strip(value, "" & Chr(9) & "" & Chr(13) & "" & Chr(10) & " "))
    End Function

    ''' <summary> 
    ''' Write a byte array into Base64 string of an XmlWriter 
    ''' </summary> 
    ''' <param name="writer">The XmlWriter to write to</param> 
    ''' <param name="name">The name of the element</param> 
    ''' <param name="value">The data to write</param> 
    Public Shared Sub WriteElementBase64(ByVal writer As XmlWriter, ByVal name As String, ByVal value As Byte())
        writer.WriteElementString(name, Convert.ToBase64String(value))
    End Sub

    ''' <summary> 
    ''' Read RSA Parameters for an RSA Provider to an XmlWriter 
    ''' </summary> 
    ''' <param name="provider">The provider to writer the parameters for</param> 
    ''' <param name="writer">The XmlWriter to write to</param> 
    ''' <param name="localName">The name of the element</param> 
    ''' <param name="includePrivateParameters">Should the private RSA parameters be included</param> 
    Public Shared Sub WriteRSAParameters(ByVal provider As RSACryptoServiceProvider, ByVal writer As XmlWriter, ByVal localName As String, ByVal includePrivateParameters As Boolean)
        Dim parameters As RSAParameters = provider.ExportParameters(includePrivateParameters)
        writer.WriteStartElement(localName)
        WriteElementBase64(writer, "Modulus", parameters.Modulus)
        WriteElementBase64(writer, "Exponent", parameters.Exponent)
        If includePrivateParameters And parameters.P IsNot Nothing Then
            WriteElementBase64(writer, "P", parameters.P)
            WriteElementBase64(writer, "Q", parameters.Q)
            WriteElementBase64(writer, "DP", parameters.DP)
            WriteElementBase64(writer, "DQ", parameters.DQ)
            WriteElementBase64(writer, "InverseQ", parameters.InverseQ)
            WriteElementBase64(writer, "D", parameters.D)
        End If
        writer.WriteEndElement()
    End Sub

    ''' <summary> 
    ''' Read RSA Parameters for an RSA Provider from an XmlReader 
    ''' </summary> 
    ''' <param name="provider">The provider to read the parameters for</param> 
    ''' <param name="reader">The XmlReader to read from</param> 
    ''' <param name="localName">The name of the element</param> 
    Public Shared Sub ReadRSAParameters(ByVal provider As RSACryptoServiceProvider, ByVal reader As XmlReader, ByVal localName As String)
        Dim parameters As New RSAParameters()
        reader.ReadStartElement(localName)
        While reader.IsStartElement()
            Select Case reader.Name
                Case "Modulus"
                    parameters.Modulus = ReadElementBase64(reader)
                    Exit Select
                Case "Exponent"
                    parameters.Exponent = ReadElementBase64(reader)
                    Exit Select
                Case "P"
                    parameters.P = ReadElementBase64(reader)
                    Exit Select
                Case "Q"
                    parameters.Q = ReadElementBase64(reader)
                    Exit Select
                Case "DP"
                    parameters.DP = ReadElementBase64(reader)
                    Exit Select
                Case "DQ"
                    parameters.DQ = ReadElementBase64(reader)
                    Exit Select
                Case "InverseQ"
                    parameters.InverseQ = ReadElementBase64(reader)
                    Exit Select
                Case "D"
                    parameters.D = ReadElementBase64(reader)
                    Exit Select
                Case Else
                    Dim [error] As String = "Unexpected XML Element: {0}"
                    Throw New XmlSyntaxException(String.Format([error], reader.Name))
            End Select
        End While
        reader.ReadEndElement()
        provider.ImportParameters(parameters)
    End Sub

    ''' <summary> 
    ''' Converts a byte array into a hexadecimal representation. 
    ''' </summary> 
    ''' <param name="data">The byte data to convert</param> 
    ''' <returns>Hexadecimal representation of the data</returns> 
    Public Shared Function ToHex(ByVal data As Byte()) As String
        Dim sb As New StringBuilder()
        For i As Integer = 0 To data.Length - 1
            If i > 0 AndAlso i Mod 2 = 0 Then
                sb.Append("-")
            End If
            sb.Append(data(i).ToString("X2", CultureInfo.InvariantCulture))
        Next
        Return sb.ToString()
    End Function

    ''' <summary> 
    ''' Converts a hexadecimal string into a byte array. 
    ''' </summary> 
    ''' <param name="hex">The hexadecimal string to convert</param> 
    ''' <returns>The converted byte data</returns> 
    Public Shared Function FromHex(ByVal hex As String) As Byte()
        Dim strippedHex As String = Strip(hex, "" & Chr(9) & "" & Chr(13) & "" & Chr(10) & " -")
        If strippedHex Is Nothing OrElse strippedHex.Length Mod 2 <> 0 Then
            Throw New FormatException("Invalid hexadecimal string")
        End If
        Dim result As Byte() = New Byte(ArraySize(strippedHex.Length \ 2) - 1) {}
        Dim i As Integer = 0, j As Integer = 0
        While i < strippedHex.Length
            Dim s As String = strippedHex.Substring(i, 2)
            result(j) = Byte.Parse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture)
            i += 2
            j += 1
        End While
        Return result
    End Function

    ''' <summary> 
    ''' Converts a byte array into a base 32 representation. 
    ''' </summary> 
    ''' <param name="data">The byte data to convert</param> 
    ''' <returns>Base32 representation of the data</returns> 
    Public Shared Function ToBase32(ByVal data As Byte()) As String
        Dim sb As New StringBuilder
        Dim index As Byte
        Dim hi As Integer = 5
        Dim currentByte As Integer = 0

        While (currentByte < data.Length)
            ' do we need to use the next byte?
            If hi > 8 Then
                ' get the last piece from the current byte, shift it to the right
                ' and increment the byte counter
                index = data(currentByte) >> (hi - 5)
                currentByte += 1
                If currentByte < data.Length Then
                    ' if we are not at the end, get the first piece from
                    ' the next byte, clear it and shift it to the left
                    index = (((data(currentByte) << (16 - hi)) >> 3) Or index)
                End If
                hi -= 3

            ElseIf hi = 8 Then
                index = data(currentByte) >> 3
                currentByte += 1
                hi -= 3
            Else
                ' simply get the stuff from the current byte
                index = (data(currentByte) << (8 - hi)) >> 3
                hi += 5
            End If
            sb.Append(Base32Chars(index))
            Dim i As Integer = sb.Length + 1
            If i > 0 And (i Mod 5) = 0 Then
                sb.Append("-")
            End If
        End While

        ' ensure we don't have a trailing separator
        '
        If sb.Length > 0 Then
            If sb(sb.Length - 1) = "-" Then
                sb.Remove(sb.Length - 1, 1)
            End If
        End If
        Return sb.ToString()
    End Function

    ''' <summary> 
    ''' Converts a base32 string into a byte array. 
    ''' </summary> 
    ''' <param name="str">The base32 string to convert</param> 
    ''' <returns>The converted byte data</returns> 
    Public Shared Function FromBase32(ByVal str As String) As Byte()
        str = Strip(str, Chr(9) & Chr(13) & Chr(10) & " -")
        str = str.ToUpper(CultureInfo.InvariantCulture)
        Dim numBytes As Integer = str.Length * 5 \ 8
        Dim bytes(numBytes - 1) As Byte

        Dim bitBuffer As Integer
        Dim currentCharIndex As Integer
        Dim bitsInBuffer As Integer

        If str.Length < 3 Then
            bytes(0) = CType(Base32Chars.IndexOf(str(0)) Or Base32Chars.IndexOf(str(1)) << 5, Byte)
            Return bytes
        End If

        bitBuffer = Base32Chars.IndexOf(str(0)) Or Base32Chars.IndexOf(str(1)) << 5
        bitsInBuffer = 10
        currentCharIndex = 2
        For i As Integer = 0 To bytes.Length - 1
            bytes(i) = CType(bitBuffer And &HFF, Byte)
            bitBuffer = bitBuffer >> 8
            bitsInBuffer -= 8
            While bitsInBuffer < 8 And currentCharIndex < str.Length
                bitBuffer = bitBuffer Or Base32Chars.IndexOf(str(currentCharIndex)) << bitsInBuffer
                currentCharIndex += 1
                bitsInBuffer += 5
            End While
        Next
        Return bytes
    End Function

    ''' <summary> 
    ''' Converts a byte array into a text representation. 
    ''' </summary> 
    ''' <param name="data">The byte data to convert</param> 
    ''' <param name="encoding">The encoding to use</param> 
    ''' <returns>Text representation of the data</returns> 
    Public Shared Function EncodeToText(ByVal data As Byte(), ByVal encoding As TextEncoding) As String
        Select Case encoding
            Case TextEncoding.Base32
                Return ToBase32(data)
            Case Else
                Return ToHex(data)
        End Select
    End Function

    ''' <summary> 
    ''' Converts a string into a byte array. 
    ''' </summary> 
    ''' <param name="text">The text to convert</param> 
    ''' <param name="encoding">The encoding to use</param> 
    ''' <returns>The converted byte data</returns> 
    Public Shared Function DecodeFromText(ByVal text As String, ByVal encoding As TextEncoding) As Byte()
        Select Case encoding
            Case TextEncoding.Base32
                Return FromBase32(text)
            Case Else
                Return FromHex(text)
        End Select
    End Function

    ''' <summary> 
    ''' Create an instance of the RSACryptoServiceProvider. 
    ''' </summary> 
    ''' <returns>An instance of the RSACryptoServiceProvider</returns> 
    Public Shared Function CreateRSACryptoServiceProvider() As RSACryptoServiceProvider
        Const keySize As Integer = 1024

        ' If this is not a service or ASP then create the RSA Service Provicer 
        ' using the default user profile key store - if this fails then fall back 
        ' to using the machine key store 
        ' 
        Dim rsa As RSACryptoServiceProvider = Nothing
        If Environment.UserInteractive AndAlso Not UseMachineKeyStore Then
            Try
                rsa = New RSACryptoServiceProvider(keySize)
            Catch
            End Try
        End If

        If rsa Is Nothing Then
            Dim cspParams As New CspParameters()
            cspParams.Flags = CspProviderFlags.UseMachineKeyStore
            rsa = New RSACryptoServiceProvider(keySize, cspParams)
        End If
        Return rsa
    End Function

    ''' <summary> 
    ''' Sign the given data using the given RSA parameters 
    ''' </summary> 
    ''' <param name="rsaProvider">The RSA Provider to use</param> 
    ''' <param name="data">The data to sign</param> 
    ''' <returns>The signature for the data</returns> 
    ''' <remarks> 
    ''' Uses <see cref="RSACryptoServiceProvider.SignHash"/> instead of 
    ''' <see cref="RSACryptoServiceProvider.SignData"/> to workaround bug in standard Microsoft 
    ''' <see cref="RSACryptoServiceProvider"/> that can cause a lengthy delay. 
    ''' See http://support.microsoft.com/default.aspx?scid=kb;en-us;948080 
    ''' </remarks> 
    Public Shared Function SignData(ByVal rsaProvider As RSACryptoServiceProvider, ByVal data As Byte()) As Byte()
        Dim sha1__1 As SHA1 = SHA1.Create()
        Dim hash As Byte() = sha1__1.ComputeHash(data)
        Return rsaProvider.SignHash(hash, Nothing)
    End Function

    ''' <summary> 
    ''' Verify the signature for the given data using the given RSA parameters 
    ''' </summary> 
    ''' <param name="rsaProvider">The RSA Provider to use</param> 
    ''' <param name="data">The data to verify</param> 
    ''' <param name="signature">The signature for the data</param> 
    ''' <returns>True if the data matches the signature</returns> 
    ''' <remarks> 
    ''' Uses <see cref="RSACryptoServiceProvider.VerifyHash"/> instead of 
    ''' <see cref="RSACryptoServiceProvider.VerifyData"/> to workaround bug in standard Microsoft 
    ''' <see cref="RSACryptoServiceProvider"/> that can cause a lengthy delay. 
    ''' See http://support.microsoft.com/default.aspx?scid=kb;en-us;948080 
    ''' </remarks> 
    Public Shared Function VerifyData(ByVal rsaProvider As RSACryptoServiceProvider, ByVal data As Byte(), ByVal signature As Byte()) As Boolean
        Dim sha1__1 As SHA1 = SHA1.Create()
        Dim hash As Byte() = sha1__1.ComputeHash(data)
        Return rsaProvider.VerifyHash(hash, Nothing, signature)
    End Function

    ''' <summary> 
    ''' Encrypt the keys of the given symmetric algorithm using an RSA public key 
    ''' </summary> 
    ''' <param name="rsaProvider">The RSA provider to use to encrypt the symmetric keys</param> 
    ''' <param name="algorithm">The symmetric algorithm</param> 
    ''' <returns>String containing the encrypted keys</returns> 
    Public Shared Function EncryptKeys(ByVal rsaProvider As RSACryptoServiceProvider, ByVal algorithm As SymmetricAlgorithm) As String
        Dim ms As New MemoryStream()

        ' encrypt the keys using RSA 
        ' 
        Dim encryptedKey As Byte() = rsaProvider.Encrypt(algorithm.Key, False)
        Dim encryptedIV As Byte() = rsaProvider.Encrypt(algorithm.IV, False)

        ' write the encrypted symmetric keys to the stream 
        ' 
        Dim encryptedKeyLength As Int32 = encryptedKey.Length
        ms.Write(BitConverter.GetBytes(encryptedKeyLength), 0, 4)
        ms.Write(encryptedKey, 0, encryptedKey.Length)

        Dim encryptedIVLength As Int32 = encryptedIV.Length
        ms.Write(BitConverter.GetBytes(encryptedIVLength), 0, 4)
        ms.Write(encryptedIV, 0, encryptedIV.Length)
        ms.Flush()
        Return Convert.ToBase64String(ms.ToArray())
    End Function

    ''' <summary> 
    ''' Set the Key and IV for the given symmetric algorithm by decrypting the keys from a string 
    ''' </summary> 
    ''' <param name="rsaProvider">The RSA provider to use to decrypt the keys</param> 
    ''' <param name="algorithm">The symmetric algorithm to set the keys for</param> 
    ''' <param name="encryptedKeys">String containing encrypted keys</param> 
    Public Shared Sub DecryptKeys(ByVal rsaProvider As RSACryptoServiceProvider, ByVal algorithm As SymmetricAlgorithm, ByVal encryptedKeys As String)
        Dim streamData As Byte() = Convert.FromBase64String(encryptedKeys)
        Dim ms As New MemoryStream(streamData)

        Dim lengthBuffer As Byte() = New Byte(-1) {}

        ' read the encrypted key and IV from the stream 
        ' 
        ms.Read(lengthBuffer, 0, 4)
        Dim encryptedKeyLength As Int32 = BitConverter.ToInt32(lengthBuffer, 0)
        Dim encryptedKey As Byte() = New Byte(encryptedKeyLength - 1) {}
        ms.Read(encryptedKey, 0, encryptedKeyLength)

        ms.Read(lengthBuffer, 0, 4)
        Dim encryptedIVLength As Int32 = BitConverter.ToInt32(lengthBuffer, 0)
        Dim encryptedIV As Byte() = New Byte(encryptedIVLength - 1) {}
        ms.Read(encryptedIV, 0, encryptedIV.Length)

        ' Decrypt the key and IV and setup the algorithm 
        ' 
        algorithm.Key = rsaProvider.Decrypt(encryptedKey, False)
        algorithm.IV = rsaProvider.Decrypt(encryptedIV, False)
    End Sub

    ''' <summary> 
    ''' Encrypt a set of key/values using the given algorithm 
    ''' </summary> 
    ''' <param name="algorithm">The algorithm to use to encrypt the data</param> 
    ''' <param name="values">A hash table containing string key/value pairs</param> 
    ''' <returns>The encrypted key/values</returns> 
    Public Shared Function EncryptValues(ByVal algorithm As SymmetricAlgorithm, ByVal values As Hashtable) As Byte()
        Dim memoryStream As New MemoryStream()
        Dim cryptoStream As New CryptoStream(memoryStream, algorithm.CreateEncryptor(), CryptoStreamMode.Write)
        Dim xmlWriter As New XmlTextWriter(cryptoStream, Encoding.UTF8)
        xmlWriter.WriteStartElement("Values")
        For Each entry As DictionaryEntry In values
            xmlWriter.WriteElementString(entry.Key.ToString(), entry.Value.ToString())
        Next
        xmlWriter.WriteEndElement()
        xmlWriter.Close()
        cryptoStream.Close()
        memoryStream.Flush()
        Return memoryStream.ToArray()
    End Function

    ''' <summary> 
    ''' Decrypt a set of key/values using the given algorithm 
    ''' </summary> 
    ''' <param name="algorithm">The algorithm to use to decrypt the values</param> 
    ''' <param name="encryptedValues">The encrypted data</param> 
    ''' <returns>A hashtable containing the string key/values</returns> 
    Public Shared Function DecryptValues(ByVal algorithm As SymmetricAlgorithm, ByVal encryptedValues As Byte()) As Hashtable
        Dim values As New Hashtable()
        Using memoryStream As New MemoryStream(encryptedValues)
            Using cryptoStream As New CryptoStream(memoryStream, algorithm.CreateDecryptor(), CryptoStreamMode.Read)
                Using xmlReader As New XmlTextReader(cryptoStream, XmlNodeType.Element, Nothing)
                    xmlReader.ReadStartElement("Values")
                    While xmlReader.IsStartElement()
                        values(xmlReader.Name) = xmlReader.ReadElementString()
                    End While
                    xmlReader.ReadEndElement()
                End Using
            End Using
        End Using
        Return values
    End Function

    ''' <summary> 
    ''' Encrypt text using the given algorithm 
    ''' </summary> 
    ''' <param name="algorithm">The algorithm to use to encrypt the data</param> 
    ''' <param name="text">The text to encrypt</param> 
    ''' <returns>The encrypted data</returns> 
    Public Shared Function EncryptText(ByVal algorithm As SymmetricAlgorithm, ByVal text As String) As Byte()
        Dim data As Byte() = Encoding.UTF8.GetBytes(text)
        Return algorithm.CreateEncryptor().TransformFinalBlock(data, 0, data.Length)
    End Function

    ''' <summary> 
    ''' Decrypt text using the given algorithm 
    ''' </summary> 
    ''' <param name="algorithm">The algorithm to use to decrypt the values</param> 
    ''' <param name="encryptedData">The encrypted text</param> 
    ''' <returns>The decryptedText</returns> 
    Public Shared Function DecryptText(ByVal algorithm As SymmetricAlgorithm, ByVal encryptedData As Byte()) As String
        Dim data As Byte() = algorithm.CreateDecryptor().TransformFinalBlock(encryptedData, 0, encryptedData.Length)
        Return Encoding.UTF8.GetString(data)
    End Function

    ''' <summary> 
    ''' Retrieve the license key for the given type from the given DLL/EXE assembly resources 
    ''' </summary> 
    ''' <param name="assembly">The assembly containing the license resources</param> 
    ''' <param name="type">The type to get the license key for</param> 
    ''' <returns>The license key if any</returns> 
    Friend Shared Function GetSavedLicenseKey(ByVal assembly As Assembly, ByVal type As Type) As String
        Dim key As String = Nothing
        Dim assemblyName As String = assembly.GetName().Name
        Dim resourceName As String = assemblyName + ".dll.licenses"
        Dim stream As Stream = assembly.GetManifestResourceStream(resourceName)
        If stream Is Nothing Then
            resourceName = assemblyName + ".exe.licenses"
            stream = assembly.GetManifestResourceStream(resourceName)
        End If
        If stream IsNot Nothing Then
            Dim formatter As IFormatter = New BinaryFormatter()
            Dim values As Object() = TryCast(formatter.Deserialize(stream), Object())
            If values IsNot Nothing Then
                Dim keys As Hashtable = TryCast(values(1), Hashtable)
                If keys IsNot Nothing Then
                    For Each entry As DictionaryEntry In keys
                        Dim typeName As String = TryCast(entry.Key, String)
                        If typeName IsNot Nothing Then
                            typeName = typeName.Trim()
                            If typeName.IndexOf(type.FullName) = 0 Then
                                key = TryCast(entry.Value, String)
                                Exit For
                            End If
                        End If
                    Next
                End If
            End If
            stream.Close()
        End If
        Return key
    End Function

    ''' <summary> 
    ''' Uninstall the given license file by deleting it 
    ''' </summary> 
    ''' <param name="path">The full file path</param> 
    Friend Shared Sub UninstallLicenseFile(ByVal path As String)
        Try
            If File.Exists(path) Then
                File.Delete(path)
            End If
        Catch ex As Exception
            If Not LicenseUtilities.HandleIOExceptions Then
                Throw
            End If
            Dim msg As String = String.Format(LicenseResources.UninstallErrorMsg, ex.Message, path)
            ShowError(LicenseResources.UninstallErrorTitle, msg)
        End Try
    End Sub

    ''' <summary> 
    ''' Display an error to a message box or the trace output 
    ''' </summary> 
    ''' <param name="title">The title for the error</param> 
    ''' <param name="message">The error message</param> 
    Friend Shared Sub ShowError(ByVal title As String, ByVal message As String)
#If ILS_ASP Then
        Trace.WriteLine(title + ": " + message)
#End If
#If ILS_WPF Then
        If (Environment.UserInteractive) Then
            System.Windows.MessageBox.Show(message, title, _
                                           System.Windows.MessageBoxButton.OK, _
                                           System.Windows.MessageBoxImage.Error)
        Else
            Trace.WriteLine(title + ": " + message)
        End If
#End If
#If ILS_FORMS Then
        If (Environment.UserInteractive) Then
            System.Windows.Forms.MessageBox.Show(message, title, _
                                             System.Windows.Forms.MessageBoxButtons.OK, _
                                             System.Windows.Forms.MessageBoxIcon.Error)
        Else
            Trace.WriteLine(title + ": " + message)
        End If
#End If

    End Sub

    ''' <summary> 
    ''' Return the default directory used to store license files 
    ''' </summary> 
    ''' <param name="context">The licence context</param> 
    ''' <param name="type">The type being licensed</param> 
    ''' <returns>The directory to look for license files</returns> 
    Friend Shared Function DefaultLicenseDirectory(ByVal context As LicenseContext, ByVal type As Type) As String
        Dim result As String = Nothing

        ' try to use the type resolver service if available 
        ' 
        If context IsNot Nothing AndAlso type IsNot Nothing Then
            Dim resolver As ITypeResolutionService = DirectCast(context.GetService(GetType(ITypeResolutionService)), ITypeResolutionService)
            If resolver IsNot Nothing Then
                result = resolver.GetPathOfAssembly(type.Assembly.GetName())
                result = Path.GetDirectoryName(result)
            End If
        End If

        If result Is Nothing Then
            If type Is Nothing Then
                result = AppDomain.CurrentDomain.BaseDirectory
            Else
                Dim assembly As Assembly = type.Assembly

                ' use the code base if possible 
                ' 
                result = assembly.CodeBase
                If result.StartsWith("file:///") Then
                    result = result.Replace("file:///", "")
                Else
                    result = type.[Module].FullyQualifiedName
                End If
                result = Path.GetDirectoryName(result)
            End If
        End If
        Return result
    End Function

    ''' <summary> 
    ''' Return the array size to use when declaring an array of the given length. 
    ''' </summary> 
    ''' <param name="length">The length of the array you are declaring</param> 
    ''' <returns>The size to declare the array</returns> 
    ''' <remarks> 
    ''' This is used to account for the difference between declaring VB and C# arrays and 
    ''' permit automatic conversion of the code to VB 
    ''' </remarks> 
    Friend Shared Function ArraySize(ByVal length As Integer) As Integer
        Return length
    End Function

    ''' <summary> 
    ''' Are the contents of the two byte arrays equal 
    ''' </summary> 
    ''' <param name="a1">The first array</param> 
    ''' <param name="a2">The second array </param> 
    ''' <returns>True if the contents of the arrays is equal</returns> 
    Friend Shared Function ArrayEqual(ByVal a1 As Byte(), ByVal a2 As Byte()) As Boolean
        If a1 Is a2 Then
            Return True
        End If
        If a1 Is Nothing OrElse a2 Is Nothing Then
            Return False
        End If
        If a1.Length <> a2.Length Then
            Return False
        End If
        Dim i As Integer
        For i = 0 To a1.Length - 1
            If a1(i) <> a2(i) Then
                Return False
            End If
        Next i
        Return True
    End Function

    '' <summary>
    '' Create a checksum for the given block of data
    '' </summary>
    '' <param name="data">The block of data to create a checksum for</param>
    '' <returns>An integer checksum</returns>
    Friend Shared Function Checksum(ByVal data As Byte()) As UInt16
        Const MaxUInt16 As Integer = 65535
        Dim hash As Integer = 5381
        Dim c As Integer
        Dim i As Integer = 0
        While i < data.Length
            c = data(i)
            Dim shift As Integer = (hash << 5)
            Dim add As Integer = UncheckedAdd(shift, hash)
            hash = add Xor c
            i += 1
        End While
        hash = hash Mod MaxUInt16
        If (hash < 0) Then
            hash = MaxUInt16 + hash + 1
        End If
        Return Convert.ToUInt16(hash)
    End Function

    '' <summary>
    '' Add the two given numbers without overflow checking
    '' </summary>
    Private Shared Function UncheckedAdd(ByVal i1 As Integer, ByVal i2 As Integer) As Integer
        Dim l1 As Long = i1
        Dim l2 As Long = i2
        Dim sum As Long = l1 + l2
        Dim result As Long
        If sum < Integer.MinValue Then
            result = Integer.MaxValue + (l1 - Integer.MinValue) + i2 + 1
        ElseIf sum > Integer.MaxValue Then
            result = Integer.MinValue + (l1 - Integer.MaxValue) + i2 - 1
        Else
            result = i1 + i2
        End If
        Return CType(result, Integer)
    End Function

    '' <summary>
    '' Implements a string hashing code algorithm equivalent to the .NET 2003 String.GetHashCode()
    '' </summary>
    '' <remarks>
    '' Microsoft have changed the underlying String.GetHashCode algorithm.  This method provides an
    '' equivalent compatible method that can be used on all platforms returning the same result.
    '' </remarks>
    '' <param name="szStr">The string to get the hash code for</param>
    '' <returns>The hash code</returns>
    Private Shared Function HashString(ByVal szStr As String) As Integer
        Dim hash As Integer = 5381
        Dim c As Integer
        Dim i As Integer = 0
        While i < szStr.Length
            c = AscW(szStr.Chars(i))
            Dim shift As Integer = (hash << 5)
            Dim add As Integer = UncheckedAdd(shift, hash)
            hash = add Xor c
            i += 1
        End While
        Return hash
    End Function 'HashString

    ''' <summary> 
    ''' Break the given xml fragment into lines 
    ''' </summary> 
    ''' <param name="xml">The xml fragment</param> 
    ''' <returns></returns> 
    Private Shared Function GetXmlLines(ByVal xml As String) As String()
        Dim crlfs As String() = {"" & Chr(13) & "" & Chr(10) & "", "" & Chr(10) & "", "" & Chr(13) & ""}
        Dim stringWriter As New StringWriter()
        Dim xmlWriter As New XmlTextWriter(stringWriter)
        Dim doc As New XmlDocument()
        doc.LoadXml(xml)
        xmlWriter.Formatting = Formatting.Indented
        doc.WriteTo(xmlWriter)
        Return stringWriter.ToString().Split(crlfs, StringSplitOptions.None)
    End Function

End Class
