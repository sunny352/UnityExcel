using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using OfficeOpenXml;
using System.IO;

public class ExampleDataConvertor
{
	private static List<ExampleData> m_data = new List<ExampleData>();
	public static bool Convert()
	{
		if (ReadExcel())
		{
			SaveBytes();
			return true;
		}
		else
		{
			return false;
		}
	}

	private static bool ReadExcel()
	{
		ExcelPackage package = TableExcelLoader.Load("ExampleData");
		if (null == package)
		{
			return false;
		}
		ExcelWorksheet sheet = package.Workbook.Worksheets["ExampleData"];
		if (null == sheet)
		{
			return false;
		}
		int defaultKey = new int();
		for (int index = 1; index <= sheet.Dimension.Rows; ++index)
		{
			var tableData = new ExampleData();
			int innerIndex = 1;
			{
				tableData.ID = sheet.Cells[index, innerIndex++].GetValue<int>();
				tableData.Name = ExcelTools.GetCellString(sheet.Cells[index, innerIndex++]);
				tableData.FloatValue = sheet.Cells[index, innerIndex++].GetValue<float>();
				try
				{
					tableData.EnumValue = (TestEnum)Enum.Parse(typeof(TestEnum), ExcelTools.GetCellString(sheet.Cells[index, innerIndex++]));
				}
				catch(System.Exception ex)
				{
					Debug.LogException(ex);
				}
				int count_FixedList_4 = 5;
				tableData.FixedList = new int[count_FixedList_4];
				for (int index_FixedList_4 = 0; index_FixedList_4 < count_FixedList_4; ++index_FixedList_4)
				{
					tableData.FixedList[index_FixedList_4] = sheet.Cells[index, innerIndex++].GetValue<int>();
				}
				int count_AutoList_4 = sheet.Cells[index, innerIndex++].GetValue<int>();
				tableData.AutoList = new int[count_AutoList_4];
				for (int index_AutoList_4 = 0; index_AutoList_4 < count_AutoList_4; ++index_AutoList_4)
				{
					tableData.AutoList[index_AutoList_4] = sheet.Cells[index, innerIndex++].GetValue<int>();
				}
				ExampleInnerData obj_InnerData_4 = new ExampleInnerData();
				{
					obj_InnerData_4.ID = sheet.Cells[index, innerIndex++].GetValue<int>();
					int count_AutoList_5 = sheet.Cells[index, innerIndex++].GetValue<int>();
					obj_InnerData_4.AutoList = new ExampleInnerInnerData[count_AutoList_5];
					for (int index_AutoList_5 = 0; index_AutoList_5 < count_AutoList_5; ++index_AutoList_5)
					{
						obj_InnerData_4.AutoList[index_AutoList_5].ID = sheet.Cells[index, innerIndex++].GetValue<int>();
						try
						{
							obj_InnerData_4.AutoList[index_AutoList_5].EnumValue = (TestEnum)Enum.Parse(typeof(TestEnum), ExcelTools.GetCellString(sheet.Cells[index, innerIndex++]));
						}
						catch(System.Exception ex)
						{
							Debug.LogException(ex);
						}
					}
				}
				tableData.InnerData = obj_InnerData_4;
				int count_InnerDataList_4 = sheet.Cells[index, innerIndex++].GetValue<int>();
				tableData.InnerDataList = new ExampleInnerData[count_InnerDataList_4];
				for (int index_InnerDataList_4 = 0; index_InnerDataList_4 < count_InnerDataList_4; ++index_InnerDataList_4)
				{
					tableData.InnerDataList[index_InnerDataList_4].ID = sheet.Cells[index, innerIndex++].GetValue<int>();
					int count_AutoList_5 = sheet.Cells[index, innerIndex++].GetValue<int>();
					tableData.InnerDataList[index_InnerDataList_4].AutoList = new ExampleInnerInnerData[count_AutoList_5];
					for (int index_AutoList_5 = 0; index_AutoList_5 < count_AutoList_5; ++index_AutoList_5)
					{
						tableData.InnerDataList[index_InnerDataList_4].AutoList[index_AutoList_5].ID = sheet.Cells[index, innerIndex++].GetValue<int>();
						try
						{
							tableData.InnerDataList[index_InnerDataList_4].AutoList[index_AutoList_5].EnumValue = (TestEnum)Enum.Parse(typeof(TestEnum), ExcelTools.GetCellString(sheet.Cells[index, innerIndex++]));
						}
						catch(System.Exception ex)
						{
							Debug.LogException(ex);
						}
					}
				}
			}
			if (tableData.ID == defaultKey)
			{
				continue;
			}
			var existKey = m_data.Find(innerItem => innerItem.ID == tableData.ID);
			if (null != existKey)
			{
				Debug.LogWarning(string.Format("Already has the key {0}, replace the old data.", tableData.ID));
				m_data.Remove(existKey);
			}

			m_data.Add(tableData);
		}
		return true;
	}

	private static bool SaveBytes()
	{
		using (FileStream bytesFile = File.Create("Assets/Resources/Tables/ExampleData.bytes"))
		{
			using (BinaryWriter writer = new BinaryWriter(bytesFile))
			{
				writer.Write(m_data.Count);
				foreach (var tableData in m_data)
				{
					writer.Write(tableData.ID);
					writer.Write(tableData.Name);
					writer.Write(tableData.FloatValue);
					writer.Write((int)tableData.EnumValue);
					writer.Write(tableData.FixedList.Length);
					foreach (var obj_FixedList_5 in tableData.FixedList)
					{
						writer.Write(obj_FixedList_5);
					}
					writer.Write(tableData.AutoList.Length);
					foreach (var obj_AutoList_5 in tableData.AutoList)
					{
						writer.Write(obj_AutoList_5);
					}
					var obj_InnerData_5 = tableData.InnerData;
					{
						writer.Write(obj_InnerData_5.ID);
						writer.Write(obj_InnerData_5.AutoList.Length);
						foreach (var obj_AutoList_6 in obj_InnerData_5.AutoList)
						{
							writer.Write(obj_AutoList_6.ID);
							writer.Write((int)obj_AutoList_6.EnumValue);
						}
					}
					writer.Write(tableData.InnerDataList.Length);
					foreach (var obj_InnerDataList_5 in tableData.InnerDataList)
					{
						writer.Write(obj_InnerDataList_5.ID);
						writer.Write(obj_InnerDataList_5.AutoList.Length);
						foreach (var obj_AutoList_6 in obj_InnerDataList_5.AutoList)
						{
							writer.Write(obj_AutoList_6.ID);
							writer.Write((int)obj_AutoList_6.EnumValue);
						}
					}
				}
			}
		}
		return true;
	}
}
