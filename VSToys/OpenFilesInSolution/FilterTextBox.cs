using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace VSToys.OpenFilesInSolution
{
	public partial class FilterTextBox : TextBox
	{
		[DllImport( "user32.dll", CharSet = CharSet.Auto, SetLastError = false )]
		private static extern IntPtr SendMessage( IntPtr hWnd, Int32 Msg, IntPtr wParam, IntPtr lParam );

		public FilterTextBox()
		{
			InitializeComponent();
		}

		protected override void WndProc( ref Message m )
		{
			if ( linkedControl != null && linkedControl.IsHandleCreated )
			{
				if ( m.Msg == 0x020A || ( ( m.Msg == 0x0100 || m.Msg == 0x0101 ) && ( m.WParam.ToInt32() == 0x26 || m.WParam.ToInt32() == 0x28 ) ) )
				{
					m.Result = SendMessage( linkedControl.Handle, m.Msg, m.WParam, m.LParam );
					return;
				}
			}

			base.WndProc( ref m );
		}

		private Control linkedControl = null;
		public Control LinkedControl { get { return linkedControl; } set { linkedControl = value; } }
	}
}
