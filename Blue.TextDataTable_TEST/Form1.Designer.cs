namespace Blue.TextDataTable.TEST
{
	partial class Form1
	{
		/// <summary>
		/// Variable del diseñador necesaria.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Limpiar los recursos que se estén usando.
		/// </summary>
		/// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Código generado por el Diseñador de Windows Forms

		/// <summary>
		/// Método necesario para admitir el Diseñador. No se puede modificar
		/// el contenido de este método con el editor de código.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.panel1 = new System.Windows.Forms.Panel();
			this.cmdQuickSearch = new System.Windows.Forms.Button();
			this.textBox2 = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.cmdFilterEditor = new System.Windows.Forms.Button();
			this.cmdUndoFilter = new System.Windows.Forms.Button();
			this.cmdDoFilter = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.imgSize_H = new System.Windows.Forms.NumericUpDown();
			this.imgSize_W = new System.Windows.Forms.NumericUpDown();
			this.cmdDataEditor = new System.Windows.Forms.Button();
			this.cmdImageTable = new System.Windows.Forms.Button();
			this.cmdTextTable = new System.Windows.Forms.Button();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
			this.panel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.imgSize_H)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.imgSize_W)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.SuspendLayout();
			// 
			// textBox1
			// 
			this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.textBox1.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.textBox1.Location = new System.Drawing.Point(0, 0);
			this.textBox1.Multiline = true;
			this.textBox1.Name = "textBox1";
			this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.textBox1.Size = new System.Drawing.Size(890, 544);
			this.textBox1.TabIndex = 0;
			this.textBox1.Text = resources.GetString("textBox1.Text");
			this.textBox1.WordWrap = false;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.cmdQuickSearch);
			this.panel1.Controls.Add(this.textBox2);
			this.panel1.Controls.Add(this.label3);
			this.panel1.Controls.Add(this.cmdFilterEditor);
			this.panel1.Controls.Add(this.cmdUndoFilter);
			this.panel1.Controls.Add(this.cmdDoFilter);
			this.panel1.Controls.Add(this.label2);
			this.panel1.Controls.Add(this.label1);
			this.panel1.Controls.Add(this.imgSize_H);
			this.panel1.Controls.Add(this.imgSize_W);
			this.panel1.Controls.Add(this.cmdDataEditor);
			this.panel1.Controls.Add(this.cmdImageTable);
			this.panel1.Controls.Add(this.cmdTextTable);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel1.Location = new System.Drawing.Point(0, 544);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(1230, 39);
			this.panel1.TabIndex = 1;
			// 
			// cmdQuickSearch
			// 
			this.cmdQuickSearch.Location = new System.Drawing.Point(907, 9);
			this.cmdQuickSearch.Name = "cmdQuickSearch";
			this.cmdQuickSearch.Size = new System.Drawing.Size(31, 23);
			this.cmdQuickSearch.TabIndex = 12;
			this.cmdQuickSearch.Text = "Go";
			this.cmdQuickSearch.UseVisualStyleBackColor = true;
			this.cmdQuickSearch.Click += new System.EventHandler(this.cmdQuickSearch_Click);
			// 
			// textBox2
			// 
			this.textBox2.Location = new System.Drawing.Point(766, 10);
			this.textBox2.Name = "textBox2";
			this.textBox2.Size = new System.Drawing.Size(147, 20);
			this.textBox2.TabIndex = 11;
			this.textBox2.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox2_KeyPress);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(692, 14);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(75, 13);
			this.label3.TabIndex = 10;
			this.label3.Text = "Quick Search:";
			// 
			// cmdFilterEditor
			// 
			this.cmdFilterEditor.Location = new System.Drawing.Point(448, 9);
			this.cmdFilterEditor.Name = "cmdFilterEditor";
			this.cmdFilterEditor.Size = new System.Drawing.Size(75, 23);
			this.cmdFilterEditor.TabIndex = 9;
			this.cmdFilterEditor.Text = "Filter Editor";
			this.cmdFilterEditor.UseVisualStyleBackColor = true;
			this.cmdFilterEditor.Click += new System.EventHandler(this.cmdFilterEditor_Click);
			// 
			// cmdUndoFilter
			// 
			this.cmdUndoFilter.Location = new System.Drawing.Point(610, 9);
			this.cmdUndoFilter.Name = "cmdUndoFilter";
			this.cmdUndoFilter.Size = new System.Drawing.Size(75, 23);
			this.cmdUndoFilter.TabIndex = 8;
			this.cmdUndoFilter.Text = "Cancel Filter";
			this.cmdUndoFilter.UseVisualStyleBackColor = true;
			this.cmdUndoFilter.Click += new System.EventHandler(this.cmdUndoFilter_Click);
			// 
			// cmdDoFilter
			// 
			this.cmdDoFilter.Location = new System.Drawing.Point(529, 9);
			this.cmdDoFilter.Name = "cmdDoFilter";
			this.cmdDoFilter.Size = new System.Drawing.Size(75, 23);
			this.cmdDoFilter.TabIndex = 7;
			this.cmdDoFilter.Text = "Apply Filter";
			this.cmdDoFilter.UseVisualStyleBackColor = true;
			this.cmdDoFilter.Click += new System.EventHandler(this.button1_Click);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(369, 14);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(12, 13);
			this.label2.TabIndex = 6;
			this.label2.Text = "x";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(260, 14);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(50, 13);
			this.label1.TabIndex = 5;
			this.label1.Text = "Img.Size:";
			// 
			// imgSize_H
			// 
			this.imgSize_H.Location = new System.Drawing.Point(382, 10);
			this.imgSize_H.Maximum = new decimal(new int[] {
            2000,
            0,
            0,
            0});
			this.imgSize_H.Name = "imgSize_H";
			this.imgSize_H.Size = new System.Drawing.Size(48, 20);
			this.imgSize_H.TabIndex = 4;
			this.imgSize_H.Value = new decimal(new int[] {
            500,
            0,
            0,
            0});
			// 
			// imgSize_W
			// 
			this.imgSize_W.Location = new System.Drawing.Point(317, 10);
			this.imgSize_W.Maximum = new decimal(new int[] {
            2000,
            0,
            0,
            0});
			this.imgSize_W.Name = "imgSize_W";
			this.imgSize_W.Size = new System.Drawing.Size(46, 20);
			this.imgSize_W.TabIndex = 3;
			this.imgSize_W.Value = new decimal(new int[] {
            800,
            0,
            0,
            0});
			// 
			// cmdDataEditor
			// 
			this.cmdDataEditor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdDataEditor.Location = new System.Drawing.Point(1143, 7);
			this.cmdDataEditor.Name = "cmdDataEditor";
			this.cmdDataEditor.Size = new System.Drawing.Size(75, 23);
			this.cmdDataEditor.TabIndex = 2;
			this.cmdDataEditor.Text = "Edit Data";
			this.cmdDataEditor.UseVisualStyleBackColor = true;
			this.cmdDataEditor.Click += new System.EventHandler(this.cmdDataEditor_Click);
			// 
			// cmdImageTable
			// 
			this.cmdImageTable.Location = new System.Drawing.Point(132, 9);
			this.cmdImageTable.Name = "cmdImageTable";
			this.cmdImageTable.Size = new System.Drawing.Size(121, 23);
			this.cmdImageTable.TabIndex = 1;
			this.cmdImageTable.Text = "Create Image Table";
			this.cmdImageTable.UseVisualStyleBackColor = true;
			this.cmdImageTable.Click += new System.EventHandler(this.cmdImageTable_Click);
			// 
			// cmdTextTable
			// 
			this.cmdTextTable.Location = new System.Drawing.Point(13, 9);
			this.cmdTextTable.Name = "cmdTextTable";
			this.cmdTextTable.Size = new System.Drawing.Size(112, 23);
			this.cmdTextTable.TabIndex = 0;
			this.cmdTextTable.Text = "Create Text Table";
			this.cmdTextTable.UseVisualStyleBackColor = true;
			this.cmdTextTable.Click += new System.EventHandler(this.cmdTextTable_Click);
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = new System.Drawing.Point(0, 0);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.textBox1);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.propertyGrid1);
			this.splitContainer1.Size = new System.Drawing.Size(1230, 544);
			this.splitContainer1.SplitterDistance = 890;
			this.splitContainer1.TabIndex = 2;
			// 
			// propertyGrid1
			// 
			this.propertyGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.propertyGrid1.Location = new System.Drawing.Point(0, 0);
			this.propertyGrid1.Name = "propertyGrid1";
			this.propertyGrid1.Size = new System.Drawing.Size(336, 544);
			this.propertyGrid1.TabIndex = 0;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1230, 583);
			this.Controls.Add(this.splitContainer1);
			this.Controls.Add(this.panel1);
			this.Name = "Form1";
			this.Text = "Blue\'s Text DataTable ";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.Shown += new System.EventHandler(this.Form1_Shown);
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.imgSize_H)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.imgSize_W)).EndInit();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel1.PerformLayout();
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.PropertyGrid propertyGrid1;
		private System.Windows.Forms.Button cmdDataEditor;
		private System.Windows.Forms.Button cmdImageTable;
		private System.Windows.Forms.Button cmdTextTable;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.NumericUpDown imgSize_H;
		private System.Windows.Forms.NumericUpDown imgSize_W;
		private System.Windows.Forms.Button cmdDoFilter;
		private System.Windows.Forms.Button cmdUndoFilter;
		private System.Windows.Forms.Button cmdFilterEditor;
		private System.Windows.Forms.Button cmdQuickSearch;
		private System.Windows.Forms.TextBox textBox2;
		private System.Windows.Forms.Label label3;
	}
}

