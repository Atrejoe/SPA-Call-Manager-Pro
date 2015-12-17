' 
' FILE: EncryptedLicense.vb 
' 
' COPYRIGHT: Copyright 2008 
' Infralution 
' 
Imports System
Imports System.ComponentModel
Imports System.Globalization

''' <summary> 
''' Defines an encrypted license for an application or component generated using the Infralution 
''' Licensing System. 
''' </summary> 
''' <remarks> 
''' The Infralution Licensing System provides a secure way of licensing .NET controls, 
''' components and applications. Licenses are protected using public key encryption to 
''' minimize possibility of cracking. 
''' </remarks> 
''' <seealso cref="EncryptedLicenseProvider"/> 
#If ILS_PUBLIC_CLASS Then
Public Class EncryptedLicense
    Inherits License
#Else
Class EncryptedLicense
    Inherits License
#End If

#Region "Member Variables"

    Private _key As String
    Private _serialNo As Int32
    Private _productInfo As String

#End Region

#Region "Public Interface"

    ''' <summary> 
    ''' Create a new Infralution Encrypted License 
    ''' </summary> 
    ''' <param name="key">The key for the license</param> 
    ''' <param name="serialNo">The serial number of the license</param> 
    ''' <param name="productInfo">The product data associated with the license</param> 
    Public Sub New(ByVal key As String, ByVal serialNo As Int32, ByVal productInfo As String)
        _key = key
        _serialNo = serialNo
        _productInfo = productInfo
    End Sub

    ''' <summary> 
    ''' The license key for the license 
    ''' </summary> 
    Public Overloads Overrides ReadOnly Property LicenseKey() As String
        Get
            Return _key
        End Get
    End Property

    ''' <summary> 
    ''' The product data associated with the license 
    ''' </summary> 
    Public ReadOnly Property ProductInfo() As String
        Get
            Return _productInfo
        End Get
    End Property

    ''' <summary> 
    ''' The unique serial no for the license 
    ''' </summary> 
    Public ReadOnly Property SerialNo() As Int32
        Get
            Return _serialNo
        End Get
    End Property

    ''' <summary> 
    ''' Cleans up any resources held by the license 
    ''' </summary> 
    Public Overloads Overrides Sub Dispose()
    End Sub

#End Region

End Class

