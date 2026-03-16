using System.Collections.Generic;
using AssetLookupTree;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga.Events;

namespace Wotc.Mtga.DuelScene.UI;

public class PlayerName : MonoBehaviour
{
	public RectTransform TextRoot;

	[SerializeField]
	private TextMeshProUGUI _text;

	[SerializeField]
	private RankDisplay _rankDisplay;

	[SerializeField]
	private PlayerDesignationWidget _commanderWidget;

	[SerializeField]
	private PlayerDesignationWidget _companionWidget;

	[SerializeField]
	private PlayerDesignationWidget _titleWidget;

	[SerializeField]
	private PlayerDesignationWidget _secondCommanderWidget;

	public GameObject _firstInfoSlot;

	public GameObject _secondInfoSlot;

	public PlayerInfoRotationIndicator _infoRotationIndicator;

	[Space(5f)]
	[SerializeField]
	private Sprite _winPipSprite;

	[SerializeField]
	private GameObject _winPipRoot;

	[SerializeField]
	private PlayerInfoWinPips _winPipSlotItem;

	[SerializeField]
	private Image[] _winPipImages;

	private bool _commanderSet;

	private bool _secondCommanderSet;

	private bool _titleSet;

	private bool _companionSet;

	private bool _isBestOfThree;

	private List<IPlayerInfoSlotItem> _firstSlotItems = new List<IPlayerInfoSlotItem>();

	private List<IPlayerInfoSlotItem> _secondSlotItems = new List<IPlayerInfoSlotItem>();

	private int _firstSlotCurrentIndex;

	private int _secondSlotCurrentIndex;

	public RankDisplay RankDisplay => _rankDisplay;

	public GameObject WinpipRoot => _winPipRoot;

	public PlayerDesignationWidget CommanderWidget => _commanderWidget;

	public PlayerDesignationWidget CompanionWidget => _companionWidget;

	public PlayerDesignationWidget TitleWidget => _titleWidget;

	public PlayerDesignationWidget SecondCommanderWidget => _secondCommanderWidget;

	public void SetName(string playerName)
	{
		_text.text = playerName;
	}

	public void SetTitle(string playerTitle)
	{
		TitleWidget?.SetText(playerTitle);
		TitleWidget.gameObject.SetActive(value: true);
		_titleSet = true;
	}

	public void SetRank(RankInfo rankInfo, IEventInfo eventInfo, AssetLookupSystem assetLookupSystem)
	{
		if (rankInfo != null && rankInfo.rankClass != RankingClassType.None && eventInfo != null && eventInfo.IsRanked)
		{
			_rankDisplay.gameObject.SetActive(value: true);
			_rankDisplay.IsLimited = eventInfo.FormatType != MDNEFormatType.Constructed;
			_rankDisplay.CalculateRankDisplay(rankInfo, assetLookupSystem);
		}
		else
		{
			_rankDisplay.gameObject.SetActive(value: false);
		}
	}

	public void SetCommanderText(string commanderName)
	{
		CommanderWidget.SetText(commanderName);
		CommanderWidget.gameObject.SetActive(value: true);
		_commanderSet = true;
	}

	public void SetSecondCommanderText(string commanderName)
	{
		SecondCommanderWidget.SetText(commanderName);
		SecondCommanderWidget.gameObject.SetActive(value: true);
		_secondCommanderSet = true;
	}

	public void SetCompanionText(string companionName)
	{
		CompanionWidget.SetText(companionName);
		CompanionWidget.gameObject.SetActive(value: true);
		_companionSet = true;
	}

	public void SetNameColor(Color labelColor)
	{
		if (!(_text.color == labelColor))
		{
			_text.color = labelColor;
		}
	}

	public void SetWins(uint requiredWins, uint playerWins)
	{
		if (requiredWins > 1)
		{
			_winPipRoot.transform.SetParent(_secondInfoSlot.transform);
			_winPipRoot.SetActive(value: true);
			_isBestOfThree = true;
		}
		for (int i = 0; i < _winPipImages.Length; i++)
		{
			_winPipImages[i].gameObject.SetActive(i < requiredWins);
			if (i < playerWins)
			{
				_winPipImages[i].sprite = _winPipSprite;
			}
		}
	}

	public void InitializeInfoSlots()
	{
		_firstSlotItems.Clear();
		_secondSlotItems.Clear();
		if (_commanderSet)
		{
			CommanderWidget.transform.SetParent(_firstInfoSlot.transform);
			_firstSlotItems.Add(_commanderWidget);
			if (_secondCommanderSet)
			{
				if (!_titleSet && !_companionSet)
				{
					SecondCommanderWidget.transform.SetParent(_secondInfoSlot.transform);
					_secondSlotItems.Add(_secondCommanderWidget);
				}
				else
				{
					SecondCommanderWidget.transform.SetParent(_firstInfoSlot.transform);
					_firstSlotItems.Add(_secondCommanderWidget);
				}
			}
		}
		if (_companionSet)
		{
			if (!_commanderSet)
			{
				CompanionWidget.transform.SetParent(_firstInfoSlot.transform);
				_firstSlotItems.Add(_companionWidget);
			}
			else
			{
				CompanionWidget.transform.SetParent(_secondInfoSlot.transform);
				_secondSlotItems.Add(_companionWidget);
			}
		}
		if (_titleSet)
		{
			if (!_commanderSet)
			{
				TitleWidget.transform.SetParent(_firstInfoSlot.transform);
				_firstSlotItems.Add(_titleWidget);
			}
			else
			{
				TitleWidget.transform.SetParent(_secondInfoSlot.transform);
				_secondSlotItems.Add(_titleWidget);
			}
		}
		if (_isBestOfThree)
		{
			_secondSlotItems.Add(_winPipSlotItem);
		}
		if (_firstSlotItems.Count > 0)
		{
			_firstInfoSlot.gameObject.SetActive(value: true);
			_firstSlotItems[_firstSlotCurrentIndex].FadeIn(0f);
			if (_firstSlotItems.Count > 1)
			{
				for (int i = 1; i < _firstSlotItems.Count; i++)
				{
					_firstSlotItems[i].FadeOut(0f);
				}
			}
		}
		if (_secondSlotItems.Count > 0)
		{
			_secondInfoSlot.gameObject.SetActive(value: true);
			_secondSlotItems[_secondSlotCurrentIndex].FadeIn(0f);
			if (_secondSlotItems.Count > 1)
			{
				for (int j = 1; j < _secondSlotItems.Count; j++)
				{
					_secondSlotItems[j].FadeOut(0f);
				}
			}
		}
		if (_firstSlotItems.Count > 1 || _secondSlotItems.Count > 1)
		{
			_infoRotationIndicator.Init((_firstSlotItems.Count >= _secondSlotItems.Count) ? _firstSlotItems.Count : _secondSlotItems.Count);
		}
	}

	public void RotateInfoSlots()
	{
		if (_firstSlotItems.Count > 1)
		{
			_firstSlotItems[_firstSlotCurrentIndex].FadeOut(1f);
			_firstSlotCurrentIndex = ++_firstSlotCurrentIndex % _firstSlotItems.Count;
			_firstSlotItems[_firstSlotCurrentIndex].FadeIn(1f);
		}
		if (_secondSlotItems.Count > 1)
		{
			_secondSlotItems[_secondSlotCurrentIndex].FadeOut(1f);
			_secondSlotCurrentIndex = ++_secondSlotCurrentIndex % _secondSlotItems.Count;
			_secondSlotItems[_secondSlotCurrentIndex].FadeIn(1f);
		}
		_infoRotationIndicator.Rotate();
	}

	public void ActivateInfoItems(bool enable)
	{
		TextRoot.gameObject.SetActive(enable);
		if (_firstSlotItems.Count > 0)
		{
			_firstInfoSlot.SetActive(enable);
			foreach (Transform item in _firstInfoSlot.transform)
			{
				item.gameObject.SetActive(enable);
			}
		}
		if (_secondSlotItems.Count <= 0)
		{
			return;
		}
		_secondInfoSlot.SetActive(enable);
		foreach (Transform item2 in _secondInfoSlot.transform)
		{
			item2.gameObject.SetActive(enable);
		}
	}
}
