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
    internal class ApplicableMigrationTranslations {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal ApplicableMigrationTranslations() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Peregrinus.Resources.ApplicableMigrationTranslations", typeof(ApplicableMigrationTranslations).Assembly);
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
        ///   Looks up a localized string similar to The given migration file does not exist..
        /// </summary>
        internal static string FileNotFoundExceptionMessage {
            get {
                return ResourceManager.GetString("FileNotFoundExceptionMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The migration script might not be a SQL file. Make sure the file has a .sql extension..
        /// </summary>
        internal static string IncorrectFileExtensionMessage {
            get {
                return ResourceManager.GetString("IncorrectFileExtensionMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The migration description is either missing or in incorrect format. Make sure there&apos;s a double underscore (_) between the version and description..
        /// </summary>
        internal static string IncorrectMigrationDescriptionFormatMessage {
            get {
                return ResourceManager.GetString("IncorrectMigrationDescriptionFormatMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The format of the migration file name was wrong. Make sure the filename starts with a &quot;V&quot; prefix..
        /// </summary>
        internal static string IncorrectMigrationFilenamePrefixMessage {
            get {
                return ResourceManager.GetString("IncorrectMigrationFilenamePrefixMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The specified version is not in the correct format. Make sure the specified version follows semver rules. See https://semver.org/.
        /// </summary>
        internal static string IncorrectMigrationVersionFormatMessage {
            get {
                return ResourceManager.GetString("IncorrectMigrationVersionFormatMessage", resourceCulture);
            }
        }
    }
}
