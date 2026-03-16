namespace Wizards.Mtga.Assets;

public interface IAssetFileValidationDetector
{
	bool RequiresValidation(string filePath);
}
