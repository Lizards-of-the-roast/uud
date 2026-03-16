using System;
using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Payloads.Cosmetic;
using AssetLookupTree.Payloads.Prefab;
using Core.Meta.MainNavigation.Profile;
using Pooling;
using UnityEngine;
using Wizards.Arena.Models.Network;
using Wotc.Mtga.Client.Models.Catalog;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Providers;

namespace Core.Meta.MainNavigation.Cosmetics;

public class DisplayItemPet : DisplayItemCosmeticBase, IDisplayItemCosmetic<PetEntry>
{
	[SerializeField]
	private Transform _petAnchor;

	[SerializeField]
	private bool _showSelectedPet = true;

	private IReadOnlyCollection<CosmeticPetEntry> _ownedPets;

	private PetEntry _selectedPet;

	private Action<PetEntry> _onCosmeticSelected;

	private Action _onDefaultSelected;

	private PetPopUpV2 _selector;

	private AssetLookupSystem _assetLookupSystem;

	private IUnityObjectPool _objectPool;

	private PetCatalog _petCatalog;

	private IClientLocProvider _clientLocProvider;

	private GameObject petInstance;

	private static readonly int InWrapper = Animator.StringToHash("InWrapper");

	private bool _showDefaultInterface;

	public void Init(Transform selectorTransform, CosmeticsProvider cosmeticsProvider, AssetLookupSystem assetLookupSystem, PetCatalog petCatalog, IUnityObjectPool objectPool, IClientLocProvider clientLocProvider, bool isSideboarding)
	{
		string prefabPathFromALT = GetPrefabPathFromALT(assetLookupSystem);
		_selector = AssetLoader.Instantiate<PetPopUpV2>(prefabPathFromALT, selectorTransform);
		_assetLookupSystem = assetLookupSystem;
		_objectPool = objectPool;
		_petCatalog = petCatalog;
		_clientLocProvider = clientLocProvider;
		_selector.Init(assetLookupSystem, cosmeticsProvider, petCatalog, objectPool, _clientLocProvider, OnPetSelected);
		_selector.SetCallbacks(OnPetSelected, OnClose);
		_selector.OnStoreClicked(OnStoreSelected);
		_selector.SetOnDefaultCallback(OnDefaultCallback);
		base.IsReadOnly = isSideboarding;
	}

	public void SetData(IReadOnlyCollection<CosmeticPetEntry> ownedPets, string currentPet, string defaultPet = "", bool showDefaultInterface = false, bool isReadOnly = false)
	{
		RemoveOldDisplayInstance();
		base.IsReadOnly = isReadOnly;
		_ownedPets = ownedPets;
		_showDefaultInterface = showDefaultInterface;
		string text = (string.IsNullOrEmpty(currentPet) ? defaultPet : currentPet);
		if (!string.IsNullOrEmpty(text))
		{
			PetPayload payloadByPetEntry = GetPayloadByPetEntry(text);
			if (payloadByPetEntry != null)
			{
				Setup3DPetFromPayload(payloadByPetEntry);
			}
		}
		_selector.SetData(currentPet, defaultPet);
	}

	public override void OpenSelector()
	{
		if (!base.IsReadOnly)
		{
			base.OpenSelector();
			_selector.Open(_ownedPets, _showDefaultInterface);
		}
	}

	public override void CloseSelector()
	{
		_selector.Close();
	}

	public void SetOnCosmeticSelected(Action<PetEntry> onCosmeticSelected)
	{
		_onCosmeticSelected = onCosmeticSelected;
	}

	public void SetOnDefaultSelected(Action action)
	{
		_onDefaultSelected = action;
	}

	private void OnDefaultCallback()
	{
		_onDefaultSelected?.Invoke();
	}

	public void OnStoreClicked(Action<Action> storeAction)
	{
		_storeAction = storeAction;
	}

	private string GetPrefabPathFromALT(AssetLookupSystem assetLookupSystem)
	{
		assetLookupSystem.Blackboard.Clear();
		string text = (assetLookupSystem.TreeLoader.LoadTree<PetPopUpV2Prefab>()?.GetPayload(assetLookupSystem.Blackboard))?.PrefabPath;
		if (text == null)
		{
			SimpleLog.LogError("Could not find prefab for lookup: AvatarSelectPanelPrefab");
			return "";
		}
		return text;
	}

	private void ShowCosmeticPreview(PetEntry previewItem)
	{
		if (previewItem == null)
		{
			if (petInstance != null)
			{
				petInstance.gameObject.SetActive(value: false);
			}
		}
		else if (_showSelectedPet)
		{
			PetPayload payloadByPetEntry = GetPayloadByPetEntry(previewItem.Name, previewItem.Variant);
			Setup3DPetFromPayload(payloadByPetEntry);
		}
	}

	private void RemoveOldDisplayInstance()
	{
		if (petInstance != null)
		{
			UnityEngine.Object.Destroy(petInstance);
		}
	}

	private void Setup3DPetFromPayload(PetPayload petPayload)
	{
		if (!(_petAnchor == null))
		{
			RemoveOldDisplayInstance();
			petInstance = _objectPool.PopObject(petPayload.WrapperPrefab.RelativePath);
			petInstance.GetComponentInChildren<Animator>().SetBool(InWrapper, value: true);
			petInstance.transform.parent = _petAnchor;
			petInstance.transform.ZeroOut();
			petInstance.transform.localRotation = Quaternion.identity;
			petInstance.gameObject.SetActive(value: true);
		}
	}

	private PetPayload GetPayloadByPetEntry(string petString)
	{
		if (!petString.Contains("."))
		{
			return null;
		}
		string[] array = petString.Split('.');
		return GetPayloadByPetEntry(array[0], array[1]);
	}

	private PetPayload GetPayloadByPetEntry(string petId, string variant)
	{
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.PetId = petId;
		_assetLookupSystem.Blackboard.PetVariantId = variant;
		return _assetLookupSystem.TreeLoader.LoadTree<PetPayload>().GetPayload(_assetLookupSystem.Blackboard);
	}

	private void OnPetSelected(PetEntry selectedPet)
	{
		ShowCosmeticPreview(selectedPet);
		_onCosmeticSelected(selectedPet);
	}
}
