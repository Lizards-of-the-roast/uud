using System;
using AssetLookupTree.Payloads.Cosmetic;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.Client.Models.Catalog;

namespace Core.Meta.MainNavigation.Cosmetics;

public class SelectPetsListItemView : MonoBehaviour
{
	[SerializeField]
	private Image Icon;

	[SerializeField]
	private GameObject LockIcon;

	[SerializeField]
	private CustomButton Button;

	[SerializeField]
	private Animator Animator;

	private bool _isOwned;

	private bool _isSkin;

	private bool _isDefault;

	private int _variantIndex;

	private bool _isSelected;

	private string _petId;

	private PetPayload _petPayload;

	private PetEntry _petEntry;

	private bool isDirty;

	private Action<PetEntry, PetPayload, SelectPetsListItemView> _buttonOnClickCallback;

	private static readonly int Selected = Animator.StringToHash("Selected");

	private static readonly int Locked = Animator.StringToHash("Locked");

	private static readonly int Default = Animator.StringToHash("AccountDefault");

	public bool IsOwned => _isOwned;

	public bool IsSkin => _isSkin;

	public bool IsDefault => _isDefault;

	public int VariantIndex => _variantIndex;

	public string PetId => _petId;

	public PetEntry PetEntry => _petEntry;

	public void Initialize(PetEntry petEntry, PetPayload petPayload, int variantIndex, bool isSkin, bool isOwned, bool isDefault, bool isSelected, Sprite icon, Action<PetEntry, PetPayload, SelectPetsListItemView> buttonOnClickCallback)
	{
		_petId = petEntry.Name + "." + petEntry.Variant;
		_variantIndex = variantIndex;
		_isOwned = isOwned;
		_isDefault = isDefault;
		_isSkin = isSkin;
		_isSelected = isSelected;
		_petEntry = petEntry;
		_petPayload = petPayload;
		_buttonOnClickCallback = buttonOnClickCallback;
		SetPetsIcon(icon);
		SetButtonCallback(buttonOnClickCallback);
		isDirty = true;
	}

	public void Update()
	{
		if (isDirty)
		{
			SetIsLocked(!IsOwned);
			SetIsDefault(IsDefault);
			SetIsSelected(_isSelected);
			isDirty = false;
		}
	}

	public void Initialize(Action<PetEntry, PetPayload, SelectPetsListItemView> buttonOnClickCallback)
	{
		_petEntry = null;
		_petPayload = null;
		_isOwned = true;
		_isSelected = true;
		_buttonOnClickCallback = buttonOnClickCallback;
		SetButtonCallback(buttonOnClickCallback);
		isDirty = true;
	}

	public void HasBeenSelected()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, base.gameObject);
		_buttonOnClickCallback?.Invoke(_petEntry, _petPayload, this);
		SetIsSelected(selected: true);
	}

	private void SetPetsIcon(Sprite iconSprite)
	{
		Icon.sprite = iconSprite;
	}

	private void SetButtonCallback(Action<PetEntry, PetPayload, SelectPetsListItemView> buttonOnClickCallback)
	{
		Button.OnClick.RemoveAllListeners();
		Button.OnClick.AddListener(HasBeenSelected);
	}

	private void SetIsLocked(bool isLocked)
	{
		Animator.SetBool(Locked, isLocked);
	}

	public void SetIsSelected(bool selected)
	{
		_isSelected = selected;
		Animator.SetBool(Selected, selected);
	}

	public void SetIsDefault(bool isDefault)
	{
		Animator.SetBool(Default, isDefault);
	}
}
