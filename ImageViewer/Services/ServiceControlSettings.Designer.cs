﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.296
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

namespace Macro.ImageViewer.Services {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0")]
    internal sealed partial class ServiceControlSettings : global::System.Configuration.ApplicationSettingsBase {
        
        private static ServiceControlSettings defaultInstance = ((ServiceControlSettings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new ServiceControlSettings())));
        
        public static ServiceControlSettings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool Enabled {
            get {
                return ((bool)(this["Enabled"]));
            }
        }
        
        /// <summary>
        /// The name of the workstation service, as it would appear in the Service Control Manager.
        /// </summary>
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Configuration.SettingsDescriptionAttribute("The name of the workstation service, as it would appear in the Service Control Ma" +
            "nager.")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Macro Workstation Shred Host Service")]
        public string ServiceName {
            get {
                return ((string)(this["ServiceName"]));
            }
        }
    }
}
