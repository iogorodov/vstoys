using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using System.Diagnostics;

namespace VSToys
{
	public sealed class VSToysEvents : IVsTrackProjectDocumentsEvents2, IVsSelectionEvents, IVsUpdateSolutionEvents2, IDisposable
	{
		private VSToysPackage package;

		private uint trackProjectDocumentsCookie = VSConstants.VSCOOKIE_NIL;
		private uint trackSelectionCookie = VSConstants.VSCOOKIE_NIL;
		private uint trackUpdateSolutionCookie = VSConstants.VSCOOKIE_NIL;
		private bool disposed = false;

		private delegate int ProcessServiceFunc<TService>( TService service );
		private int ProcessService<TService>( Type serviceType, ProcessServiceFunc<TService> process ) where TService : class
		{
			if ( package == null )
				return VSConstants.E_FAIL;

			TService service = package.GetService<TService>( serviceType );
			if ( service == null )
				return VSConstants.E_FAIL;

			return process( service );
		}

		public void Init( VSToysPackage package )
		{
			this.package = package;
			ProcessService( typeof( SVsTrackProjectDocuments ), ( IVsTrackProjectDocuments2 service ) => { return service.AdviseTrackProjectDocumentsEvents( this, out trackProjectDocumentsCookie ); } );
			ProcessService( typeof( SVsShellMonitorSelection ), ( IVsMonitorSelection service ) => { return service.AdviseSelectionEvents( this, out trackSelectionCookie ); } );
			ProcessService( typeof( SVsSolutionBuildManager ), ( IVsSolutionBuildManager2 service ) => { return service.AdviseUpdateSolutionEvents( this, out trackUpdateSolutionCookie ); } );
		}

		#region IDisposable Members

		public void Dispose()
		{
			if ( disposed )
				return;

			if ( package != null )
			{
				ProcessService( typeof( SVsTrackProjectDocuments ), ( IVsTrackProjectDocuments2 service ) => { return service.UnadviseTrackProjectDocumentsEvents( trackProjectDocumentsCookie ); } );
				ProcessService( typeof( SVsShellMonitorSelection ), ( IVsMonitorSelection service ) => { return service.UnadviseSelectionEvents( trackSelectionCookie ); } );
				ProcessService( typeof( SVsSolutionBuildManager ), ( IVsSolutionBuildManager2 service ) => { return service.UnadviseUpdateSolutionEvents( trackUpdateSolutionCookie ); } );

				trackProjectDocumentsCookie = VSConstants.VSCOOKIE_NIL;
				trackSelectionCookie = VSConstants.VSCOOKIE_NIL;
				trackUpdateSolutionCookie = VSConstants.VSCOOKIE_NIL;
			}

			disposed = true;
			GC.SuppressFinalize( this );
		}

		#endregion

		#region IVsTrackProjectDocumentsEvents2 Members

		public int OnAfterAddDirectoriesEx( int cProjects, int cDirectories, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, VSADDDIRECTORYFLAGS[] rgFlags )
		{
			return VSConstants.S_OK;
		}

		public int OnAfterAddFilesEx( int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, VSADDFILEFLAGS[] rgFlags )
		{
			return VSConstants.S_OK;
		}

		public int OnAfterRemoveDirectories( int cProjects, int cDirectories, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, VSREMOVEDIRECTORYFLAGS[] rgFlags )
		{
			return VSConstants.S_OK;
		}

		public int OnAfterRemoveFiles( int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, VSREMOVEFILEFLAGS[] rgFlags )
		{
			return VSConstants.S_OK;
		}

		public int OnAfterRenameDirectories( int cProjects, int cDirs, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgszMkOldNames, string[] rgszMkNewNames, VSRENAMEDIRECTORYFLAGS[] rgFlags )
		{
			return VSConstants.S_OK;
		}

		public int OnAfterRenameFiles( int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgszMkOldNames, string[] rgszMkNewNames, VSRENAMEFILEFLAGS[] rgFlags )
		{
			return VSConstants.S_OK;
		}

		public int OnAfterSccStatusChanged( int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, uint[] rgdwSccStatus )
		{
			return VSConstants.S_OK;
		}

		public int OnQueryAddDirectories( IVsProject pProject, int cDirectories, string[] rgpszMkDocuments, VSQUERYADDDIRECTORYFLAGS[] rgFlags, VSQUERYADDDIRECTORYRESULTS[] pSummaryResult, VSQUERYADDDIRECTORYRESULTS[] rgResults )
		{
			return VSConstants.S_OK;
		}

		public int OnQueryAddFiles( IVsProject pProject, int cFiles, string[] rgpszMkDocuments, VSQUERYADDFILEFLAGS[] rgFlags, VSQUERYADDFILERESULTS[] pSummaryResult, VSQUERYADDFILERESULTS[] rgResults )
		{
			return VSConstants.S_OK;
		}

		public int OnQueryRemoveDirectories( IVsProject pProject, int cDirectories, string[] rgpszMkDocuments, VSQUERYREMOVEDIRECTORYFLAGS[] rgFlags, VSQUERYREMOVEDIRECTORYRESULTS[] pSummaryResult, VSQUERYREMOVEDIRECTORYRESULTS[] rgResults )
		{
			return VSConstants.S_OK;
		}

		public int OnQueryRemoveFiles( IVsProject pProject, int cFiles, string[] rgpszMkDocuments, VSQUERYREMOVEFILEFLAGS[] rgFlags, VSQUERYREMOVEFILERESULTS[] pSummaryResult, VSQUERYREMOVEFILERESULTS[] rgResults )
		{
			return VSConstants.S_OK;
		}

		public int OnQueryRenameDirectories( IVsProject pProject, int cDirs, string[] rgszMkOldNames, string[] rgszMkNewNames, VSQUERYRENAMEDIRECTORYFLAGS[] rgFlags, VSQUERYRENAMEDIRECTORYRESULTS[] pSummaryResult, VSQUERYRENAMEDIRECTORYRESULTS[] rgResults )
		{
			return VSConstants.S_OK;
		}

		public int OnQueryRenameFiles( IVsProject pProject, int cFiles, string[] rgszMkOldNames, string[] rgszMkNewNames, VSQUERYRENAMEFILEFLAGS[] rgFlags, VSQUERYRENAMEFILERESULTS[] pSummaryResult, VSQUERYRENAMEFILERESULTS[] rgResults )
		{
			return VSConstants.S_OK;
		}

		#endregion

		#region IVsSelectionEvents Members

		public int OnCmdUIContextChanged( uint dwCmdUICookie, int fActive )
		{
			return VSConstants.S_OK;
		}

		public int OnElementValueChanged( uint elementid, object varValueOld, object varValueNew )
		{
			return VSConstants.S_OK;
		}

		public delegate void SelectionChangedHandler( VSToysPackage package, IVsHierarchy[] selectedItems );
		public event SelectionChangedHandler SelectionChanged = null;

		public int OnSelectionChanged( IVsHierarchy pHierOld, uint itemidOld, IVsMultiItemSelect pMISOld, ISelectionContainer pSCOld, IVsHierarchy pHierNew, uint itemidNew, IVsMultiItemSelect pMISNew, ISelectionContainer pSCNew )
		{			
			IVsHierarchy[] selectedItems = new IVsHierarchy[0];
			if ( pHierNew != null )
				selectedItems = new IVsHierarchy[1] { pHierNew };
			else if ( pMISNew != null )
			{
				uint pcItems;
				int pfSingleHierarchy;
				if ( pMISNew.GetSelectionInfo( out pcItems, out pfSingleHierarchy ) == VSConstants.S_OK )
				{
					VSITEMSELECTION[] items = new VSITEMSELECTION[pcItems];
					if ( pMISNew.GetSelectedItems( 0, pcItems, items ) == VSConstants.S_OK )
					{
						selectedItems = new IVsHierarchy[pcItems];
						for ( int i = 0; i < pcItems; ++i )
							selectedItems[i] = items[i].pHier;
					}
				}
			}

			if ( SelectionChanged != null )
				SelectionChanged( package, selectedItems );
			
			return VSConstants.S_OK;
		}

		#endregion

		#region IVsUpdateSolutionEvents2 Members

		public int OnActiveProjectCfgChange( IVsHierarchy pIVsHierarchy )
		{
			return VSConstants.S_OK;
		}

		public int UpdateProjectCfg_Begin( IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction, ref int pfCancel )
		{
			return VSConstants.S_OK;
		}

		public int UpdateProjectCfg_Done( IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction, int fSuccess, int fCancel )
		{
			return VSConstants.S_OK;
		}

		public int UpdateSolution_Begin( ref int pfCancelUpdate )
		{
			return VSConstants.S_OK;
		}

		public int UpdateSolution_Cancel()
		{
			return VSConstants.S_OK;
		}

		public delegate void UpdateSolutionDoneHandler( VSToysPackage package, bool succeeded, bool modified, bool canceled );
		public event UpdateSolutionDoneHandler UpdateSolutionDone = null;

		public int UpdateSolution_Done( int fSucceeded, int fModified, int fCancelCommand )
		{
			if ( UpdateSolutionDone != null )
				UpdateSolutionDone( package, fSucceeded != 0, fModified != 0, fCancelCommand != 0 );
			return VSConstants.S_OK;
		}

		public int UpdateSolution_StartUpdate( ref int pfCancelUpdate )
		{
			return VSConstants.S_OK;
		}

		#endregion
	}
}
