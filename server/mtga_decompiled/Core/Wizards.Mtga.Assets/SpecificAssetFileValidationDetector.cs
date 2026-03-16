namespace Wizards.Mtga.Assets;

public class SpecificAssetFileValidationDetector : IAssetFileValidationDetector
{
	private readonly string _assetToValidate;

	public SpecificAssetFileValidationDetector(string assetToValidate)
	{
		_assetToValidate = assetToValidate;
	}

	public bool RequiresValidation(string filePath)
	{
		return _assetToValidate == filePath;
	}
}
