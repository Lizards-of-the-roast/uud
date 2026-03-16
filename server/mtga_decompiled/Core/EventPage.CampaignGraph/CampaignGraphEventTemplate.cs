using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using Core.Code.Input;
using Core.Shared.Code.Utilities;
using EventPage.Components;
using MTGA.KeyboardManager;
using UnityEngine;
using UnityEngine.UI;
using Wizards.MDN;
using Wizards.Models;
using Wizards.Mtga;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Events;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Providers;

namespace EventPage.CampaignGraph;

public class CampaignGraphEventTemplate : EventTemplate
{
	private IColorChallengePlayerEvent _cachedEvent;

	[SerializeField]
	private Image _backgroundImage;

	[SerializeField]
	private Animator _backgroundAnimator;

	[SerializeField]
	private ModuleLayoutMapping[] _moduleLayoutMappings;

	[SerializeField]
	private List<EventModule> _initiatedModules = new List<EventModule>(10);

	private List<ClientInventoryUpdateReportItem> inventoryUpdates = new List<ClientInventoryUpdateReportItem>(5);

	private CampaignGraphMainButtonModule _cgMainButtonModule;

	[SerializeField]
	private CanvasGroup _modulesCanvas;

	[SerializeField]
	private Transform _inspectDeckModuleParent;

	[SerializeField]
	private float _transitionInTime = 1f;

	[SerializeField]
	private float _transitionOutTime = 0.75f;

	[SerializeField]
	private float _moduleTransitionTime = 0.5f;

	private InspectSingleDeckModule _inspectDeckModule;

	private AssetLoader.AssetTracker<Sprite> _backgroundImageSpriteTracker;

	private AltReferencedMaterial _backgroundMaterialTextureLoader;

	private IColorChallengeStrategy _colorChallengeStrategy;

	private AssetLookupSystem _assetLookupSystem;

	private CosmeticsProvider _cosmetics;

	private static readonly int Active = Animator.StringToHash("Active");

	private static readonly int DissolveOut = Animator.StringToHash("DissolveOut");

	private static readonly int DissolveIn = Animator.StringToHash("DissolveIn");

	private static readonly int FadeIn = Animator.StringToHash("FadeIn");

	public override EventContext EventContext => Pantry.Get<EventManager>().ColorMasteryEventContext;

	private IColorChallengePlayerEvent Event
	{
		get
		{
			if (!(EventContext?.PlayerEvent is IColorChallengePlayerEvent cachedEvent))
			{
				return _cachedEvent;
			}
			return _cachedEvent = cachedEvent;
		}
	}

	public string CurrentTrackName => _colorChallengeStrategy.CurrentTrackName;

	public override void Hide()
	{
		base.gameObject.SetActive(value: false);
		foreach (EventModule initiatedModule in _initiatedModules)
		{
			initiatedModule.Hide();
		}
	}

	private void OnEnable()
	{
		WrapperController.Instance.InventoryManager.Subscribe(InventoryUpdateSource.EventReward, OnInventoryUpdated);
	}

	private void OnDisable()
	{
		WrapperController.Instance.InventoryManager.UnSubscribe(InventoryUpdateSource.EventReward, OnInventoryUpdated);
	}

	public override void DisableMainButton()
	{
		if ((bool)_cgMainButtonModule)
		{
			_cgMainButtonModule.SetPlayButtonState(PlayButtonState.Disabled);
		}
	}

	public override void Init(AssetLookupSystem assetLookupSystem, KeyboardManager keyboardManager, IActionSystem actionSystem, CosmeticsProvider cosmetics, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder)
	{
		_assetLookupSystem = assetLookupSystem;
		_cosmetics = cosmetics;
		_colorChallengeStrategy = Pantry.Get<IColorChallengeStrategy>();
		string prefabPath = _assetLookupSystem.GetPrefabPath<InspectSingleDeckModulePrefab, InspectSingleDeckModule>();
		_inspectDeckModule = AssetLoader.Instantiate<InspectSingleDeckModule>(prefabPath, _inspectDeckModuleParent);
		LossHintModule item = AssetLoader.Instantiate<LossHintModule>(_assetLookupSystem.GetPrefabPath<LossHintModulePrefab, LossHintModule>(), _modulesCanvas.transform);
		_initiatedModules.Add(item);
		foreach (EventModule initiatedModule in _initiatedModules)
		{
			initiatedModule.Init(this, keyboardManager, actionSystem, _assetLookupSystem, cardDatabase, cardViewBuilder);
			if (base.EventTrackModule == null && initiatedModule.GetType().IsSubclassOf(typeof(EventTrackModule)))
			{
				base.EventTrackModule = (EventTrackModule)initiatedModule;
			}
		}
		ModuleLayoutMapping[] moduleLayoutMappings = _moduleLayoutMappings;
		for (int i = 0; i < moduleLayoutMappings.Length; i++)
		{
			ModuleLayoutMapping moduleLayoutMapping = moduleLayoutMappings[i];
			foreach (EventModule eventModule2 in moduleLayoutMapping.EventModules)
			{
				EventModule eventModule = Object.Instantiate(eventModule2, moduleLayoutMapping.LayoutGroup.transform);
				eventModule.Init(this, keyboardManager, actionSystem, _assetLookupSystem, cardDatabase, cardViewBuilder);
				_initiatedModules.Add(eventModule);
				if (base.EventTrackModule == null && eventModule.GetType().IsSubclassOf(typeof(EventTrackModule)))
				{
					base.EventTrackModule = (EventTrackModule)eventModule;
				}
			}
		}
		_backgroundImage.material = Object.Instantiate(_backgroundImage.material);
		_cgMainButtonModule = _initiatedModules.FirstOrDefault((EventModule m) => m is CampaignGraphMainButtonModule) as CampaignGraphMainButtonModule;
		_cgMainButtonModule.OnStartButtonClicked += _MainButtonModule_OnStartButtonClicked;
		_inspectDeckModule.Init(this, keyboardManager, actionSystem, _assetLookupSystem, cardDatabase, cardViewBuilder);
	}

	private void _MainButtonModule_OnStartButtonClicked()
	{
		_cosmetics.SetAvatarBasedOnEvent(Event.CurrentTrack.Name);
	}

	protected override void SetProgressBarStateInternal(EventPageStates progressBarState)
	{
		switch (progressBarState)
		{
		case EventPageStates.DisplayQuest:
			if (base.EventTrackModule != null)
			{
				base.EventTrackModule.Hide();
			}
			StartCoroutine(base.EventPage.Coroutine_ShowQuestBar());
			break;
		case EventPageStates.ClaimQuestRewards:
			if (!base.EventPage.RewardsPanel.Visible)
			{
				SetProgressBarState(EventPageStates.DisplayEvent);
			}
			else
			{
				_cgMainButtonModule.SetPlayButtonState(PlayButtonState.Disabled);
			}
			break;
		case EventPageStates.DisplayEvent:
		{
			base.EventPage.QuestProgressBar.Hide();
			if (base.EventTrackModule != null && !base.EventTrackModule.gameObject.activeSelf)
			{
				base.EventTrackModule.Show();
			}
			PostMatchContext postMatchContext = EventContext.PostMatchContext;
			if (postMatchContext != null && postMatchContext.WonGame)
			{
				SetProgressBarState(EventPageStates.ClaimEventRewards);
				break;
			}
			LateUpdateTemplate();
			EventContext.PostMatchContext = null;
			break;
		}
		case EventPageStates.ClaimEventRewards:
			if (inventoryUpdates.Count > 0 || base.EventPage.RewardsPanel.Visible)
			{
				base.EventPage.RewardsPanel.RegisterRewardWillCloseCallback(OnEventRewardsPanelClosed);
				base.EventPage.RewardsPanel.AddAndDisplayRewardsCoroutine(inventoryUpdates, Languages.ActiveLocProvider.GetLocalizedText("MainNav/Rewards/Rewards_Title"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/Rewards/EventRewards/ClaimPrizeButton"));
			}
			else
			{
				OnEventRewardsPanelClosed();
			}
			break;
		}
	}

	protected override void OnEventRewardsPanelClosed()
	{
		LateUpdateTemplate();
		inventoryUpdates.Clear();
		_colorChallengeStrategy.GoToNextNode();
		EventContext.PostMatchContext = null;
		UpdateModules();
	}

	private void OnInventoryUpdated(ClientInventoryUpdateReportItem inventoryUpdate)
	{
		inventoryUpdates.Add(inventoryUpdate);
	}

	public override void Show()
	{
		base.gameObject.SetActive(value: true);
		if (_backgroundImageSpriteTracker == null)
		{
			_backgroundImageSpriteTracker = new AssetLoader.AssetTracker<Sprite>("CampaignGraphBackgroundImageSprite");
		}
		AssetLoaderUtils.TrySetSprite(_backgroundImage, _backgroundImageSpriteTracker, GetBackgroundPath(_assetLookupSystem));
		if (_backgroundMaterialTextureLoader == null)
		{
			_backgroundMaterialTextureLoader = new AltReferencedMaterial(_backgroundImage.materialForRendering);
		}
		_backgroundMaterialTextureLoader.SetTexture("_Noise", GetDissolveNoiseTexturePath(_assetLookupSystem));
		ShowModules();
		UpdateTemplate();
	}

	public string GetBackgroundPath(AssetLookupSystem assetLookupSystem)
	{
		return ClientEventDefinitionList.GetBackgroundImagePath(assetLookupSystem, _colorChallengeStrategy.CurrentTrackName);
	}

	public string GetDissolveNoiseTexturePath(AssetLookupSystem assetLookupSystem)
	{
		return ClientEventDefinitionList.GetDissolveNoiseTexturePath(assetLookupSystem, _colorChallengeStrategy.CurrentTrackName);
	}

	public override void UpdateTemplate()
	{
		UpdateModules();
		if (EventContext.PostMatchContext != null && (EventContext.PlayerEvent.EventInfo.UpdateQuests || EventContext.PlayerEvent.EventInfo.UpdateDailyWeeklyRewards))
		{
			SetProgressBarState(EventPageStates.DisplayQuest);
		}
		else
		{
			SetProgressBarState(EventPageStates.DisplayEvent);
		}
	}

	public void LateUpdateTemplate()
	{
		foreach (EventModule initiatedModule in _initiatedModules)
		{
			initiatedModule.LateUpdateModule();
		}
	}

	protected override void ShowModules()
	{
		foreach (EventModule initiatedModule in _initiatedModules)
		{
			initiatedModule.Show();
		}
		if (Event.InPlayingMatchesModule)
		{
			_inspectDeckModule.Hide();
			return;
		}
		_inspectDeckModule.Show();
		_inspectDeckModule.transform.SetAsLastSibling();
	}

	public override void UpdateModules()
	{
		foreach (EventModule initiatedModule in _initiatedModules)
		{
			initiatedModule.UpdateModule();
		}
		if (Event.InPlayingMatchesModule)
		{
			_inspectDeckModule.Hide();
		}
	}

	public override IEnumerator PlayAnimation(EventTemplateAnimation anim)
	{
		switch (anim)
		{
		case EventTemplateAnimation.Outro:
		{
			_backgroundAnimator.SetBool(Active, value: false);
			_backgroundAnimator.SetTrigger(DissolveOut);
			_modulesCanvas.interactable = false;
			float timer = 0f;
			while (timer < _transitionOutTime)
			{
				_backgroundImage.materialForRendering.SetFloat(ShaderPropertyIds.DissolveAmountPropId, Mathf.Lerp(0.5f, 1f, timer / _transitionOutTime));
				_modulesCanvas.alpha = 1f - timer / _transitionOutTime;
				timer += Time.deltaTime;
				yield return null;
			}
			break;
		}
		case EventTemplateAnimation.Intro:
		{
			_backgroundAnimator.SetBool(Active, Event.InPlayingMatchesModule);
			_backgroundAnimator.SetTrigger(Event.InPlayingMatchesModule ? DissolveIn : FadeIn);
			float timer = 0f;
			while (timer < _transitionInTime)
			{
				if (Event.InPlayingMatchesModule)
				{
					_backgroundImage.materialForRendering.SetFloat(ShaderPropertyIds.DissolveAmountPropId, Mathf.Lerp(1f, 0.5f, timer / _transitionInTime));
				}
				_modulesCanvas.alpha = timer / _transitionInTime;
				timer += Time.deltaTime;
				yield return null;
			}
			_modulesCanvas.interactable = true;
			break;
		}
		case EventTemplateAnimation.SetActive:
			_backgroundAnimator.SetBool(Active, value: true);
			break;
		case EventTemplateAnimation.ModuleIntro:
		case EventTemplateAnimation.ModuleOutro:
			_inspectDeckModule.PlayAnimation(anim);
			foreach (EventModule initiatedModule in _initiatedModules)
			{
				initiatedModule.PlayAnimation(anim);
			}
			yield return new WaitForSeconds(_moduleTransitionTime);
			break;
		}
	}

	public void OnDestroy()
	{
		AssetLoaderUtils.CleanupImage(_backgroundImage, _backgroundImageSpriteTracker);
		_backgroundMaterialTextureLoader?.Cleanup();
	}
}
