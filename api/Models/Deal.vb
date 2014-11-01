Namespace Deals.Version1

    ''' <summary>
    ''' Represents deals.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Deal

        Public Property id As String
        Public Property headline As String
        Public Property body As String
        Public Property image As String
        Public Property isExpired As Boolean
        Public Property datePosted As DateTime

        Public Property legacy As Boolean
        Public Property hot As Boolean
        Public Property free As Boolean
        Public Property price As String
        Public Property slug As String
        Public Property sHeadline As String
        Public Property author As String
        Public Property expirationDate As DateTime
        Public Property images As List(Of String)

        'default constructor
        Public Sub New()
        End Sub

        'copy object constructor
        Public Sub New(deal As Deal)
            Me.id = deal.id
            Me.body = deal.body
            Me.datePosted = deal.datePosted
            Me.headline = deal.headline
            Me.image = deal.image
            Me.isExpired = deal.isExpired

            Me.legacy = deal.legacy
            Me.hot = deal.hot
            Me.free = deal.free
            Me.price = deal.price
            Me.slug = deal.slug
            Me.sHeadline = deal.sHeadline
            Me.author = deal.author
            Me.expirationDate = deal.expirationDate
            Me.images = deal.images
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