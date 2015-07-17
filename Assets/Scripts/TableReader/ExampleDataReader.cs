using System.Collections.Generic;
using System.IO;

public class ExampleDataReader
{
	public static ExampleData Lookup(int key)
	{
		if (null == m_data && !Load())
		{
			return null;
		}
		ExampleData data = null;
		m_data.TryGetValue(key, out data);
		return data;
	}
	public static bool Load()
	{
		byte[] bytes = TableBytesLoader.Load("ExampleData");
		if (null == bytes)
		{
			return false;
		}
		using (MemoryStream stream = new MemoryStream(bytes))
		{
			using (BinaryReader reader = new BinaryReader(stream))
			{
				m_data = new Dictionary<int, ExampleData>();
				int dataCount = reader.ReadInt32();
				for (int index = 0; index < dataCount; ++index)
				{
					ExampleData tableData = new ExampleData();
					{
						tableData.ID = reader.ReadInt32();
						tableData.Name = reader.ReadString();
						tableData.FloatValue = reader.ReadSingle();
						tableData.EnumValue = (TestEnum)reader.ReadInt32();
						int count_FixedList_6 = reader.ReadInt32();
						tableData.FixedList = new int[count_FixedList_6];
						for (int index_FixedList_6 = 0; index_FixedList_6 < count_FixedList_6; ++index_FixedList_6)
						{
							tableData.FixedList[index_FixedList_6] = reader.ReadInt32();
						}
						int count_AutoList_6 = reader.ReadInt32();
						tableData.AutoList = new int[count_AutoList_6];
						for (int index_AutoList_6 = 0; index_AutoList_6 < count_AutoList_6; ++index_AutoList_6)
						{
							tableData.AutoList[index_AutoList_6] = reader.ReadInt32();
						}
						ExampleInnerData obj_InnerData_6 = new ExampleInnerData();
						{
							obj_InnerData_6.ID = reader.ReadInt32();
							int count_AutoList_7 = reader.ReadInt32();
							obj_InnerData_6.AutoList = new ExampleInnerInnerData[count_AutoList_7];
							for (int index_AutoList_7 = 0; index_AutoList_7 < count_AutoList_7; ++index_AutoList_7)
							{
								obj_InnerData_6.AutoList[index_AutoList_7].ID = reader.ReadInt32();
								obj_InnerData_6.AutoList[index_AutoList_7].EnumValue = (TestEnum)reader.ReadInt32();
							}
						}
						tableData.InnerData = obj_InnerData_6;
						int count_InnerDataList_6 = reader.ReadInt32();
						tableData.InnerDataList = new ExampleInnerData[count_InnerDataList_6];
						for (int index_InnerDataList_6 = 0; index_InnerDataList_6 < count_InnerDataList_6; ++index_InnerDataList_6)
						{
							tableData.InnerDataList[index_InnerDataList_6].ID = reader.ReadInt32();
							int count_AutoList_7 = reader.ReadInt32();
							tableData.InnerDataList[index_InnerDataList_6].AutoList = new ExampleInnerInnerData[count_AutoList_7];
							for (int index_AutoList_7 = 0; index_AutoList_7 < count_AutoList_7; ++index_AutoList_7)
							{
								tableData.InnerDataList[index_InnerDataList_6].AutoList[index_AutoList_7].ID = reader.ReadInt32();
								tableData.InnerDataList[index_InnerDataList_6].AutoList[index_AutoList_7].EnumValue = (TestEnum)reader.ReadInt32();
							}
						}
					}
					m_data.Add(tableData.ID, tableData);
				}
			}
		}
		return true;
	}
	private static Dictionary<int, ExampleData> m_data = null;
}
