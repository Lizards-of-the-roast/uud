using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using Core.Meta.MainNavigation.Cosmetics;
using DG.Tweening;
using Pooling;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Wizards.Arena.Models.Network;
using Wizards.Mtga;
using Wizards.Unification.Models.Mercantile;
using Wotc.Mtga.Client.Models.Catalog;
using Wotc.Mtga.Client.Models.Mercantile;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Providers;

public class PetPopUp : PopupBase, IScrollHandler, IEventSystemHandler, ICosmeticSelector<(string, string)>
{
	[Header("Scaffolding")]
	[SerializeField]
	private CustomButton _confirmButton;

	[SerializeField]
	private GameObject _storeButton;

	private PetCatalog _petCatalog;

	[SerializeField]
	private RectTransform _petContainer;

	[SerializeField]
	private ScrollRect _petScrollRect;

	[SerializeField]
	private bool _cullNonVisibleItems;

	[SerializeField]
	private int _visibleItemCount = 10;

	[SerializeField]
	private float _selectorMoveDuration = 0.35f;

	[SerializeField]
	private Ease _selectorMoveEase = Ease.OutQuint;

	[Header("Pet Visuals")]
	[SerializeField]
	private PetSelector _petholder;

	[SerializeField]
	private Localize _CurrentPetText;

	[Header("Pet Tier")]
	[SerializeField]
	private GameObject _tierSelect;

	[SerializeField]
	private Button _leftArrow;

	[SerializeField]
	private Button _rightArrow;

	[FormerlySerializedAs("_currentTier")]
	[SerializeField]
	private Localize _currentTierText;

	[Header("PopUp Things")]
	[SerializeField]
	private CustomButton _button_DismissTop;

	[SerializeField]
	private CustomButton _button_DismissBottom;

	[SerializeField]
	private CustomButton _button_DismissBack;

	private CosmeticsProvider _cosmetics;

	private AssetLookupSystem _assetLookupSystem;

	private IUnityObjectPool _objectPool;

	private int _selectorIndex;

	private PetSelector _currentSelector;

	private Dictionary<string, List<string>> _petToOwnedVariants = new Dictionary<string, List<string>>();

	private List<PetSelector> _selectors = new List<PetSelector>();

	private readonly string[] TIERS = new string[3] { "Common", "Uncommon", "Rare" };

	private string savedPet = string.Empty;

	private string savedMod = string.Empty;

	private Action<(string, string)> _onPetSelected;

	private Action _onHide;

	public void Init(AssetLookupSystem assetLookupSystem, CosmeticsProvider cosmetics, PetCatalog petCatalog, Action<(string, string)> onPetSelected)
	{
		_assetLookupSystem = assetLookupSystem;
		_cosmetics = cosmetics;
		_petCatalog = petCatalog;
		_onPetSelected = onPetSelected;
	}

	protected override void Awake()
	{
		base.Awake();
		_objectPool = Pantry.Get<IUnityObjectPool>();
		_button_DismissTop.OnClick.AddListener(OnCancelClicked);
		_button_DismissBottom.OnClick.AddListener(OnCancelClicked);
		_button_DismissBack.OnClick.AddListener(OnCancelClicked);
		_leftArrow.onClick.AddListener(OnLeftArrow);
		_rightArrow.onClick.AddListener(OnRightArrow);
		_confirmButton.OnClick.AddListener(OnConfirm);
		if (_petScrollRect != null)
		{
			_petScrollRect.onValueChanged.AddListener(OnPetScrollRectValueChanged);
		}
	}

	private void OnDestroy()
	{
		_button_DismissTop.OnClick.RemoveListener(OnCancelClicked);
		_button_DismissBottom.OnClick.RemoveListener(OnCancelClicked);
		_button_DismissBack.OnClick.RemoveListener(OnCancelClicked);
		_leftArrow.onClick.RemoveListener(OnLeftArrow);
		_rightArrow.onClick.RemoveListener(OnRightArrow);
		_confirmButton.OnClick.RemoveListener(OnConfirm);
		if (_petScrollRect != null)
		{
			_petScrollRect.onValueChanged.RemoveListener(OnPetScrollRectValueChanged);
		}
	}

	private void OnLeftArrow()
	{
		if (_tierSelect.activeSelf)
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, base.gameObject);
			UpdateTierText(-1);
		}
	}

	private void OnRightArrow()
	{
		if (_tierSelect.activeSelf)
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, base.gameObject);
			UpdateTierText(1);
		}
	}

	public void Generic_Hover()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rollover, base.gameObject);
	}

	private void UpdateTierText(int delta)
	{
		if (_currentSelector != null)
		{
			_currentSelector.UpdateTier(delta);
			string text = TIERS[Mathf.Clamp(_currentSelector.variantIndex, 0, TIERS.Length - 1)];
			if (!_currentSelector.IsSkin)
			{
				_currentTierText.SetText("MainNav/PetTiers/" + text);
			}
			else
			{
				string key = $"MainNav/PetNames/{_currentSelector.petKey}_{(_currentSelector.variantIndex + 1).ToString()}";
				_CurrentPetText.SetText(key);
			}
			_currentTierText.gameObject.UpdateActive(!_currentSelector.IsSkin);
		}
	}

	private void UpdateButtonState()
	{
		if (_currentSelector != null)
		{
			_storeButton.gameObject.SetActive(_currentSelector.showLocked);
			_confirmButton.gameObject.SetActive(!_currentSelector.showLocked);
		}
	}

	public IEnumerator OpenYield()
	{
		AudioManager.PlayAudio("sfx_ui_main_card_cosmetic_picker_in", AudioManager.Default);
		savedPet = _cosmetics?.PlayerPetSelection?.name;
		_selectors.Clear();
		_petToOwnedVariants.Clear();
		_petContainer.transform.DestroyChildren();
		PetSelector selectedPet = CreatePetSelector(string.Empty, string.IsNullOrEmpty(savedPet));
		yield return null;
		IReadOnlyList<CosmeticPetEntry> ownedPets = _cosmetics.PlayerOwnedPets;
		foreach (PetEntryGroup petEntry in _petCatalog.SortedPetGroups)
		{
			IEnumerable<CosmeticPetEntry> enumerable = ownedPets.Where((CosmeticPetEntry op) => op.Name == petEntry.name);
			if (petEntry.hasVariantsInStore)
			{
				_petToOwnedVariants.Add(petEntry.name, petEntry.variantNames);
			}
			else
			{
				if (!enumerable.Any())
				{
					continue;
				}
				_petToOwnedVariants.Add(petEntry.name, enumerable.Select((CosmeticPetEntry p) => p.Variant).ToList());
			}
			bool flag = savedPet == petEntry.name;
			PetSelector petSelector = CreatePetSelector(petEntry.name, flag, enumerable != null && enumerable.Count() <= 0);
			if (flag)
			{
				selectedPet = petSelector;
			}
			yield return null;
		}
		UpdateButtonState();
		if (!string.IsNullOrEmpty(savedPet))
		{
			UpdateTierText(0);
		}
		DOTween.Kill(_petContainer);
		if (_petScrollRect != null)
		{
			float num = (float)_selectorIndex / (float)(_selectors.Count - 1);
			if (num < _petScrollRect.horizontalScrollbar.size / 2f)
			{
				num = 0f;
			}
			else if (num > 1f - _petScrollRect.horizontalScrollbar.size / 2f)
			{
				num = 1f;
			}
			_petScrollRect.DOHorizontalNormalizedPos(num, _selectorMoveDuration).SetEase(_selectorMoveEase);
		}
		Activate(activate: true);
		yield return null;
		UpdateSelectorVisibility();
		SelectPet(selectedPet);
	}

	private PetSelector CreatePetSelector(string key, bool isSelected = false, bool showLocked = false)
	{
		GameObject gameObject = _objectPool.PopObject(_petholder.gameObject);
		PetSelector petSelector = gameObject.GetComponent<PetSelector>();
		if (petSelector != null)
		{
			petSelector.gameObject.transform.SetParent(_petContainer.gameObject.transform);
			petSelector.gameObject.transform.ZeroOut();
			string item = "";
			if (isSelected)
			{
				item = _cosmetics?.PlayerPetSelection?.variant;
			}
			int variantIdx = 0;
			if (_petToOwnedVariants.TryGetValue(key, out var value))
			{
				variantIdx = Math.Max(value.IndexOf(item), 0);
			}
			petSelector.Init(key, value?.ToArray(), variantIdx, showLocked, _assetLookupSystem);
			petSelector.Button.OnClick.AddListener(delegate
			{
				SelectPet(petSelector);
			});
			_selectors.Add(petSelector);
		}
		return petSelector;
	}

	private void OnCancelClicked()
	{
		Hide();
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_cancel, base.gameObject);
	}

	public override void OnEnter()
	{
	}

	public override void OnEscape()
	{
		OnCancelClicked();
	}

	protected override void Hide()
	{
		_onHide?.Invoke();
		SelectPet(_selectors[0]);
		base.Hide();
		_button_DismissTop.enabled = true;
		_button_DismissBottom.enabled = true;
		_button_DismissBack.enabled = true;
		AudioManager.PlayAudio("sfx_ui_main_card_cosmetic_picker_out", AudioManager.Default);
	}

	public void SelectPet(PetSelector selector)
	{
		if (_currentSelector != selector)
		{
			if (_currentSelector != null)
			{
				_currentSelector.Animator.SetBool("Selected", value: false);
			}
			_currentSelector = selector;
			_currentSelector.Animator.SetBool("Selected", value: true);
			if (!string.IsNullOrEmpty(_currentSelector.petKey))
			{
				_tierSelect.SetActive(_petToOwnedVariants[_currentSelector.petKey].Count > 1);
				if (_currentSelector.IsSkin)
				{
					_CurrentPetText.SetText($"MainNav/PetNames/{_currentSelector.petKey}_{(_currentSelector.variantIndex + 1).ToString()}");
				}
				else
				{
					_CurrentPetText.SetText($"MainNav/PetNames/{_currentSelector.petKey}");
				}
				UpdateTierText(0);
			}
			else
			{
				_tierSelect.SetActive(value: false);
				_CurrentPetText.SetText("MainNav/PetSelect/None");
				AudioManager.PlayAudio(WwiseEvents.sfx_ui_cancel, base.gameObject);
			}
			SetCurrentSelector(_selectors.IndexOf(_currentSelector));
		}
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, base.gameObject);
	}

	public void SetCurrentSelector(int selectionIndex)
	{
		bool snapping = _selectorIndex == selectionIndex;
		_selectorIndex = selectionIndex;
		DOTween.Kill(_petScrollRect);
		float num = (float)_selectorIndex / (float)(_selectors.Count - 1);
		if (_petScrollRect != null)
		{
			if (num < _petScrollRect.horizontalScrollbar.size / 2f)
			{
				num = 0f;
			}
			else if (num > 1f - _petScrollRect.horizontalScrollbar.size / 2f)
			{
				num = 1f;
			}
			_petScrollRect.DOHorizontalNormalizedPos(num, _selectorMoveDuration, snapping).SetEase(_selectorMoveEase);
		}
		UpdateButtonState();
	}

	public void OnConfirm()
	{
		string text = _currentSelector?.petKey ?? null;
		if (!string.IsNullOrEmpty(text))
		{
			string text2 = _petToOwnedVariants[text][_currentSelector.variantIndex];
			if (!string.IsNullOrEmpty(text2))
			{
				_onPetSelected?.Invoke((text, text2));
			}
		}
		else
		{
			_cosmetics.SetPetSelection(null, null);
		}
		Hide();
	}

	public void OnStoreRedirect()
	{
		string text = _currentSelector?.petKey ?? null;
		string text2 = null;
		bool flag = false;
		if (!string.IsNullOrEmpty(text))
		{
			text2 = _petToOwnedVariants[text][_currentSelector.variantIndex];
			if (!string.IsNullOrEmpty(text2))
			{
				string key = text + "." + text2;
				if (_petCatalog.TryGetValue(key, out var value))
				{
					StoreTabType fallbackContext = value.StoreSection switch
					{
						EStoreSection.Pets => StoreTabType.Cosmetics, 
						EStoreSection.Bundles => StoreTabType.Bundles, 
						_ => StoreTabType.None, 
					};
					StoreItem storeItem = value.StoreItem;
					if (storeItem != null && storeItem.FeaturedIndex > -1)
					{
						fallbackContext = StoreTabType.Featured;
					}
					SceneLoader.GetSceneLoader().GoToStoreItem(value.StoreItem.Id ?? value.StoreBundles?.FirstOrDefault()?.Id, fallbackContext, "Go to store from pets popup");
				}
				Hide();
				return;
			}
		}
		Debug.LogWarningFormat($"Failed to redirect to store from pets popup.  key={0}  variant={1}  petLookupSuccess={2}", text, text2, flag.ToString());
	}

	public void OnScroll(PointerEventData eventData)
	{
		if (eventData.scrollDelta.y < 0f && _selectorIndex < _selectors.Count - 1)
		{
			SelectPet(_selectors[_selectorIndex + 1]);
		}
		if (eventData.scrollDelta.y > 0f && _selectorIndex > 0)
		{
			SelectPet(_selectors[_selectorIndex - 1]);
		}
	}

	private void OnPetScrollRectValueChanged(Vector2 arg0)
	{
		UpdateSelectorVisibility();
	}

	private void UpdateSelectorVisibility()
	{
		int count = _selectors.Count;
		float num = 0f;
		float num2 = count;
		if (_cullNonVisibleItems)
		{
			float num3 = _petScrollRect.normalizedPosition.x * (float)(count - 1);
			float num4 = (float)_visibleItemCount / 2f;
			num = num3 - num4;
			num2 = num3 + num4;
			if (num < 0f)
			{
				num2 -= num;
			}
			else if (num2 > (float)count)
			{
				num -= num2 - (float)count;
			}
		}
		for (int i = 0; i < count; i++)
		{
			if ((float)i < num || (float)i > num2)
			{
				_selectors[i].DisablePetUIVisuals();
			}
			else
			{
				_selectors[i].EnablePetUIVisuals();
			}
		}
	}

	public void SetCallbacks(Action<(string, string)> onSelected, Action onHide)
	{
		_onPetSelected = onSelected;
		OnHide = onHide;
	}
}
