Imports System.Net
Imports System.Configuration.ConfigurationManager
Imports HtmlAgilityPack
Imports Newtonsoft.Json
Imports System.Data.Objects

<HandleError()>
Public Class ApiController
    Inherits BaseController

#Region " Passwird Version 1 "

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

                    deals.Add(deal)

                    Return SerializeDeals(type, deals)
                End If

                Return ServerError()
            Catch ex As Exception
                Throw New Exception
            End Try
        End Using
    End Function

    <HttpGet()>
    <OutputCache(Duration:=60, VaryByParam:="e;type;callback")>
    Function Passwird(e As String, type As String) As ActionResult
        Dim webGet = New HtmlWeb()

        webGet.UserAgent = AppSettings("passwirdUserAgent")
        webGet.PreRequest = Function(webRequest As HttpWebRequest)
                                webRequest.Timeout = AppSettings("timeout")
                                Return True
                            End Function

        Dim document = webGet.Load(AppSettings("passwirdUrl"))

        Return PasswirdFetch(document, e, type)
    End Function

    <HttpGet()>
    <OutputCache(Duration:=60, VaryByParam:="q;e;type;callback")>
    Function PasswirdSearch(q As String, e As String, type As String) As ActionResult
        Dim webGet = New HtmlWeb()

        webGet.UserAgent = AppSettings("passwirdUserAgent")
        webGet.PreRequest = Function(webRequest As HttpWebRequest)
                                webRequest.Timeout = AppSettings("timeout")
                                Return True
                            End Function

        Dim document As HtmlAgilityPack.HtmlDocument

        Dim postData As NameValueCollection = New NameValueCollection()
        postData.Add("searchquery", HttpUtility.UrlEncode(q))
        postData.Add("match", AppSettings("passwirdSearchMatch"))
        postData.Add("resultnumber", AppSettings("passwirdSearchResultNumber"))

        Try
            document = webGet.SubmitFormValues(postData, AppSettings("passwirdSearchUrl"))
        Catch ex As Exception
            Throw New Exception
        End Try

        Return PasswirdFetch(document, e, type)
    End Function

    Private Function PasswirdFetch(document As HtmlAgilityPack.HtmlDocument, showExpiredDeals As String, type As String) As ActionResult
        If document.DocumentNode IsNot Nothing Then
            Dim divContent = document.DocumentNode.SelectNodes("//div[@id='content']")

            If divContent IsNot Nothing Then
                Dim nodes = From tag In divContent.Nodes
                            Where (tag.Name = "span" Or tag.Name = "div") And tag.InnerText <> "(expired)" And Not tag.InnerText.StartsWith("HOT")
                            Select tag

                Dim datePosted As DateTime
                Dim deals As New List(Of Deals.Version1.Deal)

                For Each n In nodes
                    If n.Name = "div" Then
                        If IsDate(n.InnerText) Then
                            datePosted = CDate(n.InnerText)
                        End If
                    Else
                        Dim deal = New Deals.Version1.Deal
                        deal.headline = n.InnerText
                        deal.datePosted = datePosted
                        'this DOM parsing code is terrible, but then again so is the HTML at passwird...
                        If n.NextSibling.NextSibling.OuterHtml.StartsWith("<span") Then
                            deal.isExpired = True
                            deal.body = n.NextSibling.NextSibling.NextSibling.NextSibling.Element("span").InnerHtml
                            deal.image = AppSettings("passwirdUrl") + n.NextSibling.NextSibling.NextSibling.NextSibling.Element("img").Attributes("src").Value
                        Else
                            deal.isExpired = False
                            deal.body = n.NextSibling.NextSibling.Element("span").InnerHtml
                            deal.image = AppSettings("passwirdUrl") + n.NextSibling.NextSibling.Element("img").Attributes("src").Value
                        End If
                        If Not (showExpiredDeals = "0" And deal.isExpired) Then
                            deals.Add(deal)
                        End If
                    End If
                Next

                Return SerializeDeals(type, deals)
            End If
        End If

        Return Nothing
    End Function

#End Region

#Region " Passwird Version 2 "

    <HttpGet()>
    <OutputCache(Duration:=3600, VaryByParam:="id;type;callback")>
    Function PasswirdDealV2(id As String, type As String) As ActionResult
        Dim deals As New List(Of Deals.Version1.Deal)
        Dim deal = New Deals.Version1.Deal

        Dim jsonString = New WebClient().DownloadString(String.Format(AppSettings("newPasswirdDealUrl"), id))

        Return PasswirdDealFetchV2(jsonString, type)
    End Function

    <HttpGet()>
    <OutputCache(Duration:=60, VaryByParam:="show;e;type;callback")>
    Function PasswirdV2(show As String, e As String, type As String) As ActionResult
        Dim showAsInt As Integer
        If String.IsNullOrEmpty(show) OrElse Not Integer.TryParse(show, showAsInt) OrElse show = 0 Then
            show = 50
        End If

        Dim jsonString = New WebClient().DownloadString(String.Format(AppSettings("newPasswirdUrl"), show))

        Return PasswirdFetchV2(jsonString, e, type)
    End Function

    <HttpGet()>
    <OutputCache(Duration:=60, VaryByParam:="q;e;type;callback")>
    Function PasswirdSearchV2(q As String, e As String, type As String) As ActionResult
        Dim jsonString = New WebClient().DownloadString(String.Format(AppSettings("newPasswirdSearchUrl"), q))

        Return PasswirdSearchFetchV2(jsonString, e, type)
    End Function

    Private Function PasswirdDealFetchV2(jsonString As String, type As String) As ActionResult
        Dim newPasswirdDeals As New Deals.Version2.Deal
        Try
            newPasswirdDeals = JsonConvert.DeserializeObject(Of Deals.Version2.Deal)(jsonString)
        Catch ex As Exception
            Throw New Exception
        End Try

        Dim deals As New List(Of Deals.Version1.Deal)
        Dim deal As New Deals.Version1.Deal

        deal.body = newPasswirdDeals.text
        deal.datePosted = newPasswirdDeals.dealDate
        deal.headline = newPasswirdDeals.title
        deal.image = newPasswirdDeals.images.Item(0)
        deal.isExpired = newPasswirdDeals.expired

        deals.Add(deal)

        Return SerializeDeals(type, deals)
    End Function

    Private Function PasswirdFetchV2(jsonString As String, showExpiredDeals As String, type As String) As ActionResult
        Dim newPasswirdDeals As New Deals.Version2.RootDeal
        Try
            newPasswirdDeals = JsonConvert.DeserializeObject(Of Deals.Version2.RootDeal)(jsonString)
        Catch ex As Exception
            Throw New Exception
        End Try

        Dim deals As New List(Of Deals.Version1.Deal)

        'translate V2 Deals to V1 Deals
        For Each d In newPasswirdDeals.data
            Dim dDate As Date = d.Key
            Dim dDeals As List(Of Deals.Version2.Deal) = d.Value

            For Each dDeal In dDeals
                If (showExpiredDeals = "0" And dDeal.expired) Then Continue For

                Dim deal As New Deals.Version1.Deal

                deal.body = dDeal.text
                deal.datePosted = dDate
                deal.headline = dDeal.title
                deal.image = dDeal.images.Item(0)
                deal.isExpired = dDeal.expired

                deals.Add(deal)
            Next
        Next

        Return SerializeDeals(type, deals)
    End Function

    Private Function PasswirdSearchFetchV2(jsonString As String, showExpiredDeals As String, type As String) As ActionResult
        Dim newPasswirdDeals As New List(Of Deals.Version2.SearchResult)
        Try
            newPasswirdDeals = JsonConvert.DeserializeObject(Of List(Of Deals.Version2.SearchResult))(jsonString)
        Catch ex As Exception
            Throw New Exception
        End Try

        Dim deals As New List(Of Deals.Version1.Deal)

        'translate V2 Deals to V1 Deals
        For Each s In newPasswirdDeals
            If (showExpiredDeals = "0" And s.obj.expired) Then Continue For

            Dim deal As New Deals.Version1.Deal

            deal.body = s.obj.text
            deal.datePosted = s.obj.dealDate
            deal.headline = s.obj.title
            deal.image = s.obj.images.Item(0)
            deal.isExpired = s.obj.expired

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