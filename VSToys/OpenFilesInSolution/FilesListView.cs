using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace VSToys.OpenFilesInSolution
{
	public partial class FilesListView : ListView
	{
		#region Icon's cache

		private const int MAX_PATH = 260;
		private const int FILE_ATTRIBUTE_NORMAL = 0x80;
		private const int SHGFI_ICON = 0x100;
		private const int SHGFI_SMALLICON = 0x1;
		private const int SHGFI_USEFILEATTRIBUTES = 0x10;

		[StructLayout( LayoutKind.Sequential )]
		private struct SHFILEINFO
		{
			public IntPtr hIcon;
			public IntPtr iIcon;
			public uint dwAttributes;
			[MarshalAs( UnmanagedType.ByValTStr, SizeConst = MAX_PATH )]
			public string szDisplayName;
			[MarshalAs( UnmanagedType.ByValTStr, SizeConst = 80 )]
			public string szTypeName;
		};

		[DllImport( "shell32" )]
		private static extern int SHGetFileInfo( string pszPath, int dwFileAttributes, ref SHFILEINFO psfi, int cbFileInfo, int uFlags );
		
		private static Dictionary<string, Icon> icons = new Dictionary<string, Icon>();

		private static Icon GetIcon( string extension )
		{
			string extensionLower = extension.ToLower();
			Icon icon = null;
			if ( icons.TryGetValue( extensionLower, out icon ) )
				return icon;

			SHFILEINFO shinfo = new SHFILEINFO();
			int hFolderIcon = SHGetFileInfo( extensionLower, FILE_ATTRIBUTE_NORMAL, ref shinfo, Marshal.SizeOf( shinfo ),
				SHGFI_ICON | SHGFI_SMALLICON | SHGFI_USEFILEATTRIBUTES );
			icon = Icon.FromHandle( shinfo.hIcon );
			icons.Add( extensionLower, icon );

			return icon;
		}

		#endregion

		#region private class FileInSolutionItem

		private class FileInSolutionItem
		{
			private readonly string project = string.Empty;
			private readonly SolutionFilesCollector.FileInSolution file = null;
			private readonly Icon icon = null;
			private ListViewItem item = null;

			public FileInSolutionItem( string project ) { this.project = project; }

			public FileInSolutionItem( SolutionFilesCollector.FileInSolution file )
			{ 
				this.file = file;
				icon = GetIcon( Path.GetExtension( file.FileName ) );
			}

			public ListViewItem GetListViewItem()
			{
				if ( item == null )
				{
					item = new ListViewItem();
					item.Tag = this;
					if ( IsFile )
					{
						item.SubItems.Add( file.FileName );
						item.SubItems.Add( file.FilePath );
					}
					else
					{
						item.SubItems.Add( project );
						item.SubItems.Add( string.Empty );
					}

				}
				return item;
			}

			public bool IsProject { get { return file == null; } } 
			public bool IsFile { get { return file != null; } }
			public Icon Icon { get { return icon; } }
			public SolutionFilesCollector.FileInSolution File { get { return file; } }
		}

		#endregion

		private List<FileInSolutionItem> files = new List<FileInSolutionItem>();
		private List<SolutionFilesCollector.FileInSolution> selectedFiles = new List<SolutionFilesCollector.FileInSolution>();
		private int fileNameColumnWidth = 150;
		private int filesCount = 0;
		private Font groupFont = null;
		
		public FilesListView()
		{
			groupFont = new Font( Font, FontStyle.Bold );
			DoubleBuffered = true;
			
			//TODO: Hide these properties
			View = View.Details;
			OwnerDraw = true;
			FullRowSelect = true;
			HideSelection = false;
			VirtualMode = true;
			HeaderStyle = ColumnHeaderStyle.None;

			Columns.Add( string.Empty ).Width = 21;
			Columns.Add( "File Name" ).Width = fileNameColumnWidth;
			Columns.Add( "File Path" ).Width = -2;

			SetStyle( ControlStyles.ResizeRedraw, true );
			SmallImageList = new ImageList();
			SmallImageList.ImageSize = new Size( 16, 16 );

			InitializeComponent();
		}

		protected override void OnFontChanged( EventArgs e )
		{
			base.OnFontChanged( e );
			groupFont = new Font( Font, FontStyle.Bold );
		}

		protected override void OnDrawItem( DrawListViewItemEventArgs e )
		{
			FileInSolutionItem item = e.Item.Tag as FileInSolutionItem;
			if ( item.IsProject && ProjectBackColor != Color.Transparent )
				e.Graphics.FillRectangle( new SolidBrush( ProjectBackColor ), e.Bounds );
			else if ( e.Item.Selected && !item.IsProject )
				e.Graphics.FillRectangle( new SolidBrush( Color.FromKnownColor( KnownColor.Highlight ) ), e.Bounds );
			else if ( e.ItemIndex % 2 == 0 )
				e.Graphics.FillRectangle( new SolidBrush( AlternativeBackColor ), e.Bounds );

			if ( ( e.State & ListViewItemStates.Focused ) == ListViewItemStates.Focused )
				e.Graphics.DrawRectangle( new Pen( Color.FromKnownColor( KnownColor.WindowText ) ) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dot },
					e.Bounds.X, e.Bounds.Y, e.Bounds.Width - 1, e.Bounds.Height - 1 );
			
			//base.OnDrawItem( e );
		}

		protected override void OnDrawSubItem( DrawListViewSubItemEventArgs e )
		{
			FileInSolutionItem item = e.Item.Tag as FileInSolutionItem;
			if ( e.ColumnIndex == 0 )
			{
				if ( item.Icon != null )
					e.Graphics.DrawIconUnstretched( item.Icon, new Rectangle( e.Bounds.X + 2, e.Bounds.Y, 16, 16 ) );
			}
			else
			{
				Point pt = new Point( e.Bounds.X, e.Bounds.Y + 2 );
				if ( item.IsProject )
					TextRenderer.DrawText( e.Graphics, e.SubItem.Text, groupFont, pt, ProjectForeColor );
				else if ( e.Item.Selected )
					TextRenderer.DrawText( e.Graphics, e.SubItem.Text, Font, pt, Color.FromKnownColor( KnownColor.HighlightText ) );
				else
					TextRenderer.DrawText( e.Graphics, e.SubItem.Text, Font, pt, ForeColor );
			}

			//base.OnDrawSubItem( e );
		}

		protected override void OnRetrieveVirtualItem( RetrieveVirtualItemEventArgs e )
		{
			e.Item = files[e.ItemIndex].GetListViewItem();
		}

		protected override void OnSelectedIndexChanged( EventArgs e )
		{
			selectedFiles.Clear();
			foreach ( var index in SelectedIndices )
			{
				if ( files[(int)index].IsFile )
					selectedFiles.Add( files[(int)index].File );
			}
			base.OnSelectedIndexChanged( e );
		}

		protected override void OnVirtualItemsSelectionRangeChanged( ListViewVirtualItemsSelectionRangeChangedEventArgs e )
		{
			selectedFiles.Clear();
			foreach ( var index in SelectedIndices )
			{
				if ( files[(int)index].IsFile )
					selectedFiles.Add( files[(int)index].File );
			}
			base.OnVirtualItemsSelectionRangeChanged( e );
		}

		public void ClearList()
		{
			BeginUpdate();
			VirtualListSize = 0;
			filesCount = 0;
			files.Clear();
			EndUpdate();
			Invalidate();
		}

		public void RebuildList( SolutionFilesCollector collector, string filter )
		{
			BeginUpdate();
			files.Clear();

			filesCount = 0;
			fileNameColumnWidth = 0;
			int bestMatchItem = -1;
			int bestMatchEnsureVisible = -1;
			string prevProjectGroup = string.Empty;
			foreach ( SolutionFilesCollector.FileInSolution file in collector.Files )
			{
				int width = TextRenderer.MeasureText( file.FileName, Font ).Width;
				if ( width > fileNameColumnWidth )
					fileNameColumnWidth = width;

				if ( file.FileName.IndexOf( filter, StringComparison.InvariantCultureIgnoreCase ) >= 0 )
				{
					bool firstFileInProject = false;
					if ( file.ProjectName != prevProjectGroup )
					{
						files.Add( new FileInSolutionItem( file.ProjectName ) );
						prevProjectGroup = file.ProjectName;
						firstFileInProject = true;
					}
					files.Add( new FileInSolutionItem( file ) );
					++filesCount;

					if ( bestMatchItem == -1 && file.FileName.StartsWith( filter, StringComparison.InvariantCultureIgnoreCase ) )
					{
						bestMatchItem = files.Count - 1;
						bestMatchEnsureVisible = files.Count - ( firstFileInProject ? 2 : 1 );
					}
				}
			}

			VirtualListSize = files.Count;
			if ( VirtualListSize > 0 )
			{
				if ( bestMatchItem >= 0 )
				{
					EnsureVisible( bestMatchEnsureVisible );
					SelectedIndices.Clear();
					SelectedIndices.Add( bestMatchItem );
					FocusedItem = Items[bestMatchItem];
				}
				else
					EnsureVisible( 0 );
			}
			else
			{
				SelectedIndices.Clear();
				OnSelectedIndexChanged( EventArgs.Empty );
			}

			EndUpdate();
			UpdateColumnWidths();
			Invalidate();
		}

		public void UpdateColumnWidths()
		{
			Columns[1].Width = fileNameColumnWidth + 20;
			Columns[2].Width = ClientSize.Width - Columns[0].Width - Columns[1].Width;
		}

		public int FilesCount { get { return filesCount; } }

		public IEnumerable<SolutionFilesCollector.FileInSolution> SelectedFiles { get { return selectedFiles; } }
		public int SelectedFilesCount { get { return selectedFiles.Count; } } 

		#region Control properties

		private Color projectForeColor = Color.FromKnownColor( KnownColor.Highlight );
		[Category( "Appearance" )]
		[DefaultValue( typeof( Color ), "Highlight" )]
		public Color ProjectForeColor { get { return projectForeColor; } set { projectForeColor = value; } }

		private Color projectBackColor = Color.FromKnownColor( KnownColor.Window );
		[Category( "Appearance" )]
		[DefaultValue( typeof( Color ), "Window" )]
		public Color ProjectBackColor { get { return projectBackColor; } set { projectBackColor = value; } }

		private Color alternativeBackColor = Color.FromKnownColor( KnownColor.Window );
		[Category( "Appearance" )]
		[DefaultValue( typeof( Color ), "Window" )]
		public Color AlternativeBackColor { get { return alternativeBackColor; } set { alternativeBackColor = value; } }

		#endregion
	}
}
