using System;
using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Payloads.Card.RulesText;
using GreClient.CardData;
using TMPro;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

public class CDCPart_Saga_ReminderText : CDCPart
{
	private readonly struct ClientLoc
	{
		public readonly string Key;

		public readonly (string, string)[] LocParams;

		public ClientLoc(string key, params (string, string)[] locParams)
		{
			Key = key ?? string.Empty;
			LocParams = locParams ?? Array.Empty<(string, string)>();
		}
	}

	private const string LOC_PARAM_LAST_SAGA_CHAPTER = "lastSagaChapter";

	private TextMeshProUGUI _reminderTextLabel;

	private void Awake()
	{
		_reminderTextLabel = GetComponent<TextMeshProUGUI>();
	}

	protected override void HandleUpdateInternal()
	{
		base.HandleUpdateInternal();
		if ((bool)_reminderTextLabel)
		{
			if (Languages.CurrentLanguage == "ja-JP")
			{
				_reminderTextLabel.fontStyle = FontStyles.Normal;
			}
			_reminderTextLabel.text = GetReminderText();
		}
	}

	private string GetReminderText()
	{
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.SetCardDataExtensive(_cachedModel);
		if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<SagaReminderText> loadedTree))
		{
			SagaReminderText payload = loadedTree.GetPayload(_assetLookupSystem.Blackboard);
			if (payload != null)
			{
				return payload.LocKey.GetText(_cardDatabase.ClientLocProvider, _cardDatabase.GreLocProvider);
			}
		}
		if (TryGetHighestSagaClientLoc(out var clientLoc))
		{
			return _localizationManager.GetLocalizedText(clientLoc.Key, clientLoc.LocParams);
		}
		return string.Empty;
	}

	private bool TryGetHighestSagaClientLoc(out ClientLoc clientLoc)
	{
		clientLoc = default(ClientLoc);
		if (_cachedModel == null)
		{
			return false;
		}
		IEnumerable<AbilityPrintingData> printingAbilities = _cachedModel.PrintingAbilities;
		if (printingAbilities == null)
		{
			return false;
		}
		int num = 0;
		foreach (AbilityPrintingData item in printingAbilities)
		{
			if (item.BaseId == 166 && item.BaseIdNumeral > num)
			{
				num = (int)item.BaseIdNumeral.Value;
			}
		}
		clientLoc = new ClientLoc("ReminderText_Saga", ("lastSagaChapter", num.ToRomanNumeral()));
		return true;
	}

	protected override void HandleDestructionInternal()
	{
		if (_reminderTextLabel != null)
		{
			_reminderTextLabel.gameObject.UpdateActive(!_cachedDestroyed);
		}
		base.HandleDestructionInternal();
	}
}
