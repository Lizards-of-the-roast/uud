using System.Collections.Generic;

namespace Wotc.Mtga.Cards.ArtCrops;

public interface IArtCropProvider
{
	ArtCrop GetCrop(string artPath, string format);

	ArtCropFormat GetFormat(string cropType);

	IEnumerable<string> GetArtPaths();

	IEnumerable<string> GetFormatNames();
}
