using Helpers;
using MediaWatch.Logging;
using MediaWatch.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

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
        public bool DoBooks { get; set; }
        public string ReportType { get; set; }

        public MediaWatcher()
        {
            FolderList = new List<FolderItem>();
            MediaList = new List<MediaItem>();
            Logger = new NLogger();
            Logger.Info("==============================================================================================");
            Logger.Info("Media Watcher ver. 3.1805.30.1");
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
            if (DoBooks)
            {
                Logger.Info("--- Books --------------------------------------------------------------------------------");
                RenderBooksAsHtml("IT");
                RenderBooksAsHtml("Inspire");
                RenderBooksAsHtml("Entrepreneurial");
            }
            if (DoLatest)
            {
                Logger.Info("---Latest--------------------------------------------------------------------------------");
                RenderLatestFilesAsHtml();
            }
            Logger.Info("== Execution Complete =======================================================================");
        }

        public void RenderMoviesAsHtml()
        {
            ReportType = "Movie Report";

            var str = new SimpleTableReport(header: $"{ReportType} {Inits}");
            str.AddStyle(
                "#container { text-align: left; background-color: #ccc; margin: 0 auto; border: 1px solid #545454; width: 641px; padding:10px; font: 13px/19px Trebuchet MS, Georgia, Times New Roman, serif; }");
            str.AddStyle("#main { margin-left:1em; }");
            str.AddStyle("#dtStamp { font-size:0.8em; }");
            str.AddStyle(".end { clear: both; }");
            str.AddStyle(".gponame { color:white; background:black }");
            str.ColumnHeadings = true;
            str.DoRowNumbers = true;
            str.ShowElapsedTime = false;
            str.IsFooter = false;
            str.AddColumn(new ReportColumn("Title", "TITLE", "{0}", typeof(String)));
            str.AddColumn(new ReportColumn("Format", "FORMAT", "{0}", typeof(String)));
            str.AddColumn(new ReportColumn("Date Added", "ADDED", "{0}", typeof(String)));
            BuildTable(
                str,
                type: "Movie",
                subType: string.Empty,
                latest: false);
            str.SetSortOrder("TITLE");
            str.RenderAsHtml(string.Format("{0}//Movies-{1}.htm", OutputDirectory, Inits), true);
        }

        public void RenderTvAsHtml()
        {
            ReportType = "TV Show Report";
            var str = new SimpleTableReport(header: $"{ReportType} {Inits}");
            str.AddStyle(
                "#container { text-align: left; background-color: #ccc; margin: 0 auto; border: 1px solid #545454; width: 641px; padding:10px; font: 13px/19px Trebuchet MS, Georgia, Times New Roman, serif; }");
            str.AddStyle("#main { margin-left:1em; }");
            str.AddStyle("#dtStamp { font-size:0.8em; }");
            str.AddStyle(".end { clear: both; }");
            str.AddStyle(".gponame { color:white; background:black }");
            str.ColumnHeadings = true;
            str.DoRowNumbers = true;
            str.ShowElapsedTime = false;
            str.IsFooter = false;
            str.AddColumn(new ReportColumn("Show", "TITLE", "{0}", typeof(String)));
            str.AddColumn(new ReportColumn("Format", "FORMAT", "{0}", typeof(String)));
            str.AddColumn(new ReportColumn("Date Added", "ADDED", "{0}", typeof(String)));
            BuildTable(
                str,
                type: "TV",
                subType: string.Empty,
                latest: false);
            str.SetSortOrder("TITLE");
            str.RenderAsHtml($"{OutputDirectory}//TV-{Inits}.htm", true);
        }

        public void RenderBooksAsHtml(string bookSubType)
        {
            ReportType = $"{bookSubType} Book Report";
            var str = new SimpleTableReport(header: $"{ReportType} {Inits}");
            str.AddStyle(
                "#container { text-align: left; background-color: #ccc; margin: 0 auto; border: 1px solid #545454; width: 1400px; padding:10px; font: 13px/19px Trebuchet MS, Georgia, Times New Roman, serif; }");
            str.AddStyle("#main { margin-left:1em; }");
            str.AddStyle("#dtStamp { font-size:0.8em; }");
            str.AddStyle(".end { clear: both; }");
            str.AddStyle(".gponame { color:white; background:black }");
            str.ColumnHeadings = true;
            str.DoRowNumbers = true;
            str.ShowElapsedTime = false;
            str.IsFooter = false;
            str.AddColumn(new ReportColumn("Category", "CATEGORY", "{0}", typeof(String)));
            str.AddColumn(new ReportColumn("Title", "TITLE", "{0}", typeof(String)));
            str.AddColumn(new ReportColumn("Format", "FORMAT", "{0}", typeof(String)));
            str.AddColumn(new ReportColumn("Date Added", "ADDED", "{0}", typeof(String)));
            BuildTable(
                str: str,
                type: "Books",
                subType: bookSubType,
                latest: false);
            str.SetSortOrder("CATEGORY,TITLE");
            str.RenderAsHtml($"{OutputDirectory}//{bookSubType}-Books-{Inits}.htm", true);
        }

        public void RenderLatestFilesAsHtml()
        {
            ReportType = "Added since";
            var ts = new TimeSpan(DaysBack, 0, 0, 0);
            CutoffDate = DateTime.Now.Subtract(ts);
            var str = new SimpleTableReport(string.Format("{2} {0} ({1} days ago)",
                                                            CutoffDate.ToShortDateString(), DaysBack, ReportType));
            str.AddStyle(
                "#container { text-align: left; background-color: #ccc; margin: 0 auto; border: 1px solid #545454; width: 641px; padding:10px; font: 13px/19px Trebuchet MS, Georgia, Times New Roman, serif; }");
            str.AddStyle("#main { margin-left:1em; }");
            str.AddStyle("#dtStamp { font-size:0.8em; }");
            str.AddStyle(".end { clear: both; }");
            str.AddStyle(".gponame { color:white; background:black }");
            str.ColumnHeadings = true;
            str.DoRowNumbers = true;
            str.ShowElapsedTime = false;
            str.IsFooter = false;
            str.AddColumn(new ReportColumn("Title", "TITLE", "{0}", typeof(String)));
            str.AddColumn(new ReportColumn("Episode", "EPISODE", "{0}", typeof(String)));
            str.AddColumn(new ReportColumn("Format", "FORMAT", "{0}", typeof(String)));
            str.AddColumn(new ReportColumn("Date Added", "ADDED", "{0}", typeof(DateTime)));
            BuildTable(
                str,
                type: "TV",
                subType: string.Empty,
                latest: true);
            str.SetSortOrder("ADDED DESC");
            str.RenderAsHtml(string.Format("{0}//Latest-{1}.htm", OutputDirectory, Inits), true);
        }

        private void BuildTable(
            SimpleTableReport str, 
            string type, 
            string subType,
            [Optional] bool latest)
        {
            FindMedia(latest,type,subType);

            if (MediaList != null)
            {
                foreach (var i in MediaList)
                {
                    if (i == null) continue;

                    if (!i.Type.Equals(type) && !latest) continue;

                    var dr = str.Body.NewRow();
                    dr["TITLE"] = i.Title;
                    dr["FORMAT"] = i.Format;
                    dr["ADDED"] = i.LibraryDate.ToShortDateString();
                    if (type.Equals("TV") && latest)
                        dr["EPISODE"] = i.Episode;
                    if (type.Equals("Books"))
                        dr["CATEGORY"] = i.Episode;
                    str.Body.Rows.Add(dr);
                }
            }
            return;
        }

        private void FindMedia(
            bool latest,
            string mediaType,
            string subType )
        {
            MediaList.Clear();
            foreach (var f in FolderList)
                LoadFiles(f, latest, mediaType, subType);
        }

        public void LoadFiles(
            FolderItem folderItem, 
            bool latest,
            string mediaType,
            string subType)
        {
            if (!folderItem.Key.StartsWith($"MediaFolder-{mediaType}"))
                return;
            if ( !string.IsNullOrEmpty(subType))
            {
                if (!folderItem.Key.StartsWith($"MediaFolder-{mediaType}-{subType}"))
                    return;
            }

            if (Directory.Exists(folderItem.Name))
            {
                Announce($"Processing folder {folderItem}");

                ProcessDirectory(
                    folderItem.Name,
                    latest,
                    mediaType);
            }
            else
                Announce($"{folderItem.Name} is not a valid directory.");
        }

        // Process all files in the directory passed in, recurseon any directories
        // that are found, and process the files they contain.
        public void ProcessDirectory(
            string targetDirectory, 
            bool latest,
            string mediaType)
        {
            switch (mediaType)
            {
                case "Books" :
                    GetFiles(targetDirectory, "pdf", latest);
                    GetFiles(targetDirectory, "rtf", latest);
                    GetFiles(targetDirectory, "mobi", latest);
                    GetFiles(targetDirectory, "epub", latest);
                    GetFiles(targetDirectory, "txt", latest);
                    GetFiles(targetDirectory, "doc", latest);
                    GetFiles(targetDirectory, "chm", latest);
                    break;

                default:
                    GetFiles(targetDirectory, "avi", latest);
                    GetFiles(targetDirectory, "mkv", latest);
                    GetFiles(targetDirectory, "VOB", latest);
                    GetFiles(targetDirectory, "mp4", latest);
                    break;
            }
        }

        private void GetFiles(
            string targetDirectory, 
            string extension, 
            bool latest)
        {
            Trace($"Processing {extension} files in {targetDirectory}...");
            // Process the list of files found in the directory.
            var fileEntries = Directory.GetFiles(
                targetDirectory,
                "*." + extension, 
                SearchOption.AllDirectories);
#if DEBUG
            Trace($"{fileEntries.Count()} entries with extension {extension}" );
#endif
            foreach (var fileName in fileEntries)
                ProcessFile(
                    fileName, 
                    targetDirectory, 
                    extension, 
                    latest);
        }

        public void ProcessFile(
            string path, 
            string parentDirectory, 
            string extension, 
            bool latest)
        {
            var fi = new FileInfo(path);

            if (!fi.Extension.Equals("." + extension))
                return;

            var properName = FixFile(fi,extension);  //  renames bodgy pdf titles

            var action = "";
            var mi = new MediaItem
            {
                Filename = properName
            };

            if (!IsValid(fi, extension))
                return;

            var addIt = true;
            if (fi.Directory != null)
                mi.Type = InferType(fi.Directory.FullName);

            mi.LibraryDate = DetermineDate(
                properName, 
                fi.DirectoryName,
                mi.Type, 
                extension);

            mi.Format = extension;

            if (mi.Type.Equals("Movie"))
            {
                mi.Episode = "movie";
                //  for VOBs title is parents parent directory
                if (extension.Equals("VOB"))
                {
                    if (fi.Directory != null)
                    {
                        var dir = new FileInfo(fi.Directory.FullName);
                        if (dir.Directory != null) mi.Title = dir.Directory.Name;
                    }
                }
                else
                    //  avi or MKV
                    if (fi.Directory != null) mi.Title = fi.Directory.Name;
            }
            if (mi.Type.Equals("TV"))
            {
                //  set title from parent directory
                if (fi.Directory != null)
                {
                    var parent = new FileInfo(fi.Directory.FullName);
                    if (parent.Directory != null) mi.Title = parent.Directory.Name;
                }
                if (mi.Title.Equals("TV"))
                    Logger.Error("No season directory for " + fi.Name);
                else
                {
                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fi.Name);
                    if (fileNameWithoutExtension != null)
                        mi.Episode = fileNameWithoutExtension.Replace('.', ' ');
                    AddMedia(mi, latest);
                    AddToLibrary(mi);
                }
                addIt = false;
                action = "added";
            }
            if (mi.Type.Equals("Books"))
            {
                //  set category from parent directory
                if (fi.Directory != null)
                {
                    mi.Episode = fi.Directory.Name;
                }
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(properName);
                if (fileNameWithoutExtension != null)
                    mi.Title = fileNameWithoutExtension.Replace('.', ' ');
                AddMedia(mi, latest);
                AddToLibrary(mi);

                addIt = false;
                action = "added";
            }
            if (addIt)
            {
                action = "added";
                AddMedia(mi, latest);
                if (!latest) AddToLibrary(mi); //  xml

                Trace($"Adding {fi.Name} from {fi.DirectoryName}");
            }
            action += " as " + mi.Title;
#if DEBUG
         if ( latest )
				Trace( $"File->{fi.FullName,-50} - {action}" );
         //else
         //   action = "invalid";
#endif
        }

        private string FixFile(FileInfo fi, string extension)
        {
            var properName = fi.Name;

            if (extension != "pdf")
                return properName;

            //  looking for first word that matches first nine characters are digits
            var word = fi.Name.Split(' ');
            var input = word[0];
            Regex rgx = new Regex(@"^\d{9}");
            var matches = rgx.IsMatch(input);
            if ( matches )
            {
                var newTitle = string.Empty;
                for (int i = 2; i < word.Length; i++)
                {
                    newTitle += word[i] + " ";
                }
                if (string.IsNullOrEmpty(newTitle))
                    return properName;

                var newFileName = fi.DirectoryName + "\\" + newTitle;
                if (File.Exists(newFileName))
                {
                    // delete it rather than rename it
                    Announce($"Deleteing:-{fi.FullName}");
                    File.Delete(fi.FullName);
                }
                else
                {
                    Announce($"Renaming {properName} to {newFileName}");
                    File.Move(fi.FullName, newFileName);
                }
                properName = newTitle;
            }
            return properName;
        }

        private void Announce(string msg)
        {
            Logger.Info(msg);
            Console.WriteLine(msg);
        }

        private void Trace(string msg)
        {
            Console.WriteLine(msg);
        }

        private void AddMedia(
            MediaItem mi, 
            bool latest)
        {
            if (latest)
            {
                var xmlItem = Master.GetMedia(mi); //  filename is the key
                                                   //  compare dates
                var libDate = xmlItem.LibraryDate;
                if (libDate < CutoffDate)
                {
#if DEBUG
                    Announce( $"    {mi.Filename} is old (library date {libDate.ToShortDateString()})" );
#endif
                    return;
                }
#if DEBUG
                Announce( $"    {mi.Filename} is new - included in the latest" );
#endif
            }

            if (ReportType.Equals("TV Show Report"))
            {
                if (!ListHasTitle(mi.Title))
                {
                    LogIt(mi);
                    MediaList.Add(mi);
                }
            }
            else
            {
                if (!ListHas(mi.Filename))
                {
                    LogIt(mi);
                    MediaList.Add(mi);
                }
            }
        }

        [Conditional("DEBUG")]
        private void LogIt(MediaItem mi)
        {
            Trace($@"  Adding {
                mi.Filename,-60
                } - format:{
                mi.Format,-6
                }, type:{
                mi.Type,-6
                }, title:{
                mi.Title,-30
                }, dated:{
                mi.LibraryDate.ToShortDateString()
                }");
        }

        private bool ListHasTitle(string title)
        {
            return MediaList.Any(mi => mi.Title.Equals(title));
        }

        private bool ListHas(string fileName)
        {
            return MediaList.Any(mi => mi.Filename.Equals(fileName));
        }

        private static bool IsValid(FileSystemInfo fi, string extension)
        {
            var isValid = true;
            var filename = Path.GetFileNameWithoutExtension(fi.Name);

            if (extension.Equals("VOB"))
            {
                if (filename != null && !filename.Equals("VTS_01_0"))
                    isValid = false;
            }
            else
            {
                if (filename != null 
                    && filename.ToUpper().IndexOf("SAMPLE") > -1)
                    isValid = false;

                if (filename != null 
                    && filename.ToUpper().IndexOf("CD2") > -1)
                    isValid = false;

                if (filename != null 
                    && filename.ToUpper().IndexOf("CD 2") > -1)
                    isValid = false;
            }
            return isValid;
        }

        private void AddToLibrary(MediaItem mi)
        {
            Master.PutMedia(mi);
        }

        private DateTime DetermineDate(
            string fileName, 
            string folder,
            string type, 
            string ext)
        {
            var theDate = DateTime.Now;
            var key = InferKey(
                fileName, 
                folder,
                type, 
                ext);

            if (Master.HaveMedia(key))
            {
                var mi = new MediaItem
                {
                    Filename = key
                };
                mi = Master.GetMedia(mi);
                theDate = mi.LibraryDate;
            }
            return theDate;
        }

        private string InferKey(
            string fileName, 
            string folder, 
            string type, 
            string ext)
        {
            //  key is the filename
            var key = fileName;
            if (type.Equals("Movie"))
            {
                if (ext.Equals("VOB"))
                {
                    if (folder != null)
                    {
                        var dir = new FileInfo(folder);
                        if (dir.Directory != null)
                            key = dir.Directory.Name;
                    }
                }
                else if (folder != null)
                    key = folder;
            }
            if (type.Equals("TV"))
            {
                if (ext.Equals("VOB"))
                {
                    if (folder != null)
                    {
                        var dir = new FileInfo(folder);
                        if (dir.Directory != null)
                            key = dir.Directory.Name;
                    }
                }
                else
                    key = fileName;
            }
            Trace($"  Key- for {fileName,-50} is {key}");
            return key;
        }

        private static string InferType(string path)
        {
            var mediaType = "Books";

            if (path.ToUpper().IndexOf("MOVIE") > -1)
            {
                mediaType = "Movie";
            }
            else if (path.ToUpper().IndexOf("TV") > -1)
            {
                mediaType = "TV";
            }
            return mediaType;
        }

        public void Configure(
            System.Collections.Specialized.NameValueCollection appSettings)
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
                if (key.StartsWith($"MediaFolder"))
                {
                    FolderList.Add(
                        new FolderItem
                        {
                            Key = key,
                            Name = appSettings[key]
                        });
                    Announce($"Adding Media Folder {key} -> {appSettings[key]}");
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
                else if (key.StartsWith("Report-Books"))
                {
                    var books = appSettings[key];
                    if (books.Equals("Yes"))
                        DoBooks = true;
                }
                i++;
            }
            var mm = new MediaMaster(
                name: "Media",
                dirName: XmlDirectory,
                fileName: "media.xml",
                logger: Logger);

            Master = mm;
        }

        public void DumpXml()
        {
            Master.Dump2Xml();
        }
    }
}