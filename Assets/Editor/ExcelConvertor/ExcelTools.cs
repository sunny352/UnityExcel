using OfficeOpenXml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class ExcelTools
{
	[MenuItem("ExcelTools/生成读表代码")]
	public static void CreateAllTableReader()
	{
		Directory.Delete(TableReaderFolder, true);
		AssetDatabase.Refresh();
		Directory.CreateDirectory(TableReaderFolder);

		List<Type> preLoadList = new List<Type>();
		CreateTableReader(typeof(ExampleData), preLoadList);


		AssetDatabase.Refresh();
	}
	private static readonly string TableReaderFolder = "Assets/Scripts/TableReader";
	private static void CreateTableReader(Type type, List<Type> preLoadList)
	{
		if (!type.IsClass)
		{
			Debug.LogWarning("Type " + type.ToString() + "is not class.");
			return;
		}
		using (StreamWriter writer = File.CreateText(TableReaderFolder + "/" + type.ToString() + "Reader.cs"))
		{
			writer.Write(CreateCS(type));
		}
		object[] preloadAttrs = type.GetCustomAttributes(typeof(PreLoadAttributes), true);
		if (null != preloadAttrs && preloadAttrs.Length > 1)
		{
			preLoadList.Add(type);
		}
	}
	public static string CreateCS(Type type)
	{
		PropertyInfo[] props = type.GetProperties();
		PropertyInfo keyInfo = null;
		foreach (var prop in props)
		{
			object[] tablePropList = prop.GetCustomAttributes(typeof(KeyPropAttributes), false);
			if (null != tablePropList && tablePropList.Length > 0)
			{
				keyInfo = prop;
				break;
			}
		}
		if (null == keyInfo)
		{
			return CreateDictionayCS(type, keyInfo);
		}
		else
		{
			return CreateListCS(type);
		}
	}
	public static string CreateDictionayCS(Type type, PropertyInfo keyInfo)
	{
		string csStr = "using System.Collections.Generic;\nusing System.IO;\n\npublic class {ClassName}Reader\n{\n\tpublic static {ClassName} Lookup({KeyType} key)\n\t{\n\t\tif (null == m_data && !Load())\n\t\t{\n\t\t\treturn null;\n\t\t}\n\t\t{ClassName} data = null;\n\t\tm_data.TryGetValue(key, out data);\n\t\treturn data;\n\t}\n\tpublic static bool Load()\n\t{\n\t\tbyte[] bytes = TableBytesLoader.Load(\"{ClassName}\");\n\t\tif (null == bytes)\n\t\t{\n\t\t\treturn false;\n\t\t}\n\t\tusing (MemoryStream stream = new MemoryStream(bytes))\n\t\t{\n\t\t\tusing (BinaryReader reader = new BinaryReader(stream))\n\t\t\t{\n\t\t\t\tm_data = new Dictionary<{KeyType}, {ClassName}>();\n\t\t\t\tint dataCount = reader.ReadInt32();\n\t\t\t\tfor (int index = 0; index < dataCount; ++index)\n\t\t\t\t{\n\t\t\t\t\t{ClassName} tableData = new {ClassName}();\n{ReadObject}\t\t\t\t\tm_data.Add(tableData.{KeyPropName}, tableData);\n\t\t\t\t}\n\t\t\t}\n\t\t}\n\t\treturn true;\n\t}\n\tprivate static Dictionary<{KeyType}, {ClassName}> m_data = null;\n}\n";
		csStr = csStr.Replace("{ClassName}", type.ToString());
		csStr = csStr.Replace("{KeyType}", GetTypeName(keyInfo.PropertyType));
		csStr = csStr.Replace("{KeyPropName}", keyInfo.Name);
		csStr = csStr.Replace("{ReadObject}", CreateObjectReaderString(type, null));
		return csStr;
	}
	public static string CreateListCS(Type type)
	{
		string csStr = "using System;\nusing System.Collections.Generic;\nusing System.IO;\n\npublic class {ClassName}Reader\n{\n\tpublic static {ClassName} Lookup(Predicate<{ClassName}> condition)\n\t{\n\t\tif (null == m_data && !Load())\n\t\t{\n\t\t\treturn null;\n\t\t}\n\t\treturn m_data.Find(condition);\n\t}\n\tpublic static List<{ClassName}> LookupAll(Predicate<{ClassName}> condition)\n\t{\n\t\tif (null == m_data && !Load())\n\t\t{\n\t\t\treturn null;\n\t\t}\n\t\treturn m_data.FindAll(condition);\n\t}\n\tpublic static bool Load()\n\t{\n\t\tbyte[] bytes = TableBytesLoader.Load(\"{ClassName}\");\n\t\tif (null == bytes)\n\t\t{\n\t\t\treturn false;\n\t\t}\n\t\tusing (MemoryStream stream = new MemoryStream(bytes))\n\t\t{\n\t\t\tusing (BinaryReader reader = new BinaryReader(stream))\n\t\t\t{\n\t\t\t\tm_data = new List<{ClassName}>();\n\t\t\t\tint dataCount = reader.ReadInt32();\n\t\t\t\tfor (int index = 0; index < dataCount; ++index)\n\t\t\t\t{\n\t\t\t\t\t{ClassName} tableData = new {ClassName}();\n{ReadObject}\t\t\t\t\tm_data.Add(tableData);\n\t\t\t\t}\n\t\t\t}\n\t\t}\n\t\treturn true;\n\t}\n\tprivate static List<{ClassName}> m_data = null;\n}\n";
		csStr = csStr.Replace("{ClassName}", type.ToString());
		csStr = csStr.Replace("{ReadObject}", CreateObjectReaderString(type, null));
		return csStr;
	}
	public static string CreateObjectReaderString(Type type, string preTab)
	{
		if (null == type)
		{
			return string.Empty;
		}
		if (string.IsNullOrEmpty(preTab))
		{
			preTab = "\t\t\t\t\t";
		}
		string csStr = string.Format("{0}{{\n", preTab);
		preTab += "\t";

		PropertyInfo[] propInfoList = type.GetProperties();
		foreach (var propInfo in propInfoList)
		{
			if (propInfo.PropertyType.IsEnum)
			{
				csStr += string.Format("{0}tableData.{1} = ({2})reader.ReadInt32();\n", preTab, propInfo.Name, propInfo.PropertyType.ToString());
			}
			else if (propInfo.PropertyType.IsPrimitive)
			{
				string typeName = propInfo.PropertyType.ToString();
				typeName = typeName.Substring("System.".Length);
				csStr += string.Format("{0}tableData.{1} = reader.Read{2}();\n", preTab, propInfo.Name, typeName);
			}
			else if (propInfo.PropertyType == typeof(string))
			{
				csStr += string.Format("{0}tableData.{1} = reader.ReadString();\n", preTab, propInfo.Name);
			}
			else if (propInfo.PropertyType.IsArray)
			{
				string countName = string.Format("count_{0}_{1}", propInfo.Name, preTab.Length - 6);
				csStr += string.Format("{0}int {1} = reader.ReadInt32();\n", preTab, countName);
				string subTypeName = propInfo.PropertyType.ToString();
				subTypeName = subTypeName.Substring(0, subTypeName.Length - 2);
				Type subType = GetTypeByName(subTypeName);
				if (null != subType)
				{
					csStr += string.Format("{0}tableData.{1} = new {2}[{3}];\n", preTab, propInfo.Name, GetTypeName(subType), countName);
					csStr += string.Format("{0}for (int index_{1} = 0; index_{1} < {2}; ++index_{1})\n", preTab, preTab.Length - 6, countName);
					if (subType.IsEnum)
					{
						csStr += string.Format("{0}{{\n", preTab);
						csStr += string.Format("{0}tableData.{1}[index_{2}] = ({3})reader.ReadInt32();\n", preTab + "\t", propInfo.Name, preTab.Length - 6, subTypeName);
						csStr += string.Format("{0}}}\n", preTab);
					}
					else if (subType.IsPrimitive)
					{
						csStr += string.Format("{0}{{\n", preTab);
						string typeName = subType.ToString();
						typeName = typeName.Substring("System.".Length);
						csStr += string.Format("{0}tableData.{1}[index_{2}] = reader.Read{3}();\n", preTab + "\t", propInfo.Name, preTab.Length - 6, typeName);
						csStr += string.Format("{0}}}\n", preTab);
					}
					else if (subType == typeof(string))
					{
						csStr += string.Format("{0}{{\n", preTab);
						csStr += string.Format("{0}tableData.{1}[index_{2}] = reader.ReadString();\n", preTab + "\t", propInfo.Name, preTab.Length - 6);
						csStr += string.Format("{0}}}\n", preTab);
					}
					else
					{
						csStr += CreateObjectReaderString(subType, preTab);
					}
				}
			}
			else
			{
				csStr += string.Format("{0}{1} obj_{2} = new {1}();\n", preTab, propInfo.Name, preTab.Length - 6);
				csStr += CreateObjectReaderString(propInfo.PropertyType, preTab);
				csStr += string.Format("{0}tableData.{1} = obj_{2};\n", preTab, propInfo.Name, preTab.Length - 6);
			}
		}
		preTab = preTab.Substring(1);
		csStr += string.Format("{0}}}\n", preTab);
		return csStr;
	}



	public static void CreateExcel()
	{
		var package = CreateExcel(typeof(ExampleData));
		Debug.Log(package.File.ToString());
	}
	public static ExcelPackage CreateExcel(Type type)
	{
		Directory.CreateDirectory("Tables/");
		FileInfo newFile = new FileInfo("Tables/" + type.ToString() + ".xlsx");
		ExcelPackage package = new ExcelPackage(newFile);
		ExcelWorksheet sheet = package.Workbook.Worksheets[type.ToString()];
		if (null == sheet)
		{
			sheet = package.Workbook.Worksheets.Add(type.ToString());
		}
		CreateCells(type, sheet, 1);
		package.Save();
		return package;
	}
	public static void ReadExcel()
	{
		var list = ReadExcel<ExampleData>();
		foreach (var item in list)
		{
			Debug.Log(item.ToString());
		}
	}
	public static List<T> ReadExcel<T>()
	{
		Type type = typeof(T);
		FileInfo newFile = new FileInfo("Tables/" + type.ToString() + ".xlsx");
		ExcelPackage package = new ExcelPackage(newFile);
		ExcelWorksheet sheet = package.Workbook.Worksheets[type.ToString()];
		List<T> excelObjList = new List<T>();
		for (int index = 0; index < sheet.Dimension.Rows - 3; ++index)
		{
			excelObjList.Add((T)System.Activator.CreateInstance(type));
		}
		if (excelObjList.Count < 1)
		{
			return excelObjList;
		}
		for (int index = 1; index <= sheet.Dimension.Columns; ++index)
		{
			string propName = sheet.Cells[1, index].GetValue<string>();
			string propType = sheet.Cells[2, index].GetValue<string>();
			PropertyInfo info = GetPropertyInfo(type, propName);
			if (null == info)
			{
				continue;
			}
			if (!info.PropertyType.IsArray)
			{
				for (int innerIndex = 0; innerIndex < excelObjList.Count; ++innerIndex)
				{
					ExcelRange cell = sheet.Cells[innerIndex + 4, index];
					excelObjList[innerIndex] = (T)GetCellValue(excelObjList[innerIndex], cell, info, propType);
				}
			}
			else
			{
				var subPropType = propType.Substring(0, propType.Length - 2);
				Type subType = GetTypeByName(subPropType);
				if (null == subType)
				{
					continue;
				}
				Type realType = GetTypeByName(propType);
				if (null == realType)
				{
					continue;
				}
				int maxCellCount = 0;
				for (int innerIndex = 0; innerIndex < excelObjList.Count; ++innerIndex)
				{
					ExcelRange cell = sheet.Cells[innerIndex + 4, index];
					int length = cell.GetValue<int>();
					if (length < 1)
					{
						continue;
					}
					ArrayList temp = new ArrayList();
					int currentCellCount = 1;
					for (int subIndex = 0; subIndex < length; ++subIndex)
					{
						object subObj = GetCellValue(sheet.Cells[innerIndex + 4, index + currentCellCount], subPropType);
						if (null == subObj)
						{
							subObj = System.Activator.CreateInstance(subType);
							++currentCellCount;
						}
						else
						{
							Debug.LogWarning(subObj);
							++currentCellCount;
						}
						temp.Add(subObj);
					}
					info.SetValue(excelObjList[innerIndex], temp.ToArray(subType), null);
					maxCellCount = maxCellCount < currentCellCount ? currentCellCount : maxCellCount;
				}
				index += maxCellCount - 1;
			}
		}
		return excelObjList;
	}

	private static PropertyInfo GetPropertyInfo(Type type, string propName)
	{
		var propList = type.GetProperties();
		PropertyInfo propInfo = null;
		foreach (var info in propList)
		{
			if (null != propInfo)
			{
				break;
			}
			object[] tablePropList = info.GetCustomAttributes(typeof(TablePropAttributes), false);
			foreach (object tableProp in tablePropList)
			{
				var prop = tableProp as TablePropAttributes;
				if (prop.CNName == propName)
				{
					propInfo = info;
					break;
				}
			}
		}
		return propInfo;
	}

	private static object GetCellValue(object obj, ExcelRange cell, PropertyInfo propInfo, string propType)
	{
		if (null == propInfo)
		{
			return obj;
		}
		propInfo.SetValue(obj, GetCellValue(cell, propType), null);
		return obj;
	}

	private static object GetCellValue(ExcelRange cell, string propType)
	{
		switch (propType)
		{
			case "bool":
				return cell.GetValue<bool>();
			case "int":
				return cell.GetValue<int>();
			case "float":
				return cell.GetValue<float>();
			case "double":
				return cell.GetValue<double>();
			case "string":
				return cell.GetValue<string>();
			default:
				break;
		}
		Type realType = GetTypeByName(propType);
		if (realType.IsEnum)
		{
			return Enum.Parse(realType, cell.GetValue<string>());
		}
		return null;
	}

	private static int CreateCells(Type type, ExcelWorksheet sheet, int currentCell)
	{
		PropertyInfo[] propInfo = type.GetProperties();
		foreach (PropertyInfo info in propInfo)
		{
			if (info.PropertyType.IsArray)
			{
				currentCell = CreateArrayCells(info, sheet, currentCell);
			}
			else if (info.PropertyType.IsPrimitive
				|| info.PropertyType == typeof(string)
				|| info.PropertyType.IsEnum)
			{
				object[] tablePropList = info.GetCustomAttributes(typeof(TablePropAttributes), false);
				foreach (object tableProp in tablePropList)
				{
					var prop = tableProp as TablePropAttributes;
					sheet.Cells[1, currentCell].Value = prop.CNName;
					sheet.Cells[2, currentCell].Value = GetTypeName(info.PropertyType);
					if (info.PropertyType.IsEnum)
					{
						string enumDesc = prop.Desc + "\n";
						var enumNames = Enum.GetNames(info.PropertyType);
						foreach (string enumName in enumNames)
						{
							enumDesc += "\n" + enumName;
						}
						sheet.Cells[3, currentCell].Value = enumDesc;
					}
					else
					{
						sheet.Cells[3, currentCell].Value = prop.Desc;
					}
					break;
				}
				currentCell++;
			}
			else
			{
				currentCell = CreateCells(info.PropertyType, sheet, currentCell);
			}
		}
		return currentCell;
	}
	private static string GetTypeName(Type type)
	{
		if (typeof(bool) == type)
		{
			return "bool";
		}
		if (typeof(int) == type)
		{
			return "int";
		}
		if (typeof(float) == type)
		{
			return "float";
		}
		if (typeof(double) == type)
		{
			return "double";
		}
		if (typeof(string) == type)
		{
			return "string";
		}

		if (typeof(bool[]) == type)
		{
			return "bool[]";
		}
		if (typeof(int[]) == type)
		{
			return "int[]";
		}
		if (typeof(float[]) == type)
		{
			return "float[]";
		}
		if (typeof(double[]) == type)
		{
			return "double[]";
		}
		if (typeof(string[]) == type)
		{
			return "string[]";
		}
		return type.ToString();
	}
	private static Type GetTypeByName(string typeName)
	{
		switch (typeName)
		{
			case "bool":
				return typeof(bool);
			case "int":
				return typeof(int);
			case "float":
				return typeof(float);
			case "double":
				return typeof(double);
			case "string":
				return typeof(string);
			case "bool[]":
				return typeof(bool[]);
			case "int[]":
				return typeof(int[]);
			case "float[]":
				return typeof(float[]);
			case "double[]":
				return typeof(double[]);
			case "string[]":
				return typeof(string[]);
			default:
				Type type = Type.GetType(typeName);
				if (type == null)
				{
					type = Type.GetType(typeName + ", UnityEngine");
				}
				if (type == null)
				{
					type = Type.GetType(typeName + ", Assembly-CSharp-firstpass");
				}
				if (type == null)
				{
					type = Type.GetType(typeName + ", Assembly-CSharp");
				}
				return type;
		}
	}
	private static int CreateArrayCells(PropertyInfo propInfo, ExcelWorksheet sheet, int currentCell)
	{
		if (!propInfo.PropertyType.IsArray)
		{
			return currentCell;
		}
		object[] tablePropList = propInfo.GetCustomAttributes(typeof(TablePropAttributes), false);
		foreach (object tableProp in tablePropList)
		{
			var prop = tableProp as TablePropAttributes;
			sheet.Cells[1, currentCell].Value = prop.CNName;
			sheet.Cells[2, currentCell].Value = GetTypeName(propInfo.PropertyType);
			sheet.Cells[3, currentCell].Value = prop.Desc;
			break;
		}
		return ++currentCell;
	}
}
