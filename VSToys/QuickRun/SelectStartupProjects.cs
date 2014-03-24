using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using EnvDTE;

namespace VSToys.QuickRun
{
	public partial class SelectStartupProjects : Form
	{
		private bool skipUpdate = false;

		public SelectStartupProjects( string solutionName, List<Project> projects, Dictionary<string, bool> excludedProjects )
		{
			InitializeComponent();
			Text = string.Format( Text, solutionName );

			foreach ( var project in projects )
				listBoxProjects.Items.Add( project.Name, !excludedProjects.ContainsKey( project.Name ) );

			OnListBoxProjectsItemCheck( listBoxProjects, new ItemCheckEventArgs( -1, CheckState.Indeterminate, CheckState.Indeterminate ) );
		}

		private void OnListBoxProjectsItemCheck( object sender, ItemCheckEventArgs e )
		{
			if ( skipUpdate )
				return;

			bool allChecked = true;
			bool allUnchecked = true;
			for ( int i = 0; i < listBoxProjects.Items.Count; ++i )
			{
				bool itemChecked = ( e.Index == i ) ? e.NewValue == CheckState.Checked : listBoxProjects.GetItemChecked( i );
				allChecked &= itemChecked;
				allUnchecked &= !itemChecked;
				if ( !allChecked && !allUnchecked )
					break;
			}

			skipUpdate = true;
			if ( allChecked == allUnchecked )
				checkBoxSelectAll.CheckState = CheckState.Indeterminate;
			else if ( allChecked )
				checkBoxSelectAll.CheckState = CheckState.Checked;
			else if ( allUnchecked )
				checkBoxSelectAll.CheckState = CheckState.Unchecked;
			skipUpdate = false;
		}

		private void OnCheckBoxSelectAllCheckedChanged( object sender, EventArgs e )
		{
			if ( skipUpdate )
				return;

			if ( checkBoxSelectAll.CheckState == CheckState.Indeterminate )
				return;

			skipUpdate = true;
			for ( int i = 0; i < listBoxProjects.Items.Count; ++i )
				listBoxProjects.SetItemChecked( i, checkBoxSelectAll.Checked );
			skipUpdate = false;
		}

		public IEnumerable<string> GetExcludedProjects()
		{
			List<string> result = new List<string>();
			for ( int i = 0; i < listBoxProjects.Items.Count; ++i )
			{
				if ( !listBoxProjects.GetItemChecked( i ) )
					result.Add( listBoxProjects.Items[i].ToString() );
			}
			return result; 
		}
	}
}
