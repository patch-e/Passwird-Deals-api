Imports System.Net
Imports System.Configuration.ConfigurationManager
Imports Newtonsoft.Json

<HandleError()>
Public Class ApiController
    Inherits BaseController

#Region " Single Deal "

    <HttpGet()>
    <OutputCache(Duration:=3600, VaryByParam:="id;type;callback")>
    Function PasswirdDealV2(id As String, type As String) As ActionResult
        Dim deals As New List(Of Deals.Version1.Deal)
        Dim deal = New Deals.Version1.Deal

        Dim jsonString = New WebClient().DownloadString(String.Format(AppSettings("passwirdUrl") + AppSettings("passwirdDealUrl"), id))

        Return PasswirdDealFetchV2(jsonString, type)
    End Function

    <HttpGet()>
    <OutputCache(Duration:=3600, VaryByParam:="id;type;callback")>
    Function PasswirdDeal(id As Integer, type As String) As ActionResult
        Dim deals As New List(Of Deals.Version1.Deal)
        Dim deal = New Deals.Version1.Deal

        Using context As New PushModelContainer
            Try
                Dim lastDeal = (From d In context.LastDeals
                                Where d.id = id).FirstOrDefault()

                If lastDeal IsNot Nothing Then
                    deal.body = lastDeal.body
                    deal.datePosted = lastDeal.datePosted
                    deal.headline = lastDeal.headline
                    deal.image = lastDeal.image
                    deal.isExpired = lastDeal.isExpired

                    deal.id = lastDeal.C_id
                    deal.legacy = lastDeal.legacy
                    deal.hot = lastDeal.hot
                    deal.free = lastDeal.free
                    deal.price = lastDeal.price
                    deal.slug = lastDeal.slug
                    deal.sHeadline = lastDeal.sHeadline
                    deal.author = lastDeal.author
                    deal.expirationDate = IIf(lastDeal.expirationDate.HasValue, lastDeal.expirationDate, Date.Now)

                    deals.Add(deal)

                    Return SerializeDeals(type, deals)
                End If

                Return ServerError()
            Catch ex As Exception
                Throw ex
            End Try
        End Using
    End Function

    Private Function PasswirdDealFetchV2(jsonString As String, type As String) As ActionResult
        Dim dDeal As New Deals.Version2.Deal
        Try
            dDeal = JsonConvert.DeserializeObject(Of Deals.Version2.Deal)(jsonString)
        Catch ex As Exception
            Throw ex
        End Try

        Dim deals As New List(Of Deals.Version1.Deal)
        Dim deal As New Deals.Version1.Deal

        deal.id = dDeal._id
        deal.body = dDeal.text
        deal.datePosted = dDeal.dealDate
        deal.headline = dDeal.title

        If dDeal.images.Count > 0 Then
            If Not dDeal.legacy Then
                deal.image = PrependImagePrefix(dDeal.images.Item(0))
            Else
                deal.image = dDeal.images.Item(0)
            End If
        Else
            deal.image = AppSettings("errorImageUrl")
        End If

        deal.isExpired = dDeal.expired

        deal.legacy = dDeal.legacy
        deal.hot = dDeal.hot
        deal.free = dDeal.free
        deal.price = dDeal.price
        deal.slug = dDeal.slug
        deal.sHeadline = dDeal.sTitle
        deal.author = dDeal.author
        deal.expirationDate = dDeal.expirationDate
        deal.images = dDeal.images

        deals.Add(deal)

        Return SerializeDeals(type, deals)
    End Function

#End Region

#Region " Front Page Deals "

    <HttpGet()>
    <OutputCache(Duration:=60, VaryByParam:="show;e;type;callback")>
    Function Passwird(show As String, e As String, type As String) As ActionResult
        Dim showAsInt As Integer
        If String.IsNullOrEmpty(show) OrElse Not Integer.TryParse(show, showAsInt) OrElse show = 0 Then
            show = 50
        End If

        Dim jsonString = New WebClient().DownloadString(String.Format(AppSettings("passwirdUrl") + AppSettings("passwirdDealsUrl"), show))

        Return PasswirdFetchV2(jsonString, e, type)
    End Function

    Private Function PasswirdFetchV2(jsonString As String, showExpiredDeals As String, type As String) As ActionResult
        Dim newPasswirdDeals As New Deals.Version2.RootDeal
        Try
            newPasswirdDeals = JsonConvert.DeserializeObject(Of Deals.Version2.RootDeal)(jsonString)
        Catch ex As Exception
            Throw ex
        End Try

        Dim deals As New List(Of Deals.Version1.Deal)

        'translate V2 Deals to V1 Deals
        For Each d In newPasswirdDeals.results
            Dim dDate As Date = d.Key
            Dim dDeals As List(Of Deals.Version2.Deal) = d.Value

            For Each dDeal In dDeals
                If (showExpiredDeals = "0" And dDeal.expired) Then Continue For

                Dim deal As New Deals.Version1.Deal

                deal.id = dDeal._id
                deal.body = dDeal.text
                deal.datePosted = dDate
                deal.headline = dDeal.title

                If dDeal.images.Count > 0 Then
                    If Not dDeal.legacy Then
                        deal.image = PrependImagePrefix(dDeal.images.Item(0))
                    Else
                        deal.image = dDeal.images.Item(0)
                    End If
                Else
                    deal.image = AppSettings("errorImageUrl")
                End If

                deal.isExpired = dDeal.expired

                deal.legacy = dDeal.legacy
                deal.hot = dDeal.hot
                deal.free = dDeal.free
                deal.price = dDeal.price
                deal.slug = dDeal.slug
                deal.sHeadline = dDeal.sTitle
                deal.author = dDeal.author
                deal.expirationDate = dDeal.expirationDate
                deal.images = dDeal.images

                deals.Add(deal)
            Next
        Next

        Return SerializeDeals(type, deals)
    End Function

#End Region

#Region " Search Deals "

    <HttpGet()>
    <OutputCache(Duration:=60, VaryByParam:="q;e;type;callback")>
    Function PasswirdSearch(q As String, e As String, type As String) As ActionResult
        Dim jsonString = New WebClient().DownloadString(String.Format(AppSettings("passwirdUrl") + AppSettings("passwirdSearchUrl"), q))

        Return PasswirdSearchFetchV2(jsonString, e, type)
    End Function

    Private Function PasswirdSearchFetchV2(jsonString As String, showExpiredDeals As String, type As String) As ActionResult
        Dim newPasswirdDeals As New List(Of Deals.Version2.SearchResult)
        Try
            newPasswirdDeals = JsonConvert.DeserializeObject(Of List(Of Deals.Version2.SearchResult))(jsonString)
        Catch ex As Exception
            Throw ex
        End Try

        Dim deals As New List(Of Deals.Version1.Deal)

        'translate V2 Deals to V1 Deals
        For Each s In newPasswirdDeals
            If (showExpiredDeals = "0" And s.obj.expired) Then Continue For

            Dim deal As New Deals.Version1.Deal

            deal.id = s.obj._id
            deal.body = s.obj.text
            deal.datePosted = s.obj.dealDate
            deal.headline = s.obj.title

            If s.obj.images.Count > 0 Then
                If Not s.obj.legacy Then
                    deal.image = PrependImagePrefix(s.obj.images.Item(0))
                Else
                    deal.image = s.obj.images.Item(0)
                End If
            Else
                deal.image = AppSettings("errorImageUrl")
            End If

            deal.isExpired = s.obj.expired

            deal.legacy = s.obj.legacy
            deal.hot = s.obj.hot
            deal.free = s.obj.free
            deal.price = s.obj.price
            deal.slug = s.obj.slug
            deal.sHeadline = s.obj.sTitle
            deal.author = s.obj.author
            deal.expirationDate = s.obj.expirationDate
            deal.images = s.obj.images

            deals.Add(deal)
        Next

        Return SerializeDeals(type, deals)
    End Function

#End Region

#Region " Common "

    <HttpGet()>
    Function ServerError() As ActionResult
        Dim deals As New List(Of Deals.Version1.Deal)
        Dim deal = New Deals.Version1.Deal

        deal.headline = api.My.Resources.errorHeadline
        deal.datePosted = Date.Now
        deal.isExpired = False
        deal.body = api.My.Resources.errorBody
        deal.image = AppSettings("errorImageUrl")

        deals.Add(deal)

        Return Json(New With {.deals = deals}, JsonRequestBehavior.AllowGet)
    End Function

    Private Function SerializeDeals(type As String, deals As List(Of Deals.Version1.Deal))
        Select Case type
            Case "json"
                Return Json(deals, JsonRequestBehavior.AllowGet)
            Case "jsonp"
                Return Jsonp(deals)
            Case Else
                Return Json(New With {.deals = deals}, JsonRequestBehavior.AllowGet)
        End Select
    End Function

    Private Function PrependImagePrefix(imageUrl As String) As String
        If Not imageUrl.StartsWith("http") Then
            imageUrl = AppSettings("passwirdImageUrl") + imageUrl
        End If

        Dim path As String = ""
        Dim imageUri = New Uri(imageUrl)

        For i As Integer = 0 To imageUri.Segments.Length - 1
            If i = imageUri.Segments.Length - 1 Then
                path += AppSettings("passwirdImagePrefix") +
                        Regex.Replace(imageUri.Segments(i),
                                      AppSettings("passwirdImagePrefixRegex"),
                                      String.Empty)
            Else
                path += imageUri.Segments(i)
            End If
        Next

        Return imageUri.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped) + path
    End Function

#End Region

#Region " Push Notifications "

    <HttpPost()>
    Function ResetBadgeCount(token As String) As ActionResult
        Dim rows = 0, message = ""

        If String.IsNullOrEmpty(token) Then
            rows = -1
        Else
            Using context As New PushModelContainer
                Try
                    Dim deviceToken = (From dt In context.DeviceTokens
                                       Where dt.Token = token).FirstOrDefault()

                    If deviceToken IsNot Nothing Then
                        deviceToken.BadgeCount = 0

                        rows = context.SaveChanges()
                    End If
                Catch ex As Exception
                    rows = -1
                End Try
            End Using
        End If

        Return GetOutput(rows)
    End Function

    <HttpPost()>
    Function RegisterDeviceToken(token As String, app As String) As ActionResult
        Dim rows = 0, message = "", dev = False

        If String.IsNullOrEmpty(token) Or String.IsNullOrEmpty(app) Then
            rows = -1
        Else
            Using context As New PushModelContainer
                Try
                    Dim deviceToken = (From dt In context.DeviceTokens
                                       Where dt.Token = token).FirstOrDefault()

                    If deviceToken Is Nothing Then
                        deviceToken = New DeviceToken
                        deviceToken.Token = token
                        deviceToken.AppName = app
                        deviceToken.Development = dev
                        deviceToken.BadgeCount = 0
                        deviceToken.Active = True
                        deviceToken.LastActivity = Date.Now()

                        context.DeviceTokens.AddObject(deviceToken)
                    Else
                        deviceToken.Active = True
                    End If

                    rows = context.SaveChanges()
                Catch ex As Exception
                    rows = -1
                End Try
            End Using
        End If

        Return GetOutput(rows)
    End Function

    <HttpPost()>
    Function UnregisterDeviceToken(token As String, app As String) As ActionResult
        Dim rows = 0, message = ""

        If String.IsNullOrEmpty(token) Or String.IsNullOrEmpty(app) Then
            rows = -1
        Else
            Using context As New PushModelContainer
                Try
                    Dim deviceToken = (From dt In context.DeviceTokens
                                       Where dt.Token = token).FirstOrDefault()

                    If deviceToken IsNot Nothing Then
                        deviceToken.Active = False

                        rows = context.SaveChanges()
                    End If
                Catch ex As Exception
                    rows = -1
                End Try
            End Using
        End If

        Return GetOutput(rows)
    End Function

    Private Function GetOutput(rows As Integer) As ViewResult
        Dim message = ""

        Select Case rows
            Case Is > 0
                message = "row_saved"
            Case 0
                message = "row_not_saved"
            Case Else
                message = "row_save_error"
        End Select

        ViewData.Add("message", message)

        Return View("Output")
    End Function

#End Region

End Class