'
' FILE: EvaluationMonitor.vb
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
''' Defines the base class for managing time/usage limited evaluations of products. The 
''' <see cref="RegistryEvaluationMonitor"/> and <see cref="IsolatedStorageEvaluationMonitor"/> classes provide
''' concrete instances of this class that store the data in the registry and isolated storage respectively.
''' </summary>
''' <remarks>
''' <para>
''' Instantiate an instance of a derived class to read/write the evaluation parameters for the 
''' given product. The <see cref="EvaluationMonitor.FirstUseDate"/> is set the first time that
''' the class is instantiated. The <see cref="EvaluationMonitor.LastUseDate"/> and 
''' <see cref="EvaluationMonitor.UsageCount"/> properties are set each time the class is 
''' instantiated (or once per day if the <see cref="EvaluationMonitor.CountUsageOncePerDay"/> property is set to true).
''' </para>
''' <para>
''' Note that evaluation data must be stored somewhere on the users 
''' hard disk. It is therefore not too difficult for a sophisticated user to determine the 
''' changes made either to registry keys or files (using file/registry monitoring software) 
''' and restore the state of these to their pre-installation state (thus resetting the 
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
Public MustInherit Class EvaluationMonitor
    Implements IDisposable
#Else
MustInherit Class EvaluationMonitor
    Implements IDisposable
#End If

#Region "Member Variables"

    Private _productId As String
    Private _usageCount As Integer
    Private _firstUseDate As DateTime
    Private _lastUseDate As DateTime
    Private _invalid As Boolean = False
    Private _countUsageOncePerDay As Boolean

#End Region

#Region "Public Interface"

    ''' <summary>
    ''' Initialize a new instance of the evaluation monitor.
    ''' </summary>
    ''' <param name="countUsageOncePerDay">Should the usage count only be incremented once per day</param>
    ''' <param name="productId">A unique Id for this product</param>
    ''' <param name="suppressExceptions">
    ''' If true then any exceptions thrown while reading or creating the evaluation data are caught and ignored
    ''' </param>
    ''' <remarks>
    ''' If countUsageOncePerDay is set to true then the <see cref="UsageCount"/> is only incremented once
    ''' for each day that the product is actually used. If countUsageOncePerDay is false then the <see cref="UsageCount"/>
    ''' is incremented each time a new evaluation monitor is instantiated for a product
    ''' </remarks>
    Public Sub New(ByVal productId As String, ByVal countUsageOncePerDay As Boolean, ByVal suppressExceptions As Boolean)
        _productId = productId
        _countUsageOncePerDay = countUsageOncePerDay
        _firstUseDate = DateTime.UtcNow
        _lastUseDate = _firstUseDate
        _usageCount = 0

        Try
            ReadData(productId, _firstUseDate, _lastUseDate, _usageCount)
        Catch 
            If Not suppressExceptions Then
                Throw 
            End If
        End Try

        Dim hoursSinceLastUse As Double = DateTime.UtcNow.Subtract(_lastUseDate).TotalHours
        Dim newLastUseDate As DateTime = _lastUseDate

        ' detect winding the clock back on the PC - give them six hours of grace to allow for
        ' daylight saving adjustments etc
        '
        If hoursSinceLastUse < -6.0R Then
            _invalid = True
        Else
            ' if we are incrementing the usage count everytime OR if it more than a day since the 
            ' last use then increment the usage count
            '
            If Not _countUsageOncePerDay OrElse hoursSinceLastUse > 24 Then
                _usageCount += 1
                newLastUseDate = DateTime.UtcNow
            End If
        End If

        ' set the initial usage count
        '
        If _usageCount = 0 Then
            _usageCount = 1
        End If

        Try
            WriteData(productId, _firstUseDate, newLastUseDate, _usageCount)
        Catch  
            If Not suppressExceptions Then
                Throw 
            End If
        End Try
    End Sub

    ''' <summary>
    ''' Get whether the <see cref="UsageCount"/> should be incremented only once per day 
    ''' </summary>
    ''' <remarks>
    ''' If CountUsageOncePerDay is set to true then the <see cref="UsageCount"/> is only incremented once
    ''' for each day that the product is actually used. If CountUsageOncePerDay is false then 
    ''' the <see cref="UsageCount"/> is incremented each time a new evaluation monitor is instantiated for 
    ''' a given product id.  To change this property set the corresponding parameter in the constructor.
    ''' </remarks>
    Public Readonly Property CountUsageOncePerDay() As Boolean
        Get
            Return _countUsageOncePerDay
        End Get
    End Property

    ''' <summary>
    ''' Return the number of times the product has been used 
    ''' </summary>
    ''' <remarks>
    ''' If <see cref="CountUsageOncePerDay"/> is set to true then the UsageCount is only incremented once
    ''' for each day that the product is actually used. If <see cref="CountUsageOncePerDay"/> is false then 
    ''' the UsageCount is incremented each time a new evaluation monitor is instantiated for 
    ''' a given product id. Typically you should instantiate an EvaluationMonitor object just once in your 
    ''' software.
    ''' </remarks>
    Public ReadOnly Property UsageCount() As Integer
        Get
            Return _usageCount
        End Get
    End Property

    ''' <summary>
    ''' Return the date/time the product was first used
    ''' </summary>
    Public ReadOnly Property FirstUseDate() As DateTime
        Get
            Return _firstUseDate
        End Get
    End Property

    ''' <summary>
    ''' Return the date/time the product was last used
    ''' </summary>
    ''' <remarks>
    ''' If <see cref="CountUsageOncePerDay"/> is set to true then this is the date
    ''' at which the usage count was last updated - otherwise it is the date at 
    ''' which the EvaluationMonitor was last created.
    ''' </remarks>
    Public ReadOnly Property LastUseDate() As DateTime
        Get
            Return _lastUseDate
        End Get
    End Property

    ''' <summary>
    ''' Return the number of days since the product was first run.
    ''' </summary>
    Public ReadOnly Property DaysInUse() As Integer
        Get
            Return DateTime.UtcNow.Subtract(_firstUseDate).Days + 1
        End Get
    End Property

    ''' <summary>
    ''' Returns true if the evaluation monitor detects attempts to circumvent
    ''' evaluation limits by tampering with the hidden evaluation data or winding
    ''' the PC clock backwards 
    ''' </summary>
    Public ReadOnly Property Invalid() As Boolean
        Get
            Return _invalid
        End Get
    End Property

    ''' <summary>
    ''' Allows you to reset the evaluation period.
    ''' </summary>
    ''' <remarks>
    ''' This can be useful if a customer needs an extension to their evaluation period
    ''' </remarks>
    Public Sub Reset(ByVal suppressExceptions As Boolean)
        Try
            DeleteData(_productId)
        Catch 
            If Not suppressExceptions Then
                Throw 
            End If
        End Try
        _usageCount = 1
        _firstUseDate = DateTime.UtcNow
        _lastUseDate = _firstUseDate
        _invalid = False
    End Sub

#End Region

#Region "Local Methods"

    ''' <summary>
    ''' Overridden by derived classes to delete the evaluation data
    ''' </summary>
    Protected MustOverride Sub DeleteData(ByVal productId As String)

    ''' <summary>
    ''' Overridden by derived classes to read existing evaluation data (if any) 
    ''' from persistent storage
    ''' </summary>
    ''' <param name="productId">The unique product Id</param>
    ''' <param name="firstUseDate">Returns the date the evaluation monitor was first used</param>
    ''' <param name="lastUseDate">Returns the date the evaluation monitor was last used</param>
    ''' <param name="usageCount">Returns the usage count</param>
    Protected MustOverride Sub ReadData(ByVal productId As String, ByRef firstUseDate As DateTime, ByRef lastUseDate As DateTime, ByRef usageCount As Integer)

    ''' <summary>
    ''' Overridden by derived classes to write the updated evaluation data to persistent storage
    ''' </summary>
    ''' <param name="productId">The unique product Id</param>
    ''' <param name="firstUseDate">The date the evaluation monitor was first used</param>
    ''' <param name="lastUseDate">The date the evaluation monitor was last used</param>
    ''' <param name="usageCount">The usage count</param>
    Protected MustOverride Sub WriteData(ByVal productId As String, ByVal firstUseDate As DateTime, ByVal lastUseDate As DateTime, ByVal usageCount As Integer)

#End Region

#Region "IDisposable Members"

    '''' <summary>
    '''' Free resources used by the EvaluationMonitor
    '''' </summary>
    Public Overridable Sub Dispose() Implements IDisposable.Dispose
    End Sub

#End Region
End Class