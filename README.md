## Simple video chat application  using  Signal R and Web RTC in Asp.Net MVC
### 1. create a asp.net mvc application <br>
### 2. Install Signal R package with this command  <br>
Install-Package Microsoft.AspNet.SignalR <br>
### 3. add this tag to appssettings in your webconfig file   <br>
```
 <add key="owin:AutomaticAppStartup" value="true" />  
```
### 4. Add a startup class with this content  <br>
```
Imports Owin
Imports Microsoft.AspNet.SignalR
Public Class Startup
 Public Sub Configuration(app As IAppBuilder)
  app.MapSignalR()
 End Sub
End Class
```
### 5. Add a  chathub class with this content 
```
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
```
### 6. Add a controller called VideoChat and add view to its index action ,use these codes on it 
```
@Code
   ViewData("Title") = "Index"
End Code

<!DOCTYPE html>
<html>
<head>
   <meta name="viewport" content="width=device-width" />
   <title>Index</title>
   <script src="https://cdnjs.cloudflare.com/ajax/libs/jquery/3.6.0/jquery.min.js"></script>
   <script src="https://cdnjs.cloudflare.com/ajax/libs/signalr.js/2.4.2/jquery.signalR.min.js"></script>
   <script src="/Chat/signalr/hubs"></script>
</head>
<body>
   <div>
      <video id="localVideo" autoplay muted></video>
      <video id="remoteVideo" autoplay></video>
      <button id="startCall">شروع تماس</button>
   </div>
   <script>
      const connection = $.hubConnection("/Chat/signalr");
      const chatHub = connection.createHubProxy("chatHub");
      let localStream;
      let peerConnection;
      const config = {
         iceServers: [
            { urls: "stun:stun.l.google.com:19302" } // STUN server برای NAT traversal
         ]
      };
      // شروع اتصال به سرور
      connection.start()
         .done(() => console.log("✅ اتصال SignalR برقرار شد!"))
         .fail(err => console.error("❌ خطا در اتصال SignalR:", err));

      // ارسال درخواست تماس
      document.getElementById("startCall").addEventListener("click", function () {
         chatHub.invoke("StartCall", "کاربر1")
            .fail(err => console.error("❌ خطا در ارسال درخواست:", err));
      });
      // دریافت پیام تماس از سرور
      chatHub.on("receiveCall", function (userName) {
         console.log(userName + " تماس را شروع کرد!");
         startWebRTC(true);
      });
      chatHub.on("receiveMessage", function (type, message) {
         if (type === "offer") {
            const offer = JSON.parse(message);
            startWebRTC(false); // فقط وقتی طرف گیرنده هست، WebRTC را آغاز کن
            peerConnection.setRemoteDescription(new RTCSessionDescription(offer));
            peerConnection.createAnswer()
               .then(answer => {
                  peerConnection.setLocalDescription(answer)
                     .then(() => {
                        chatHub.invoke("SendMessage", "answer", JSON.stringify(answer));
                     })
               });

         } else if (type === "answer") {
            const answer = JSON.parse(message);
            peerConnection.setRemoteDescription(new RTCSessionDescription(answer));
         } else if (type === "ice") {
            const candidate = new RTCIceCandidate(JSON.parse(message));
            peerConnection.addIceCandidate(candidate);
         }
      });

      // پیاده‌سازی WebRTC
      navigator.mediaDevices.getUserMedia({ video: true, audio: true })
         .then(stream => {

            localStream = stream;

            document.getElementById("localVideo").srcObject = stream;
         })
         .catch(error => console.error("خطا در دریافت ویدیو: ", error));

      function startWebRTC(isCaller) {
         peerConnection = new RTCPeerConnection(config);

         // اضافه کردن استریم محلی به peer connection
         localStream.getTracks().forEach(track => {
            peerConnection.addTrack(track, localStream);
         });

         // دریافت استریم طرف مقابل
         peerConnection.ontrack = event => {
            document.getElementById("remoteVideo").srcObject = event.streams[0];
         };

         // ارسال ICE Candidateها به طرف مقابل
         peerConnection.onicecandidate = event => {
            if (event.candidate) {
               chatHub.invoke("SendMessage", "ice", JSON.stringify(event.candidate));
            }
         };
         if (isCaller) {
            peerConnection.createOffer()
               .then(offer => {
                  peerConnection.setLocalDescription(offer);
                  chatHub.invoke("SendMessage", "offer", JSON.stringify(offer));
               });

            peerConnection.createOffer()
               .then(offer => peerConnection.setLocalDescription(offer)
                  .then(() => {
                     chatHub.invoke("SendMessage", "offer", JSON.stringify(peerConnection.localDescription));
                  })
               );
         }
      }
   </script>
</body>
</html>
```



