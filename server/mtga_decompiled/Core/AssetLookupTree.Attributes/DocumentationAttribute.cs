using System;
using System.Diagnostics;
using Wotc.Mtga.Loc.CachingPatterns;

namespace AssetLookupTree.Attributes;

[Conditional("UNITY_EDITOR")]
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public abstract class DocumentationAttribute : Attribute
{
	public string Description;

	public string[] Examples = Array.Empty<string>();

	protected static readonly ICachingPattern<int, string> _packedTextCache = new DictionaryCache<int, string>();

	protected DocumentationAttribute(string description)
	{
		Description = description;
	}

	public abstract string GetPackedText();
}
