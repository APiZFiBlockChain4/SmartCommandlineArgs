﻿//------------------------------------------------------------------------------
// <copyright file="CmdArgsToolWindowPackage.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;

namespace SmartCmdArgs
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(CmdArgsToolWindow))]
    [Guid(CmdArgsToolWindowPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class CmdArgsToolWindowPackage : Package
    {
        /// <summary>
        /// CmdArgsToolWindowPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "131b0c0a-5dd0-4680-b261-86ab5387b86e";

        private EnvDTE.DTE appObject;
        private EnvDTE.SolutionEvents solutionEvents;
        private EnvDTE.CommandEvents commandEvents;

        private readonly string _VSConstants_VSStd97CmdID_GUID;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmdArgsToolWindow"/> class.
        /// </summary>
        public CmdArgsToolWindowPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.


            // cache guid value
            _VSConstants_VSStd97CmdID_GUID = typeof(VSConstants.VSStd97CmdID).GUID.ToString("B").ToUpper();

            // TODO add option keys to store custom data in suo file
            // this.AddOptionKey("Test");
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            CmdArgsToolWindowCommand.Initialize(this);
            base.Initialize();

            this.appObject = (EnvDTE.DTE)GetService(typeof(SDTE));

            // see: https://support.microsoft.com/en-us/kb/555430
            this.solutionEvents = this.appObject.Events.SolutionEvents;
            this.commandEvents = this.appObject.Events.CommandEvents;

            this.solutionEvents.Opened += SolutionEvents_Opened;
            this.solutionEvents.AfterClosing += SolutionEvents_AfterClosing;
            this.commandEvents.AfterExecute += CommandEvents_AfterExecute;
        }


        protected override void OnLoadOptions(string key, Stream stream)
        {
            base.OnLoadOptions(key, stream);
        }
        protected override void OnSaveOptions(string key, Stream stream)
        {
            base.OnSaveOptions(key, stream);
        }


        #endregion

        #region VS Events

        private void SolutionEvents_Opened()
        {
            var startupProjects = this.appObject?.Solution?.SolutionBuild?.StartupProjects as object[];
            string prjName = startupProjects?.FirstOrDefault() as string;

            EnvDTE.Project project;
            bool found = FindProject(this.appObject?.Solution, prjName, out project);

            // TODO
            //var p0 = project?.ConfigurationManager?.ActiveConfiguration?.Properties?.Item("StartArguments"); // CLR project?
            //var p1 = project?.ConfigurationManager?.ActiveConfiguration?.Properties?.Item("CommandArguments"); C project?
        }

        private bool FindProject(EnvDTE.Solution sln, string uniqueName, out EnvDTE.Project foundProject)
        {
            foundProject = null;

            if (sln == null || uniqueName == null)
                return false;

            foreach (EnvDTE.Project project in sln.Projects)
            {
                if (project.UniqueName == uniqueName)
                {
                    foundProject = project;
                    return true;
                }
                else if(project.Kind == EnvDTE80.ProjectKinds.vsProjectKindSolutionFolder)
                {
                    // TODO search solution folders
                }
            }

            return false;
        }

        private void SolutionEvents_AfterClosing()
        {

        }

        private void CommandEvents_AfterExecute(string Guid, int ID, object CustomIn, object CustomOut)
        {
            if (Guid == _VSConstants_VSStd97CmdID_GUID)
            {
                switch ((VSConstants.VSStd97CmdID)ID)
                {
                    case VSConstants.VSStd97CmdID.SetStartupProject:
                        // startup project changed
                        break;
                    default:
                        break;
                }
            }
        }
        #endregion
    }
}