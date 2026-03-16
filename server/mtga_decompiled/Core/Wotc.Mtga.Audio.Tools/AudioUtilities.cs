using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Wotc.Mtga.Audio.Tools;

public static class AudioUtilities
{
	private static readonly SortedDictionary<string, WwiseEventDefinition> _wwiseEventDefinitions = new SortedDictionary<string, WwiseEventDefinition>();

	public static IReadOnlyDictionary<string, WwiseEventDefinition> GetAllWwiseEvents()
	{
		if (_wwiseEventDefinitions.Count == 0)
		{
			try
			{
				LoadWwiseEventsFromJson();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		return _wwiseEventDefinitions;
	}

	public static bool EventExists(string eventName)
	{
		return GetAllWwiseEvents().ContainsKey(eventName);
	}

	private static void LoadWwiseEventsFromJson()
	{
		FileInfo fileInfo = new FileInfo(Path.Combine("BuildDataSources", "Audio", "WwiseMapping.json"));
		if (!fileInfo.Exists)
		{
			return;
		}
		using TextReader reader = FileSystemUtils.OpenText(fileInfo.FullName);
		using JsonTextReader jsonTextReader = new JsonTextReader(reader);
		jsonTextReader.Read();
		jsonTextReader.Read();
		jsonTextReader.Read();
		while (jsonTextReader.TokenType != JsonToken.EndArray)
		{
			jsonTextReader.Read();
			string pckName = jsonTextReader.ReadAsString();
			jsonTextReader.Read();
			jsonTextReader.Read();
			jsonTextReader.Read();
			while (jsonTextReader.TokenType != JsonToken.EndArray)
			{
				jsonTextReader.Read();
				string text = jsonTextReader.ReadAsString();
				jsonTextReader.Read();
				string guid = jsonTextReader.ReadAsString();
				jsonTextReader.Read();
				if (!string.IsNullOrEmpty(text) && !_wwiseEventDefinitions.ContainsKey(text))
				{
					_wwiseEventDefinitions.Add(text, new WwiseEventDefinition(guid, text, pckName));
				}
				jsonTextReader.Read();
			}
			jsonTextReader.Read();
			jsonTextReader.Read();
		}
		jsonTextReader.Read();
	}
}
