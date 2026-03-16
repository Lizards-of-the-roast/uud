using Core.Code.Utils.PlayerPrefsUtils;

public static class PlayerPrefsExt
{
	public static bool GetBool(string key)
	{
		return CachedPlayerPrefs.GetInt(key) == 1;
	}

	public static bool GetBool(string key, bool defaultValue)
	{
		return CachedPlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;
	}

	public static void SetBool(string key, bool value, bool save = false)
	{
		CachedPlayerPrefs.SetInt(key, value ? 1 : 0);
		if (save)
		{
			CachedPlayerPrefs.Save();
		}
	}

	public static float GetFloat(string key)
	{
		return CachedPlayerPrefs.GetFloat(key);
	}

	public static void SetFloat(string key, float value)
	{
		CachedPlayerPrefs.SetFloat(key, value);
	}

	public static int GetInt(string key, int defaultValue = 0)
	{
		return CachedPlayerPrefs.GetInt(key, defaultValue);
	}

	public static void SetInt(string key, int value, bool save = false)
	{
		CachedPlayerPrefs.SetInt(key, value);
		if (save)
		{
			CachedPlayerPrefs.Save();
		}
	}

	public static string GetString(string key, string defaultValue = "")
	{
		return CachedPlayerPrefs.GetString(key, defaultValue);
	}

	public static void SetString(string key, string value)
	{
		CachedPlayerPrefs.SetString(key, value);
	}

	public static void DeleteKey(string key)
	{
		CachedPlayerPrefs.DeleteKey(key);
	}

	public static bool HasKey(string key)
	{
		return CachedPlayerPrefs.HasKey(key);
	}

	public static void Save()
	{
		CachedPlayerPrefs.Save();
	}
}
