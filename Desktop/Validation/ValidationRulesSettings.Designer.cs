﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.296
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

namespace Macro.Desktop.Validation {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0")]
    internal sealed partial class ValidationRulesSettings : global::System.Configuration.ApplicationSettingsBase {
        
        private static ValidationRulesSettings defaultInstance = ((ValidationRulesSettings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new ValidationRulesSettings())));
        
        public static ValidationRulesSettings Default {
            get {
                return defaultInstance;
            }
        }
        
        /// <summary>
        /// XML document specifying custom user-interface validation rules.
        /// </summary>
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Configuration.SettingsDescriptionAttribute("XML document specifying custom user-interface validation rules.")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("ValidationRules.xml")]
        public string CustomRulesXml {
            get {
                return ((string)(this["CustomRulesXml"]));
            }
        }
    }
}
