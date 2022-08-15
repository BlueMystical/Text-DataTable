using System;
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
			this.myTable = new TextDataTable(@"Data\table_config.json"); //<- File is in the bin folder
			this.propertyGrid1.SelectedObject = this.myTable.TConfiguration;
		}

		private void cmdTextTable_Click(object sender, EventArgs e)
		{
			//STEP 3: BUILD THE DATATABLE:
			this.textBox1.Text = this.myTable.Build_TextDataTable();
		}

		private void cmdImageTable_Click(object sender, EventArgs e)
		{
			//YOU CAN ALSO BUILD IT AS A PNG IMAGE:
			System.Drawing.Bitmap myImageTable = this.myTable.Build_ImageDataTable(new Size(800, 450));
			myImageTable.Save(@"C:\Temp\myImage.png", System.Drawing.Imaging.ImageFormat.Png);

			System.Diagnostics.Process.Start(@"C:\Temp\myImage.png");
		}

		private void cmdDataEditor_Click(object sender, EventArgs e)
		{
			Forms.DataEditor Form = new Forms.DataEditor(myTable.TConfiguration);
			if (Form.ShowDialog() == DialogResult.OK)
			{
				myTable.TConfiguration = Form.MyTableConfiguration;
			}
		}
	}
}
