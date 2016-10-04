using System;
using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using MediaWatch.Models;
using Helpers;

namespace MediaWatch
{
	public class MediaMaster : Helpers.XmlCache
	{
      ILog Logger;

		#region  Constructors

		public MediaMaster( string name, string dirName, string fileName, ILog logger )
			: base( name )
		{
         Logger = logger;
			Filename = dirName + fileName;
			try
			{
				//  load HT from the xml
				XmlDoc = new XmlDocument();
				XmlDoc.Load( dirName + fileName );
				XmlNode listNode = XmlDoc.ChildNodes[ 2 ];
				foreach ( XmlNode node in listNode.ChildNodes )
					AddXmlMedia( node );

				//  Create the XML navigation objects to allow xpath queries
				epXmlDoc = new XPathDocument( fileName );
				// bad implementation - does not throw an exeception if XML is invalid
				Utility.Announce( string.Format( "{0} loaded OK!", fileName ) );
				nav = epXmlDoc.CreateNavigator();

				DumpHt();
				DumpMedia();
				Utility.Announce( string.Format( "MediaMaster constructed : {0}-{1}", name, dirName + fileName ) );

			}
			catch ( IOException e )
			{
				Utility.Announce( string.Format( "Unable to open {1} xmlfile - {0}", e.Message, dirName + fileName ) );
			}
		}

		#endregion

		#region  Reading

		public MediaItem GetMedia( MediaItem mi )
		{
			MediaItem m;
			if ( TheHT.ContainsKey( mi.Filename ) )
			{
            m = (MediaItem)TheHT[mi.Filename];
				CacheHits++;
			}
			else
			{
				//  new it up
				m = new MediaItem();
            m.Filename = mi.Filename;
            m.Title = mi.Title;
				m.LibraryDate = mi.LibraryDate;
				m.Episode = mi.Episode;
            m.Type = mi.Type;
            m.Format = mi.Format;
				PutMedia( m );
				CacheMisses++;
			}
			return m;
		}

		public bool HaveMedia( string filename )
		{
			return TheHT.ContainsKey( filename );
		}


		#endregion

		#region  Writing

		public void PutMedia( MediaItem m )
		{
         if ( m.Filename.Equals( "VTS_01_0.VOB" ) )
            m.Filename = m.Title;

         if ( !TheHT.ContainsKey( m.Filename ) )
         {
            TheHT.Add( m.Filename, m );
            IsDirty = true;
         }
         else
         {
            Logger.Info( string.Format( "    {0} already in XML", m.Filename ) );
         }
		}

		private void AddXmlMedia( XmlNode node )
		{
			AddMediaItem( new MediaItem( node ) );
		}

		public void AddMediaItem( MediaItem m )
		{
			TheHT.Add( m.Filename, m );
		}

		#endregion

		#region  Persistence

		/// <summary>
		///   Converts the memory hash table to XML
		/// </summary>
		public void Dump2Xml()
		{
			if ( ( TheHT.Count > 0 ) && IsDirty )
			{
				XmlTextWriter writer = new
						  XmlTextWriter( string.Format( "{0}", Filename ), null );

				writer.WriteStartDocument();
				writer.WriteComment( "Comments: " + Name );
				writer.WriteStartElement( "media-list" );

				IDictionaryEnumerator myEnumerator = TheHT.GetEnumerator();
				while ( myEnumerator.MoveNext() )
				{
					MediaItem m = (MediaItem) myEnumerator.Value;
					WriteMediaNode( writer, m );
				}
				writer.WriteEndElement();
				writer.WriteEndDocument();
				writer.Close();
				Utility.Announce( string.Format( "{0} created.",  Filename ) );
			}
			else
				Utility.Announce( string.Format( "No changes to {0}.", Filename ) );

		}

		private static	void WriteMediaNode( XmlTextWriter writer, MediaItem m )
		{
			writer.WriteStartElement( "media-item" );
			WriteElement( writer, "filename", m.Filename );
			WriteElement( writer, "type", m.Type );
			WriteElement( writer, "title", m.Title );
			WriteElement( writer, "episode", m.Episode );
			WriteElement( writer, "libdate", m.LibraryDate.ToShortDateString() );
			writer.WriteEndElement();
		}

		#endregion

		#region  Logging

		public void DumpMedia()
		{
			IDictionaryEnumerator myEnumerator = TheHT.GetEnumerator();
			while ( myEnumerator.MoveNext() )
			{
				MediaItem s = (MediaItem) myEnumerator.Value;
				Utility.Announce( string.Format( "Season {0}:- ", myEnumerator.Key ) );
			}
		}

		#endregion
	}
}
