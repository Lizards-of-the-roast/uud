using System;
using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Payloads.Cosmetic;
using GreClient.CardData;
using GreClient.Rules;
using Pooling;
using UnityEngine;
using Wizards.Mtga.Inventory;
using Wotc.Mtga.DuelScene.Interactions;

namespace Wotc.Mtga.DuelScene.Universal;

public class UniversalBattlefieldPet : IUniversalBattlefieldGroup
{
	private Func<GREPlayerNum, MatchManager.PlayerInfo> _playerInfoProvider;

	private AssetLookupSystem _assetLookupSystem;

	private Vector2? _cachedDimensions;

	public Vector3 Position { get; private set; }

	public Vector2 Dimensions { get; private set; }

	public IEnumerable<UniversalBattlefieldStack> VisibleStacks => Array.Empty<UniversalBattlefieldStack>();

	public IEnumerable<UniversalBattlefieldStack> AllStacks => Array.Empty<UniversalBattlefieldStack>();

	public IEnumerable<CardLayoutData> CardLayoutDatas => Array.Empty<CardLayoutData>();

	public UniversalBattlefieldGroup.Configuration Config { get; private set; }

	public bool IgnoreRegionSpacing => true;

	public UniversalBattlefieldPet(UniversalBattlefieldGroup.Configuration config, Func<GREPlayerNum, MatchManager.PlayerInfo> playerInfoProvider, AssetLookupSystem assetLookupSystem)
	{
		Config = config;
		_playerInfoProvider = playerInfoProvider;
		_assetLookupSystem = assetLookupSystem;
	}

	public void GenerateLayoutDatas(MtgGameState gameState, WorkflowBase workflow, IReadOnlyCollection<CardLayoutData> solvedLayoutDatas, bool collapse, uint expandedStackParentId)
	{
		if (!_cachedDimensions.HasValue)
		{
			_cachedDimensions = Vector2.zero;
			ClientPetSelection clientPetSelection = _playerInfoProvider?.Invoke(Config.RegionController)?.PetSelection;
			if (clientPetSelection != null && !string.IsNullOrEmpty(clientPetSelection.name))
			{
				_cachedDimensions = Config.Scale;
				_assetLookupSystem.Blackboard.Clear();
				_assetLookupSystem.Blackboard.PetId = clientPetSelection.name;
				_assetLookupSystem.Blackboard.PetVariantId = clientPetSelection.variant;
				_assetLookupSystem.Blackboard.GREPlayerNum = Config.RegionController;
				if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<PetOffset> loadedTree))
				{
					PetOffset payload = loadedTree.GetPayload(_assetLookupSystem.Blackboard);
					if (payload != null && payload.OverrideMobileBattlefieldDimensions)
					{
						_cachedDimensions = payload.MobileBattlefieldDimensions;
					}
				}
			}
		}
		Dimensions = _cachedDimensions.Value;
		Position = CdcVector3.right * Dimensions.x / 2f;
	}

	public void ApplyPositionalDelta(Vector3 posDelta)
	{
		Position += posDelta;
	}

	public bool TryAddCard(DuelScene_CDC cardView, GameManager gameManager, bool isFocusPlayer)
	{
		return false;
	}

	public bool TryAddCard(ICardDataAdapter cardModel, GameManager gameManager, bool isFocusPlayer)
	{
		return false;
	}

	public void Clear(IObjectPool pool)
	{
	}
}
