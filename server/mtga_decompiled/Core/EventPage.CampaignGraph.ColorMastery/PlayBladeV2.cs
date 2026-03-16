using System;
using AssetLookupTree;
using Core.Code.Input;
using UnityEngine;
using Wotc.Mtga.Events;

namespace EventPage.CampaignGraph.ColorMastery;

public class PlayBladeV2 : MonoBehaviour, IBackActionHandler
{
	[SerializeField]
	private Animator _animator;

	[SerializeField]
	private Transform _panelContainer;

	[Header("Prefabs")]
	[SerializeField]
	private ColorMasteryPanel _colorMasteryPanelPrefab;

	public Action OnHide;

	public Action OnShow;

	private IActionSystem _actionSystem;

	private bool _showing;

	private static readonly int Open = Animator.StringToHash("Open");

	private static readonly int Hover = Animator.StringToHash("Hover");

	private static readonly int Disabled = Animator.StringToHash("Disabled");

	public ColorMasteryPanel ColorMasteryPanel { get; private set; }

	public void Inject(IActionSystem actionSystem)
	{
		_actionSystem = actionSystem;
	}

	public void Hide()
	{
		if (_showing)
		{
			_showing = false;
			_animator.ResetTrigger(Open);
			_animator.ResetTrigger(Hover);
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_play_shelf_close, AudioManager.Default);
			_actionSystem.PopFocus(this);
			OnHide?.Invoke();
		}
	}

	public void Disable()
	{
		_animator.SetBool(Disabled, value: true);
	}

	public void Enable()
	{
		_animator.SetBool(Disabled, value: false);
	}

	public void Show()
	{
		if (!_showing)
		{
			_showing = true;
			_animator.SetTrigger(Open);
			_animator.ResetTrigger(Hover);
			_actionSystem.PushFocus(this);
			OnShow?.Invoke();
		}
	}

	public ColorMasteryPanel InitColorMasteryPanel(IColorChallengeStrategy strategy, string activeEvent, AssetLookupSystem assetLookupSystem)
	{
		if (ColorMasteryPanel == null)
		{
			ColorMasteryPanel = UnityEngine.Object.Instantiate(_colorMasteryPanelPrefab, _panelContainer);
		}
		ColorMasteryPanel.Init(strategy, activeEvent, assetLookupSystem).SetOnItemClickedCallback(delegate
		{
			Hide();
		});
		return ColorMasteryPanel;
	}

	public void OnBack(ActionContext context)
	{
		Hide();
	}
}
