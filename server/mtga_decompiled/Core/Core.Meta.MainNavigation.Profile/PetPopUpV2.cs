using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using AssetLookupTree.Payloads.Cosmetic;
using Core.Meta.MainNavigation.Cosmetics;
using DG.Tweening;
using Pooling;
using UnityEngine;
using Wizards.Arena.Models.Network;
using Wizards.Unification.Models.Mercantile;
using Wotc.Mtga.Client.Models.Catalog;
using Wotc.Mtga.Client.Models.Mercantile;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Providers;

namespace Core.Meta.MainNavigation.Profile;

public class PetPopUpV2 : PopupBase, ICosmeticSelector<PetEntry>
{
	[Header("Left side pet options")]
	[SerializeField]
	private SelectPetsListItemView petListItemView;

	[SerializeField]
	protected RectTransform _petContainer;

	[SerializeField]
	private CustomButton _petHitbox;

	[Header("Right Side Pet Preview")]
	[SerializeField]
	private Transform selectedPetVisualTransform;

	[SerializeField]
	protected Localize _currentTierText;

	[SerializeField]
	protected Localize _currentPetText;

	[Header("Buttons")]
	[SerializeField]
	private CustomButton _confirmButton;

	[SerializeField]
	private CustomButton _confirmSetDefaultButton;

	[SerializeField]
	protected CustomButton _backButton;

	[SerializeField]
	private CustomButton _makeDefault;

	[SerializeField]
	private GameObject _storeButton;

	private IUnityObjectPool _objectPool;

	private AssetLookupSystem _assetLookupSystem;

	private CosmeticsProvider _cosmetics;

	private PetCatalog _petCatalog;

	private IClientLocProvider _clientLocProvider;

	private GameObject selectedPetInstance;

	private readonly AssetTracker _assetTracker = new AssetTracker();

	private SelectPetsListItemView _selectPetListIcon;

	private string _defaultPet = "";

	private SelectPetsListItemView defaultPetListItemView;

	private string _currentPet;

	private bool _isDefaultSelector;

	private readonly string[] TIERS = new string[3] { "Common", "Uncommon", "Rare" };

	private static readonly int InWrapper = Animator.StringToHash("InWrapper");

	private static readonly int Wrapper_Hover = Animator.StringToHash("Wrapper_Hover");

	private static readonly int Wrapper_Click = Animator.StringToHash("Wrapper_Click");

	private Action<PetEntry> _onPetSelected;

	private Action _onDefaultCallback;

	private Action<Action> _storeAction;

	public void Init(AssetLookupSystem assetLookupSystem, CosmeticsProvider cosmeticsProvider, PetCatalog petCatalog, IUnityObjectPool objectPool, IClientLocProvider clientLocProvider, Action<PetEntry> onPetSelected)
	{
		_objectPool = objectPool;
		_assetLookupSystem = assetLookupSystem;
		_petCatalog = petCatalog;
		_onPetSelected = onPetSelected;
		_cosmetics = cosmeticsProvider;
		_clientLocProvider = clientLocProvider;
	}

	protected override void Awake()
	{
		_confirmButton.OnClick.AddListener(OnConfirm);
		_confirmSetDefaultButton.OnClick.AddListener(OnConfirm);
		_backButton.OnClick.AddListener(OnCancelClicked);
		_makeDefault.OnClick.AddListener(SetDefaultPet);
	}

	public override void OnEscape()
	{
		OnCancelClicked();
	}

	public override void OnEnter()
	{
	}

	public void SetData(string currentPet, string defaultPet)
	{
		_currentPet = currentPet;
		if (string.IsNullOrEmpty(currentPet))
		{
			_currentPet = defaultPet;
		}
		_defaultPet = defaultPet;
	}

	private void SetDefaultPet()
	{
		if (!(_selectPetListIcon == null))
		{
			if (defaultPetListItemView != null)
			{
				defaultPetListItemView.SetIsDefault(isDefault: false);
			}
			_defaultPet = _selectPetListIcon.PetId;
			_selectPetListIcon.SetIsDefault(isDefault: true);
			defaultPetListItemView = _selectPetListIcon;
			string newName;
			string newVariant;
			if (_selectPetListIcon.PetEntry != null)
			{
				string text = _selectPetListIcon.PetEntry.Name;
				string variant = _selectPetListIcon.PetEntry.Variant;
				newName = text;
				newVariant = variant;
			}
			else
			{
				newName = "";
				newVariant = "";
			}
			StartCoroutine(Coroutine_SetDefaultPet(newName, newVariant));
		}
	}

	private IEnumerator Coroutine_SetDefaultPet(string newName, string newVariant)
	{
		yield return _cosmetics.SetPetSelection(newName, newVariant).AsCoroutine();
		UpdateButtonState(_selectPetListIcon.IsOwned);
		_onDefaultCallback?.Invoke();
	}

	public void SetOnDefaultCallback(Action onDefaultCallback)
	{
		_onDefaultCallback = onDefaultCallback;
	}

	private void OnDestroy()
	{
		_confirmButton.OnClick.RemoveListener(OnConfirm);
		_backButton.OnClick.RemoveListener(OnCancelClicked);
		_assetTracker.Cleanup();
	}

	private void OnCancelClicked()
	{
		Hide();
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_cancel, base.gameObject);
	}

	private void UpdateNameAndTierTextForNoPetSelected()
	{
		_currentPetText.SetText("MainNav/PetSelect/None");
		_currentTierText.gameObject.SetActive(value: false);
	}

	private void UpdateNameAndTierText(PetEntry pet, bool isSkin)
	{
		string key = PetUtils.KeyForPetDetails(pet, _clientLocProvider);
		if (!isSkin)
		{
			string text = TIERS[Mathf.Clamp(pet.Level, 0, TIERS.Length - 1)];
			_currentTierText.gameObject.SetActive(value: true);
			_currentTierText.SetText("MainNav/PetTiers/" + text);
		}
		else
		{
			_currentTierText.gameObject.SetActive(value: false);
		}
		_currentPetText.SetText(key);
		_currentTierText.gameObject.SetActive(!isSkin);
	}

	private void Build2DPetListView(IEnumerable<PetEntryGroup> petEntryGroups, IReadOnlyCollection<CosmeticPetEntry> ownedPets)
	{
		SelectPetsListItemView selectPetsListItemView = UnityEngine.Object.Instantiate(petListItemView, _petContainer);
		selectPetsListItemView.Initialize(OnPetListSelected);
		OnPetListSelected(null, null, selectPetsListItemView);
		foreach (PetEntryGroup petEntryGroup in petEntryGroups)
		{
			int num = 0;
			int count = petEntryGroup.variants.Count;
			foreach (PetEntry variant in petEntryGroup.variants)
			{
				PetPayload petPayload = GetPetPayload(variant.Name, variant.Variant);
				bool flag = ownedPets.FirstOrDefault((CosmeticPetEntry x) => x.Id == variant.Id) != null;
				if (variant.StoreSection == EStoreSection.None && !flag)
				{
					continue;
				}
				if (petPayload == null)
				{
					Debug.LogError("failed to get pet payload: " + variant.Name + "." + variant.Variant);
					continue;
				}
				SelectPetsListItemView selectPetsListItemView2 = UnityEngine.Object.Instantiate(petListItemView, _petContainer);
				Sprite icon = AssetLoader.AcquireAndTrackAsset(_assetTracker, variant.Id, petPayload.Icon);
				bool isSkin = count == 1;
				bool flag2 = _defaultPet == variant.Name + "." + variant.Variant;
				bool flag3 = _currentPet == petEntryGroup.name + "." + variant.Variant;
				selectPetsListItemView2.Initialize(variant, petPayload, num, isSkin, flag, flag2, flag3, icon, OnPetListSelected);
				if (flag3)
				{
					OnPetListSelected(variant, petPayload, selectPetsListItemView2);
				}
				if (flag2)
				{
					defaultPetListItemView = selectPetsListItemView2;
				}
				num++;
			}
			UpdateButtonState(isOwned: true);
		}
	}

	private void OnPetListSelected(PetEntry selectedPetEntry, PetPayload selectedPetPayload, SelectPetsListItemView selectPetsListItemView)
	{
		if (selectedPetInstance != null)
		{
			_objectPool.PushObject(selectedPetInstance.gameObject);
		}
		if (_selectPetListIcon != null)
		{
			_selectPetListIcon.SetIsSelected(selected: false);
		}
		_selectPetListIcon = selectPetsListItemView;
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, base.gameObject);
		if (selectedPetInstance != null)
		{
			ClearPreviousPetListeners();
		}
		if (selectedPetPayload == null)
		{
			UpdateNameAndTierTextForNoPetSelected();
			UpdateButtonState(isOwned: true);
			return;
		}
		Show3DPet(selectedPetPayload);
		OnPetPopupActive();
		UpdateNameAndTierText(selectedPetEntry, selectPetsListItemView.IsSkin);
		UpdateButtonState(selectPetsListItemView.IsOwned);
	}

	private void OnPetPopupActive()
	{
		if (!(selectedPetInstance != null))
		{
			return;
		}
		Animator componentInChildren = selectedPetInstance.GetComponentInChildren<Animator>();
		if (componentInChildren != null)
		{
			if (componentInChildren.ContainsParameter(InWrapper))
			{
				componentInChildren.SetBool(InWrapper, value: true);
			}
			if (componentInChildren.ContainsParameter(Wrapper_Hover))
			{
				componentInChildren.Play(Wrapper_Hover);
			}
			SetUpHover(componentInChildren);
			SetUpClick(componentInChildren);
		}
	}

	private void Show3DPet(PetPayload selectedPetPayload)
	{
		selectedPetInstance = _objectPool.PopObject(selectedPetPayload.WrapperPrefab.RelativePath);
		selectedPetInstance.transform.parent = selectedPetVisualTransform;
		selectedPetInstance.transform.ZeroOut();
		selectedPetInstance.transform.localRotation = Quaternion.identity;
	}

	private void ClearPreviousPetListeners()
	{
		_petHitbox.OnMouseover.RemoveAllListeners();
		_petHitbox.OnMouseoff.RemoveAllListeners();
		_petHitbox.OnClick.RemoveAllListeners();
	}

	private void SetUpHover(Animator petAnimator)
	{
		_petHitbox.OnMouseover.AddListener(delegate
		{
			if (petAnimator.ContainsParameter(InWrapper))
			{
				petAnimator.SetBool(InWrapper, value: true);
			}
			if (petAnimator.ContainsParameter(Wrapper_Hover))
			{
				petAnimator.SetBool(Wrapper_Hover, value: true);
			}
		});
		_petHitbox.OnMouseoff.AddListener(delegate
		{
			if (petAnimator.ContainsParameter(InWrapper))
			{
				petAnimator.SetBool(InWrapper, value: true);
			}
			if (petAnimator.ContainsParameter(Wrapper_Hover))
			{
				petAnimator.SetBool(Wrapper_Hover, value: false);
			}
		});
	}

	private void SetUpClick(Animator petAnimator)
	{
		_petHitbox.OnClick.AddListener(delegate
		{
			if (petAnimator.ContainsParameter(InWrapper))
			{
				petAnimator.SetBool(InWrapper, value: true);
			}
			if (petAnimator.ContainsParameter(Wrapper_Click))
			{
				petAnimator.SetTrigger(Wrapper_Click);
			}
		});
	}

	private PetPayload GetPetPayload(string petKey, string variant)
	{
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.PetId = petKey;
		_assetLookupSystem.Blackboard.PetVariantId = variant;
		return _assetLookupSystem.TreeLoader.LoadTree<PetPayload>().GetPayload(_assetLookupSystem.Blackboard);
	}

	public void Open(IReadOnlyCollection<CosmeticPetEntry> ownedPets, bool showDefault)
	{
		_isDefaultSelector = showDefault;
		AudioManager.PlayAudio("sfx_ui_main_card_cosmetic_picker_in", AudioManager.Default);
		_petContainer.transform.DestroyChildren();
		Build2DPetListView(_petCatalog.SortedPetGroups, ownedPets);
		DOTween.Kill(_petContainer);
		Activate(activate: true);
		OnPetPopupActive();
	}

	private void UpdateButtonState(bool isOwned)
	{
		_storeButton.gameObject.SetActive(!isOwned);
		_confirmSetDefaultButton.gameObject.SetActive(isOwned && !_isDefaultSelector);
		_makeDefault.gameObject.SetActive(_isDefaultSelector && isOwned);
		_makeDefault.Interactable = _selectPetListIcon.PetId != _defaultPet;
		_confirmButton.gameObject.SetActive(isOwned && _isDefaultSelector);
		_confirmButton.Interactable = _selectPetListIcon.PetId != _currentPet;
	}

	public void OnConfirm()
	{
		_onPetSelected?.Invoke(_selectPetListIcon.PetEntry);
		_currentPet = _selectPetListIcon.PetId;
		Hide();
	}

	public void OnStoreRedirect()
	{
		if (!string.IsNullOrEmpty(_selectPetListIcon.PetId))
		{
			if (_petCatalog.TryGetValue(_selectPetListIcon.PetId, out var pet))
			{
				StoreTabType targetStoreTab;
				switch (pet.StoreSection)
				{
				case EStoreSection.Pets:
					targetStoreTab = StoreTabType.Cosmetics;
					break;
				case EStoreSection.Bundles:
					targetStoreTab = StoreTabType.Bundles;
					break;
				default:
					targetStoreTab = StoreTabType.None;
					break;
				}
				StoreItem storeItem = pet.StoreItem;
				if (storeItem != null && storeItem.FeaturedIndex > -1)
				{
					targetStoreTab = StoreTabType.Featured;
				}
				Action obj = delegate
				{
					SceneLoader.GetSceneLoader().GoToStoreItem(pet.StoreItem?.Id ?? pet.StoreBundles?.FirstOrDefault()?.Id, targetStoreTab, "Go to store from pets popup");
				};
				_storeAction(obj);
			}
			Hide();
		}
		else
		{
			Debug.LogWarningFormat("Failed to redirect to store from pets popup.  petId=" + _selectPetListIcon.PetId);
		}
	}

	public void OnStoreClicked(Action<Action> storeAction)
	{
		_storeAction = storeAction;
	}

	public void SetCallbacks(Action<PetEntry> onSelected, Action onHide)
	{
		_onPetSelected = onSelected;
		OnHide = onHide;
	}

	public void Close()
	{
		base.gameObject.SetActive(value: false);
	}
}
