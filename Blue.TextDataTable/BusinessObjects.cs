using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Blue.TextDataTable
{
	[Newtonsoft.Json.JsonObject, TypeConverter(typeof(ExpandableObjectConverter))]
	public class TableConfiguration
	{
		public TableConfiguration()
		{
			properties.font = new Fuente()
			{
				font_name = "Courier New",
				font_size = 12,
				font_style = System.Drawing.FontStyle.Regular
			};
			properties.borders = new Borders()
			{
				type = "simple",
				color_argb = "255, 255, 106, 0",
			};
			properties.borders.symbols = new List<SymbolElement>(new SymbolElement[] {
				// Double Border
				new SymbolElement()
				{
					Top = new SymbolChar()
					{
						Left = '╔',
						Right = '╗',
						Middle = '╦',
						Border = '═'
					},
					Middle = new SymbolChar()
					{
						Left = '╠',
						Right = '╣',
						Middle = '╬',
						Border = '║'
					},
					Bottom = new SymbolChar()
					{
						Left = '╚',
						Right = '╝',
						Middle = '╩',
						Border = '═'
					},
					Sides = new SymbolChar()
					{
						Left = '║',
						Right = '║',
						Border = '═'
					},
				},
				// Simple Border
				new SymbolElement()
				{
					Top = new SymbolChar()
					{
						Left = '┌',
						Right = '┐',
						Middle = '┬',
						Border = '─'
					},
					Middle = new SymbolChar()
					{
						Left = '├',
						Right = '┤',
						Middle = '┼',
						Border = '│'
					},
					Bottom = new SymbolChar()
					{
						Left = '└',
						Right = '┘',
						Middle = '┴',
						Border = '─'
					},
					Sides = new SymbolChar()
					{
						Left = '│',
						Right = '│',
						Border = '─'
					},
				},
			});
		}

		/// <summary>[Required] Behavior Properties of the Table.</summary>
		[Description("[Required] Behavior Properties of the Table"), Category("Behavior"), DisplayName("Properties")]
		public Propiedades properties { get; set; } = new Propiedades();

		/// <summary>[Optional] Box used to show a Custom Title for the Table.</summary>
		[Description("[Optional] Box used to show a Custom Title for the Table"), Category("Appearance"), DisplayName("Header")]
		public Header header { get; set; }

		/// <summary>[Optional] Box used to show a Custom Text for the Table.</summary>
		[Description("[Optional] Box used to show a Custom Text for the Table"), Category("Appearance"), DisplayName("Footer")]
		public Header footer { get; set; }

		/// <summary>[Required] Definition of the Columns.</summary>
		[Description("[Required] Definition of the Columns"), Category("Data"), DisplayName("Columns")]
		public List<Column> columns { get; set; }

		/// <summary>[Optional] Columns to Sort By, Up to 4 Columns.</summary>
		[Description("[Optional] Columns to Sort By, Up to 4 Columns."), Category("Data"), DisplayName("Sorting")]
		public Sorting sorting { get; set; }

		/// <summary>[Optional] Columns to Group By, Up to 4 Columns.</summary>
		[Description("[Optional] Columns to Group By, Up to 4 Columns."), Category("Data"), DisplayName("Grouping")]
		public Grouping grouping { get; set; }

		/// <summary>[Optional] Calculated Fields for the Columns.</summary>
		[Description("[Optional] Calculated Fields for the Columns"), Category("Data"), DisplayName("Summary")]
		public List<Summary> summary { get; set; }

		/// <summary>[Required] Dynamic Data to feed the Table.</summary>
		[Description("[Required] Dynamic Data to feed the Table."), Category("Data"), DisplayName("Dynamic Data")]
		public dynamic data { get; set; }
	}

	[Newtonsoft.Json.JsonObject, TypeConverter(typeof(ExpandableObjectConverter))]
	public class Propiedades
	{
		public Propiedades() { }

		/// <summary>Font Options for this Element. [Image Table Only]</summary>
		[DisplayName("Font"), Description("Font Options for this Element. [Image Table Only]"), Category("Appearance")]
		public Fuente font { get; set; } = new Fuente();

		[Description("Borders for the Table"), Category("Appearance"), DisplayName("Borders")]
		public Borders borders { get; set; } = new Borders();

		[Description("General Config"), Category("Appearance"), DisplayName("Table")]
		public Table table { get; set; } = new Table();
	}

	[Newtonsoft.Json.JsonObject, TypeConverter(typeof(ExpandableObjectConverter))]
	public class Fuente
	{
		public Fuente() { }

		/// <summary>Name of the Font, Only for ImageTable, use a monospaced font.</summary>
		[Description("Name of the Font, Only for ImageTable, use a monospaced font."),
		 DisplayName("Font Name"), DefaultValue("Courier New"), Category("Appearance")]
		public string font_name { get; set; } = "Courier New";

		/// <summary>Size of the Font, Only for ImageTable</summary>
		[Description("Size of the Font, Only for ImageTable"),
		 DisplayName("Font Size"), Category("Appearance"), DefaultValue(12)]
		public int font_size { get; set; } = 10;

		/// <summary>Set the Font Style: 'Regular', 'Bold', 'Italic', 'Underline'</summary>
		public System.Drawing.FontStyle font_style { get; set; } = System.Drawing.FontStyle.Regular;

		public System.Drawing.Font ToFont()
		{
			return new System.Drawing.Font(this.font_name, font_size, font_style);
		}
	}

	[Newtonsoft.Json.JsonObject, TypeConverter(typeof(ExpandableObjectConverter))]
	public class Borders
	{
		public Borders() { }

		[Description("Type of Borders: 'simple', 'doble'"),
		DisplayName("Border Type"), DefaultValue("simple"), Category("Appearance")]
		public string type { get; set; } = "simple";

		[Description("ARGB Color for the Borders"),
		DisplayName("Border Color"), DefaultValue("255, 255, 106, 0"), Category("Appearance")]
		public string color_argb { get; set; } = "255, 255, 106, 0";

		[DisplayName("Border Symbols"), Description("Char Symbols used for the Borders"), Category("Appearance")]
		public List<SymbolElement> symbols { get; set; }
	}

	[Newtonsoft.Json.JsonObject, TypeConverter(typeof(ExpandableObjectConverter))]
	public class SymbolElement
	{
		public SymbolElement() { }

		[Description("Symbols for the Top Border")]
		public SymbolChar Top { get; set; }

		[Description("Symbols for the Middle Border")]
		public SymbolChar Middle { get; set; }

		[Description("Symbols for the Bottom Border")]
		public SymbolChar Bottom { get; set; }

		[Description("Symbols for the Sides Border")]
		public SymbolChar Sides { get; set; }
	}

	[Newtonsoft.Json.JsonObject, TypeConverter(typeof(ExpandableObjectConverter))]
	public class SymbolChar
	{
		public SymbolChar() { }

		public char Left { get; set; }
		public char Right { get; set; }
		public char Middle { get; set; }
		public char Border { get; set; }
	}

	[Newtonsoft.Json.JsonObject, TypeConverter(typeof(ExpandableObjectConverter))]
	public class Table
	{
		public Table() { }

		/// <summary>Configuration for the Column Headers</summary>
		public table_config column_headers { get; set; } = new table_config();

		/// <summary>Configuration for the Rows of Data</summary>
		public table_config data_rows { get; set; } = new table_config();
	}

	[Newtonsoft.Json.JsonObject, TypeConverter(typeof(ExpandableObjectConverter))]
	public class table_config
	{
		public table_config() { }

		/// <summary>Margin Spaces around the Texts, in Characters (only for Text Tables)</summary>
		[Description("Margin Spaces around the Texts, in Characters (only for Text Tables)"),
		DisplayName("Cell Padding"), DefaultValue(1)]
		public int cell_padding { get; set; } = 1;

		/// <summary>Height of the Row Cells in Pixels (Only for Image Tables)</summary>
		[Description("Height of the Row Cells in Pixels (Only for Image Tables)"),
		DisplayName("Cell Height"), DefaultValue(30)]
		public int cell_height { get; set; } = 30;

		/// <summary>Colors for the Box containing this element. [Image Table Only]</summary>
		[DisplayName("Colors"), Description("Colors for the Box containing this element. [Image Table Only]"), Category("Appearance")]
		public Colores colors { get; set; } = new Colores();

		/// <summary>Font Options for this Element. [Image Table Only]</summary>
		[DisplayName("Font"), Description("Font Options for this Element. [Image Table Only]"), Category("Appearance")]
		public Fuente font { get; set; } = new Fuente()
		{
			font_name = "Courier New",
			font_style = System.Drawing.FontStyle.Regular,
			font_size = 12
		};
	}

	[Newtonsoft.Json.JsonObject, TypeConverter(typeof(ExpandableObjectConverter))]
	public class Header
	{
		public Header() { }
		public Header(string Title)
		{
			this.title = Title;
			this.size.length = Title.Length + 2;
		}

		/// <summary>Text for the Title</summary>
		[Description("Text for the Title"), Category("Appearance"),
		DisplayName("Title"), DefaultValue("Example Text")]
		public string title { get; set; }

		/// <summary>Text Horizontal Align: [left, center, right] default 'center'</summary>
		[DisplayName("Text Align"), Description("Text Align: [left, center, right]"), Category("Appearance")]
		public string text_align { get; set; } = "center";

		/// <summary>Size for the Box containing this element.</summary>
		[DisplayName("Size"), Description("Size for the Box containing this element."), Category("Appearance")]
		public Tamaño size { get; set; } = new Tamaño();

		/// <summary>Colors for the Box containing this element.</summary>
		[DisplayName("Colors"), Description("Colors for the Box containing this element."), Category("Appearance")]
		public Colores colors { get; set; } = new Colores();

		/// <summary>Font Options for this Element.</summary>
		[DisplayName("Font"), Description("Font Options for this Element."), Category("Appearance")]
		public Fuente font { get; set; } = new Fuente()
		{
			font_name = "Courier New",
			font_style = System.Drawing.FontStyle.Regular,
			font_size = 12
		};
	}

	[Newtonsoft.Json.JsonObject, TypeConverter(typeof(ExpandableObjectConverter))]
	public class Tamaño
	{
		public Tamaño() { }

		/// <summary>Width in pixels for the Box, only for ImageTable</summary>
		[Description("Width in pixels for the Header/Footer Box, only for ImageTable"),
		DisplayName("Width"), DefaultValue(500), Category("Appearance")]
		public int width { get; set; } = 500;

		/// <summary>Height in pixels for the Box, only for ImageTable</summary>
		[Description("Height in pixels for the Header/Footer Box, only for ImageTable"),
		DisplayName("Height"), DefaultValue(30), Category("Appearance")]
		public int height { get; set; } = 30;

		/// <summary>Width in Characters for the Box, only for TextTable</summary>
		[Description("Width in Characters for the Header/Footer Box, only for TextTable"),
		DisplayName("Length"), DefaultValue(50), Category("Appearance")]
		public int length { get; set; } = 50;

		/// <summary>Gets or set if this element will use the whole Row. [for both types of table]</summary>
		[Description("Gets or set if this element will use the whole Row."),
		DisplayName("Use Whole Row"), DefaultValue(50), Category("Appearance")]
		public bool use_whole_row { get; set; } = false;
	}

	[Newtonsoft.Json.JsonObject, TypeConverter(typeof(ExpandableObjectConverter))]
	public class Colores
	{
		public Colores() { }

		/// <summary>Background Color for the Header/Footer Box, only for ImageTable</summary>
		[Description("Background Color for the Header/Footer Box, only for ImageTable"),
		DisplayName("Background Color"), DefaultValue("128, 0, 0, 100"), Category("Appearance")]
		public string backcolor_argb { get; set; } = "128, 0, 0, 100";

		/// <summary>Text Color for the Header/Footer Box, only for ImageTable</summary>
		[Description("Text Color for the Header/Footer Box, only for ImageTable"),
		DisplayName("Foreground Color"), DefaultValue("255,255,255,255"), Category("Appearance")]
		public string forecolor_argb { get; set; } = "255,255,255,255";

		/// <summary>Text Color for the Header/Footer Box, only for ImageTable</summary>
		[Description("Alternative Color for the Box, only for ImageTable"),
		DisplayName("Alternative Color"), DefaultValue("255,255,255,255"), Category("Appearance")]
		public string altcolor_argb { get; set; } = "255,255,255,255";

		/// <summary>Returns the specified Color.</summary>
		/// <param name="pColorName">Name of the Property that holds the Color</param>
		public System.Drawing.Color ToColor(string pColorName)
		{
			System.Drawing.Color _ret = System.Drawing.Color.Transparent;

			string pARGBColor;
			switch (pColorName)
			{
				case "forecolor_argb":
					pARGBColor = this.forecolor_argb;
					break;
				case "backcolor_argb":
					pARGBColor = this.backcolor_argb;
					break;
				default:
					pARGBColor = this.altcolor_argb;
					break;
			}

			if (pARGBColor != null && pARGBColor != string.Empty)
			{
				var ColorArray = pARGBColor.Split(new char[] { ',' });
				if (ColorArray != null && ColorArray.Length > 0)
				{
					_ret = System.Drawing.Color.FromArgb(
						Convert.ToInt32(ColorArray[0]),
						Convert.ToInt32(ColorArray[1]),
						Convert.ToInt32(ColorArray[2]),
						Convert.ToInt32(ColorArray[3])
					);
				}
			}
			return _ret;
		}

		/// <summary>Returns a Color Brush for the specified Color.</summary>
		/// <param name="pColorName">Name of the Property that holds the Color</param>
		public System.Drawing.SolidBrush ToBrush(string pColorName)
		{
			return new System.Drawing.SolidBrush(this.ToColor(pColorName));
		}
	}

	[Newtonsoft.Json.JsonObject, TypeConverter(typeof(ExpandableObjectConverter))]
	public class Column
	{
		public Column() { }
		public Column(string Field, string Title)
		{
			this.field = Field;
			this.title = Title;
		}


		/// <summary>[REQUIRED] Display Name for the field Column.</summary>
		[DisplayName("Title"), Description("Header Text for the Column"), Category("Appearance")]
		public string title { get; set; }

		/// <summary>[REQUIRED] Name of the field that holds the data.
		///<para>If 'type' is set to 'Calculated' then 'field' can be an Expression for a Calculated Value for the current Row of Data.</para>
		///<para>example: COUNT(field1), SUM(field1,field2,field3), CONCAT(field1,field2) etc.</para>
		///<para>Supported: COUNT, SUM, AVG, MAX, MIN, FIRST, LAST, CONCAT</para> </summary>
		[DisplayName("Field"), Description("Data Field for the Column"), Category("Appearance")]
		public string field { get; set; }

		/// <summary>[REQUIRED] Type of the DataField.
		/// <para>Supported Types: string,int,decimal,DateTime,boolean, Calculated</para></summary>
		[DisplayName("Type"), Description("Type of the DataField: [string,int,long,decimal,DateTime,boolean, Calculated]"), Category("Appearance")]
		public string type { get; set; }

		/// <summary>Width in pixels for the Column Box, only for ImageTable.</summary>
		[DisplayName("Width"), Description("Width in pixels for the Column Box, only for ImageTable"), Category("Appearance")]
		public int width { get; set; }

		/// <summary>Width in Characters for the Column Box, only for TextTable.</summary>
		[DisplayName("Length"), Description("Width in Characters for the Column Box, only for TextTable"), Category("Appearance")]
		public int length { get; set; }

		[DisplayName("Format"), Description("Format Used to Show the Data"), Category("Appearance")]
		public string format { get; set; }

		/// <summary>Text Horizontal Align: [left, center, right] default 'center'</summary>
		[DisplayName("Align"), Description("Text Align: [left, center, right]"), Category("Appearance")]
		public string align { get; set; } = "center";
	}


	[Newtonsoft.Json.JsonObject, TypeConverter(typeof(ExpandableObjectConverter))]
	public class Summary
	{
		public Summary() { }
		public Summary(string Field, string Agregate)
		{
			this.field = Field;
			this.agregate = Agregate;
		}

		[DisplayName("Field"), Description("Data Field for the Column"), Category("Appearance")]
		public string field { get; set; }

		/// <summary>Agregate Function, Supported: COUNT, SUM, AVG, MAX, MIN, FIRST, LAST</summary>
		[DisplayName("Agregate"), Description("Agregate Function, Supported: COUNT, SUM, AVG, MAX, MIN, FIRST, LAST"), Category("Appearance")]
		public string agregate { get; set; }

		[DisplayName("Format"), Description("Format Used to Show the Data"), Category("Appearance")]
		public string format { get; set; }
	}

	[Newtonsoft.Json.JsonObject, TypeConverter(typeof(ExpandableObjectConverter))]
	public class Grouping
	{
		public Grouping() { }
		public Grouping(bool pEnabled) { enabled = pEnabled; }

		/// <summary>Enable or Disable the Data Grouping. Default: False</summary>
		[DisplayName("Enabled"), Description("Enable or Disable the Data Grouping."), Category("Data")]
		public bool enabled { get; set; } = false;

		/// <summary>Data Fields to Group by</summary>
		[DisplayName("Fields"), Description("Data Fields to Group by"), Category("Data")]
		public List<string> fields { get; set; }

		/// <summary>Hide or Show the Records Count for each Group.</summary>
		[DisplayName("Show Count"), Description("Hide or Show the Records Count for each Group."), Category("Data")]
		public bool show_count { get; set; } = true;

		/// <summary>Format to Show the Record Count.</summary>
		public string CountFormat { get; set; } = "{0:n0} Records";

		/// <summary>Hide or Shows the Summaries for each Group.</summary>
		[DisplayName("Show Summaries"), Description("Hide or Shows the Summaries for each Group."), Category("Data")]
		public bool show_summary { get; set; } = true;

		/// <summary>If 'true' draws the Column headers on every Group.
		/// If 'false' the column headers are drawn only at the top, before the Groups.</summary>
		[DisplayName("Repeat Headers"), Description("If 'true' draws the Column headers on every Group."), Category("Data")]
		public bool repeat_column_headers { get; set; } = true;

		/// <summary>Colors for the Box containing this element. [Image Table Only]</summary>
		[DisplayName("Colors"), Description("Colors for the Box containing this element. [Image Table Only]"), Category("Appearance")]
		public Colores colors { get; set; } = new Colores();

		/// <summary>Font Options for this Element. [Image Table Only]</summary>
		[DisplayName("Font"), Description("Font Options for this Element. [Image Table Only]"), Category("Appearance")]
		public Fuente font { get; set; } = new Fuente()
		{
			font_name = "Courier New",
			font_style = System.Drawing.FontStyle.Regular,
			font_size = 12
		};
	}

	[Newtonsoft.Json.JsonObject, TypeConverter(typeof(ExpandableObjectConverter))]
	public class Sorting
	{
		public Sorting() { }
		public Sorting(bool pEnabled) { enabled = pEnabled; }

		/// <summary>Enable or Disable the Data Sorting. Default: False</summary>
		[DisplayName("Enabled"), Description("Enable or Disable the Data Sorting."), Category("Data")]
		public bool enabled { get; set; } = false;

		/// <summary>Data Fields to Sort by, up to 4 Sorting Fields. Default Sort Direction is 'ASC'.
		/// <para>[REQUIRED] if 'enabled' is true.</para>
		/// <para>You can specify the direction for each field:  'field_name1 DESC', 'field_name2 ASC'</para></summary>
		[DisplayName("Fields"), Description("Data Fields to Sort by. Default Sort Direction is 'ASC'. [REQUIRED] if 'enabled' is true."), Category("Data")]
		public List<string> fields { get; set; }
	}

	public class GroupData
	{
		public GroupData() { }

		/// <summary>Lines for the Group Header: The Top Line, the Middle line with text and the Bottom Border.</summary>
		public List<string> Header { get; set; }

		/// <summary>Text Content of the Group Header.</summary>
		public string HeaderData { get; set; }

		/// <summary>Lines for the Group Body showing the Filtered Data, each 2 lines is a Row of Data.</summary>
		public List<string> Body { get; set; }

		/// <summary>Lines for the Group Footer: used to show Summary Information.</summary>
		public List<string> Footer { get; set; }

		/// <summary>Filtered Data for this Group. Each is a Row of Data.</summary>
		public List<dynamic> data { get; set; }

		/// <summary>Column Definitions of the Grouping Fields.</summary>
		public List<Column> Columns { get; set; }

		/// <summary>Record Count for this Group.</summary>
		public int Count { get; set; }
		
	}

	/// <summary>Returns a JSONPath expression for a Field/value Comparation.</summary>
	public class FilterCriteria : System.ICloneable
	{
		public FilterCriteria() { }
		public FilterCriteria(string Field, object Value)
		{
			this.field = Field;
			this.value = Value;
		}

		public string field { get; set; }
		public object value { get; set; }

		/// <summary>Comparation Operators, between the field and the Value.
		/// <para>Supported: ==, !=, &lt;, &lt;=, &gt;, &gt;=, START, END, CONTAINS.</para>
		/// <para>Default: '=='</para></summary>
		public string comparator { get; set; } = "==";
		

		/// <summary>Is this an Optional Filter? if so, adds the 'OR' prefix. Default false.</summary>
		public bool isOR { get; set; } = false;

		/// <summary>Is this an Exclusive Filter? if so, adds the 'AND' prefix. Default true</summary>
		public bool isAND { get; set; } = true;

		/// <summary>Is this the First Filter? if so, do not adds the prefix. Default false.</summary>
		public bool isFirst { get; set; } = false;

		/// <summary>Is this Filter Enabled?</summary>
		public bool enabled { get; set; } = true;

		/// <summary>Returns an expression for the Field/value.</summary>
		public override string ToString()
		{
			if (this.enabled)
			{
				string _Prefix = string.Empty;
				string _COMPARA = string.Empty;

				if (!isFirst)
				{
					if (isAND) _Prefix = " && ";
					if (isOR) _Prefix = " || ";
				}
				switch (comparator)
				{
					case "START":	_COMPARA = "=~"; break;
					case "END":		_COMPARA = "=~"; break;
					case "CONTAINS": _COMPARA = "=~"; break;
					default:  _COMPARA = comparator; break;
				}
				return string.Format("{0}@.{1} {2} {3}", _Prefix, field, _COMPARA, GetValue());
			}
			else
			{
				return string.Empty;
			}
		}
		/// <summary>Returns an expression for the Field/value.</summary>
		/// <param name="ShowDisabled">Hide or Show the disabled filters.</param>
		public string ToString(bool ShowDisabled)
		{
			if (ShowDisabled || this.enabled)
			{
				string _Prefix = string.Empty;
				string _COMPARA = string.Empty;

				if (!isFirst)
				{
					if (isAND) _Prefix = " && ";
					if (isOR) _Prefix = " || ";
				}
				switch (comparator)
				{
					case "START":	_COMPARA = "=~"; break;
					case "END":		_COMPARA = "=~"; break;
					case "CONTAINS": _COMPARA = "=~"; break;
					default:  _COMPARA = comparator; break;
				}

				return string.Format("{0}@.{1} {2} {3}", _Prefix, field, _COMPARA, GetValue());
			}
			else
			{
				return string.Empty;
			}
		}

		/// <summary>Returns the Value of the Field formatted for use in a JSONPath expression.</summary>
		private string GetValue()
		{
			string _ret = (this.value != null) ? this.value.ToString() : string.Empty;

			switch (this.value.GetType().ToString())
			{
				case "System.String":	_ret = string.Format("'{0}'", _ret); break;
				case "System.Int32":	break;				
				case "System.Int64":	break;
				case "System.Decimal":	_ret = _ret.Replace(',', '.'); break;
				case "System.Double":	_ret = _ret.Replace(',', '.'); break;
				case "System.DateTime": _ret = string.Format("{0}", Newtonsoft.Json.JsonConvert.SerializeObject(this.value).Replace('"', '\'')); break;
				case "System.Boolean":	_ret = _ret.ToLower(); break;

				default: _ret = this.value.ToString(); break;
			}

			switch (comparator)
			{
				case "START":	_ret = string.Format(@"/{0}.*/i", this.value.ToString()); break;
				case "END":		_ret = string.Format(@"/.*{0}/i", this.value.ToString()); break;
				case "CONTAINS": _ret = string.Format(@"/.*{0}.*/i", this.value.ToString()); break;
				default:		break;
			}

			return _ret;
		}

		/// <summary>Permite Copiar por Valor el Objeto con todas sus propiedades y atributos.</summary>
		public object Clone()
		{
			return this.MemberwiseClone();
		}

	}

	public class ImageMapping
	{
		public ImageMapping() { }
		public ImageMapping(int RowIndex, int ColIndex)
		{
			row_index = RowIndex;
			col_index = ColIndex;
		}

		/// <summary>Type of Element.</summary>
		public TableElements ElementType { get; set; }

		/// <summary>Un-formatted raw Value.</summary>
		public object RawValue { get; set; }

		/// <summary>Text Shown in the CellBox.</summary>
		public string CellText { get; set; }


		public int row_index { get; set; }
		public int col_index { get; set; }

		
		public System.Drawing.Rectangle CellBox { get; set; }

		public object Column { get; set; }
		public object Row { get; set; }

		/// <summary>Checks if a Pixel is in the boundaries of the Box.</summary>
		/// <param name="Pixel">Coordinates of the Pixel to look for.</param>
		public bool IsPixelInBox(System.Drawing.Point Pixel)
		{
			bool _ret = false;

			if (this.CellBox != null)
			{
				bool XisInRange = false;
				bool YisInRange = false;

				if (Pixel.X >= CellBox.Location.X && Pixel.X <= (CellBox.Location.X + CellBox.Width))
				{
					XisInRange = true;
				}
				if (Pixel.Y >= CellBox.Location.Y && Pixel.Y <= (CellBox.Location.Y + CellBox.Height))
				{
					YisInRange = true;
				}
				_ret = (XisInRange && YisInRange);
			}

			return _ret;
		}
	}

	public class MyEventArgs : EventArgs
	{
		public string EventType { get; set; }
		public object EventData { get; set; }
	}

	public enum TableElements
	{
		TABLE_HEADER = 0,
		GROUP_HEADER,
		COLUMN_ROW,
		DATA_ROW,
		GROUP_SUMMARY,
		TABLE_SUMMARY,
		TABLE_FOOTER
	}

	public class Row
	{
		Dictionary<string, object> fields = new Dictionary<string, object>();
		private long rIndex;

		public Row(long rIndex)
		{
			this.rIndex = rIndex;
		}

		public object this[string name]
		{
			get
			{
				if (fields.ContainsKey(name))
				{
					return fields[name];
				}
				return null;
			}
			set
			{
				fields[name] = value;
			}
		}

		public long RIndex
		{
			get { return rIndex; }
			set { rIndex = value; }
		}
	}
}
