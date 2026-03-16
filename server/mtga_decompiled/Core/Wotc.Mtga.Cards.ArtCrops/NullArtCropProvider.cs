using System;
using System.Collections.Generic;

namespace Wotc.Mtga.Cards.ArtCrops;

public sealed class NullArtCropProvider : IArtCropProvider
{
	public static readonly IArtCropProvider Default = new NullArtCropProvider();

	public ArtCrop GetCrop(string artPath, string format)
	{
		return null;
	}

	public ArtCropFormat GetFormat(string cropType)
	{
		return null;
	}

	public IEnumerable<string> GetArtPaths()
	{
		return Array.Empty<string>();
	}

	public IEnumerable<string> GetFormatNames()
	{
		return Array.Empty<string>();
	}
}
