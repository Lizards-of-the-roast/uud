using System;
using System.Collections.Generic;
using Core.Meta.MainNavigation.Store;
using UnityEngine;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

[RequireComponent(typeof(Animator))]
public class Tab : MonoBehaviour
{
	[SerializeField]
	private Localize _text;

	[SerializeField]
	private Localize _subText;

	[SerializeField]
	public GameObject _activeTabIndicator;

	[SerializeField]
	private GameObject _notificationDot;

	[SerializeField]
	private GameObject _sparkyHighlight;

	private Animator _animator;

	private static readonly int Lock = Animator.StringToHash("Lock");

	private static readonly int PrizeWall = Animator.StringToHash("TabType");

	public GameObject SparkyHighlight => _sparkyHighlight;

	public ETabTypeForAnimator TabType { get; private set; }

	public bool Locked { get; private set; }

	public event Action<Tab> Clicked;

	private void OnEnable()
	{
		if (_animator == null)
		{
			_animator = GetComponent<Animator>();
		}
		_animator.SetBool(Lock, Locked);
		_animator.SetInteger(PrizeWall, (int)TabType);
	}

	private void OnDestroy()
	{
		this.Clicked = null;
	}

	public void SetTabType(ETabTypeForAnimator tabType)
	{
		TabType = tabType;
		if (_animator == null)
		{
			_animator = GetComponent<Animator>();
		}
		if (_animator.isActiveAndEnabled)
		{
			_animator.SetInteger(PrizeWall, (int)TabType);
		}
	}

	public void SetLabel(MTGALocalizedString locTerm)
	{
		if (_text != null)
		{
			_text.SetText(locTerm);
		}
	}

	public void SetSubLabel(string key, Dictionary<string, string> tokenValues)
	{
		if (_subText != null)
		{
			_subText.SetText(key, tokenValues);
		}
	}

	public void OnClicked()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_whoosh_01, base.gameObject);
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, base.gameObject);
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_card_cosmetic_editing_browser_rollover, base.gameObject);
		this.Clicked?.Invoke(this);
	}

	public void OnHover()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rollover, base.gameObject);
	}

	public void SetLocked(bool locked)
	{
		Locked = locked;
		if (_animator == null)
		{
			_animator = GetComponent<Animator>();
		}
		if (_animator.isActiveAndEnabled)
		{
			_animator.SetBool(Lock, Locked);
		}
	}

	public void SetTabActiveVisuals(bool show)
	{
		_activeTabIndicator.UpdateActive(show);
	}

	public void SetSparkyBeacons(string beaconName)
	{
		SceneObjectBeacon sceneObjectBeacon = base.gameObject.AddComponent<SceneObjectBeacon>();
		sceneObjectBeacon.BeaconName = "StoreTab_" + beaconName;
		sceneObjectBeacon.InitializeBeacon();
		if (_sparkyHighlight != null)
		{
			SceneObjectBeacon sceneObjectBeacon2 = _sparkyHighlight.AddComponent<SceneObjectBeacon>();
			sceneObjectBeacon2.BeaconName = "StoreTab_" + beaconName + "_Highlight";
			sceneObjectBeacon2.InitializeBeacon();
		}
	}

	public void SetPipVisible(bool show)
	{
		_notificationDot.UpdateActive(show);
	}
}
