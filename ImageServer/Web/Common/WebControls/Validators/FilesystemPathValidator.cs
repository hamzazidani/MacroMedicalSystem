#region License

// Copyright (c) 2011, ClearCanvas Inc.
// All rights reserved.
// http://www.ClearCanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.ClearCanvas.ca/OSLv3.0

#endregion

using System;
using System.ServiceModel;
using Macro.ImageServer.Common.Utilities;
using Macro.ImageServer.Web.Common.Utilities;

namespace Macro.ImageServer.Web.Common.WebControls.Validators
{
    /// <summary>
    /// Validate a Web UI input control containing a filesystem path.
    /// </summary>
    /// <remarks>
    /// This validator calls a web service to validate the result. The web service must accept one parameter named "path".
    /// </remarks>
    /// 
    /// <example>
    /// <para>
    /// The following block adds validation for the filesystem path using a web service ValidateFilesystemPath 
    /// located at /Services/ValidationServices.asmx (relative to the current web site). 
    /// The parameter(s) is generated by the javascript script function ValidationFilesystemPathParams(). 
    /// </para>
    /// <para>
    /// If the path is not valid, the error indicator control InvalidInputIndicator will show up on the screen.
    /// </para>
    /// <code>
    ///  
    ///  function ValidationFilesystemPathParams()
    ///  {
    ///     input = document.getElementById('" + PathTextBox.ClientID + @"');
    ///     params = new Array();
    ///     params.path = input.value;
    ///
    ///     return params;
    ///  }
    /// 
    /// </code>
    /// 
    /// <code>
    /// <uc1:InvalidInputIndicator ID="InvalidPathHint" runat="server" 
    ///     ImageUrl="~/images/icons/HelpSmall.png"
    ///     Visible="true"/>
    ///                                                        
    /// <Macro:FilesystemPathValidator runat="server" ID="PathValidator" 
    ///         ControlToValidate="PathTextBox"
    ///         InputName="Filesystem Path" 
    ///         InvalidInputColor="#FAFFB5" 
    ///         InvalidInputIndicatorID="InvalidPathHint"
    ///         ServicePath="/Services/ValidationServices.asmx" 
    ///         ServiceOperation="ValidateFilesystemPath"
    ///         ParamsFunction="ValidationFilesystemPathParams" 
    ///         ErrorMessage="Invalid path"
    ///         Display="None" ValidationGroup="vg1"/> 
    /// </code>
    /// </example>
    public class FilesystemPathValidator : WebServiceValidator
    {
        #region Protected Methods

        protected override bool OnServerSideEvaluate()
        {
            String path = GetControlValidationValue(ControlToValidate);

            if (String.IsNullOrEmpty(path))
            {
                ErrorMessage = ValidationErrors.FilesystemPathCannotBeEmpty;
                return false;
            }

            try
            {
                FilesystemInfo fsInfo = ServerUtility.GetFilesystemInfo(path);

                return fsInfo != null && fsInfo.Exists;
            }
            catch (EndpointNotFoundException e)
            {
                ErrorMessage = e.Message;
                return true;
            }
            catch (IndexOutOfRangeException)
            {
                ErrorMessage = String.Format(ValidationErrors.FilesystemPathInvalidOrUnreachable, path);
                return false;
            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
                return false;
            }
        }

        #endregion Protected Methods
    }
}