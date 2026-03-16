using Wotc.Mtga.Cards.ArtCrops;
using Wotc.Mtga.Cards.Database;

namespace Wotc.Mtga.Wrapper.PacketSelect;

public class PacketArtProvider : IPacketArtProvider
{
	private const string CROP_TYPE = "Frameless";

	private readonly CardArtTextureLoader _artTextureLoader;

	private readonly IArtCropProvider _cropCropDatabase;

	public PacketArtProvider(CardArtTextureLoader artTextureLoader, IArtCropProvider cropDatabase)
	{
		_artTextureLoader = artTextureLoader;
		_cropCropDatabase = cropDatabase;
	}

	public PacketArt GetPacketArt(AssetTracker assetTracker, string assetTrackingKey, uint artId)
	{
		string artPath = CardArtUtil.GetArtPath(artId.ToString());
		return new PacketArt(_artTextureLoader.AcquireCardArt(assetTracker, assetTrackingKey, artPath), _cropCropDatabase.GetCrop(artPath, "Frameless"));
	}
}
