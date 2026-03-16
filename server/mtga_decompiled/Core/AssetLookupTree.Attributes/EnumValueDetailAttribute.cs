using System;

namespace AssetLookupTree.Attributes;

public class EnumValueDetailAttribute : ValueDetailAttribute
{
	public readonly Type EnumType;

	public EnumValueDetailAttribute(Type enumType)
	{
		if (enumType == null)
		{
			throw new ArgumentNullException("enumType");
		}
		if (!typeof(Enum).IsAssignableFrom(enumType))
		{
			throw new ArgumentException("type " + enumType.Name + " is not an Enum");
		}
		if (!enumType.IsEnum)
		{
			throw new ArgumentException("type " + enumType.Name + " is not an Enum");
		}
		EnumType = enumType;
	}
}
