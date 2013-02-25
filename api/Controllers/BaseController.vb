''' <summary>
''' BaseController inherited by all controllers.
''' </summary>
''' <remarks></remarks>
Public Class BaseController
    Inherits System.Web.Mvc.Controller

    ''' <summary>
    ''' Default action for all view controllers.
    ''' </summary>
    ''' <returns>The ActionResult to the view.</returns>
    ''' <remarks></remarks>
    Function Index() As ActionResult
        Return View()
    End Function

    ''' <summary>
    ''' Overloaded Jsonp function for returning JsonpResult type.
    ''' </summary>
    ''' <param name="data">The data to turn into Jsonp.</param>
    ''' <returns>The JsonpResult.</returns>
    ''' <remarks></remarks>
    Protected Function Jsonp(ByVal data As Object) As JsonpResult
        Return Jsonp(data, Nothing)
    End Function

    ''' <summary>
    ''' Overloaded Jsonp function for returning JsonpResult type.
    ''' </summary>
    ''' <param name="data">The data to turn into Jsonp.</param>
    ''' <param name="contentType">The content type of the result.</param>
    ''' <returns>The JsonpResult.</returns>
    ''' <remarks></remarks>
    Protected Function Jsonp(ByVal data As Object, ByVal contentType As String) As JsonpResult
        Return Jsonp(data, contentType, Nothing)
    End Function

    ''' <summary>
    ''' Overloaded Jsonp function for returning JsonpResult type.
    ''' </summary>
    ''' <param name="data">The data to turn into Jsonp.</param>
    ''' <param name="contentType">The content type of the result.</param>
    ''' <param name="contentEncoding">The content encoding of the result.</param>
    ''' <returns>The JsonpResult.</returns>
    ''' <remarks></remarks>
    Protected Overridable Function Jsonp(ByVal data As Object, ByVal contentType As String, ByVal contentEncoding As Encoding) As JsonpResult
        Return New JsonpResult() With { _
            .Data = data, _
            .ContentType = contentType, _
            .ContentEncoding = contentEncoding _
        }
    End Function

End Class