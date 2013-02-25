﻿
'------------------------------------------------------------------------------
' <auto-generated>
'    This code was generated from a template.
'
'    Manual changes to this file may cause unexpected behavior in your application.
'    Manual changes to this file will be overwritten if the code is regenerated.
' </auto-generated>
'------------------------------------------------------------------------------

Imports System
Imports System.Data.Objects
Imports System.Data.Objects.DataClasses
Imports System.Data.EntityClient
Imports System.ComponentModel
Imports System.Xml.Serialization
Imports System.Runtime.Serialization

<Assembly: EdmSchemaAttribute("5788b676-45e6-4df1-8267-d769d59db830")>

#Region "Contexts"

''' <summary>
''' No Metadata Documentation available.
''' </summary>
Public Partial Class PushModelContainer
    Inherits ObjectContext

    #Region "Constructors"

    ''' <summary>
    ''' Initializes a new PushModelContainer object using the connection string found in the 'PushModelContainer' section of the application configuration file.
    ''' </summary>
    Public Sub New()
        MyBase.New("name=PushModelContainer", "PushModelContainer")
    MyBase.ContextOptions.LazyLoadingEnabled = true
        OnContextCreated()
    End Sub

    ''' <summary>
    ''' Initialize a new PushModelContainer object.
    ''' </summary>
    Public Sub New(ByVal connectionString As String)
        MyBase.New(connectionString, "PushModelContainer")
    MyBase.ContextOptions.LazyLoadingEnabled = true
        OnContextCreated()
    End Sub

    ''' <summary>
    ''' Initialize a new PushModelContainer object.
    ''' </summary>
    Public Sub New(ByVal connection As EntityConnection)
        MyBase.New(connection, "PushModelContainer")
    MyBase.ContextOptions.LazyLoadingEnabled = true
        OnContextCreated()
    End Sub

    #End Region

    #Region "Partial Methods"

    Partial Private Sub OnContextCreated()
    End Sub

    #End Region

    #Region "ObjectSet Properties"

    ''' <summary>
    ''' No Metadata Documentation available.
    ''' </summary>
    Public ReadOnly Property DeviceTokens() As ObjectSet(Of DeviceToken)
        Get
            If (_DeviceTokens Is Nothing) Then
                _DeviceTokens = MyBase.CreateObjectSet(Of DeviceToken)("DeviceTokens")
            End If
            Return _DeviceTokens
        End Get
    End Property

    Private _DeviceTokens As ObjectSet(Of DeviceToken)

    ''' <summary>
    ''' No Metadata Documentation available.
    ''' </summary>
    Public ReadOnly Property LastDeals() As ObjectSet(Of LastDeal)
        Get
            If (_LastDeals Is Nothing) Then
                _LastDeals = MyBase.CreateObjectSet(Of LastDeal)("LastDeals")
            End If
            Return _LastDeals
        End Get
    End Property

    Private _LastDeals As ObjectSet(Of LastDeal)

    #End Region
    #Region "AddTo Methods"

    ''' <summary>
    ''' Deprecated Method for adding a new object to the DeviceTokens EntitySet. Consider using the .Add method of the associated ObjectSet(Of T) property instead.
    ''' </summary>
    Public Sub AddToDeviceTokens(ByVal deviceToken As DeviceToken)
        MyBase.AddObject("DeviceTokens", deviceToken)
    End Sub

    ''' <summary>
    ''' Deprecated Method for adding a new object to the LastDeals EntitySet. Consider using the .Add method of the associated ObjectSet(Of T) property instead.
    ''' </summary>
    Public Sub AddToLastDeals(ByVal lastDeal As LastDeal)
        MyBase.AddObject("LastDeals", lastDeal)
    End Sub

    #End Region
End Class

#End Region
#Region "Entities"

''' <summary>
''' No Metadata Documentation available.
''' </summary>
<EdmEntityTypeAttribute(NamespaceName:="PushModel", Name:="DeviceToken")>
<Serializable()>
<DataContractAttribute(IsReference:=True)>
Public Partial Class DeviceToken
    Inherits EntityObject
    #Region "Factory Method"

    ''' <summary>
    ''' Create a new DeviceToken object.
    ''' </summary>
    ''' <param name="token">Initial value of the Token property.</param>
    ''' <param name="appName">Initial value of the AppName property.</param>
    ''' <param name="development">Initial value of the Development property.</param>
    ''' <param name="badgeCount">Initial value of the BadgeCount property.</param>
    Public Shared Function CreateDeviceToken(token As Global.System.String, appName As Global.System.String, development As Global.System.Boolean, badgeCount As Global.System.Int32) As DeviceToken
        Dim deviceToken as DeviceToken = New DeviceToken
        deviceToken.Token = token
        deviceToken.AppName = appName
        deviceToken.Development = development
        deviceToken.BadgeCount = badgeCount
        Return deviceToken
    End Function

    #End Region
    #Region "Primitive Properties"

    ''' <summary>
    ''' No Metadata Documentation available.
    ''' </summary>
    <EdmScalarPropertyAttribute(EntityKeyProperty:=true, IsNullable:=false)>
    <DataMemberAttribute()>
    Public Property Token() As Global.System.String
        Get
            Return _Token
        End Get
        Set
            If (_Token <> Value) Then
                OnTokenChanging(value)
                ReportPropertyChanging("Token")
                _Token = StructuralObject.SetValidValue(value, false)
                ReportPropertyChanged("Token")
                OnTokenChanged()
            End If
        End Set
    End Property

    Private _Token As Global.System.String
    Private Partial Sub OnTokenChanging(value As Global.System.String)
    End Sub

    Private Partial Sub OnTokenChanged()
    End Sub

    ''' <summary>
    ''' No Metadata Documentation available.
    ''' </summary>
    <EdmScalarPropertyAttribute(EntityKeyProperty:=false, IsNullable:=false)>
    <DataMemberAttribute()>
    Public Property AppName() As Global.System.String
        Get
            Return _AppName
        End Get
        Set
            OnAppNameChanging(value)
            ReportPropertyChanging("AppName")
            _AppName = StructuralObject.SetValidValue(value, false)
            ReportPropertyChanged("AppName")
            OnAppNameChanged()
        End Set
    End Property

    Private _AppName As Global.System.String
    Private Partial Sub OnAppNameChanging(value As Global.System.String)
    End Sub

    Private Partial Sub OnAppNameChanged()
    End Sub

    ''' <summary>
    ''' No Metadata Documentation available.
    ''' </summary>
    <EdmScalarPropertyAttribute(EntityKeyProperty:=false, IsNullable:=false)>
    <DataMemberAttribute()>
    Public Property Development() As Global.System.Boolean
        Get
            Return _Development
        End Get
        Set
            OnDevelopmentChanging(value)
            ReportPropertyChanging("Development")
            _Development = StructuralObject.SetValidValue(value)
            ReportPropertyChanged("Development")
            OnDevelopmentChanged()
        End Set
    End Property

    Private _Development As Global.System.Boolean
    Private Partial Sub OnDevelopmentChanging(value As Global.System.Boolean)
    End Sub

    Private Partial Sub OnDevelopmentChanged()
    End Sub

    ''' <summary>
    ''' No Metadata Documentation available.
    ''' </summary>
    <EdmScalarPropertyAttribute(EntityKeyProperty:=false, IsNullable:=false)>
    <DataMemberAttribute()>
    Public Property BadgeCount() As Global.System.Int32
        Get
            Return _BadgeCount
        End Get
        Set
            OnBadgeCountChanging(value)
            ReportPropertyChanging("BadgeCount")
            _BadgeCount = StructuralObject.SetValidValue(value)
            ReportPropertyChanged("BadgeCount")
            OnBadgeCountChanged()
        End Set
    End Property

    Private _BadgeCount As Global.System.Int32
    Private Partial Sub OnBadgeCountChanging(value As Global.System.Int32)
    End Sub

    Private Partial Sub OnBadgeCountChanged()
    End Sub

    #End Region
End Class

''' <summary>
''' No Metadata Documentation available.
''' </summary>
<EdmEntityTypeAttribute(NamespaceName:="PushModel", Name:="LastDeal")>
<Serializable()>
<DataContractAttribute(IsReference:=True)>
Public Partial Class LastDeal
    Inherits EntityObject
    #Region "Factory Method"

    ''' <summary>
    ''' Create a new LastDeal object.
    ''' </summary>
    ''' <param name="id">Initial value of the id property.</param>
    ''' <param name="headline">Initial value of the headline property.</param>
    ''' <param name="body">Initial value of the body property.</param>
    ''' <param name="image">Initial value of the image property.</param>
    ''' <param name="isExpired">Initial value of the isExpired property.</param>
    ''' <param name="datePosted">Initial value of the datePosted property.</param>
    Public Shared Function CreateLastDeal(id As Global.System.Int32, headline As Global.System.String, body As Global.System.String, image As Global.System.String, isExpired As Global.System.Boolean, datePosted As Global.System.DateTime) As LastDeal
        Dim lastDeal as LastDeal = New LastDeal
        lastDeal.id = id
        lastDeal.headline = headline
        lastDeal.body = body
        lastDeal.image = image
        lastDeal.isExpired = isExpired
        lastDeal.datePosted = datePosted
        Return lastDeal
    End Function

    #End Region
    #Region "Primitive Properties"

    ''' <summary>
    ''' No Metadata Documentation available.
    ''' </summary>
    <EdmScalarPropertyAttribute(EntityKeyProperty:=true, IsNullable:=false)>
    <DataMemberAttribute()>
    Public Property id() As Global.System.Int32
        Get
            Return _id
        End Get
        Set
            If (_id <> Value) Then
                OnidChanging(value)
                ReportPropertyChanging("id")
                _id = StructuralObject.SetValidValue(value)
                ReportPropertyChanged("id")
                OnidChanged()
            End If
        End Set
    End Property

    Private _id As Global.System.Int32
    Private Partial Sub OnidChanging(value As Global.System.Int32)
    End Sub

    Private Partial Sub OnidChanged()
    End Sub

    ''' <summary>
    ''' No Metadata Documentation available.
    ''' </summary>
    <EdmScalarPropertyAttribute(EntityKeyProperty:=false, IsNullable:=false)>
    <DataMemberAttribute()>
    Public Property headline() As Global.System.String
        Get
            Return _headline
        End Get
        Set
            OnheadlineChanging(value)
            ReportPropertyChanging("headline")
            _headline = StructuralObject.SetValidValue(value, false)
            ReportPropertyChanged("headline")
            OnheadlineChanged()
        End Set
    End Property

    Private _headline As Global.System.String
    Private Partial Sub OnheadlineChanging(value As Global.System.String)
    End Sub

    Private Partial Sub OnheadlineChanged()
    End Sub

    ''' <summary>
    ''' No Metadata Documentation available.
    ''' </summary>
    <EdmScalarPropertyAttribute(EntityKeyProperty:=false, IsNullable:=false)>
    <DataMemberAttribute()>
    Public Property body() As Global.System.String
        Get
            Return _body
        End Get
        Set
            OnbodyChanging(value)
            ReportPropertyChanging("body")
            _body = StructuralObject.SetValidValue(value, false)
            ReportPropertyChanged("body")
            OnbodyChanged()
        End Set
    End Property

    Private _body As Global.System.String
    Private Partial Sub OnbodyChanging(value As Global.System.String)
    End Sub

    Private Partial Sub OnbodyChanged()
    End Sub

    ''' <summary>
    ''' No Metadata Documentation available.
    ''' </summary>
    <EdmScalarPropertyAttribute(EntityKeyProperty:=false, IsNullable:=false)>
    <DataMemberAttribute()>
    Public Property image() As Global.System.String
        Get
            Return _image
        End Get
        Set
            OnimageChanging(value)
            ReportPropertyChanging("image")
            _image = StructuralObject.SetValidValue(value, false)
            ReportPropertyChanged("image")
            OnimageChanged()
        End Set
    End Property

    Private _image As Global.System.String
    Private Partial Sub OnimageChanging(value As Global.System.String)
    End Sub

    Private Partial Sub OnimageChanged()
    End Sub

    ''' <summary>
    ''' No Metadata Documentation available.
    ''' </summary>
    <EdmScalarPropertyAttribute(EntityKeyProperty:=false, IsNullable:=false)>
    <DataMemberAttribute()>
    Public Property isExpired() As Global.System.Boolean
        Get
            Return _isExpired
        End Get
        Set
            OnisExpiredChanging(value)
            ReportPropertyChanging("isExpired")
            _isExpired = StructuralObject.SetValidValue(value)
            ReportPropertyChanged("isExpired")
            OnisExpiredChanged()
        End Set
    End Property

    Private _isExpired As Global.System.Boolean
    Private Partial Sub OnisExpiredChanging(value As Global.System.Boolean)
    End Sub

    Private Partial Sub OnisExpiredChanged()
    End Sub

    ''' <summary>
    ''' No Metadata Documentation available.
    ''' </summary>
    <EdmScalarPropertyAttribute(EntityKeyProperty:=false, IsNullable:=false)>
    <DataMemberAttribute()>
    Public Property datePosted() As Global.System.DateTime
        Get
            Return _datePosted
        End Get
        Set
            OndatePostedChanging(value)
            ReportPropertyChanging("datePosted")
            _datePosted = StructuralObject.SetValidValue(value)
            ReportPropertyChanged("datePosted")
            OndatePostedChanged()
        End Set
    End Property

    Private _datePosted As Global.System.DateTime
    Private Partial Sub OndatePostedChanging(value As Global.System.DateTime)
    End Sub

    Private Partial Sub OndatePostedChanged()
    End Sub

    #End Region
End Class

#End Region

