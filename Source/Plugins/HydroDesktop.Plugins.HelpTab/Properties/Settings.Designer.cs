﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18444
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace HydroDesktop.Plugins.HelpTab.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "11.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("http://userguide.hydrodesktop.org")]
        public string remoteHelpUri {
            get {
                return ((string)(this["remoteHelpUri"]));
            }
            set {
                this["remoteHelpUri"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("http://hydrodesktop.codeplex.com/discussions")]
        public string discussionForumUri {
            get {
                return ((string)(this["discussionForumUri"]));
            }
            set {
                this["discussionForumUri"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("http://hydrodesktop.codeplex.com/workitem/list/basic")]
        public string issueTrackerUri {
            get {
                return ((string)(this["issueTrackerUri"]));
            }
            set {
                this["issueTrackerUri"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("https://www.cuahsi.org/Contact")]
        public string hisCommentUri {
            get {
                return ((string)(this["hisCommentUri"]));
            }
            set {
                this["hisCommentUri"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("mailto:questions@cuahsi.org?subject=HydroDesktop&body=Dear CUAHSI User Support Sp" +
            "ecialist,%0A%0A")]
        public string commentMailtoLink {
            get {
                return ((string)(this["commentMailtoLink"]));
            }
            set {
                this["commentMailtoLink"] = value;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("HydroDesktop User Guide.pdf")]
        public string localHelpUri {
            get {
                return ((string)(this["localHelpUri"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("http://quickstart.hydrodesktop.org")]
        public string quickStartUri {
            get {
                return ((string)(this["quickStartUri"]));
            }
            set {
                this["quickStartUri"] = value;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("HydroDesktop_Quick_Start_Guide_1.5.pdf")]
        public string QuickStartGuideName {
            get {
                return ((string)(this["QuickStartGuideName"]));
            }
        }
    }
}
