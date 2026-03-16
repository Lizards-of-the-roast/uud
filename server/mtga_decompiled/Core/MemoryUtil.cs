using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling;

public class MemoryUtil
{
	private class MemoryObject
	{
		public string Name;

		public double Size;

		public uint Count;

		public double TotalSize => Size * (double)Count;
	}

	public static void DumpMemoryUsageToFile()
	{
		string memoryDocumentsPath = GetMemoryDocumentsPath();
		string path = string.Format("MemoryDump_{0}.txt", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
		using StreamWriter streamWriter = FileSystemUtils.CreateText(Path.Combine(memoryDocumentsPath, path));
		streamWriter.Write(GetMemoryUsageString());
	}

	private static string GetMemoryDocumentsPath()
	{
		if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
		{
			string text = Path.Combine(Application.persistentDataPath, "Memory");
			Directory.CreateDirectory(text);
			return text;
		}
		string text2 = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/MTGA/Memory/";
		Directory.CreateDirectory(text2);
		return text2;
	}

	public static void DumpMemoryUsageToLog()
	{
		Debug.Log(GetMemoryUsageString());
	}

	public static string GetMemoryUsageString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(GetMemoryUsage<Shader>());
		stringBuilder.Append(GetMemoryUsage<Material>());
		stringBuilder.Append(GetMemoryUsage<ParticleSystem>());
		stringBuilder.Append(GetMemoryUsage<Mesh>());
		stringBuilder.Append(GetMemoryUsage<Texture>());
		return stringBuilder.ToString();
	}

	public static string GetMemoryUsage<T>() where T : UnityEngine.Object
	{
		Resources.UnloadUnusedAssets();
		T[] array = Resources.FindObjectsOfTypeAll<T>();
		Dictionary<T, MemoryObject> dictionary = new Dictionary<T, MemoryObject>();
		T[] array2 = array;
		foreach (T val in array2)
		{
			if (dictionary.ContainsKey(val))
			{
				dictionary[val].Count++;
				continue;
			}
			MemoryObject memoryObject = new MemoryObject();
			memoryObject.Name = val.name;
			memoryObject.Size = (double)Profiler.GetRuntimeMemorySizeLong(val) / 1000000.0;
			memoryObject.Count = 1u;
			dictionary.Add(val, memoryObject);
		}
		List<MemoryObject> list = new List<MemoryObject>(dictionary.Values);
		list.Sort(delegate(MemoryObject x, MemoryObject y)
		{
			int num2 = y.TotalSize.CompareTo(x.TotalSize);
			if (num2 == 0)
			{
				num2 = x.Name.CompareTo(y.Name);
			}
			return num2;
		});
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("####################");
		stringBuilder.AppendLine($"Start {typeof(T).ToString()} Memory Usage");
		stringBuilder.AppendLine("####################");
		double num = 0.0;
		foreach (MemoryObject item in list)
		{
			stringBuilder.AppendLine($"name {item.Name} -singleSize {item.Size} -count {item.Count} -totalSize {item.TotalSize}");
			num += item.TotalSize;
		}
		stringBuilder.AppendLine($"Total {num}");
		stringBuilder.AppendLine("####################");
		stringBuilder.AppendLine($"End {typeof(T).ToString()} Memory Usage");
		stringBuilder.AppendLine("####################");
		return stringBuilder.ToString();
	}

	public static void DumpObjectsCountToFile()
	{
		string memoryDocumentsPath = GetMemoryDocumentsPath();
		string path = string.Format("ObjectCountsDump_{0}.txt", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
		File.WriteAllText(Path.Combine(memoryDocumentsPath, path), DumpObjectCounts());
	}

	private static string DumpObjectCounts()
	{
		int num = 0;
		Dictionary<Type, int> dictionary = new Dictionary<Type, int>();
		UnityEngine.Object.FindObjectsOfType<UnityEngine.Object>();
		UnityEngine.Object[] array = Resources.FindObjectsOfTypeAll(typeof(UnityEngine.Object));
		for (int i = 0; i < array.Length; i++)
		{
			Type type = array[i].GetType();
			if (!dictionary.ContainsKey(type))
			{
				dictionary.Add(type, 0);
			}
			dictionary[type]++;
			num++;
		}
		List<KeyValuePair<Type, int>> list = new List<KeyValuePair<Type, int>>();
		foreach (KeyValuePair<Type, int> item in dictionary)
		{
			list.Add(item);
		}
		list.Sort(delegate(KeyValuePair<Type, int> x, KeyValuePair<Type, int> y)
		{
			int num2 = y.Value.CompareTo(x.Value);
			if (num2 == 0)
			{
				num2 = y.Key.ToString().CompareTo(x.Key.ToString());
			}
			return num2;
		});
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<Type, int> item2 in list)
		{
			stringBuilder.AppendLine($"{item2.Value} {item2.Key.ToString()}");
		}
		stringBuilder.AppendLine($"Total: {num}");
		return stringBuilder.ToString();
	}
}
