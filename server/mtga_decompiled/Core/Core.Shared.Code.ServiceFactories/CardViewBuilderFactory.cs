using AssetLookupTree;
using Core.Code.AssetLookupTree.AssetLookup;
using Pooling;
using Wizards.Mtga;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Loc;

namespace Core.Shared.Code.ServiceFactories;

public class CardViewBuilderFactory
{
	public static CardViewBuilder Create()
	{
		AssetLookupSystem assetLookupSystem = Pantry.Get<AssetLookupManager>().AssetLookupSystem;
		CardMaterialBuilder cardMaterialBuilder = Pantry.Get<CardMaterialBuilder>();
		CardDatabase cdb = Pantry.Get<CardDatabase>();
		IUnityObjectPool unityPool = Pantry.Get<IUnityObjectPool>();
		IObjectPool genericPool = Pantry.Get<IObjectPool>();
		MatchManager matchManager = Pantry.Get<MatchManager>();
		ResourceErrorMessageManager resourceErrorMessageManager = Pantry.Get<ResourceErrorMessageManager>();
		IBILogger biLogger = Pantry.Get<IBILogger>();
		IClientLocProvider localizationManager = Pantry.Get<IClientLocProvider>();
		CardColorCaches cardColorCaches = Pantry.Get<CardColorCaches>();
		return new CardViewBuilder(cdb, unityPool, genericPool, cardMaterialBuilder, assetLookupSystem, localizationManager, matchManager, biLogger, resourceErrorMessageManager, cardColorCaches);
	}
}
