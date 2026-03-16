namespace Wizards.Mtga.Assets;

public class AssetFileValidationDetector : IAssetFileValidationDetector
{
	private readonly bool _validateFiles;

	public AssetFileValidationDetector(bool validateFiles)
	{
		_validateFiles = validateFiles;
	}

	public bool RequiresValidation(string filePath)
	{
		return _validateFiles;
	}
}
