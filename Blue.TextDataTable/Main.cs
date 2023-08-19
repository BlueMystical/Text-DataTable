using Blue.TextDataTable.Forms;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Blue.TextDataTable
{
	/// <summary>Class able to produce DataTables in Text Mode.
	/// Author: Jhollman Chacon (Blue Mystic) - 2022</summary>
	public class TextDataTable
	{
		public TextDataTable()
		{		

		}
		public TextDataTable(string pConfigJsonFile)
		{
			ConfigJsonFile = pConfigJsonFile;
			LoadConfiguration();
		}

		public event EventHandler<MyEventArgs> OnTableDataChanged;//<- Declaracion del Evento:

		#region Public Properties

		/// <summary>Full Path to the JSON Config file.</summary>
		public string ConfigJsonFile { get; set; }

		public TableConfiguration TConfiguration { get; set; }

		/// <summary>Set or Return the Data Shown in the Table.</summary>
		public dynamic DataSource { get; set; }

		/// <summary>Set or Return the Un-Sorted, un-grouped Original Data.
		/// <para>Use 'RefreshData()' method to Restore the Shown Data to it's Original.</para>
		/// <para>Use 'RefreshData(newData)' to set a new DataSet</para></summary>
		public dynamic OriginalData { get; set; }

		public List<dynamic> WorkingData { get; set; }

		/// <summary>Stores Information about every CellBox in the Table.
		/// <para>Useful for Pixel Mapping over the Image, Call 'GetPixelInfo' method.</para></summary>
		public List<ImageMapping> CellBoxMappings { get; set; }

		/// <summary>JsonPath list of Criteria filters for the current data</summary>
		public List<FilterCriteria> FilterCriteria { get; set; }

		#endregion

		#region Private Fields

		/// <summary>Holds the Data by Groups.</summary>
		private List<GroupData> GroupedData { get; set; }

		/// <summary>Temporary holds the Data for Filtering purposes.</summary>
		private Newtonsoft.Json.Linq.JArray JSonData = null;

		#endregion

		#region Public Methods

		/// <summary>Loads the JSON Configuration File.</summary>
		public void LoadConfiguration()
		{
			try
			{
				if (ConfigJsonFile != null && ConfigJsonFile != string.Empty)
				{
					if (File.Exists(ConfigJsonFile))
					{
						using (TextReader reader = new StreamReader(ConfigJsonFile))
						{
							var fileContents = reader.ReadToEnd(); reader.Close();
							TConfiguration = Newtonsoft.Json.JsonConvert.DeserializeObject<TableConfiguration>(fileContents);
						}
						if (TConfiguration.data != null && TConfiguration.data.Count > 0)
						{
							this.RefreshData(TConfiguration.data); //<- This set the DataSource
						}
					}
					else
					{
						MessageBox.Show("Configuration File Not Found.", "TextDataTable - ERROR 404", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}
				else
				{
					MessageBox.Show("Configuration File Not Found.", "TextDataTable - ERROR 404", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "TextDataTable - ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		/// <summary>Builds the DataTable on a Text String.
		/// <para>[NOTE]: The Font must be Monospaced.</para></summary>
		public string Build_TextDataTable()
		{
			string _ret = string.Empty;
			try
			{
				if (TConfiguration != null)
				{
					#region Config

					//Determina el Tipo de Borde a usar:
					var Borders = (TConfiguration.properties.borders.type == "simple") ?
						TConfiguration.properties.borders.symbols[1] :
						TConfiguration.properties.borders.symbols[0];

					List<int> ColunmPositions = new List<int>(); //<- The Left position (in characters) for each Column.
					int Margin = TConfiguration.properties.table.data_rows.cell_padding;
					StringBuilder Lines = new StringBuilder();
					string CellText = string.Empty;
					int ColumnSize = 0;
					int TableSize = 0; //<- Width in Characters of the Table's Body

					#endregion

					#region Table Header

					//Calculate the Table's Body Width:
					if (TConfiguration.columns != null && TConfiguration.columns.Count > 0)
					{
						foreach (var _Column in TConfiguration.columns)
						{
							TableSize += _Column.length + (Margin * 2) + 1;
						}
						TableSize++;
					}

					if (TConfiguration.header != null)
					{
						CellText = TConfiguration.header.title.ToString();

						//May use the whole row or just the header width
						ColumnSize = (!TConfiguration.header.size.use_whole_row) ?
							TConfiguration.header.size.length :
							TableSize;

						string LINE = string.Empty;

						//Linea Superior:
						LINE = new string(Borders.Top.Border, ColumnSize - 2);
						LINE = string.Format("{0}{1}{2}", Borders.Top.Left, LINE, Borders.Top.Right);
						Lines.AppendLine(
							AlinearTexto(LINE, TableSize, TConfiguration.header.text_align)
						);

						//Lineas Laterales y Texto:
						LINE = AlinearTexto(CellText, ColumnSize - 2, TConfiguration.header.text_align);
						LINE = string.Format("{0}{1}{2}", Borders.Sides.Left, LINE, Borders.Sides.Right);
						Lines.AppendLine(
							AlinearTexto(LINE, TableSize, TConfiguration.header.text_align)
						);

						//Linea Inferior:
						LINE = new string(Borders.Bottom.Border, ColumnSize - 2);
						LINE = string.Format("{0}{1}{2}", Borders.Bottom.Left, LINE, Borders.Bottom.Right);
						Lines.AppendLine(
							AlinearTexto(LINE, TableSize, TConfiguration.header.text_align)
						);
					}

					#endregion

					#region Data Sorting

					//If the DataSorting is Configured and Enabled, here is where it's Applied:
					if (DataSource != null && DataSource.Count > 0)
					{
						if (TConfiguration.sorting != null && TConfiguration.sorting.enabled)
						{
							if (TConfiguration.sorting.fields != null && TConfiguration.sorting.fields.Count > 0)
							{
								DataSource = DynamicSorting(DataSource, TConfiguration.sorting.fields);
							}
						}
					}

					#endregion

					#region Data Grouping

					List<string> GroupColumnHeaders = null; //<- Used only when  'repeat_column_headers' is false.

					if (DataSource != null && DataSource.Count > 0)
					{
						//If the DataGrouping is Configured and Enabled, here is where it is Applied:

						if (TConfiguration.grouping != null && TConfiguration.grouping.enabled)
						{
							if (TConfiguration.grouping.fields != null && TConfiguration.grouping.fields.Count > 0)
							{
								//1. Group the Data:
								GroupedData = DynamicGrouping(DataSource, TConfiguration.grouping.fields);

								//2. Build the Groups and their components:
								if (GroupedData != null && GroupedData.Count > 0)
								{
									bool repeat_column_headers = TConfiguration.grouping.repeat_column_headers;
									bool show_summary = TConfiguration.grouping.show_summary;
									bool show_Count = TConfiguration.grouping.show_count;

									foreach (var Group in GroupedData)
									{
										//ColumnSize = (int)Group.columns[0].length + (Margin * 2);
										ColumnSize = (int)Group.HeaderData.Length + (Margin * 2);

										#region Group Header

										Group.Header = new List<string>();

										CellText = AlinearTexto(Group.HeaderData, ColumnSize);

										//Linea Superior:
										Group.Header.Add(string.Format("{0}{1}{2}",
											 Borders.Top.Left,
											 new string(Borders.Top.Border, ColumnSize),
											 Borders.Top.Right
										));

										//Lineas Laterales y Texto:
										Group.Header.Add(string.Format("{0}{1}{2} {3}",
											Borders.Sides.Left,
											CellText,
											Borders.Sides.Right,
											(show_Count) ? string.Format(TConfiguration.grouping.CountFormat, Group.Count) : string.Empty
										));

										//Linea Inferior:
										Group.Header.Add(string.Format("{0}{1}{2}",
											 Borders.Bottom.Left,
											 new string(Convert.ToChar(Borders.Top.Border), ColumnSize),
											 Borders.Bottom.Right
										));

										#endregion

										#region Group Body

										if (Group.data != null && Group.data.Count > 0)
										{
											Group.Body = new List<string>();

											int DataCount = Group.data.Count;
											for (int RowIndex = -1; RowIndex < DataCount; RowIndex++)
											{
												//La Primera Fila (RowIndex = -1) es el Cabezal
												//Las demas son filas de Datos

												string Cell_Top = string.Empty;
												string Cell_Mid = string.Empty;

												int ColumnIndex = 0;
												foreach (var _Column in TConfiguration.columns)
												{
													ColumnSize = (int)_Column.length + (Margin * 2);
													char CornerL = (char)0; //<-Empty

													//Determina la Esquina a Usar: ┌ ─ ┬ ─ ┐
													if (ColumnIndex == 0)
													{
														//Si es la Primera Columna:
														if (repeat_column_headers)
														{
															CornerL = (RowIndex >= 0) ?
																Borders.Middle.Left :
																Borders.Top.Left;
														}
														else
														{
															CornerL = (RowIndex > 0) ?
																	Borders.Middle.Left :
																	Borders.Top.Left;
														}
													}
													else
													{
														//Si es una Columna del Medio:
														if (repeat_column_headers)
														{
															CornerL = (RowIndex < 0) ?
																Borders.Top.Middle :
																Borders.Middle.Middle;
														}
														else
														{
															CornerL = (RowIndex > 0) ?
																Borders.Middle.Middle :
																Borders.Top.Middle;
														}
													}

													//Obtiene el Texto de la Celda:
													if (RowIndex < 0) //Primera Fila para los Headers:
													{
														//Aplica el Formato a la Celda:
														CellText = AlinearTexto(_Column.title, ColumnSize);
													}
													else //Fila de Datos:
													{
														var RowData = Group.data[RowIndex];
														string FieldName = string.Empty;
														object FieldValue = null;

														//Obtiene el Valor de la Celda:	
														if (_Column.type == "Calculated")
														{
															FieldName = _Column.field;
															FieldValue = GetExpressionValue(_Column.field, _Column, RowData);
														}
														else
														{
															var propertyInfo = System.ComponentModel.TypeDescriptor.
																GetProperties((object)RowData).
																Find(_Column.field, true);

															FieldName = propertyInfo.Name;
															FieldValue = propertyInfo.GetValue(RowData);
														}

														//Aplica el Formato a la Celda:
														CellText = AplicarFormato(FieldValue, _Column);
														CellText = AlinearTexto(CellText, ColumnSize, _Column.align);
													}

													if (ColunmPositions.Count < TConfiguration.columns.Count)
													{
														ColunmPositions.Add(Cell_Top.Length);
													}

													//Linea Superior:
													Cell_Top += string.Format("{0}{1}",
														CornerL,
														new string(Borders.Top.Border, ColumnSize)
													);

													//Lineas Laterales y Texto:
													Cell_Mid += string.Format("{0}{1}",
														Borders.Sides.Left.ToString(),
														CellText
													);

													//si es la ultima columna, agrega el Borde:
													if (ColumnIndex >= TConfiguration.columns.Count - 1)
													{
														if (repeat_column_headers)
														{
															Cell_Top += (RowIndex >= 0) ?
																Convert.ToString(Borders.Middle.Right) :
																Convert.ToString(Borders.Top.Right);
														}
														else
														{
															Cell_Top += (RowIndex > 0) ?
																Convert.ToString(Borders.Middle.Right) :
																Convert.ToString(Borders.Top.Right);
														}
														Cell_Mid += Convert.ToString(Borders.Middle.Border);
													}

													ColumnIndex++;
												}

												// Determinar si se dibujan los Headers
												if (RowIndex == -1)
												{
													if (repeat_column_headers)
													{
														Group.Body.Add(Cell_Top);
														Group.Body.Add(Cell_Mid);
													}
													else
													{
														if (GroupColumnHeaders == null)
														{
															GroupColumnHeaders = new List<string>();
														}

														if (GroupColumnHeaders.Count < 2)
														{
															GroupColumnHeaders.Add(Cell_Top);
															GroupColumnHeaders.Add(Cell_Mid);
														}
													}
												}
												else
												{
													Group.Body.Add(Cell_Top);
													Group.Body.Add(Cell_Mid);
												}
											}

											// Dibujar la Linea del Fondo 
											string Cell_Bottom = Borders.Bottom.Left.ToString();
											foreach (var _Column in TConfiguration.columns)
											{
												ColumnSize = (int)_Column.length + (Margin * 2);
												Cell_Bottom += new string(Convert.ToChar(Borders.Bottom.Border), ColumnSize);
												Cell_Bottom += Convert.ToString(Borders.Bottom.Middle);
											}
											Cell_Bottom = Cell_Bottom.Substring(0, Cell_Bottom.Length - 1);
											Cell_Bottom += Convert.ToString(Borders.Bottom.Right);

											Group.Body.Add(Cell_Bottom);

											if (!repeat_column_headers && GroupColumnHeaders != null)
											{
												if (GroupColumnHeaders.Count < 3)
												{
													GroupColumnHeaders.Add(Cell_Bottom);
												}
											}
										}

										#endregion

										#region Group Summary

										if (show_summary && TConfiguration.summary != null && TConfiguration.summary.Count > 0)
										{
											string Cell_Top = string.Empty;
											string Cell_Mid = string.Empty;
											string Cell_Bot = string.Empty;

											var SummaryData = GetSummaryValues(TConfiguration.summary, Group.data);
											Group.Footer = new List<string>();

											int SumaryIndex = 0;
											foreach (var _Summary in TConfiguration.summary)
											{
												int ColIndex = 0;
												int LeftSpam = 0;
												foreach (var _Column in TConfiguration.columns)
												{
													if (_Column.field == _Summary.field)
													{
														LeftSpam = ColunmPositions[ColIndex];
														ColumnSize = (int)_Column.length + (Margin * 2);
														//Aplica el Formato a la Celda:
														CellText = AplicarFormato(SummaryData[SumaryIndex], _Summary.format.ToString(), _Column.type.ToString()); //Calcular el Valor
														CellText = AlinearTexto(CellText, ColumnSize, _Column.align);
														break;
													}
													ColIndex++;
												}

												//Determina la Esquina a Usar:
												if (SumaryIndex == 0 && TConfiguration.summary.Count == 1) //<- Si es La Unica Columna
												{
													//Linea Superior:
													Cell_Top += string.Format("{0}{1}{2}{3}",
														new string(' ', LeftSpam),
														Borders.Top.Left,
														new string((Borders.Top.Border), ColumnSize),
														Borders.Top.Right
													);
													//Lineas Laterales y Texto:
													Cell_Mid += string.Format("{0}{1}{2}{3}",
														new string(' ', LeftSpam),
														Borders.Sides.Left,
														CellText,
														Borders.Middle.Border
													);
													//Linea Inferior:
													Cell_Bot += string.Format("{0}{1}{2}{3}",
														new string(' ', LeftSpam),
														(Borders.Bottom.Left),
														new string((Borders.Bottom.Border), ColumnSize),
														Borders.Bottom.Right
													);
												}
												else if (SumaryIndex == 0) //<- Si es la Primera Columna (de Varias):
												{
													//Linea Superior:
													Cell_Top += string.Format("{0}{1}{2}",
														new string(' ', LeftSpam),
														(Borders.Top.Left),
														new string((Borders.Top.Border), ColumnSize)
													);
													//Lineas Laterales y Texto:
													Cell_Mid += string.Format("{0}{1}{2}",
														new string(' ', LeftSpam),
														(Borders.Sides.Left),
														CellText
													);
													//Linea Inferior:
													Cell_Bot += string.Format("{0}{1}{2}",
														new string(' ', LeftSpam),
														Borders.Bottom.Left,
														new string(Borders.Bottom.Border, ColumnSize)
													);
												}
												else if (SumaryIndex == TConfiguration.summary.Count - 1) //<- si es la ultima columna:
												{
													//Linea Superior:
													Cell_Top += string.Format("{0}{1}{2}",
														(Borders.Top.Middle),
														new string((Borders.Top.Border), ColumnSize),
														(Borders.Top.Right)
													);
													//Lineas Laterales y Texto:
													Cell_Mid += string.Format("{0}{1}{2}",
														(Borders.Sides.Left),
														CellText,
														(Borders.Middle.Border)
													);
													//Linea Inferior:
													Cell_Bot += string.Format("{0}{1}{2}",
														Borders.Bottom.Middle,
														new string(Borders.Bottom.Border, ColumnSize),
														Borders.Bottom.Right
													);
												}
												else //<- Si es una Columna del Medio:
												{
													//Linea Superior:
													Cell_Top += string.Format("{0}{1}",
														Borders.Top.Middle,
														new string(Borders.Top.Border, ColumnSize)
													);
													//Lineas Laterales y Texto:
													Cell_Mid += string.Format("{0}{1}",
														Borders.Sides.Left,
														CellText
													);
													//Linea Inferior:
													Cell_Bot += string.Format("{0}{1}",
														Borders.Bottom.Middle,
														new string(Borders.Bottom.Border, ColumnSize)
													);
												}
												SumaryIndex++;
											}

											Group.Footer.Add(Cell_Top);
											Group.Footer.Add(Cell_Mid);
											Group.Footer.Add(Cell_Bot);
										}

										#endregion
									}
								}
							}
						}
						else
						{
							GroupedData = null;
						}
					}
					else
					{
						GroupedData = null;
					}

					#endregion

					//Si los datos fueron Agrupados, mostrar los Grupos:
					if (GroupedData != null && GroupedData.Count > 0)
					{
						#region Grouped Data

						//Dibujar los Encabezados al Principio de los Grupos:
						if (TConfiguration.grouping.repeat_column_headers == false)
						{
							if (GroupColumnHeaders != null)
							{
								foreach (string linea in GroupColumnHeaders)
								{
									Lines.AppendLine(linea);
								}
							}
						}
						foreach (var group in GroupedData)
						{
							//This shows the Column names and values used for grouping:
							if (group.Header != null && group.Header.Count > 0)
							{
								foreach (string linea in group.Header)
								{
									Lines.AppendLine(linea);
								}
							}
							//This is the Data of each Group:
							if (group.Body != null && group.Body.Count > 0)
							{
								foreach (string linea in group.Body)
								{
									Lines.AppendLine(linea);
								}
								TableSize = group.Body[group.Body.Count - 1].Length;
							}
							//This is Group Summary:
							if (group.Footer != null && group.Footer.Count > 0)
							{
								foreach (string linea in group.Footer)
								{
									Lines.AppendLine(linea);
								}
							}
						}

						#endregion

						#region Table Summary

						if (DataSource != null && DataSource.Count > 0)
						{
							if (TConfiguration.summary != null && TConfiguration.summary.Count > 0)
							{
								string Cell_Top = string.Empty;
								string Cell_Mid = string.Empty;
								string Cell_Bot = string.Empty;

								// Dibujar la Linea del Fondo 
								string Cell_Bottom = Borders.Bottom.Left.ToString();
								foreach (var _Column in TConfiguration.columns)
								{
									ColumnSize = (int)_Column.length + (Margin * 2);
									Cell_Bottom += new string(Convert.ToChar(Borders.Bottom.Border), ColumnSize);
									Cell_Bottom += Convert.ToString(Borders.Bottom.Middle);
								}
								Cell_Bottom = Cell_Bottom.Substring(0, Cell_Bottom.Length - 1);
								Cell_Bottom += Convert.ToString(Borders.Bottom.Right);
								Lines.AppendLine(Cell_Bottom.Replace(Borders.Bottom.Middle, Borders.Bottom.Border));

								var SummaryData = GetSummaryValues(TConfiguration.summary, DataSource);

								int SumaryIndex = 0;
								foreach (var _Summary in TConfiguration.summary)
								{
									int ColIndex = 0;
									int LeftSpam = 0;
									foreach (var _Column in TConfiguration.columns)
									{
										if (_Column.field == _Summary.field)
										{
											LeftSpam = ColunmPositions[ColIndex];
											ColumnSize = (int)_Column.length + (Margin * 2);
											//Aplica el Formato a la Celda:
											CellText = AplicarFormato(SummaryData[SumaryIndex], _Summary.format.ToString(), _Column.type.ToString()); //Calcular el Valor
											CellText = AlinearTexto(CellText, ColumnSize, _Column.align);
											break;
										}
										ColIndex++;
									}

									//Determina la Esquina a Usar:
									if (SumaryIndex == 0 && TConfiguration.summary.Count == 1) //<- Si es La Unica Columna
									{
										//Linea Superior:
										Cell_Top += string.Format("{0}{1}{2}{3}",
											new string(' ', LeftSpam),
											Borders.Top.Left,
											new string(Convert.ToChar(Borders.Top.Border), ColumnSize),
											Borders.Top.Right
										);
										//Lineas Laterales y Texto:
										Cell_Mid += string.Format("{0}{1}{2}{3}",
											new string(' ', LeftSpam),
											Borders.Sides.Left,
											CellText,
											Borders.Middle.Border
										);
										//Linea Inferior:
										Cell_Bot += string.Format("{0}{1}{2}{3}",
											new string(' ', LeftSpam),
											Convert.ToChar(Borders.Bottom.Left),
											new string(Convert.ToChar(Borders.Bottom.Border), ColumnSize),
											Borders.Bottom.Right
										);
									}
									else if (SumaryIndex == 0) //<- Si es la Primera Columna (de Varias):
									{
										//Linea Superior:
										Cell_Top += string.Format("{0}{1}{2}",
											new string(' ', LeftSpam),
											Convert.ToChar(Borders.Top.Left),
											new string(Convert.ToChar(Borders.Top.Border), ColumnSize)
										);
										//Lineas Laterales y Texto:
										Cell_Mid += string.Format("{0}{1}{2}",
											new string(' ', LeftSpam),
											Convert.ToChar(Borders.Sides.Left),
											CellText
										);
										//Linea Inferior:
										Cell_Bot += string.Format("{0}{1}{2}",
											new string(' ', LeftSpam),
											Convert.ToChar(Borders.Bottom.Left),
											new string(Convert.ToChar(Borders.Bottom.Border), ColumnSize)
										);
									}
									else if (SumaryIndex == TConfiguration.summary.Count - 1) //<- si es la ultima columna:
									{
										//Linea Superior:
										Cell_Top += string.Format("{0}{1}{2}",
											Convert.ToChar(Borders.Top.Middle),
											new string(Convert.ToChar(Borders.Top.Border), ColumnSize),
											Convert.ToChar(Borders.Top.Right)
										);
										//Lineas Laterales y Texto:
										Cell_Mid += string.Format("{0}{1}{2}",
											Convert.ToChar(Borders.Sides.Left),
											CellText,
											Convert.ToChar(Borders.Middle.Border)
										);
										//Linea Inferior:
										Cell_Bot += string.Format("{0}{1}{2}",
											Convert.ToChar(Borders.Bottom.Middle),
											new string(Convert.ToChar(Borders.Bottom.Border), ColumnSize),
											Convert.ToChar(Borders.Bottom.Right)
										);
									}
									else //<- Si es una Columna del Medio:
									{
										//Linea Superior:
										Cell_Top += string.Format("{0}{1}",
											Convert.ToChar(Borders.Top.Middle),
											new string(Convert.ToChar(Borders.Top.Border), ColumnSize)
										);
										//Lineas Laterales y Texto:
										Cell_Mid += string.Format("{0}{1}",
											Convert.ToChar(Borders.Sides.Left),
											CellText
										);
										//Linea Inferior:
										Cell_Bot += string.Format("{0}{1}",
											Convert.ToChar(Borders.Bottom.Middle),
											new string(Convert.ToChar(Borders.Bottom.Border), ColumnSize)
										);
									}
									SumaryIndex++;
								}

								Lines.AppendLine(Cell_Top);
								Lines.AppendLine(Cell_Mid);
								Lines.AppendLine(Cell_Bot);
							}
						}

						#endregion
					}
					else //<- Si no hay Grupos, Obtener los datos como son:
					{
						#region Data Rows						

						if (DataSource != null && DataSource.Count > 0)
						{
							int DataCount = DataSource.Count;
							if (true)
							{
								for (int RowIndex = -1; RowIndex < DataCount; RowIndex++)
								{
									//La Primera Fila (RowIndex = -1) es el Cabezal
									//Las demas son filas de Datos

									string Cell_Top = string.Empty;
									string Cell_Mid = string.Empty;

									int ColumnIndex = 0;
									foreach (var _Column in TConfiguration.columns)
									{
										//2. Column Headers:
										//╔════════════════╦═════╦════════╦═══════════════════════╦═══════╗
										//║  System Name   ║Jumps║ Lenght ║     Star Type         ║ Scoop ║
										//        0           1      2              3                 4

										ColumnSize = (int)_Column.length + (Margin * 2);
										char CornerL = (char)0; //<-Empty

										//Determina la Esquina a Usar:
										if (ColumnIndex == 0) //<- Si es la Primera Columna:
										{											
											if (RowIndex < 0) //<- Si es la Primera Fila
											{
												CornerL = Borders.Top.Left;												
											}
											else
											{												
												if (RowIndex == 0)
												{
													// Si es la segunda fila (de datos) pero no se muestran los Headers
													CornerL = (!TConfiguration.properties.table.column_headers.Visible) ?
														Borders.Top.Left :
														Borders.Middle.Left;
												}
												else
												{
													//CornerL = Borders.Middle.Left;
													CornerL = (RowIndex >= 0) ?
														Borders.Middle.Left :
														Borders.Top.Left;
												}
											}
											
										}
										else //<- Si es una Columna del Medio:
										{
											if (RowIndex == 0)
											{
												// Si es la segunda fila (de datos) pero no se muestran los Headers
												CornerL = (!TConfiguration.properties.table.column_headers.Visible) ?
													Borders.Top.Middle :
													Borders.Middle.Middle;
											}
											else
											{
												CornerL = (RowIndex < 0) ?
													Borders.Top.Middle :
													Borders.Middle.Middle;
											}
										}

										//Obtiene el Texto de la Celda:
										if (RowIndex < 0) //Primera Fila para los Headers:
										{
											//Aplica el Formato a la Celda:
											CellText = AlinearTexto(_Column.title, ColumnSize);
										}
										else //Fila de Datos:
										{
											var RowData = DataSource[RowIndex];
											string FieldName = string.Empty;
											object FieldValue = null;

											//Obtiene el Valor de la Celda:	
											if (_Column.type == "Calculated")
											{
												FieldName = _Column.field;
												FieldValue = GetExpressionValue(_Column.field, _Column, RowData);
											}
											else
											{
												var propertyInfo = System.ComponentModel.TypeDescriptor.
													GetProperties((object)RowData).
													Find(_Column.field, true);

												FieldName = propertyInfo.Name;
												FieldValue = propertyInfo.GetValue(RowData);
											}

											//Aplica el Formato a la Celda:
											CellText = AplicarFormato(FieldValue, _Column);
											CellText = AlinearTexto(CellText, ColumnSize, _Column.align);
										}

										//Linea Superior:
										Cell_Top += string.Format("{0}{1}",
											CornerL,
											new string(Convert.ToChar(Borders.Top.Border), ColumnSize)
										);

										if (ColunmPositions.Count < TConfiguration.columns.Count)
										{
											ColunmPositions.Add(Cell_Top.Length);
										}

										//Lineas Laterales y Texto:
										Cell_Mid += string.Format("{0}{1}",
											Borders.Sides.Left.ToString(),
											CellText
										);

										//si es la ultima columna, agrega el Borde:
										if (ColumnIndex >= TConfiguration.columns.Count - 1)
										{
											if (RowIndex == 0)
											{
												// Si es la segunda fila (de datos) pero no se muestran los Headers
												if (!TConfiguration.properties.table.column_headers.Visible)
												{
													Cell_Top += Convert.ToString(Borders.Top.Right);
													Cell_Mid += Convert.ToString(Borders.Middle.Border);
												}
												else
												{
													Cell_Top += Convert.ToString(Borders.Middle.Right);
													Cell_Mid += Convert.ToString(Borders.Middle.Border);
												}
											}
											else
											{
												Cell_Top += (RowIndex >= 0) ? Convert.ToString(Borders.Middle.Right) : Convert.ToString(Borders.Top.Right);
												Cell_Mid += Convert.ToString(Borders.Middle.Border);
											}										
										}

										ColumnIndex++;
									}

									if (RowIndex < 0)
									{
										if (TConfiguration.properties.table.column_headers.Visible)
										{
											Lines.AppendLine(Cell_Top);
											Lines.AppendLine(Cell_Mid);
										}										
									}
									else
									{
										Lines.AppendLine(Cell_Top);
										Lines.AppendLine(Cell_Mid);
									}
								}
							}						

							// Dibujar la Linea del Fondo 
							string Cell_Bottom = Borders.Bottom.Left.ToString();
							foreach (var _Column in TConfiguration.columns)
							{
								ColumnSize = (int)_Column.length + (Margin * 2);
								Cell_Bottom += new string(Convert.ToChar(Borders.Bottom.Border), ColumnSize);
								Cell_Bottom += Convert.ToString(Borders.Bottom.Middle);
							}
							Cell_Bottom = Cell_Bottom.Substring(0, Cell_Bottom.Length - 1);
							Cell_Bottom += Convert.ToString(Borders.Bottom.Right);

							Lines.AppendLine(Cell_Bottom);
							//if (TConfiguration.properties.table.column_headers.Visible && RowIndex >= 0)
							//{
								
							//}

							TableSize = Cell_Bottom.Length;
						}
						else
						{
							#region No Data						

							//Draw 2 Rows: 1st with the Column Headers and 2nd one Empty:
							for (int RowIndex = 0; RowIndex < 2; RowIndex++)
							{
								string Cell_Top = string.Empty;
								string Cell_Mid = string.Empty;
								string Cell_Down = string.Empty;
								int ColumnIndex = 0;

								foreach (var _Column in TConfiguration.columns)
								{
									ColumnSize = (int)_Column.length + (Margin * 2);
									CellText = (RowIndex == 0) ? AlinearTexto(_Column.title, ColumnSize) : new string(' ', ColumnSize);

									//Linea Superior:
									Cell_Top += string.Format("{0}{1}",
										(RowIndex == 0) ?
											(ColumnIndex == 0 ? Borders.Top.Left : Borders.Top.Middle) :
											(ColumnIndex == 0 ? Borders.Middle.Left : Borders.Middle.Middle),
										new string(Borders.Top.Border, ColumnSize)
									);
									//Lineas Laterales y Texto:
									Cell_Mid += string.Format("{0}{1}",
										Borders.Sides.Left,
										CellText
									);
									//Linea Inferior:
									Cell_Down += string.Format("{0}{1}",
										(RowIndex == 0) ?
											(ColumnIndex == 0 ? Borders.Middle.Left : Borders.Middle.Middle) :
											(ColumnIndex == 0 ? Borders.Bottom.Left : Borders.Bottom.Middle),
										new string(Borders.Top.Border, ColumnSize)
									);

									//si es la ultima columna, agrega el Borde:
									if (ColumnIndex >= TConfiguration.columns.Count - 1)
									{
										Cell_Top += (RowIndex == 0) ? Borders.Top.Right : Borders.Middle.Right;
										Cell_Mid += Convert.ToString(Borders.Middle.Border);
										Cell_Down += Convert.ToString(Borders.Bottom.Right);
									}

									ColumnIndex++;
								}

								Lines.AppendLine(Cell_Top);
								Lines.AppendLine(Cell_Mid);
								if (RowIndex > 0)
								{
									Lines.AppendLine(Cell_Down);
								}
							}

							#endregion
						}

						#endregion

						#region Table Summary

						if (DataSource != null && DataSource.Count > 0)
						{
							if (TConfiguration.summary != null && TConfiguration.summary.Count > 0)
							{
								string Cell_Top = string.Empty;
								string Cell_Mid = string.Empty;
								string Cell_Bot = string.Empty;

								var SummaryData = GetSummaryValues(TConfiguration.summary, DataSource);

								int SumaryIndex = 0;
								foreach (var _Summary in TConfiguration.summary)
								{
									int ColIndex = 0;
									int LeftSpam = 0;
									foreach (var _Column in TConfiguration.columns)
									{
										if (_Column.field == _Summary.field)
										{
											LeftSpam = ColunmPositions[ColIndex];
											ColumnSize = (int)_Column.length + (Margin * 2);
											//Aplica el Formato a la Celda:
											CellText = AplicarFormato(SummaryData[SumaryIndex], _Summary.format.ToString(), _Column.type.ToString()); //Calcular el Valor
											CellText = AlinearTexto(CellText, ColumnSize, _Column.align);
											break;
										}
										ColIndex++;
									}

									//Determina la Esquina a Usar:
									if (SumaryIndex == 0 && TConfiguration.summary.Count == 1) //<- Si es La Unica Columna
									{
										//Linea Superior:
										Cell_Top += string.Format("{0}{1}{2}{3}",
											new string(' ', LeftSpam),
											Borders.Top.Left,
											new string(Convert.ToChar(Borders.Top.Border), ColumnSize),
											Borders.Top.Right
										);
										//Lineas Laterales y Texto:
										Cell_Mid += string.Format("{0}{1}{2}{3}",
											new string(' ', LeftSpam),
											Borders.Sides.Left,
											CellText,
											Borders.Middle.Border
										);
										//Linea Inferior:
										Cell_Bot += string.Format("{0}{1}{2}{3}",
											new string(' ', LeftSpam),
											Convert.ToChar(Borders.Bottom.Left),
											new string(Convert.ToChar(Borders.Bottom.Border), ColumnSize),
											Borders.Bottom.Right
										);
									}
									else if (SumaryIndex == 0) //<- Si es la Primera Columna (de Varias):
									{
										//Linea Superior:
										Cell_Top += string.Format("{0}{1}{2}",
											new string(' ', LeftSpam),
											Convert.ToChar(Borders.Top.Left),
											new string(Convert.ToChar(Borders.Top.Border), ColumnSize)
										);
										//Lineas Laterales y Texto:
										Cell_Mid += string.Format("{0}{1}{2}",
											new string(' ', LeftSpam),
											Convert.ToChar(Borders.Sides.Left),
											CellText
										);
										//Linea Inferior:
										Cell_Bot += string.Format("{0}{1}{2}",
											new string(' ', LeftSpam),
											Convert.ToChar(Borders.Bottom.Left),
											new string(Convert.ToChar(Borders.Bottom.Border), ColumnSize)
										);
									}
									else if (SumaryIndex == TConfiguration.summary.Count - 1) //<- si es la ultima columna:
									{
										//Linea Superior:
										Cell_Top += string.Format("{0}{1}{2}",
											Convert.ToChar(Borders.Top.Middle),
											new string(Convert.ToChar(Borders.Top.Border), ColumnSize),
											Convert.ToChar(Borders.Top.Right)
										);
										//Lineas Laterales y Texto:
										Cell_Mid += string.Format("{0}{1}{2}",
											Convert.ToChar(Borders.Sides.Left),
											CellText,
											Convert.ToChar(Borders.Middle.Border)
										);
										//Linea Inferior:
										Cell_Bot += string.Format("{0}{1}{2}",
											Convert.ToChar(Borders.Bottom.Middle),
											new string(Convert.ToChar(Borders.Bottom.Border), ColumnSize),
											Convert.ToChar(Borders.Bottom.Right)
										);
									}
									else //<- Si es una Columna del Medio:
									{
										//Linea Superior:
										Cell_Top += string.Format("{0}{1}",
											Convert.ToChar(Borders.Top.Middle),
											new string(Convert.ToChar(Borders.Top.Border), ColumnSize)
										);
										//Lineas Laterales y Texto:
										Cell_Mid += string.Format("{0}{1}",
											Convert.ToChar(Borders.Sides.Left),
											CellText
										);
										//Linea Inferior:
										Cell_Bot += string.Format("{0}{1}",
											Convert.ToChar(Borders.Bottom.Middle),
											new string(Convert.ToChar(Borders.Bottom.Border), ColumnSize)
										);
									}
									SumaryIndex++;
								}

								Lines.AppendLine(Cell_Top);
								Lines.AppendLine(Cell_Mid);
								Lines.AppendLine(Cell_Bot);
							}
						}

						#endregion
					}

					#region Table Footer

					if (TConfiguration.footer != null)
					{
						CellText = TConfiguration.footer.title.ToString();

						//May use the whole row or just the header width
						ColumnSize = (!TConfiguration.footer.size.use_whole_row) ?
							TConfiguration.footer.size.length :
							TableSize;

						string LINE = string.Empty;

						//Linea Superior:
						LINE = new string(Borders.Top.Border, ColumnSize - 2);
						LINE = string.Format("{0}{1}{2}", Borders.Top.Left, LINE, Borders.Top.Right);
						Lines.AppendLine(
							AlinearTexto(LINE, TableSize, TConfiguration.footer.text_align)
						);

						//Lineas Laterales y Texto:
						LINE = AlinearTexto(CellText, ColumnSize - 2, TConfiguration.footer.text_align);
						LINE = string.Format("{0}{1}{2}", Borders.Sides.Left, LINE, Borders.Sides.Right);
						Lines.AppendLine(
							AlinearTexto(LINE, TableSize, TConfiguration.footer.text_align)
						);

						//Linea Inferior:
						LINE = new string(Borders.Bottom.Border, ColumnSize - 2);
						LINE = string.Format("{0}{1}{2}", Borders.Bottom.Left, LINE, Borders.Bottom.Right);
						Lines.AppendLine(
							AlinearTexto(LINE, TableSize, TConfiguration.footer.text_align)
						);
					}

					#endregion

					_ret = Lines.ToString();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return _ret;
		}

		/// <summary>Builds the DataTable on an Image.</summary>
		/// <param name="pImageSize">Size for the returned Image</param>
		public Bitmap Build_ImageDataTable(Size pImageSize)
		{
			System.Drawing.Bitmap image = null;
			try
			{
				if (TConfiguration != null)
				{
					image = DrawNewImage(pImageSize);
					using (var g = Graphics.FromImage(image))
					{
						//Todas las opciones en Alta Calidad:
						//g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
						//g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
						//g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
						//g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

						#region Config

						Color BorderColor = StringToColor(TConfiguration.properties.borders.color_argb);
						Pen BorderPen = new Pen(BorderColor, 1.0f);

						//This would be the default Values:
						SolidBrush BackgroundBrush = TConfiguration.properties.table.data_rows.colors.ToBrush("backcolor_argb");
						SolidBrush TextBrush = TConfiguration.properties.table.data_rows.colors.ToBrush("forecolor_argb");
						Font Fuente = TConfiguration.properties.font.ToFont();

						//Table's Header & Footer:
						SolidBrush HeaderFooter_TextColor = (TConfiguration.header != null) ? new SolidBrush(StringToColor(TConfiguration.header.colors.forecolor_argb)) : TextBrush;
						SolidBrush HeaderFooter_BackColor = (TConfiguration.header != null) ? new SolidBrush(StringToColor(TConfiguration.header.colors.backcolor_argb)) : BackgroundBrush;
						Font HeaderFooter_Font = (TConfiguration.header != null) ? TConfiguration.header.font.ToFont() : Fuente;

						//Columns:
						SolidBrush ColumnHeaders_TextColor = new SolidBrush(StringToColor(TConfiguration.properties.table.column_headers.colors.forecolor_argb));
						SolidBrush ColumnHeaders_BackColor = new SolidBrush(StringToColor(TConfiguration.properties.table.column_headers.colors.backcolor_argb));
						Font ColumnHeaders_Font = TConfiguration.properties.table.column_headers.font.ToFont();

						//Rows:
						SolidBrush DataRows_TextColor = new SolidBrush(StringToColor(TConfiguration.properties.table.data_rows.colors.forecolor_argb));
						SolidBrush DataRows_BackColor = new SolidBrush(StringToColor(TConfiguration.properties.table.data_rows.colors.backcolor_argb));
						Font DataRows_Font = TConfiguration.properties.table.data_rows.font.ToFont();

						//Groups:
						SolidBrush Groups_TextColor = (TConfiguration.grouping != null) ? new SolidBrush(StringToColor(TConfiguration.grouping.colors.forecolor_argb)) : TextBrush;
						SolidBrush Groups_BackColor = (TConfiguration.grouping != null) ? new SolidBrush(StringToColor(TConfiguration.grouping.colors.backcolor_argb)) : BackgroundBrush;
						Font Groups_Font = (TConfiguration.grouping != null) ? TConfiguration.grouping.font.ToFont() : Fuente;


						Rectangle CellBox = new Rectangle(0, 0, 0, 0);
						Point TextPosition = new Point(0, 0);
						Point RowPosition = new Point(0, 0);
						Point StartPos = new Point(0, 0);
						SizeF TextSize = new SizeF();
						Size RowSize = new Size();

						int Margin = TConfiguration.properties.table.data_rows.cell_padding * 10;
						StartPos = new Point(Margin, Margin);
						int ColumnSize = 0;
						int TableSize = 0; //<- Width in Pixels of the Table's Body

						string CellText = string.Empty;
						int RowIndex = 0;
						int ColIndex = 0;

						List<int> ColunmPositions = new List<int>(); //<- X coordinate for Each Column
						CellBoxMappings = new List<ImageMapping>();


						//Calculate the Table's Body Width:
						if (TConfiguration.columns != null && TConfiguration.columns.Count > 0)
						{
							foreach (var _Column in TConfiguration.columns)
							{
								TableSize += (int)_Column.width;
							}
							TableSize++; //<- Size in Pixels of all the Fields in the Body

							if (pImageSize.Width < TableSize)
							{
								MessageBox.Show(string.Format("The Image Width is Insuficient!\r\nShould be {0}px at least.", TableSize));
							}
						}

						#endregion

						#region Table Header

						if (TConfiguration.header != null)
						{
							//Obtiene el tamaño del Texto en pixeles:
							TextSize = g.MeasureString(TConfiguration.header.title, HeaderFooter_Font);

							if (TConfiguration.header.size.use_whole_row)
							{
								RowSize = new Size(
									TableSize,
									Math.Max(TConfiguration.header.size.height, (int)TextSize.Height)
								);
							}
							else
							{
								//If the specified Footer size is insuficient, it gets fixed to the Text Size:
								RowSize = new Size(
									Math.Max(TConfiguration.header.size.width, (int)TextSize.Width),
									Math.Max(TConfiguration.header.size.height, (int)TextSize.Height)
								);
							}

							//The Header gets Centered respect to the Table's Body:
							RowPosition = new Point(
								Convert.ToInt32(StartPos.X + ((TableSize - RowSize.Width) / 2)),
								StartPos.Y
							);

							//Crea la Caja para el Header:
							CellBox = new Rectangle(RowPosition, RowSize);

							// Calcula la posicion para centrar el Texto:
							TextPosition = AlinearTexto(CellBox, TextSize, TConfiguration.header.text_align);

							// Dibuja el Header:
							g.FillRectangle(HeaderFooter_BackColor, CellBox);    //<- El Fondo
							g.DrawRectangle(BorderPen, CellBox);                //<- El Borde
							g.DrawString(TConfiguration.header.title,           //<- El Texto 
								HeaderFooter_Font,
								HeaderFooter_TextColor,
								TextPosition);

							// Registra la Posicion final (el principio de la siguiente Fila):
							StartPos.Y = CellBox.Location.Y + CellBox.Height + Margin;

							CellBoxMappings.Add(new ImageMapping(ColIndex, RowIndex)
							{
								ElementType = TableElements.TABLE_HEADER,
								RawValue = TConfiguration.header.title,

								CellText = CellText,
								CellBox = CellBox,

								Column = TConfiguration.header,
								Row = null
							});
						}

						#endregion

						#region Data Sorting

						//If the DataSorting is Configured and Enabled, here is where it's Applied:
						if (DataSource != null && DataSource.Count > 0)
						{
							if (TConfiguration.sorting != null && TConfiguration.sorting.enabled)
							{
								if (TConfiguration.sorting.fields != null && TConfiguration.sorting.fields.Count > 0)
								{
									DataSource = DynamicSorting(DataSource, TConfiguration.sorting.fields);
								}
							}
						}

						#endregion

						#region Data Grouping

						//List<string> GroupColumnHeaders = null; //<- Used only when  'repeat_column_headers' is false.

						if (DataSource != null && DataSource.Count > 0)
						{
							//If the DataGrouping is Configured and Enabled, here is where it is Applied:

							if (TConfiguration.grouping != null && TConfiguration.grouping.enabled)
							{
								if (TConfiguration.grouping.fields != null && TConfiguration.grouping.fields.Count > 0)
								{
									//Here we do the Actual Data Grouping:
									GroupedData = DynamicGrouping(DataSource, TConfiguration.grouping.fields);

									//Here we Draw the Grouped Data:
									if (GroupedData != null && GroupedData.Count > 0)
									{
										bool repeat_column_headers = TConfiguration.grouping.repeat_column_headers;
										bool show_summary = TConfiguration.grouping.show_summary;
										bool show_Count = TConfiguration.grouping.show_count;

										//Do we need to draw the Column Headers here ? or they got drawn with each Group?
										#region Column Headers

										if (!repeat_column_headers)
										{
											//Yes! we gotta do it here :-(

											RowPosition = new Point(StartPos.X, StartPos.Y);
											ColunmPositions = new List<int>();
											ColIndex = 0;
											RowIndex = 0;

											foreach (var _Column in TConfiguration.columns)
											{
												CellText = _Column.title;
												TextSize = g.MeasureString(CellText, ColumnHeaders_Font);

												RowSize = new Size(_Column.width, Math.Max(TConfiguration.properties.table.data_rows.cell_height, (int)TextSize.Height));
												CellBox = new Rectangle(RowPosition, RowSize);

												TextPosition = AlinearTexto(CellBox, TextSize);

												g.FillRectangle(ColumnHeaders_BackColor, CellBox);    //<- El Fondo
												g.DrawRectangle(BorderPen, CellBox);          //<- El Borde
												g.DrawString(CellText,                        //<- El Texto 
													ColumnHeaders_Font,
													ColumnHeaders_TextColor,
													TextPosition);

												RowPosition.X = CellBox.X + CellBox.Width;
												ColunmPositions.Add(CellBox.X);

												CellBoxMappings.Add(new ImageMapping(ColIndex, RowIndex)
												{
													ElementType = TableElements.COLUMN_ROW,
													RawValue = _Column.title,

													CellText = CellText,
													CellBox = CellBox,

													Column = _Column,
													Row = null
												});
												ColIndex++;
											}

											// Registra la Posicion final (el principio de la siguiente Fila):
											StartPos.Y = CellBox.Location.Y + CellBox.Height + Margin;


										}

										#endregion

										ColIndex = 0;
										RowIndex = 0;

										foreach (var Group in GroupedData)
										{
											ColumnSize = (int)Group.HeaderData.Length + (Margin * 2);

											#region Group Header

											Group.Header = new List<string>();

											CellText = Group.HeaderData; // AlinearTexto(Group.HeaderData, ColumnSize);
											TextSize = g.MeasureString(CellText, Groups_Font);

											//If the specified Header size is insuficient, it gets fixed to the Text Size:
											RowSize = new Size(
												Math.Max(ColumnSize, (int)TextSize.Width),
												Math.Max(TConfiguration.properties.table.data_rows.cell_height, (int)TextSize.Height)
											);
											RowPosition = new Point(StartPos.X, StartPos.Y);
											CellBox = new Rectangle(RowPosition, RowSize);
											TextPosition = AlinearTexto(CellBox, TextSize);

											g.FillRectangle(Groups_BackColor, CellBox);    //<- El Fondo
											g.DrawRectangle(BorderPen, CellBox);          //<- El Borde
											g.DrawString(CellText,                        //<- El Texto 
												Groups_Font,
												Groups_TextColor,
												TextPosition);

											CellBoxMappings.Add(new ImageMapping(ColIndex, RowIndex)
											{
												ElementType = TableElements.GROUP_HEADER,
												RawValue = Group.HeaderData,

												CellText = CellText,
												CellBox = CellBox,

												Column = Group,
												Row = null
											});
											ColIndex++;

											if (show_Count)
											{
												CellText = string.Format(TConfiguration.grouping.CountFormat, Group.Count);
												TextSize = g.MeasureString(CellText, Groups_Font);
												RowSize = new Size(
													Math.Max(ColumnSize, (int)TextSize.Width),
													Math.Max(TConfiguration.properties.table.data_rows.cell_height, (int)TextSize.Height)
												);
												RowPosition = new Point(CellBox.X + CellBox.Width + Margin, CellBox.Y);
												CellBox = new Rectangle(RowPosition, RowSize);
												TextPosition = AlinearTexto(CellBox, TextSize);

												g.DrawString(CellText,                        //<- El Texto 
													Groups_Font,
													Groups_TextColor,
													TextPosition
												);
											}

											RowPosition.Y = CellBox.Location.Y + CellBox.Height;
											ColunmPositions.Add(RowPosition.X);

											// Registra la Posicion final (el principio de la siguiente Fila):
											StartPos.Y = CellBox.Location.Y + CellBox.Height + Margin;

											#endregion

											#region Column Headers

											//Do we need to draw the Column Headers here ? or they got drawn at the begining of the table?
											if (repeat_column_headers)
											{
												//Yes! we gotta do it here :-(

												RowPosition = new Point(StartPos.X, StartPos.Y);
												ColunmPositions = new List<int>();
												RowIndex = 0;
												ColIndex = 0;

												foreach (var _Column in TConfiguration.columns)
												{
													CellText = _Column.title;
													TextSize = g.MeasureString(CellText, ColumnHeaders_Font);

													RowSize = new Size(_Column.width, //<- Column width is constant as defined but row Height is ajusted to the font if needed
														Math.Max(TConfiguration.properties.table.data_rows.cell_height, (int)TextSize.Height));
													CellBox = new Rectangle(RowPosition, RowSize);

													TextPosition = AlinearTexto(CellBox, TextSize);

													g.FillRectangle(ColumnHeaders_BackColor, CellBox);    //<- El Fondo
													g.DrawRectangle(BorderPen, CellBox);          //<- El Borde
													g.DrawString(CellText,                        //<- El Texto 
														ColumnHeaders_Font,
														ColumnHeaders_TextColor,
														TextPosition);

													RowPosition.X = CellBox.X + CellBox.Width;
													ColunmPositions.Add(CellBox.X);

													CellBoxMappings.Add(new ImageMapping(ColIndex, RowIndex)
													{
														ElementType = TableElements.COLUMN_ROW,
														RawValue = _Column.title,

														CellText = CellText,
														CellBox = CellBox,

														Column = _Column,
														Row = null
													});
													ColIndex++;
												}

												// Registra la Posicion final (el principio de la siguiente Fila):
												StartPos.Y = CellBox.Location.Y + CellBox.Height;
											}

											#endregion

											#region Group Body (Data Rows)

											if (Group.data != null && Group.data.Count > 0)
											{
												RowIndex = 0;
												foreach (var _RowData in Group.data)
												{
													RowPosition = new Point(StartPos.X, StartPos.Y);
													ColIndex = 0;

													foreach (var _Column in TConfiguration.columns)
													{
														//Obtiene el Valor de la Celda:	
														string FieldName = string.Empty;
														object FieldValue = null;

														if (_Column.type == "Calculated")
														{
															FieldValue = GetExpressionValue(_Column.field, _Column, _RowData);
														}
														else
														{
															var propertyInfo = System.ComponentModel.TypeDescriptor.
																GetProperties((object)_RowData).
																Find(_Column.field, true);

															FieldName = propertyInfo.Name;
															FieldValue = propertyInfo.GetValue(_RowData);
														}

														CellText = AplicarFormato(FieldValue, _Column.format, _Column.type);
														TextSize = g.MeasureString(CellText, DataRows_Font);
														RowSize = new Size(_Column.width, //<- Column width is constant as defined but row Height is ajusted to the font if needed
																	  Math.Max(TConfiguration.properties.table.data_rows.cell_height, (int)TextSize.Height));
														CellBox = new Rectangle(RowPosition, RowSize);
														TextPosition = AlinearTexto(CellBox, TextSize, _Column.align);

														g.FillRectangle(DataRows_BackColor, CellBox);    //<- El Fondo
														g.DrawRectangle(BorderPen, CellBox);          //<- El Borde
														g.DrawString(CellText, DataRows_Font, DataRows_TextColor, TextPosition);  //<- El Texto 

														RowPosition.X = CellBox.Location.X + CellBox.Width;
														RowPosition.Y = StartPos.Y;

														CellBoxMappings.Add(new ImageMapping(ColIndex, RowIndex)
														{
															ElementType = TableElements.DATA_ROW,
															RawValue = FieldValue,

															CellText = CellText,
															CellBox = CellBox,

															Column = _Column,
															Row = _RowData
														});
														ColIndex++;
													}

													// Registra la Posicion final (el principio de la siguiente Fila):
													StartPos.Y = CellBox.Location.Y + CellBox.Height;
													RowIndex++;
												}

												//Posicion Final del Grupo:
												StartPos.Y += Margin;
											}

											#endregion

											#region Group Summary

											if (TConfiguration.summary != null && TConfiguration.summary.Count > 0)
											{
												List<decimal> SummaryData = GetSummaryValues(TConfiguration.summary, Group.data);

												int SumaryIndex = 0;
												foreach (var _Summary in TConfiguration.summary)
												{
													ColIndex = 0;
													int LeftSpam = 0;

													CellText = string.Empty;
													RowPosition = new Point(StartPos.X, StartPos.Y);

													foreach (var _Column in TConfiguration.columns)
													{
														if (_Column.field == _Summary.field)
														{
															LeftSpam = ColunmPositions[ColIndex];
															RowPosition = new Point(LeftSpam, RowPosition.Y);

															CellText = AplicarFormato(SummaryData[SumaryIndex], _Summary.format, _Column.type);
															TextSize = g.MeasureString(CellText, DataRows_Font);
															RowSize = new Size(_Column.width, //<- Column width is constant as defined but row Height is ajusted to the font if needed
																	  Math.Max(TConfiguration.properties.table.data_rows.cell_height, (int)TextSize.Height));
															CellBox = new Rectangle(RowPosition, RowSize);
															TextPosition = AlinearTexto(CellBox, TextSize, _Column.align);

															g.FillRectangle(ColumnHeaders_BackColor, CellBox);    //<- El Fondo
															g.DrawRectangle(BorderPen, CellBox);          //<- El Borde
															g.DrawString(CellText,                       //<- El Texto 
																DataRows_Font,
																ColumnHeaders_TextColor,
																TextPosition);

															RowPosition.Y = CellBox.Location.Y + CellBox.Height;

															CellBoxMappings.Add(new ImageMapping(ColIndex, RowIndex)
															{
																ElementType = TableElements.GROUP_SUMMARY,
																RawValue = SummaryData[SumaryIndex],

																CellText = CellText,
																CellBox = CellBox,

																Column = _Column,
																Row = _Summary
															});
														}
														ColIndex++;
													}
													SumaryIndex++;
												}

												// Registra la Posicion final (el principio de la siguiente Fila):
												StartPos.Y = CellBox.Location.Y + CellBox.Height;
												StartPos.Y += Margin;
											}

											#endregion
										}

										//If there is Sumary, draw a line before showing the Table's Summary
										if (TConfiguration.summary != null && TConfiguration.summary.Count > 0)
										{
											var EndPoint = new Point(StartPos.X + TableSize, StartPos.Y);

											//the line has 'horns':  └───────────────┘
											g.DrawLine(BorderPen, StartPos, new Point(StartPos.X, StartPos.Y - Margin)); //<- Left Horn
											g.DrawLine(BorderPen, StartPos, EndPoint); //<- the Line
											g.DrawLine(BorderPen, EndPoint, new Point(EndPoint.X, EndPoint.Y - Margin)); //<- Right Horn
										}
									}
								}
							}
							else
							{
								GroupedData = null;
							}
						}
						else
						{
							GroupedData = null;
						}

						#endregion

						#region UnGrouped Data

						//Grouping not Enabled, lets show the data as it is:
						if (GroupedData == null)
						{
							#region Column Headers

							RowPosition = new Point(StartPos.X, StartPos.Y);
							ColunmPositions = new List<int>();
							ColIndex = 0;
							RowIndex = 0;

							foreach (var _Column in TConfiguration.columns)
							{
								CellText = _Column.title;
								TextSize = g.MeasureString(CellText, ColumnHeaders_Font);

								RowSize = new Size(_Column.width, //<- Column width is constant as defined but row Height is ajusted to the font if needed
									Math.Max(TConfiguration.properties.table.data_rows.cell_height, (int)TextSize.Height));
								CellBox = new Rectangle(RowPosition, RowSize);

								TextPosition = AlinearTexto(CellBox, TextSize);
																
								if (TConfiguration.properties.table.column_headers.Visible)
								{
									g.FillRectangle(ColumnHeaders_BackColor, CellBox);    //<- El Fondo
									g.DrawRectangle(BorderPen, CellBox);          //<- El Borde
									g.DrawString(CellText,                        //<- El Texto 
										ColumnHeaders_Font,
										ColumnHeaders_TextColor,
										TextPosition);
								}

								RowPosition.X = CellBox.X + CellBox.Width;
								ColunmPositions.Add(CellBox.X);

								CellBoxMappings.Add(new ImageMapping(ColIndex, RowIndex)
								{
									ElementType = TableElements.COLUMN_ROW,
									RawValue = _Column.title,

									CellText = CellText,
									CellBox = CellBox,

									Column = _Column,
									Row = null
								});
								ColIndex++;
							}

							// Registra la Posicion final (el principio de la siguiente Fila):
							StartPos.Y = CellBox.Location.Y + CellBox.Height;

							#endregion

							#region Data Rows

							if (DataSource != null && DataSource.Count > 0)
							{
								foreach (var _RowData in DataSource)
								{
									RowPosition = new Point(StartPos.X, StartPos.Y);

									foreach (var _Column in TConfiguration.columns)
									{
										//Obtiene el Valor de la Celda:	
										string FieldName = string.Empty;
										object FieldValue = null;

										if (_Column.type == "Calculated")
										{
											FieldValue = GetExpressionValue(_Column.field, _Column, _RowData);
										}
										else
										{
											var propertyInfo = System.ComponentModel.TypeDescriptor.
												GetProperties((object)_RowData).
												Find(_Column.field, true);

											FieldName = propertyInfo.Name;
											FieldValue = propertyInfo.GetValue(_RowData);
										}

										CellText = AplicarFormato(FieldValue, _Column.format, _Column.type);
										TextSize = g.MeasureString(CellText, DataRows_Font);
										RowSize = new Size(_Column.width, //<- Column width is constant as defined but row Height is ajusted to the font if needed
													  Math.Max(TConfiguration.properties.table.data_rows.cell_height, (int)TextSize.Height));
										CellBox = new Rectangle(RowPosition, RowSize);
										TextPosition = AlinearTexto(CellBox, TextSize, _Column.align);

										g.FillRectangle(DataRows_BackColor, CellBox);    //<- El Fondo
										g.DrawRectangle(BorderPen, CellBox);          //<- El Borde
										g.DrawString(CellText, DataRows_Font, DataRows_TextColor, TextPosition);  //<- El Texto 

										RowPosition.X = CellBox.Location.X + CellBox.Width;
										RowPosition.Y = StartPos.Y;
									}

									// Registra la Posicion final (el principio de la siguiente Fila):
									StartPos.Y = CellBox.Location.Y + CellBox.Height;
								}
							}
							else
							{
								//There is NO data to show
								// Only draw 1 row of empty columns
								RowPosition = new Point(StartPos.X, StartPos.Y);

								foreach (var _Column in TConfiguration.columns)
								{
									CellText = string.Empty;
									TextSize = g.MeasureString(CellText, ColumnHeaders_Font);

									RowSize = new Size(_Column.width, //<- Column width is constant as defined but row Height is ajusted to the font if needed
										Math.Max(TConfiguration.properties.table.data_rows.cell_height, (int)TextSize.Height));
									CellBox = new Rectangle(RowPosition, RowSize);

									TextPosition = AlinearTexto(CellBox, TextSize);

									g.FillRectangle(DataRows_BackColor, CellBox);    //<- El Fondo
									g.DrawRectangle(BorderPen, CellBox);          //<- El Borde
									g.DrawString(CellText,                        //<- El Texto 
										ColumnHeaders_Font,
										ColumnHeaders_TextColor,
										TextPosition);

									RowPosition.X = CellBox.X + CellBox.Width;
									ColunmPositions.Add(CellBox.X);
								}

								// Registra la Posicion final (el principio de la siguiente Fila):
								StartPos.Y = CellBox.Location.Y + CellBox.Height;
							}
							#endregion

							//Registra la Posicion final (el principio de la siguiente Fila):
							StartPos.Y = CellBox.Location.Y + CellBox.Height + 12;
						}

						#endregion

						#region Table Summary

						if (TConfiguration.summary != null && TConfiguration.summary.Count > 0)
						{
							List<decimal> SummaryData = GetSummaryValues(TConfiguration.summary, DataSource);
							if (SummaryData != null)
							{
								int SumaryIndex = 0;
								foreach (var _Summary in TConfiguration.summary)
								{
									ColIndex = 0;

									int LeftSpam = 0;

									CellText = string.Empty;
									RowPosition = new Point(StartPos.X, StartPos.Y);

									foreach (var _Column in TConfiguration.columns)
									{
										if (_Column.field == _Summary.field)
										{
											LeftSpam = ColunmPositions[ColIndex];
											RowPosition = new Point(LeftSpam, RowPosition.Y);

											CellText = AplicarFormato(SummaryData[SumaryIndex], _Summary.format, _Column.type);
											TextSize = g.MeasureString(CellText, DataRows_Font);
											RowSize = new Size(_Column.width, //<- Column width is constant as defined but row Height is ajusted to the font if needed
													  Math.Max(TConfiguration.properties.table.data_rows.cell_height, (int)TextSize.Height));
											CellBox = new Rectangle(RowPosition, RowSize);
											TextPosition = AlinearTexto(CellBox, TextSize, _Column.align);

											g.FillRectangle(ColumnHeaders_BackColor, CellBox);    //<- El Fondo
											g.DrawRectangle(BorderPen, CellBox);          //<- El Borde
											g.DrawString(CellText,                       //<- El Texto 
												DataRows_Font,
												ColumnHeaders_TextColor,
												TextPosition);

											RowPosition.Y = CellBox.Location.Y + CellBox.Height;

											CellBoxMappings.Add(new ImageMapping(ColIndex, SumaryIndex)
											{
												ElementType = TableElements.TABLE_SUMMARY,
												RawValue = SummaryData[SumaryIndex],

												CellText = CellText,
												CellBox = CellBox,

												Column = _Column,
												Row = _Summary
											});
										}
										ColIndex++;
									}
									SumaryIndex++;
								}

								// Registra la Posicion final (el principio de la siguiente Fila):
								StartPos.Y = CellBox.Location.Y + CellBox.Height;
								StartPos.Y += Margin;
							}
						}

						//Registra la Posicion final (el principio de la siguiente Fila):
						StartPos.Y = CellBox.Location.Y + CellBox.Height + 12;

						#endregion

						#region Table Footer

						if (TConfiguration.footer != null)
						{
							HeaderFooter_Font = TConfiguration.footer.font.ToFont();
							HeaderFooter_TextColor = new SolidBrush(StringToColor(TConfiguration.footer.colors.forecolor_argb));
							HeaderFooter_BackColor = new SolidBrush(StringToColor(TConfiguration.footer.colors.backcolor_argb));

							TextSize = g.MeasureString(TConfiguration.footer.title, HeaderFooter_Font);
							RowSize = new Size(0, 0);

							if (TConfiguration.footer.size.use_whole_row)
							{
								RowSize = new Size(
									TableSize,
									Math.Max(TConfiguration.footer.size.height, (int)TextSize.Height)
								);
							}
							else
							{
								//If the specified Footer size is insuficient, it gets fixed to the Text Size:
								RowSize = new Size(
									Math.Max(TConfiguration.footer.size.width, (int)TextSize.Width),
									Math.Max(TConfiguration.footer.size.height, (int)TextSize.Height)
								);
							}

							//The Footer gets Centered respect to the Table's Body:
							RowPosition = new Point(
								Convert.ToInt32(StartPos.X + ((TableSize - RowSize.Width) / 2)),
								StartPos.Y
							);
							CellBox = new Rectangle(RowPosition, RowSize);
							TextPosition = AlinearTexto(CellBox, TextSize, TConfiguration.footer.text_align);

							g.FillRectangle(HeaderFooter_BackColor, CellBox);    //<- El Fondo
							g.DrawRectangle(BorderPen, CellBox);          //<- El Borde
							g.DrawString(TConfiguration.footer.title,       //<- El Texto 
								HeaderFooter_Font,
								HeaderFooter_TextColor,
								TextPosition
							);

							//Registra la Posicion final (el principio de la siguiente Fila):
							StartPos.Y = CellBox.Location.Y + CellBox.Height + 12;

							CellBoxMappings.Add(new ImageMapping(ColIndex, 0)
							{
								ElementType = TableElements.TABLE_FOOTER,
								RawValue = TConfiguration.footer.title,

								CellText = TConfiguration.footer.title,
								CellBox = CellBox,

								Column = TConfiguration.footer,
								Row = null
							});
						}

						#endregion

						Console.WriteLine(StartPos.Y); //<- Height in Pixels of all the drawn elements
						if (pImageSize.Height < StartPos.Y)
						{
							MessageBox.Show(string.Format("The Image Height is Insuficient!\r\nShould be {0}px at least.", StartPos.Y));
						}

						g.Flush();
						image = new Bitmap(image);

						Console.WriteLine(CellBoxMappings.Count);
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return image;
		}

		/// <summary>Invokes the Filter Editor.
		/// Returns the list of Criteria created.</summary>
		/// <param name="pCritera">[Optional] List of Filters to be applied.</param>
		/// <param name="Apply">[Optional] Apply the Filters in the current data. Default: 'true'</param>
		public List<FilterCriteria> CallFilterEditor(List<FilterCriteria> pCritera = null, bool Apply = true)
		{
			List<FilterCriteria> _ret = null;
			try
			{
				FilterEditor Form = new FilterEditor(this.TConfiguration.columns, this.OriginalData);
				Form.Criteria = (pCritera != null) ? pCritera : this.FilterCriteria; //<- If we already have a filter then we use it
				if (Form.ShowDialog() == DialogResult.OK)
				{
					_ret = Form.Criteria;
					this.FilterCriteria = Form.Criteria;
					this.FilterData(this.FilterCriteria, true);

					if (Apply)
					{
						//Reconstruir la Tabla y disparar un evento para informar del cambio:
						OnTableDataChanged(this.Build_TextDataTable(), 
							new MyEventArgs { EventType = "FILTER", EventData = Form.Criteria }); //<- Disparador del Evento.
					}
				}
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
		public List<dynamic> FilterData(string Criteria, bool Apply = false)
		{
			List<dynamic> _ret = null;
			try
			{
				if (Criteria != null && Criteria != string.Empty)
				{
					/* WE ARE USING 'JSONPath' TO DO THE FILTERING because i was unable to make it work with Linq */
					//https://www.newtonsoft.com/json/help/html/SelectToken.htm
					//https://www.newtonsoft.com/json/help/html/QueryJsonSelectTokenJsonPath.htm
					//https://www.newtonsoft.com/json/help/html/QueryingLINQtoJSON.htm

					//https://goessner.net/articles/JsonPath/
					//https://docs.hevodata.com/sources/streaming/rest-api/writing-jsonpath-expressions/
					//https://docs.mashery.com/connectorsguide/GUID-751D8687-F803-460A-BC76-67A37779BE7A.html
					//https://www.ietf.org/archive/id/draft-ietf-jsonpath-base-01.html
					//http://jsonpath.com/

					//Examples:
					//$[?(@.system_name == 'HR 1201')]
					//$[?(@.scoop != false)]
					//$[?(@.jumps >= 2)]
					//$[?(@.startDate >= '2022-01-30T00:00:00-03:00')]		//<- DateTime Comparations

					//$[?(@.system_name == 'HR 1201' && @.scoop == false)]  //<-- AND
					//$[?(@.system_name == 'HR 1201' || @.scoop == false)]  //<-- OR

					//$[?(@.system_name =~ /.*1568/i )]	//<- matches all who END with '*1568' (case-insensitive).
					//$[?(@.system_name =~ /GCR.*/i )]	//<- matches all who START with 'GCR' (case-insensitive).
					//$[?(@.system_name =~ /.*ven.*/i )] //<- matches all who CONTAINS 'VEN'   (case-insensitive).

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

							//If true, the Filtered data becomes the Data to show on the Table
							//Need to call the 'Build_Table..' method to actually draw the Table.
							if (Apply)
							{
								DataSource = _ret;
								OnTableDataChanged(_ret, null); //<- Disparador del Evento.
							}
						}
						else
						{
							//No Data Found for the Filter, Returns an Empty (not Null) List:
							_ret = new List<dynamic>();
							if (Apply)
							{
								DataSource = _ret;
								OnTableDataChanged(_ret, null); //<- Disparador del Evento.
							}
						}
					}
					else
					{
						throw new Exception("Eror 404 - No Data Loaded on the Table.");
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return _ret;
		}

		/// <summary>Filters the data using JSONPath expressions.</summary>
		/// <param name="Criteria">Expressions to Filter the data.</param>
		/// <param name="Apply">If true, the Filtered data becomes the Data to show on the Table. Need to call the 'Build' table method.</param>
		public List<dynamic> FilterData(List<FilterCriteria> Criteria, bool Apply = false)
		{
			List<dynamic> _ret = null;
			try
			{
				//Crea una Expression JSONPath:
				if (Criteria != null && Criteria.Count > 0)
				{
					string Expresion = string.Empty;
					foreach (var Filter in Criteria)
					{
						Expresion += Filter.ToString();
					}

					Expresion = string.Format("$[?({0})]", Expresion);

					_ret = FilterData(Expresion, Apply);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return _ret;
		}

		/// <summary>Do a quick Text Search on all fields (up to 10) querying for the Search String.</summary>
		/// <param name="pSearchString">Text to query for. Culture Invariant Case Insensitive.</param>
		/// <param name="Apply">If true, the Filtered data becomes the Data to show on the Table. Need to call the 'Build' table method.</param>
		public List<dynamic> QuickSearch(string pSearchString, bool Apply = false)
		{
			List<dynamic> _ret = null;
			try
			{
				if (pSearchString != null && pSearchString != string.Empty)
				{
					List<string> Fields = new List<string>();
					foreach (var _Column in TConfiguration.columns)
					{
						Fields.Add(_Column.field);
					}

					_ret = QuickSearch(pSearchString, Apply, Fields.ToArray());
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return _ret;
		}

		/// <summary>Do a quick Text Search on selected fields (up to 10) querying for the Search String.</summary>
		/// <param name="pSearchString">Text to query for. Culture Invariant Case Insensitive.</param>
		/// <param name="Apply">If true, the Filtered data becomes the Data to show on the Table. Need to call the 'Build' table method.</param>
		/// <param name="pFields">List of Fields were to Search</param>
		public List<dynamic> QuickSearch(string pSearchString, bool Apply = false, params string[] pFields)
		{
			List<dynamic> _ret = null;
			try
			{
				if (pSearchString != null && pSearchString != string.Empty)
				{
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

						var Results = from DataRow in JSonData select DataRow;

						//var predicate = PredicateBuilder.JToken<Newtonsoft.Json.Linq.JToken>();
						//foreach (var _Column in TConfiguration.columns)
						//{
						//	predicate = predicate.Or(p => p[_Column.field].Contains(pSearchString) == true);
						//}
						//var FData = JSonData.Children().Where(predicate);
						//Expression<Func<Newtonsoft.Json.Linq.JToken, bool>>  predicate = JSonData.Children().ContainsStrings(pSearchString);
						//var FData = JSonData.Children().Where(JSonData.Children().ContainsStrings(pSearchString));

						#region Expression Builder

						/* I could not make an Expression builder for dynamic data, therefore... this: */

						switch (pFields.Length)
						{
							case 1:
								Results = from DataRow in JSonData
										  where
											CompareStrings(DataRow[pFields[0]], pSearchString)
										  select DataRow;
								break;
							case 2:
								Results = from DataRow in JSonData
										  where
										  CompareStrings(DataRow[pFields[0]], pSearchString) ||
										  CompareStrings(DataRow[pFields[1]], pSearchString)
										  select DataRow;
								break;
							case 3:
								Results = from DataRow in JSonData
										  where
											CompareStrings(DataRow[pFields[0]], pSearchString) ||
											CompareStrings(DataRow[pFields[1]], pSearchString) ||
											CompareStrings(DataRow[pFields[2]], pSearchString)
										  select DataRow;
								break;
							case 4:
								Results = from DataRow in JSonData
										  where
											CompareStrings(DataRow[pFields[0]], pSearchString) ||
											CompareStrings(DataRow[pFields[1]], pSearchString) ||
											CompareStrings(DataRow[pFields[2]], pSearchString) ||
											CompareStrings(DataRow[pFields[3]], pSearchString)
										  select DataRow;
								break;
							case 5:
								Results = from DataRow in JSonData
										  where
											CompareStrings(DataRow[pFields[0]], pSearchString) ||
											CompareStrings(DataRow[pFields[1]], pSearchString) ||
											CompareStrings(DataRow[pFields[2]], pSearchString) ||
											CompareStrings(DataRow[pFields[3]], pSearchString) ||
											CompareStrings(DataRow[pFields[4]], pSearchString)
										  select DataRow;
								break;
							case 6:
								Results = from DataRow in JSonData
										  where
											CompareStrings(DataRow[pFields[0]], pSearchString) ||
											CompareStrings(DataRow[pFields[1]], pSearchString) ||
											CompareStrings(DataRow[pFields[2]], pSearchString) ||
											CompareStrings(DataRow[pFields[3]], pSearchString) ||
											CompareStrings(DataRow[pFields[4]], pSearchString) ||
											CompareStrings(DataRow[pFields[5]], pSearchString)
										  select DataRow;
								break;
							case 7:
								Results = from DataRow in JSonData
										  where
											CompareStrings(DataRow[pFields[0]], pSearchString) ||
											CompareStrings(DataRow[pFields[1]], pSearchString) ||
											CompareStrings(DataRow[pFields[2]], pSearchString) ||
											CompareStrings(DataRow[pFields[3]], pSearchString) ||
											CompareStrings(DataRow[pFields[4]], pSearchString) ||
											CompareStrings(DataRow[pFields[5]], pSearchString) ||
											CompareStrings(DataRow[pFields[6]], pSearchString)
										  select DataRow;
								break;
							case 8:
								Results = from DataRow in JSonData
										  where
											CompareStrings(DataRow[pFields[0]], pSearchString) ||
											CompareStrings(DataRow[pFields[1]], pSearchString) ||
											CompareStrings(DataRow[pFields[2]], pSearchString) ||
											CompareStrings(DataRow[pFields[3]], pSearchString) ||
											CompareStrings(DataRow[pFields[4]], pSearchString) ||
											CompareStrings(DataRow[pFields[5]], pSearchString) ||
											CompareStrings(DataRow[pFields[6]], pSearchString) ||
											CompareStrings(DataRow[pFields[7]], pSearchString)
										  select DataRow;
								break;
							case 9:
								Results = from DataRow in JSonData
										  where
											CompareStrings(DataRow[pFields[0]], pSearchString) ||
											CompareStrings(DataRow[pFields[1]], pSearchString) ||
											CompareStrings(DataRow[pFields[2]], pSearchString) ||
											CompareStrings(DataRow[pFields[3]], pSearchString) ||
											CompareStrings(DataRow[pFields[4]], pSearchString) ||
											CompareStrings(DataRow[pFields[5]], pSearchString) ||
											CompareStrings(DataRow[pFields[6]], pSearchString) ||
											CompareStrings(DataRow[pFields[7]], pSearchString) ||
											CompareStrings(DataRow[pFields[8]], pSearchString)
										  select DataRow;
								break;

							default: //Searching is limited to the first 10 columns:
								Results = from DataRow in JSonData
										  where
											CompareStrings(DataRow[pFields[0]], pSearchString) ||
											CompareStrings(DataRow[pFields[1]], pSearchString) ||
											CompareStrings(DataRow[pFields[2]], pSearchString) ||
											CompareStrings(DataRow[pFields[3]], pSearchString) ||
											CompareStrings(DataRow[pFields[4]], pSearchString) ||
											CompareStrings(DataRow[pFields[5]], pSearchString) ||
											CompareStrings(DataRow[pFields[6]], pSearchString) ||
											CompareStrings(DataRow[pFields[7]], pSearchString) ||
											CompareStrings(DataRow[pFields[8]], pSearchString) ||
											CompareStrings(DataRow[pFields[9]], pSearchString)
										  select DataRow;
								break;
						}

						#endregion

						List<Newtonsoft.Json.Linq.JToken> FilteredData = Results.ToList();

						//3. Convert the Filtered Data into a List of Dynamic Objects:
						if (FilteredData != null && FilteredData.Count > 0)
						{
							_ret = new List<dynamic>();
							foreach (Newtonsoft.Json.Linq.JToken item in FilteredData.ToList())
							{
								_ret.Add(item.ToObject<dynamic>());
							}

							//If true, the Filtered data becomes the Data to show on the Table
							//Need to call the 'Build_Table..' method to actually draw the Table.
							if (Apply)
							{
								DataSource = _ret;
							}
						}
						else
						{
							//No Data Found for the Filter, Returns an Empty (not Null) List:
							_ret = new List<dynamic>();
							if (Apply)
							{
								DataSource = _ret;
							}
						}
					}
					else
					{
						throw new Exception("Eror 404 - No Data Loaded on the Table.");
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return _ret;
		}

		/// <summary>Updates the Shown Data using the Original Data.</summary>
		public void RefreshData()
		{
			try
			{
				if (OriginalData != null && OriginalData.Count > 0)
				{
					DataSource = Newtonsoft.Json.JsonConvert.DeserializeObject<List<dynamic>>(
									  Newtonsoft.Json.JsonConvert.SerializeObject(OriginalData)
								);
				}
			}
			catch (Exception ex) { throw ex; }
		}

		/// <summary>Updates the Shown Data using a new DataSet, this new data becomes the Original Data now.</summary>
		/// <param name="pNewData">New DataSource</param>
		public void RefreshData(dynamic pNewData)
		{
			try
			{
				//if (pNewData != null)
				//{
				//	Console.WriteLine(pNewData);

				//	//Convierte 'Newtonsoft.Json.Linq.JArray' a 'List<dynamic>':
				//	if (pNewData is Newtonsoft.Json.Linq.JArray)
				//	{
				//		OriginalData = pNewData.ToObject<List<dynamic>>(); //<- Preserves the Un-Sorted Data.
				//	}
				//	else
				//	{
				//		OriginalData = Newtonsoft.Json.JsonConvert.DeserializeObject<List<dynamic>>(
				//							Newtonsoft.Json.JsonConvert.SerializeObject(pNewData)
				//						);
				//	}

				//}

				//Updates the Shown DataSet with new data:
				if (pNewData != null && pNewData.Count > 0)
				{
					Console.WriteLine(pNewData);
					OriginalData = pNewData;
					DataSource = OriginalData;

					JSonData = Newtonsoft.Json.Linq.JArray.FromObject(
									Newtonsoft.Json.JsonConvert.DeserializeObject(
										Newtonsoft.Json.JsonConvert.SerializeObject(OriginalData)
									)
					);

					long Index = 0;
					this.WorkingData = new List<dynamic>();
					Console.WriteLine(JSonData[0].GetType());
					foreach (var Data in JSonData)
					{
						WorkingData.Add(new { Index, Data }); //<- Newtonsoft.Json.Linq.JObject
						Index++;
					}
				}
			}
			catch (Exception ex) { throw ex; }
		}

		/// <summary>Returns the Information of the Cell that cointains the indicated Pixel.</summary>
		/// <param name="Pixel">Coordinates of the pixel to look for.</param>
		public ImageMapping GetPixelInfo(Point Pixel)
		{
			ImageMapping _ret = null;
			try
			{
				if (this.CellBoxMappings != null && this.CellBoxMappings.Count > 0)
				{
					foreach (var item in this.CellBoxMappings)
					{
						if (item.IsPixelInBox(Pixel))
						{
							_ret = item;
							break;
						}
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return _ret;
		}

		/// <summary>Gets the Index of an element in the 'OriginalData' datasource.</summary>
		/// <param name="PixelInfo">dynamic JObject to compare</param>
		public int GetPixelDataIndex(dynamic PixelInfo)
		{
			int _ret = -1;
			try
			{
				if (PixelInfo != null && this.OriginalData != null)
				{
					int Index = 0;
					foreach (dynamic data in this.OriginalData)
					{
						
						if (Newtonsoft.Json.Linq.JToken.DeepEquals(JObject.FromObject(data), PixelInfo))
						{
							_ret = Index; break;
						}
						Index++;
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return _ret;
		}

		#endregion

		#region Utility Methods

		

		/// <summary>Returns 'true' if SearchString Contains pValue.
		/// <para>Culture Invariant Case Insensitive.</para> </summary>
		/// <param name="pValue"></param>
		/// <param name="pSearchString"></param>
		private bool CompareStrings(object pValue, string pSearchString)
		{
			bool _ret = false;
			if (pSearchString != null)
			{
				_ret = System.Globalization.CultureInfo.InvariantCulture.
					CompareInfo.IndexOf(
						GetStringValue(pValue), pSearchString,
						System.Globalization.CompareOptions.IgnoreCase
				) >= 0;
			}
			return _ret;
		}
		private string GetStringValue(object pValue)
		{
			string _ret = string.Empty;
			if (pValue != null)
			{
				_ret = Convert.ToString(pValue);
			}

			return _ret;
		}

		/// <summary>Get the Values for the Summary Fields.
		/// <para>Supported Functions: COUNT, SUM, AVG, MAX, MIN, FIRST, LAST</para> </summary>
		/// <param name="Columns">Array of All Columns</param>
		/// <param name="Summaries">Array of All Summaries, Supported: COUNT, SUM, AVG, MAX, MIN, FIRST, LAST</param>
		private List<decimal> GetSummaryValues(dynamic Summaries, dynamic Data)
		{
			List<decimal> _ret = null;
			try
			{
				decimal[] sumaryValues = new decimal[Summaries.Count];
				decimal SumValue = 0;
				decimal CountValue = 0;

				if (Data != null && Data.Count > 0 && Summaries != null && Summaries.Count > 0)
				{
					foreach (var _dataRow in Data)
					{
						for (int SummaryIndex = 0; SummaryIndex < Summaries.Count; SummaryIndex++)
						{
							var propertyInfo = System.ComponentModel.TypeDescriptor.
								GetProperties((object)_dataRow).
								Find(Summaries[SummaryIndex].field.ToString(), true);

							string FieldName = propertyInfo.Name;
							object FieldValue = propertyInfo.GetValue(_dataRow);
							decimal CurrentValue = 0;

							switch (Summaries[SummaryIndex].agregate.ToString())
							{
								case "COUNT":
									sumaryValues[SummaryIndex] += 1;
									break;

								case "SUM":
									sumaryValues[SummaryIndex] += Convert.ToDecimal(FieldValue);
									break;

								case "AVG":
									SumValue += Convert.ToDecimal(FieldValue);
									CountValue += 1;
									sumaryValues[SummaryIndex] = SumValue / CountValue;
									break;

								case "FIRST":
									sumaryValues[SummaryIndex] = Convert.ToDecimal(propertyInfo.GetValue(Data[0]));
									break;

								case "LAST":
									sumaryValues[SummaryIndex] = Convert.ToDecimal(propertyInfo.GetValue(Data[Data.Count - 1]));
									break;

								case "MAX":
									CurrentValue = Convert.ToDecimal(FieldValue);
									if (CurrentValue > sumaryValues[SummaryIndex])
									{
										sumaryValues[SummaryIndex] = CurrentValue;
									}
									break;
								case "MIN":
									CurrentValue = Convert.ToDecimal(FieldValue);
									if (CurrentValue < sumaryValues[SummaryIndex])
									{
										sumaryValues[SummaryIndex] = CurrentValue;
									}
									break;

								default:
									break;
							}
						}
					}
					_ret = new List<decimal>(sumaryValues);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return _ret;
		}

		/// <summary>Gets the Value for a Calculated Field.
		/// <para>Supported Functions: COUNT, SUM, AVG, MAX, MIN, FIRST, LAST, CONCAT</para> </summary>
		/// <param name="pExpression">COUNT(field), SUM(field,field2,field3), CONCAT(field,field2) etc.</param>
		/// <param name="pColumn">Column Information</param>
		private object GetExpressionValue(string pExpression, Column pColumn, dynamic RowData)
		{
			object _ret = null;
			try
			{
				if (pExpression != null && pExpression != string.Empty && RowData != null)
				{
					//1. Get the Agregate Function:
					string[] Palabras = pExpression.Split(new char[] { '(' });
					string Function = Palabras[0];

					//2. Get the involved Fields:
					string[] Fields = Palabras[1].Replace(")", "").
									  Split(new char[] { ',' });

					//3. Get the Values for the Fields:
					List<object> Values = new List<object>();
					foreach (string _Field in Fields)
					{
						var propertyInfo = System.ComponentModel.TypeDescriptor.
							GetProperties((object)RowData).
							Find(_Field, true);

						string FieldName = propertyInfo.Name;
						object FieldValue = propertyInfo.GetValue(RowData);

						if (FieldValue != null && FieldValue.ToString() != string.Empty)
						{
							Values.Add(FieldValue);
						}
					}
					string ConcaText = string.Empty;
					decimal CalculatedValue = 0;

					switch (Function)
					{
						case "COUNT": CalculatedValue = (decimal)Values.Count; break;
						case "SUM":
							foreach (var _value in Values)
							{
								CalculatedValue += Convert.ToDecimal(_value);
							}
							break;
						case "MIN":
							foreach (var _value in Values)
							{
								if (Convert.ToDecimal(_value) < CalculatedValue)
								{
									CalculatedValue = Convert.ToDecimal(_value);
								}
							}
							break;
						case "MAX":
							foreach (var _value in Values)
							{
								if (Convert.ToDecimal(_value) > CalculatedValue)
								{
									CalculatedValue = Convert.ToDecimal(_value);
								}
							}
							break;
						case "AVG":
							foreach (var _value in Values)
							{
								CalculatedValue += Convert.ToDecimal(_value);
							}
							CalculatedValue = CalculatedValue / Values.Count;
							break;

						case "FIRST":
							CalculatedValue = Convert.ToDecimal(Values[0]);
							break;

						case "LAST":
							CalculatedValue = Convert.ToDecimal(Values[Values.Count - 1]);
							break;

						case "CONCAT":
							foreach (var _value in Values)
							{
								ConcaText = string.Format("{0} {1}", ConcaText, Convert.ToString(_value));
							}
							break;
						default:
							break;
					}

					if (Function == "CONCAT")
					{
						_ret = ConcaText;
					}
					else
					{
						_ret = CalculatedValue;
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return _ret;
		}

		/// <summary>Text Alignment for Text Tables Only.</summary>
		/// <param name="pText">Text String</param>
		/// <param name="MaxLenght">Column Width</param>
		/// <param name="pAlign">'left', 'center' or 'right', default: 'center'</param>
		private string AlinearTexto(string pText, int MaxLenght, string pAlign = "center")
		{
			string _ret = string.Empty;
			try
			{
				pText = pText.Trim();
				if (pText.Length < MaxLenght)
				{
					//Si es menor, se rellena con 'Fill':
					switch (pAlign)
					{
						case "left": _ret = pText.PadRight(MaxLenght); break;
						case "center": _ret = PadBoth(pText, MaxLenght); break;
						case "right": _ret = pText.PadLeft(MaxLenght); break;
						default: break;
					}
				}
				else if (pText.Length > MaxLenght)
				{
					//Si es mayor, se recorta:
					_ret = pText.Substring(0, MaxLenght);
				}
				else
				{
					//Si es Igual, devuelve la misma:
					_ret = pText;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return _ret;
		}
		private string PadBoth(string source, int length, char Fill = ' ')
		{
			int spaces = length - source.Length;
			int padLeft = spaces / 2 + source.Length;
			return source.PadLeft(padLeft, Fill).PadRight(length, Fill);

		}

		/// <summary>Text Alignment for Image Tables Only.</summary>
		/// <param name="Box">The Cell containing the Text</param>
		/// <param name="TextSize">Size of the Text</param>
		/// <param name="Alignment">'left', 'center' or 'right', default: 'center'</param>
		/// <param name="Padding">Horizontal Padding, default 2px.</param>
		public Point AlinearTexto(Rectangle Box, SizeF TextSize, string Alignment = "center", int Padding = 2)
		{
			Point TextPosition = new Point(0, 0);
			try
			{
				switch (Alignment)
				{
					case "left":
						TextPosition = new Point(
							Box.X + Padding,
							Convert.ToInt32((Box.Height - TextSize.Height) / 2) + Box.Y
						);
						break;
					case "right":
						TextPosition = new Point(
							  Convert.ToInt32(Box.X + (Box.Width - TextSize.Width) - Padding),
							  Convert.ToInt32((Box.Height - TextSize.Height) / 2) + Box.Y
						  );
						break;
					default:
						TextPosition = new Point(
							Convert.ToInt32((Box.Width - TextSize.Width) / 2) + Box.X,
							Convert.ToInt32((Box.Height - TextSize.Height) / 2) + Box.Y
						);
						break;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return TextPosition;
		}

		private string AplicarFormato(object pValue, dynamic pColumn)
		{
			string _ret = pValue != null ? pValue.ToString() : string.Empty;
			string myFormat = (pColumn.format != null && pColumn.format.ToString() != string.Empty) ? pColumn.format.ToString() : string.Empty;
			try
			{
				string myType = (pColumn.type != null && pColumn.type.ToString() != string.Empty) ? pColumn.type.ToString() : "string";

				if (myFormat != string.Empty && _ret != string.Empty)
				{
					switch (myType)
					{
						case "int": _ret = string.Format(myFormat, Convert.ToInt32(pValue)); break;
						case "long": _ret = string.Format(myFormat, Convert.ToInt64(pValue)); break;
						case "decimal": _ret = string.Format(myFormat, Convert.ToDecimal(pValue)); break;
						case "DateTime": _ret = string.Format(myFormat, Convert.ToDateTime(pValue)); break;
						case "string": _ret = string.Format(myFormat, Convert.ToString(pValue)); break;
						case "boolean": _ret = string.Format(myFormat, Convert.ToBoolean(pValue)); break;
						case "Calculated": _ret = string.Format(myFormat, Convert.ToDecimal(pValue)); break;
						default:
							_ret = string.Format(myFormat, Convert.ToString(pValue));
							break;
					}
				}
			}
			catch (Exception ex)
			{
				_ret = string.Format(myFormat, pValue);
				//MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return _ret;
		}
		private string AplicarFormato(object pValue, string pFormat, string pDataType)
		{
			string _ret = pValue != null ? pValue.ToString() : string.Empty;
			string myFormat = (pFormat != null && pFormat != string.Empty) ? pFormat : string.Empty;
			string myType = (pDataType != null && pDataType != string.Empty) ? pDataType : "string";

			try
			{
				if (myFormat != string.Empty && _ret != string.Empty)
				{
					switch (myType)
					{
						case "int": _ret = string.Format(myFormat, Convert.ToInt32(pValue)); break;
						case "long": _ret = string.Format(myFormat, Convert.ToInt64(pValue)); break;
						case "decimal": _ret = string.Format(myFormat, Convert.ToDecimal(pValue)); break;
						case "DateTime": _ret = string.Format(myFormat, Convert.ToDateTime(pValue)); break;
						case "string": _ret = string.Format(myFormat, Convert.ToString(pValue)); break;
						case "boolean": _ret = string.Format(myFormat, Convert.ToBoolean(pValue)); break;
						case "Calculated": _ret = string.Format(myFormat, Convert.ToDecimal(pValue)); break;
						default:
							_ret = string.Format(myFormat, Convert.ToString(pValue));
							break;
					}
				}
			}
			catch (Exception ex)
			{
				_ret = string.Format(myFormat, pValue);
				//MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return _ret;
		}


		private Bitmap DrawNewImage(Size size)
		{
			var image = new Bitmap(size.Width, size.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			using (var g = Graphics.FromImage(image))
			{
				//Todas las opciones en Alta Calidad:
				g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
				g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
				g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
				g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

				g.Clear(Color.Transparent); //<- Background
				g.Flush();
				return new System.Drawing.Bitmap(image, size.Width, size.Height);
			}
		}

		private Color StringToColor(string pARGBColor)
		{
			Color _ret = Color.Transparent;
			try
			{
				if (pARGBColor != null && pARGBColor != string.Empty)
				{
					var ColorArray = pARGBColor.Split(new char[] { ',' });
					if (ColorArray != null && ColorArray.Length > 0)
					{
						_ret = Color.FromArgb(
							Convert.ToInt32(ColorArray[0]),
							Convert.ToInt32(ColorArray[1]),
							Convert.ToInt32(ColorArray[2]),
							Convert.ToInt32(ColorArray[3])
						);
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return _ret;
		}

		private Dictionary<string, object> GetPropertyKeysForDynamic(dynamic dynamicToGetPropertiesFor)
		{
			Newtonsoft.Json.Linq.JObject attributesAsJObject = dynamicToGetPropertiesFor;
			return attributesAsJObject.ToObject<Dictionary<string, object>>();
		}
		public List<string> GetPropertyKeysForDynamicEx(dynamic dynamicToGetPropertiesFor)
		{
			JObject attributesAsJObject = dynamicToGetPropertiesFor;
			Dictionary<string, object> values = attributesAsJObject.ToObject<Dictionary<string, object>>();
			List<string> toReturn = new List<string>();
			foreach (string key in values.Keys)
			{
				toReturn.Add(key);
			}
			return toReturn;
		}

		/// <summary>Sorts Dynamic data by multiple fields (Up to 4 Columns).</summary>
		/// <param name="Input">Data to be Sorted.</param>
		/// <param name="SortingFields">List of Fields to be sorted, can add 'ASC' or 'DESC' to indicate the Sort Direction.</param>
		private List<dynamic> DynamicSorting(dynamic Input, List<string> SortingFields)
		{
			List<dynamic> _ret = null;
			try
			{
				if (SortingFields != null && SortingFields.Count > 0)
				{
					//Des-Serializa la lista de Campos a Ordenar:
					var FieldList = new List<KeyValuePair<string, string>>(); //<- field_name, Direction
					foreach (string field in SortingFields)
					{
						if (field != string.Empty)
						{
							string[] Palabras = field.Split(new char[] { ' ' });
							string FieldName = Palabras[0];
							string Direction = (Palabras.Length > 1) ? Palabras[1] : "ASC";

							FieldList.Add(new KeyValuePair<string, string>(Palabras[0], Direction));
						}
					}

					List<dynamic> DataInput = null;
					bool IsDynamicObject = (Input is System.Dynamic.IDynamicMetaObjectProvider);

					//Convierte 'Newtonsoft.Json.Linq.JArray' a 'List<dynamic>':
					if (Input is Newtonsoft.Json.Linq.JArray)
					{
						DataInput = Input.ToObject<List<dynamic>>(); //<- Preserves the Un-Sorted Data.
					}
					else
					{
						DataInput = Newtonsoft.Json.JsonConvert.DeserializeObject<List<dynamic>>(
									Newtonsoft.Json.JsonConvert.SerializeObject(Input)
								);
					}

					if (DataInput != null && DataInput.Count > 0)
					{
						DataInput.Sort((a, b) =>
						{
							int result = 0;
							System.ComponentModel.PropertyDescriptor propA;
							System.ComponentModel.PropertyDescriptor propB;

							//1º Field:
							if (result == 0 && FieldList.Count >= 1)
							{
								propA = System.ComponentModel.TypeDescriptor.
											GetProperties((object)a).
											Find(FieldList[0].Key, true);
								propB = System.ComponentModel.TypeDescriptor.
											GetProperties((object)b).
											Find(FieldList[0].Key, true);

								if (FieldList[0].Value == "ASC")
								{
									result = propA.GetValue(a).
									   CompareTo(propB.GetValue(b)
									);
								}
								if (FieldList[0].Value == "DESC")
								{
									result = propB.GetValue(b).
									   CompareTo(propA.GetValue(a)
									);
								}
							}

							//2º Field:
							if (result == 0 && FieldList.Count >= 2)
							{
								propA = System.ComponentModel.TypeDescriptor.
											GetProperties((object)a).
											Find(FieldList[1].Key, true);
								propB = System.ComponentModel.TypeDescriptor.
											GetProperties((object)b).
											Find(FieldList[1].Key, true);

								if (FieldList[1].Value == "ASC")
								{
									result = propA.GetValue(a).
									   CompareTo(propB.GetValue(b)
									);
								}
								if (FieldList[1].Value == "DESC")
								{
									result = propB.GetValue(b).
									   CompareTo(propA.GetValue(a)
									);
								}
							}

							//3º Field:
							if (result == 0 && FieldList.Count >= 3)
							{
								propA = System.ComponentModel.TypeDescriptor.
											GetProperties((object)a).
											Find(FieldList[2].Key, true);
								propB = System.ComponentModel.TypeDescriptor.
											GetProperties((object)b).
											Find(FieldList[2].Key, true);

								if (FieldList[2].Value == "ASC")
								{
									result = propA.GetValue(a).
									   CompareTo(propB.GetValue(b)
									);
								}
								if (FieldList[2].Value == "DESC")
								{
									result = propB.GetValue(b).
									   CompareTo(propA.GetValue(a)
									);
								}
							}

							//4º Field:
							if (result == 0 && FieldList.Count == 4) //<- Up to 4 Sorting Fields
							{
								propA = System.ComponentModel.TypeDescriptor.
											GetProperties((object)a).
											Find(FieldList[3].Key, true);
								propB = System.ComponentModel.TypeDescriptor.
											GetProperties((object)b).
											Find(FieldList[3].Key, true);

								if (FieldList[3].Value == "ASC")
								{
									result = propA.GetValue(a).
									   CompareTo(propB.GetValue(b)
									);
								}
								if (FieldList[3].Value == "DESC")
								{
									result = propB.GetValue(b).
									   CompareTo(propA.GetValue(a)
									);
								}
							}

							return result;
						});
					}
					_ret = DataInput;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return _ret;
		}

		/// <summary>Groups Dynamic Data by multiple fields (up to 4 Columns)</summary>
		/// <param name="Input">Data to be Sorted.</param>
		/// <param name="SortingFields">List of Fields to do the Grouping.</param>
		private List<GroupData> DynamicGrouping(dynamic Input, List<string> SortingFields)
		{
			List<GroupData> _ret = null;
			try
			{
				//0. Convert the Data into a Dynamic List, because this method is designed for Anonymous data (dynamic).
				bool IsDynamicObject = (Input is System.Dynamic.IDynamicMetaObjectProvider);
				List<dynamic> DataSorted = new List<dynamic>();

				if (IsDynamicObject)
				{
					DataSorted = new List<dynamic>(Input);
				}
				else
				{
					string ToJSON = Newtonsoft.Json.JsonConvert.SerializeObject(Input);
					DataSorted = Newtonsoft.Json.JsonConvert.DeserializeObject<List<dynamic>>(ToJSON);
					//DataSorted = DataSorted.ToObject<List<dynamic>>();
				}

				if (DataSorted != null && DataSorted.Count > 0)
				{
					List<Column> Columns = new List<Column>();
					_ret = new List<GroupData>();
					dynamic Uniques = null;

					//Get the Columns Definitions:					
					foreach (string _Col in SortingFields)
					{
						Columns.Add(TConfiguration.columns.Find(x => x.field == _Col));
					}

					//1. Get Unique Values for each Group Field:  
					if (SortingFields.Count == 1)
					{
						var grouping = from s in DataSorted
									   group s by new
									   {
										   filter1 = GetPropertyValueX(s, SortingFields[0])
									   } into GR
									   select new
									   {
										   Group = GR.First(),
										   Count = GR.Count(),
										   Filter1 = GR.Key.filter1.Parent,
									   };
						Uniques = grouping.ToList();

					}
					if (SortingFields.Count == 2)
					{
						var grouping = from s in DataSorted
									   group s by new
									   {
										   filter1 = GetPropertyValueX(s, SortingFields[0]),
										   filter2 = GetPropertyValueX(s, SortingFields[1])
									   } into GR
									   select new
									   {
										   Group = GR.First(),
										   Count = GR.Count(),
										   Filter1 = GR.Key.filter1.Parent,
										   Filter2 = GR.Key.filter2.Parent,
									   };
						Uniques = grouping.ToList();
					}
					if (SortingFields.Count == 3)
					{
						var grouping = from s in DataSorted
									   group s by new
									   {
										   filter1 = GetPropertyValueX(s, SortingFields[0]),
										   filter2 = GetPropertyValueX(s, SortingFields[1]),
										   filter3 = GetPropertyValueX(s, SortingFields[2])
									   } into GR
									   select new
									   {
										   Group = GR.First(),
										   Count = GR.Count(),
										   Filter1 = GR.Key.filter1.Parent,
										   Filter2 = GR.Key.filter2.Parent,
										   Filter3 = GR.Key.filter3.Parent
									   };
						Uniques = grouping.ToList();
					}
					if (SortingFields.Count == 4)
					{
						var grouping = from s in DataSorted
									   group s by new
									   {
										   filter1 = GetPropertyValueX(s, SortingFields[0]),
										   filter2 = GetPropertyValueX(s, SortingFields[1]),
										   filter3 = GetPropertyValueX(s, SortingFields[2]),
										   filter4 = GetPropertyValueX(s, SortingFields[3])
									   } into GR
									   select new
									   {
										   Group = GR.First(),
										   Count = GR.Count(),
										   Filter1 = GR.Key.filter1.Parent,
										   Filter2 = GR.Key.filter2.Parent,
										   Filter3 = GR.Key.filter3.Parent,
										   Filter4 = GR.Key.filter4.Parent
									   };
						Uniques = grouping.ToList();
					}

					if (Uniques != null)
					{
						foreach (var Group in Uniques)
						{
							dynamic FilteredData = null;

							if (SortingFields.Count == 1)
							{
								//2. Find all Data having the Unique Value in the Group Field:
								FilteredData = DataSorted.FindAll(y =>
									GetPropertyValueX(y, SortingFields[0]) == GetPropertyValueX(Group.Group, SortingFields[0])
								);
								if (FilteredData != null)
								{
									var PropValue_0 = GetPropertyValueX(Group.Group, Columns[0].field);

									//3. Create a new Group with the Data:
									_ret.Add(new GroupData()
									{
										Columns = Columns,
										data = FilteredData,
										Count = Group.Count,
										HeaderData = string.Format("[{0}: {1}]",
											Columns[0].title, AplicarFormato(PropValue_0, Columns[0]))
									});
								}
							}
							if (SortingFields.Count == 2)
							{
								FilteredData = DataSorted.FindAll(y =>
									GetPropertyValueX(y, SortingFields[0]) == GetPropertyValueX(Group.Group, SortingFields[0]) &&
									GetPropertyValueX(y, SortingFields[1]) == GetPropertyValueX(Group.Group, SortingFields[1])
								);
								if (FilteredData != null)
								{
									var PropValue_0 = GetPropertyValueX(Group.Group, Columns[0].field);
									var PropValue_1 = GetPropertyValueX(Group.Group, Columns[1].field);

									//3. Create a new Group with the Data:
									_ret.Add(new GroupData()
									{
										Columns = Columns,
										data = FilteredData,
										Count = Group.Count,
										HeaderData = string.Format("[{0}: {1}], [{2}: {3}]",
											Columns[0].title, AplicarFormato(PropValue_0, Columns[0]),
											Columns[1].title, AplicarFormato(PropValue_1, Columns[1])
										)
									});
								}
							}
							if (SortingFields.Count == 3)
							{
								FilteredData = DataSorted.FindAll(y =>
									GetPropertyValueX(y, SortingFields[0]) == GetPropertyValueX(Group.Group, SortingFields[0]) &&
									GetPropertyValueX(y, SortingFields[1]) == GetPropertyValueX(Group.Group, SortingFields[1]) &&
									GetPropertyValueX(y, SortingFields[2]) == GetPropertyValueX(Group.Group, SortingFields[2])
								);
								if (FilteredData != null)
								{
									var PropValue_0 = GetPropertyValueX(Group.Group, Columns[0].field);
									var PropValue_1 = GetPropertyValueX(Group.Group, Columns[1].field);
									var PropValue_2 = GetPropertyValueX(Group.Group, Columns[2].field);

									//3. Create a new Group with the Data:
									_ret.Add(new GroupData()
									{
										Columns = Columns,
										data = FilteredData,
										Count = Group.Count,
										HeaderData = string.Format("[{0}: {1}], [{2}: {3}], [{4}: {5}]",
											Columns[0].title, AplicarFormato(PropValue_0, Columns[0]),
											Columns[1].title, AplicarFormato(PropValue_1, Columns[1]),
											Columns[2].title, AplicarFormato(PropValue_2, Columns[2])
										)
									});
								}
							}
							if (SortingFields.Count == 4)
							{
								FilteredData = DataSorted.FindAll(y =>
									GetPropertyValueX(y, SortingFields[0]) == GetPropertyValueX(Group.Group, SortingFields[0]) &&
									GetPropertyValueX(y, SortingFields[1]) == GetPropertyValueX(Group.Group, SortingFields[1]) &&
									GetPropertyValueX(y, SortingFields[2]) == GetPropertyValueX(Group.Group, SortingFields[2]) &&
									GetPropertyValueX(y, SortingFields[3]) == GetPropertyValueX(Group.Group, SortingFields[3])
								);
								if (FilteredData != null)
								{
									var PropValue_0 = GetPropertyValueX(Group.Group, Columns[0].field);
									var PropValue_1 = GetPropertyValueX(Group.Group, Columns[1].field);
									var PropValue_2 = GetPropertyValueX(Group.Group, Columns[2].field);
									var PropValue_3 = GetPropertyValueX(Group.Group, Columns[3].field);

									//3. Create a new Group with the Data:
									_ret.Add(new GroupData()
									{
										Columns = Columns,
										data = FilteredData,
										Count = Group.Count,
										HeaderData = string.Format("[{0}: {1}], [{2}: {3}], [{4}: {5}], [{6}: {7}]",
											Columns[0].title, AplicarFormato(PropValue_0, Columns[0]),
											Columns[1].title, AplicarFormato(PropValue_1, Columns[1]),
											Columns[2].title, AplicarFormato(PropValue_2, Columns[2]),
											Columns[3].title, AplicarFormato(PropValue_3, Columns[3])
										)
									});
								}
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return _ret;
		}


		private static object GetPropertyValue(object obj, string propertyName)
		{
			return obj.GetType().GetProperty(propertyName).GetValue(obj, null);
		}
		private static object GetPropertyValueX(object obj, string propertyName)
		{
			return System.ComponentModel.TypeDescriptor.
											GetProperties((object)obj).
											Find(propertyName, true).
											GetValue(obj);
		}
		private static object GetPropertyX(object obj, string propertyName)
		{
			return System.ComponentModel.TypeDescriptor.
											GetProperties((object)obj).
											Find(propertyName, true);
		}

		/// <summary>Do a quick Text Search on all fields querying for the Search String.</summary>
		/// <param name="pSearchString">Text to query for.</param>
		private List<dynamic> QuickSearch_Path(string pSearchString)
		{
			/* DEPRECATED !
			 * Due this method only works with text fields and JSONPath doesnt give much freedom to custom queries */
			List<dynamic> _ret = null;
			try
			{
				var Filters = new List<FilterCriteria>();
				_ret = QuickSearch_Path(pSearchString, out Filters);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return _ret;
		}
		/// <summary>Do a quick Text Search on all fields querying for the Search String.</summary>
		/// <param name="pSearchString">Text to query for.</param>
		/// <param name="Filters">Ouputs the builded Filters for the Query.</param>
		private List<dynamic> QuickSearch_Path(string pSearchString, out List<FilterCriteria> Filters)
		{
			/* DEPRECATED !
			 * Due this method only works with text fields and JSONPath doesnt give much freedom to custom queries */
			List<dynamic> _ret = null;
			Filters = new List<FilterCriteria>();

			try
			{
				//We need to build a JSONPath with expressions for each field that contains the SearchString,
				//Something like this:
				//$[?(@.Field1 =~ /.*blue.*/i || @.Field2 =~ /.*blue.*/i || @.Field3 =~ /.*blue.*/i || @.Field4 =~ /.*blue.*/i || @.ColorField =~ /.*blue.*/i)]

				int Counter = 0;
				string Expresion = string.Empty;

				foreach (Column field in TConfiguration.columns)
				{
					if (field.type != "Calculated")
					{
						var Filter = new FilterCriteria(field.field, pSearchString)
						{
							isFirst = (Counter == 0),
							comparator = "CONTAINS",
							isOR = true
						};
						Expresion += Filter.ToString();
						Filters.Add(Filter);
						Counter++;
					}
				}
				//Assemble the whole Expression:
				Expresion = string.Format("$[?({0})]", Expresion);

				//Then Get the Data for it:
				_ret = FilterData(Expresion, true);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return _ret;
		}

		#endregion
	}
}
