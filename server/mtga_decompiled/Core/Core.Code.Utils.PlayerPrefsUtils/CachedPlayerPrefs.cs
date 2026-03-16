using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Code.Utils.PlayerPrefsUtils;

public static class CachedPlayerPrefs
{
	private static readonly Dictionary<string, int> _ints = new Dictionary<string, int>();

	private static readonly Dictionary<string, float> _floats = new Dictionary<string, float>();

	private static readonly Dictionary<string, string> _strings = new Dictionary<string, string>();

	public static bool HasKey(string key)
	{
		if (!_ints.ContainsKey(key) && !_strings.ContainsKey(key) && !_floats.ContainsKey(key))
		{
			return PlayerPrefs.HasKey(key);
		}
		return true;
	}

	public static void DeleteKey(string key)
	{
		PlayerPrefs.DeleteKey(key);
		if (!_ints.Remove(key) && !_floats.Remove(key))
		{
			_strings.Remove(key);
		}
	}

	public static void Save()
	{
		PlayerPrefs.Save();
	}

	public static string GetString(string key, string defaultValue = "")
	{
		return Get(key, defaultValue, _strings, PlayerPrefs.GetString);
	}

	public static void SetString(string key, string value)
	{
		Set(key, value, _strings, PlayerPrefs.SetString);
	}

	public static int GetInt(string key, int defaultValue = 0)
	{
		return Get(key, defaultValue, _ints, PlayerPrefs.GetInt);
	}

	public static void SetInt(string key, int value)
	{
		Set(key, value, _ints, PlayerPrefs.SetInt);
	}

	public static float GetFloat(string key, float defaultValue = 0f)
	{
		return Get(key, defaultValue, _floats, PlayerPrefs.GetFloat);
	}

	public static void SetFloat(string key, float value)
	{
		Set(key, value, _floats, PlayerPrefs.SetFloat);
	}

	public static List<string> GetStringList(string key, List<string> defaultValue = null, char separator = ',')
	{
		string text = Get(key, null, _strings, PlayerPrefs.GetString);
		if (string.IsNullOrEmpty(text))
		{
			return defaultValue;
		}
		return new List<string>(text.Split(separator));
	}

	public static void SetStringList(string key, List<string> value, string separator = ",")
	{
		string value2 = ((value == null) ? null : string.Join(separator, value));
		Set(key, value2, _strings, PlayerPrefs.SetString);
	}

	private static T Get<T>(string key, T defaultVal, Dictionary<string, T> cache, Func<string, T, T> playerPrefsGetter)
	{
		if (!cache.ContainsKey(key))
		{
			cache[key] = playerPrefsGetter(key, defaultVal);
		}
		return cache[key];
	}

	private static void Set<T>(string key, T value, Dictionary<string, T> cache, Action<string, T> playerPrefsSetter) where T : IEquatable<T>
	{
		if (cache.ContainsKey(key))
		{
			T val = cache[key];
			if ((val == null && value == null) || (val != null && val.Equals(value)))
			{
				return;
			}
		}
		cache[key] = value;
		playerPrefsSetter(key, value);
	}
}
