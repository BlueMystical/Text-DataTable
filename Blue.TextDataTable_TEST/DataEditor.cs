using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Blue.TextDataTable.TEST
{
	public partial class DataEditor : Form
	{
		public TableConfiguration MyTableConfiguration { get; set; }
		public bool CustomDataSet { get; set; } = false;

		public DataEditor(TableConfiguration pTableConfiguration)
		{
			InitializeComponent();
			MyTableConfiguration = pTableConfiguration;
		}

		private void DataEditor_Load(object sender, EventArgs e)
		{
			this.dataGridView.DataSource = this.MyTableConfiguration.data;
		}

		private void dataGridView_DataSourceChanged(object sender, EventArgs e)
		{
			if (this.MyTableConfiguration.data != null)
			{
				this.textBox1.Text = JsonConvert.SerializeObject(this.MyTableConfiguration.data, Formatting.Indented);
			}
		}	

		private void dataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			if (this.MyTableConfiguration.data != null)
			{
				this.textBox1.Text = JsonConvert.SerializeObject(this.MyTableConfiguration.data, Formatting.Indented);
			}
		}

		private void cmdApply_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
		}
		private void cmdCustomData_Click(object sender, EventArgs e)
		{
			if (this.MyTableConfiguration.data != null)
			{
				//YOU CAN USE ANY OBJECT DATASET AS SOURCE FOR THE DATA;

				List<myCustomData> CustomData = new List<myCustomData>(new myCustomData[] {
					new myCustomData()
					{
						Field1 = 1001,
						Field2 = "Custom Text for Field 1",
						Field3 = 1001.9876m,
						Field4 = DateTime.Today.AddDays(-10),
						ColorField = System.Drawing.Color.FromArgb(128,255,255,0),
						SizeField = new System.Drawing.Size(50, 60),
						FontField = new System.Drawing.Font("Arial", 12, System.Drawing.FontStyle.Regular)
					},
					new myCustomData()
					{
						Field1 = 1002,
						Field2 = "Custom Text for Field 2",
						Field3 = 507.2345m,
						Field4 = DateTime.Today.AddDays(-8),
						ColorField = System.Drawing.Color.Brown,
						FontField = new System.Drawing.Font("Arial", 12, System.Drawing.FontStyle.Bold)
					},
					new myCustomData()
					{
						Field1 = 1003,
						Field2 = "Custom Text for Field 3",
						Field3 = 7777.666m,
						Field4 = DateTime.Today.AddDays(-6),
						ColorField = System.Drawing.Color.Pink
					},
					new myCustomData()
					{
						Field1 = 1004,
						Field2 = "Custom Text for Field 4",
						Field3 = 2054.94446m,
						Field4 = DateTime.Today.AddDays(-4),
						ColorField = System.Drawing.Color.Orange
					},
					new myCustomData()
					{
						Field1 = 1005,
						Field2 = "Custom Text for Field 5",
						Field3 = 1001.9876m,
						Field4 = DateTime.Today.AddDays(-2),
						ColorField = System.Drawing.Color.BlueViolet
					},
					new myCustomData()
					{
						Field1 = 1006,
						Field2 = "Custom Text for Field 6",
						Field3 = 99.75643m,
						Field4 = DateTime.Today.AddDays(0),
						ColorField = System.Drawing.Color.Turquoise
					},
					//---------------------------------------------------------
					new myCustomData()
					{
						Field1 = 1001,
						Field2 = "Custom Text for Field 1",
						Field3 = 101.9876m,
						Field4 = DateTime.Today.AddDays(-10),
						ColorField = System.Drawing.Color.BlueViolet
					},
					new myCustomData()
					{
						Field1 = 1002,
						Field2 = "Custom Text for Field 2",
						Field3 = 5070.2345m,
						Field4 = DateTime.Today.AddDays(-8),
						ColorField = System.Drawing.Color.BlueViolet
					},
					new myCustomData()
					{
						Field1 = 1003,
						Field2 = "Custom Text for Field 3",
						Field3 = 777.666m,
						Field4 = DateTime.Today.AddDays(-6),
						ColorField = System.Drawing.Color.BlueViolet
					},
					new myCustomData()
					{
						Field1 = 1004,
						Field2 = "Custom Text for Field 4",
						Field3 = 254.94446m,
						Field4 = DateTime.Today.AddDays(-4),
						ColorField = System.Drawing.Color.Orange
					},
					new myCustomData()
					{
						Field1 = 1005,
						Field2 = "Custom Text for Field 5",
						Field3 = 101.9876m,
						Field4 = DateTime.Today.AddDays(-2),
						ColorField = System.Drawing.Color.BlueViolet
					},
					new myCustomData()
					{
						Field1 = 1006,
						Field2 = "Custom Text for Field 6",
						Field3 = 999.75643m,
						Field4 = DateTime.Today.AddDays(0),
						ColorField = System.Drawing.Color.Turquoise
					},
					//---------------------------------------------------------------
					new myCustomData()
					{
						Field1 = 1001,
						Field2 = "Custom Text for Field 1",
						Field3 = 101.9876m,
						Field4 = DateTime.Today.AddDays(-10),
						ColorField = System.Drawing.Color.BlueViolet
					},
					new myCustomData()
					{
						Field1 = 1002,
						Field2 = "Custom Text for Field 2",
						Field3 = 5070.2345m,
						Field4 = DateTime.Today.AddDays(-8),
						ColorField = System.Drawing.Color.BlueViolet
					},
					new myCustomData()
					{
						Field1 = 1003,
						Field2 = "Custom Text for Field 3",
						Field3 = 777.666m,
						Field4 = DateTime.Today.AddDays(-6),
						ColorField = System.Drawing.Color.BlueViolet
					},
					new myCustomData()
					{
						Field1 = 1004,
						Field2 = "Custom Text for Field 4",
						Field3 = 254.94446m,
						Field4 = DateTime.Today.AddDays(-4),
						ColorField = System.Drawing.Color.Orange
					},
					new myCustomData()
					{
						Field1 = 1005,
						Field2 = "Custom Text for Field 5",
						Field3 = 101.9876m,
						Field4 = DateTime.Today.AddDays(-2),
						ColorField = System.Drawing.Color.BlueViolet
					},
					new myCustomData()
					{
						Field1 = 1006,
						Field2 = "Custom Text for Field 6",
						Field3 = 999.75643m,
						Field4 = DateTime.Today.AddDays(0),
						ColorField = System.Drawing.Color.Turquoise
					}
				});

				this.CustomDataSet = true;
				this.MyTableConfiguration.data = CustomData;
				this.dataGridView.DataSource = this.MyTableConfiguration.data;

				//Since We completely changed the DataSet, now We need to Update the Column Definitions:
				this.MyTableConfiguration.columns = new List<Column>(new Column[] {
					new Column("Field1","Field 1")
					{
						type = "int",
						width = 100,
						length = 5,
						format = "{0:n0}"
					},
					new Column("Field2","Field 2")
					{
						type = "string",
						width = 250,
						length = 25
					},
					new Column("Field3","Field 3")
					{
						type = "decimal",
						width = 150,
						length = 8,
						align = "right",
						format = "{0:C2}"
					},
					new Column("Field4","Field 4")
					{
						type = "DateTime",
						width = 100,
						length = 8,
						align = "center",
						format = "{0:d}"
					},
					new Column("ColorField","Color")
					{
						type = "Color",
						width = 200,
						length = 20,
						format = null
					},
					/* HERE WE ARE GOING TO ADD A CALCULATED FIELD WICH WILL BE THE SUM OF	Field1 and Field3 */
					new Column()
					{
						title = "Calculated",
						field = "SUM(Field3,Field1)",
						type = "Calculated",
						width = 130,
						length = 8,
						align = "right",
						format = "{0:n2}"
					},
					/* FOR THIS EXAMPLE, WE WILL IGNORE THE REST OF THE FIELDS */
				});

				//Lets add a Summary Field: (This is Optional)
				this.MyTableConfiguration.summary = new List<Summary>()
				{
					new Summary("Field3", "SUM")
					{
						format = "{0:n2}"
					},
					new Summary("Field4", "COUNT")
					{
						format = "{0} days"
					}
				};

				/* FOR THIS EXAMPLE, WE ALSO GOING TO IGNORE THE FOOTER */
				this.MyTableConfiguration.header = new Header("Custom DataSet on the Table");
				this.MyTableConfiguration.footer = null;				

				/* ENABLE AND SORT THE DATA BY 2 FIELDS  */
				this.MyTableConfiguration.sorting = new Sorting(true)
				{
					fields = new List<string>(new string[] {
						"Field1 DESC",
						"Field3" //<- 'ASC' is default
					})
				};

				/* GROUPING */
				this.MyTableConfiguration.grouping = new Grouping(true)
				{
					repeat_column_headers = false,
					show_summary = true,
					fields = new List<string>(new string[] {
						"Field2", "ColorField"
					})
				};
			}
		}

		private void button1_Click(object sender, EventArgs e)
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
				this.CustomDataSet = true;
				this.MyTableConfiguration.data = MyData;
				this.dataGridView.DataSource = this.MyTableConfiguration.data;

				//----------------------------------------				
				//3. Get the Field Names from the First Object:
				var _Fields = GetPropertyKeysForDynamic(MyData[0]);

				//4. Since We completely changed the DataSet, now We need to Update the Column Definitions:
				this.MyTableConfiguration.columns = new List<Column>();
				foreach (var prop in _Fields)
				{
					string Ttype = prop.Value.GetType().Name.ToLower();
					this.MyTableConfiguration.columns.Add(new Column(prop.Key, prop.Key)
					{
						type = Ttype,
						width = 250,
						length = 25
					});
				}

				this.MyTableConfiguration.header = new Header("Custom DataSource");
				this.MyTableConfiguration.footer = null;
				this.MyTableConfiguration.sorting = null;
				this.MyTableConfiguration.grouping = null;
				this.MyTableConfiguration.summary = null;
			}

		}

		private Dictionary<string, object> GetPropertyKeysForDynamic(dynamic dynamicToGetPropertiesFor)
		{
			Newtonsoft.Json.Linq.JObject attributesAsJObject = dynamicToGetPropertiesFor;
			return attributesAsJObject.ToObject<Dictionary<string, object>>();
		}
		public List<string> GetPropertyKeysForDynamicEx(dynamic dynamicToGetPropertiesFor)
		{
			Newtonsoft.Json.Linq.JObject attributesAsJObject = dynamicToGetPropertiesFor;
			Dictionary<string, object> values = attributesAsJObject.ToObject<Dictionary<string, object>>();
			List<string> toReturn = new List<string>();
			foreach (string key in values.Keys)
			{
				toReturn.Add(key);
			}
			return toReturn;
		}
	}

	public class myCustomData
	{
		public myCustomData() { }

		public int Field1 { get; set; }
		public string Field2 { get; set; }
		public decimal Field3 { get; set; }
		public DateTime Field4 { get; set; }

		public System.Drawing.Color ColorField { get; set; }
		public System.Drawing.Size SizeField { get; set; }
		public System.Drawing.Font FontField { get; set; }
	}
}
