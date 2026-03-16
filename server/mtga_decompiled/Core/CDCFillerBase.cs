using System.Collections.Generic;
using AssetLookupTree;
using GreClient.CardData;
using GreClient.Rules;
using Pooling;
using UnityEngine;
using Wotc.Mtga.CardParts;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene.Interactions;

public abstract class CDCFillerBase : MonoBehaviour
{
	[SerializeField]
	private int _priority;

	protected ICardDatabaseAdapter _cardDatabase = NullCardDatabaseAdapter.Default;

	protected AssetLookupSystem _assetLookupSystem;

	protected CardMaterialBuilder _cardMaterialBuilder;

	protected CardColorCaches _cardColorCaches;

	protected bool _hasBeenInit;

	protected AssetTracker _assetTracker = new AssetTracker();

	protected IUnityObjectPool _unityObjectPool;

	public int Priority => _priority;

	public abstract int RawFieldType { get; }

	public abstract void UpdateField(ICardDataAdapter model, CardHolderType cardHolderType, HashSet<CDCFillerBase> otherFillers, CDCViewMetadata viewMetadata, MtgGameState gameState, WorkflowBase currentInteraction);

	public abstract void SetDestroyed(bool isDestroyed);

	public virtual void Init(ICardDatabaseAdapter cardDatabase, AssetLookupSystem assetLookupSystem, CardMaterialBuilder cardMaterialBuilder, IUnityObjectPool unityObjectPool, CardColorCaches cardColorCaches)
	{
		_cardDatabase = cardDatabase ?? NullCardDatabaseAdapter.Default;
		_assetLookupSystem = assetLookupSystem;
		_cardMaterialBuilder = cardMaterialBuilder;
		_unityObjectPool = unityObjectPool;
		_cardColorCaches = cardColorCaches;
	}

	public virtual void Cleanup()
	{
		_cardDatabase = null;
		_assetLookupSystem = default(AssetLookupSystem);
		_assetTracker.Cleanup();
	}

	internal virtual void OnDestroy()
	{
		_assetTracker.Cleanup();
	}
}
