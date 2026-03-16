using System.Diagnostics;

namespace AssetLookupTree.Attributes;

[Conditional("UNITY_EDITOR")]
public class ExtractorDocumentationAttribute : DocumentationAttribute
{
	public ExtractorDocumentationAttribute(string description)
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
		if (Examples.Length != 0)
		{
			value = value + "\n  Examples:\n    •" + string.Join(",\n    •", Examples);
		}
		DocumentationAttribute._packedTextCache.SetCached(GetHashCode(), value);
		return value;
	}
}
