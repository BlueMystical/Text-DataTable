using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Blue.TextDataTable.TEST
{
	public partial class Form1 : Form
	{
		/// <summary>Class able to produce DataTables in Text Mode.
		/// Author: Jhollman Chacon (Blue Mystic) - 2022</summary>
		private TextDataTable myTable; //<- STEP 1:  DECLARE THE CLASS
		private List<FilterCriteria> Criteria { get; set; }

		public Form1()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			/* CREATE A CUSTOM FILTER  */
			Criteria = new List<FilterCriteria>(
				new FilterCriteria[] {
					new FilterCriteria("scoop", true)
					{
						isFirst = true
					},
					new FilterCriteria("jump_len", 0.0m)
					{
						comparator = ">="
					}
			});
		}
		private void Form1_Shown(object sender, EventArgs e)
		{
			//STEP 2:  LOAD THE CONFIGURATION
			myTable = new TextDataTable(@"table_config.json"); //<- File is in the bin folder

			//Show the Config in the Property grid:
			propertyGrid1.SelectedObject = myTable.TConfiguration;
		}

		private void cmdTextTable_Click(object sender, EventArgs e)
		{
			//STEP 3: BUILD THE DATATABLE (in Text Mode):
			textBox1.Text = myTable.Build_TextDataTable();
		}

		private void cmdImageTable_Click(object sender, EventArgs e)
		{
			//STEP 3: BUILD THE DATATABLE (in Image Mode):
			System.Drawing.Bitmap myImageTable = myTable.Build_ImageDataTable(
				new Size(Convert.ToInt32(imgSize_W.Value),
						 Convert.ToInt32(imgSize_H.Value))
			);
			myImageTable.Save(@"C:\Temp\myImage.png", System.Drawing.Imaging.ImageFormat.Png);

			System.Diagnostics.Process.Start(@"C:\Temp\myImage.png");

			//You can get Information from the Image Mapping:
			//Here we simulate a Mouse click Event over the Image to get the Coordinates (X,Y) of a Pixel
			//Then we get the Information of the 'Cell' under that Pixel

			var PixelInfo = myTable.GetPixelInfo(new Point(279, 160));
			if (PixelInfo != null)
			{
				Console.WriteLine(string.Format("You Clicked on a '{0}' who has the '{1}' value.",
					PixelInfo.ElementType.ToString(), PixelInfo.CellText));

				//If the pixel is over one of the Rows with Data:
				if (PixelInfo.ElementType == TableElements.DATA_ROW) //<- There are other Elements you can click on and get info
				{
					/* Obtener y Modificar el Objeto referenciado x 'PixelInfo' */

					//1. Obtener el Indice del Elemento Referenciado:
					int dataindex = myTable.GetPixelDataIndex((dynamic)PixelInfo.Row);
					if (dataindex >= 0)
					{
						//2. Obtener los datos de dicho Indice:
						var myData = myTable.OriginalData[dataindex];						

						//3. Obtener el Valor actual del Campo:
						Console.WriteLine(myData[((Column)PixelInfo.Column).field]);

						//4. Modificar el Valor del Campo:
						myData[((Column)PixelInfo.Column).field] = 40.4m;

						//5. Actualizar la Tabla:
						myTable.OriginalData[dataindex] = myData;
						myTable.RefreshData();

						//BE AWARE THAT THIS MODIFICATION WAS POST-IMAGE GENERATION, 
						//you need to re-create the image to see the changes by calling the 'Build_ImageDataTable' method.
					}
				}
			}
		}

		private void cmdDataEditor_Click(object sender, EventArgs e)
		{
			//Example of Data Customization and Table Configuration: See inside the Form
			DataEditor Form = new DataEditor(myTable.TConfiguration);
			if (Form.ShowDialog() == DialogResult.OK)
			{
				this.Criteria = null;
				myTable.TConfiguration = Form.MyTableConfiguration;
				myTable.RefreshData(myTable.TConfiguration.data);

				textBox1.Text = myTable.Build_TextDataTable();
			}
		}



		private void button1_Click(object sender, EventArgs e)
		{
			/* YOU CAN APPLY A FILTER ON THE DATA IN 2 WAYS: */

			// 1. If your KungFu is strong, then write yourself a JSONPath Expression:	 
			string JSONPathExpression = @"$[?(@.jumps >= 2)]";

			JSONPathExpression = @"$[?((@.jump_len >= 0.0 || @.star_type != 'F (White) Star') && (@.scoop == true))]";
			JSONPathExpression = @"[*][?((@.jump_len >= 0.0 || @.star_type != 'F (White) Star'), ?(@.scoop == true)]";
			JSONPathExpression = @"[?(@.name=='e2e')]";
			JSONPathExpression = @"[*][?(@.jump_len >= 0.0 || @.star_type != 'F (White) Star'), ?(@.scoop == true)]";
			JSONPathExpression = @"$..[?((@.category=='fiction' || @.author=='Nigel Rees') && (@.price < 10))]";

			//var Data = myTable.FilterData(JSONPathExpression, true);

			/* Or.. If you are not that confident at Kicking butts, then build a List of Criteria
			 * either manually (see the 'Form1_Load' method here)
			 * or by Invoking the FilterEditor (see below)	*/
			var Data = myTable.FilterData(Criteria, true);

			//After getting the data filtered you need to show it, in this case on the TextTable:
			textBox1.Text = myTable.Build_TextDataTable();
		}

		private void cmdFilterEditor_Click(object sender, EventArgs e)
		{
			/* Here we invoke the Filter Editor */
			myTable.OnTableDataChanged += (Sender, Args) =>
			{
				textBox1.Text = Sender.ToString();
			};
			this.Criteria = myTable.CallFilterEditor(this.Criteria);
		}

		private void cmdUndoFilter_Click(object sender, EventArgs e)
		{
			// UNDO THE FILTERING RESTORING THE ORIGINAL DATA
			myTable.RefreshData();

			textBox1.Text = myTable.Build_TextDataTable();
		}

		private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
		{
			//Se Presionó la Tecla ENTER
			if (e.KeyChar == (char)Keys.Enter)
			{
				/* Do a quick Text Search on all fields querying for the Search String. */
				var Data = myTable.QuickSearch(textBox2.Text, true);

				//After getting the data filtered you need to show it, in this case on the TextTable:
				textBox1.Text = myTable.Build_TextDataTable();
			}
		}
		private void cmdQuickSearch_Click(object sender, EventArgs e)
		{
			/* Do a quick Text Search on all fields querying for the Search String. */

			var Data = myTable.QuickSearch(textBox2.Text, true);

			//After getting the data filtered you need to show it, in this case on the TextTable:
			textBox1.Text = myTable.Build_TextDataTable();
		}

		private void cmdLoadJSON_Click(object sender, EventArgs e)
		{
			OpenFileDialog OFDialog = new OpenFileDialog()
			{
				Filter = "JSON Data|*.json;*.txt",
				FilterIndex = 0,
				DefaultExt = "json",
				AddExtension = true,
				CheckPathExists = true,
				CheckFileExists = true,
				InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
			};

			if (OFDialog.ShowDialog() == DialogResult.OK)
			{
				List<dynamic> MyData = new List<dynamic>();

				//1. Open and Parse the Json to a Dynamic Array
				var JsonData = Newtonsoft.Json.Linq.JArray.Parse(
					System.IO.File.ReadAllText(OFDialog.FileName, System.Text.Encoding.UTF8));
				foreach (Newtonsoft.Json.Linq.JToken item in JsonData.ToList())
				{
					MyData.Add(item.ToObject<dynamic>());
				}

				//2. Set the new DataSource:
				var BlueDataTable = new TextDataTable();
				var BlueDTConfig = new TableConfiguration();
				

				//----------------------------------------				
				//3. Get the Field Names from the First Object:
				var _Fields = GetPropertyKeysForDynamic(MyData[0]);

				//4. Since We completely changed the DataSet, now We need to Update the Column Definitions:
				BlueDTConfig.columns = new List<Column>();
				foreach (var prop in _Fields)
				{
					string Ttype = prop.Value.GetType().Name.ToLower();
					BlueDTConfig.columns.Add(new Column(prop.Key, prop.Key)
					{
						type = Ttype,
						width = 250,
						length = 25
					});
				}

				BlueDTConfig.data = MyData;
				BlueDTConfig.header = new Header("Custom DataSource");
				BlueDTConfig.footer = null;
				BlueDTConfig.sorting = null;
				BlueDTConfig.grouping = null;
				BlueDTConfig.summary = null;

				BlueDataTable.TConfiguration = BlueDTConfig;
				BlueDataTable.DataSource = MyData;

				this.Criteria = null;
				myTable = BlueDataTable;
				myTable.TConfiguration = BlueDTConfig;
				myTable.RefreshData(myTable.TConfiguration.data);

				textBox1.Text = myTable.Build_TextDataTable();
				propertyGrid1.SelectedObject = myTable.TConfiguration;
			}
		}

		private Dictionary<string, object> GetPropertyKeysForDynamic(dynamic dynamicToGetPropertiesFor)
		{
			Newtonsoft.Json.Linq.JObject attributesAsJObject = dynamicToGetPropertiesFor;
			return attributesAsJObject.ToObject<Dictionary<string, object>>();
		}


	}
}
