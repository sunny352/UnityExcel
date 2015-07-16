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
	public int ID { get; set; }
	public string Name { get; set; }
	public float FloatValue { get; set; }
	public TestEnum EnumValue { get; set; }
	[ArrayLengthAttributes(5)]
	public int[] FixedList { get; set; }
	public int[] AutoList { get; set; }
}
