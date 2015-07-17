using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;

public class ConvertTools
{
	[MenuItem("ExcelTools/转换所有表")]
	public static void ConvertAll()
	{
		if (!Directory.Exists("Assets/Resources/Tables"))
		{
			Directory.CreateDirectory("Assets/Resources/Tables");
		}
		ExampleDataConvertor.Convert();
	}
}
