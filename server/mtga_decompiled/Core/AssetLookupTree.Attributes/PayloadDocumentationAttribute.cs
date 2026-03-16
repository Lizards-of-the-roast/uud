using System;
using System.Diagnostics;

namespace AssetLookupTree.Attributes;

[Conditional("UNITY_EDITOR")]
public class PayloadDocumentationAttribute : DocumentationAttribute
{
	public string[] PrimaryBlackboardContent = Array.Empty<string>();

	public PayloadDocumentationAttribute(string description)
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
		if (PrimaryBlackboardContent.Length != 0)
		{
			value = value + "\n  Blackboard Content:\n    •" + string.Join(",\n    •", PrimaryBlackboardContent);
		}
		DocumentationAttribute._packedTextCache.SetCached(GetHashCode(), value);
		return value;
	}
}
