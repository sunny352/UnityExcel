using System;
using System.Collections.Generic;
using System.IO;

public class ExampleDataReader
{
	public static ExampleData Lookup(Predicate<ExampleData> condition)
	{
		if (null == m_data && !Load())
		{
			return null;
		}
		return m_data.Find(condition);
	}
	public static List<ExampleData> LookupAll(Predicate<ExampleData> condition)
	{
		if (null == m_data && !Load())
		{
			return null;
		}
		return m_data.FindAll(condition);
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
				m_data = new List<ExampleData>();
				int dataCount = reader.ReadInt32();
				for (int index = 0; index < dataCount; ++index)
				{
					ExampleData tableData = new ExampleData();
					{
						tableData.ID = reader.ReadInt32();
						tableData.Name = reader.ReadString();
						tableData.FloatValue = reader.ReadSingle();
						tableData.EnumValue = (TestEnum)reader.ReadInt32();
						int count_IntList_0 = reader.ReadInt32();
						tableData.IntList = new int[count_IntList_0];
						for (int index_0 = 0; index_0 < count_IntList_0; ++index_0)
						{
							tableData.IntList[index_0] = reader.ReadInt32();
						}
					}
					m_data.Add(tableData);
				}
			}
		}
		return true;
	}
	private static List<ExampleData> m_data = null;
}
