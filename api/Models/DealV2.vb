Namespace Deals.Version2

    ''' <summary>
    ''' Represents version 2 deals.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Deal

        Public Property title As String
        Public Property expired As Boolean
        Public Property timestamp As String
        Public Property [date] As String
        Public Property link As String
        Public Property body As String
        Public Property image As String

    End Class

    ''' <summary>
    ''' Represents the root deal.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class RootDeal

        Public Property numDeals As Integer
        Public Property deals As List(Of Deal)

    End Class

End Namespace