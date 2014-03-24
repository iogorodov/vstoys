using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell.Interop;

namespace VSToys.OpenFilesInSolution
{
  public partial class OpenFilesInSolutionDialog : Form
  {
		private readonly SolutionFilesCollector filesCollector = new SolutionFilesCollector();

		public OpenFilesInSolutionDialog( IServiceProvider serviceProvider )
    {
			filesCollector.Init( serviceProvider );

			InitializeComponent();
    }

		private void ShowMessage( bool error )
		{
			files.Visible = !error;
			files.Enabled = !error;
			filter.Enabled = !error;
			filter.Text = string.Empty;
			filter.BackColor = error ? Color.FromKnownColor( KnownColor.Control ) : Color.FromKnownColor( KnownColor.Info );
			infoLabel.BackColor = error ? Color.FromKnownColor( KnownColor.Control ) : Color.FromKnownColor( KnownColor.Info );
			bottomPanel.BackColor = error ? Color.FromKnownColor( KnownColor.Control ) : Color.FromKnownColor( KnownColor.Info );
			messageLabel.Visible = error;
			messageLabel.Enabled = error;
		}

		protected override void OnLoad( EventArgs e )
		{
			base.OnLoad( e );

			bottomPanel.Height = filter.Height;

			if ( filesCollector.IsEmpty )
			{
				ShowMessage( true );
				files.ClearList();
				infoLabel.Text = "0 / 0";
			}
			else
			{
				ShowMessage( false );
				files.RebuildList( filesCollector, filter.Text );
				infoLabel.Text = files.FilesCount + " / " + filesCollector.FilesCount;
			}
		}

		protected override void OnClientSizeChanged( EventArgs e )
		{
			base.OnClientSizeChanged( e );
			files.UpdateColumnWidths();
		}

		private void OnOkButtonClick( object sender, EventArgs e )
		{
			foreach ( var file in files.SelectedFiles )
				file.OpenFile();
			Close();
		}

		private void OnFilterClientSizeChanged( object sender, EventArgs e )
		{
			bottomPanel.Height = ( sender as Control ).Height;
		}

		private void OnFilterTextChanged( object sender, EventArgs e )
		{
			files.RebuildList( filesCollector, filter.Text );
			infoLabel.Text = files.FilesCount + " / " + filesCollector.FilesCount;
		}

		private void OnFilesListViewEnter( object sender, EventArgs e )
		{
			filter.Focus();
		}

		private void FilesSelectedIndexChanged( object sender, EventArgs e )
		{
			okButton.Enabled = files.SelectedFilesCount > 0;
		}

		private void FilesVirtualItemsSelectionChanged( object sender, ListViewVirtualItemsSelectionRangeChangedEventArgs e )
		{
			okButton.Enabled = files.SelectedFilesCount > 0;
		}

		private void FilesMouseDoubleClick( object sender, MouseEventArgs e )
		{
			if ( files.SelectedFilesCount > 0 )
			{
				foreach ( var file in files.SelectedFiles )
					file.OpenFile();
				Close();
			}
		}

		private const int cmdidOpenFilesInSolution = 0x100;

		private static void ShowOpenFilesInSolutionDialog( VSToysPackage package, object sender, EventArgs args )
		{
			IVsUIShell uiShell = package.GetService<IVsUIShell>( typeof( SVsUIShell ) );
			if ( uiShell == null )
				return;

			IntPtr hwnd = IntPtr.Zero;
			Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure( uiShell.GetDialogOwnerHwnd( out hwnd ) );

			new OpenFilesInSolutionDialog( package ).ShowDialog( NativeWindow.FromHandle( hwnd ) );
		}

		public static void Initialize( VSToysPackage package )
		{
			package.AddMenuCommandItem( cmdidOpenFilesInSolution, ShowOpenFilesInSolutionDialog, null );
		}
  }
}
