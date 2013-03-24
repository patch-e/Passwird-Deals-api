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
    <OutputCache(Duration:=60, VaryByParam:="e")>
    Function Passwird(type As String, e As String) As ActionResult
        Dim webGet = New HtmlWeb()

        webGet.UserAgent = AppSettings("passwirdUserAgent")
        webGet.PreRequest = Function(webRequest As HttpWebRequest)
                                webRequest.Timeout = AppSettings("timeout")
                                Return True
                            End Function

        Dim document = webGet.Load(AppSettings("passwirdUrl"))

        Return PasswirdFetch(type, document, e)
    End Function

    <HttpGet()>
    <OutputCache(Duration:=60, VaryByParam:="q")>
    Function PasswirdSearch(type As String, q As String, e As String) As ActionResult
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

        document = webGet.SubmitFormValues(postData, AppSettings("passwirdSearchUrl"))

        Return PasswirdFetch(type, document, e)
    End Function

    Private Function PasswirdFetch(type As String, document As HtmlAgilityPack.HtmlDocument, showDeals As String) As ActionResult
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
                        If Not (showDeals = "0" And deal.isExpired) Then
                            deals.Add(deal)
                        End If
                    End If
                Next

                Select Case type
                    Case "json"
                        Return Json(deals, JsonRequestBehavior.AllowGet)
                    Case "jsonp"
                        Return Jsonp(deals)
                    Case Else
                        Return Json(New With {.deals = deals}, JsonRequestBehavior.AllowGet)
                End Select
            End If
        End If

        Return Nothing
    End Function

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

#End Region

#Region " Passwird Version 2 "

    <HttpGet()>
    <OutputCache(Duration:=60, VaryByParam:="e")>
    Function PasswirdV2(type As String, e As String) As ActionResult
        Dim jsonString = New WebClient().DownloadString(AppSettings("newPasswirdUrl"))

        Return PasswirdFetchV2(type, jsonString, e)
    End Function

    <HttpGet()>
    <OutputCache(Duration:=60, VaryByParam:="q")>
    Function PasswirdSearchV2(type As String, q As String, e As String) As ActionResult
        Dim jsonString = New WebClient().DownloadString(String.Format(AppSettings("newPasswirdSearchUrl"), q))

        Return PasswirdFetchV2(type, jsonString, e)
    End Function

    Private Function PasswirdFetchV2(type As String, jsonString As String, showDeals As String) As ActionResult
        Dim newPasswirdDeals As New Deals.Version2.RootDeal
        Try
            newPasswirdDeals = JsonConvert.DeserializeObject(Of Deals.Version2.RootDeal)(jsonString)
        Catch ex As Exception
            'caught bad input or passwird outage
            Throw New Exception
        End Try

        Dim deals As New List(Of Deals.Version2.Deal)
        If newPasswirdDeals.numDeals > 0 Then
            For Each d In newPasswirdDeals.deals
                Dim deal = New Deals.Version2.Deal

                deal.title = d.title
                deal.body = d.body
                deal.date = d.date
                deal.image = d.image
                deal.expired = IIf(d.expired = "yes", True, False)
                deal.link = d.link
                deal.timestamp = d.timestamp

                If Not (showDeals = "0" And deal.expired) Then
                    deals.Add(deal)
                End If

            Next
        End If

        Select Case type
            Case "json"
                Return Json(deals, JsonRequestBehavior.AllowGet)
            Case "jsonp"
                Return Jsonp(deals)
            Case Else
                Return Json(New With {.deals = deals}, JsonRequestBehavior.AllowGet)
        End Select
    End Function

    <HttpGet()>
    Function ServerErrorV2() As ActionResult
        Dim deals As New List(Of Deals.Version2.Deal)
        Dim deal = New Deals.Version2.Deal

        deal.title = api.My.Resources.errorHeadline
        deal.date = Date.Now
        deal.expired = False
        deal.body = api.My.Resources.errorBody
        deal.image = AppSettings("errorImageUrl")

        deals.Add(deal)

        Return Json(New With {.deals = deals}, JsonRequestBehavior.AllowGet)
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
                        deviceToken.AppName = app
                        deviceToken.Token = token
                        deviceToken.Development = dev
                        deviceToken.BadgeCount = 0
                    Else
                        deviceToken.Development = dev
                        deviceToken.BadgeCount = 0
                    End If
                    context.DeviceTokens.AddObject(deviceToken)

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
                        context.DeviceTokens.DeleteObject(deviceToken)

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