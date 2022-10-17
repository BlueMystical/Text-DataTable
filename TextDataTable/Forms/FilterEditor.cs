using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace TextDataTable.Forms
{
	public partial class FilterEditor : Form
	{
		#region Public & Private Properties

		private List<Column> Fields { get; set; }
		public string JSON_Expression { get; set; }
		private List<dynamic> DataSource { get; set; }
		public dynamic OriginalData { get; set; }		
		public List<FilterCriteria> Criteria { get; set; }

		/// <summary>Temporary holds the Data for Filtering purposes.</summary>
		private Newtonsoft.Json.Linq.JArray JSonData = null;
		private bool IsLoading = false;

		#endregion

		#region Form Constructors

		public FilterEditor(List<Column> pFields, dynamic pOriginalData)
		{
			InitializeComponent();
			this.Fields = pFields;
			this.OriginalData = pOriginalData;
		}

		private void FilterEditor_Load(object sender, EventArgs e)
		{
			try
			{
				if (this.Fields != null && this.Fields.Count > 0)
				{
					this.cboFields.Items.Clear();
					foreach (var Field in this.Fields)
					{
						this.cboFields.Items.Add(Field.field);
					}
				}

				ListCriteria(0);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
		private void FilterEditor_Shown(object sender, EventArgs e)
		{

		}

		#endregion

		#region Public & Private Methods

		public void LoadData()
		{
			try
			{
				if (this.OriginalData != null && this.OriginalData.Count > 0)
				{
					this.txtExpression.Text = BuildExpression();
					this.DataSource = FilterData(this.txtExpression.Text);
					this.dataGridView1.DataSource = this.DataSource;				

					this.lblRecordCounter.Text = string.Format("{0:n0} Records", this.DataSource.Count);					
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
		private void ListCriteria(int Sel_Index)
		{
			try
			{
				this.checkedListBox1.Items.Clear();
				if (this.Criteria != null && this.Criteria.Count > 0)
				{
					IsLoading = true;
					int EnabledFilters = 0;
					foreach (var Filter in Criteria)
					{
						this.checkedListBox1.Items.Add(Filter.ToString(true), Filter.enabled);
						if (Filter.enabled) EnabledFilters++;
					}

					if (Sel_Index >= 0) this.checkedListBox1.SelectedIndex = Sel_Index;

					this.lblFilterCounter.Text = string.Format("{0} Filters", EnabledFilters);
				}
				else
				{
					this.lblFilterCounter.Text = "0 Filters";
				}

				LoadData();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally { IsLoading = false; }
		}
		private void ShowCriteria(FilterCriteria Filter)
		{
			try
			{
				if (Filter != null)
				{
					cboOperator.Text = (Filter.isOR ? "OR" : "AND");
					cboFields.Text = Filter.field;
					cboComparator.Text = Filter.comparator;
					txtValue.Text = Filter.value.ToString();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
		private FilterCriteria GetCriteria()
		{
			FilterCriteria Filter = new FilterCriteria();
			try
			{
				Filter.isAND = (cboOperator.Text == "AND");
				Filter.isOR = (cboOperator.Text == "OR");
				Filter.comparator = cboComparator.Text;
				Filter.field = cboFields.Text;				

				object Value = null;
				var Column = Fields.Find(x => x.field == Filter.field);
				if (Column != null)
				{
					try
					{
						switch (Column.type) //string,int,long,decimal,DateTime,boolean, Calculated
						{
							case "string": Value = Convert.ToString(txtValue.Text); break;
							case "int": Value = Convert.ToInt32(txtValue.Text); break;
							case "long": Value = Convert.ToInt64(txtValue.Text); break;
							case "decimal": Value = Convert.ToDecimal(txtValue.Text); break;
							case "DateTime": Value = Convert.ToDateTime(txtValue.Text); break;
							case "boolean": Value = Convert.ToBoolean(txtValue.Text); break;
							default:
								Value = Convert.ToString(txtValue.Text); break;
						}
					}
					catch {
						//Ignore type Parse Errors
						Value = Convert.ToString(txtValue.Text);
					} 
				}
				Filter.value = Value;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return Filter;
		}
		private string BuildExpression()
		{
			string _ret = string.Empty;
			try
			{
				if (this.Criteria != null && this.Criteria.Count > 0)
				{
					int Index = 0;
					this.JSON_Expression = string.Empty;
					foreach (var Filter in Criteria)
					{
						if (Filter.enabled)
						{
							Filter.isFirst = (Index == 0);
							this.JSON_Expression += Filter.ToString();
							Index++;
						}						
					}

					this.JSON_Expression = (this.JSON_Expression != string.Empty) ? string.Format("$[?({0})]", this.JSON_Expression) : string.Empty;					
				}
				else
				{
					this.JSON_Expression = string.Empty;
					this.lblFilterCounter.Text = "0 Filters";
				}
				_ret = this.JSON_Expression;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return _ret;
		}

		/// <summary>Filters the data using JSONPath expressions.</summary>
		/// <param name="Criteria">JSONPath expression to Filter the data.</param>
		/// <param name="Apply">If true, the Filtered data becomes the Data to show on the Table. Need to call the 'Build' table method.</param>
		public List<dynamic> FilterData(string Criteria)
		{
			List<dynamic> _ret = null;
			try
			{
				if (Criteria != null && Criteria != string.Empty)
				{
					/* WE ARE USING 'JSONPath' TO DO THE FILTERING because i was unable to make it work with Linq */
					//https://www.newtonsoft.com/json/help/html/SelectToken.htm
					//https://www.newtonsoft.com/json/help/html/QueryJsonSelectTokenJsonPath.htm
					//https://goessner.net/articles/JsonPath/
					//https://docs.hevodata.com/sources/streaming/rest-api/writing-jsonpath-expressions/

					//Examples:
					//$[?(@.system_name == 'HR 1201')]
					//$[?(@.scoop != false)]
					//$[?(@.jumps >= 2)]
					//$[?(@.startDate >= '2022-01-30T00:00:00-03:00')]		//<- DateTime Comparations
					//$[?(@.system_name == 'HR 1201' && @.scoop == false)]  //<-- AND
					//$[?(@.system_name == 'HR 1201' || @.scoop == false)]  //<-- OR

					//$[?(@.scoop == False || @.jump_len >= 0.0)]
					//$[?(@.firstName =~ /vi.*?/i)]			//<- matches elements whose firstName starts with vi(case-insensitive).

					if (OriginalData != null && OriginalData.Count > 0)
					{
						//1. Convert the Data into a JSon Array (JArray):
						if (JSonData == null)
						{
							JSonData = Newtonsoft.Json.Linq.JArray.FromObject(
								Newtonsoft.Json.JsonConvert.DeserializeObject(
									Newtonsoft.Json.JsonConvert.SerializeObject(OriginalData)
								)
							);
						}

						//2. Use a JsonPath to get the data filtered:
						IEnumerable<Newtonsoft.Json.Linq.JToken> JSONPath_Results = JSonData.SelectTokens(Criteria);
						var FilteredData = JSONPath_Results.ToList();

						//3. Convert the Filtered Data into a List of Dynamic Objects:
						if (FilteredData != null && FilteredData.Count > 0)
						{
							_ret = new List<dynamic>();
							foreach (Newtonsoft.Json.Linq.JToken item in FilteredData.ToList())
							{
								_ret.Add(item.ToObject<dynamic>());
							}
						}
						else
						{
							//No Data Found for the Filter, Returns an Empty (not Null) List:
							_ret = new List<dynamic>();
						}
					}
					else
					{
						throw new Exception("Eror 404 - No Data Loaded on the Table.");
					}
				}
				else
				{
					//Si 'Criteria' está vacio, devuelve todos los Registros:
					if (OriginalData != null && OriginalData.Count > 0)
					{
						if (JSonData == null)
						{
							JSonData = Newtonsoft.Json.Linq.JArray.FromObject(
								Newtonsoft.Json.JsonConvert.DeserializeObject(
									Newtonsoft.Json.JsonConvert.SerializeObject(OriginalData)
								)
							);
						}
					}
					_ret = JSonData.ToObject<List<dynamic>>();
				}

			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return _ret;
		}

		#endregion 

		#region Control Events & Buttons

		private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (sender != null)
			{
				int Sel_Index = ((CheckedListBox)sender).SelectedIndex;
				if (Sel_Index >= 0)
				{
					if (this.Criteria != null && this.Criteria.Count > 0)
					{
						ShowCriteria(this.Criteria[Sel_Index]);
					}
				}
			}
		}
		private void checkedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
		{
			if (!IsLoading && e != null && e.CurrentValue != e.NewValue)
			{
				this.Criteria[e.Index].enabled = (e.NewValue == CheckState.Checked) ? true : false;
				LoadData();
			}

			int EnabledFilters = 0;
			foreach (var filter in this.Criteria)
			{
				if (filter.enabled) EnabledFilters++;
			}
			this.lblFilterCounter.Text = string.Format("{0} Filters", EnabledFilters);
		}

		private void cmdAddFilter_Click(object sender, EventArgs e)
		{
			FilterCriteria Filter = GetCriteria();
			if (Filter != null)
			{
				if (this.Criteria == null) this.Criteria = new List<FilterCriteria>();

				this.Criteria.Add(Filter);
				ListCriteria(this.Criteria.Count-1);
			}
		}
		private void cmdEditFilter_Click(object sender, EventArgs e)
		{
			FilterCriteria Filter = GetCriteria();
			if (Filter != null)
			{
				int Sel_Index = checkedListBox1.SelectedIndex;
				if (Sel_Index >= 0)
				{
					if (Sel_Index == 0) Filter.isFirst = true;
					this.Criteria[Sel_Index] = Filter;

					ListCriteria(Sel_Index);
				}				
			}
		}
		private void cmdRemoveFilter_Click(object sender, EventArgs e)
		{
			int Sel_Index = checkedListBox1.SelectedIndex;
			if (Sel_Index >= 0)
			{
				var Filter = this.Criteria[Sel_Index];
				if (MessageBox.Show("Remove Selected Filter?", "Confirm?", 
					MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
				{
					this.Criteria.RemoveAt(Sel_Index);
					ListCriteria(0);
				}
			}
		}
		private void cmdClearfilters_Click(object sender, EventArgs e)
		{
			if (MessageBox.Show("Remove All Filters?", "Confirm?",
					MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				this.Criteria = new List<FilterCriteria>();
				ListCriteria(-1);				
			}
		}

		private void cmdOK_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
		}
		private void cmdCancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

		#endregion 
	}
}
