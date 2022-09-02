using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using TextDataTable.Forms;

namespace TextDataTable
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
			this.Criteria = new List<FilterCriteria>(
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
			myTable = new TextDataTable(@"Data\table_config.json"); //<- File is in the bin folder

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
			var PixelInfo = myTable.GetPixelInfo(new Point(279,160));
			if (PixelInfo != null)
			{
				Console.WriteLine(string.Format("You Clicked on a '{0}' who has the '{1}' value.", 
					PixelInfo.ElementType.ToString(), PixelInfo.CellText));

				if (PixelInfo.ElementType == TableElements.DATA_ROW)
				{
					dynamic data = myTable.OriginalData[(dynamic)PixelInfo.Row];
					Console.WriteLine(data[((Column)PixelInfo.Column).field]);
					data[((Column)PixelInfo.Column).field] = 40.4m;

					myTable.OriginalData[PixelInfo.col_index] = data;
				}
			}
		}

		private void cmdDataEditor_Click(object sender, EventArgs e)
		{
			//Example of Data Customization and Table Configuration: See inside the Form
			Forms.DataEditor Form = new Forms.DataEditor(myTable.TConfiguration);
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
			FilterEditor Form = new FilterEditor(myTable.TConfiguration.columns, myTable.OriginalData);
			Form.Criteria = this.Criteria; //<- If we already have a filter then we use it

			if (Form.ShowDialog() == DialogResult.OK)
			{
				this.Criteria = Form.Criteria;

				var Data = myTable.FilterData(Criteria, true);

				textBox1.Text = myTable.Build_TextDataTable();
			}
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
	}
}
