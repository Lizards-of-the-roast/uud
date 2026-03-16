using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Wizards.Mtga.FrontDoorModels;

namespace Wotc.Mtga.Wrapper.BonusPack;

internal class PackProgressEvents : MonoBehaviour
{
	[SerializeField]
	[Tooltip("Triggers whenever the pack progress changes with the new count of tokens towards a bonus pack.")]
	private UnityEvent<int, int> _progressChanged;

	[SerializeField]
	[Tooltip("Triggers whenever the pack progress count changes with the percentage of completion towards a bonus pack.")]
	private UnityEvent<float> _progressPercentageCompletion;

	[SerializeField]
	[Tooltip("Triggers right before the BonusPackManager attempts to redeem bonus packs.")]
	private UnityEvent<Dictionary<string, int>> _beforePacksPurchased;

	[SerializeField]
	[Tooltip("Triggers whenever any number of bonus packs are being purchased/granted to the user.")]
	private UnityEvent _packsPurchased;

	[SerializeField]
	[Tooltip("Triggers whenever any number of bonus packs are being purchased/granted to the user with the count of packs granted.")]
	private UnityEvent<Dictionary<string, int>> _packPurchasedCount;

	[SerializeField]
	private bool _triggerPackProgressEventsOnEnable;

	private BonusPackManager _bonusPackManager;

	public UnityEvent<int, int> ProgressChanged => _progressChanged;

	public UnityEvent<float> ProgressPercentageCompletion => _progressPercentageCompletion;

	public UnityEvent<Dictionary<string, int>> BeforePacksPurchased => _beforePacksPurchased;

	public UnityEvent PacksPurchased => _packsPurchased;

	public UnityEvent<Dictionary<string, int>> PackPurchasedCount => _packPurchasedCount;

	private IEnumerator AssignBonusPackManager()
	{
		yield return new WaitUntil(() => WrapperController.Instance?.BonusPackManager != null && WrapperController.Instance?.InventoryManager.Inventory != null && WrapperController.Instance?.InventoryManager.Inventory.CustomTokens != null);
		_bonusPackManager = WrapperController.Instance.BonusPackManager;
		_bonusPackManager.OnProgressCurrentChanged += OnProgressCurrentChanged;
		_bonusPackManager.BeforePacksPurchased += OnBeforePacksPurchased;
		_bonusPackManager.OnPacksPurchased += OnPacksPurchased;
		if (_triggerPackProgressEventsOnEnable)
		{
			OnProgressCurrentChanged(0, 0);
		}
	}

	private void OnDestroy()
	{
		if (_bonusPackManager != null)
		{
			_bonusPackManager.OnProgressCurrentChanged -= OnProgressCurrentChanged;
			_bonusPackManager.OnPacksPurchased -= OnPacksPurchased;
		}
	}

	private void OnEnable()
	{
		StartCoroutine(AssignBonusPackManager());
	}

	private void OnBeforePacksPurchased(Dictionary<string, int> expectedPacks)
	{
		_beforePacksPurchased.Invoke(expectedPacks);
	}

	private void OnPacksPurchased(InventoryInfoShared inventoryInfo)
	{
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		foreach (InventoryChangeShared change in inventoryInfo.Changes)
		{
			foreach (BoosterStackShared booster in change.Boosters)
			{
				dictionary.TryGetValue(booster.CollationId.ToString(), out var value);
				value += booster.Count;
				dictionary[booster.CollationId.ToString()] = value;
			}
		}
		_packsPurchased.Invoke();
		_packPurchasedCount.Invoke(dictionary);
	}

	private void OnProgressCurrentChanged(int currentProgress, int delta)
	{
		_progressChanged.Invoke(_bonusPackManager.ProgressCurrent, delta);
		_progressPercentageCompletion.Invoke((float)_bonusPackManager.ProgressCurrent / (float)_bonusPackManager.ProgressMax);
	}
}
