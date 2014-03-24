using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace VSToys.SimpleToys
{
	public sealed class BeepOnBuild
	{
		private const int MB_ICONHAND = 0x10;
		private const int MB_ICONASTERISK = 0x40;

		[DllImport( "user32" )]
		private static extern int MessageBeep( uint uType );

		private BeepOnBuild( VSToysPackage package )
		{
			package.Events.UpdateSolutionDone += new VSToysEvents.UpdateSolutionDoneHandler( OnUpdateSolutionDone );
		}

		private void OnUpdateSolutionDone( VSToysPackage package, bool succeeded, bool modified, bool canceled )
		{
			if ( !succeeded || canceled )
				MessageBeep( MB_ICONHAND );
			else if ( modified )
				MessageBeep( MB_ICONASTERISK );
		}

		private static BeepOnBuild beepOnBuild = null;

		public static void Initialize( VSToysPackage package )
		{
			beepOnBuild = new BeepOnBuild( package );
		}
	}
}
