namespace Wizards.Mtga.Assets;

public interface IAssetFileSignature
{
	long Length { get; }

	byte[] Sha256Hash { get; }

	uint? Crc32 { get; }
}
