using Helpers;
using System.Configuration;
using System.IO;

namespace MediaWatch
{
    public class Program
    {
        private static void Main()
        {
            var mw = new MediaWatcher
            {
                OutputDirectory = Utility.OutputDirectory()
            };

            if (!Directory.Exists(mw.OutputDirectory))
                mw.OutputDirectory = Directory.GetCurrentDirectory();

            mw.Configure(ConfigurationManager.AppSettings);

            mw.Execute();

            mw.DumpXml();
        }
    }
}