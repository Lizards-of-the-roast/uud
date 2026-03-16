using System;
using System.IO;
using AssetLookupTree;
using AssetLookupTree.Payloads.CardHolder;
using AssetLookupTree.Payloads.Prefab;
using MovementSystem;
using UnityEngine;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga.DuelScene;

public class CardHolderBuilder : ICardHolderBuilder
{
	private readonly AssetLookupSystem _assetLookupSystem;

	private readonly GameManager _gameManager;

	private readonly IEntityViewManager _viewManager;

	private readonly ISplineMovementSystem _splineMovementSystem;

	private readonly CardViewBuilder _cardViewBuilder;

	private readonly IClientLocProvider _locProvider;

	private readonly MatchManager _matchManager;

	private readonly ISignalDispatch<CardHolderCreatedSignalArgs> _cardHolderCreatedEvent;

	private readonly ISignalDispatch<CardHolderDeletedSignalArgs> _cardHolderDeletedEvent;

	private Transform _cardHolderRoot;

	public CardHolderBuilder(GameManager gameManager, AssetLookupSystem assetLookupSystem, IEntityViewManager viewManager, ISplineMovementSystem splineMovementSystem, CardViewBuilder cardViewBuilder, IClientLocProvider locProvider, MatchManager matchManager, ISignalDispatch<CardHolderCreatedSignalArgs> cardHolderCreatedEvent, ISignalDispatch<CardHolderDeletedSignalArgs> cardHolderDeletedEvent)
	{
		_gameManager = gameManager;
		_assetLookupSystem = assetLookupSystem;
		_viewManager = viewManager;
		_splineMovementSystem = splineMovementSystem;
		_cardViewBuilder = cardViewBuilder;
		_locProvider = locProvider;
		_matchManager = matchManager;
		_cardHolderCreatedEvent = cardHolderCreatedEvent;
		_cardHolderDeletedEvent = cardHolderDeletedEvent;
	}

	public ICardHolder CreateCardHolder(CardHolderType cardHolderType, GREPlayerNum owner)
	{
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.CardHolderType = cardHolderType;
		_assetLookupSystem.Blackboard.GREPlayerNum = owner;
		ICardHolder cardHolder = CreateCardHolderInternal();
		_assetLookupSystem.Blackboard.Clear();
		if (cardHolder is CardHolderBase cardHolderBase)
		{
			cardHolderBase.Init(_gameManager, _viewManager, _splineMovementSystem, _cardViewBuilder, _locProvider, _matchManager);
		}
		_cardHolderCreatedEvent.Dispatch(new CardHolderCreatedSignalArgs(this, cardHolder));
		return cardHolder;
	}

	public bool DestroyCardHolder(ICardHolder cardHolder)
	{
		_cardHolderDeletedEvent.Dispatch(new CardHolderDeletedSignalArgs(this, cardHolder));
		if (cardHolder is IDisposable disposable)
		{
			disposable.Dispose();
		}
		if (cardHolder is CardHolderBase cardHolderBase && (bool)cardHolderBase.gameObject)
		{
			UnityEngine.Object.Destroy(cardHolderBase.gameObject);
		}
		return true;
	}

	private ICardHolder CreateCardHolderInternal()
	{
		string prefabPath = GetPrefabPath();
		if (string.IsNullOrEmpty(prefabPath))
		{
			return null;
		}
		if ((object)_cardHolderRoot == null)
		{
			_cardHolderRoot = new GameObject("CardHolders").transform;
		}
		CardHolderBase cardHolderBase = AssetLoader.Instantiate<CardHolderBase>(prefabPath, _cardHolderRoot);
		cardHolderBase.name = Path.GetFileNameWithoutExtension(prefabPath);
		CardHolder_Transform cardholderTransform = GetCardholderTransform();
		if (cardholderTransform == null)
		{
			return cardHolderBase;
		}
		Transform transform = cardHolderBase.transform;
		transform.position = cardholderTransform.OffsetData.PositionOffset;
		if (cardholderTransform.OffsetData.RotationIsWorld)
		{
			transform.eulerAngles = cardholderTransform.OffsetData.RotationOffset;
		}
		else
		{
			transform.localEulerAngles = cardholderTransform.OffsetData.RotationOffset;
		}
		if (cardholderTransform.OffsetData.ScaleIsWorld)
		{
			Vector3 lossyScale = transform.lossyScale;
			transform.localScale = new Vector3(cardholderTransform.OffsetData.ScaleMultiplier.x / lossyScale.x, cardholderTransform.OffsetData.ScaleMultiplier.y / lossyScale.y, cardholderTransform.OffsetData.ScaleMultiplier.z / lossyScale.z);
		}
		else
		{
			transform.localScale = cardholderTransform.OffsetData.ScaleMultiplier;
		}
		return cardHolderBase;
	}

	private string GetPrefabPath()
	{
		if (!_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<CardHolderBasePrefab> loadedTree))
		{
			return string.Empty;
		}
		CardHolderBasePrefab payload = loadedTree.GetPayload(_assetLookupSystem.Blackboard);
		if (payload == null)
		{
			return string.Empty;
		}
		return payload.PrefabPath;
	}

	private CardHolder_Transform GetCardholderTransform()
	{
		if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<CardHolder_Transform> loadedTree))
		{
			CardHolder_Transform payload = loadedTree.GetPayload(_assetLookupSystem.Blackboard);
			if (payload != null)
			{
				return payload;
			}
		}
		return null;
	}
}
