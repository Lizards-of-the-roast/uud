namespace AssetLookupTree.Attributes;

public class StringLengthValidationAttribute : ValueValidationAttribute
{
	public readonly uint Min;

	public readonly uint Max;

	public StringLengthValidationAttribute(uint min, uint max)
	{
		Min = min;
		Max = max;
	}
}
