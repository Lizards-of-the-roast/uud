using System;
using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Payloads.Card;
using GreClient.CardData;
using TMPro;
using UnityEngine;
using Wotc.Mtga.Cards.Parts.Textbox;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

public class CDCPart_TextBox_Dungeon : CDCPart_Textbox_SuperBase
{
	private const uint NO_ACTIVE_ROOM_ID = 0u;

	private readonly uint[] _activeRoomArray = new uint[1];

	[SerializeField]
	private List<DungeonRoomTextbox> _dungeonRooms;

	private readonly List<(string, string)> _cachedLabelStrings = new List<(string, string)>();

	private string _lastLanguageUsed = string.Empty;

	protected override void HandleUpdateInternal()
	{
		base.HandleUpdateInternal();
		if (Languages.CurrentLanguage != _lastLanguageUsed)
		{
			_lastLanguageUsed = Languages.CurrentLanguage;
			_cachedLabelStrings.Clear();
			foreach (AbilityPrintingData printingAbility in _cachedModel.PrintingAbilities)
			{
				string formatForAbility = base.GetFormatForAbility(_cachedModel.InstanceId, printingAbility, AbilityState.Normal);
				string abilityTextByCardAbilityGrpId = _cardDatabase.AbilityTextProvider.GetAbilityTextByCardAbilityGrpId(_cachedModel.GrpId, printingAbility.Id, _cachedModel.AbilityIds);
				string[] array = abilityTextByCardAbilityGrpId.Split('—');
				if (array.Length == 2)
				{
					string item = string.Format(formatForAbility, array[0]);
					string item2 = string.Format(formatForAbility, array[1]);
					_cachedLabelStrings.Add((item, item2));
				}
				else
				{
					string item3 = string.Format(formatForAbility, abilityTextByCardAbilityGrpId);
					_cachedLabelStrings.Add((string.Empty, item3));
				}
			}
		}
		TMP_FontAsset titleFont = GetTitleFont();
		TMP_FontAsset fontAsset = _fontAsset;
		for (int i = 0; i < _cachedLabelStrings.Count && i < _dungeonRooms.Count; i++)
		{
			(string, string) tuple = _cachedLabelStrings[i];
			string item4 = tuple.Item1;
			string item5 = tuple.Item2;
			DungeonRoomTextbox dungeonRoomTextbox = _dungeonRooms[i];
			dungeonRoomTextbox.SetFont(titleFont, fontAsset);
			dungeonRoomTextbox.SetLabels(item4, item5);
		}
		_activeRoomArray[0] = CurrentRoomGrpId(_cachedModel);
		if (_activeRoomArray[0] != 0)
		{
			SetDungeonRoomInteractions((IReadOnlyCollection<uint>)(object)Array.Empty<uint>(), (IReadOnlyCollection<uint>)(object)_activeRoomArray, (IReadOnlyCollection<uint>)(object)_activeRoomArray, null);
		}
		FitTextInTextbox();
	}

	private void FitTextInTextbox()
	{
		foreach (DungeonRoomTextbox dungeonRoom in _dungeonRooms)
		{
			dungeonRoom.FitTextInTextbox(_supportedFontSizes);
		}
	}

	public void SetDungeonRoomInteractions(IReadOnlyCollection<uint> selectableIds, IReadOnlyCollection<uint> selectedIds, IReadOnlyCollection<uint> activeIds, Action<uint> dungeonRoomPressed)
	{
		for (int i = 0; i < _cachedModel.PrintingAbilities.Count; i++)
		{
			uint roomAbilityID = _cachedModel.PrintingAbilities[i].Id;
			bool flag = selectableIds.Contains(roomAbilityID);
			bool flag2 = selectedIds.Contains(roomAbilityID);
			bool flag3 = activeIds.Contains(roomAbilityID);
			Action roomPressed = null;
			DungeonRoomTextbox.RoomHighlight roomHighlight = DungeonRoomTextbox.RoomHighlight.Dimmed;
			if (roomAbilityID == CurrentRoomGrpId(_cachedModel))
			{
				roomHighlight = DungeonRoomTextbox.RoomHighlight.Selectable;
			}
			else if (flag2)
			{
				roomPressed = delegate
				{
					dungeonRoomPressed(roomAbilityID);
				};
				roomHighlight = DungeonRoomTextbox.RoomHighlight.Selected;
			}
			else if (flag)
			{
				roomPressed = delegate
				{
					dungeonRoomPressed(roomAbilityID);
				};
				roomHighlight = DungeonRoomTextbox.RoomHighlight.Selectable;
			}
			else if (flag3)
			{
				roomHighlight = DungeonRoomTextbox.RoomHighlight.Default;
			}
			_dungeonRooms[i].SetInteraction(roomHighlight, roomPressed);
		}
	}

	public override void HandleCleanup()
	{
		base.HandleCleanup();
		foreach (DungeonRoomTextbox dungeonRoom in _dungeonRooms)
		{
			dungeonRoom.SetInteraction(DungeonRoomTextbox.RoomHighlight.Default, null);
		}
	}

	private TMP_FontAsset GetTitleFont()
	{
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.SetCardDataExtensive(_cachedModel);
		_assetLookupSystem.Blackboard.SetCdcViewMetadata(_cachedViewMetadata);
		_assetLookupSystem.Blackboard.GameState = base.GetCurrentGameState?.Invoke();
		_assetLookupSystem.Blackboard.FieldFillerType = CDCFieldFillerFieldType.DungeonRoomTitle;
		_assetLookupSystem.Blackboard.CardHolderType = _cachedCardHolderType;
		_assetLookupSystem.Blackboard.Language = Languages.CurrentLanguage;
		if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<FieldFont> loadedTree))
		{
			FieldFont payload = loadedTree.GetPayload(_assetLookupSystem.Blackboard);
			if (payload != null)
			{
				TMP_FontAsset tMP_FontAsset = AssetLoader.AcquireAndTrackAsset(_assetTracker, "FieldFont", payload.FontAssetReference);
				if ((object)tMP_FontAsset != null)
				{
					return tMP_FontAsset;
				}
			}
		}
		return null;
	}

	private static uint CurrentRoomGrpId(ICardDataAdapter cardData)
	{
		if (cardData == null)
		{
			return 0u;
		}
		if (cardData.Owner == null)
		{
			return 0u;
		}
		if (cardData.Owner.DungeonState.Equals(null))
		{
			return 0u;
		}
		return cardData.Owner.DungeonState.CurrentRoomGrpId;
	}
}
