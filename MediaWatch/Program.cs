using System;
using System.IO;
using System.Configuration;

namespace MediaWatch
{
	public class Program
	{
		static void Main()
		{
			var mw = new MediaWatcher {OutputDirectory = Utility.OutputDirectory()};
			if ( !Directory.Exists( mw.OutputDirectory ) )
				mw.OutputDirectory = Directory.GetCurrentDirectory();

			mw.Inits = "LSC";

			//  load up all the media folders
			var appSettings = ConfigurationManager.AppSettings;
    
			// Get the collection enumerator.
			var appSettingsEnum =  appSettings.Keys.GetEnumerator();

			 // Loop through the collection and
			 // display the appSettings key, value pairs.
			 var i = 0;
			 while ( appSettingsEnum.MoveNext() )
			 {
				 var key = appSettings.Keys[ i ];
				 if ( key.StartsWith( "MediaFolder" ) )
				 {
					 mw.FolderList.Add( new Models.FolderItem { Name = appSettings[ key ] } );
					 Utility.Announce( string.Format( "Adding Media Folder {0}", appSettings[ key ] ) );
				 }
				 else if ( key.StartsWith( "OutputDirectory" ) )
				 {
					 mw.OutputDirectory = appSettings[ key ];
				 }
				 else if ( key.StartsWith( "XmlDirectory" ) )
				 {
					 mw.XmlDirectory = appSettings[ key ];
				 }
				 else if ( key.StartsWith( "Inits" ) )
				 {
					 mw.Inits = appSettings[ key ];
				 }
				 else if ( key.StartsWith( "DaysBack" ) )
				 {
					 mw.DaysBack = Int32.Parse( appSettings[ key ] );
				 }
             else if ( key.StartsWith( "Report-Latest" ) )
             {
                var latest = appSettings[ key ];
                if ( latest.Equals("Yes") )
                    mw.DoLatest = true;
             }
             else if ( key.StartsWith( "Report-Movies" ) )
             {
                var movies = appSettings[ key ];
                if ( movies.Equals( "Yes" ) )
                  mw.DoMovies = true;
             }
             else if ( key.StartsWith( "Report-TV" ) )
             {
                var tv = appSettings[ key ];
                if ( tv.Equals( "Yes" ) )
                  mw.DoTv = true;
             }
				 i++;
			 }

			var mm = new MediaMaster( "Media", mw.XmlDirectory, "media.xml", mw.Logger );
			mw.Master = mm;

         if ( mw.DoMovies )
         {
            mw.Logger.Info( "---Movies--------------------------------------------------------------------------------" );
            mw.RenderMoviesAsHtml();
         }
         if ( mw.DoTv )
         {
            mw.Logger.Info( "---TV------------------------------------------------------------------------------------" );
            mw.RenderTvAsHtml();
         }
         if ( mw.DoLatest )
         {
            mw.Logger.Info( "---Latest--------------------------------------------------------------------------------" );
            mw.RenderLatestFilesAsHtml();
         }

			mm.Dump2Xml();
		}
	}
}
