<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ForgotPasswordConfirm.aspx.cs" Inherits="EXPAppManagement.ForgotPasswordConfirm" %>
<!DOCTYPE html>
<html>
<head>
    <meta charset="UTF-8">
    <meta http-equiv="X-UA-Compatible" content="IE=edge">
    <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no">
    <meta name="description" content="Metro, a sleek, intuitive, and powerful framework for faster and easier web development for Windows Metro Style.">
    <meta name="keywords" content="HTML, CSS, JS, JavaScript, framework, metro, front-end, frontend, web development">
    <meta name="author" content="Sergey Pimenov and Metro UI CSS contributors">


    <link href="/css/metro-icons.css" rel="stylesheet" />
    <link href="/css/metro-all.css?ver=@@b-version:47876" rel="stylesheet" />
    <link href="/css/metro.css" rel="stylesheet" />


    <title>Kindredfm App Management</title>

    <style>
        .login-form {
            width: 500px;
            height: auto;
            top: 50%;
            margin-top: -160px;
        }
    </style>

</head>


<body class="h-vh-100 bg-gray" style="background-image:url()">
 <form id="form1" class="login-form bg-white p-6 mx-auto border bd-default win-shadow" runat="server">
      <img src="Images/kindred-logo.png" style="max-width:75%;max-height:75px" />
      <div>
         <hr />
         <asp:PlaceHolder runat="server" ID="LoginStatus" Visible="false">
            <p>
               <asp:Literal runat="server" ID="StatusText" />
            </p>
         </asp:PlaceHolder>
         <asp:PlaceHolder runat="server" ID="LoginForm" Visible="false">
            <div style="margin-bottom: 10px">
               <asp:Label runat="server" AssociatedControlID="UserName">User name</asp:Label>
               <div>
                  <asp:TextBox runat="server" ID="UserName" />
               </div>
            </div>
            <div style="margin-bottom: 10px">
               <asp:Label runat="server" AssociatedControlID="Password">New Password</asp:Label>
               <div>
                  <asp:TextBox runat="server" ID="Password" TextMode="Password" />
               </div>
            </div>
              <div style="margin-bottom: 10px">
               <asp:Label runat="server" AssociatedControlID="VerificationCode">Verification Token</asp:Label>
               <div>
                  <asp:TextBox runat="server" ID="VerificationCode" />
               </div>
            </div>
            <div style="margin-bottom: 10px">
               <div>
                  <asp:Button runat="server" OnClick="SignIn" Text="Reset Password" />
               </div>
            </div>
         </asp:PlaceHolder>
      
      </div>
   </form>
    <script src="//metroui.org.ua/js/jquery-3.3.1.min.js"></script>
    <script src="//metroui.org.ua/metro/js/metro.js"></script>

    <script>
        function invalidForm() {
            var form = $(".login-form");
            form.addClass("ani-ring");
            setTimeout(function () {
                form.removeClass("ani-ring");
            }, 1000);
        }
        function validateForm() {
            $(".login-form").animate({
                opacity: 0
            });
        }
    </script>

</body>
</html>



