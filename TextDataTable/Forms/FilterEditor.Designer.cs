namespace TextDataTable.Forms
{
	partial class FilterEditor
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.cmdEditFilter = new System.Windows.Forms.Button();
			this.cmdAddFilter = new System.Windows.Forms.Button();
			this.cboOperator = new System.Windows.Forms.ComboBox();
			this.label3 = new System.Windows.Forms.Label();
			this.txtValue = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.cboComparator = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.cmdRemoveFilter = new System.Windows.Forms.Button();
			this.cboFields = new System.Windows.Forms.ComboBox();
			this.cmdClearfilters = new System.Windows.Forms.Button();
			this.cmdOK = new System.Windows.Forms.Button();
			this.cmdCancel = new System.Windows.Forms.Button();
			this.txtExpression = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.dataGridView1 = new System.Windows.Forms.DataGridView();
			this.checkedListBox1 = new System.Windows.Forms.CheckedListBox();
			this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this.lblRecordCounter = new System.Windows.Forms.ToolStripStatusLabel();
			this.lblFilterCounter = new System.Windows.Forms.ToolStripStatusLabel();
			this.groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
			this.statusStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.cmdEditFilter);
			this.groupBox1.Controls.Add(this.cmdAddFilter);
			this.groupBox1.Controls.Add(this.cboOperator);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.txtValue);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.cboComparator);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.cmdRemoveFilter);
			this.groupBox1.Controls.Add(this.cboFields);
			this.groupBox1.Location = new System.Drawing.Point(236, 8);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(386, 95);
			this.groupBox1.TabIndex = 1;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Filter Criteria:";
			// 
			// cmdEditFilter
			// 
			this.cmdEditFilter.Location = new System.Drawing.Point(215, 67);
			this.cmdEditFilter.Name = "cmdEditFilter";
			this.cmdEditFilter.Size = new System.Drawing.Size(75, 23);
			this.cmdEditFilter.TabIndex = 8;
			this.cmdEditFilter.Text = "Edit Filter";
			this.cmdEditFilter.UseVisualStyleBackColor = true;
			this.cmdEditFilter.Click += new System.EventHandler(this.cmdEditFilter_Click);
			// 
			// cmdAddFilter
			// 
			this.cmdAddFilter.Location = new System.Drawing.Point(134, 67);
			this.cmdAddFilter.Name = "cmdAddFilter";
			this.cmdAddFilter.Size = new System.Drawing.Size(75, 23);
			this.cmdAddFilter.TabIndex = 7;
			this.cmdAddFilter.Text = "Add Filter";
			this.cmdAddFilter.UseVisualStyleBackColor = true;
			this.cmdAddFilter.Click += new System.EventHandler(this.cmdAddFilter_Click);
			// 
			// cboOperator
			// 
			this.cboOperator.FormattingEnabled = true;
			this.cboOperator.Items.AddRange(new object[] {
            "AND",
            "OR"});
			this.cboOperator.Location = new System.Drawing.Point(7, 40);
			this.cboOperator.Name = "cboOperator";
			this.cboOperator.Size = new System.Drawing.Size(53, 21);
			this.cboOperator.TabIndex = 6;
			this.cboOperator.Text = "AND";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(293, 24);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(37, 13);
			this.label3.TabIndex = 5;
			this.label3.Text = "Value:";
			// 
			// txtValue
			// 
			this.txtValue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtValue.Location = new System.Drawing.Point(267, 40);
			this.txtValue.Name = "txtValue";
			this.txtValue.Size = new System.Drawing.Size(113, 20);
			this.txtValue.TabIndex = 4;
			this.txtValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(176, 24);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(52, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "Compare:";
			// 
			// cboComparator
			// 
			this.cboComparator.FormattingEnabled = true;
			this.cboComparator.Items.AddRange(new object[] {
            "==",
            "!=",
            "<",
            "<=",
            ">",
            ">=",
            "START",
            "END",
            "CONTAINS"});
			this.cboComparator.Location = new System.Drawing.Point(179, 40);
			this.cboComparator.Name = "cboComparator";
			this.cboComparator.Size = new System.Drawing.Size(82, 21);
			this.cboComparator.TabIndex = 2;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(95, 24);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(32, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "Field:";
			// 
			// cmdRemoveFilter
			// 
			this.cmdRemoveFilter.Location = new System.Drawing.Point(296, 67);
			this.cmdRemoveFilter.Name = "cmdRemoveFilter";
			this.cmdRemoveFilter.Size = new System.Drawing.Size(75, 23);
			this.cmdRemoveFilter.TabIndex = 8;
			this.cmdRemoveFilter.Text = "Remove Filter";
			this.cmdRemoveFilter.UseVisualStyleBackColor = true;
			this.cmdRemoveFilter.Click += new System.EventHandler(this.cmdRemoveFilter_Click);
			// 
			// cboFields
			// 
			this.cboFields.FormattingEnabled = true;
			this.cboFields.Location = new System.Drawing.Point(66, 40);
			this.cboFields.Name = "cboFields";
			this.cboFields.Size = new System.Drawing.Size(107, 21);
			this.cboFields.TabIndex = 0;
			// 
			// cmdClearfilters
			// 
			this.cmdClearfilters.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdClearfilters.Location = new System.Drawing.Point(547, 137);
			this.cmdClearfilters.Name = "cmdClearfilters";
			this.cmdClearfilters.Size = new System.Drawing.Size(75, 23);
			this.cmdClearfilters.TabIndex = 9;
			this.cmdClearfilters.Text = "Clear all";
			this.cmdClearfilters.UseVisualStyleBackColor = true;
			this.cmdClearfilters.Click += new System.EventHandler(this.cmdClearfilters_Click);
			// 
			// cmdOK
			// 
			this.cmdOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdOK.Location = new System.Drawing.Point(547, 191);
			this.cmdOK.Name = "cmdOK";
			this.cmdOK.Size = new System.Drawing.Size(75, 23);
			this.cmdOK.TabIndex = 2;
			this.cmdOK.Text = "Apply Filters";
			this.cmdOK.UseVisualStyleBackColor = true;
			this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
			// 
			// cmdCancel
			// 
			this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cmdCancel.Location = new System.Drawing.Point(547, 164);
			this.cmdCancel.Name = "cmdCancel";
			this.cmdCancel.Size = new System.Drawing.Size(75, 23);
			this.cmdCancel.TabIndex = 3;
			this.cmdCancel.Text = "Cancel";
			this.cmdCancel.UseVisualStyleBackColor = true;
			this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
			// 
			// txtExpression
			// 
			this.txtExpression.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtExpression.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtExpression.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtExpression.Location = new System.Drawing.Point(7, 138);
			this.txtExpression.Multiline = true;
			this.txtExpression.Name = "txtExpression";
			this.txtExpression.Size = new System.Drawing.Size(537, 76);
			this.txtExpression.TabIndex = 10;
			this.txtExpression.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// label4
			// 
			this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(319, 120);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(161, 13);
			this.label4.TabIndex = 11;
			this.label4.Text = "Complete JSONPath Expression:";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(12, 8);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(93, 13);
			this.label5.TabIndex = 12;
			this.label5.Text = "JSON Path Filters:";
			// 
			// dataGridView1
			// 
			this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridView1.Location = new System.Drawing.Point(7, 224);
			this.dataGridView1.Name = "dataGridView1";
			this.dataGridView1.Size = new System.Drawing.Size(615, 161);
			this.dataGridView1.TabIndex = 13;
			// 
			// checkedListBox1
			// 
			this.checkedListBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.checkedListBox1.FormattingEnabled = true;
			this.checkedListBox1.Location = new System.Drawing.Point(8, 24);
			this.checkedListBox1.Name = "checkedListBox1";
			this.checkedListBox1.Size = new System.Drawing.Size(222, 109);
			this.checkedListBox1.TabIndex = 14;
			this.checkedListBox1.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.checkedListBox1_ItemCheck);
			this.checkedListBox1.SelectedIndexChanged += new System.EventHandler(this.checkedListBox1_SelectedIndexChanged);
			// 
			// statusStrip1
			// 
			this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblFilterCounter,
            this.lblRecordCounter});
			this.statusStrip1.Location = new System.Drawing.Point(0, 390);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new System.Drawing.Size(625, 22);
			this.statusStrip1.TabIndex = 15;
			this.statusStrip1.Text = "statusStrip1";
			// 
			// lblRecordCounter
			// 
			this.lblRecordCounter.Name = "lblRecordCounter";
			this.lblRecordCounter.Size = new System.Drawing.Size(61, 17);
			this.lblRecordCounter.Text = "0 Records.";
			// 
			// lblFilterCounter
			// 
			this.lblFilterCounter.Name = "lblFilterCounter";
			this.lblFilterCounter.Size = new System.Drawing.Size(50, 17);
			this.lblFilterCounter.Text = "0 Filters.";
			// 
			// FilterEditor
			// 
			this.AcceptButton = this.cmdOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cmdCancel;
			this.ClientSize = new System.Drawing.Size(625, 412);
			this.Controls.Add(this.statusStrip1);
			this.Controls.Add(this.checkedListBox1);
			this.Controls.Add(this.dataGridView1);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.txtExpression);
			this.Controls.Add(this.cmdClearfilters);
			this.Controls.Add(this.cmdCancel);
			this.Controls.Add(this.cmdOK);
			this.Controls.Add(this.groupBox1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FilterEditor";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "FilterEditor";
			this.Load += new System.EventHandler(this.FilterEditor_Load);
			this.Shown += new System.EventHandler(this.FilterEditor_Shown);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
			this.statusStrip1.ResumeLayout(false);
			this.statusStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button cmdAddFilter;
		private System.Windows.Forms.ComboBox cboOperator;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox txtValue;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox cboComparator;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox cboFields;
		private System.Windows.Forms.Button cmdRemoveFilter;
		private System.Windows.Forms.Button cmdClearfilters;
		private System.Windows.Forms.Button cmdOK;
		private System.Windows.Forms.Button cmdCancel;
		private System.Windows.Forms.TextBox txtExpression;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Button cmdEditFilter;
		private System.Windows.Forms.DataGridView dataGridView1;
		private System.Windows.Forms.CheckedListBox checkedListBox1;
		private System.Windows.Forms.StatusStrip statusStrip1;
		private System.Windows.Forms.ToolStripStatusLabel lblFilterCounter;
		private System.Windows.Forms.ToolStripStatusLabel lblRecordCounter;
	}
}