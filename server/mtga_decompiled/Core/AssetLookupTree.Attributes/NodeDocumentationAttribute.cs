namespace AssetLookupTree.Attributes;

public class NodeDocumentationAttribute : DocumentationAttribute
{
	public NodeDocumentationAttribute(string description)
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
		DocumentationAttribute._packedTextCache.SetCached(GetHashCode(), value);
		return value;
	}
}
