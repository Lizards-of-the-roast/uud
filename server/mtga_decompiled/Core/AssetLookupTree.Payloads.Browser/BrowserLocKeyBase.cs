using System;
using System.Collections.Generic;
using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Payloads.Browser;

public abstract class BrowserLocKeyBase : IPayload
{
	public enum ParamExtractorType
	{
		Invalid,
		Min,
		Max
	}

	public string LocKey;

	public List<(string, ParamExtractorType)> LocParamExtractors = new List<(string, ParamExtractorType)>();

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}

	public (string, string)[] GetLocParams(IBlackboard bb)
	{
		if (LocParamExtractors.Count == 0)
		{
			return Array.Empty<(string, string)>();
		}
		if (bb == null)
		{
			return Array.Empty<(string, string)>();
		}
		List<(string, string)> list = new List<(string, string)>(LocParamExtractors.Count);
		foreach (var locParamExtractor in LocParamExtractors)
		{
			string item = locParamExtractor.Item1;
			ParamExtractorType item2 = locParamExtractor.Item2;
			string text = null;
			switch (item2)
			{
			case ParamExtractorType.Min:
				text = bb.SelectCardBrowserMinMax?.min.ToString();
				break;
			case ParamExtractorType.Max:
				text = bb.SelectCardBrowserMinMax?.max.ToString();
				break;
			}
			if (!string.IsNullOrWhiteSpace(item) && !string.IsNullOrWhiteSpace(text))
			{
				list.Add((item, text));
			}
		}
		return list.ToArray();
	}
}
