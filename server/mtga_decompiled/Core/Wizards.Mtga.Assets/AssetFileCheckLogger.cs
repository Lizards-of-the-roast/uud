using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using Wotc.Mtga;

namespace Wizards.Mtga.Assets;

public static class AssetFileCheckLogger
{
	public static void LogResultsToFile(IEnumerable<string> manifestHashes, IDictionary<string, AssetFileInfo> expectedAssetsByName, IDictionary<string, FileInfo> existingAssetsByName, IDictionary<string, FileInfo> assetsToDeleteByName, IDictionary<string, FileInfo> corruptAssetsByName)
	{
		StringBuilder stringBuilder = new StringBuilder();
		AppendLogHeading(stringBuilder, "Source Manifest Hashes");
		foreach (string manifestHash in manifestHashes)
		{
			stringBuilder.AppendLine(manifestHash);
		}
		AppendLogHeading(stringBuilder, "Files To Download");
		foreach (KeyValuePair<string, AssetFileInfo> item in expectedAssetsByName)
		{
			stringBuilder.AppendLine($"{item.Key} - {item.Value.Priority}");
		}
		AppendLogHeading(stringBuilder, "Files To Skip");
		foreach (KeyValuePair<string, FileInfo> item2 in existingAssetsByName)
		{
			stringBuilder.AppendLine(item2.Key);
		}
		AppendLogHeading(stringBuilder, "Files To Remove");
		foreach (KeyValuePair<string, FileInfo> item3 in assetsToDeleteByName)
		{
			stringBuilder.AppendLine(item3.Key);
		}
		AppendLogHeading(stringBuilder, "Corrupted Files");
		foreach (KeyValuePair<string, FileInfo> item4 in corruptAssetsByName)
		{
			stringBuilder.AppendLine(item4.Key);
		}
		GenerateLogFile(stringBuilder);
	}

	private static void AppendLogHeading(StringBuilder logBuilder, string heading)
	{
		logBuilder.AppendLine();
		logBuilder.AppendLine("----------");
		logBuilder.AppendLine(heading);
		logBuilder.AppendLine("----------");
		logBuilder.AppendLine();
	}

	public static void GenerateTimingsLog(string label, TimeSpan network, long downloadBytes, int maxParallelism)
	{
		double num = (double)downloadBytes / 1048576.0;
		double num2 = num / network.TotalSeconds;
		Debug.Log($"Downloaded {num:N2}m in {network.TotalSeconds:N2}s ({num2:N2}m/s) across {maxParallelism} tasks");
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine($"Downloaded mb: {num:N3}");
		stringBuilder.AppendLine($"Total download time (s): {network.TotalSeconds:N3}");
		stringBuilder.AppendLine($"Total download speed (mb/s): {num2:N3}");
		stringBuilder.AppendLine($"Max Parallelism: {maxParallelism}");
		using StreamWriter streamWriter = FileSystemUtils.CreateText(DownloadLogPath(label + "_timings"));
		streamWriter.Write(stringBuilder.ToString());
	}

	private static void GenerateLogFile(StringBuilder logBuilder)
	{
		using StreamWriter streamWriter = FileSystemUtils.CreateText(DownloadLogPath("downloads"));
		streamWriter.Write(logBuilder.ToString());
	}

	private static string DownloadLogPath(string prefix)
	{
		string text = Path.Combine(Utilities.GetLogPath(), "DownloadLogs");
		Directory.CreateDirectory(text);
		return Path.Combine(text, prefix + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".log");
	}
}
