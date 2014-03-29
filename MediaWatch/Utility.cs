using System;
using System.Diagnostics;
using System.Reflection;
using System.Collections;


namespace MediaWatch
{
	public static class Utility
	{

		/// <summary>
		///   Different machines will need to output to different places due to local policies
		///   not a good idea to have the durectory as a konstant
		/// </summary>
		/// <returns></returns>
		public static string OutputDirectory()
		{
			string outputDir;
			if ( HostName() == Constants.K_WORK_MACHINE )
				outputDir = @"c:\Userdata\temp\Media\";  //  not allowed to use the C drive much
			else
				//  could be Vesuvius, laptop, Rimmer linked to Dropbox
				outputDir = @"c:\public\GridStat\";

			return outputDir;
		}

		public static string HostName()
		{
			return Environment.MachineName;
		}

		public static decimal Percent( int quotient, int divisor )
		{
			return 100 * Average( quotient, divisor );
		}

		public static decimal Average( int quotient, int divisor )
		{
			//  need to do decimal other wise INT() will occur
			if ( divisor == 0 ) return 0.0M;
			return ( Decimal.Parse( quotient.ToString() ) /
				Decimal.Parse( divisor.ToString() ) );
		}

		public static void Announce( string rpt, int indent = 3 )
		{
			int theLength = rpt.Length + indent;
			rpt = rpt.PadLeft( theLength, ' ' );
			Console.WriteLine( rpt );
//			WriteLog( rpt );
#if DEBUG
			Debug.WriteLine( rpt );
#endif
		}


		public static void CopyFile( string fromFile, string targetFile )
		{
			string sourcePath = OutputDirectory();
			string targetPath = OutputDirectory();
			string sourceFile = System.IO.Path.Combine( sourcePath, fromFile );
			string destFile = System.IO.Path.Combine( targetPath, targetFile );
			// To copy a file to another location and 
			// overwrite the destination file if it already exists.
			System.IO.File.Copy( sourceFile, destFile, true );
		}

	}
}
