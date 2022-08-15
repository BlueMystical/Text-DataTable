using System.Collections.Generic;
using System.ComponentModel;

namespace TextDataTable
{
	[Newtonsoft.Json.JsonObject, TypeConverter(typeof(ExpandableObjectConverter))]
	public class TableConfiguration
	{
		public TableConfiguration() { }

		[Description("Behavior Properties of the Table"), Category("Behavior"), DisplayName("Properties")]
		public Propiedades properties { get; set; }

		[Description("Box used to show a Custom Title for the Table"), Category("Appearance"), DisplayName("Header")]
		public Header header { get; set; }

		[Description("Box used to show a Custom Text for the Table"), Category("Appearance"), DisplayName("Footer")]
		public Header footer { get; set; }

		[Description("Definition of the Columns"), Category("Data"), DisplayName("Columns")]
		public List<Column> columns { get; set; }

		[Description("Calculated Fields for the Columns"), Category("Data"), DisplayName("Summary")]
		public List<Summary> summary { get; set; }

		[Description("Dynamic Data, [field_name: value,..]"), Category("Data"), DisplayName("Dynamic Data")]
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

		[Description("ARGB Color for the Table Background"),
		DisplayName("Back Color"), DefaultValue("128, 0, 0, 0")]
		public string backcolor_argb { get; set; }

		[Description("ARGB Color for the Text Font, Only for ImageTable"),
		 DisplayName("Text Color"), DefaultValue("255, 255, 106, 0"), Category("Appearance")]
		public string forecolor_argb { get; set; }

		[Description("Margin Spaces around the Texts, in Characters"), 
		DisplayName("Cell Padding"), DefaultValue(1)]
		public int cell_padding { get; set; }

		[Description("Height of the Row Cells in Pixels"),
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
}
