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
				}
			}
		}
		return true;
	}
}
