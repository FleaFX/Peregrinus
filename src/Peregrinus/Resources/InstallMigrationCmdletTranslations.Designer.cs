﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Peregrinus.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class InstallMigrationCmdletTranslations {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal InstallMigrationCmdletTranslations() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Peregrinus.Resources.InstallMigrationCmdletTranslations", typeof(InstallMigrationCmdletTranslations).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to MigrationFailed.
        /// </summary>
        internal static string ApplicableMigrationFailedResultErrorId {
            get {
                return ResourceManager.GetString("ApplicableMigrationFailedResultErrorId", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Migrating {0}.
        /// </summary>
        internal static string ApplicableMigrationSucceededResultActivity {
            get {
                return ResourceManager.GetString("ApplicableMigrationSucceededResultActivity", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0}: migration was succesfully applied..
        /// </summary>
        internal static string ApplicableMigrationSucceededResultDescription {
            get {
                return ResourceManager.GetString("ApplicableMigrationSucceededResultDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Migrating {0}.
        /// </summary>
        internal static string ApplicableMigrationSucceededWithRollbackResultActivity {
            get {
                return ResourceManager.GetString("ApplicableMigrationSucceededWithRollbackResultActivity", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0}: migration was succesfully applied. Found {1} as the last prerelease migration and rolled that back in the process..
        /// </summary>
        internal static string ApplicableMigrationSucceededWithRollbackResultDescription {
            get {
                return ResourceManager.GetString("ApplicableMigrationSucceededWithRollbackResultDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Migrating {0}.
        /// </summary>
        internal static string AppliedMigrationSkippedResultActivity {
            get {
                return ResourceManager.GetString("AppliedMigrationSkippedResultActivity", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0}: migration was skipped. It was already applied to the target database..
        /// </summary>
        internal static string AppliedMigrationSkippedResultDescription {
            get {
                return ResourceManager.GetString("AppliedMigrationSkippedResultDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to MigrationVersionAnachronism.
        /// </summary>
        internal static string MigrationVersionAnachronismResultErrorId {
            get {
                return ResourceManager.GetString("MigrationVersionAnachronismResultErrorId", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The attempted migration has a version earlier than the version of the last applied migration..
        /// </summary>
        internal static string MigrationVersionAnachronismResultExceptionMessage {
            get {
                return ResourceManager.GetString("MigrationVersionAnachronismResultExceptionMessage", resourceCulture);
            }
        }
    }
}
