using UnityEngine;
using System.Collections;
using System;

//这个特性用于修饰类，表示需要预先加载
public sealed class PreLoadAttributes : Attribute
{
	public PreLoadAttributes()
	{

	}
}

//修饰属性，表示被修饰的属性是索引，一个类中只能有一个索引
public sealed class KeyPropAttributes : Attribute
{
	public KeyPropAttributes()
	{

	}
}

//专用于修饰数组属性，描述数组固定长度
public sealed class ArrayLengthAttributes : Attribute
{
	public int Length { get; private set; }
	public ArrayLengthAttributes(int length)
	{
		Length = length;
	}
}