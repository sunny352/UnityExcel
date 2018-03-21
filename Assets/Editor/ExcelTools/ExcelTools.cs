using OfficeOpenXml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using OfficeOpenXml.FormulaParsing;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using UnityEditor;
using UnityEngine;

public class ExcelTools
{
	private static Type[] m_tableTypes = { 
		typeof(ExampleData)
	};

	private static readonly string TableReaderFolder = "Assets/Scripts/TableReader";
	private static readonly string TableConvertorFolder = "Assets/Editor/ExcelConvertor";
	[MenuItem("ExcelTools/生成表格代码")]
	public static void CreateAllTableCodes()
	{
		CreateAllTableConvertor();
		CreateAllTableReader();
	}
	//[MenuItem("ExcelTools/生成读表代码")]
	public static void CreateAllTableReader()
	{
		Directory.Delete(TableReaderFolder, true);
		AssetDatabase.Refresh();
		Directory.CreateDirectory(TableReaderFolder);

		using (StreamWriter writer = File.CreateText(TableReaderFolder + "/TableManager.cs"))
		{
			string csStr = "using UnityEngine;\nusing System.Collections;\n\npublic class TableManager\n{\n\tpublic static void LoadTables()\n\t{\n";
			foreach (Type type in m_tableTypes)
			{
				CreateTableReader(type);
				if (type.IsDefined(typeof(PreLoadAttributes), true))
				{
					csStr += string.Format("\t\t{0}Reader.Load();\n", type.ToString());
				}
			}
			csStr += "\t}\n}\n";
			writer.Write(csStr);
		}

		AssetDatabase.Refresh();
	}
	private static void CreateTableReader(Type type)
	{
		if (!type.IsClass)
		{
			Debug.LogWarning("Type " + type.ToString() + "is not class.");
			return;
		}
		using (StreamWriter writer = File.CreateText(TableReaderFolder + "/" + type.ToString() + "Reader.cs"))
		{
			writer.Write(CreateReaderCS(type));
		}
	}
	public static string CreateReaderCS(Type type)
	{
		PropertyInfo[] props = type.GetProperties();
		PropertyInfo keyInfo = null;
		foreach (var prop in props)
		{
			if (prop.IsDefined(typeof(KeyPropAttributes), false))
			{
				keyInfo = prop;
				break;
			}
		}
		if (null != keyInfo)
		{
			return CreateDictionayReader(type, keyInfo);
		}
		else
		{
			return CreateListReader(type);
		}
	}
	public static string CreateDictionayReader(Type type, PropertyInfo keyInfo)
	{
		string csStr = "using System.Collections.Generic;\nusing System.IO;\n\npublic class {ClassName}Reader\n{\n\tpublic static {ClassName} Lookup({KeyType} key)\n\t{\n\t\tif (null == m_data && !Load())\n\t\t{\n\t\t\treturn null;\n\t\t}\n\t\t{ClassName} data = null;\n\t\tm_data.TryGetValue(key, out data);\n\t\treturn data;\n\t}\n\tpublic static bool Load()\n\t{\n\t\tbyte[] bytes = TableBytesLoader.Load(\"{ClassName}\");\n\t\tif (null == bytes)\n\t\t{\n\t\t\treturn false;\n\t\t}\n\t\tusing (MemoryStream stream = new MemoryStream(bytes))\n\t\t{\n\t\t\tusing (BinaryReader reader = new BinaryReader(stream))\n\t\t\t{\n\t\t\t\tm_data = new Dictionary<{KeyType}, {ClassName}>();\n\t\t\t\tint dataCount = reader.ReadInt32();\n\t\t\t\tfor (int index = 0; index < dataCount; ++index)\n\t\t\t\t{\n\t\t\t\t\t{ClassName} tableData = new {ClassName}();\n{ReadObject}\t\t\t\t\tm_data.Add(tableData.{KeyPropName}, tableData);\n\t\t\t\t}\n\t\t\t}\n\t\t}\n\t\treturn true;\n\t}\n\tprivate static Dictionary<{KeyType}, {ClassName}> m_data = null;\n}\n";
		csStr = csStr.Replace("{ClassName}", type.ToString());
		csStr = csStr.Replace("{KeyType}", GetTypeName(keyInfo.PropertyType));
		csStr = csStr.Replace("{KeyPropName}", keyInfo.Name);
		csStr = csStr.Replace("{ReadObject}", CreateObjectReaderString(type, "\t\t\t\t\t", "tableData"));
		return csStr;
	}
	public static string CreateListReader(Type type)
	{
		string csStr = "using System;\nusing System.Collections.Generic;\nusing System.IO;\n\npublic class {ClassName}Reader\n{\n\tpublic static {ClassName} Lookup(Predicate<{ClassName}> condition)\n\t{\n\t\tif (null == m_data && !Load())\n\t\t{\n\t\t\treturn null;\n\t\t}\n\t\treturn m_data.Find(condition);\n\t}\n\tpublic static List<{ClassName}> LookupAll(Predicate<{ClassName}> condition)\n\t{\n\t\tif (null == m_data && !Load())\n\t\t{\n\t\t\treturn null;\n\t\t}\n\t\treturn m_data.FindAll(condition);\n\t}\n\tpublic static bool Load()\n\t{\n\t\tbyte[] bytes = TableBytesLoader.Load(\"{ClassName}\");\n\t\tif (null == bytes)\n\t\t{\n\t\t\treturn false;\n\t\t}\n\t\tusing (MemoryStream stream = new MemoryStream(bytes))\n\t\t{\n\t\t\tusing (BinaryReader reader = new BinaryReader(stream))\n\t\t\t{\n\t\t\t\tm_data = new List<{ClassName}>();\n\t\t\t\tint dataCount = reader.ReadInt32();\n\t\t\t\tfor (int index = 0; index < dataCount; ++index)\n\t\t\t\t{\n\t\t\t\t\t{ClassName} tableData = new {ClassName}();\n{ReadObject}\t\t\t\t\tm_data.Add(tableData);\n\t\t\t\t}\n\t\t\t}\n\t\t}\n\t\treturn true;\n\t}\n\tprivate static List<{ClassName}> m_data = null;\n}\n";
		csStr = csStr.Replace("{ClassName}", type.ToString());
		csStr = csStr.Replace("{ReadObject}", CreateObjectReaderString(type, "\t\t\t\t\t", "tableData"));
		return csStr;
	}
	public static string Push(ref string preTab)
	{
		string returnStr = string.Format("{0}{{\n", preTab);
		preTab += "\t";
		return returnStr;
	}
	public static string Pop(ref string preTab)
	{
		preTab = preTab.Substring(1);
		return string.Format("{0}}}\n", preTab);
	}
	public static string CreateObjectReaderString(Type type, string preTab, string dataName)
	{
		string csStr = Push(ref preTab);

		PropertyInfo[] propInfoList = type.GetProperties();
		foreach (var propInfo in propInfoList)
		{
			if (propInfo.PropertyType.IsEnum)
			{
				csStr += string.Format("{0}{1}.{2} = ({3})reader.ReadInt32();\n", preTab, dataName, propInfo.Name, propInfo.PropertyType.ToString());
			}
			else if (propInfo.PropertyType.IsPrimitive)
			{
				string typeName = propInfo.PropertyType.ToString();
				typeName = typeName.Substring("System.".Length);
				csStr += string.Format("{0}{1}.{2} = reader.Read{3}();\n", preTab, dataName, propInfo.Name, typeName);
			}
			else if (propInfo.PropertyType == typeof(string))
			{
				csStr += string.Format("{0}{1}.{2} = reader.ReadString();\n", preTab, dataName, propInfo.Name);
			}
			else if (propInfo.PropertyType.IsArray)
			{
				string countName = string.Format("count_{0}_{1}", propInfo.Name, preTab.Length);
				string indexName = string.Format("index_{0}_{1}", propInfo.Name, preTab.Length);
				string newDataName = string.Format("{0}.{1}[{2}]", dataName, propInfo.Name, indexName);

				string subTypeName = propInfo.PropertyType.ToString();
				subTypeName = subTypeName.Substring(0, subTypeName.Length - 2);
				Type subType = GetTypeByName(subTypeName);
				if (null != subType)
				{
					csStr += string.Format("{0}int {1} = reader.ReadInt32();\n", preTab, countName);
					csStr += string.Format("{0}{1}.{2} = new {3}[{4}];\n", preTab, dataName, propInfo.Name, GetTypeName(subType), countName);
					csStr += string.Format("{0}for (int {1} = 0; {1} < {2}; ++{1})\n", preTab, indexName, countName);
					if (subType.IsEnum)
					{
						csStr += Push(ref preTab);
						csStr += string.Format("{0}{1} = ({2})reader.ReadInt32();\n", preTab, newDataName, subTypeName);
						csStr += Pop(ref preTab);
					}
					else if (subType.IsPrimitive)
					{
						csStr += Push(ref preTab);
						csStr += string.Format("{0}{1} = reader.Read{2}();\n", preTab, newDataName, subTypeName.Substring("System.".Length));
						csStr += Pop(ref preTab);
					}
					else if (subType == typeof(string))
					{
						csStr += Push(ref preTab);
						csStr += string.Format("{0}{1} = reader.ReadString();\n", preTab, newDataName);
						csStr += Pop(ref preTab);
					}
					else
					{
						csStr += CreateObjectReaderString(subType, preTab, newDataName);
					}
				}
			}
			else
			{
				string objName = string.Format("obj_{0}_{1}", propInfo.Name, preTab.Length);
				csStr += string.Format("{0}{1} {2} = new {1}();\n", preTab, GetTypeName(propInfo.PropertyType), objName);
				csStr += CreateObjectReaderString(propInfo.PropertyType, preTab, objName);
				csStr += string.Format("{0}{1}.{2} = {3};\n", preTab, dataName, propInfo.Name, objName);
			}
		}
		csStr += Pop(ref preTab);
		return csStr;
	}

	//[MenuItem("ExcelTools/生成Excel转换代码")]
	public static void CreateAllTableConvertor()
	{
		Directory.Delete(TableConvertorFolder, true);
		AssetDatabase.Refresh();
		Directory.CreateDirectory(TableConvertorFolder);

		using (StreamWriter writer = File.CreateText(TableConvertorFolder + "/ConvertorTools.cs"))
		{
			string csStr = "using UnityEngine;\nusing System.Collections;\nusing UnityEditor;\nusing System.IO;\n\npublic class ConvertTools\n{\n\t[MenuItem(\"ExcelTools/转换所有表\")]\n\tpublic static void ConvertAll()\n\t{\n\t\tif (!Directory.Exists(\"Assets/Resources/Tables\"))\n\t\t{\n\t\t\tDirectory.CreateDirectory(\"Assets/Resources/Tables\");\n\t\t}\n";
			foreach (Type type in m_tableTypes)
			{
				CreateTableConvertor(type);
				csStr += string.Format("\t\t{0}Convertor.Convert();\n", type.ToString());
			}
			csStr += "\t}\n}\n";
			writer.Write(csStr);
		}
		AssetDatabase.Refresh();
	}
	public static void CreateTableConvertor(Type type)
	{
		if (!type.IsClass)
		{
			Debug.LogWarning("Type " + type.ToString() + "is not class.");
			return;
		}
		using (StreamWriter writer = File.CreateText(TableConvertorFolder + "/" + type.ToString() + "Convertor.cs"))
		{
			writer.Write(CreateConvertorCS(type));
		}
	}
	public static string CreateConvertorCS(Type type)
	{
		PropertyInfo[] props = type.GetProperties();
		PropertyInfo keyInfo = null;
		foreach (var prop in props)
		{
			if (prop.IsDefined(typeof(KeyPropAttributes), false))
			{
				keyInfo = prop;
				break;
			}
		}
		if (null != keyInfo)
		{
			return CreateDictionayConvertor(type, keyInfo);
		}
		else
		{
			return CreateListConvertor(type);
		}
	}
	public static string CreateDictionayConvertor(Type type, PropertyInfo keyInfo)
	{
		string csStr = "using UnityEngine;\nusing System;\nusing System.Collections;\nusing System.Collections.Generic;\nusing OfficeOpenXml;\nusing System.IO;\n\npublic class {ClassName}Convertor\n{\n\tprivate static List<{ClassName}> m_data = new List<{ClassName}>();\n\tpublic static bool Convert()\n\t{\n\t\tif (ReadExcel())\n\t\t{\n\t\t\tSaveBytes();\n\t\t\treturn true;\n\t\t}\n\t\telse\n\t\t{\n\t\t\treturn false;\n\t\t}\n\t}\n\n\tprivate static bool ReadExcel()\n\t{\n\t\tExcelPackage package = TableExcelLoader.Load(\"{ClassName}\");\n\t\tif (null == package)\n\t\t{\n\t\t\treturn false;\n\t\t}\n\t\tExcelWorksheet sheet = package.Workbook.Worksheets[\"{ClassName}\"];\n\t\tif (null == sheet)\n\t\t{\n\t\t\treturn false;\n\t\t}\n\t\t{KeyType} defaultKey = new {KeyType}();\n\t\tfor (int index = 1; index <= sheet.Dimension.Rows; ++index)\n\t\t{\n\t\t\tvar tableData = new {ClassName}();\n\t\t\tint innerIndex = 1;\n{ReadExcel}\t\t\tif (tableData.{KeyPropName} == defaultKey)\n\t\t\t{\n\t\t\t\tcontinue;\n\t\t\t}\n\t\t\tvar existKey = m_data.Find(innerItem => innerItem.{KeyPropName} == tableData.{KeyPropName});\n\t\t\tif (null != existKey)\n\t\t\t{\n\t\t\t\tDebug.LogWarning(string.Format(\"Already has the key {0}, replace the old data.\", tableData.{KeyPropName}));\n\t\t\t\tm_data.Remove(existKey);\n\t\t\t}\n\n\t\t\tm_data.Add(tableData);\n\t\t}\n\t\treturn true;\n\t}\n\n\tprivate static bool SaveBytes()\n\t{\n\t\tusing (FileStream bytesFile = File.Create(\"Assets/Resources/Tables/{ClassName}.bytes\"))\n\t\t{\n\t\t\tusing (BinaryWriter writer = new BinaryWriter(bytesFile))\n\t\t\t{\n\t\t\t\twriter.Write(m_data.Count);\n\t\t\t\tforeach (var tableData in m_data)\n{WriteBytes}\t\t\t}\n\t\t}\n\t\treturn true;\n\t}\n}\n";
		csStr = csStr.Replace("{ClassName}", type.ToString());
		csStr = csStr.Replace("{KeyType}", GetTypeName(keyInfo.PropertyType));
		csStr = csStr.Replace("{KeyPropName}", keyInfo.Name);
		csStr = csStr.Replace("{ReadExcel}", CreateObjrectReadExcel(type, "\t\t\t", "tableData"));
		csStr = csStr.Replace("{WriteBytes}", CreateObjrectWriteBytes(type, "\t\t\t\t", "tableData"));
		return csStr;
	}
	public static string CreateListConvertor(Type type)
	{
		string csStr = "using UnityEngine;\nusing System;\nusing System.Collections;\nusing System.Collections.Generic;\nusing OfficeOpenXml;\nusing System.IO;\n\npublic class {ClassName}Convertor\n{\n\tprivate static List<{ClassName}> m_data = new List<{ClassName}>();\n\tpublic static bool Convert()\n\t{\n\t\tif (ReadExcel())\n\t\t{\n\t\t\tSaveBytes();\n\t\t\treturn true;\n\t\t}\n\t\telse\n\t\t{\n\t\t\treturn false;\n\t\t}\n\t}\n\n\tprivate static bool ReadExcel()\n\t{\n\t\tExcelPackage package = TableExcelLoader.Load(\"{ClassName}\");\n\t\tif (null == package)\n\t\t{\n\t\t\treturn false;\n\t\t}\n\t\tExcelWorksheet sheet = package.Workbook.Worksheets[\"{ClassName}\"];\n\t\tif (null == sheet)\n\t\t{\n\t\t\treturn false;\n\t\t}\n\t\tfor (int index = 1; index <= sheet.Dimension.Rows; ++index)\n\t\t{\n\t\t\tvar tableData = new {ClassName}();\n\t\t\tint innerIndex = 1;\n{ReadExcel}\t\t\tm_data.Add(tableData);\n\t\t}\n\t\treturn true;\n\t}\n\n\tprivate static bool SaveBytes()\n\t{\n\t\tusing (FileStream bytesFile = File.Create(\"Assets/Resources/Tables/{ClassName}.bytes\"))\n\t\t{\n\t\t\tusing (BinaryWriter writer = new BinaryWriter(bytesFile))\n\t\t\t{\n\t\t\t\twriter.Write(m_data.Count);\n\t\t\t\tforeach (var tableData in m_data)\n{WriteBytes}\t\t\t}\n\t\t}\n\t\treturn true;\n\t}\n}\n";
		csStr = csStr.Replace("{ClassName}", type.ToString());
		csStr = csStr.Replace("{ReadExcel}", CreateObjrectReadExcel(type, "\t\t\t", "tableData"));
		csStr = csStr.Replace("{WriteBytes}", CreateObjrectWriteBytes(type, "\t\t\t\t", "tableData"));
		return csStr;
	}

	public static string GetCellString(ExcelRangeBase cell)
	{
		var value = cell.GetValue<string>();
		return string.IsNullOrEmpty(value) ? string.Empty : value;
	}
	public static string CreateObjrectReadExcel(Type type, string preTab, string dataName)
	{
		string csStr = Push(ref preTab);

		PropertyInfo[] propInfoList = type.GetProperties();
		foreach (var propInfo in propInfoList)
		{
			if (propInfo.PropertyType.IsEnum)
			{
				csStr += string.Format("{0}try\n", preTab);
				csStr += Push(ref preTab);
				csStr += string.Format("{0}{1}.{2} = ({3})Enum.Parse(typeof({3}), ExcelTools.GetCellString(sheet.Cells[index, innerIndex++]));\n", preTab, dataName, propInfo.Name, propInfo.PropertyType.ToString());
				csStr += Pop(ref preTab);
				csStr += string.Format("{0}catch(System.Exception ex)\n{0}{{\n{0}\tDebug.LogException(ex);\n{0}}}\n", preTab);
			}
			else if (propInfo.PropertyType.IsPrimitive)
			{
				csStr += string.Format("{0}{1}.{2} = sheet.Cells[index, innerIndex++].GetValue<{3}>();\n", preTab, dataName, propInfo.Name, GetTypeName(propInfo.PropertyType));
			}
			else if (propInfo.PropertyType == typeof(string))
			{
				csStr += string.Format("{0}{1}.{2} = ExcelTools.GetCellString(sheet.Cells[index, innerIndex++]);\n", preTab, dataName, propInfo.Name);
			}
			else if (propInfo.PropertyType.IsArray)
			{
				string countName = string.Format("count_{0}_{1}", propInfo.Name, preTab.Length);
				string indexName = string.Format("index_{0}_{1}", propInfo.Name, preTab.Length);
				string newDataName = string.Format("{0}.{1}[{2}]", dataName, propInfo.Name, indexName);

				object[] arrayLengthAttr = propInfo.GetCustomAttributes(typeof(ArrayLengthAttributes), true);
				if (null != arrayLengthAttr && arrayLengthAttr.Length > 0)
				{
					foreach (object attr in arrayLengthAttr)
					{
						ArrayLengthAttributes realAttr = attr as ArrayLengthAttributes;
						csStr += string.Format("{0}int {1} = {2};\n", preTab, countName, realAttr.Length);
						break;
					}
				}
				else
				{
					csStr += string.Format("{0}int {1} = sheet.Cells[index, innerIndex++].GetValue<int>();\n", preTab, countName);
				}

				string subTypeName = propInfo.PropertyType.ToString();
				subTypeName = subTypeName.Substring(0, subTypeName.Length - 2);
				Type subType = GetTypeByName(subTypeName);
				if (null != subType)
				{
					csStr += string.Format("{0}{1}.{2} = new {3}[{4}];\n", preTab, dataName, propInfo.Name, GetTypeName(subType), countName);
					csStr += string.Format("{0}for (int {1} = 0; {1} < {2}; ++{1})\n", preTab, indexName, countName);
					if (subType.IsEnum)
					{
						csStr += string.Format("{0}try\n", preTab);
						csStr += Push(ref preTab);
						csStr += string.Format("{0}{1} = ({2})Enum.Parse(typeof({2}), ExcelTools.GetCellString(sheet.Cells[index, innerIndex++]));\n", preTab, newDataName, GetTypeName(subType));
						csStr += Pop(ref preTab);
						csStr += string.Format("{0}catch(System.Exception ex)\n{0}{{\n{0}\tDebug.LogException(ex);\n{0}}}\n", preTab);
					}
					else if (subType.IsPrimitive)
					{
						csStr += Push(ref preTab);
						csStr += string.Format("{0}{1} = sheet.Cells[index, innerIndex++].GetValue<{2}>();\n", preTab, newDataName, GetTypeName(subType));
						csStr += Pop(ref preTab);
					}
					else if (subType == typeof(string))
					{
						csStr += Push(ref preTab);
						csStr += string.Format("{0}{1} = ExcelTools.GetCellString(sheet.Cells[index, innerIndex++]);\n", preTab, newDataName);
						csStr += Pop(ref preTab);
					}
					else
					{
						csStr += CreateObjrectReadExcel(GetTypeByName(subTypeName), preTab, newDataName);
					}
				}
			}
			else
			{
				string newDataName = string.Format("obj_{0}_{1}", propInfo.Name, preTab.Length);
				csStr += string.Format("{0}{1} {2} = new {1}();\n", preTab, GetTypeName(propInfo.PropertyType), newDataName);
				csStr += CreateObjrectReadExcel(propInfo.PropertyType, preTab, newDataName);
				csStr += string.Format("{0}{1}.{2} = {3};\n", preTab, dataName, propInfo.Name, newDataName);
			}
		}
		csStr += Pop(ref preTab);
		return csStr;
	}
	public static string CreateObjrectWriteBytes(Type type, string preTab, string dataName)
	{
		string csStr = Push(ref preTab);

		PropertyInfo[] propInfoList = type.GetProperties();
		foreach (var propInfo in propInfoList)
		{
			if (propInfo.PropertyType.IsEnum)
			{
				csStr += string.Format("{0}writer.Write((int){1}.{2});\n", preTab, dataName, propInfo.Name);
			}
			else if (propInfo.PropertyType.IsPrimitive)
			{
				csStr += string.Format("{0}writer.Write({1}.{2});\n", preTab, dataName, propInfo.Name);
			}
			else if (propInfo.PropertyType == typeof(string))
			{
				csStr += string.Format("{0}writer.Write({1}.{2});\n", preTab, dataName, propInfo.Name);
			}
			else if (propInfo.PropertyType.IsArray)
			{
				string newDataName = string.Format("obj_{0}_{1}", propInfo.Name, preTab.Length);
				csStr += string.Format("{0}writer.Write({1}.{2}.Length);\n", preTab, dataName, propInfo.Name);
				csStr += string.Format("{0}foreach (var {1} in {2}.{3})\n", preTab, newDataName, dataName, propInfo.Name);
				string subTypeName = propInfo.PropertyType.ToString();
				subTypeName = subTypeName.Substring(0, subTypeName.Length - 2);
				Type subType = GetTypeByName(subTypeName);
				if (subType.IsEnum)
				{
					csStr += Push(ref preTab);
					csStr += string.Format("{0}writer.Write((int){1});\n", preTab, newDataName);
					csStr += Pop(ref preTab);
				}
				else if (subType.IsPrimitive)
				{
					csStr += Push(ref preTab);
					csStr += string.Format("{0}writer.Write({1});\n", preTab, newDataName);
					csStr += Pop(ref preTab);
				}
				else if (subType == typeof(string))
				{
					csStr += Push(ref preTab);
					csStr += string.Format("{0}writer.Write({1});\n", preTab, newDataName);
					csStr += Pop(ref preTab);
				}
				else
				{
					csStr += CreateObjrectWriteBytes(subType, preTab, newDataName);
				}
			}
			else
			{
				string newDataName = string.Format("obj_{0}_{1}", propInfo.Name, preTab.Length);
				csStr += string.Format("{0}var {1} = tableData.{2};\n", preTab, newDataName, propInfo.Name);
				csStr += CreateObjrectWriteBytes(propInfo.PropertyType, preTab, newDataName);
			}
		}
		csStr += Pop(ref preTab);
		return csStr;
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
}
