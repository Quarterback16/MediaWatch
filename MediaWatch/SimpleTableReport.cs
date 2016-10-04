using Helpers;
using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace MediaWatch
{

		public class SimpleTableReport
		{
			private readonly string _header;

			/// <summary>
			/// DataRows to be reported on.
			/// </summary>
			private DataTable _body;

			private readonly DataColumnCollection _cols;
			private readonly string _footer;
			private readonly ArrayList _columns;

			private bool _rowNumbers;
			private bool _showElapTime = true;
			private bool _carryRow = true;
			private bool _isfooter = true;

			private string _timeTaken = "";
			private string _subHeader = "";

			private readonly ElapsedTimer _et;

			#region  CONSTRUCTORS

			public SimpleTableReport()
			{
				_columns = new ArrayList();
				_et = new ElapsedTimer();
				_et.Start( DateTime.Now );
				StyleList = new ArrayList();
				_body = new DataTable();
				_cols = Body.Columns;
			}

			/// <summary>
			///   Report with a header and a footer
			/// </summary>
			/// <param name="header"></param>
			/// <param name="footer"></param>
			public SimpleTableReport( string header, string footer )
			{
				ReportHeader = header;
				ReportFooter = footer;
				_columns = new ArrayList();
				_et = new ElapsedTimer();
				_et.Start( DateTime.Now );
				StyleList = new ArrayList();
				_body = new DataTable();
				_cols = Body.Columns;
				SubHeader = string.Empty;
			}

			/// <summary>
			///   No footer
			/// </summary>
			/// <param name="header"></param>
			public SimpleTableReport( string header )
			{
				_header = header;
            ReportHeader = header;
				_footer = String.Empty;
				_columns = new ArrayList();
				_et = new ElapsedTimer();
				_et.Start( DateTime.Now );
				StyleList = new ArrayList();
				_body = new DataTable();
				_cols = _body.Columns;
				SubHeader = string.Empty;

				Utility.Announce(string.Format("SimpleTableReport {0} created", header ));
			}

			#endregion

			#region   Accessors

			public string TimeTaken
			{
				get { return _timeTaken; }
				set { _timeTaken = value; }
			}

			public bool ShowElapsedTime
			{
				get { return _showElapTime; }
				set { _showElapTime = value; }
			}

			public bool CarryRow
			{
				get { return _carryRow; }
				set { _carryRow = value; }
			}

			public bool DoRowNumbers
			{
				get { return _rowNumbers; }
				set { _rowNumbers = value; }
			}

			public bool ColumnHeadings { get; set; }

			public string ReportHeader { get; set; }

			public string ReportFooter { get; set; }

			public bool Totals { get; set; }

			public int LastTotal { get; set; }

			public string SubHeader
			{
				get { return _subHeader; }
				set { _subHeader = value; }
			}

			public ArrayList StyleList { get; set; }

			public bool IsFooter
			{
				get { return _isfooter; }
				set { _isfooter = value; }
			}

			/// <summary>
			/// DataRows to be reported on.
			/// </summary>
			public DataTable Body
			{
				get { return _body; }
				set { _body = value; }
			}

			#endregion

			#region  Output

			#region  HTML Rendition

			/// <summary>
			///   Express the report in HTML. 
			/// </summary>
			/// <param name="fileName">The output DOS file fame.  Include a directory.</param>
			/// <param name="persist">Whether to delete the file or not, sometimes we just want the string.</param>
			public string RenderAsHtml( string fileName, bool persist )
			{
				var h = new HTMLFile( fileName, ReportHeader + " as of " + DateTime.Now.ToLongDateString() );
				AddStyles( h );
				//var html = string.Format( "<h3>{0}</h3>", ReportHeader ) + Header( _header );
            var html = Header(_header);
            if (SubHeader.Length > 0) html += SubHeader;
				html += BodyOut();
				h.AddToBody( html );
				_et.Stop( DateTime.Now );
				TimeTaken = _et.TimeOut();
				h.AddToBody( IsFooter ? ReportFooter : Footer() );
				if ( persist ) h.Render();
				return html;
			}

			private void AddStyles( HTMLFile h )
			{
				IEnumerator styleEnumerator = StyleList.GetEnumerator();
				while ( styleEnumerator.MoveNext() )
					h.AddStyle( styleEnumerator.Current.ToString() );
			}

			public void LoadBody( DataTable data )
			{
				Body = data;
			}

			public void SetFilter( string filt )
			{
				Body.DefaultView.RowFilter = filt;
			}

			public void SetSortOrder( string order )
			{
				Body.DefaultView.Sort = order;
			}

			public void AddColumn( ReportColumn c )
			{
				_columns.Add( c );

				if ( c.Type != null )
					_cols.Add( c.Source, c.Type );
			}

			public void AddStyle( string style )
			{
				StyleList.Add( style );
			}

			public string BodyOut()
			{
				int rowCount = 0;
				bool bBlank = false;
				int[] tot = new int[ _columns.Count ];
				for ( int i = 0; i < _columns.Count; i++ )
					tot[ i ] = 0;

				string sLastData = "";
				string s = "";

				if ( Body != null )
				{
					s += HtmlLib.TableOpen( "border=1 cellpadding='3' cellspacing='3'" );
					s += ColHeaders();
					//  now just add a series of rows for each record
					for ( int j = 0; j < Body.DefaultView.Count; j++ )
					{
						rowCount++;
						if ( IsEven( rowCount ) )
							s += HtmlLib.TableRowOpen( "BGCOLOR='MOCCASIN'" );
						else
							s += HtmlLib.TableRowOpen();

						if ( DoRowNumbers )
							s += HtmlLib.TableDataAttr( rowCount.ToString(), "ALIGN='RIGHT' VALIGN='TOP'" );

						//  plug in the data for each column defined
						for ( int i = 0; i < _columns.Count; i++ )
						{
							ReportColumn col = (ReportColumn) _columns[ i ];
							DataColumn dc = Body.Columns[ col.Source ];

							string sVal = Body.DefaultView[ j ][ col.Source ].ToString();
							string sData = FormatData( dc, col.Format, sVal );

							if ( col.CanAccumulate )
							{
								Totals = true;
								if ( sVal.Length > 0 ) tot[ i ] += (int) Decimal.Parse( sVal );
							}

							if ( i == 0 )
							{
								if ( sData == sLastData )
									bBlank = true;
								else
								{
									sLastData = sData;
									bBlank = false;
								}
							}
							if ( i > 5 ) bBlank = false;
							if ( bBlank && !CarryRow ) sData = " ";
							if ( col.BackGroundColourDelegateFromRole != null )
								s += HtmlLib.TableDataAttr( sData, AttrFor( dc, col.BackGroundColourDelegateFromRole, sVal ) );
							else
								s += HtmlLib.TableDataAttr( sData, AttrFor( dc, col.BackGroundColourDelegate, sVal ) );

						}
						s += HtmlLib.TableRowClose();
					}
					s += TotalLine( tot );
					s += HtmlLib.TableClose();
				}
				return s;
			}

			private string TotalLine( int[] tot )
			{
				string tl = "";

				if ( !Totals ) return "";

				if ( Body != null )
				{
					tl = HtmlLib.TableRowOpen();
					if ( DoRowNumbers )
						tl += HtmlLib.TableDataAttr( "Totals", "ALIGN='RIGHT' VALIGN='TOP'" );

					for ( int i = 0; i < _columns.Count; i++ )
					{
						ReportColumn col = (ReportColumn) _columns[ i ];
						if ( col.CanAccumulate )
						{
							DataColumn dc = Body.Columns[ col.Source ];
							string sData = FormatData( dc, col.Format, tot[ i ].ToString() );
							tl += HtmlLib.TableDataAttr( sData, AttrFor( dc, (ReportColumn.ColourDelegate) null, "" ) );
							LastTotal = tot[ i ];
						}
						else
							tl += HtmlLib.TableData( HtmlLib.HTMLPad( "", 1 ) );
					}
					tl += HtmlLib.TableRowClose();
				}
				return tl;
			}

			private string ColHeaders()
			{
				var headers = "";
				if ( _columns != null )
				{
					if ( _rowNumbers ) headers = HtmlLib.TableHeader( "Row" );

					//for ( int i = 0; i < _columns.Count; i++ )
					//{
					//   ReportColumn col = (ReportColumn)_columns[i];
					//   headers += HtmlLib.TableHeader( col.Header );
					//}
					headers = _columns.Cast<ReportColumn>().Aggregate( headers, 
						( current, col ) => current + HtmlLib.TableHeader( col.Header ) );
				}
				return headers;
			}

			private static string AttrFor( DataColumn dc, ReportColumn.ColourDelegateFromRole bgColour, string theValue )
			{
				string sAttr = "";
				if ( dc != null )
				{
					if ( dc.DataType == Type.GetType( "System.Decimal" ) ||
						  dc.DataType == Type.GetType( "System.Int32" ) )
						sAttr = "ALIGN='RIGHT'";
					//  this centres all strings
					//if ( dc.DataType.Equals( Type.GetType( "System.String" ) ) )
					//   sAttr = "ALIGN='CENTER'";

					if ( bgColour != null )
					{
						if ( ( !string.IsNullOrEmpty( theValue ) ) )
							sAttr += " BGCOLOR=" + bgColour( theValue );
					}
				}
				return sAttr + " VALIGN='TOP'";
			}

			private static string AttrFor( DataColumn dc, ReportColumn.ColourDelegate bgColour, string theValue )
			{
				string sAttr = "";
				if ( dc != null )
				{
					if ( dc.DataType == Type.GetType( "System.Decimal" ) ||
						  dc.DataType == Type.GetType( "System.Int32" ) )
						sAttr = "ALIGN='RIGHT'";
					//if ( dc.DataType.Equals( Type.GetType( "System.String" ) ) )
					//   sAttr = "ALIGN='CENTER'";

					if ( bgColour != null )
					{
						if ( ( !string.IsNullOrEmpty( theValue ) ) )
							sAttr += " BGCOLOR=" + bgColour( Int32.Parse( theValue ) );
					}
				}
				return sAttr + " VALIGN='TOP'";
			}

			private static string FormatData( DataColumn dc, string format, string data )
			{
				string sOut = data;

				if ( data != String.Empty )
				{
					if ( dc.DataType == Type.GetType( "System.Decimal" ) || dc.DataType == Type.GetType( "System.Int32" ) )
						sOut = Decimal.Parse( data ).Equals( -1 ) ? "--" : string.Format( format, Decimal.Parse( data ) );
            if ( dc.DataType == Type.GetType( "System.DateTime" ) )
               sOut = string.Format( format, DateTime.Parse( data ).ToShortDateString() );
         }
         return sOut;
			}

			private static bool IsEven( int someNumber )
			{
				return someNumber == ( someNumber / 2 * 2 );
			}

			private string Header( string cHeading )
			{
				string htmlOut = HtmlLib.TableOpen( "class='title' cellpadding='0' cellspacing='0' width='100%'" ) + "\n\t"
									  + HtmlLib.TableRowOpen( TopLine() ) + "\n\t\t"
									  + HtmlLib.TableDataAttr( HtmlLib.Bold( cHeading ), "colspan='2' class='gponame'" ) + "\n\t"
									  + HtmlLib.TableRowClose() + "\n\t"
									  + HtmlLib.TableRowOpen() + "\n\t\t"
									  + HtmlLib.TableDataAttr( TopLine(), "id='dtstamp'" ) + "\n\t\t"
									  + HtmlLib.TableData( HtmlLib.Div( "objshowhide", "tabindex='0'" ) ) + "\n\t"
									  + HtmlLib.TableRowClose() + "\n"
									  + HtmlLib.TableClose() + "\n";
				return htmlOut;
			}

			private string TopLine()
			{
				string theDate = string.Format( "Report Date: {0} {1}", DateTime.Now.ToLongDateString(), DateTime.Now.ToShortTimeString() );
				//			if ( string.IsNullOrEmpty( this.SubHeader ) )
				return theDate;
				//			else
				//				return string.Format( "{0}&nbsp.&nbsp.&nbsp.&nbsp.&nbsp.&nbsp.&nbsp.&nbsp.&nbsp.&nbsp.&nbsp.{1}", SubHeader, theDate );
			}

			private string Footer()
			{
				string htmlOut = HtmlLib.TableOpen( "class='title' cellpadding='0' cellspacing='0'" ) + "\n\t"
									  + HtmlLib.TableRowOpen() + "\n\t\t"
									  + HtmlLib.TableDataAttr( _footer, "colspan='2' class='gponame'" ) + "\n\t"
									  + HtmlLib.TableRowClose() + "\n\t";
				if ( ShowElapsedTime )
				{
					htmlOut += HtmlLib.TableRowOpen() + "\n\t\t"
								  + HtmlLib.TableDataAttr( "elapsed time:" + TimeTaken, "id='dtstamp'" ) + "\n\t\t"
								  + HtmlLib.TableData( HtmlLib.Div( "objshowhide", "tabindex='0'" ) ) + "\n\t"
								  + HtmlLib.TableRowClose() + "\n";
				}
				htmlOut += HtmlLib.TableClose() + "\n";
				return htmlOut;
			}

			#endregion

			#region  CSV Rendition

			public void RenderAsCsv( string fileName )
			{
				using ( FileStream fs =
						 File.Create( string.Format( "{0}{1}.csv", Utility.OutputDirectory(), fileName ) ) )
				using ( StreamWriter sw = new StreamWriter( fs ) )
					foreach ( DataRow dr in _body.Rows )
						sw.WriteLine( CsvLine( dr ) );
			}


			private string CsvLine( DataRow dr )
			{
				StringBuilder sb = new StringBuilder();
				foreach ( DataColumn col in _body.Columns )
					sb.Append( string.Format( "{0},", dr[ col.ColumnName ] ) );

				return sb.ToString();
			}

			#endregion


			#endregion

		} //end SimpleTableReport

		#region  Helper classes

		public class ReportColumn
		{
			/// <summary>
			/// The name of the column which would appear in the column header.
			/// </summary>
			public string Header;

			/// <summary>
			/// The name of the field used to populate the column.
			/// </summary>
			public string Source;

			/// <summary>
			/// The formating template for the data.
			/// </summary>
			public string Format;

			public Type Type;

			public delegate string ColourDelegate( int colValue );

			public delegate string ColourDelegateFromRole( string colValue );

			private ColourDelegateFromRole _colourDelegateFromRole;
			private ColourDelegate _colourDelegate;

			public ReportColumn( string header, string source, string format )
			{
				Header = header;
				Source = source;
				Format = format;
				CanAccumulate = false;
			}

			public ReportColumn( string header, string source, string format, ColourDelegateFromRole colourDelegateIn )
			{
				Header = header;
				Source = source;
				Format = format;
				CanAccumulate = false;
				_colourDelegateFromRole = colourDelegateIn;
			}
			public ReportColumn( string header, string source, string format, ColourDelegate colourDelegateIn )
			{
				Header = header;
				Source = source;
				Format = format;
				CanAccumulate = false;
				_colourDelegate = colourDelegateIn;
			}

			public ReportColumn( string header, string source, string format, Type type )
			{
				Header = header;
				Source = source;
				Format = format;
				Type = type;
				CanAccumulate = false;
			}

			public ReportColumn( string header, string source, string format, Type type, bool tally )
			{
				Header = header;
				Source = source;
				Format = format;
				Type = type;
				CanAccumulate = tally;
			}

			public ReportColumn( string header, string source, string format, bool tally )
			{
				Header = header;
				Source = source;
				Format = format;
				CanAccumulate = tally;
			}

			public bool CanAccumulate { get; set; }

			public ColourDelegate BackGroundColourDelegate
			{
				get { return _colourDelegate; }
				set { _colourDelegate = value; }
			}

			public ColourDelegateFromRole BackGroundColourDelegateFromRole
			{
				get { return _colourDelegateFromRole; }
				set { _colourDelegateFromRole = value; }
			}
		}

		#endregion

}
