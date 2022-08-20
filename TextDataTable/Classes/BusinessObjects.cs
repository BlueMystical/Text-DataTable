using System.Collections.Generic;
using System.ComponentModel;

namespace TextDataTable
{
	[Newtonsoft.Json.JsonObject, TypeConverter(typeof(ExpandableObjectConverter))]
	public class TableConfiguration
	{
		public TableConfiguration() { }

		/// <summary>[Required] Behavior Properties of the Table.</summary>
		[Description("[Required] Behavior Properties of the Table"), Category("Behavior"), DisplayName("Properties")]
		public Propiedades properties { get; set; }

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

		[Description("Font used to render the ImageTable"), Category("Appearance"), DisplayName("Font")]
		public Fuente font { get; set; }

		[Description("Borders for the Table"), Category("Appearance"), DisplayName("Borders")]
		public Borders borders { get; set; }

		[Description("General Config"), Category("Appearance"), DisplayName("Table")]
		public Table table { get; set; }
	}

	[Newtonsoft.Json.JsonObject, TypeConverter(typeof(ExpandableObjectConverter))]
	public class Fuente
	{
		public Fuente() { }

		[Description("Name of the Font, Only for ImageTable, use a monospaced font."),
		 DisplayName("Font Name"), DefaultValue("Courier New"), Category("Appearance")]
		public string font_name { get; set; }

		[Description("Size of the Font, Only for ImageTable"),
		 DisplayName("Font Size"), Category("Appearance"), DefaultValue(12)]
		public int font_size { get; set; }
	}

	[Newtonsoft.Json.JsonObject, TypeConverter(typeof(ExpandableObjectConverter))]
	public class Borders
	{
		public Borders() { }

		[Description("Type of Borders: 'simple', 'doble'"),
		DisplayName("Border Type"), DefaultValue("simple"), Category("Appearance")] 
		public string type { get; set; }

		[Description("ARGB Color for the Borders"),
		DisplayName("Border Color"), DefaultValue("255, 255, 106, 0"), Category("Appearance")]
		public string color_argb { get; set; }

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

		[Description("ARGB Color for the Table Background (only for Image Tables)"),
		DisplayName("Back Color"), DefaultValue("128, 0, 0, 0")]
		public string backcolor_argb { get; set; }

		[Description("ARGB Color for the Text Font, Only for ImageTable (only for Image Tables)"),
		 DisplayName("Text Color"), DefaultValue("255, 255, 106, 0"), Category("Appearance")]
		public string forecolor_argb { get; set; }

		[Description("Margin Spaces around the Texts, in Characters (only for Text Tables)"), 
		DisplayName("Cell Padding"), DefaultValue(1)]
		public int cell_padding { get; set; }

		[Description("Height of the Row Cells in Pixels (Only for Image Tables)"),
		DisplayName("Cell Height"), DefaultValue(30)]
		public int cell_height { get; set; }

		
	}

	[Newtonsoft.Json.JsonObject, TypeConverter(typeof(ExpandableObjectConverter))]
	public class Header
	{
		public Header() { }

		[Description("Text for the Title"), Category("Appearance"),
		DisplayName("Title"), DefaultValue("Example Text")]
		public string title { get; set; }

		[Description("Width in pixels for the Header/Footer Box, only for ImageTable"), 
		DisplayName("Width"), DefaultValue(100), Category("Appearance")]
		public int width { get; set; }

		[Description("Width in Characters for the Header/Footer Box, only for TextTable"),
		DisplayName("Length"), DefaultValue(100), Category("Appearance")]
		public int length { get; set; }

		/// <summary>Text Horizontal Align: [left, center, right] default 'center'</summary>
		[DisplayName("Align"), Description("Text Align: [left, center, right]"), Category("Appearance")]
		public string align { get; set; } = "center";

		[Description("Background Color for the Header/Footer Box, only for ImageTable"),
		DisplayName("Background Color"), DefaultValue("128, 0, 0, 0"), Category("Appearance")]
		public string backcolor_argb { get; set; }

		[Description("Text Color for the Header/Footer Box, only for ImageTable"),
		DisplayName("Foreground Color"), DefaultValue("255,255,255,255"), Category("Appearance")]
		public string forecolor_argb { get; set; }
	}

	[Newtonsoft.Json.JsonObject, TypeConverter(typeof(ExpandableObjectConverter))]
	public class Column
	{
		public Column() { }

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

		///// <summary>Hide or Show the Grouped Columns in the Table.</summary>
		//[DisplayName("Show Columns"), Description("Hide or Show the Grouped Columns in the Table."), Category("Data")]
		//public bool hide_group_columns { get; set; } = true;

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

	public class Row
	{
		Dictionary<string, object> properties = new Dictionary<string, object>();
		private int rIndex;

		public Row(int rIndex)
		{
			this.rIndex = rIndex;
		}

		public object this[string name]
		{
			get
			{
				if (properties.ContainsKey(name))
				{
					return properties[name];
				}
				return null;
			}
			set
			{
				properties[name] = value;
			}
		}

		public int RIndex
		{
			get { return rIndex; }
			set { rIndex = value; }
		}
	}
}
