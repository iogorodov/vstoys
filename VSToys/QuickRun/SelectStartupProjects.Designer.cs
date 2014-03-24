namespace VSToys.QuickRun
{
	partial class SelectStartupProjects
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose( bool disposing )
		{
			if ( disposing && ( components != null ) )
			{
				components.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.buttonOk = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.listBoxProjects = new System.Windows.Forms.CheckedListBox();
			this.checkBoxSelectAll = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// buttonOk
			// 
			this.buttonOk.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right ) ) );
			this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOk.Location = new System.Drawing.Point( 197, 12 );
			this.buttonOk.Name = "buttonOk";
			this.buttonOk.Size = new System.Drawing.Size( 75, 23 );
			this.buttonOk.TabIndex = 1;
			this.buttonOk.Text = "OK";
			this.buttonOk.UseVisualStyleBackColor = true;
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right ) ) );
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Location = new System.Drawing.Point( 197, 41 );
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size( 75, 23 );
			this.buttonCancel.TabIndex = 2;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			// 
			// listBoxProjects
			// 
			this.listBoxProjects.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom )
									| System.Windows.Forms.AnchorStyles.Left )
									| System.Windows.Forms.AnchorStyles.Right ) ) );
			this.listBoxProjects.CheckOnClick = true;
			this.listBoxProjects.FormattingEnabled = true;
			this.listBoxProjects.IntegralHeight = false;
			this.listBoxProjects.Location = new System.Drawing.Point( 12, 12 );
			this.listBoxProjects.Name = "listBoxProjects";
			this.listBoxProjects.Size = new System.Drawing.Size( 179, 242 );
			this.listBoxProjects.TabIndex = 3;
			this.listBoxProjects.ThreeDCheckBoxes = true;
			this.listBoxProjects.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler( this.OnListBoxProjectsItemCheck );
			// 
			// checkBoxSelectAll
			// 
			this.checkBoxSelectAll.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right ) ) );
			this.checkBoxSelectAll.AutoSize = true;
			this.checkBoxSelectAll.Location = new System.Drawing.Point( 197, 237 );
			this.checkBoxSelectAll.Name = "checkBoxSelectAll";
			this.checkBoxSelectAll.Size = new System.Drawing.Size( 70, 17 );
			this.checkBoxSelectAll.TabIndex = 4;
			this.checkBoxSelectAll.Text = "Select All";
			this.checkBoxSelectAll.UseVisualStyleBackColor = true;
			this.checkBoxSelectAll.CheckedChanged += new System.EventHandler( this.OnCheckBoxSelectAllCheckedChanged );
			// 
			// SelectStartupProjects
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size( 284, 266 );
			this.Controls.Add( this.checkBoxSelectAll );
			this.Controls.Add( this.listBoxProjects );
			this.Controls.Add( this.buttonCancel );
			this.Controls.Add( this.buttonOk );
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SelectStartupProjects";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Startup Projects for Solution \"{0}\"";
			this.ResumeLayout( false );
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button buttonOk;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.CheckedListBox listBoxProjects;
		private System.Windows.Forms.CheckBox checkBoxSelectAll;

	}
}