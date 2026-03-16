using System;
using System.Collections.Generic;
using System.IO;
using GreClient.CardData;
using UnityEngine;
using Wotc.Mtga.Cards.Database;

namespace AssetLookupTree.Payloads.Card;

public class ArtIdOverride : IPayload
{
	public enum StringFormatParameter
	{
		ArtId,
		GrpId,
		SetCode
	}

	[Serializable]
	public class ArtReplacement
	{
		public string Property;

		public string FileName;

		public string Keyword;

		public string Trigger;

		[HideInInspector]
		public string ArtPath;

		public ArtReplacement(string property)
		{
			Property = property;
		}

		public static string[] GetFileNames(List<ArtReplacement> artReplacements)
		{
			string[] array = new string[artReplacements.Count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = artReplacements[i].FileName;
			}
			return array;
		}
	}

	private const string ROOT_ART_PATH = "Assets/Core";

	public string ArtPath = string.Empty;

	public string ArtistCredit = string.Empty;

	public readonly List<ArtReplacement> ArtReplacements = new List<ArtReplacement>();

	public List<StringFormatParameter> StringFormatParameters = new List<StringFormatParameter>();

	public List<ArtReplacement> GetArtReplacements(CardPrintingData printingData = null)
	{
		string text = "Assets/Core/" + ArtPath + "/";
		for (int i = 0; i < ArtReplacements.Count; i++)
		{
			string text2 = ArtReplacements[i].FileName;
			if (!string.IsNullOrEmpty(text2))
			{
				if (printingData != null)
				{
					text2 = string.Format(text2, GetStringFormatArguments(printingData));
				}
				string[] array = text2.Split('_');
				if (array != null && array.Length >= 1 && uint.TryParse(array[0], out var result))
				{
					text2 = result.ToString().PadLeft(6, '0').Substring(0, 3)
						.PadRight(6, '0') + "/" + text2;
				}
				ArtReplacements[i].ArtPath = text + text2;
			}
		}
		return ArtReplacements;
	}

	private object[] GetStringFormatArguments(CardPrintingData printingData)
	{
		List<object> list = new List<object>();
		using (List<StringFormatParameter>.Enumerator enumerator = StringFormatParameters.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				switch (enumerator.Current)
				{
				case StringFormatParameter.ArtId:
					list.Add(printingData.ArtId);
					break;
				case StringFormatParameter.GrpId:
					list.Add(printingData.GrpId);
					break;
				case StringFormatParameter.SetCode:
					list.Add(printingData.ExpansionCode);
					break;
				default:
					list.Add(printingData.ArtId);
					break;
				}
			}
		}
		return list.ToArray();
	}

	public IEnumerable<string> GetFilePaths()
	{
		foreach (ArtReplacement artReplacement in GetArtReplacements())
		{
			if (artReplacement.FileName.Contains("{0}"))
			{
				continue;
			}
			foreach (string item in CardArtUtil.ArtFileTypesInSearchOrder)
			{
				string text = artReplacement.ArtPath + item;
				if (File.Exists(text))
				{
					yield return text;
				}
			}
		}
	}
}
