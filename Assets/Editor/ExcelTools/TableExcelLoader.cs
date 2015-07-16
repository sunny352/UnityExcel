using UnityEngine;
using System.Collections;
using OfficeOpenXml;
using System.IO;

public class TableExcelLoader
{
	public delegate ExcelPackage TableLoaderDelegate(string tableName);
	public static TableLoaderDelegate Load
	{
		get { return null == m_loader ? DefaultLoader : m_loader; }
		set { m_loader = value; }
	}

	private static TableLoaderDelegate m_loader = null;
	private static ExcelPackage DefaultLoader(string tableName)
	{
		FileInfo newFile = new FileInfo("Tables/" + tableName + ".xlsx");
		if (newFile.Exists)
		{
			return new ExcelPackage(newFile);
		}
		else
		{
			return null;
		}
	}
}
