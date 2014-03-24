using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace VSToys.OpenFilesInSolution
{
	public sealed class SolutionFilesCollector : IVsSolutionEvents3, IVsSolutionEvents4, IVsTrackProjectDocumentsEvents2, IDisposable
	{
		#region public sealed class FileInSolution

		public sealed class FileInSolution
		{
			public readonly string FileName;
			public readonly string FilePath;
			public readonly string ProjectName;

			private readonly IVsUIHierarchy projectHeirarchy;
			private readonly IVsUIHierarchy fileHeirarchy;
			private readonly UInt32 fileId;
			private readonly UInt32 projectId;

			public FileInSolution( IVsUIHierarchy projectHeirarchy, UInt32 projectId, IVsUIHierarchy fileHeirarchy, UInt32 fileId )
			{
				this.projectHeirarchy = projectHeirarchy;
				this.fileHeirarchy = fileHeirarchy;
				this.fileId = fileId;
				this.projectId = projectId;

				FileName = string.Empty;
				FilePath = string.Empty;
				ProjectName = string.Empty;

				object pVar;

				if ( projectHeirarchy.GetProperty( projectId, (int)__VSHPROPID.VSHPROPID_Name, out pVar ) == VSConstants.S_OK )
					ProjectName = (string)pVar;

				string projectPath = string.Empty;
				if ( projectHeirarchy.GetProperty( projectId, (int)__VSHPROPID.VSHPROPID_ProjectDir, out pVar ) == VSConstants.S_OK )
					projectPath = (string)pVar;

				if ( fileHeirarchy != null )
				{
					string fileSavePath = string.Empty;
					if ( fileHeirarchy.GetProperty( fileId, (int)__VSHPROPID.VSHPROPID_SaveName, out pVar ) == VSConstants.S_OK )
					{
						fileSavePath = (string)pVar;
						FilePath = Path.Combine( projectPath, fileSavePath );
						FileName = Path.GetFileName( fileSavePath );
					}
				}
			}

			public void OpenFile()
			{
				IVsProject project = projectHeirarchy as IVsProject;
				if ( project == null )
					return;

				Guid rguidLogicalView = Guid.Empty;
				IVsWindowFrame ppWindowFrame = null;
				if ( project.OpenItem( fileId, ref rguidLogicalView, new IntPtr( -1 ), out ppWindowFrame ) == VSConstants.S_OK )
					ppWindowFrame.Show();
			}

			public IVsUIHierarchy Hierarchy { get { return fileHeirarchy; } }
			public uint HierarchyID { get { return fileId; } }

			public static int Compare( FileInSolution a, FileInSolution b )
			{
				int result = string.Compare( a.ProjectName, b.ProjectName, StringComparison.InvariantCultureIgnoreCase );
				if ( result != 0 )
					return result;

				return string.Compare( a.FileName, b.FileName, StringComparison.InvariantCultureIgnoreCase );
			}
		}

		#endregion

		#region Private fields

		private IServiceProvider serviceProvider = null;
		private IVsHierarchy ignoreHierarchy = null;

    private uint eventsCookie = (uint)Constants.VSCOOKIE_NIL;
		private uint trackProjectDocumentsCookie = (uint)Constants.VSCOOKIE_NIL;
    private bool disposed = false;

		private List<FileInSolution> files = new List<FileInSolution>();

		//private Thread listBuilderThread = null;

    #endregion

    #region Hierarchy enumeration

    private static uint GetItemId( object pvar )
    {
      if ( pvar == null )
        return VSConstants.VSITEMID_NIL;
      if ( pvar is int )
        return (uint)(int)pvar;
      if ( pvar is uint )
        return (uint)pvar;
      if ( pvar is short )
        return (uint)(short)pvar;
      if ( pvar is ushort )
        return (uint)(ushort)pvar;
      if ( pvar is long )
        return (uint)(long)pvar;
      return VSConstants.VSITEMID_NIL;
    }

		private static void EnumHierarchyItems( IVsHierarchy hierarchy, uint itemid, IVsHierarchy ignoreHierarchy, bool hierIsSolution, IVsUIHierarchy projectHeirarchy, uint projectItemId, List<FileInSolution> files )
    {
      int hr;
      IntPtr nestedHierarchyObj;
      uint nestedItemId;
      Guid hierGuid = typeof( IVsHierarchy ).GUID;

			if ( hierarchy == ignoreHierarchy )
				return;

      hr = hierarchy.GetNestedHierarchy( itemid, ref hierGuid, out nestedHierarchyObj, out nestedItemId );
      if ( hr == VSConstants.S_OK && nestedHierarchyObj != IntPtr.Zero )
      {
        IVsHierarchy nestedHierarchy = Marshal.GetObjectForIUnknown( nestedHierarchyObj ) as IVsHierarchy;
        Marshal.Release( nestedHierarchyObj );
        if ( nestedHierarchy != null )
					EnumHierarchyItems( nestedHierarchy, nestedItemId, ignoreHierarchy, false, projectHeirarchy, itemid, files );
      }
      else
      {
				object pVar;

				Guid typeGuid = Guid.Empty;
				hr = hierarchy.GetGuidProperty( itemid, (int)__VSHPROPID.VSHPROPID_TypeGuid, out typeGuid );
				if ( typeGuid == VSConstants.GUID_ItemType_PhysicalFile )
				{
					if ( projectHeirarchy != null )
					{
						hr = hierarchy.GetProperty( itemid, (int)__VSHPROPID.VSHPROPID_SaveName, out pVar );
						string filePath = (string)pVar;
						IVsUIHierarchy uiHierarchy = hierarchy as IVsUIHierarchy;
						if ( !string.IsNullOrEmpty( filePath ) && uiHierarchy != null )
							files.Add( new FileInSolution( projectHeirarchy, projectItemId, uiHierarchy, itemid ) );
					}
				}
				else if ( typeGuid != VSConstants.GUID_ItemType_SubProject && typeGuid != VSConstants.GUID_ItemType_PhysicalFolder && typeGuid != VSConstants.GUID_ItemType_VirtualFolder )
				{
					hr = hierarchy.GetProperty( itemid, (int)__VSHPROPID.VSHPROPID_ProjectDir, out pVar );
					string projectPath = (string)pVar;

					hr = hierarchy.GetProperty( itemid, (int)__VSHPROPID.VSHPROPID_Name, out pVar );
					string projectName = (string)pVar;

					IVsUIHierarchy uiHierarchy = hierarchy as IVsUIHierarchy;
					projectHeirarchy = uiHierarchy;

					projectItemId = itemid;
				}

				__VSHPROPID propid = hierIsSolution ? __VSHPROPID.VSHPROPID_FirstVisibleChild : __VSHPROPID.VSHPROPID_FirstChild;
        hr = hierarchy.GetProperty( itemid, (int)propid, out pVar );

        if ( hr == VSConstants.S_OK )
        {
          uint childId = GetItemId( pVar );
          while ( childId != VSConstants.VSITEMID_NIL )
          {
						EnumHierarchyItems( hierarchy, childId, ignoreHierarchy, false, projectHeirarchy, projectItemId, files );

            propid = hierIsSolution ? __VSHPROPID.VSHPROPID_NextVisibleSibling : __VSHPROPID.VSHPROPID_NextSibling;
            hr = hierarchy.GetProperty( childId, (int)propid, out pVar );
            if ( VSConstants.S_OK == hr )
              childId = GetItemId( pVar );
            else
              break;
          }
        }
      }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Initialize SolutionFileCollector
    /// </summary>
    /// <param name="serviceProvider">Object that provides services for SolutionFileCollector</param>
    /// <returns>Returns true if SolutionFilesCollector successfully initialized</returns>
    public bool Init( IServiceProvider _serviceProvider )
    {
			serviceProvider = _serviceProvider;

			IVsSolution solution = serviceProvider.GetService( typeof( SVsSolution ) ) as IVsSolution;
      if ( solution == null )
        return false;

      solution.AdviseSolutionEvents( this, out eventsCookie );

			IVsTrackProjectDocuments2 trackProjectDocuments = serviceProvider.GetService( typeof( SVsTrackProjectDocuments ) ) as IVsTrackProjectDocuments2;
			if ( trackProjectDocuments == null )
				return false;
			
			trackProjectDocuments.AdviseTrackProjectDocumentsEvents( this, out trackProjectDocumentsCookie );

      //listBuilderThread.DoWork += new DoWorkEventHandler( OnDoWork );
      //listBuilderThread.RunWorkerCompleted += new RunWorkerCompletedEventHandler( OnRunWorkerCompleted );

			RebuildList( null );
      return true;
    }

    private void RebuildList( IVsHierarchy ignoredHierarcy )
    {
			//if ( IsBusy )
			//{
			//  listBuilderThread.Abort();
			//  listBuilderThread = null;
			//}

			//listBuilderThread = new Thread( OnDoWork );
			//listBuilderThread.Start( this );
			this.ignoreHierarchy = ignoredHierarcy;
			OnDoWork( this );
			this.ignoreHierarchy = null;
    }

    private void OnProjectsChanged()
    {
      if ( ProjectsChangedEvent != null )
        ProjectsChangedEvent( this );
    }

    #endregion

    #region List builder thread events' handlers

    private static void OnDoWork( object _solutionFilesCollector )
    {
      SolutionFilesCollector solutionFilesCollector = _solutionFilesCollector as SolutionFilesCollector;
      if ( solutionFilesCollector == null )
        return;

      solutionFilesCollector.files.Clear();
      solutionFilesCollector.OnProjectsChanged();

			IVsHierarchy solutionHierarchy = solutionFilesCollector.serviceProvider.GetService( typeof( SVsSolution ) ) as IVsHierarchy;
      if ( solutionHierarchy == null )
        return;

      EnumHierarchyItems( solutionHierarchy, VSConstants.VSITEMID_ROOT, solutionFilesCollector.ignoreHierarchy , true, null, 0, solutionFilesCollector.files );
			solutionFilesCollector.files.Sort( FileInSolution.Compare );

			//solutionFilesCollector.listBuilderThread = null;
			solutionFilesCollector.OnProjectsChanged();
    }

    #endregion

    #region Public properties

    /// <summary>
    /// Returns enumerator for projects in solution. Returns null in solution is currently loading
    /// </summary>
    public IEnumerable<FileInSolution> Files { get { return IsBusy ? null : files; } }

    /// <summary>
    /// Returns true if there is no any projects in solution
    /// </summary>
    public bool IsEmpty { get { return files.Count == 0; } }

    /// <summary>
    /// Returns true if solution currently loading
    /// </summary>
		public bool IsBusy { get { return false; /* listBuilderThread != null && listBuilderThread.IsAlive; */ } }

		public int FilesCount { get { return files.Count; } }

    public delegate void ProjectsChangedEventHandler( SolutionFilesCollector collector );
    
    /// <summary>
    /// Fired after list of projects are changed
    /// </summary>
    public event ProjectsChangedEventHandler ProjectsChangedEvent = null;

    #endregion

    #region IVsSolutionEvents3 Members

    public int OnAfterCloseSolution( object pUnkReserved )
    {
			RebuildList( null );
			return VSConstants.S_OK;
    }

    public int OnAfterClosingChildren( IVsHierarchy pHierarchy )
    {
			RebuildList( null );
			return VSConstants.S_OK;
    }

    public int OnAfterLoadProject( IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy )
    {
      return VSConstants.S_OK;
    }

    public int OnAfterMergeSolution( object pUnkReserved )
    {
			RebuildList( null );
      return VSConstants.S_OK;
    }

    public int OnAfterOpenProject( IVsHierarchy pHierarchy, int fAdded )
    {
			if ( fAdded != 0 )
				RebuildList( null );
			return VSConstants.S_OK;
    }

    public int OnAfterOpenSolution( object pUnkReserved, int fNewSolution )
    {
			RebuildList( null );
      return VSConstants.S_OK;
    }

    public int OnAfterOpeningChildren( IVsHierarchy pHierarchy )
    {
      return VSConstants.S_OK;
    }

    public int OnBeforeCloseProject( IVsHierarchy pHierarchy, int fRemoved )
    {
			if ( fRemoved != 0 )
				RebuildList( pHierarchy );
			return VSConstants.S_OK;
    }

    public int OnBeforeCloseSolution( object pUnkReserved )
    {
			return VSConstants.S_OK;
    }

    public int OnBeforeClosingChildren( IVsHierarchy pHierarchy )
    {
			return VSConstants.S_OK;
    }

    public int OnBeforeOpeningChildren( IVsHierarchy pHierarchy )
    {
			return VSConstants.S_OK;
    }

    public int OnBeforeUnloadProject( IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy )
    {
      return VSConstants.S_OK;
    }

    public int OnQueryCloseProject( IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel )
    {
      return VSConstants.S_OK;
    }

    public int OnQueryCloseSolution( object pUnkReserved, ref int pfCancel )
    {
      return VSConstants.S_OK;
    }

    public int OnQueryUnloadProject( IVsHierarchy pRealHierarchy, ref int pfCancel )
    {
      return VSConstants.S_OK;
    }

    #endregion

    #region IVsSolutionEvents4 Members

    public int OnAfterAsynchOpenProject( IVsHierarchy pHierarchy, int fAdded )
    {
      RebuildList( null );
      return VSConstants.S_OK;
    }

    public int OnAfterChangeProjectParent( IVsHierarchy pHierarchy )
    {
      RebuildList( null );
      return VSConstants.S_OK;
    }

    public int OnAfterRenameProject( IVsHierarchy pHierarchy )
    {
      RebuildList( null );
      return VSConstants.S_OK;
    }

    public int OnQueryChangeProjectParent( IVsHierarchy pHierarchy, IVsHierarchy pNewParentHier, ref int pfCancel )
    {
      return VSConstants.S_OK;
    }

    #endregion

		#region IVsTrackProjectDocumentsEvents2 Members

		public int OnAfterAddDirectoriesEx( int cProjects, int cDirectories, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, VSADDDIRECTORYFLAGS[] rgFlags )
		{
			RebuildList( null );
			return VSConstants.S_OK;
		}

		public int OnAfterAddFilesEx( int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, VSADDFILEFLAGS[] rgFlags )
		{
			RebuildList( null );
			return VSConstants.S_OK;
		}

		public int OnAfterRemoveDirectories( int cProjects, int cDirectories, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, VSREMOVEDIRECTORYFLAGS[] rgFlags )
		{
			RebuildList( null );
			return VSConstants.S_OK;
		}

		public int OnAfterRemoveFiles( int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, VSREMOVEFILEFLAGS[] rgFlags )
		{
			RebuildList( null );
			return VSConstants.S_OK;
		}

		public int OnAfterRenameDirectories( int cProjects, int cDirs, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgszMkOldNames, string[] rgszMkNewNames, VSRENAMEDIRECTORYFLAGS[] rgFlags )
		{
			RebuildList( null );
			return VSConstants.S_OK;
		}

		public int OnAfterRenameFiles( int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgszMkOldNames, string[] rgszMkNewNames, VSRENAMEFILEFLAGS[] rgFlags )
		{
			RebuildList( null );
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

    #region IDisposable Members

    public void Dispose()
    {
      if ( !disposed )
      {
				if ( serviceProvider != null )
				{
					if ( eventsCookie != VSConstants.VSCOOKIE_NIL )
					{
						IVsSolution solution = serviceProvider.GetService( typeof( SVsSolution ) ) as IVsSolution;
						solution.UnadviseSolutionEvents( eventsCookie );
						eventsCookie = VSConstants.VSCOOKIE_NIL;
					}
		
					if ( trackProjectDocumentsCookie != VSConstants.VSCOOKIE_NIL )
					{
						IVsTrackProjectDocuments2 trackProjectDocuments = serviceProvider.GetService( typeof( SVsTrackProjectDocuments ) ) as IVsTrackProjectDocuments2;
						trackProjectDocuments.UnadviseTrackProjectDocumentsEvents( trackProjectDocumentsCookie );
						trackProjectDocumentsCookie = VSConstants.VSCOOKIE_NIL;
					}
				}

				disposed = true;
      }
      GC.SuppressFinalize( this );
    }

    #endregion
	}
}
