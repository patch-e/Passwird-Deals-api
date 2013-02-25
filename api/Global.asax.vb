Public Class MvcApplication
    Inherits System.Web.HttpApplication

    Shared Sub RegisterRoutes(ByVal routes As RouteCollection)
        routes.IgnoreRoute("{resource}.axd/{*pathInfo}")

        Dim actions = {"Passwird", "PasswirdSearch",
                       "PasswirdV2", "PasswirdSearchV2",
                       "RegisterDeviceToken", "UnregisterDeviceToken",
                       "ResetBadgeCount", "ServerError"}
        routes.MapRoute(
            "Default",
            "",
            New With {.controller = "api", .action = "Index"}
        )
        For Each action In actions
            routes.MapRoute(
                action,
                action,
                New With {.controller = "api", .action = action}
            )
        Next
    End Sub

    Sub Application_Start()
        AreaRegistration.RegisterAllAreas()
        RegisterRoutes(RouteTable.Routes)
    End Sub

End Class