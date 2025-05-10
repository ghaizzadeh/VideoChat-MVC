Imports System.Web.Mvc

Namespace Controllers
    Public Class VideoChatController
        Inherits Controller

        ' GET: VideoChat
        Function Index() As ActionResult
            Return View()
        End Function
    End Class
End Namespace