using UnityEngine;

namespace Wotc.Mtga.Wrapper.PacketSelect;

public class NullPacketArtProvider : IPacketArtProvider
{
	public PacketArt GetPacketArt(AssetTracker assetTracker, string assetTrackingKey, uint artId)
	{
		return new PacketArt(new Texture2D(1, 1), null);
	}
}
