'
' FILE: RegistryEvaluationMonitor.vb
'
' COPYRIGHT: Copyright 2009 
' Infralution
'
Imports System
Imports Microsoft.Win32
Imports System.Security.Cryptography
Imports System.Text
Imports System.IO
Imports System.Globalization

''' <summary>
''' Defines an implementation of the <see cref="EvaluationMonitor"/> base class that stores
''' the evaluation data in a hidden, encrypted key within the windows registry.
''' </summary>
''' <remarks>
''' <para>
''' Instantiate an instance of this class to read/write the evaluation parameters for the 
''' given product. Note that a sophisticated user could determine the 
''' changes made to registry keys (using registry monitoring software) and restore the state 
''' of these to their pre-installation state (thus resetting the 
''' evaluation period). For this reason it is recommended that you don't rely on this 
''' mechanism alone. You should also consider limiting the functionality of your product 
''' in some way or adding nag screens to discourage long term use of evaluation versions.
''' </para>
''' <para>
''' If you have a data oriented application you can increase the security of evaluations by
''' storing the current <see cref="EvaluationMonitor.UsageCount"/> somewhere in your database each time the 
''' application runs and cross checking this with the number returned by the EvaluationMonitor.
''' </para>
''' </remarks>
#If ILS_PUBLIC_CLASS Then
Public Class RegistryEvaluationMonitor
    Inherits EvaluationMonitor
#Else
Class RegistryEvaluationMonitor
    Inherits EvaluationMonitor
#End If

    #Region "Member Variables"
    
    Private _baseKeyName As String
    
    Private _rootKey As RegistryKey
    
    Private _usageKeyName As String
    Private _firstUseKeyName As String
    Private _lastUseKeyName As String
    
    ' Sub field names for saving data. Designed to
    ' blend in with their surroundings
    '
    Private Const classUsageKey As String = "TypeLib"
    Private Const classFirstUseKey As String = "InprocServer32"
    Private Const classLastUseKey As String = "Control"
    
    Private Const userUsageKey As String = "Software\Microsoft\WAB\WAB4"
    Private Const userFirstUseKey As String = "Software\Microsoft\WAB\WAB Sort State"
    Private Const userLastUseKey As String = "Software\Microsoft\WAB\WAB4\LastFind"
    
    ' parameters for encrypting evaluation data
    '
    Private Shared _desKey As Byte() = New Byte() {&H12, &H75, &Ha8, &Hf1, &H32, &Hed, _ 
    &H13, &Hf2}
    Private Shared _desIV As Byte() = New Byte() {&Ha3, &Hef, &Hd6, &H21, &H37, &H80, _ 
    &Hcc, &Hb1}
    
    
    #End Region
    
    #Region "Public Interface"
    
    ''' <summary>
    ''' Initialize a new instance of the evaluation monitor.
    ''' </summary>
    ''' <param name="countUsageOncePerDay">Should the usage count only be incremented once per day</param>
    ''' <param name="password">A unique password for this product</param>
    ''' <param name="suppressExceptions">
    ''' If true then any exceptions thrown while reading or creating the evaluation data are caught and ignored
    ''' </param>
    ''' <remarks>
    ''' If countUsageOncePerDay is set to true then the <see cref="EvaluationMonitor.UsageCount"/> is only incremented once
    ''' for each day that the product is actually used. If countUsageOncePerDay is false then the 
    ''' <see cref="EvaluationMonitor.UsageCount"/> is incremented each time a new evaluation monitor is instantiated for a product
    ''' </remarks>
    Public Sub New(ByVal password As String, ByVal countUsageOncePerDay As Boolean, ByVal suppressExceptions As Boolean)
        MyBase.New(password, countUsageOncePerDay, suppressExceptions)
    End Sub
    
    ''' <summary>
    ''' Initialize a new instance of the evaluation monitor.
    ''' </summary>
    ''' <param name="password">A unique password for this product</param>
    ''' <remarks>
    ''' Same as calling RegistryEvaluationMonitor(password, false, true)
    ''' </remarks>
    Public Sub New(ByVal password As String)
        Me.New(password, False, True)
    End Sub
    
    #End Region
    
    #Region "Overrides"
    
    ''' <summary>
    ''' Read existing evaluation data (if any) from the registry 
    ''' </summary>
    ''' <param name="productId">The unique product Id</param>
    ''' <param name="firstUseDate">Returns the date the evaluation monitor was first used</param>
    ''' <param name="lastUseDate">Returns the date the evaluation monitor was last used</param>
    ''' <param name="usageCount">Returns the usage count</param>
    Protected Overloads Overrides Sub ReadData(ByVal productId As String, ByRef firstUseDate As DateTime, ByRef lastUseDate As DateTime, ByRef usageCount As Integer)

        Dim os As OperatingSystem = Environment.OSVersion
        Dim forceCurrentUserHive As Boolean = (Environment.UserInteractive And os.Platform = PlatformID.Win32NT And os.Version.Major >= 6)

        If Not forceCurrentUserHive Then

            ' test whether we can write to HKEY_CLASSES_ROOT\CLSID
            '
            Try

                _rootKey = Registry.ClassesRoot.OpenSubKey("CLSID", True)
                _rootKey.CreateSubKey(productId)
                _rootKey.DeleteSubKey(productId, False)

                ' if that succeeded then set up the keys appropriately
                '
                _usageKeyName = classUsageKey
                _firstUseKeyName = classFirstUseKey
                _lastUseKeyName = classLastUseKey
            Catch
                ' can't write to HKEY_CLASSES_ROOT so revert to using HKEY_CURRENT_USER
                '
                If _rootKey IsNot Nothing Then
                    _rootKey.Close()
                    _rootKey = Nothing
                End If
             End Try
        End If

        If _rootKey Is Nothing Then
            _rootKey = Registry.CurrentUser.OpenSubKey("Identities", True)
            If _rootKey Is Nothing Then
                _rootKey = Registry.CurrentUser.CreateSubKey("Identities")
            End If
            _usageKeyName = userUsageKey
            _firstUseKeyName = userFirstUseKey
            _lastUseKeyName = userLastUseKey
        End If

        ' find the base key
        '
        Dim baseKey As RegistryKey = FindBaseKey(productId)

        ' if we found the base key then read the sub keys
        '
        If baseKey IsNot Nothing Then
            _baseKeyName = GetLocalName(baseKey)
            Using baseKey
                Using firstUseKey As RegistryKey = OpenSubKey(baseKey, _firstUseKeyName)
                    firstUseDate = DecryptDate(DirectCast(firstUseKey.GetValue(Nothing), Byte()))
                End Using
                Using lastUseKey As RegistryKey = OpenSubKey(baseKey, _lastUseKeyName)
                    lastUseDate = DecryptDate(DirectCast(lastUseKey.GetValue(Nothing), Byte()))
                End Using
                Using usageKey As RegistryKey = OpenSubKey(baseKey, _usageKeyName)
                    Dim countString As String = Decrypt(DirectCast(usageKey.GetValue(Nothing), Byte()))
                    usageCount = Integer.Parse(countString, CultureInfo.InvariantCulture)
                End Using
            End Using
        End If
    End Sub
    
    ''' <summary>
    ''' Overridden by derived classes to write the updated evaluation data to persistent storage
    ''' </summary>
    ''' <param name="productId">The unique product Id</param>
    ''' <param name="firstUseDate">The date the evaluation monitor was first used</param>
    ''' <param name="lastUseDate">The date the evaluation monitor was last used</param>
    ''' <param name="usageCount">The usage count</param>
    Protected Overloads Overrides Sub WriteData(ByVal productId As String, ByVal firstUseDate As DateTime, ByVal lastUseDate As DateTime, ByVal usageCount As Integer)
        ' delete the previous base key (if any)
        '
        DeleteData(productId)
        
        ' generate a new base key each time
        '
        _baseKeyName = NewBaseKeyName()
        Using baseKey As RegistryKey = _rootKey.CreateSubKey(_baseKeyName)
            Dim passwordData As Byte() = Encrypt(productId)
            baseKey.SetValue(Nothing, passwordData)
            
            Using firstUseKey As RegistryKey = baseKey.CreateSubKey(_firstUseKeyName)
                Using lastUseKey As RegistryKey = baseKey.CreateSubKey(_lastUseKeyName)
                    Using usageKey As RegistryKey = baseKey.CreateSubKey(_usageKeyName)
                        firstUseKey.SetValue(Nothing, EncryptDate(firstUseDate))
                        lastUseKey.SetValue(Nothing, EncryptDate(lastUseDate))
                        usageKey.SetValue(Nothing, Encrypt(usageCount.ToString(CultureInfo.InvariantCulture)))
                    End Using
                End Using
            End Using
        End Using
    End Sub
    
    ''' <summary>
    ''' Delete the evaluation data
    ''' </summary>
    ''' <param name="productId">The unique product Id</param>
    Protected Overloads Overrides Sub DeleteData(ByVal productId As String)
        If _rootKey IsNot Nothing AndAlso _baseKeyName IsNot Nothing Then
            _rootKey.DeleteSubKeyTree(_baseKeyName)
            _baseKeyName = Nothing
        End If
    End Sub
    
    #End Region
    
    #Region "Local Methods"
    
    
    ''' <summary>
    ''' Find the base key for this product
    ''' </summary>
    ''' <param name="productId">The productId to find the key for</param>
    ''' <returns>The base registry key used to store the data</returns>
    Private Function FindBaseKey(ByVal productId As String) As RegistryKey
        Dim passwordData As Byte() = Encrypt(productId)
        Dim classIDs As String() = _rootKey.GetSubKeyNames()
        For Each classID As String In classIDs
            Try
                Dim key As RegistryKey = _rootKey.OpenSubKey(classID)
                Dim keyValue As Object = key.GetValue(Nothing)
                If TypeOf keyValue Is Byte() Then
                    If DataEquals(TryCast(keyValue, Byte()), passwordData) Then
                        Return key
                    End If
                End If
                key.Close()
            Catch
                ' if we can't open the key for some reason just go to the next key
            End Try
        Next
        Return Nothing
    End Function
    
    ''' <summary>
    ''' Return the local name of a given registry key
    ''' </summary>
    ''' <param name="key">The key to get the name of</param>
    ''' <returns>The local name of the registry key</returns>
    Private Function GetLocalName(ByVal key As RegistryKey) As String
        Dim name As String = key.Name
        Dim i As Integer = name.LastIndexOf("\")
        Return name.Substring(i + 1)
    End Function
    
    ''' <summary>
    ''' Return true if the given sub key already exists 
    ''' </summary>
    ''' <param name="key"></param>
    ''' <param name="subKeyName"></param>
    ''' <returns>True if the key exists</returns>
    Private Function KeyExists(ByVal key As RegistryKey, ByVal subKeyName As String) As Boolean
        Try
            Using subKey As RegistryKey = key.OpenSubKey(subKeyName)
                Return (subKey IsNot Nothing)
            End Using
        Catch
            ' if opening the key fails for some reason then assume the key
            ' exists so we won't overwrite it
            '
            Return True
        End Try
    End Function
    
    ''' <summary>
    ''' Generate a new base key name
    ''' </summary>
    Private Function NewBaseKeyName() As String
        ' create the registry key with a unique name each time - this makes it a little
        ' more difficult for people to find the key
        '
        Dim result As String = Nothing
        
        Dim retryCount As Integer = 0
        Do
            Dim guid__1 As String = Guid.NewGuid().ToString().ToUpper()
            
            ' force the GUID to start with a zero - this potentially improves the performance
            ' when locating the key by up to a factor of sixteen
            '
            result = String.Format("{{0{0}}}", guid__1.Substring(1))
            If retryCount > 100 Then
                Throw New InvalidOperationException("Unable to create a unique base key")
            End If
                
            retryCount += 1
        Loop While KeyExists(_rootKey, result)
        Return result
    End Function
    
    ''' <summary>
    ''' Open a given sub key (readonly) 
    ''' </summary>
    ''' <param name="parentKey">The name of the parent key</param>
    ''' <param name="subKeyName">The name of the sub key</param>
    ''' <returns>The sub key</returns>
    Private Function OpenSubKey(ByVal parentKey As RegistryKey, ByVal subKeyName As String) As RegistryKey
        Dim key As RegistryKey = parentKey.OpenSubKey(subKeyName, False)
        If key Is Nothing Then
            Throw New InvalidOperationException(String.Format("SubKey ({0}) does not exist", subKeyName))
        End If
        Return key
    End Function
    
    ''' <summary>
    ''' Encrypt the given text
    ''' </summary>
    ''' <param name="text">The text to encrypt</param>
    ''' <returns>Encrypted byte array</returns>
    Private Function Encrypt(ByVal text As String) As Byte()
        Dim des As New DESCryptoServiceProvider()
        des.Key = _desKey
        des.IV = _desIV
        Dim data As Byte() = ASCIIEncoding.ASCII.GetBytes(text)
        Return des.CreateEncryptor().TransformFinalBlock(data, 0, data.Length)
    End Function
    
    ''' <summary>
    ''' Decrypt the given byte data to text
    ''' </summary>
    ''' <param name="data">The byte data to decrypt</param>
    ''' <returns>The decrypted text</returns>
    Private Function Decrypt(ByVal data As Byte()) As String
        Dim des As New DESCryptoServiceProvider()
        des.Key = _desKey
        des.IV = _desIV
        Dim decryptedData As Byte() = des.CreateDecryptor().TransformFinalBlock(data, 0, data.Length)
        Return ASCIIEncoding.ASCII.GetString(decryptedData)
    End Function
    
    ''' <summary>
    ''' Encrypt a date
    ''' </summary>
    ''' <param name="date">The date to encrypt</param>
    ''' <returns>The encrypted date data</returns>
    Private Function EncryptDate(ByVal [date] As DateTime) As Byte()
        ' add "I" to the date to indicate that the date is encrypted using an invariant culture
        '
        Dim dateString As String = "I" & [date].ToString(CultureInfo.InvariantCulture)
        Return Encrypt(dateString)
    End Function
    
    ''' <summary>
    ''' Decrypt a date
    ''' </summary>
    ''' <param name="data">The data to decrypt</param>
    ''' <returns>The descryped date</returns>
    Private Function DecryptDate(ByVal data As Byte()) As DateTime
        Dim dateString As String = Decrypt(data)
        Dim culture As CultureInfo = CultureInfo.CurrentCulture
        
        ' earlier versions of EvaluationMonitor did not use an invariant culture to
        ' to encrypt the date - check if the date was encrypted using invariant culture
        '
        If dateString(0) = "I"c Then
            dateString = dateString.Substring(1)
            culture = CultureInfo.InvariantCulture
        End If
        Return DateTime.Parse(dateString, culture)
    End Function
    
    ''' <summary>
    ''' Are the contents of the two byte arrays equal
    ''' </summary>
    ''' <param name="a1">The first array</param>
    ''' <param name="a2">The second array </param>
    ''' <returns>True if the contents of the arrays is equal</returns>
    Private Function DataEquals(ByVal a1 As Byte(), ByVal a2 As Byte()) As Boolean
        If a1 is a2 Then
            Return True
        End If
        If (a1 Is Nothing) OrElse (a2 Is Nothing) Then
            Return False
        End If
        If a1.Length <> a2.Length Then
            Return False
        End If
        For i As Integer = 0 To a1.Length - 1
            If a1(i) <> a2(i) Then
                Return False
            End If
        Next
        Return True
    End Function
    
    #End Region
    
    #Region "IDisposable Members"
    
    ''' <summary>
    ''' Free resources used by the EvaluationMonitor
    ''' </summary>
    Public Overloads Overrides Sub Dispose()
        If _rootKey IsNot Nothing Then
            _rootKey.Close()
            _rootKey = Nothing
        End If
    End Sub
    
    #End Region
End Class