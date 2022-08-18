using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
//using System.Linq.Dynamic;
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

		public dynamic OriginalData { get; set; }
		public dynamic SortedData { get; set; }
		public List<GroupData> GroupedData { get; set; }

		#endregion

		#region Main Methods

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

					int Margin = TConfiguration.properties.table.cell_padding;
					StringBuilder Lines = new StringBuilder();
					string CellText = string.Empty;
					int ColumnSize = 0;

					#endregion

					#region Header

					if (TConfiguration.header != null)
					{
						//1. Crea la Caja para el Header:
						CellText = TConfiguration.header.title;
						ColumnSize = TConfiguration.header.length + (Margin * 2);

						//Linea Superior:
						Lines.AppendLine(string.Format("{0}{1}{2}",
							Borders.Top.Left.ToString(),
							new string(Convert.ToChar(Borders.Top.Border), ColumnSize),
							Borders.Top.Right.ToString()
						));

						//Lineas Laterales y Texto:
						Lines.AppendLine(string.Format("{0}{1}{2}",
							Borders.Sides.Left.ToString(),
							AlinearTexto(CellText, ColumnSize, "center"),
							Borders.Sides.Right.ToString()
						));

						//Linea Inferior:
						Lines.AppendLine(string.Format("{0}{1}{2}",
							Borders.Bottom.Left.ToString(),
							new string(Convert.ToChar(Borders.Bottom.Border), ColumnSize),
							Borders.Bottom.Right.ToString()
						));
					}

					#endregion

					#region Data Sorting

					if (TConfiguration.data != null && TConfiguration.data.Count > 0)
					{
						OriginalData = TConfiguration.data; //<- Preserves the Un-Sorted Data.

						if (TConfiguration.sorting != null && TConfiguration.sorting.enabled)
						{
							if (TConfiguration.sorting.fields != null && TConfiguration.sorting.fields.Count > 0)
							{
								SortedData = DynamicSorting(TConfiguration.data, TConfiguration.sorting.fields);
								TConfiguration.data = SortedData; //<- Sets the Data To Show
							}
						}
					}
					//If Sorting is not Enabled, Data keeps the original order.

					#endregion

					#region Data Grouping

					List<int> ColunmPositions = new List<int>(); //<- The Left position (in characters) for each Column.
					List<string> GroupColumnHeaders = null; //<- Used only when  'repeat_column_headers' is false.

					if (TConfiguration.data != null && TConfiguration.data.Count > 0)
					{
						if (TConfiguration.grouping != null && TConfiguration.grouping.enabled)
						{
							if (TConfiguration.grouping.fields != null && TConfiguration.grouping.fields.Count > 0)
							{
								//1. Group the Data:
								GroupedData = DynamicGrouping(TConfiguration.data, TConfiguration.grouping.fields);

								//2. Build the Groups and their components:
								if (GroupedData != null && GroupedData.Count > 0)
								{
									bool repeat_column_headers = TConfiguration.grouping.repeat_column_headers;
									bool hide_group_columns = TConfiguration.grouping.hide_group_columns; //<- TODO: not used yet.
									bool show_summary = TConfiguration.grouping.show_summary;

									foreach (var Group in GroupedData)
									{
										ColumnSize = (int)Group.column.length + (Margin * 2);

										#region Group Header

										Group.Header = new List<string>();

										CellText = AlinearTexto(Group.CellData, ColumnSize);
										CellText = AlinearTexto(CellText, ColumnSize, Group.column.align);

										//Linea Superior:
										Group.Header.Add(string.Format("{0}{1}{2}",
											 Borders.Top.Left,
											 new string(Borders.Top.Border, ColumnSize),
											 Borders.Top.Right
										));

										//Lineas Laterales y Texto:
										Group.Header.Add(string.Format("{0}{1}{2}",
											Borders.Sides.Left,
											CellText,
											Borders.Sides.Right
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

					#endregion

					//Si los datos fueron Agrupados, mostrar los Grupos:
					if (GroupedData != null && GroupedData.Count > 0)
					{
						#region Grouped Table

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

						if (TConfiguration.data != null && TConfiguration.data.Count > 0)
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

								var SummaryData = GetSummaryValues(TConfiguration.summary, TConfiguration.data);

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

						if (TConfiguration.data != null && TConfiguration.data.Count > 0)
						{
							int DataCount = TConfiguration.data.Count;
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
										var RowData = TConfiguration.data[RowIndex];
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

									ColunmPositions.Add(Cell_Top.Length);

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
						}
						else
						{
							#region No Data

							//1. Crea la Caja para el Header:
							CellText = "No Data.";
							ColumnSize = CellText.Length + (Margin * 2);

							//Linea Superior:
							Lines.AppendLine(string.Format("{0}{1}{2}",
								Borders.Top.Left.ToString(),
								new string(Borders.Top.Border, ColumnSize),
								Borders.Top.Right.ToString()
							));

							//Lineas Laterales y Texto:
							Lines.AppendLine(string.Format("{0}{1}{2}",
								Borders.Sides.Left.ToString(),
								AlinearTexto(CellText, ColumnSize),
								Borders.Sides.Right.ToString()
							));

							//Linea Inferior:
							Lines.AppendLine(string.Format("{0}{1}{2}",
								Borders.Bottom.Left.ToString(),
								new string(Borders.Bottom.Border, ColumnSize),
								Borders.Bottom.Right.ToString()
							));

							#endregion
						}

						#endregion

						#region Summary

						if (TConfiguration.data != null && TConfiguration.data.Count > 0)
						{
							if (TConfiguration.summary != null && TConfiguration.summary.Count > 0)
							{
								string Cell_Top = string.Empty;
								string Cell_Mid = string.Empty;
								string Cell_Bot = string.Empty;

								var SummaryData = GetSummaryValues(TConfiguration.summary, TConfiguration.data);

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
						Lines.AppendLine(string.Format("{0}{1}{2}",
							Borders.Top.Left.ToString(),
							new string(Convert.ToChar(Borders.Top.Border), ColumnSize),
							Borders.Top.Right.ToString()
						));

						//Lineas Laterales y Texto:
						Lines.AppendLine(string.Format("{0}{1}{2}",
							Borders.Sides.Left.ToString(),
							AlinearTexto(CellText, ColumnSize),
							Borders.Sides.Right.ToString()
						));

						//Linea Inferior:
						Lines.AppendLine(string.Format("{0}{1}{2}",
							Borders.Bottom.Left.ToString(),
							new string(Convert.ToChar(Borders.Bottom.Border), ColumnSize),
							Borders.Bottom.Right.ToString()
						));
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

						if (TConfiguration.data != null && TConfiguration.data.Count > 0)
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

								foreach (var _RowData in TConfiguration.data)
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

						if (TConfiguration.data != null && TConfiguration.data.Count > 0)
						{
							if (TConfiguration.summary != null && TConfiguration.summary.Count > 0)
							{
								List<decimal> SummaryData = GetSummaryValues(TConfiguration.summary, TConfiguration.data);

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

		#endregion

		#region Utility Methods

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

					//Convierte 'Newtonsoft.Json.Linq.JArray' a 'List<dynamic>':  Newtonsoft.Json.Linq.JObject
					if (Input is Newtonsoft.Json.Linq.JArray)
					{
						DataInput = TConfiguration.data.ToObject<List<dynamic>>();
					}
					else
					{
						//Convierte un Array de Objetos estaticos en un Array de Objetos Dynamicos:
						DataInput = new List<dynamic>();
						foreach (var _Data in Input)
						{
							DataInput.Add((dynamic)_Data);
						}
					}

					if (DataInput != null && DataInput.Count > 0)
					{
						DataInput.Sort((a, b) =>
						{
							int result = 0;
							System.ComponentModel.PropertyDescriptor propA;
							System.ComponentModel.PropertyDescriptor propB;

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
				//0. Convert the Data into a Dynamic List:
				List<dynamic> DataSorted = new List<dynamic>();
				foreach (var _Data in Input)
				{
					DataSorted.Add((dynamic)_Data);
				}

				if (DataSorted != null && DataSorted.Count > 0)
				{
					_ret = new List<GroupData>();

					//Get the Column Definition:
					var ColumnInfo = TConfiguration.columns.Find(x => x.field == SortingFields[0]);

					//1. Get Unique Values for each Group Field: (Trae solo los valores del Campo Agrupado)	
					//var Uniques = DataSorted.GroupBy(x => GetPropertyValueX(x, SortingFields[0]) ).ToList();

					//Trae Registros Unicos con Todos los Campos:
					var Uniques = DataSorted.GroupBy(x => 
						GetPropertyValueX(x, SortingFields[0])
					).Select(g => g.First()).ToList();





					//var Uniques2 = DataSorted.GroupBy(x => new { x.Column1, x.Column2 }).ToList();

					//var Uniques2 = DataSorted.GroupBy(x => new {
					//	x.Column1,
					//	x.Column2
					//}).ToList();

					//var Uniques2 = DataSorted.GroupBy(x => [
					//	GetPropertyValue(x, Field),
					//	GetPropertyValue(x, Field)
					//]);

					//var propA = System.ComponentModel.TypeDescriptor.
					//					GetProperties((object)a).
					//					Find(Field, true);

					if (Uniques != null)
					{
						foreach (var item in Uniques)
						{
							//2. Find all Data having the Unique Value in the Group Field:
							var Fdata = DataSorted.FindAll(y => GetPropertyValueX(y, SortingFields[0]) == GetPropertyValueX(item, SortingFields[0]));
							if (Fdata != null)
							{
								//3. Create a new Group with the Data:
								_ret.Add(new GroupData()
								{
									data = Fdata,
									column = ColumnInfo,
									CellData = GetPropertyValueX(item, SortingFields[0])
								});
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

		#endregion
	}


}
