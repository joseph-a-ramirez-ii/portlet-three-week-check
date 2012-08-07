<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Default_View.ascx.cs" Inherits="CUS.ICS.ThreeWeekCheck.Default_View" %>
<%@ Register TagPrefix="jenzabar" Namespace="Jenzabar.Common.Web.UI.Controls" Assembly="Jenzabar.Common" %>
<%@ Register TagPrefix="fwk" Namespace="Jenzabar.Portal.Framework.Web.UI.Controls" Assembly="Jenzabar.Portal.Framework.Web" %>

<table cellpadding="5" width="100%">
<tr>
<td align="center">
<br />
<asp:Label ID="lblTitleSemester" Text="<b>Semester</b>" runat="server" />
</td>
</tr>
<tr>
<td align="center">
<asp:Label ID="lblTitleCourseName" Text="<b>Course - TEST 333 01 E - CourseName</b>" runat="server" />
</td>
</tr>
<tr>
<td>
<asp:DropDownList ID="ddlCourse"  AutoPostBack="true" runat="server" OnSelectedIndexChanged="ddlCourse_SelectedIndexChanged" />
    <br />
    </td>
    </tr>
</table>

<asp:DataList Width="100%" OnItemDataBound="DataList1_ItemDataBound" ID="DataList1" runat="server" DataSourceID="SqlDataSource1">
<ItemTemplate>
<table cellpadding="5" border="5" width="100%">
<tr>
<td style="width:50%">
<div>
<asp:Image ImageAlign="top" ID="imgPhoto" Width="133" ImageUrl='<%# Eval("ID_NUM") %>' runat="server" />
</div>
<br /><br />
<fwk:MyInfoPopup id="ppMyInfo" NameFormat="LastNameFirst" User='<%#Jenzabar.Portal.Framework.PortalUser.FindByHostID( DataBinder.Eval(Container.DataItem, "ID_NUM").ToString() )%>' ClickableName="true"  runat="server">
</fwk:MyInfoPopup><br />
<asp:Label ID="lblID" runat="server" Text='<%# Eval("ID_NUM") %>' /> <br />
<br />
<asp:Label ID="lblAthlete" runat="server" Text='<%# Eval("Athlete") %>' />
</td>
<td>
    <asp:CheckBox runat="server" Checked='<%# Eval("FLG_ABSENCES") %>' ID="FLG_ABSENCES" Text="Excess Absences" /><br />
    <asp:CheckBox runat="server" Checked='<%# Eval("FLG_WORK") %>' ID="FLG_WORK" Text="Unsatisfactory Work" /><br />
    <asp:CheckBox runat="server" Checked='<%# Eval("FLG_PARTIC") %>' ID="FLG_PARTIC" Text="Not Participating" /><br />
    <asp:CheckBox runat="server" Checked='<%# Eval("FLG_TUTOR") %>' ID="FLG_TUTOR" Text="Suggest Tutoring" /><br />
    <asp:CheckBox runat="server" Checked='<%# Eval("FLG_COUNSEL") %>' ID="FLG_COUNSEL" Text="Suggest Psychological Counseling" /><br />
</td>
</tr>
<tr>
<td colspan=2>
<jenzabar:textboxeditor id="txtText" runat="server" MaxLength="2000000" Innerhtml='<%# Eval("COMMENT") %>' />
</td>
</tr>
</table>
<br />
</ItemTemplate>
</asp:DataList>
<br />
<asp:Button runat="server" ID="btnSave" Text="Submit" OnClick="btnSave_Click" />
<asp:Button runat="server" ID="btnCancel" Text="Cancel" OnClientClick="return confirm('Discard Changes?');" OnClick="btnCancel_Click" />
<asp:Button runat="server" ID="btnBack" Text="Go Back" OnClick="btnCancel_Click" Visible="false" />
<asp:SqlDataSource ID="SqlDataSource1" runat="server" ProviderName="System.Data.SqlClient"></asp:SqlDataSource>