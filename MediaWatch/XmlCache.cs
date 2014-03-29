using System.Xml;
using System.Xml.XPath;
using System.Collections;

namespace MediaWatch
{
	public class XmlCache : ICache
	{
		protected XPathDocument epXmlDoc;
		protected XPathNavigator nav;

		private XmlDocument xmlDoc;

		private Hashtable theHT;

		private string filename;

		private int cacheHits = 0;
		private int cacheMisses = 0;

		private bool isDirty = false;

		private string name;

		public XmlCache( string entityName )
		{
			//RosterLib.Utility.Announce(string.Format("XmlCache.Init Constructing {0} master", entityName ) );

			Name = entityName;
			TheHT = new Hashtable();
		}

		public int CacheHits
		{
			get { return cacheHits; }
			set { cacheHits = value; }
		}

		public int CacheMisses
		{
			get { return cacheMisses; }
			set { cacheMisses = value; }
		}

		public bool IsDirty
		{
			get { return isDirty; }
			set { isDirty = value; }
		}

		public XmlDocument XmlDoc
		{
			get { return xmlDoc; }
			set { xmlDoc = value; }
		}

		public Hashtable TheHT
		{
			get { return theHT; }
			set { theHT = value; }
		}

		public string Filename
		{
			get { return filename; }
			set { filename = value; }
		}

		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		public string StatsMessage()
		{
			return string.Format( "{2} Cache hits {0} misses {1}", CacheHits, CacheMisses, Name );
		}

		public static void WriteElement( XmlTextWriter writer, string name, string text )
		{
			writer.WriteStartElement( name );
			writer.WriteString( text );
			writer.WriteEndElement();
		}

		public void DumpHt()
		{
			IDictionaryEnumerator myEnumerator = TheHT.GetEnumerator();
			int i = 0;
			Utility.Announce( "\t-INDEX-\t-KEY-\t-VALUE-" );
			while ( myEnumerator.MoveNext() )
				Utility.Announce( string.Format( "\t[{0}]:\t{1}\t{2}", i++, myEnumerator.Key, myEnumerator.Value ) );
		}
	}
}
