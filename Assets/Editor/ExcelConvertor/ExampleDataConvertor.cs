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
				tableData.Name = sheet.Cells[index, innerIndex++].GetValue<string>();
				tableData.FloatValue = sheet.Cells[index, innerIndex++].GetValue<float>();
				try
				{
					tableData.EnumValue = (TestEnum)Enum.Parse(typeof(TestEnum), sheet.Cells[index, innerIndex++].GetValue<string>());
				}
				catch(System.Exception ex)
				{
					Debug.LogException(ex);
				}
				int count_FixedList_0 = 5;
				tableData.FixedList = new int[count_FixedList_0];
				for (int index_FixedList_0 = 0; index_FixedList_0 < count_FixedList_0; ++index_FixedList_0)
				{
					tableData.FixedList[index_FixedList_0] = sheet.Cells[index, innerIndex++].GetValue<int>();
				}
				int count_AutoList_0 = sheet.Cells[index, innerIndex++].GetValue<int>();
				tableData.AutoList = new int[count_AutoList_0];
				for (int index_AutoList_0 = 0; index_AutoList_0 < count_AutoList_0; ++index_AutoList_0)
				{
					tableData.AutoList[index_AutoList_0] = sheet.Cells[index, innerIndex++].GetValue<int>();
				}
				ExampleInnerData obj_InnerData_0 = new ExampleInnerData();
				{
					obj_InnerData_0.ID = sheet.Cells[index, innerIndex++].GetValue<int>();
					int count_AutoList_1 = sheet.Cells[index, innerIndex++].GetValue<int>();
					obj_InnerData_0.AutoList = new ExampleInnerInnerData[count_AutoList_1];
					for (int index_AutoList_1 = 0; index_AutoList_1 < count_AutoList_1; ++index_AutoList_1)
					{
						obj_InnerData_0.AutoList[index_AutoList_1].ID = sheet.Cells[index, innerIndex++].GetValue<int>();
						try
						{
							obj_InnerData_0.AutoList[index_AutoList_1].EnumValue = (TestEnum)Enum.Parse(typeof(TestEnum), sheet.Cells[index, innerIndex++].GetValue<string>());
						}
						catch(System.Exception ex)
						{
							Debug.LogException(ex);
						}
					}
				}
				tableData.InnerData = obj_InnerData_0;
				int count_InnerDataList_0 = sheet.Cells[index, innerIndex++].GetValue<int>();
				tableData.InnerDataList = new ExampleInnerData[count_InnerDataList_0];
				for (int index_InnerDataList_0 = 0; index_InnerDataList_0 < count_InnerDataList_0; ++index_InnerDataList_0)
				{
					tableData.InnerDataList[index_InnerDataList_0].ID = sheet.Cells[index, innerIndex++].GetValue<int>();
					int count_AutoList_1 = sheet.Cells[index, innerIndex++].GetValue<int>();
					tableData.InnerDataList[index_InnerDataList_0].AutoList = new ExampleInnerInnerData[count_AutoList_1];
					for (int index_AutoList_1 = 0; index_AutoList_1 < count_AutoList_1; ++index_AutoList_1)
					{
						tableData.InnerDataList[index_InnerDataList_0].AutoList[index_AutoList_1].ID = sheet.Cells[index, innerIndex++].GetValue<int>();
						try
						{
							tableData.InnerDataList[index_InnerDataList_0].AutoList[index_AutoList_1].EnumValue = (TestEnum)Enum.Parse(typeof(TestEnum), sheet.Cells[index, innerIndex++].GetValue<string>());
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
					foreach (var obj_FixedList_0 in tableData.FixedList)
					{
						writer.Write(obj_FixedList_0);
					}
					writer.Write(tableData.AutoList.Length);
					foreach (var obj_AutoList_0 in tableData.AutoList)
					{
						writer.Write(obj_AutoList_0);
					}
					ExampleInnerData obj_InnerData_0 = tableData.InnerData;
					{
						writer.Write(obj_InnerData_0.ID);
						writer.Write(obj_InnerData_0.AutoList.Length);
						foreach (var obj_AutoList_1 in obj_InnerData_0.AutoList)
						{
							writer.Write(obj_AutoList_1.ID);
							writer.Write((int)obj_AutoList_1.EnumValue);
						}
					}
					writer.Write(tableData.InnerDataList.Length);
					foreach (var obj_InnerDataList_0 in tableData.InnerDataList)
					{
						writer.Write(obj_InnerDataList_0.ID);
						writer.Write(obj_InnerDataList_0.AutoList.Length);
						foreach (var obj_AutoList_1 in obj_InnerDataList_0.AutoList)
						{
							writer.Write(obj_AutoList_1.ID);
							writer.Write((int)obj_AutoList_1.EnumValue);
						}
					}
				}
			}
		}
		return true;
	}
}
