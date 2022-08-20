using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace TextDataTable
{
	public partial class Form1 : Form
	{
		/// <summary>Class able to produce DataTables in Text Mode.
		/// Author: Jhollman Chacon (Blue Mystic) - 2022</summary>
		private TextDataTable myTable; //<- STEP 1:  DECLARE THE CLASS

		public Form1()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{

		}
		private void Form1_Shown(object sender, EventArgs e)
		{
			//STEP 2:  LOAD THE CONFIGURATION
			myTable = new TextDataTable(@"Data\table_config.json"); //<- File is in the bin folder

			//The Data is a property of the Configuration Object: 'this.myTable.TConfiguration.data'

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
		}

		private void cmdDataEditor_Click(object sender, EventArgs e)
		{
			//Example of Data Customization and Table Configuration: See inside the Form
			Forms.DataEditor Form = new Forms.DataEditor(myTable.TConfiguration);
			if (Form.ShowDialog() == DialogResult.OK)
			{
				myTable.TConfiguration = Form.MyTableConfiguration;
				textBox1.Text = myTable.Build_TextDataTable();
			}
		}

		public void AddFilter()
		{
			try
			{
				//Operators:  <, >, ==, <=, >=, !=, IN(1,2,..)

				List<dynamic> Criteria = new List<dynamic>(
					new dynamic[] {
					new
					{
						field = "field1",
						value = 1234,
						comparator = ">="
					},
					new
					{
						field = "field2",
						value = 666,
						comparator = "!="
					},
					new
					{
						field = "field3",
						value = 666,
						comparator = "=="
					}
				});

				var Data = myTable.FilterData(Criteria);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void button1_Click(object sender, EventArgs e)
		{
			List<dynamic> Criteria = new List<dynamic>(
					new dynamic[] {
					new
					{
						field = "scoop",
						value = false,
						comparator = "=="
					}
				});

			var Data = myTable.FilterData(Criteria);
		}
	}
}
