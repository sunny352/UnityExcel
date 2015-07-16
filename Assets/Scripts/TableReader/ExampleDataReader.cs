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
						for (int index_0 = 0; index_0 < count_FixedList_0; ++index_0)
						{
							tableData.FixedList[index_0] = reader.ReadInt32();
						}
						int count_AutoList_0 = reader.ReadInt32();
						tableData.AutoList = new int[count_AutoList_0];
						for (int index_0 = 0; index_0 < count_AutoList_0; ++index_0)
						{
							tableData.AutoList[index_0] = reader.ReadInt32();
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
