using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Blackboard;
using AssetLookupTree.Payloads.Card;
using GreClient.CardData;
using Pooling;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga.Hangers;

public class ParameterizedCardHangerProvider : IHangerConfigProvider
{
	private readonly AssetLookupSystem _assetLookupSystem;

	private readonly IClientLocProvider _clientLocProvider;

	private readonly IObjectPool _genericPool;

	public ParameterizedCardHangerProvider(AssetLookupSystem assetLookupSystem, IClientLocProvider clientLocProvider, IObjectPool genericPool)
	{
		_assetLookupSystem = assetLookupSystem;
		_clientLocProvider = clientLocProvider;
		_genericPool = genericPool;
	}

	public IEnumerable<HangerConfig> GetHangerConfigs(ICardDataAdapter model)
	{
		if (!_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<ParameterizedInfoHanger> loadedTree))
		{
			yield break;
		}
		HashSet<ParameterizedInfoHanger> hangerEntries = _genericPool.PopObject<HashSet<ParameterizedInfoHanger>>();
		IBlackboard bb = _assetLookupSystem.Blackboard;
		bb.Clear();
		bb.SetCardDataExtensive(model);
		if (loadedTree.GetPayloadLayered(_assetLookupSystem.Blackboard, hangerEntries))
		{
			foreach (ParameterizedInfoHanger item in hangerEntries)
			{
				yield return GenerateHanger(bb, item, _clientLocProvider);
			}
		}
		hangerEntries.Clear();
		_genericPool.PushObject(hangerEntries);
	}

	public static HangerConfig GenerateHanger(IBlackboard filledBB, ParameterizedInfoHanger hangerEntry, IClientLocProvider clientLocProvider)
	{
		(string, string)[] locParams = hangerEntry.BuildParameters(filledBB);
		string localizedText = clientLocProvider.GetLocalizedText(hangerEntry.HeaderLocKey, locParams);
		string localizedText2 = clientLocProvider.GetLocalizedText(hangerEntry.BodyLocKey, locParams);
		string spritePath = hangerEntry.SpriteRef?.RelativePath ?? null;
		return new HangerConfig(localizedText, localizedText2, null, spritePath);
	}
}
