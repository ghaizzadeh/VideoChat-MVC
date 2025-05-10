Imports Microsoft.AspNet.SignalR

Public Class ChatHub
 Inherits Hub

 Public Sub SendMessage(userName As String, message As String)
  Clients.Others.receiveMessage(userName, message)
 End Sub

 Public Sub StartCall(userName As String)
  Clients.Others.receiveCall(userName)
 End Sub
End Class