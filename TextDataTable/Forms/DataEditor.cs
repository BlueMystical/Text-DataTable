using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace TextDataTable.Forms
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
						ColorField = System.Drawing.Color.FromArgb(128,255,255,0)
					},
					new myCustomData()
					{
						Field1 = 1002,
						Field2 = "Custom Text for Field 2",
						Field3 = 507.2345m,
						Field4 = DateTime.Today.AddDays(-8),
						ColorField = System.Drawing.Color.Brown
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
						ColorField = System.Drawing.Color.BlueViolet
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
						ColorField = System.Drawing.Color.BlueViolet
					}
				});

				this.CustomDataSet = true;
				this.MyTableConfiguration.data = CustomData;
				this.dataGridView.DataSource = this.MyTableConfiguration.data;

				//Since We completely changed the DataSet, now We need to Update the Column Definitions:
				this.MyTableConfiguration.columns = new List<Column>(new Column[] {
					new Column()
					{
						title = "Field 1",
						field = "Field1",
						type = "int",
						width = 100,
						length = 5,
						format = "{0:n0}"
					},
					new Column()
					{
						title = "Field 2",
						field = "Field2",
						type = "string",
						width = 250,
						length = 25
					},
					new Column()
					{
						title = "Field 3",
						field = "Field3",
						type = "decimal",
						width = 150,
						length = 8,
						align = "right",
						format = "{0:C2}"
					},
					new Column()
					{
						title = "Field 4",
						field = "Field4",
						type = "DateTime",
						width = 100,
						length = 8,
						align = "center",
						format = "{0:d}"
					},
					new Column()
					{
						title = "Color",
						field = "ColorField",
						type = "Color",
						width = 400,
						length = 28,
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
					new Summary()
					{
						field = "Field3",
						agregate = "SUM",
						format = "{0:n2}"
					},
					new Summary()
					{
						field = "Field4",
						agregate = "COUNT",
						format = "{0} days"
					}
				};

				/* FOR THIS EXAMPLE, WE ALSO GOING TO IGNORE THE FOOTER */
				this.MyTableConfiguration.footer = null;
				this.MyTableConfiguration.header.title = "Custom DataSet on the Table";

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
						"Field2"
					})
				};
			}
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
	}
}
