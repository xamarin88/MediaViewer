<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Viewer.aspx.cs" Inherits="MediaViewer.Viewer" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="frmMediaViewer" runat="server">
        <!-- Image viewer -->
        <div>
            <b>Please select your image here:</b>
            <hr />
            <asp:DropDownList ID="ddlImages" runat="server" AppendDataBoundItems="true" AutoPostBack="true" OnSelectedIndexChanged="FetchImage">
                <asp:ListItem Text="Select Image" Value="0" />
            </asp:DropDownList>
            <hr />
            <asp:Image ID="imgView" runat="server" Height="393px" Visible="false" Width="490px" />
        </div>
        <!-- Video viewer -->
        <div>
            <b>Please select your video here:</b>
            <hr />
            <asp:DropDownList ID="ddlVideos" runat="server" AppendDataBoundItems="true" AutoPostBack="true" OnSelectedIndexChanged="FetchVideo">
                <asp:ListItem Text="Select Video" Value="0" />
            </asp:DropDownList>
            <hr />
            <video width="400" controls>
                <source type="video/mp4" id="vdView" runat="server" visible="false" Height="300px" Width="400px" />
            </video>
        </div>
    </form>
</body>
</html>
