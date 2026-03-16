using AssetLookupTree;
using AssetLookupTree.Payloads.Cosmetic;
using UnityEngine;
using Wizards.Mtga.Inventory;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Providers;

namespace Wotc.Mtga.DuelScene.Companions;

public class CompanionBuilder : ICompanionBuilder
{
	private readonly AssetLookupSystem _assetLookupSystem;

	private readonly GameManager _gameManager;

	private readonly CosmeticsProvider _cosmeticsProvider;

	private Transform _localRoot;

	private Transform _opponentRoot;

	public CompanionBuilder(AssetLookupSystem assetLookupSystem, CosmeticsProvider cosmeticsProvider, GameManager gameManager)
	{
		_assetLookupSystem = assetLookupSystem;
		_cosmeticsProvider = cosmeticsProvider;
		_gameManager = gameManager;
		Transform transform = new GameObject("Companions").transform;
		transform.ZeroOut();
		_localRoot = new GameObject("LocalTotemRoot").transform;
		_localRoot.parent = transform;
		_localRoot.ZeroOut();
		_localRoot.localPosition = new Vector3(24f, 0f, -1f);
		_opponentRoot = new GameObject("OpponentTotemRoot").transform;
		_opponentRoot.parent = transform;
		_opponentRoot.ZeroOut();
		_opponentRoot.localPosition = new Vector3(-24f, 0f, -1f);
		_opponentRoot.localEulerAngles = new Vector3(0f, 180f, 0f);
	}

	public AccessoryController Create(CompanionData companionData)
	{
		AccessoryController accessoryController = null;
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.PetId = companionData.Id;
		_assetLookupSystem.Blackboard.PetVariantId = companionData.Variant;
		_assetLookupSystem.Blackboard.GREPlayerNum = companionData.OwnerType;
		Transform transform = ((companionData.OwnerType == GREPlayerNum.Opponent) ? _opponentRoot : _localRoot);
		if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<PetPayload> loadedTree))
		{
			PetPayload payload = loadedTree.GetPayload(_assetLookupSystem.Blackboard);
			if (payload != null)
			{
				GameObject gameObject = AssetLoader.Instantiate(payload.BattlefieldPrefab.RelativePath, transform);
				gameObject.transform.SetParent(transform);
				gameObject.transform.ZeroOut();
				accessoryController = gameObject.GetComponent<AccessoryController>();
				if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<PetOffset> loadedTree2))
				{
					PetOffset payload2 = loadedTree2.GetPayload(_assetLookupSystem.Blackboard);
					if (payload2 != null)
					{
						transform.localPosition += payload2.Offset.PositionOffset;
						gameObject.transform.localEulerAngles = payload2.Offset.RotationOffset;
						gameObject.transform.localScale = payload2.Offset.ScaleMultiplier;
					}
				}
				ClientPetSelection petSelection = new ClientPetSelection
				{
					name = companionData.Id,
					variant = companionData.Variant
				};
				accessoryController.Init(_gameManager, companionData.OwnerType, _cosmeticsProvider, petSelection);
			}
		}
		_assetLookupSystem.Blackboard.Clear();
		return accessoryController;
	}
}
