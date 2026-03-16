using System;
using AssetLookupTree;
using Core.Meta.MainNavigation.Store.Data;
using Core.Meta.MainNavigation.Store.Utils;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Arena.Enums.Card;

namespace Core.Meta.MainNavigation.Store;

public class StoreSetFilterToggle : MonoBehaviour
{
	[SerializeField]
	private Image _symbol;

	private bool isOn;

	[SerializeField]
	private Animator _animator;

	private Action<StoreSetFilterModel> _onClickAction;

	private AssetTracker _assetTracker = new AssetTracker();

	private static readonly int AnimState_Enabled = Animator.StringToHash("Enabled");

	private static readonly int Over = Animator.StringToHash("Over");

	public StoreSetFilterModel Model { get; private set; }

	public void Initialize(AssetLookupSystem assetLookupSystem, StoreSetFilterModel model, Action<StoreSetFilterModel> onClickAction, SetAvailability availableInStandard)
	{
		Model = model;
		_onClickAction = onClickAction;
		_symbol.sprite = StoreSetUtils.SpriteForSetName(model.SetSymbolAsCollationMapping, assetLookupSystem, _assetTracker, availableInStandard);
	}

	public void OnClick()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, base.gameObject);
		if (!isOn)
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_whoosh_01, base.gameObject);
			_onClickAction(Model);
		}
	}

	public void Select()
	{
		if (!isOn)
		{
			isOn = true;
			_animator.SetBool(AnimState_Enabled, value: true);
		}
	}

	public void Deselect()
	{
		if (isOn)
		{
			isOn = false;
			_animator.SetBool(AnimState_Enabled, value: false);
		}
	}

	public void ForceRefresh()
	{
		_animator.SetBool(AnimState_Enabled, isOn);
	}

	public void OnOver()
	{
		_animator.SetBool(Over, value: true);
	}

	public void OnOut()
	{
		_animator.SetBool(Over, value: false);
	}

	public void CleanUp()
	{
		_assetTracker.Cleanup();
	}
}
