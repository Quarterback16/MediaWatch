using System;
using System.IO;
using System.Configuration;
using Helpers;

namespace MediaWatch
{
	public class Program
	{
		static void Main()
		{
			var mw = new MediaWatcher {OutputDirectory = Utility.OutputDirectory()};
			if ( !Directory.Exists( mw.OutputDirectory ) )
				mw.OutputDirectory = Directory.GetCurrentDirectory();

         mw.Configure(ConfigurationManager.AppSettings);

         mw.Execute();

			mw.DumpXml();
		}

	}
}
