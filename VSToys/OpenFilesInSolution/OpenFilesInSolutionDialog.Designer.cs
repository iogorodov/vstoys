namespace VSToys.OpenFilesInSolution
{
  partial class OpenFilesInSolutionDialog
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
      if ( disposing && (components != null) )
      {
        components.Dispose();
      }

			if ( disposing )
				filesCollector.Dispose();
      base.Dispose( disposing );
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
			this.buttonsPanel = new System.Windows.Forms.Panel();
			this.okButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.separatorPanel2 = new System.Windows.Forms.Panel();
			this.messageLabel = new System.Windows.Forms.Label();
			this.infoLabel = new System.Windows.Forms.Label();
			this.separatorPanel1 = new System.Windows.Forms.Panel();
			this.bottomPanel = new System.Windows.Forms.Panel();
			this.files = new VSToys.OpenFilesInSolution.FilesListView();
			this.filter = new VSToys.OpenFilesInSolution.FilterTextBox();
			this.buttonsPanel.SuspendLayout();
			this.bottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// buttonsPanel
			// 
			this.buttonsPanel.Controls.Add( this.okButton );
			this.buttonsPanel.Controls.Add( this.cancelButton );
			this.buttonsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.buttonsPanel.Location = new System.Drawing.Point( 0, 336 );
			this.buttonsPanel.Name = "buttonsPanel";
			this.buttonsPanel.Size = new System.Drawing.Size( 684, 30 );
			this.buttonsPanel.TabIndex = 1;
			// 
			// okButton
			// 
			this.okButton.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right ) ) );
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Enabled = false;
			this.okButton.Location = new System.Drawing.Point( 525, 4 );
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size( 75, 23 );
			this.okButton.TabIndex = 2;
			this.okButton.Text = "Open";
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler( this.OnOkButtonClick );
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right ) ) );
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point( 606, 4 );
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size( 75, 23 );
			this.cancelButton.TabIndex = 3;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// separatorPanel2
			// 
			this.separatorPanel2.BackColor = System.Drawing.SystemColors.ControlDark;
			this.separatorPanel2.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.separatorPanel2.Location = new System.Drawing.Point( 0, 335 );
			this.separatorPanel2.Name = "separatorPanel2";
			this.separatorPanel2.Size = new System.Drawing.Size( 684, 1 );
			this.separatorPanel2.TabIndex = 10;
			// 
			// messageLabel
			// 
			this.messageLabel.BackColor = System.Drawing.SystemColors.Window;
			this.messageLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.messageLabel.ForeColor = System.Drawing.Color.Blue;
			this.messageLabel.Location = new System.Drawing.Point( 0, 0 );
			this.messageLabel.Name = "messageLabel";
			this.messageLabel.Size = new System.Drawing.Size( 684, 321 );
			this.messageLabel.TabIndex = 11;
			this.messageLabel.Text = "Solution not open or there is no any files in solution.";
			this.messageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// infoLabel
			// 
			this.infoLabel.AutoSize = true;
			this.infoLabel.BackColor = System.Drawing.SystemColors.Info;
			this.infoLabel.Dock = System.Windows.Forms.DockStyle.Right;
			this.infoLabel.ForeColor = System.Drawing.SystemColors.ControlDark;
			this.infoLabel.Location = new System.Drawing.Point( 654, 0 );
			this.infoLabel.Name = "infoLabel";
			this.infoLabel.Size = new System.Drawing.Size( 30, 13 );
			this.infoLabel.TabIndex = 1;
			this.infoLabel.Text = "0 / 0";
			this.infoLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// separatorPanel1
			// 
			this.separatorPanel1.BackColor = System.Drawing.SystemColors.ControlDark;
			this.separatorPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.separatorPanel1.Location = new System.Drawing.Point( 0, 321 );
			this.separatorPanel1.Name = "separatorPanel1";
			this.separatorPanel1.Size = new System.Drawing.Size( 684, 1 );
			this.separatorPanel1.TabIndex = 9;
			// 
			// bottomPanel
			// 
			this.bottomPanel.BackColor = System.Drawing.SystemColors.Info;
			this.bottomPanel.Controls.Add( this.filter );
			this.bottomPanel.Controls.Add( this.infoLabel );
			this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.bottomPanel.Location = new System.Drawing.Point( 0, 322 );
			this.bottomPanel.Name = "bottomPanel";
			this.bottomPanel.Padding = new System.Windows.Forms.Padding( 3, 0, 0, 0 );
			this.bottomPanel.Size = new System.Drawing.Size( 684, 13 );
			this.bottomPanel.TabIndex = 12;
			// 
			// files
			// 
			this.files.AlternativeBackColor = System.Drawing.Color.WhiteSmoke;
			this.files.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.files.Dock = System.Windows.Forms.DockStyle.Fill;
			this.files.FullRowSelect = true;
			this.files.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.files.HideSelection = false;
			this.files.Location = new System.Drawing.Point( 0, 0 );
			this.files.Name = "files";
			this.files.OwnerDraw = true;
			this.files.ProjectBackColor = System.Drawing.Color.Transparent;
			this.files.ProjectForeColor = System.Drawing.Color.RoyalBlue;
			this.files.ShowGroups = false;
			this.files.Size = new System.Drawing.Size( 684, 321 );
			this.files.TabIndex = 1;
			this.files.UseCompatibleStateImageBehavior = false;
			this.files.View = System.Windows.Forms.View.Details;
			this.files.VirtualMode = true;
			this.files.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler( this.FilesMouseDoubleClick );
			this.files.VirtualItemsSelectionRangeChanged += new System.Windows.Forms.ListViewVirtualItemsSelectionRangeChangedEventHandler( this.FilesVirtualItemsSelectionChanged );
			this.files.SelectedIndexChanged += new System.EventHandler( this.FilesSelectedIndexChanged );
			this.files.Enter += new System.EventHandler( this.OnFilesListViewEnter );
			// 
			// filter
			// 
			this.filter.BackColor = System.Drawing.SystemColors.Info;
			this.filter.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.filter.Dock = System.Windows.Forms.DockStyle.Fill;
			this.filter.LinkedControl = this.files;
			this.filter.Location = new System.Drawing.Point( 3, 0 );
			this.filter.Name = "filter";
			this.filter.Size = new System.Drawing.Size( 651, 13 );
			this.filter.TabIndex = 0;
			this.filter.TextChanged += new System.EventHandler( this.OnFilterTextChanged );
			this.filter.ClientSizeChanged += new System.EventHandler( this.OnFilterClientSizeChanged );
			// 
			// OpenFilesInSolutionDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size( 684, 366 );
			this.Controls.Add( this.files );
			this.Controls.Add( this.messageLabel );
			this.Controls.Add( this.separatorPanel1 );
			this.Controls.Add( this.bottomPanel );
			this.Controls.Add( this.separatorPanel2 );
			this.Controls.Add( this.buttonsPanel );
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.KeyPreview = true;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size( 700, 400 );
			this.Name = "OpenFilesInSolutionDialog";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Text = "Open Files in Solution";
			this.buttonsPanel.ResumeLayout( false );
			this.bottomPanel.ResumeLayout( false );
			this.bottomPanel.PerformLayout();
			this.ResumeLayout( false );

    }

    #endregion

		private FilesListView files;
		private FilterTextBox filter;
		private System.Windows.Forms.Panel buttonsPanel;
    private System.Windows.Forms.Button okButton;
    private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Panel separatorPanel2;
		private System.Windows.Forms.Label messageLabel;
		private System.Windows.Forms.Label infoLabel;
		private System.Windows.Forms.Panel separatorPanel1;
		private System.Windows.Forms.Panel bottomPanel;
  }
}