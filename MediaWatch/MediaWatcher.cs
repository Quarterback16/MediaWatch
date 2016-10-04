using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using MediaWatch.Logging;
using MediaWatch.Models;
using Helpers;

namespace MediaWatch
{
	public class MediaWatcher
	{
		public MediaMaster Master { get; set; }
		public string OutputDirectory { get; set; }
		public string XmlDirectory { get; set; }
		public string Inits { get; set; }
		public int DaysBack { get; set; }
		public DateTime CutoffDate { get; set; }
		public List<MediaItem> MediaList { get; set; }
		public List<FolderItem> FolderList { get; set; }
		public NLogger Logger { get; set; }
		public bool DoLatest { get; set; }
		public bool DoMovies { get; set; }
		public bool DoTv { get; set; }
		public string ReportType { get; set; }

		public MediaWatcher()
		{
			FolderList = new List<FolderItem>();
			MediaList = new List<MediaItem>();
			Logger = new NLogger();
			Logger.Info( "Media Watcher ver. 2.00" );
		}

      public void Execute()
      {
         if (DoMovies)
         {
            Logger.Info("---Movies--------------------------------------------------------------------------------");
            RenderMoviesAsHtml();
         }
         if (DoTv)
         {
            Logger.Info("---TV------------------------------------------------------------------------------------");
            RenderTvAsHtml();
         }
         if (DoLatest)
         {
            Logger.Info("---Latest--------------------------------------------------------------------------------");
            RenderLatestFilesAsHtml();
         }
      }

		public void RenderMoviesAsHtml()
		{
			ReportType = "Movie Report";
         
			var str = new SimpleTableReport( string.Format( "{1} {0}", Inits, ReportType ) );
			str.AddStyle(
				"#container { text-align: left; background-color: #ccc; margin: 0 auto; border: 1px solid #545454; width: 641px; padding:10px; font: 13px/19px Trebuchet MS, Georgia, Times New Roman, serif; }" );
			str.AddStyle( "#main { margin-left:1em; }" );
			str.AddStyle( "#dtStamp { font-size:0.8em; }" );
			str.AddStyle( ".end { clear: both; }" );
			str.AddStyle( ".gponame { color:white; background:black }" );
			str.ColumnHeadings = true;
			str.DoRowNumbers = true;
			str.ShowElapsedTime = false;
			str.IsFooter = false;
			str.AddColumn( new ReportColumn( "Title", "TITLE", "{0}", typeof (String) ) );
			str.AddColumn( new ReportColumn( "Format", "FORMAT", "{0}", typeof (String) ) );
			str.AddColumn( new ReportColumn( "Date Added", "ADDED", "{0}", typeof (String) ) );
			BuildTable( str, "Movie", false );
			str.SetSortOrder( "TITLE" );
			str.RenderAsHtml( string.Format( "{0}//Movies-{1}.htm", OutputDirectory, Inits ), true );
		}

		public void RenderTvAsHtml()
		{
			ReportType = "TV Show Report";
			var str = new SimpleTableReport( string.Format( "{1} {0}", Inits, ReportType ) );
			str.AddStyle(
				"#container { text-align: left; background-color: #ccc; margin: 0 auto; border: 1px solid #545454; width: 641px; padding:10px; font: 13px/19px Trebuchet MS, Georgia, Times New Roman, serif; }" );
			str.AddStyle( "#main { margin-left:1em; }" );
			str.AddStyle( "#dtStamp { font-size:0.8em; }" );
			str.AddStyle( ".end { clear: both; }" );
			str.AddStyle( ".gponame { color:white; background:black }" );
			str.ColumnHeadings = true;
			str.DoRowNumbers = true;
			str.ShowElapsedTime = false;
			str.IsFooter = false;
			str.AddColumn( new ReportColumn( "Show", "TITLE", "{0}", typeof (String) ) );
			str.AddColumn( new ReportColumn( "Format", "FORMAT", "{0}", typeof (String) ) );
			str.AddColumn( new ReportColumn( "Date Added", "ADDED", "{0}", typeof (String) ) );
			BuildTable( str, "TV", false );
			str.SetSortOrder( "TITLE" );
			str.RenderAsHtml( string.Format( "{0}//TV-{1}.htm", OutputDirectory, Inits ), true );
		}

		public void RenderLatestFilesAsHtml()
		{
			ReportType = "Added since";
			var ts = new TimeSpan( DaysBack, 0, 0, 0 );
			CutoffDate = DateTime.Now.Subtract( ts );
			var str = new SimpleTableReport( string.Format( "{2} {0} ({1} days ago)",
			                                                CutoffDate.ToShortDateString(), DaysBack, ReportType ) );
			str.AddStyle(
				"#container { text-align: left; background-color: #ccc; margin: 0 auto; border: 1px solid #545454; width: 641px; padding:10px; font: 13px/19px Trebuchet MS, Georgia, Times New Roman, serif; }" );
			str.AddStyle( "#main { margin-left:1em; }" );
			str.AddStyle( "#dtStamp { font-size:0.8em; }" );
			str.AddStyle( ".end { clear: both; }" );
			str.AddStyle( ".gponame { color:white; background:black }" );
			str.ColumnHeadings = true;
			str.DoRowNumbers = true;
			str.ShowElapsedTime = false;
			str.IsFooter = false;
			str.AddColumn( new ReportColumn( "Title", "TITLE", "{0}", typeof (String) ) );
			str.AddColumn( new ReportColumn( "Episode", "EPISODE", "{0}", typeof (String) ) );
			str.AddColumn( new ReportColumn( "Format", "FORMAT", "{0}", typeof (String) ) );
			str.AddColumn( new ReportColumn( "Date Added", "ADDED", "{0}", typeof (String) ) );
			BuildTable( str, "TV", true );
			str.SetSortOrder( "ADDED DESC" );
			str.RenderAsHtml( string.Format( "{0}//Latest-{1}.htm", OutputDirectory, Inits ), true );
		}

		private void BuildTable( SimpleTableReport str, string type, [Optional] bool latest )
		{
			FindMedia( latest );

			if (MediaList != null)
			{
				foreach ( var i in MediaList )
				{
					if (i == null) continue;

					if (!i.Type.Equals( type ) && !latest) continue;

					var dr = str.Body.NewRow();
					dr[ "TITLE" ] = i.Title;
					dr[ "FORMAT" ] = i.Format;
					dr[ "ADDED" ] = i.LibraryDate.ToShortDateString();
					if (type.Equals( "TV" ) && latest)
						dr[ "EPISODE" ] = i.Episode;
					str.Body.Rows.Add( dr );
				}
			}
			return;
		}

		private void FindMedia( bool latest )
		{
			MediaList.Clear();
			foreach ( var f in FolderList )
				LoadFiles( f.Name, latest );
		}

		public void LoadFiles( string directory, bool latest )
		{
			if (Directory.Exists( directory ))
				ProcessDirectory( directory, latest );
			else
				Utility.Announce( string.Format( "{0} is not a valid directory.", directory ) );
		}

		// Process all files in the directory passed in, recurseon any directories 
		// that are found, and process the files they contain.
		public void ProcessDirectory( string targetDirectory, bool latest )
		{
			GetFiles( targetDirectory, "avi", latest );
			GetFiles( targetDirectory, "mkv", latest );
			GetFiles( targetDirectory, "VOB", latest );
			GetFiles( targetDirectory, "mp4", latest );
		}

		private void GetFiles( string targetDirectory, string extension, bool latest )
		{
			Utility.Announce( string.Format( "Processing {1} files in {0}...", targetDirectory, extension ) );
			// Process the list of files found in the directory.
			var fileEntries = Directory.GetFiles( targetDirectory, "*." + extension, SearchOption.AllDirectories );
#if DEBUG
         Logger.Info( string.Format( "{0} entries with extension {1}", fileEntries.Count(), extension ) );
#endif
         foreach ( var fileName in fileEntries )
				ProcessFile( fileName, targetDirectory, extension, latest );
		}

		public void ProcessFile( string path, string parentDirectory, string extension, bool latest )
		{
			var fi = new FileInfo( path );

			if (!fi.Extension.Equals( "." + extension )) return;

			var action = "";
			var mi = new MediaItem {Filename = fi.Name};

			if (!IsValid( fi, extension )) return;

			var addIt = true;
			if (fi.Directory != null) mi.Type = InferType( fi.Directory.FullName );
			mi.LibraryDate = DetermineDate( fi, mi.Type, extension );
			mi.Format = extension;

			if (mi.Type.Equals( "Movie" ))
			{
				mi.Episode = "movie";
				//  for VOBs title is parents parent directory
				if (extension.Equals( "VOB" ))
				{
					if (fi.Directory != null)
					{
						var dir = new FileInfo( fi.Directory.FullName );
						if (dir.Directory != null) mi.Title = dir.Directory.Name;
					}
				}
				else
					//  avi or MKV
					if (fi.Directory != null) mi.Title = fi.Directory.Name;
			}
			if (mi.Type.Equals( "TV" ))
			{
				//  set title from parent directory
				if (fi.Directory != null)
				{
					var parent = new FileInfo( fi.Directory.FullName );
					if (parent.Directory != null) mi.Title = parent.Directory.Name;
				}
				if (mi.Title.Equals( "TV" ))
					Logger.Error( "No season directory for " + fi.Name );
				else
				{
					var fileNameWithoutExtension = Path.GetFileNameWithoutExtension( fi.Name );
					if (fileNameWithoutExtension != null)
						mi.Episode = fileNameWithoutExtension.Replace( '.', ' ' );
					AddMedia( mi, latest );
					AddToLibrary( mi );
				}
				addIt = false;
				action = "added";
			}

			if (addIt)
			{
				action = "added";
				AddMedia( mi, latest );
				if (!latest) AddToLibrary( mi ); //  xml

				Utility.Announce( string.Format( "Adding {0} from {1}", fi.Name, fi.DirectoryName ) );
			}
			action += " as " + mi.Title;
#if DEBUG
         if ( latest )
				Logger.Info( string.Format( "File->{0,-50} - {1}", fi.FullName, action ) );
         //else
         //   action = "invalid";
#endif
      }

		private void AddMedia( MediaItem mi, bool latest )
		{
			if (latest)
			{
				var xmlItem = Master.GetMedia( mi ); //  filename is the key
				//  compare dates
				var libDate = xmlItem.LibraryDate;
				if (libDate < CutoffDate)
				{
#if DEBUG
               Logger.Info( string.Format( "    {0} is old (library date {1})", mi.Filename, libDate.ToShortDateString() ) );
#endif
               return;
				}
#if DEBUG
            Logger.Info( string.Format( "    {0} is new - included in the latest", mi.Filename ) );
#endif
         }

			if (ReportType.Equals( "TV Show Report" ))
			{
				if (!ListHasTitle( mi.Title ))
				{
					LogIt( mi );
					MediaList.Add( mi );
				}
			}
			else
			{
				if (!ListHas( mi.Filename ))
				{
					LogIt( mi );
					MediaList.Add( mi );
				}
			}
		}

		[Conditional("DEBUG")]
		private void LogIt( MediaItem mi )
		{
			Logger.Info( string.Format( "  Adding {0,-60} - format:{1,-6}, type:{3,-6}, title:{2,-30}, dated:{4}",
			                            mi.Filename, mi.Format, mi.Title, mi.Type, mi.LibraryDate.ToShortDateString() ) );
		}

		private bool ListHasTitle( string title )
		{
			return MediaList.Any( mi => mi.Title.Equals( title ) );
		}

		private bool ListHas( string fileName )
		{
			return MediaList.Any( mi => mi.Filename.Equals( fileName ) );
		}

		private static bool IsValid( FileSystemInfo fi, string extension )
		{
			var isValid = true;
			var filename = Path.GetFileNameWithoutExtension( fi.Name );

			if (extension.Equals( "VOB" ))
			{
				if (filename != null && !filename.Equals( "VTS_01_0" ))
					isValid = false;
			}
			else
			{
				if ( filename != null && filename.ToUpper().IndexOf( "SAMPLE" ) > -1 ) isValid = false;
				if ( filename != null && filename.ToUpper().IndexOf( "CD2" ) > -1 ) isValid = false;
				if ( filename != null && filename.ToUpper().IndexOf( "CD 2" ) > -1 ) isValid = false;
			}
			return isValid;
		}

		private void AddToLibrary( MediaItem mi )
		{
			Master.PutMedia( mi );
		}

		private DateTime DetermineDate( FileInfo fi, string type, string ext )
		{
			var theDate = DateTime.Now;
			var key = InferKey( fi, type, ext );
			if (Master.HaveMedia( key ))
			{
				var mi = new MediaItem {Filename = key};
				mi = Master.GetMedia( mi );
				theDate = mi.LibraryDate;
			}
			return theDate;
		}

		private string InferKey( FileInfo fi, string type, string ext )
		{
			//  key is the filename
			var key = "";
			if (type.Equals( "Movie" ))
			{
				if (ext.Equals( "VOB" ))
				{
					if (fi.Directory != null)
					{
						var dir = new FileInfo( fi.Directory.FullName );
						if (dir.Directory != null) key = dir.Directory.Name;
					}
				}
				else if (fi.Directory != null) key = fi.Directory.Name;
			}
			if (type.Equals( "TV" ))
			{
				if (ext.Equals( "VOB" ))
				{
					if (fi.Directory != null)
					{
						var dir = new FileInfo( fi.Directory.FullName );
						if (dir.Directory != null) key = dir.Directory.Name;
					}
				}
				else
					key = fi.Name;
			}
			Logger.Info( string.Format( "  Key- for {0,-50} is {1}", fi.Name, key ) );
			return key;
		}

		private static string InferType( string path )
		{
			var mediaType = "???";

			if (path.ToUpper().IndexOf( "MOVIE" ) > -1)
			{
				mediaType = "Movie";
			}
			else if (path.ToUpper().IndexOf( "TV" ) > -1)
			{
				mediaType = "TV";
			}
			return mediaType;
		}

      public void Configure(System.Collections.Specialized.NameValueCollection appSettings )
      {
         Inits = "LSC";

         //  load up all the media folders

         // Get the collection enumerator.
         var appSettingsEnum = appSettings.Keys.GetEnumerator();

         // Loop through the collection and
         // display the appSettings key, value pairs.
         var i = 0;
         while (appSettingsEnum.MoveNext())
         {
            var key = appSettings.Keys[i];
            if (key.StartsWith("MediaFolder"))
            {
               FolderList.Add(new Models.FolderItem { Name = appSettings[key] });
               Utility.Announce(string.Format("Adding Media Folder {0}", appSettings[key]));
            }
            else if (key.StartsWith("OutputDirectory"))
            {
               OutputDirectory = appSettings[key];
            }
            else if (key.StartsWith("XmlDirectory"))
            {
               XmlDirectory = appSettings[key];
            }
            else if (key.StartsWith("Inits"))
            {
               Inits = appSettings[key];
            }
            else if (key.StartsWith("DaysBack"))
            {
               DaysBack = Int32.Parse(appSettings[key]);
            }
            else if (key.StartsWith("Report-Latest"))
            {
               var latest = appSettings[key];
               if (latest.Equals("Yes"))
                  DoLatest = true;
            }
            else if (key.StartsWith("Report-Movies"))
            {
               var movies = appSettings[key];
               if (movies.Equals("Yes"))
                  DoMovies = true;
            }
            else if (key.StartsWith("Report-TV"))
            {
               var tv = appSettings[key];
               if (tv.Equals("Yes"))
                  DoTv = true;
            }
            i++;
         }
         var mm = new MediaMaster("Media", XmlDirectory, "media.xml", Logger);
         Master = mm;
      }

      public void DumpXml()
      {
         Master.Dump2Xml();
      }
   }
}