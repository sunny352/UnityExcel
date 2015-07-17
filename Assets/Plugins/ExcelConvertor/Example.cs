using UnityEngine;
using System.Collections;

public enum TestEnum
{
	enNone,
	enFirst,
	enSecond,
}

public class ExampleInnerInnerData
{
	public int ID { get; set; }
	public TestEnum EnumValue { get; set; }
}

public class ExampleInnerData
{
	public int ID { get; set; }
	public ExampleInnerInnerData[] AutoList { get; set; }
}

[PreLoadAttributes]
public class ExampleData
{
	[KeyPropAttributes]
	public int ID { get; set; }
	public string Name { get; set; }
	public float FloatValue { get; set; }
	public TestEnum EnumValue { get; set; }
	[ArrayLengthAttributes(5)]
	public int[] FixedList { get; set; }
	public int[] AutoList { get; set; }
	public ExampleInnerData InnerData { get; set; }
	public ExampleInnerData[] InnerDataList { get; set; }
}
