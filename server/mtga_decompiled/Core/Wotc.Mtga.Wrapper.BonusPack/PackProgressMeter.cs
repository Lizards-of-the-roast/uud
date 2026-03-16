using System.Collections;
using System.Collections.Generic;
using Core.Shared.Code;
using TMPro;
using UnityEngine;
using Wizards.Mtga;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga.Wrapper.BonusPack;

[RequireComponent(typeof(PackProgressEvents))]
public class PackProgressMeter : PopupBase
{
	[SerializeField]
	private Animator _animator;

	[SerializeField]
	private TextMeshProUGUI _packCountLeftText;

	[SerializeField]
	private float _delayFromSceneLoadToGrant = 2f;

	public static readonly int MeterPipCountParam = Animator.StringToHash("MeterPips");

	public static readonly int BonusPackSpawnCountParam = Animator.StringToHash("SpawnBonusPack");

	private static readonly int _bonusPackTriggerParam = Animator.StringToHash("GrantBooster");

	private const string _packProgressRemainingLocalizationInlineParamaterName = "remaining";

	private BonusPackManager _bonusPackManager;

	private PackProgressEvents _packProgressEvents;

	private int MeterPipCount
	{
		set
		{
			_animator.SetInteger(MeterPipCountParam, value);
			_packCountLeftText.text = Languages.ActiveLocProvider.GetLocalizedText("MainNav/PackProgressMeter/CallToAction", ("remaining", _bonusPackManager.RemainingPackCountToReward.ToString()));
		}
	}

	private int BonusPackSpawnCount
	{
		set
		{
			_animator.SetInteger(BonusPackSpawnCountParam, value);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_packProgressEvents = GetComponent<PackProgressEvents>();
		_packProgressEvents.ProgressChanged.AddListener(OnProgressCurrentChanged);
		_packProgressEvents.BeforePacksPurchased.AddListener(OnBeforePacksPurchased);
		_packProgressEvents.PackPurchasedCount.AddListener(OnPacksPurchased);
		_bonusPackManager = WrapperController.Instance.BonusPackManager;
	}

	private void OnBeforePacksPurchased(Dictionary<string, int> expectedPacks)
	{
		if (base.IsShowing)
		{
			_bonusPackManager.HidePacks(expectedPacks);
		}
	}

	private void OnPacksPurchased(Dictionary<string, int> packsPurchased)
	{
		int num = 0;
		foreach (int value in packsPurchased.Values)
		{
			num += value;
		}
		BonusPackSpawnCount = num;
		_animator.SetTrigger(_bonusPackTriggerParam);
	}

	private void OnProgressCurrentChanged(int currentProgress, int delta)
	{
		if (delta >= 0)
		{
			MeterPipCount = currentProgress;
		}
	}

	protected override void Show()
	{
		if (!base.IsShowing)
		{
			base.Show();
			RefreshPackMeterParams();
			SceneLoadedCoroutine();
		}
	}

	private void RefreshPackMeterParams()
	{
		MeterPipCount = _bonusPackManager.ProgressCurrent;
		BonusPackSpawnCount = 0;
		_animator.ResetTrigger(_bonusPackTriggerParam);
	}

	private void SceneLoadedCoroutine()
	{
		Pantry.Get<GlobalCoroutineExecutor>().StartGlobalCoroutine(SceneLoaded());
	}

	private IEnumerator SceneLoaded()
	{
		yield return new WaitForSeconds(_delayFromSceneLoadToGrant);
		if (_bonusPackManager.CanRedeem)
		{
			_bonusPackManager.Redeem();
		}
	}

	internal void ResetParametersAfterAnimation()
	{
		MeterPipCount = _bonusPackManager.ProgressCurrent;
		BonusPackSpawnCount = _bonusPackManager.RedeemablePackCount;
	}

	public void InStorePurchaseConfirmation(bool inPurchaseConfirmation)
	{
		if (inPurchaseConfirmation)
		{
			_packCountLeftText.text = "";
		}
		else
		{
			RefreshPackMeterParams();
		}
	}

	public override void OnEscape()
	{
		_popupManager?.ToggleMenu();
	}

	public override void OnEnter()
	{
	}
}
