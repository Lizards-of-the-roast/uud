using System;
using UnityEngine;

internal static class EnumSelectionGrid<T> where T : Enum
{
	private static readonly string[] names;

	static EnumSelectionGrid()
	{
		names = Enum.GetNames(typeof(T));
	}

	public static T Draw(T selected, int width)
	{
		return (T)(object)GUILayout.SelectionGrid((int)(object)selected, names, width);
	}
}
