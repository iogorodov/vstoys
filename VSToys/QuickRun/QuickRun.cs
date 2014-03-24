using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.IO;

namespace VSToys.QuickRun
{
	public class QuickRun : VSToysSettings.IConfigObject
	{
		private const int cmdidArgumentsCombo = 0x101;
		private const int cmdidArgumentsComboGetList = 0x102;
		private const int cmdidStartupProjectCombo = 0x103;
		private const int cmdidStartupProjectComboGetList = 0x104;
		private static readonly Guid SolutionFolderGuid = new Guid( "66a26720-8fb5-11d2-aa7e-00c04f688dde" );

		[VSToysSettings.Settings( VSToysSettings.Scope.Application )]
		private readonly List<string> lastArguments = new List<string>();
		private readonly List<Project> projects = new List<Project>();
		private readonly Dictionary<string, bool> excludedProjectsMap = new Dictionary<string, bool>();

		[VSToysSettings.Settings( VSToysSettings.Scope.Solution )]
		private readonly List<string> excludedProjects = new List<string>();

		private QuickRun( VSToysPackage package )
		{
			package.AddMenuCommandItem( cmdidArgumentsCombo, OnArgumentsCombo, OnArgumentsComboQuery );
			package.AddMenuCommandItem( cmdidArgumentsComboGetList, OnArgumentsComboGetList, OnArgumentsComboQuery );
			package.AddMenuCommandItem( cmdidStartupProjectCombo, OnStartupProjectCombo, OnStartupProjectComboQuery );
			package.AddMenuCommandItem( cmdidStartupProjectComboGetList, OnStartupProjectComboGetList, OnStartupProjectComboQuery );
		}

		#region Helpers for work with startup project and its command line arguments

		/// <summary>
		/// Determinate if Visual Studio in design mode i.e. we can change command line arguments
		/// </summary>
		/// <param name="dte"></param>
		/// <returns></returns>
		private bool IsInDesignMode( DTE dte )
		{
			if ( dte.Mode != vsIDEMode.vsIDEModeDesign )
				return false;

			Solution solution = dte.Solution;
			if ( solution == null )
				return false;

			SolutionBuild solutionBuild = solution.SolutionBuild;
			if ( solutionBuild == null )
				return false;

			if ( solutionBuild.BuildState == vsBuildState.vsBuildStateInProgress )
				return false;

			return true;
		}

		/// <summary>
		/// Get sturtup project from loaded solution
		/// </summary>
		/// <returns>Current startup project or null if no solution loaded</returns>
		private Project GetStartupProject( DTE dte )
		{
			Solution solution = dte.Solution;
			if ( solution == null )
				return null;

			SolutionBuild solutionBuild = solution.SolutionBuild;
			if ( solutionBuild == null )
				return null;

			Array startupProjects = (Array)solutionBuild.StartupProjects;
			if ( startupProjects == null || startupProjects.Length != 1 )
				return null;

			string projectUniqueName = (string)startupProjects.GetValue( 0 );
			Stack<Project> solutionFolders = new Stack<Project>();
			foreach ( Project project in solution.Projects )
			{
				if ( project.UniqueName == projectUniqueName )
					return project;
				else
					solutionFolders.Push( project );
			}

			while ( solutionFolders.Count > 0 )
			{
				Project project = solutionFolders.Pop();
				foreach ( ProjectItem item in project.ProjectItems )
				{
					Project subProject = item.SubProject;
					if ( subProject != null )
					{
						if ( subProject.UniqueName == projectUniqueName )
							return subProject;
						else
							solutionFolders.Push( subProject );
					}
				}
			}

			return null;
		}

		private bool SetStartupProject( DTE dte, Project project )
		{
			Solution solution = dte.Solution;
			if ( solution == null )
				return false;

			SolutionBuild solutionBuild = solution.SolutionBuild;
			if ( solutionBuild == null )
				return false;

			solutionBuild.StartupProjects = project.UniqueName;
			return true;
		}

		/// <summary>
		/// Check than current startup project has property with command line arguments
		/// </summary>
		/// <returns>True if current startup project has property with command line arguments</returns>
		private bool CanSetProjectArguments( DTE dte )
		{
			Project project = GetStartupProject( dte );

			if ( project == null )
				return false;

			foreach ( Property property in project.ConfigurationManager.ActiveConfiguration.Properties )
			{
				if ( property.Name == "CommandArguments" || property.Name == "StartArguments" )
					return true;
			}

			return false;
		}

		/// <summary>
		/// Get command line arguments for current startup project
		/// </summary>
		/// <returns>Command line arguments or null if no startup project or it doesn't have command line property</returns>
		private string GetProjectArguments( DTE dte )
		{
			Project project = GetStartupProject( dte );

			if ( project == null )
				return null;

			foreach ( Property property in project.ConfigurationManager.ActiveConfiguration.Properties )
			{
				if ( property.Name == "CommandArguments" || property.Name == "StartArguments" )
					return (string)property.Value;
			}

			return null;
		}

		/// <summary>
		/// Set command line arguments for current startup project
		/// </summary>
		/// <param name="arguments">Command line arguments</param>
		/// <returns>False if no startup project or it doesn't have command line property</returns>
		private bool SetProjectArguments( DTE dte, string arguments )
		{
			Project project = GetStartupProject( dte );

			if ( project == null )
				return false;

			foreach ( Property property in project.ConfigurationManager.ActiveConfiguration.Properties )
			{
				if ( property.Name == "CommandArguments" || property.Name == "StartArguments" )
				{
					string value = (string)property.Value;
					if ( value != arguments )
					{
						AddArguments( arguments );
						property.Value = arguments;
					}
					return true;
				}
			}

			return false;
		}

		private bool HasProjects( DTE dte )
		{
			Solution solution = dte.Solution;
			if ( solution == null )
				return false;

			Stack<Project> solutionFolders = new Stack<Project>();
			foreach ( Project project in solution.Projects )
			{
				if ( new Guid( project.Kind ) == SolutionFolderGuid )
					solutionFolders.Push( project );
				else
					return true;
			}

			while ( solutionFolders.Count > 0 )
			{
				Project project = solutionFolders.Pop();
				foreach ( ProjectItem item in project.ProjectItems )
				{
					Project subProject = item.SubProject;
					if ( subProject != null )
					{
						if ( new Guid( subProject.Kind ) == SolutionFolderGuid )
							solutionFolders.Push( subProject );
						else
							return true;
					}
				}
			}

			return false;
		}

		private List<Project> AcqureProjectsList( DTE dte, bool filter )
		{
			List<Project> result = new List<Project>();
			
			Solution solution = dte.Solution;
			if ( solution == null )
				return result;

			Stack<Project> solutionFolders = new Stack<Project>();
			foreach ( Project project in solution.Projects )
			{
				if ( new Guid( project.Kind ) == SolutionFolderGuid )
					solutionFolders.Push( project );
				else if ( !filter || !excludedProjectsMap.ContainsKey( project.Name ) )
					result.Add( project );
			}

			while ( solutionFolders.Count > 0 )
			{
				Project project = solutionFolders.Pop();
				foreach ( ProjectItem item in project.ProjectItems )
				{
					Project subProject = item.SubProject;
					if ( subProject != null )
					{
						if ( new Guid( subProject.Kind ) == SolutionFolderGuid )
							solutionFolders.Push( subProject );
						else if ( !filter || !excludedProjectsMap.ContainsKey( subProject.Name ) )
							result.Add( subProject );
					}
				}
			}

			result.Sort( ( Project a, Project b ) => { return string.Compare( a.Name, b.Name, StringComparison.CurrentCultureIgnoreCase ); } );
			return result;
		}

		private void AcqureProjectsList( DTE dte )
		{
			projects.Clear();
			projects.AddRange( AcqureProjectsList( dte, true ) );
		}

		#endregion

		#region Helpers for work with last used command line arguments

		private void AddArguments( string arguments )
		{
			if ( string.IsNullOrEmpty( arguments ) )
				return;

			for ( int i = 0; i < lastArguments.Count; ++i )
			{
				if ( lastArguments[i] == arguments )
				{
					lastArguments.RemoveAt( i );
				}
			}

			lastArguments.Insert( 0, arguments );
			if ( lastArguments.Count > 20 )
			{
				lastArguments.RemoveRange( 20, lastArguments.Count - 20 );
			}
		}

		#endregion

		#region Command and Query handlers for command line arguments

		private void OnArgumentsCombo( VSToysPackage package, object sender, EventArgs args )
		{
			OleMenuCmdEventArgs oleEventArgs = args as OleMenuCmdEventArgs;
			if ( oleEventArgs.InValue is Object[] )
			{
				Object[] objectsArgs = oleEventArgs.InValue as Object[];
				if ( objectsArgs != null && objectsArgs.Length == 5 && objectsArgs[1] is int && objectsArgs[4] is string )
				{
					int messageCode = (int)objectsArgs[1];
					if ( messageCode == 0x0008 )
					{
						SetProjectArguments( package.GetService<DTE>(), objectsArgs[4] as string );
					}
				}
			}
			else if ( oleEventArgs.InValue == null )
			{
				Marshal.GetNativeVariantForObject( GetProjectArguments( package.GetService<DTE>() ), oleEventArgs.OutValue );
			}
			else if ( oleEventArgs.InValue is string )
			{
				SetProjectArguments( package.GetService<DTE>(), oleEventArgs.InValue as string );
			}
		}

		private void OnArgumentsComboQuery( VSToysPackage package, object sender, EventArgs args )
		{
			DTE dte = package.GetService<DTE>();
			( sender as OleMenuCommand ).Enabled = CanSetProjectArguments( dte ) && IsInDesignMode( dte );
		}

		private void OnArgumentsComboGetList( VSToysPackage package, object sender, EventArgs eventArgs )
		{
			Marshal.GetNativeVariantForObject( lastArguments.ToArray(), ( eventArgs as OleMenuCmdEventArgs ).OutValue );
		}

		#endregion

		#region Command and Query handlers for startup project

		private void OnStartupProjectCombo( VSToysPackage package, object sender, EventArgs args )
		{
			OleMenuCmdEventArgs eventArgs = args as OleMenuCmdEventArgs;
			if ( eventArgs.OutValue != IntPtr.Zero )
			{
				Project startupProject = GetStartupProject( package.GetService<DTE>() );
				if ( startupProject != null )
					Marshal.GetNativeVariantForObject( startupProject.Name, eventArgs.OutValue );
			}
			else if ( eventArgs.InValue != null )
			{
				int index = (int)( eventArgs.InValue );
				if ( index >= 0 && index < projects.Count )
				{
					SetStartupProject( package.GetService<DTE>(), projects[index] );
				}
				else
				{
					IVsUIShell uiShell = package.GetService<IVsUIShell>( typeof( SVsUIShell ) );
					if ( uiShell == null )
						return;

					IntPtr hwnd = IntPtr.Zero;
					Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure( uiShell.GetDialogOwnerHwnd( out hwnd ) );

					DTE dte = package.GetService<DTE>();
					string solutionName = Path.GetFileNameWithoutExtension( dte.Solution.FullName );
					SelectStartupProjects dialog = new SelectStartupProjects( solutionName, AcqureProjectsList( dte, false ), excludedProjectsMap );
					if ( dialog.ShowDialog( NativeWindow.FromHandle( hwnd ) ) == DialogResult.OK )
					{
						excludedProjectsMap.Clear();
						foreach ( var project in dialog.GetExcludedProjects() )
							excludedProjectsMap.Add( project, true );
					}
				}
			}
		}

		private void OnStartupProjectComboQuery( VSToysPackage package, object sender, EventArgs args )
		{
			DTE dte = package.GetService<DTE>();
			( sender as OleMenuCommand ).Enabled = HasProjects( dte ) && IsInDesignMode( dte );
		}

		private void OnStartupProjectComboGetList( VSToysPackage package, object sender, EventArgs eventArgs )
		{
			AcqureProjectsList( package.GetService<DTE>() );
			List<string> names = projects.ConvertAll<string>( ( Project project ) => { return project.Name; } );
			names.Add( "<Manage...>" );
			Marshal.GetNativeVariantForObject( names.ToArray(), ( eventArgs as OleMenuCmdEventArgs ).OutValue );
		}

		#endregion

		#region IConfigObject Members

		public void OnBeforeSerialize( VSToysPackage package, VSToysSettings.Scope scope )
		{
			if ( scope == VSToysSettings.Scope.Solution )
			{
				excludedProjects.Clear();
				excludedProjects.AddRange( excludedProjectsMap.Keys );
			}
		}

		public void OnAfterDeserialize( VSToysPackage package, VSToysSettings.Scope scope )
		{
			if ( scope == VSToysSettings.Scope.Solution )
			{
				excludedProjectsMap.Clear();
				excludedProjects.ForEach( ( project ) => { excludedProjectsMap.Add( project, true ); } );
			}
		}

		#endregion

		private static QuickRun quickRun = null;

		public static void Initialize( VSToysPackage package )
		{
			quickRun = new QuickRun( package );
			package.Settings.RegisterObject( quickRun );
		}
	}
}
