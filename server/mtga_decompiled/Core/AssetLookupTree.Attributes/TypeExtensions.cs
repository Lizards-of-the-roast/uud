using System;

namespace AssetLookupTree.Attributes;

public static class TypeExtensions
{
	private const string NO_DOCUMENTATION = "No Documentation Found";

	public static string GetDocText(this Type type)
	{
		if (type == null)
		{
			return string.Empty;
		}
		object[] customAttributes = type.GetCustomAttributes(inherit: true);
		for (int i = 0; i < customAttributes.Length; i++)
		{
			if (customAttributes[i] is DocumentationAttribute documentationAttribute)
			{
				return documentationAttribute.GetPackedText();
			}
		}
		return "No Documentation Found";
	}

	public static bool TryGetDocText(this Type type, out string docText)
	{
		if (type == null)
		{
			docText = string.Empty;
			return false;
		}
		object[] customAttributes = type.GetCustomAttributes(inherit: true);
		for (int i = 0; i < customAttributes.Length; i++)
		{
			if (customAttributes[i] is DocumentationAttribute documentationAttribute)
			{
				docText = documentationAttribute.GetPackedText();
				return true;
			}
		}
		docText = "No Documentation Found";
		return false;
	}

	public static T GetDocAttribute<T>(this Type type) where T : DocumentationAttribute
	{
		if (type == null)
		{
			return null;
		}
		object[] customAttributes = type.GetCustomAttributes(inherit: true);
		for (int i = 0; i < customAttributes.Length; i++)
		{
			if (customAttributes[i] is T result)
			{
				return result;
			}
		}
		return null;
	}
}
