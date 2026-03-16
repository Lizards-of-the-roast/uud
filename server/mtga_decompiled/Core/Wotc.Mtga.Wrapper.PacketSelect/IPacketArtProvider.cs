namespace Wotc.Mtga.Wrapper.PacketSelect;

public interface IPacketArtProvider
{
	PacketArt GetPacketArt(AssetTracker assetTracker, string assetTrackingKey, uint artId);
}
