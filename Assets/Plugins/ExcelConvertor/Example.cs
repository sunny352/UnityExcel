using UnityEngine;
using System.Collections;

public enum TestEnum
{
	enNone,
	enFirst,
	enSecond,
}

[PreLoadAttributes]
public class ExampleData
{
	[KeyPropAttributes]
	[TablePropAttributes("ID", "唯一识别ID")]
	public int ID { get; set; }
	[TablePropAttributes("名称", "条目名称")]
	public string Name { get; set; }
	[TablePropAttributes("浮点数值", "测试浮点数值")]
	public float FloatValue { get; set; }
	[TablePropAttributes("枚举值", "测试枚举值")]
	public TestEnum EnumValue { get; set; }
	[TablePropAttributes("列表", "测试整形数组")]
	public int[] IntList { get; set; }

	public override string ToString()
	{
		return string.Format("{0} {1} {2} {3} {4}", ID, Name, FloatValue, EnumValue, null != IntList ? IntList.Length : 0);
	}
}
