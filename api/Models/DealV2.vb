﻿Namespace Deals.Version2

    ''' <summary>
    ''' Represents version 2 deals.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Deal

        Public Property slug As String
        Public Property title As String
        Public Property text As String
        Public Property sTitle As String
        Public Property author As String
        Public Property _id As String
        Public Property legacy As Boolean
        Public Property images As List(Of String)
        Public Property hot As Boolean
        Public Property expired As Boolean
        Public Property dealDate As DateTime

    End Class

    ''' <summary>
    ''' Represents the root deal.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class RootDeal

        Public Property pages As Integer
        Public Property page As Integer
        Public Property count As Integer
        Public Property query As String
        Public Property data As Dictionary(Of String, List(Of Deal))
        Public Property view As String
        Public Property showExpired As Boolean

    End Class

End Namespace