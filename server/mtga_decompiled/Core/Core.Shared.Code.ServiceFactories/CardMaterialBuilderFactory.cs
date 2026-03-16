using AssetLookupTree;
using Core.Code.AssetLookupTree.AssetLookup;
using Wizards.Mtga;
using Wotc.Mtga.Cards.ArtCrops;

namespace Core.Shared.Code.ServiceFactories;

public class CardMaterialBuilderFactory
{
	public static CardMaterialBuilder Create()
	{
		IBILogger biLogger = Pantry.Get<IBILogger>();
		CardArtTextureLoader artTextureLoader = new CardArtTextureLoader();
		IArtCropProvider cardArtCropDatabase = ArtCropDatabaseUtils.LoadBestProvider(biLogger);
		AssetLookupSystem assetLookupSystem = Pantry.Get<AssetLookupManager>().AssetLookupSystem;
		CardColorCaches cardColorCaches = Pantry.Get<CardColorCaches>();
		return new CardMaterialBuilder(assetLookupSystem, artTextureLoader, cardArtCropDatabase, cardColorCaches);
	}
}
