﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Windows.Forms;


namespace TextDataTable
{
	/// <summary>Class able to produce DataTables in Text Mode.
	/// Author: Jhollman Chacon (Blue Mystic) - 2022</summary>
	public class TextDataTable
	{
		public TextDataTable() { }
		public TextDataTable(string pConfigJsonFile)
		{
			ConfigJsonFile = pConfigJsonFile;
			LoadConfiguration();
		}

		#region Public Properties

		/// <summary>Full Path to the JSON Config file.</summary>
		public string ConfigJsonFile { get; set; }

		public TableConfiguration TConfiguration { get; set; }

		/// <summary>Set or Return the Data Shown in the Table.</summary>
		public dynamic DataSource { get; set; }

		/// <summary>Set or Return the Un-Sorted, un-grouped Original Data.
		/// <para>Use 'RefreshData()' method to Restore the Shown Data to it's Original.</para>
		/// <para>Use 'RefreshData(newData)' to set a new DataSet</para></summary>
		public List<dynamic> OriginalData { get; set; }

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
					int Margin = TConfiguration.properties.table.cell_padding;
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
							ColumnSize = (int)_Column.length + (Margin * 2);
							TableSize += ColumnSize + 1;
						}
						TableSize++;
					}

					if (TConfiguration.header != null)
					{
						//1. Crea la Caja para el Header:
						CellText = TConfiguration.header.title;
						ColumnSize = TConfiguration.header.length + (Margin * 2);

						//Linea Superior:
						Lines.AppendLine(AlinearTexto(
							string.Format("{0}{1}{2}",
								Borders.Top.Left.ToString(),
								new string(Convert.ToChar(Borders.Top.Border), ColumnSize),
								Borders.Top.Right.ToString()
						), TableSize, TConfiguration.header.align));

						//Lineas Laterales y Texto:
						Lines.AppendLine(AlinearTexto(
							string.Format("{0}{1}{2}",
								Borders.Sides.Left.ToString(),
								AlinearTexto(CellText, ColumnSize, "center"),
								Borders.Sides.Right.ToString()
						), TableSize, TConfiguration.header.align));

						//Linea Inferior:
						Lines.AppendLine(AlinearTexto(
							string.Format("{0}{1}{2}",
								Borders.Bottom.Left.ToString(),
								new string(Convert.ToChar(Borders.Bottom.Border), ColumnSize),
								Borders.Bottom.Right.ToString()
						), TableSize, TConfiguration.header.align));
					}

					#endregion

					#region Data Sorting

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
					//If Sorting is not Enabled, Data keeps the original order.

					#endregion

					#region Data Grouping

					List<string> GroupColumnHeaders = null; //<- Used only when  'repeat_column_headers' is false.

					if (DataSource != null && DataSource.Count > 0)
					{
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
							if (group.Header != null && group.Header.Count > 0)
							{
								foreach (string linea in group.Header)
								{
									Lines.AppendLine(linea);
								}
							}
							if (group.Body != null && group.Body.Count > 0)
							{
								foreach (string linea in group.Body)
								{
									Lines.AppendLine(linea);
								}
								TableSize = group.Body[group.Body.Count - 1].Length;
							}
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
									if (ColumnIndex == 0)
									{
										//Si es la Primera Columna:
										CornerL = (RowIndex >= 0) ?
											Borders.Middle.Left :
											Borders.Top.Left;
									}
									else if (ColumnIndex == TConfiguration.columns.Count)
									{
										//si es la ultima columna:
										CornerL = (RowIndex >= 0) ?
											Borders.Middle.Right :
											Borders.Top.Middle;
									}
									else
									{
										//Si es una Columna del Medio:
										CornerL = (RowIndex < 0) ?
											Borders.Top.Middle :
											Borders.Middle.Middle;
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

									if (ColunmPositions.Count < TConfiguration.columns.Count)
									{
										ColunmPositions.Add(Cell_Top.Length);
									}

									//Linea Superior:
									Cell_Top += string.Format("{0}{1}",
										CornerL,
										new string(Convert.ToChar(Borders.Top.Border), ColumnSize)
									);

									//Lineas Laterales y Texto:
									Cell_Mid += string.Format("{0}{1}",
										Borders.Sides.Left.ToString(),
										CellText
									);

									//si es la ultima columna, agrega el Borde:
									if (ColumnIndex >= TConfiguration.columns.Count - 1)
									{
										Cell_Top += (RowIndex >= 0) ? Convert.ToString(Borders.Middle.Right) : Convert.ToString(Borders.Top.Right);
										Cell_Mid += Convert.ToString(Borders.Middle.Border);
									}

									ColumnIndex++;
								}

								Lines.AppendLine(Cell_Top);
								Lines.AppendLine(Cell_Mid);
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

						#region Summary

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

					#region Footer

					if (TConfiguration.footer != null)
					{
						CellText = TConfiguration.footer.title.ToString();
						ColumnSize = TConfiguration.footer.length + (Margin * 2);

						//Linea Superior:
						Lines.AppendLine(AlinearTexto(
							string.Format("{0}{1}{2}",
								Borders.Top.Left.ToString(),
								new string(Convert.ToChar(Borders.Top.Border), ColumnSize),
								Borders.Top.Right.ToString()
						), TableSize, TConfiguration.footer.align));

						//Lineas Laterales y Texto:
						Lines.AppendLine(AlinearTexto(
							string.Format("{0}{1}{2}",
								Borders.Sides.Left.ToString(),
								AlinearTexto(CellText, ColumnSize),
								Borders.Sides.Right.ToString()
						), TableSize, TConfiguration.footer.align));

						//Linea Inferior:
						Lines.AppendLine(AlinearTexto(
							string.Format("{0}{1}{2}",
								Borders.Bottom.Left.ToString(),
								new string(Convert.ToChar(Borders.Bottom.Border), ColumnSize),
								Borders.Bottom.Right.ToString()
						), TableSize, TConfiguration.footer.align));
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

		public Bitmap Build_ImageDataTable(Size pImageSize)
		{
			System.Drawing.Bitmap image = null;
			try
			{
				if (TConfiguration != null)
				{
					#region Config

					Color BackgroundColor = StringToColor(TConfiguration.properties.table.backcolor_argb);
					Color BorderColor = StringToColor(TConfiguration.properties.borders.color_argb);
					Color FontColor = StringToColor(TConfiguration.properties.table.forecolor_argb);

					SolidBrush BackgroundBrush = new SolidBrush(BackgroundColor);
					SolidBrush TextBrush = new SolidBrush(FontColor);
					Pen BorderPen = new Pen(BorderColor, 1.0f);

					Font Fuente = new Font(TConfiguration.properties.font.font_name, TConfiguration.properties.font.font_size);
					Point StartPos = new Point(4, 4);

					#endregion

					image = DrawNewImage(pImageSize);
					using (var g = Graphics.FromImage(image))
					{
						Rectangle CellBox;
						Point TextPosition;
						Point RowPosition;
						SizeF TextSize;

						List<int> ColunmPositions = new List<int>();

						#region Header

						//1. Crea la Caja para el Header:
						CellBox = new Rectangle(StartPos.X + 2, StartPos.Y + 2, TConfiguration.header.width, TConfiguration.properties.table.cell_height);

						//2. Obtiene el tamaño del Texto en pixeles:
						TextSize = g.MeasureString(TConfiguration.header.title, Fuente);
						CellBox.Width = (int)TextSize.Width + 20; //<- Ajusta el ancho del header segun sea necesario

						//3. Calcula la posicion para centrar el Texto:
						TextPosition = new Point(
							Convert.ToInt32((CellBox.Width - TextSize.Width) / 2),
							Convert.ToInt32((CellBox.Height - TextSize.Height) / 2) + 4
						);

						BackgroundBrush = new SolidBrush(StringToColor(TConfiguration.header.backcolor_argb));
						TextBrush = new SolidBrush(StringToColor(TConfiguration.header.forecolor_argb));

						//4. Dibuja el Header:
						g.FillRectangle(BackgroundBrush, CellBox);    //<- El Fondo
						g.DrawRectangle(BorderPen, CellBox);          //<- El Borde
						g.DrawString(TConfiguration.header.title, Fuente, TextBrush, TextPosition);  //<- El Texto 

						//Registra la Posicion final (el principio de la siguiente Fila):
						StartPos.Y = CellBox.Location.Y + CellBox.Height + 12;

						#endregion

						#region Data Rows

						if (DataSource != null && DataSource.Count > 0)
						{
							BackgroundBrush = new SolidBrush(BackgroundColor);
							TextBrush = new SolidBrush(StringToColor(TConfiguration.properties.table.forecolor_argb));

							RowPosition = new Point(StartPos.X, StartPos.Y);
							foreach (var _Column in TConfiguration.columns)
							{
								Size RowSize = new Size(_Column.width, TConfiguration.properties.table.cell_height);

								#region 1. Column Headers:

								CellBox = new Rectangle(RowPosition, RowSize);
								TextSize = g.MeasureString(_Column.title.ToString(), Fuente);

								TextPosition = new Point(
									Convert.ToInt32((CellBox.Width - TextSize.Width) / 2) + RowPosition.X,
									Convert.ToInt32((CellBox.Height - TextSize.Height) / 2) + RowPosition.Y
								);

								g.FillRectangle(BackgroundBrush, CellBox);    //<- El Fondo
								g.DrawRectangle(BorderPen, CellBox);          //<- El Borde
								g.DrawString(_Column.title,                   //<- El Texto 
									Fuente,
									new SolidBrush(StringToColor(TConfiguration.header.forecolor_argb)),
									TextPosition);

								RowPosition.Y = CellBox.Location.Y + CellBox.Height;
								ColunmPositions.Add(RowPosition.X);

								#endregion

								#region 2. The Rows with Data:

								foreach (var _RowData in DataSource)
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

									string CellText = AplicarFormato(FieldValue, _Column.format, _Column.type);
									TextSize = g.MeasureString(CellText, Fuente);
									CellBox = new Rectangle(RowPosition, RowSize);

									switch (_Column.align)
									{
										case "left":
											TextPosition = new Point(
											   RowPosition.X + 4,
											   Convert.ToInt32((CellBox.Height - TextSize.Height) / 2) + RowPosition.Y
										   ); break;
										case "center":
											TextPosition = new Point(
											  Convert.ToInt32((CellBox.Width - TextSize.Width) / 2) + RowPosition.X,
											  Convert.ToInt32((CellBox.Height - TextSize.Height) / 2) + RowPosition.Y
										  ); break;
										case "right":
											TextPosition = new Point(
											  Convert.ToInt32((CellBox.Width - TextSize.Width) - 4) + RowPosition.X,
											  Convert.ToInt32((CellBox.Height - TextSize.Height) / 2) + RowPosition.Y
										  ); break;
									}

									g.FillRectangle(BackgroundBrush, CellBox);    //<- El Fondo
									g.DrawRectangle(BorderPen, CellBox);          //<- El Borde
									g.DrawString(CellText, Fuente, TextBrush, TextPosition);  //<- El Texto 

									RowPosition.Y = CellBox.Location.Y + CellBox.Height;
								}
								#endregion

								RowPosition.X = CellBox.Location.X + CellBox.Width;
								RowPosition.Y = StartPos.Y;
							}
						}

						//Registra la Posicion final (el principio de la siguiente Fila):
						StartPos.Y = CellBox.Location.Y + CellBox.Height + 12;

						#endregion

						#region Summary

						if (DataSource != null && DataSource.Count > 0)
						{
							if (TConfiguration.summary != null && TConfiguration.summary.Count > 0)
							{
								List<decimal> SummaryData = GetSummaryValues(TConfiguration.summary, DataSource);

								int SumaryIndex = 0;
								foreach (var _Summary in TConfiguration.summary)
								{
									int ColIndex = 0;
									int LeftSpam = 0;
									string CellText = string.Empty;
									RowPosition = new Point(StartPos.X, StartPos.Y);

									foreach (var _Column in TConfiguration.columns)
									{
										if (_Column.field == _Summary.field)
										{
											LeftSpam = ColunmPositions[ColIndex];
											RowPosition = new Point(RowPosition.X + LeftSpam, RowPosition.Y);
											Size RowSize = new Size(_Column.width, TConfiguration.properties.table.cell_height);

											CellText = AplicarFormato(SummaryData[SumaryIndex], _Summary.format, _Column.type); //Calcular el Valor
											TextSize = g.MeasureString(CellText, Fuente);
											CellBox = new Rectangle(RowPosition, RowSize);

											switch (_Column.align)
											{
												case "left":
													TextPosition = new Point(
													   RowPosition.X + 4,
													   Convert.ToInt32((CellBox.Height - TextSize.Height) / 2) + RowPosition.Y
												   ); break;
												case "center":
													TextPosition = new Point(
													  Convert.ToInt32((CellBox.Width - TextSize.Width) / 2) + RowPosition.X,
													  Convert.ToInt32((CellBox.Height - TextSize.Height) / 2) + RowPosition.Y
												  ); break;
												case "right":
													TextPosition = new Point(
													  Convert.ToInt32((CellBox.Width - TextSize.Width) - 4) + RowPosition.X,
													  Convert.ToInt32((CellBox.Height - TextSize.Height) / 2) + RowPosition.Y
												  ); break;
											}

											g.FillRectangle(BackgroundBrush, CellBox);    //<- El Fondo
											g.DrawRectangle(BorderPen, CellBox);          //<- El Borde
											g.DrawString(CellText,                       //<- El Texto 
												Fuente,
												TextBrush,
												TextPosition);

											RowPosition.Y = CellBox.Location.Y + CellBox.Height;
										}
										ColIndex++;
									}
									SumaryIndex++;
								}
							}
						}

						//Registra la Posicion final (el principio de la siguiente Fila):
						StartPos.Y = CellBox.Location.Y + CellBox.Height + 12;

						#endregion

						#region Footer

						if (TConfiguration.footer != null)
						{
							//1. Crea la Caja para el Header:
							CellBox = new Rectangle(StartPos.X + 2, StartPos.Y + 2,
								TConfiguration.footer.width,
								TConfiguration.properties.table.cell_height);

							//2. Obtiene el tamaño del Texto en pixeles:
							TextSize = g.MeasureString(TConfiguration.footer.title, Fuente);

							//3. Calcula la posicion para centrar el Texto:
							TextPosition = new Point(
								Convert.ToInt32((CellBox.Width - TextSize.Width) / 2),
								Convert.ToInt32((CellBox.Location.Y - TextSize.Height) / 2) + 4
							);

							//4. Dibuja el Header:
							BackgroundBrush = new SolidBrush(StringToColor(TConfiguration.footer.backcolor_argb));
							TextBrush = new SolidBrush(StringToColor(TConfiguration.footer.forecolor_argb));

							g.FillRectangle(BackgroundBrush, CellBox);    //<- El Fondo
							g.DrawRectangle(BorderPen, CellBox);          //<- El Borde
							g.DrawString(TConfiguration.footer.title,       //<- El Texto 
								Fuente,
								TextBrush,
								new Point(TextPosition.X, CellBox.Location.Y + 6)
							);

							//Registra la Posicion final (el principio de la siguiente Fila):
							StartPos.Y = CellBox.Location.Y + CellBox.Height + 12;
						}

						#endregion


						g.Flush();
						image = new Bitmap(image);
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return image;
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

						switch (TConfiguration.columns.Count)
						{
							case 1:
								Results = from DataRow in JSonData
										  where
											CompareStrings(DataRow[TConfiguration.columns[0].field], pSearchString)
										  select DataRow;
								break;
							case 2:
								Results = from DataRow in JSonData
										  where
										  CompareStrings(DataRow[TConfiguration.columns[0].field], pSearchString) ||
										  CompareStrings(DataRow[TConfiguration.columns[1].field], pSearchString)
										  select DataRow;
								break;
							case 3:
								Results = from DataRow in JSonData
										  where
											CompareStrings(DataRow[TConfiguration.columns[0].field], pSearchString) ||
											CompareStrings(DataRow[TConfiguration.columns[1].field], pSearchString) ||
											CompareStrings(DataRow[TConfiguration.columns[2].field], pSearchString)
										  select DataRow;
								break;
							case 4:
								Results = from DataRow in JSonData
										  where
											CompareStrings(DataRow[TConfiguration.columns[0].field], pSearchString) ||
											CompareStrings(DataRow[TConfiguration.columns[1].field], pSearchString) ||
											CompareStrings(DataRow[TConfiguration.columns[2].field], pSearchString) ||
											CompareStrings(DataRow[TConfiguration.columns[3].field], pSearchString)
										  select DataRow;
								break;
							case 5:
								Results = from DataRow in JSonData
										  where
											CompareStrings(DataRow[TConfiguration.columns[0].field], pSearchString) ||
											CompareStrings(DataRow[TConfiguration.columns[1].field], pSearchString) ||
											CompareStrings(DataRow[TConfiguration.columns[2].field], pSearchString) ||
											CompareStrings(DataRow[TConfiguration.columns[3].field], pSearchString) ||
											CompareStrings(DataRow[TConfiguration.columns[4].field], pSearchString)
										  select DataRow;
								break;
							case 6:
								Results = from DataRow in JSonData
										  where
											CompareStrings(DataRow[TConfiguration.columns[0].field], pSearchString) ||
											CompareStrings(DataRow[TConfiguration.columns[1].field], pSearchString) ||
											CompareStrings(DataRow[TConfiguration.columns[2].field], pSearchString) ||
											CompareStrings(DataRow[TConfiguration.columns[3].field], pSearchString) ||
											CompareStrings(DataRow[TConfiguration.columns[4].field], pSearchString) ||
											CompareStrings(DataRow[TConfiguration.columns[5].field], pSearchString)
										  select DataRow;
								break;
							case 7:
								Results = from DataRow in JSonData
										  where
											CompareStrings(DataRow[TConfiguration.columns[0].field], pSearchString) ||
											CompareStrings(DataRow[TConfiguration.columns[1].field], pSearchString) ||
											CompareStrings(DataRow[TConfiguration.columns[2].field], pSearchString) ||
											CompareStrings(DataRow[TConfiguration.columns[3].field], pSearchString) ||
											CompareStrings(DataRow[TConfiguration.columns[4].field], pSearchString) ||
											CompareStrings(DataRow[TConfiguration.columns[5].field], pSearchString) ||
											CompareStrings(DataRow[TConfiguration.columns[6].field], pSearchString)
										  select DataRow;
								break;
							case 8:
								Results = from DataRow in JSonData
										  where
											CompareStrings(DataRow[TConfiguration.columns[0].field], pSearchString) ||
											CompareStrings(DataRow[TConfiguration.columns[1].field], pSearchString) ||
											CompareStrings(DataRow[TConfiguration.columns[2].field], pSearchString) ||
											CompareStrings(DataRow[TConfiguration.columns[3].field], pSearchString) ||
											CompareStrings(DataRow[TConfiguration.columns[4].field], pSearchString) ||
											CompareStrings(DataRow[TConfiguration.columns[5].field], pSearchString) ||
											CompareStrings(DataRow[TConfiguration.columns[6].field], pSearchString) ||
											CompareStrings(DataRow[TConfiguration.columns[7].field], pSearchString)
										  select DataRow;
								break;
							case 9:
								Results = from DataRow in JSonData
										  where
											CompareStrings(DataRow[TConfiguration.columns[0].field], pSearchString) ||
											CompareStrings(DataRow[TConfiguration.columns[1].field], pSearchString) ||
											CompareStrings(DataRow[TConfiguration.columns[2].field], pSearchString) ||
											CompareStrings(DataRow[TConfiguration.columns[3].field], pSearchString) ||
											CompareStrings(DataRow[TConfiguration.columns[4].field], pSearchString) ||
											CompareStrings(DataRow[TConfiguration.columns[5].field], pSearchString) ||
											CompareStrings(DataRow[TConfiguration.columns[6].field], pSearchString) ||
											CompareStrings(DataRow[TConfiguration.columns[7].field], pSearchString) ||
											CompareStrings(DataRow[TConfiguration.columns[8].field], pSearchString)
										  select DataRow;
								break;

							default: //Searching is limited to the first 10 columns:
								Results = from DataRow in JSonData
										  where
											CompareStrings(DataRow[TConfiguration.columns[0].field], pSearchString) ||
											CompareStrings(DataRow[TConfiguration.columns[1].field], pSearchString) ||
											CompareStrings(DataRow[TConfiguration.columns[2].field], pSearchString) ||
											CompareStrings(DataRow[TConfiguration.columns[3].field], pSearchString) ||
											CompareStrings(DataRow[TConfiguration.columns[4].field], pSearchString) ||
											CompareStrings(DataRow[TConfiguration.columns[5].field], pSearchString) ||
											CompareStrings(DataRow[TConfiguration.columns[6].field], pSearchString) ||
											CompareStrings(DataRow[TConfiguration.columns[7].field], pSearchString) ||
											CompareStrings(DataRow[TConfiguration.columns[8].field], pSearchString) ||
											CompareStrings(DataRow[TConfiguration.columns[9].field], pSearchString)
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
							if (Apply) DataSource = _ret;
						}
						else
						{
							//No Data Found for the Filter, Returns an Empty (not Null) List:
							_ret = new List<dynamic>();
							if (Apply) DataSource = _ret;
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
				if (pNewData != null)
				{
					//Convierte 'Newtonsoft.Json.Linq.JArray' a 'List<dynamic>':
					if (pNewData is Newtonsoft.Json.Linq.JArray)
					{
						OriginalData = pNewData.ToObject<List<dynamic>>(); //<- Preserves the Un-Sorted Data.
					}
					else
					{
						OriginalData = Newtonsoft.Json.JsonConvert.DeserializeObject<List<dynamic>>(
											Newtonsoft.Json.JsonConvert.SerializeObject(pNewData)
										);
					}
				}

				//Updates the Shown DataSet with new data:
				if (OriginalData != null && OriginalData.Count > 0)
				{
					DataSource = OriginalData;

					JSonData = Newtonsoft.Json.Linq.JArray.FromObject(
									Newtonsoft.Json.JsonConvert.DeserializeObject(
										Newtonsoft.Json.JsonConvert.SerializeObject(OriginalData)
									)
					);
				}
			}
			catch (Exception ex) { throw ex; }
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

	public static class PredicateBuilder
	{
		//http://www.albahari.com/nutshell/predicatebuilder.aspx

		public static Expression<Func<Newtonsoft.Json.Linq.JToken, bool>> ContainsStrings(this Newtonsoft.Json.Linq.JToken Token, params string[] keywords)
		{
			var predicate = PredicateBuilder.False<Newtonsoft.Json.Linq.JToken>();
			foreach (string keyword in keywords)
				predicate = predicate.Or(p => p.ToString().Contains(keyword));

			return predicate;
		}
		//public static Expression<Func<Newtonsoft.Json.Linq.JToken, bool>> ContainsStrings(this Newtonsoft.Json.Linq.JEnumerable<Newtonsoft.Json.Linq.JToken> Tokens, string SearString)
		//{
		//	var predicate = PredicateBuilder.False<Newtonsoft.Json.Linq.JToken>();
		//	foreach (var token in Tokens)
		//	{
		//		predicate = predicate.OrJ(p => p.Contains(SearString));
		//	}

		//	return Tokens.Where(predicate); // predicate;
		//}

		public static Expression<Func<T, bool>> True<T>() { return f => true; }
		public static Expression<Func<T, bool>> False<T>() { return f => false; }

		public static Expression<Func<Newtonsoft.Json.Linq.JToken, bool>> JToken<T>() { return f => false; }
		public static Expression<Func<Newtonsoft.Json.Linq.JToken, bool>> OrJ<T>(this Expression<Func<Newtonsoft.Json.Linq.JToken, bool>> expr1,
																					 Expression<Func<Newtonsoft.Json.Linq.JToken, bool>> expr2)
		{
			var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
			return Expression.Lambda<Func<Newtonsoft.Json.Linq.JToken, bool>>
				  (Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);
		}

		public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1,
														   Expression<Func<T, bool>> expr2)
		{
			var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
			return Expression.Lambda<Func<T, bool>>
				  (Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);
		}

		public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1,
															 Expression<Func<T, bool>> expr2)
		{
			var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
			return Expression.Lambda<Func<T, bool>>
				  (Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters);
		}
	}

	//public static class ExpressionBuilder
	//{
	//	/// <summary>Devuelve 'true' si la cadena es Nula o Vacia.</summary>
	//	/// <param name="input">Cadena de Texto a Validar</param>
	//	public static bool IsNullOrEmpty(this String input)
	//	{
	//		return !(input != null && input != string.Empty);
	//	}
	//	public static bool IsNull(this object input)
	//	{
	//		return (input == null);
	//	}

	//	public static Expression<Func<T, bool>> ToExpression<T>(string andOrOperator, string propName, string opr, string value, Expression<Func<T, bool>> expr = null)
	//	{
	//		Expression<Func<T, bool>> func = null;
	//		try
	//		{
	//			ParameterExpression paramExpr = Expression.Parameter(typeof(T));
	//			var arrProp = propName.Split('.').ToList();
	//			Expression binExpr = null;
	//			string partName = string.Empty;
	//			arrProp.ForEach(x =>
	//			{
	//				Expression tempExpr = null;
	//				partName = partName.IsNull() ? x : partName + "." + x;
	//				if (partName == propName)
	//				{
	//					var member = NestedExprProp(paramExpr, partName);
	//					var type = member.Type.Name == "Nullable`1" ? Nullable.GetUnderlyingType(member.Type) : member.Type;
	//					tempExpr = ApplyFilter(opr, member, Expression.Convert(ToExprConstant(type, value), member.Type));
	//				}
	//				else
	//					tempExpr = ApplyFilter("!=", NestedExprProp(paramExpr, partName), Expression.Constant(null));
	//				if (binExpr != null)
	//					binExpr = Expression.AndAlso(binExpr, tempExpr);
	//				else
	//					binExpr = tempExpr;
	//			});
	//			Expression<Func<T, bool>> innerExpr = Expression.Lambda<Func<T, bool>>(binExpr, paramExpr);
	//			if (expr != null)
	//				innerExpr = (andOrOperator.IsNull() || andOrOperator == "And" || andOrOperator == "AND" || andOrOperator == "&&") ? innerExpr.And(expr) : innerExpr.Or(expr);
	//			func = innerExpr;
	//		}
	//		catch { }
	//		return func;
	//	}

	//	private static MemberExpression NestedExprProp(Expression expr, string propName)
	//	{
	//		string[] arrProp = propName.Split('.');
	//		int arrPropCount = arrProp.Length;
	//		return (arrPropCount > 1) ? Expression.Property(NestedExprProp(expr, arrProp.Take(arrPropCount - 1).Aggregate((a, i) => a + "." + i)), arrProp[arrPropCount - 1]) : Expression.Property(expr, propName);
	//	}

	//	private static Expression ToExprConstant(Type prop, string value)
	//	{
	//		if (value.IsNull())
	//			return Expression.Constant(value);
	//		object val = null;
	//		switch (prop.FullName)
	//		{
	//			case "System.Guid":
	//				val = new Guid(value);// value.ToGuid();
	//				break;
	//			default:
	//				val = Convert.ChangeType(value, Type.GetType(prop.FullName));
	//				break;
	//		}
	//		return Expression.Constant(val);
	//	}

	//	private static Expression ApplyFilter(string opr, Expression left, Expression right)
	//	{
	//		Expression InnerLambda = null;
	//		switch (opr)
	//		{
	//			case "==":
	//			case "=":
	//				InnerLambda = Expression.Equal(left, right);
	//				break;
	//			case "<":
	//				InnerLambda = Expression.LessThan(left, right);
	//				break;
	//			case ">":
	//				InnerLambda = Expression.GreaterThan(left, right);
	//				break;
	//			case ">=":
	//				InnerLambda = Expression.GreaterThanOrEqual(left, right);
	//				break;
	//			case "<=":
	//				InnerLambda = Expression.LessThanOrEqual(left, right);
	//				break;
	//			case "!=":
	//				InnerLambda = Expression.NotEqual(left, right);
	//				break;
	//			case "&&":
	//				InnerLambda = Expression.And(left, right);
	//				break;
	//			case "||":
	//				InnerLambda = Expression.Or(left, right);
	//				break;
	//			case "LIKE":
	//				InnerLambda = Expression.Call(left, typeof(string).GetMethod("Contains", new Type[] { typeof(string) }), right);
	//				break;
	//			case "NOTLIKE":
	//				InnerLambda = Expression.Not(Expression.Call(left, typeof(string).GetMethod("Contains", new Type[] { typeof(string) }), right));
	//				break;
	//		}
	//		return InnerLambda;
	//	}

	//	public static Expression<Func<T, object>> PropExpr<T>(string PropName)
	//	{
	//		ParameterExpression paramExpr = Expression.Parameter(typeof(T));
	//		var tempExpr = Extentions.NestedExprProp(paramExpr, PropName);
	//		return Expression.Lambda<Func<T, object>>(Expression.Convert(Expression.Lambda(tempExpr, paramExpr).Body, typeof(object)), paramExpr);

	//	}
	//	public static IQueryOver<T, T> OrderBy<T>(this IQueryOver<T, T> Collection, string sidx, string sord)
	//	{
	//		return sord == "asc" ? Collection.OrderBy(NHibernate.Criterion.Projections.Property(sidx)).Asc : Collection.OrderBy(NHibernate.Criterion.Projections.Property(sidx)).Desc;
	//	}

	//	public static Expression<Func<T, TResult>> And<T, TResult>(this Expression<Func<T, TResult>> expr1, Expression<Func<T, TResult>> expr2)
	//	{
	//		var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
	//		return Expression.Lambda<Func<T, TResult>>(Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters);
	//	}

	//	public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
	//	{
	//		var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
	//		return Expression.Lambda<Func<T, bool>>(Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);
	//	}
	//}

	//IQueryable TextFilter_Untyped(IQueryable source, string term)
	//{
	//	if (string.IsNullOrEmpty(term)) { return source; }
	//	Type elementType = source.ElementType;

	//	// The logic for building the ParameterExpression and the LambdaExpression's body is the same as in the previous example,
	//	// but has been refactored into the constructBody function.
	//	(Expression? body, ParameterExpression? prm) = constructBody(elementType, term);
	//	if (body is null) { return source; }

	//	Expression filteredTree = Call(
	//		typeof(Queryable),
	//		"Where",
	//		new[] { elementType },
	//		source.Expression,
	//		Lambda(body, prm!)
	//	);

	//	return source.Provider.CreateQuery(filteredTree);
	//}

	//	IQueryable<T> TextFilter<T>(IQueryable<T> source, string term)
	//	{
	//		if (string.IsNullOrEmpty(term)) { return source; }

	//		// T is a compile-time placeholder for the element type of the query.
	//		Type elementType = typeof(T);

	//		// Get all the string properties on this specific type.
	//		System.Reflection.PropertyInfo[] stringProperties =
	//			elementType.GetProperties()
	//				.Where(x => x.PropertyType == typeof(string))
	//				.ToArray();
	//		if (!stringProperties.Any()) { return source; }

	//		// Get the right overload of String.Contains
	//		System.Reflection.MethodInfo containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });

	//		// Create a parameter for the expression tree:
	//		// the 'x' in 'x => x.PropertyName.Contains("term")'
	//		// The type of this parameter is the query's element type
	//		ParameterExpression prm = Parameter(elementType);

	//		// Map each property to an expression tree node
	//		IEnumerable<Expression> expressions = stringProperties
	//			.Select(prp =>
	//				// For each property, we have to construct an expression tree node like x.PropertyName.Contains("term")
	//				Call(                  // .Contains(...) 
	//					Property(          // .PropertyName
	//						prm,           // x 
	//						prp
	//					),
	//					containsMethod,
	//					Constant(term)     // "term" 
	//				)
	//			);

	//		// Combine all the resultant expression nodes using ||
	//		Expression body = expressions
	//			.Aggregate(
	//				(prev, current) => Or(prev, current)
	//			);

	//		// Wrap the expression body in a compile-time-typed lambda expression
	//		Expression<Func<T, bool>> lambda = Lambda<Func<T, bool>>(body, prm);

	//		// Because the lambda is compile-time-typed (albeit with a generic parameter), we can use it with the Where method
	//		return source.Where(lambda);
	//	}
	//}

	//public static class ExpressionBuilder
	//{
	//	/* DEPRECATED!
	//	 * I could not make this to work with Anonymous (dynamic) data sets. */
	//	public static Expression<Func<T, bool>> GetExpression<T>(IList<DynamicFilter> filters)
	//	{
	//		if (filters.Count == 0)
	//		{
	//			return null;
	//		}

	//		ParameterExpression param = Expression.Parameter(typeof(T), "t");
	//		Expression exp = null;

	//		if (filters.Count == 1)
	//		{
	//			exp = GetExpression<T>(param, filters[0]);
	//		}
	//		else if (filters.Count == 2)
	//		{
	//			exp = GetExpression<T>(param, filters[0], filters[1]);
	//		}
	//		else
	//		{
	//			while (filters.Count > 0)
	//			{
	//				var f1 = filters[0];
	//				var f2 = filters[1];

	//				exp = exp == null
	//					? GetExpression<T>(param, filters[0], filters[1])
	//					: Expression.AndAlso(exp, GetExpression<T>(param, filters[0], filters[1]));

	//				filters.Remove(f1);
	//				filters.Remove(f2);

	//				if (filters.Count == 1)
	//				{
	//					exp = Expression.AndAlso(exp, GetExpression<T>(param, filters[0]));
	//					filters.RemoveAt(0);
	//				}
	//			}
	//		}

	//		return Expression.Lambda<Func<T, bool>>(exp, param);
	//	}

	//	private static Expression GetExpression<T>(ParameterExpression param, DynamicFilter filter)
	//	{
	//		MemberExpression member = Expression.Property(param, filter.PropertyName);
	//		ConstantExpression constant = Expression.Constant(filter.Value);

	//		return Expression.Equal(member, constant);
	//	}

	//	private static BinaryExpression GetExpression<T>(ParameterExpression param, DynamicFilter filter1, DynamicFilter filter2)
	//	{
	//		Expression bin1 = GetExpression<T>(param, filter1);
	//		Expression bin2 = GetExpression<T>(param, filter2);

	//		return Expression.AndAlso(bin1, bin2);
	//	}
	//}
	//public class DynamicFilter
	//{
	//	public string PropertyName { get; set; }
	//	public object Value { get; set; }
	//}


}
