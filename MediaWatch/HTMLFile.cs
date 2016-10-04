using Helpers;
using System;
using System.Collections;
using System.IO;

namespace MediaWatch
{
		/// <summary>
		/// Summary description for HTMLFile.
		/// </summary>
		public class HTMLFile
		{
			private readonly string filename;
			private readonly string title;
			private readonly string header;
			private ArrayList styleList;
			private readonly string cssFile;
			private readonly string script1;
			private readonly string script2;
			private readonly ArrayList bodyList;

			public HTMLFile( string filenameIn, string titleIn )
			{
				filename = filenameIn;
				title = titleIn;
				header = String.Empty;
				bodyList = new ArrayList();
				StyleList = new ArrayList();
			}

			public HTMLFile( string filenameIn, string titleIn, string headerIn,
								  string cssFileIn, string script1In, string script2In )
			{
				filename = filenameIn;
				title = titleIn;
				header = headerIn;
				cssFile = cssFileIn;
				script1 = script1In;
				script2 = script2In;
				bodyList = new ArrayList();
				StyleList = new ArrayList();
			}

			public ArrayList StyleList
			{
				get { return styleList; }
				set { styleList = value; }
			}

			public void AddToBody( string strBody )
			{
				bodyList.Add( strBody );
			}


			public void Render()
			{
				StreamWriter sw = new StreamWriter( @filename, false );
				if ( header.Length > 0 )
					sw.WriteLine( HtmlLib.HTMLOpenPlus( header ) );
				else
					sw.WriteLine( HtmlLib.HTMLOpen() );
				sw.WriteLine( HtmlLib.HeadOpen() );
				sw.WriteLine( "\t" + HtmlLib.HTMLTitle( title ) );
				if ( cssFile != null )
				{
					if ( cssFile.Length > 0 )
						sw.WriteLine( "\t" + HtmlLib.CSSLink( cssFile ) );
				}
				if ( script1 != null )
				{
					if ( script1.Length > 0 )
						sw.WriteLine( "\t" + HtmlLib.VBScriptFile( script1 ) );
				}
				if ( script2 != null )
				{
					if ( script2.Length > 0 )
						sw.WriteLine( "\t" + HtmlLib.JSScriptFile( script2 ) );
				}

				if ( StyleList.Count > 0 )
				{
					IEnumerator styleEnumerator = StyleList.GetEnumerator();
					sw.WriteLine( "\t" + HtmlLib.StyleOpen() );
					while ( styleEnumerator.MoveNext() )
						sw.WriteLine( "\t" + styleEnumerator.Current );
					sw.WriteLine( "\t" + HtmlLib.StyleClose() );
				}

				sw.WriteLine( HtmlLib.HeadClose() );
				sw.WriteLine( HtmlLib.BodyOpen() );

				sw.WriteLine( HtmlLib.DivOpen( "id=\"container\"" ) );

				//  Add the body parts
				IEnumerator myEnumerator = bodyList.GetEnumerator();
				while ( myEnumerator.MoveNext() )
					sw.WriteLine( myEnumerator.Current );

				sw.WriteLine( HtmlLib.DivClose() );

				sw.WriteLine( HtmlLib.BodyClose() );
				sw.WriteLine( HtmlLib.HTMLClose() );
				sw.Close();

				Utility.Announce( string.Format( "   {0} has been rendered", filename ) );
			}

			public void AddStyle( string style )
			{
				StyleList.Add( style );
			}
		}
}
