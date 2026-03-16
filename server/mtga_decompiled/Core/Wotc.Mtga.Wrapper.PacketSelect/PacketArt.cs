using UnityEngine;
using Wotc.Mtga.Cards.ArtCrops;

namespace Wotc.Mtga.Wrapper.PacketSelect;

public readonly struct PacketArt
{
	public readonly Texture Texture;

	public readonly ArtCrop Crop;

	public PacketArt(Texture texture, ArtCrop crop)
	{
		Texture = texture;
		Crop = crop;
	}
}
