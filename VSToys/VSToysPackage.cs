using System;
using System.ComponentModel.Design;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace VSToys
{
	/// <summary>
	/// This is the class that implements the package exposed by this assembly.
	///
	/// The minimum requirement for a class to be considered a valid package for Visual Studio
	/// is to implement the IVsPackage interface and register itself with the shell.
	/// This package uses the helper classes defined inside the Managed Package Framework (MPF)
	/// to do it: it derives from the Package class that provides the implementation of the 
	/// IVsPackage interface and uses the registration attributes defined in the framework to 
	/// register itself and its components with the shell.
	/// </summary>
	// This attribute tells the registration utility (regpkg.exe) that this class needs
	// to be registered as package.
	[PackageRegistration( UseManagedResourcesOnly = true )]
	// A Visual Studio component can be registered under different regitry roots; for instance
	// when you debug your package you want to register it in the experimental hive. This
	// attribute specifies the registry root to use if no one is provided to regpkg.exe with
	// the /root switch.
	[DefaultRegistryRoot( "Software\\Microsoft\\VisualStudio\\9.0" )]
	// This attribute is used to register the informations needed to show the this package
	// in the Help/About dialog of Visual Studio.
	[InstalledProductRegistration( false, "#110", "#112", "0.9", IconResourceID = 400 )]
	// In order be loaded inside Visual Studio in a machine that has not the VS SDK installed, 
	// package needs to have a valid load key (it can be requested at 
	// http://msdn.microsoft.com/vstudio/extend/). This attributes tells the shell that this 
	// package has a load key embedded in its resources.
	[ProvideLoadKey( "Standard", "0.9", "VSToys", "iogorodov@gmail.com", 114 )]
	[ProvideMenuResource( 1000, 20 )]
	[Guid( "d94a8225-a439-4f38-9139-28ec78144f60" )]
	public sealed class VSToysPackage : Package
	{
		private VSToysSettings settings = new VSToysSettings();
		private VSToysEvents events = new VSToysEvents();

		/// <summary>
		/// Default constructor of the package.
		/// Inside this method you can place any initialization code that does not require 
		/// any Visual Studio service because at this point the package object is created but 
		/// not sited yet inside Visual Studio environment. The place to do all the other 
		/// initialization is the Initialize method.
		/// </summary>
		public VSToysPackage()
		{
			AddOptionKey( GetType().Name );
		}

		public delegate void VSToysEventHandler( VSToysPackage package, object sender, EventArgs args );

		/// <summary>
		/// Add our command handlers for menu (commands must exist in the .vsct file)
		/// </summary>
		public void AddMenuCommandItem( int cmdID, VSToysEventHandler commandEventHandler, VSToysEventHandler queryEventHandler )
		{
			OleMenuCommandService mcs = GetService<OleMenuCommandService>( typeof( IMenuCommandService ) );
			if ( mcs == null )
				return;

			CommandID menuCommandID = new CommandID( new Guid( "3831e03d-0d99-4925-831e-fbc56839dc25" ), (int)cmdID );
			OleMenuCommand menuItem = new OleMenuCommand( ( object sender, EventArgs args ) => { commandEventHandler( this, sender, args ); }, menuCommandID );

			if ( queryEventHandler != null )
				menuItem.BeforeQueryStatus += ( object sender, EventArgs args ) => { queryEventHandler( this, sender, args ); };

			mcs.AddCommand( menuItem );
		}

		public T GetService<T>( Type serviceType ) where T : class
		{
			return GetService( serviceType ) as T;
		}

		public T GetService<T>() where T : class
		{
			return GetService<T>( typeof( T ) );
		}

		public VSToysSettings Settings { get { return settings; } }
		public VSToysEvents Events { get { return events; } }

		#region Package members

		/// <summary>
		/// Initialization of the package; this method is called right after the package is sited, so this is the place
		/// where you can put all the initilaization code that rely on services provided by VisualStudio.
		/// </summary>
		protected override void Initialize()
		{
			base.Initialize();

			events.Init( this );

			OpenFilesInSolution.OpenFilesInSolutionDialog.Initialize( this );
			QuickRun.QuickRun.Initialize( this );
			SimpleToys.BeepOnBuild.Initialize( this );

			settings.LoadApplicationSettings( this );
		}

		protected override void OnLoadOptions( string key, Stream stream )
		{
			if ( key == GetType().Name )
				settings.LoadSolutionSettings( this, stream );
			else
				base.OnLoadOptions( key, stream );
		}

		protected override void OnSaveOptions( string key, Stream stream )
		{
			if ( key == GetType().Name )
				settings.StoreSolutionSettings( this, stream );
			else
				base.OnSaveOptions( key, stream );
		}

		protected override int QueryClose( out bool canClose )
		{
			settings.StoreApplicationSettings( this );
			return base.QueryClose( out canClose );
		}

		#endregion
	}
}