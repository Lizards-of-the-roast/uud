using System;
using System.Collections.Generic;
using System.Reflection;

namespace Wotc.Mtga.Network;

public static class ModelConversionUtils
{
	public static T2 CloneAndConvert<T2>(this object first) where T2 : new()
	{
		T2 val = new T2();
		Type type = first.GetType();
		Type type2 = val.GetType();
		FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
		FieldInfo[] fields2 = type2.GetFields(BindingFlags.Instance | BindingFlags.Public);
		if (fields.Length != fields2.Length)
		{
			throw new InvalidCastException($"Unable to cast {type} to {type2}!");
		}
		for (int i = 0; i < fields.Length; i++)
		{
			FieldInfo fieldInfo = fields[i];
			FieldInfo fieldInfo2 = fields2[i];
			if (!fieldInfo.Name.Equals(fieldInfo2.Name) || fieldInfo.FieldType != fieldInfo2.FieldType)
			{
				throw new InvalidCastException($"Unable to cast {type} to {type2}!");
			}
			fieldInfo2.SetValue(val, fieldInfo.GetValue(first));
		}
		return val;
	}

	public static List<T> CloneAndConvert<T>(this IEnumerable<object> first) where T : new()
	{
		List<T> list = new List<T>();
		foreach (object item in first)
		{
			list.Add(item.CloneAndConvert<T>());
		}
		return list;
	}
}
