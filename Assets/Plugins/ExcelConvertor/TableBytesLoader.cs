using UnityEngine;
using System.Collections;
using System;

public class TableBytesLoader
{
	public delegate byte[] TableLoaderDelegate(string tableName);
	public static TableLoaderDelegate Load
	{ 
		get { return null == m_loader ? DefaultLoader : m_loader; } 
		set { m_loader = value; } 
	}

	private static TableLoaderDelegate m_loader = null;
	private static byte[] DefaultLoader(string tableName)
	{
		TextAsset tableAssets = Resources.Load<TextAsset>(string.Format("Tables/{0}", tableName));
		if (null == tableAssets)
		{
			return null;
		}
		else
		{
			byte[] tableBytes = new byte[tableAssets.bytes.Length];
			Array.Copy(tableBytes, tableAssets.bytes, tableAssets.bytes.Length);
			Resources.UnloadAsset(tableAssets);
			return tableBytes;
		}
	}
}
