<%--  License

// Copyright (c) 2011, ClearCanvas Inc.
// All rights reserved.
// http://www.ClearCanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.ClearCanvas.ca/OSLv3.0

--%>

<%@ Import Namespace="Resources" %>

<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DeletedStudiesSearchPanel.ascx.cs" Inherits="Macro.ImageServer.Web.Application.Pages.Admin.Audit.DeletedStudies.DeletedStudiesSearchPanel" %>

<%@ Register Src="SearchResultGridView.ascx" TagName="SearchResultGridView" TagPrefix="localAsp" %>

<asp:ScriptManagerProxy runat="server" ID="ScriptManagerProxy" />

<asp:UpdatePanel ID="SearchUpdatePanel" runat="server" UpdateMode="conditional">
    <ContentTemplate>
            <asp:Table ID="Table1" runat="server" BorderStyle="none">
                <asp:TableRow>
                    <asp:TableCell HorizontalAlign="right" VerticalAlign="Bottom" >                    
                       <table cellpadding="0" cellspacing="0"  width="100%"  border="0" >
                            <tr>
                                <td align="left">
                                <asp:Panel ID="Panel6" runat="server" CssClass="SearchPanelContentWithoutTabs" DefaultButton="SearchButton">
                                    <table width="100%" cellpadding="0" cellspacing="0">
                                        <tr>
                                        <td>
                                            <table cellpadding="0" cellspacing="0">
                                            <tr>

                                            <td align="left" valign="bottom">
                                                <asp:Label ID="Label1" runat="server" Text="<%$Resources: SearchFieldLabels,PatientName %>" CssClass="SearchTextBoxLabel"
                                                    EnableViewState="False" /><br />
                                                <asp:TextBox ID="PatientName" runat="server" CssClass="SearchTextBox" ToolTip="<%$Resources: Tooltips, SearchByPatientName %>" />
                                            </td>
                                            <td align="left" valign="bottom">
                                                <asp:Label ID="Label2" runat="server" Text="<%$Resources: SearchFieldLabels,PatientID%>" CssClass="SearchTextBoxLabel"
                                                    EnableViewState="False" /><br />
                                                <asp:TextBox ID="PatientId" runat="server" CssClass="SearchTextBox" ToolTip="<%$Resources: Tooltips, SearchByPatientID %>" />
                                            </td>
                                            <td align="left" valign="bottom">
                                                <asp:Label ID="Label3" runat="server" Text="<%$Resources: SearchFieldLabels,AccessionNumber %>" CssClass="SearchTextBoxLabel"
                                                    EnableViewState="False" /><br />
                                                <asp:TextBox ID="AccessionNumber" runat="server" CssClass="SearchTextBox" ToolTip="<%$Resources: Tooltips, SearchByAccessionNumber %>" />
                                            </td>
                                            <td align="left" valign="bottom">
                                                <asp:Label ID="Label5" runat="server" Text="<%$Resources: SearchFieldLabels,StudyDate %>" CssClass="SearchTextBoxLabel" EnableViewState="false"/>
                                                <asp:LinkButton ID="ClearStudyDateButton" runat="server" Text="X" CssClass="SmallLink"/><br />
                                                <ccUI:TextBox ID="StudyDate" runat="server" CssClass="SearchDateBox" ReadOnly="true" ToolTip="<%$Resources: Tooltips, SearchByStudyDate%>" />
                                            </td>
                                            <td align="left" valign="bottom">
                                                <asp:Label ID="Label4" runat="server" Text="<%$Resources: SearchFieldLabels,StudyDescription %>" CssClass="SearchTextBoxLabel"
                                                    EnableViewState="False" /><br />
                                                <asp:TextBox ID="StudyDescription" runat="server"  CssClass="SearchTextBox" ToolTip="<%$Resources: Tooltips, SearchByDescription %>" />
                                            </td>   
                                            <td align="left" valign="bottom">
                                                <asp:Label ID="Label6" runat="server" Text="<%$Resources: SearchFieldLabels,DeletedBy%>" CssClass="SearchTextBoxLabel"
                                                    EnableViewState="False" /><br />
                                                <asp:TextBox ID="DeletedBy" runat="server"  CssClass="SearchTextBox" ToolTip="<%$Resources: Tooltips, SearchByPersonWhoDeletedStudies %>" />
                                            </td>                                            
                                            <td valign="bottom">
                                                <asp:Panel ID="Panel1" runat="server" CssClass="SearchButtonPanel"><ccUI:ToolbarButton ID="SearchButton" runat="server" SkinID="<%$Image:SearchIcon%>" OnClick="SearchButton_Click" /></asp:Panel>
                                            </td>
                                            </tr>
                                            </table>
                                        </td>
                                     </tr>
                                    </table>
                                </asp:Panel>
                                </td>
                            </tr>
                        </table>

                        <ccUI:CalendarExtender ID="StudyDateCalendarExtender" runat="server" TargetControlID="StudyDate"
                            CssClass="Calendar">
                        </ccUI:CalendarExtender>
                    </asp:TableCell>
                </asp:TableRow>
                <asp:TableRow>
                    <asp:TableCell>
                        <table width="100%" cellpadding="3" cellspacing="0" class="ToolbarButtonPanel">
                            <tr><td >
                            <asp:UpdatePanel ID="ToolBarUpdatePanel" runat="server" UpdateMode="Conditional">
                                <ContentTemplate>
                                    <asp:Panel ID="ToolbarButtons" runat="server" CssClass="ToolbarButtons">
                                        <ccUI:ToolbarButton ID="ViewStudyDetailsButton" runat="server" SkinID="<%$Image:ViewDetailsButton%>" OnClick="ViewDetailsButtonClicked" />
                                        <ccUI:ToolbarButton ID="DeleteButton" runat="server" SkinID="<%$Image:DeleteButton%>" OnClick="DeleteButtonClicked" />
                                    </asp:Panel>
                             </ContentTemplate>
                          </asp:UpdatePanel>                  
                        </td></tr>
                        <tr><td>

                         <asp:Panel ID="Panel2" runat="server"   CssClass="SearchPanelResultContainer">
                            <table width="100%" cellpadding="0" cellspacing="0">
                                 <tr><td><ccAsp:GridPager ID="GridPagerTop" runat="server"  TargetControlID="SearchResultGridView1"/></td></tr>                        
                                <tr><td style="background-color: white;">
                                
                                    <localAsp:SearchResultGridView ID="SearchResultGridView1" runat="server" />
                                
                                </td></tr>
                            </table>                        
                        </asp:Panel>
                        </td>
                        </tr>
                        </table>
                    </asp:TableCell>
                </asp:TableRow>
            </asp:Table>
            
           
    </ContentTemplate>
    <Triggers>
        <asp:AsyncPostBackTrigger ControlID="SearchButton" EventName="Click" />
    </Triggers>

</asp:UpdatePanel>
 



                                