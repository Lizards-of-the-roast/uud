using System;
using System.IO;
using System.Text;
using UnityEngine;

public static class PersistentStorage
{
	public static bool Save(string fileName, string data)
	{
		try
		{
			File.WriteAllText(Path.Combine(Application.persistentDataPath, fileName), data, Encoding.UTF8);
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogError($"Error saving file {fileName}: {ex.ToString()}");
			return false;
		}
	}

	public static string Load(string fileName)
	{
		try
		{
			return File.ReadAllText(Path.Combine(Application.persistentDataPath, fileName), Encoding.UTF8);
		}
		catch (Exception ex)
		{
			Debug.LogWarning($"Error loading file {fileName}: {ex.ToString()}");
			return null;
		}
	}

	public static void Delete(string fileName)
	{
		try
		{
			File.Delete(Path.Combine(Application.persistentDataPath, fileName));
		}
		catch (Exception)
		{
		}
	}
}
