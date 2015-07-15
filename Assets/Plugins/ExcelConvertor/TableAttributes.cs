using UnityEngine;
using System.Collections;
using System;

//描述属性与表中数据的对应关系
public sealed class TablePropAttributes : Attribute
{
	public string CNName { get; set; }
	public string Desc { get; set; }
	public TablePropAttributes(string cnName, string desc)
	{
		CNName = cnName;
		Desc = desc;
	}
}

//修饰属性，表示被修饰的属性是索引，一个类中只能有一个索引
public sealed class KeyPropAttributes : Attribute
{
	public KeyPropAttributes()
	{

	}
}

//这个特性用于修饰类，表示需要预先加载
public sealed class PreLoadAttributes : Attribute
{
	public PreLoadAttributes()
	{

	}
}