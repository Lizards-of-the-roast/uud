using System;
using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Payloads.CardHolder;
using GreClient.CardData;
using GreClient.Rules;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Wotc.Mtga.DuelScene.Examine;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.UI;

public class ViewPrintingButton : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI _label;

	[SerializeField]
	private Image _icon;

	[SerializeField]
	private EventTrigger _eventTrigger;

	public ExamineState CurrentState;

	public BASE_CDC ClonedCardView;

	protected IClientLocProvider _locProvider;

	protected AssetLookupSystem _assetLookupSystem;

	protected ICardDataAdapter _sourceModel;

	protected CardHolderType _sourceCardHolder;

	public virtual ExamineState DefaultState { get; protected set; } = ExamineState.Instance;

	public event System.Action Clicked;

	protected virtual bool IsToggled()
	{
		return CurrentState != DefaultState;
	}

	protected virtual void Awake()
	{
		EventTrigger.TriggerEvent triggerEvent = new EventTrigger.TriggerEvent();
		triggerEvent.AddListener(onClick);
		_eventTrigger.triggers.Add(new EventTrigger.Entry
		{
			eventID = EventTriggerType.PointerClick,
			callback = triggerEvent
		});
		SetButtonText();
		SetObjActive(active: false);
		void onClick(BaseEventData eventData)
		{
			this.Clicked?.Invoke();
		}
	}

	public virtual void Init(IClientLocProvider clientLocProvider, AssetLookupSystem assetLookupSystem, BASE_CDC clonedCardView)
	{
		_locProvider = clientLocProvider;
		_assetLookupSystem = assetLookupSystem;
		ClonedCardView = clonedCardView;
		SetObjActive(active: true);
		CurrentState = DefaultState;
	}

	private void OnDestroy()
	{
		this.Clicked = null;
		List<EventTrigger.Entry> triggers = _eventTrigger.triggers;
		foreach (EventTrigger.Entry item in triggers)
		{
			item.callback.RemoveAllListeners();
		}
		triggers.Clear();
	}

	public void Clear()
	{
		CurrentState = ExamineState.None;
		_sourceModel = null;
		SetObjActive(active: false);
		ButtonCheckmarkOn(active: false);
	}

	public void SetText(string text)
	{
		_label.SetText(text);
	}

	public void ButtonCheckmarkOn(bool active)
	{
		if ((bool)_icon)
		{
			_icon.enabled = active;
		}
	}

	public bool IsButtonCheckmarkOn()
	{
		return _icon.enabled;
	}

	public void SetObjActive(bool active)
	{
		if ((bool)base.gameObject && base.gameObject.activeSelf != active)
		{
			base.gameObject.SetActive(active);
		}
	}

	public virtual ExamineState FindNextExamineState()
	{
		ExamineState examineState = CurrentState;
		switch (CurrentState)
		{
		case ExamineState.Instance:
		{
			if (CardUtilities.InstanceHasBaseAbility(_sourceModel, 254u))
			{
				examineState = ExamineState.Specialize;
				break;
			}
			MtgCardInstance instance = _sourceModel.Instance;
			examineState = ((instance == null || instance.MutationChildrenIds.Count <= 0) ? ExamineState.Printing : ExamineState.PrintingWithMutations);
			break;
		}
		case ExamineState.Printing:
		case ExamineState.PrintingWithMutations:
		case ExamineState.Specialize:
			examineState = ExamineState.Instance;
			break;
		default:
			Debug.Log("Unknown ExamineState");
			break;
		}
		CurrentState = examineState;
		return examineState;
	}

	public virtual void SetButtonText()
	{
		SetText(_locProvider.GetLocalizedText(GetButtonTextKey(_sourceModel)));
	}

	private static string GetButtonTextKey(ICardDataAdapter model)
	{
		if (CardUtilities.InstanceHasBaseAbility(model, 254u))
		{
			return "DuelScene/ClientPrompt/ExamineSpecialize";
		}
		if (model != null && model.Instance.MutationChildrenIds.Count > 0)
		{
			return "DuelScene/ClientPrompt/ExamineMutations";
		}
		if (model.HasPerpetualChanges() && !PerpetualChangeUtilities.DoesInstanceOnlyHavePerpetualDifferencesFromPrinting(model))
		{
			return "DuelScene/ClientPrompt/ExaminePerpetual";
		}
		return "DuelScene/ClientPrompt/ExaminePrinting";
	}

	public virtual bool ShouldShowButton(ExamineState currentState, ICardDataAdapter sourceModel, ICardDataAdapter examineModel)
	{
		_sourceModel = sourceModel;
		CurrentState = currentState;
		if (sourceModel == null)
		{
			return false;
		}
		if (_sourceModel.Printing.HasFrameLanguage() && !FrameLanguageUtilities.FrameLanguageMatchesCurrent(_sourceModel.Printing, Languages.CurrentLanguage))
		{
			return true;
		}
		if (sourceModel.Instance.GrpId == 3)
		{
			List<MtgCardInstance> mutationChildren = sourceModel.Instance.MutationChildren;
			if (mutationChildren != null && mutationChildren.Count == 0)
			{
				return false;
			}
		}
		switch (sourceModel.ObjectType)
		{
		case GameObjectType.SplitLeft:
		case GameObjectType.SplitRight:
			return true;
		case GameObjectType.Emblem:
		case GameObjectType.Boon:
			return false;
		case GameObjectType.Ability:
			return sourceModel.ZoneType == ZoneType.Stack;
		default:
		{
			bool num = sourceModel.ZoneType == ZoneType.Stack && (CardUtilities.IsAnyOmenOrAdventureFace(sourceModel.LinkedFaceType) || sourceModel.LinkedFaceType == LinkedFace.PrototypeChild);
			LinkedFace linkedFaceType = sourceModel.LinkedFaceType;
			bool flag = linkedFaceType == LinkedFace.SplitCard || linkedFaceType == LinkedFace.SpecializeChild;
			if (num || flag)
			{
				return true;
			}
			if (sourceModel.Instance.Abilities.Exists((AbilityPrintingData x) => x.Id == sourceModel.GrpId))
			{
				return false;
			}
			return CardUtilities.DoesInstanceDifferMeaningfullyFromPrinting(sourceModel);
		}
		}
	}

	public void SetupToggle()
	{
		bool flag = ShouldShowButton(CurrentState, _sourceModel, ClonedCardView.Model);
		if (!flag)
		{
			CurrentState = ExamineState.None;
			ButtonCheckmarkOn(active: false);
		}
		SetObjActive(flag);
	}

	public void UpdateSourceModel(ICardDataAdapter sourceModel, CardHolderType sourceCardHolder = CardHolderType.Invalid)
	{
		_sourceModel = sourceModel;
		_sourceCardHolder = sourceCardHolder;
	}

	public ICardDataAdapter GetSourceModel()
	{
		return _sourceModel;
	}

	public void LayoutToggleRect(DuelScene_CDC clonedCardView)
	{
		ClonedCardView = clonedCardView;
		RectTransform component = base.transform.parent.GetComponent<RectTransform>();
		if (!(ClonedCardView == null) && _assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<Examine_ToggleRect> loadedTree))
		{
			_assetLookupSystem.Blackboard.Clear();
			_assetLookupSystem.Blackboard.SetCardDataExtensive(ClonedCardView.Model);
			Examine_ToggleRect payload = loadedTree.GetPayload(_assetLookupSystem.Blackboard);
			component.anchoredPosition = payload.Position;
			component.sizeDelta = new Vector2(payload.Width, 0f);
			_assetLookupSystem.Blackboard.Clear();
		}
	}
}
