using System;
using System.Diagnostics;

namespace AssetLookupTree.Attributes;

[Conditional("UNITY_EDITOR")]
public class IndirectorDocumentationAttribute : DocumentationAttribute
{
	public string[] IndirectionTargets = Array.Empty<string>();

	public IndirectorDocumentationAttribute(string description)
		: base(description)
	{
	}

	public override string GetPackedText()
	{
		if (DocumentationAttribute._packedTextCache.TryGetCached(GetHashCode(), out var value))
		{
			return value;
		}
		value = Description;
		if (IndirectionTargets.Length != 0)
		{
			value = value + "\n  Indirects To:\n    •" + string.Join(",\n    •", IndirectionTargets);
		}
		if (Examples.Length != 0)
		{
			value = value + "\n  Examples:\n    •" + string.Join(",\n    •", Examples);
		}
		DocumentationAttribute._packedTextCache.SetCached(GetHashCode(), value);
		return value;
	}
}
