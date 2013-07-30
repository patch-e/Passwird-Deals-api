Namespace Deals.Version1

    ''' <summary>
    ''' Represents version 1 deals.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Deal

        Public Property headline As String
        Public Property body As String
        Public Property image As String
        Public Property isExpired As Boolean
        Public Property datePosted As DateTime

    End Class

    ''' <summary>
    ''' Represents the root deal.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class RootDeal

        Public Property deals As List(Of Deal)

    End Class

End Namespace