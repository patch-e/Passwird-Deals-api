﻿Namespace Deals.Version1

    ''' <summary>
    ''' Represents deals.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Deal

        Public Property headline As String
        Public Property body As String
        Public Property image As String
        Public Property isExpired As Boolean
        Public Property datePosted As DateTime

        'default constructor
        Public Sub New()
        End Sub

        'copy object constructor
        Public Sub New(deal As Deal)
            Me.body = deal.body
            Me.datePosted = deal.datePosted
            Me.headline = deal.headline
            Me.image = deal.image
            Me.isExpired = deal.isExpired
        End Sub

    End Class

    ''' <summary>
    ''' Represents the root deal.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class RootDeal

        Public Property deals As List(Of Deal)

    End Class

End Namespace