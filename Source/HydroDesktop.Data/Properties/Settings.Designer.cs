﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.239
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace HydroDesktop.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("http://hiscentral.cuahsi.org/webservices/hiscentral.asmx")]
        public string defaultHISCentralURL {
            get {
                return ((string)(this["defaultHISCentralURL"]));
            }
            set {
                this["defaultHISCentralURL"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(@"<?xml version=""1.0"" encoding=""utf-16""?>
<ArrayOfString xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <string>http://hiscentral.cuahsi.org/webservices/hiscentral.asmx</string>
  <string>http://water.sdsc.edu/hiscentral/webservices/hiscentral.asmx</string>
</ArrayOfString>")]
        public global::System.Collections.Specialized.StringCollection hisCentralURLList {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["hisCentralURLList"]));
            }
            set {
                this["hisCentralURLList"] = value;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("default.hdprj")]
        public string DefaultHDProjectName {
            get {
                return ((string)(this["DefaultHDProjectName"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("settings.xml")]
        public string settingsFileName {
            get {
                return ((string)(this["settingsFileName"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("HydroDesktop")]
        public string TempDirectoryName {
            get {
                return ((string)(this["TempDirectoryName"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("projects\\default")]
        public string defaultProjectDirectory {
            get {
                return ((string)(this["defaultProjectDirectory"]));
            }
            set {
                this["defaultProjectDirectory"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("HydroDesktop")]
        public string ApplicationDirectoryName {
            get {
                return ((string)(this["ApplicationDirectoryName"]));
            }
            set {
                this["ApplicationDirectoryName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("default.sqlite")]
        public string DefaultDatabaseName {
            get {
                return ((string)(this["DefaultDatabaseName"]));
            }
            set {
                this["DefaultDatabaseName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("MetadataCache.sqlite")]
        public string DefaultMetadataCacheName {
            get {
                return ((string)(this["DefaultMetadataCacheName"]));
            }
            set {
                this["DefaultMetadataCacheName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Append")]
        public string DownloadOption {
            get {
                return ((string)(this["DownloadOption"]));
            }
            set {
                this["DownloadOption"] = value;
            }
        }
    }
}
