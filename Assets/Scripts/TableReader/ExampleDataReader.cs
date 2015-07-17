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
						int count_FixedList_0 = reader.ReadInt32();
						tableData.FixedList = new int[count_FixedList_0];
						for (int index_FixedList_0 = 0; index_FixedList_0 < count_FixedList_0; ++index_FixedList_0)
						{
							tableData.FixedList[index_FixedList_0] = reader.ReadInt32();
						}
						int count_AutoList_0 = reader.ReadInt32();
						tableData.AutoList = new int[count_AutoList_0];
						for (int index_AutoList_0 = 0; index_AutoList_0 < count_AutoList_0; ++index_AutoList_0)
						{
							tableData.AutoList[index_AutoList_0] = reader.ReadInt32();
						}
						ExampleInnerData obj_InnerData_0 = new ExampleInnerData();
						{
							obj_InnerData_0.ID = reader.ReadInt32();
							int count_AutoList_1 = reader.ReadInt32();
							obj_InnerData_0.AutoList = new ExampleInnerInnerData[count_AutoList_1];
							for (int index_AutoList_1 = 0; index_AutoList_1 < count_AutoList_1; ++index_AutoList_1)
							{
								obj_InnerData_0.AutoList[index_AutoList_1].ID = reader.ReadInt32();
								obj_InnerData_0.AutoList[index_AutoList_1].EnumValue = (TestEnum)reader.ReadInt32();
							}
						}
						tableData.InnerData = obj_InnerData_0;
						int count_InnerDataList_0 = reader.ReadInt32();
						tableData.InnerDataList = new ExampleInnerData[count_InnerDataList_0];
						for (int index_InnerDataList_0 = 0; index_InnerDataList_0 < count_InnerDataList_0; ++index_InnerDataList_0)
						{
							tableData.InnerDataList[index_InnerDataList_0].ID = reader.ReadInt32();
							int count_AutoList_1 = reader.ReadInt32();
							tableData.InnerDataList[index_InnerDataList_0].AutoList = new ExampleInnerInnerData[count_AutoList_1];
							for (int index_AutoList_1 = 0; index_AutoList_1 < count_AutoList_1; ++index_AutoList_1)
							{
								tableData.InnerDataList[index_InnerDataList_0].AutoList[index_AutoList_1].ID = reader.ReadInt32();
								tableData.InnerDataList[index_InnerDataList_0].AutoList[index_AutoList_1].EnumValue = (TestEnum)reader.ReadInt32();
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
