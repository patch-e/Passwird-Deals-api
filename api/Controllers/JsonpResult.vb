Imports System.Web.Script.Serialization

''' <summary>
''' Provides Jsonp output support.
''' </summary>
''' <remarks></remarks>
Public Class JsonpResult
    Inherits JsonResult

    Public Overrides Sub ExecuteResult(ByVal context As ControllerContext)
        If context Is Nothing Then
            Throw New ArgumentNullException("context")
        End If

        Dim response As HttpResponseBase = context.HttpContext.Response

        If Not String.IsNullOrEmpty(ContentType) Then
            response.ContentType = ContentType
        Else
            response.ContentType = "application/json"
        End If

        If ContentEncoding IsNot Nothing Then
            response.ContentEncoding = ContentEncoding
        End If

        If Data IsNot Nothing Then
            Dim request As HttpRequestBase = context.HttpContext.Request

            Dim serializer As New JavaScriptSerializer()
            If request.Params("callback") IsNot Nothing Then
                response.Write(request.Params("callback") + "(" & serializer.Serialize(Data) & ")")
            Else
                response.Write(serializer.Serialize(Data))
            End If
        End If
    End Sub

End Class