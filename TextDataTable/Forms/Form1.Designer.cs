namespace TextDataTable
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
			this.cmdDataEditor = new System.Windows.Forms.Button();
			this.cmdImageTable = new System.Windows.Forms.Button();
			this.cmdTextTable = new System.Windows.Forms.Button();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
			this.panel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.SuspendLayout();
			// 
			// textBox1
			// 
			this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.textBox1.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.textBox1.Location = new System.Drawing.Point(0, 0);
			this.textBox1.Multiline = true;
			this.textBox1.Name = "textBox1";
			this.textBox1.Size = new System.Drawing.Size(639, 411);
			this.textBox1.TabIndex = 0;
			this.textBox1.Text = resources.GetString("textBox1.Text");
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.cmdDataEditor);
			this.panel1.Controls.Add(this.cmdImageTable);
			this.panel1.Controls.Add(this.cmdTextTable);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel1.Location = new System.Drawing.Point(0, 411);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(1004, 39);
			this.panel1.TabIndex = 1;
			// 
			// cmdDataEditor
			// 
			this.cmdDataEditor.Location = new System.Drawing.Point(260, 7);
			this.cmdDataEditor.Name = "cmdDataEditor";
			this.cmdDataEditor.Size = new System.Drawing.Size(75, 23);
			this.cmdDataEditor.TabIndex = 2;
			this.cmdDataEditor.Text = "Edit Data";
			this.cmdDataEditor.UseVisualStyleBackColor = true;
			this.cmdDataEditor.Click += new System.EventHandler(this.cmdDataEditor_Click);
			// 
			// cmdImageTable
			// 
			this.cmdImageTable.Location = new System.Drawing.Point(132, 7);
			this.cmdImageTable.Name = "cmdImageTable";
			this.cmdImageTable.Size = new System.Drawing.Size(121, 23);
			this.cmdImageTable.TabIndex = 1;
			this.cmdImageTable.Text = "Create Image Table";
			this.cmdImageTable.UseVisualStyleBackColor = true;
			this.cmdImageTable.Click += new System.EventHandler(this.cmdImageTable_Click);
			// 
			// cmdTextTable
			// 
			this.cmdTextTable.Location = new System.Drawing.Point(13, 7);
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
			this.splitContainer1.Size = new System.Drawing.Size(1004, 411);
			this.splitContainer1.SplitterDistance = 639;
			this.splitContainer1.TabIndex = 2;
			// 
			// propertyGrid1
			// 
			this.propertyGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.propertyGrid1.Location = new System.Drawing.Point(0, 0);
			this.propertyGrid1.Name = "propertyGrid1";
			this.propertyGrid1.Size = new System.Drawing.Size(361, 411);
			this.propertyGrid1.TabIndex = 0;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1004, 450);
			this.Controls.Add(this.splitContainer1);
			this.Controls.Add(this.panel1);
			this.Name = "Form1";
			this.Text = "Blue\'s Text DataTable ";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.Shown += new System.EventHandler(this.Form1_Shown);
			this.panel1.ResumeLayout(false);
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
	}
}

