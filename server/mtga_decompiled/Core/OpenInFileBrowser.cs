using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public static class OpenInFileBrowser
{
	private static void OpenInOSX(string path, bool openInsideFolder)
	{
		string text = path.Replace("\\", "/");
		if (!text.StartsWith("\""))
		{
			text = "\"" + text;
		}
		if (!text.EndsWith("\""))
		{
			text += "\"";
		}
		string arguments = (openInsideFolder ? "" : "-R ") + text;
		try
		{
			Process.Start("open", arguments);
		}
		catch (Win32Exception ex)
		{
			ex.HelpLink = "";
		}
	}

	private static void OpenInWindows(string path, bool openInsideFolder)
	{
		string text = path.Replace("/", "\\");
		try
		{
			Process.Start("explorer.exe", (openInsideFolder ? "/root," : "/select,") + text);
		}
		catch (Win32Exception ex)
		{
			ex.HelpLink = "";
		}
	}

	public static void Open(string path)
	{
		if (!File.Exists(path))
		{
			path = Path.GetDirectoryName(path);
		}
		bool openInsideFolder = Directory.Exists(path);
		if (Application.platform == RuntimePlatform.WindowsEditor)
		{
			OpenInWindows(path, openInsideFolder);
		}
		else if (Application.platform == RuntimePlatform.OSXEditor)
		{
			OpenInOSX(path, openInsideFolder);
		}
	}
}
