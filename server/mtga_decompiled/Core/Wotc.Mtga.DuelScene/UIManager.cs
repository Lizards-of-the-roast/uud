using System;
using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using AssetLookupTree.Payloads.UI.DuelScene;
using GreClient.Rules;
using Pooling;
using UnityEngine;
using Wizards.Mtga.Platforms;
using WorkflowVisuals;
using Wotc.Mtga.DuelScene.PlayerNameViews;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class UIManager : IDisposable
{
	private GameManager _gameManager;

	private AutoResponseManager _autoResponseManager;

	private CanvasManager _canvasManager;

	private AssetLookupSystem _assetLookupSystem;

	private SettingsMenuHost _settingsMenuHost;

	private MatchManager _matchManager;

	private IUnityObjectPool _objectPool;

	private IObjectPool _genericPool;

	private NPEState _npeState;

	private IPlayerNameViewManager _playerNameViewManager;

	private StyledButton _primaryButton;

	private StyledButton _secondaryButton;

	private KeyboardToggleButton _undoButton;

	private ButtonPhaseContext _buttonPhaseContext;

	private List<PromptButtonData> _buttonData;

	private NPEPrompt _buttonNotice;

	private Transform _buttonsLayout;

	private UnlocalizedMTGAString _unlocalizedMtgaString = new UnlocalizedMTGAString();

	public PlayerNames PlayerNames { get; private set; }

	public View_UserPrompt UserPrompt { get; private set; }

	public bool ShowEndTurnButton { get; set; } = true;

	public bool ShowPhaseLadder { get; set; } = true;

	public ManaColorSelector ManaColorSelector { get; private set; }

	public ButtonPhaseLadder PhaseLadder { get; private set; }

	public View_TurnChanged TurnChanged { get; private set; }

	public TooltipSystem TooltipSystem { get; private set; }

	public AttackerCost AttackerCost { get; private set; }

	public ConfirmWidget ConfirmWidget { get; private set; }

	public FullControl FullControl { get; private set; }

	public EndTurnButton EndTurnButton { get; private set; }

	public BattleFieldStaticElementsLayout BattleFieldStaticElementsLayout { get; private set; }

	public UIManager(CanvasManager canvasManager, GameManager gameManager, AssetLookupSystem assetLookupSystem, IUnityObjectPool objectPool, IObjectPool genericPool, TooltipSystem tooltipSystem, MatchManager matchManager, SettingsMenuHost settingsMenuHost, NPEState npeState, IPlayerNameViewManager playerNameViewManager)
	{
		_gameManager = gameManager;
		_autoResponseManager = _gameManager.AutoRespManager;
		_assetLookupSystem = assetLookupSystem;
		_canvasManager = canvasManager;
		TooltipSystem = tooltipSystem;
		_objectPool = objectPool;
		_genericPool = genericPool;
		_matchManager = matchManager;
		_settingsMenuHost = settingsMenuHost;
		_npeState = npeState;
		_playerNameViewManager = playerNameViewManager;
		SpawnPrefabs();
	}

	private void SpawnPrefabs()
	{
		Camera mainCamera = _gameManager.MainCamera;
		Transform canvasRoot = _canvasManager.GetCanvasRoot(CanvasLayer.Overlay);
		Transform canvasRoot2 = _canvasManager.GetCanvasRoot(CanvasLayer.ScreenSpace_Default);
		Transform canvasRoot3 = _canvasManager.GetCanvasRoot(CanvasLayer.ScreenSpace_UnderStack);
		Transform canvasRoot4 = _canvasManager.GetCanvasRoot(CanvasLayer.ScreenSpace_UnderStack_ScaledCorrectly);
		Transform canvasRoot5 = _canvasManager.GetCanvasRoot(CanvasLayer.ScreenSpace_Default, "Turn Prompt");
		_assetLookupSystem.Blackboard.Clear();
		DuelSceneUIPrefabs payload = _assetLookupSystem.TreeLoader.LoadTree<DuelSceneUIPrefabs>().GetPayload(_assetLookupSystem.Blackboard);
		BattleFieldStaticElementsLayout = AssetLoader.Instantiate(payload.BattlefieldLayoutRef, canvasRoot2);
		BattleFieldStaticElementsLayout.SetCamera(_gameManager.MainCamera);
		SpawnButtons(canvasRoot2);
		SpawnPromptUI(canvasRoot2);
		SpawnTurnChangedUI(canvasRoot5);
		SettingsButton.Instantiate(_assetLookupSystem, _settingsMenuHost, _matchManager, canvasRoot);
		if (PlatformUtils.IsHandheld() && !PlatformUtils.IsAspectRatio4x3())
		{
			PlayerNames = AssetLoader.Instantiate(payload.PlayerNamesRef, canvasRoot2);
		}
		else
		{
			PlayerNames = AssetLoader.Instantiate(payload.PlayerNamesRef, canvasRoot3);
		}
		PlayerNames.Init(_matchManager, _gameManager, _gameManager.CardDatabase, _npeState, _playerNameViewManager);
		if (PlatformUtils.IsHandheld() && !PlatformUtils.IsAspectRatio4x3())
		{
			PlayerNames.ConfigurePlayerNamesForHandheld16x9(canvasRoot3);
		}
		ManaColorSelector = AssetLoader.Instantiate(payload.ManaColorSelectorRef, canvasRoot4);
		ManaColorSelector.Init(mainCamera, _objectPool, _genericPool, _assetLookupSystem);
		ConfirmWidget = AssetLoader.Instantiate(payload.ConfirmWidgetRef, canvasRoot4);
		ConfirmWidget.Init(mainCamera);
		AttackerCost = AssetLoader.Instantiate(payload.AttackerCostRef, canvasRoot2);
		RefreshLayout();
		ScreenEventController.Instance.OnScreenChanged += RefreshLayout;
	}

	public void RefreshLayout()
	{
		BattleFieldStaticElementsLayout.StartCoroutine(BattleFieldStaticElementsLayout.UpdatePromptButtonsAnchorPosition(_buttonsLayout.GetComponent<RectTransform>()));
		BattleFieldStaticElementsLayout.StartCoroutine(BattleFieldStaticElementsLayout.UpdateUserNamesAnchorPosition(_playerNameViewManager.GetAllPlayerNameDataList()));
		if (FullControl != null)
		{
			BattleFieldStaticElementsLayout.StartCoroutine(BattleFieldStaticElementsLayout.UpdateFullControlAnchorPosition(FullControl.GetComponent<RectTransform>()));
		}
	}

	private void SpawnButtons(Transform screenSpaceDefaultRoot)
	{
		DuelSceneButtonPrefabs payload = _assetLookupSystem.TreeLoader.LoadTree<DuelSceneButtonPrefabs>().GetPayload(_assetLookupSystem.Blackboard);
		_buttonsLayout = AssetLoader.Instantiate(payload.ButtonsLayoutRef, screenSpaceDefaultRoot);
		if (_gameManager.NpeDirector == null)
		{
			if (PlatformUtils.IsHandheld())
			{
				_undoButton = AssetLoader.Instantiate(payload.UndoButtonRef, screenSpaceDefaultRoot);
				FullControl = AssetLoader.Instantiate(payload.FullControlRef, screenSpaceDefaultRoot);
			}
			else
			{
				_undoButton = AssetLoader.Instantiate(payload.UndoButtonRef, _buttonsLayout);
				FullControl = AssetLoader.Instantiate(payload.FullControlRef, _buttonsLayout);
			}
			FullControl.Init(TooltipSystem);
		}
		PhaseLadder = AssetLoader.Instantiate(payload.ButtonPhaseLadderRef, _buttonsLayout);
		_primaryButton = AssetLoader.Instantiate(payload.PrimaryButtonRef, _buttonsLayout);
		_primaryButton.Init(_assetLookupSystem);
		_primaryButton.SetModel(new PromptButtonData
		{
			Tag = ButtonTag.Primary,
			Style = ButtonStyle.StyleType.Waiting
		});
		_secondaryButton = AssetLoader.Instantiate(payload.SecondaryButtonRef, _buttonsLayout);
		_secondaryButton.Init(_assetLookupSystem);
		StyledButton primaryButton = _primaryButton;
		primaryButton.Rollover = (System.Action)Delegate.Combine(primaryButton.Rollover, new System.Action(UpdateNextPhaseHint));
		StyledButton primaryButton2 = _primaryButton;
		primaryButton2.Rollout = (System.Action)Delegate.Combine(primaryButton2.Rollout, new System.Action(UpdateNextPhaseHint));
		StyledButton secondaryButton = _secondaryButton;
		secondaryButton.Rollover = (System.Action)Delegate.Combine(secondaryButton.Rollover, new System.Action(UpdateNextPhaseHint));
		StyledButton secondaryButton2 = _secondaryButton;
		secondaryButton2.Rollout = (System.Action)Delegate.Combine(secondaryButton2.Rollout, new System.Action(UpdateNextPhaseHint));
		_secondaryButton.transform.SetSiblingIndex(_primaryButton.transform.GetSiblingIndex());
		_buttonPhaseContext = AssetLoader.Instantiate(payload.ButtonPhaseContextRef, _buttonsLayout);
		_buttonPhaseContext.Show(visible: false);
		PhaseLadder.ButtonPhaseContext = _buttonPhaseContext;
		EndTurnButton = AssetLoader.Instantiate(payload.ButtonPhaseToEndRef, _buttonsLayout);
	}

	private void SpawnPromptUI(Transform root)
	{
		string prefabPath = _assetLookupSystem.GetPrefabPath<View_UserPromptPrefab, View_UserPrompt>();
		UserPrompt = AssetLoader.Instantiate<View_UserPrompt>(prefabPath, root);
	}

	private void SpawnTurnChangedUI(Transform root)
	{
		string prefabPath = _assetLookupSystem.GetPrefabPath<TurnPromptPayload, View_TurnChanged>();
		TurnChanged = AssetLoader.Instantiate<View_TurnChanged>(prefabPath, root);
	}

	public void Dispose()
	{
		if (_primaryButton != null)
		{
			StyledButton primaryButton = _primaryButton;
			primaryButton.Rollover = (System.Action)Delegate.Remove(primaryButton.Rollover, new System.Action(UpdateNextPhaseHint));
			StyledButton primaryButton2 = _primaryButton;
			primaryButton2.Rollout = (System.Action)Delegate.Remove(primaryButton2.Rollout, new System.Action(UpdateNextPhaseHint));
		}
		if (_secondaryButton != null)
		{
			StyledButton secondaryButton = _secondaryButton;
			secondaryButton.Rollover = (System.Action)Delegate.Remove(secondaryButton.Rollover, new System.Action(UpdateNextPhaseHint));
			StyledButton secondaryButton2 = _secondaryButton;
			secondaryButton2.Rollout = (System.Action)Delegate.Remove(secondaryButton2.Rollout, new System.Action(UpdateNextPhaseHint));
		}
		if (ScreenEventController.Instance != null)
		{
			ScreenEventController.Instance.OnScreenChanged -= RefreshLayout;
		}
		_gameManager = null;
	}

	public void ClearInteractions()
	{
		MtgPlayer activePlayer = _gameManager.LatestGameState.ActivePlayer;
		bool flag = activePlayer != null && !activePlayer.IsLocalPlayer;
		_primaryButton.SetModel(new PromptButtonData
		{
			ButtonText = (flag ? "DuelScene/StartingPlayer/Opponents_Turn" : null),
			Enabled = false,
			Tag = ButtonTag.Primary,
			Style = (flag ? ButtonStyle.StyleType.OpponentsTurn : ButtonStyle.StyleType.Waiting)
		});
		if (_autoResponseManager.ResolveAllEnabled)
		{
			_secondaryButton.SetModel(new PromptButtonData
			{
				ButtonText = "DuelScene/ClientPrompt/ResolveAll",
				Style = ButtonStyle.StyleType.ToggleOn,
				Tag = ButtonTag.ResolveAll,
				ClearsInteractions = false,
				ButtonCallback = delegate
				{
					_autoResponseManager.SetResolveAll(enabled: false);
				}
			});
		}
		else
		{
			_secondaryButton.ResetButton();
		}
		if (_undoButton != null)
		{
			_undoButton.SetToggled(on: false);
			_undoButton.OnToggled.RemoveAllListeners();
		}
		_buttonData = null;
		UpdateNextPhaseHint();
	}

	public void SetWorkflowButtons(Buttons buttons)
	{
		SetUpMainButtons(buttons.WorkflowButtons, buttons.CancelData);
		SetUpUndoButton(buttons.UndoData);
	}

	public void ShowButtons(bool show)
	{
		_primaryButton.SetActive(show);
		PhaseLadder.gameObject.SetActive(show && ShowPhaseLadder);
		EndTurnButton.gameObject.SetActive(ShowEndTurnButton);
		EndTurnButton.SetEnabled(show);
		_secondaryButton.SetActive(_secondaryButton.HasInteraction && show);
	}

	private void SetUpMainButtons(List<PromptButtonData> buttonData, PromptButtonData cancelData)
	{
		_buttonData = buttonData;
		bool flag = cancelData != null;
		bool flag2 = buttonData.Count > 0;
		if (!flag2 && !flag)
		{
			ClearInteractions();
			return;
		}
		PromptButtonData primaryModel = null;
		PromptButtonData secondaryModel = null;
		if (flag2)
		{
			primaryModel = _buttonData[0];
			if (buttonData.Count > 1)
			{
				secondaryModel = _buttonData[1];
			}
			else if (flag)
			{
				secondaryModel = cancelData;
			}
			else if (_autoResponseManager.ResolveAllEnabled)
			{
				secondaryModel = new PromptButtonData
				{
					ButtonText = "DuelScene/ClientPrompt/ResolveAll",
					ButtonCallback = delegate
					{
						_autoResponseManager.SetResolveAll(enabled: false);
					},
					ClearsInteractions = false,
					Style = ButtonStyle.StyleType.ToggleOn,
					Tag = ButtonTag.ResolveAll
				};
			}
		}
		else if (flag)
		{
			primaryModel = cancelData;
			if (_autoResponseManager.ResolveAllEnabled)
			{
				secondaryModel = new PromptButtonData
				{
					ButtonText = "DuelScene/ClientPrompt/ResolveAll",
					ButtonCallback = delegate
					{
						_autoResponseManager.SetResolveAll(enabled: false);
					},
					ClearsInteractions = false,
					Style = ButtonStyle.StyleType.ToggleOn,
					Tag = ButtonTag.ResolveAll
				};
			}
		}
		PromptButtonData promptButtonData = primaryModel;
		promptButtonData.ButtonCallback = (System.Action)Delegate.Combine(promptButtonData.ButtonCallback, (System.Action)delegate
		{
			_gameManager.InteractionSystem.CancelAnyDrag();
			if (primaryModel.ClearsInteractions)
			{
				ClearInteractions();
			}
		});
		_primaryButton.SetModel(primaryModel);
		if (primaryModel.TooltipData != null)
		{
			TooltipSystem.AddDynamicTooltip(_primaryButton.gameObject, primaryModel.TooltipData, new TooltipProperties
			{
				HoverDurationUntilShow = 0f,
				FontSize = 21f,
				Padding = new Vector2(60f, 5f)
			});
		}
		else
		{
			TooltipSystem.RemoveDynamicTooltip(_primaryButton.gameObject);
		}
		if (secondaryModel != null)
		{
			PromptButtonData promptButtonData2 = secondaryModel;
			promptButtonData2.ButtonCallback = (System.Action)Delegate.Combine(promptButtonData2.ButtonCallback, (System.Action)delegate
			{
				_gameManager.InteractionSystem.CancelAnyDrag();
				if (secondaryModel.ClearsInteractions)
				{
					ClearInteractions();
				}
			});
			_secondaryButton.SetModel(secondaryModel);
			if (secondaryModel.TooltipData != null)
			{
				TooltipSystem.AddDynamicTooltip(_secondaryButton.gameObject, secondaryModel.TooltipData, new TooltipProperties
				{
					HoverDurationUntilShow = 0f,
					FontSize = 21f,
					Padding = new Vector2(60f, 5f)
				});
			}
			else
			{
				TooltipSystem.RemoveDynamicTooltip(_secondaryButton.gameObject);
			}
		}
		else
		{
			_secondaryButton.ResetButton();
			TooltipSystem.RemoveDynamicTooltip(_secondaryButton.gameObject);
		}
		UpdateNextPhaseHint();
	}

	private void SetUpUndoButton(PromptButtonData undoData)
	{
		if (!_undoButton)
		{
			return;
		}
		_undoButton.SetToggled(undoData != null);
		_undoButton.OnToggled.RemoveAllListeners();
		if (undoData != null)
		{
			_unlocalizedMtgaString.Key = Languages.ActiveLocProvider.GetLocalizedText("DuelScene/SettingsMenu/Gameplay/Undo");
			_undoButton.SetLabelText(_unlocalizedMtgaString);
			_undoButton.OnToggled.AddListener(delegate
			{
				undoData.ButtonCallback?.Invoke();
			});
		}
	}

	private void UpdateNextPhaseHint()
	{
		if (_secondaryButton.Hover)
		{
			List<PromptButtonData> buttonData = _buttonData;
			if (buttonData != null && buttonData.Count >= 2)
			{
				PhaseLadder.HintNextPhase(_buttonData[1].NextPhase, _buttonData[1].NextStep, _secondaryButton.Hover);
				return;
			}
		}
		List<PromptButtonData> buttonData2 = _buttonData;
		if (buttonData2 != null && buttonData2.Count >= 1)
		{
			PhaseLadder.HintNextPhase(_buttonData[0].NextPhase, _buttonData[0].NextStep, _primaryButton.Hover);
		}
		else
		{
			PhaseLadder.HintNextPhase(Phase.None, Step.None);
		}
	}

	public void SetCanvasInputEnabled(bool enabled)
	{
		_canvasManager.SetCanvasInputEnabled(enabled);
	}

	public void UpdateActivePlayer(GREPlayerNum player)
	{
		ButtonStyle.StyleType style = _primaryButton.Style;
		PromptButtonData promptButtonData = null;
		if (player == GREPlayerNum.Opponent && style == ButtonStyle.StyleType.Waiting)
		{
			promptButtonData = new PromptButtonData
			{
				ButtonText = "DuelScene/StartingPlayer/Opponents_Turn",
				Style = ButtonStyle.StyleType.OpponentsTurn,
				Enabled = false
			};
		}
		else if (player == GREPlayerNum.LocalPlayer && style == ButtonStyle.StyleType.OpponentsTurn)
		{
			promptButtonData = new PromptButtonData
			{
				Style = ButtonStyle.StyleType.Waiting,
				Enabled = false
			};
		}
		if (promptButtonData != null)
		{
			_primaryButton.SetModel(promptButtonData);
		}
		EndTurnButton.UpdateActivePlayer(player);
	}

	public StyledButton GetButtonWithTag(ButtonTag tag)
	{
		StyledButton result = null;
		if (_primaryButton.Tag == tag)
		{
			result = _primaryButton;
		}
		else if (_secondaryButton.Tag == tag)
		{
			result = _secondaryButton;
		}
		return result;
	}

	public StyledButton GetButtonWithStyle(ButtonStyle.StyleType style)
	{
		StyledButton result = null;
		if (_primaryButton.Style == style)
		{
			result = _primaryButton;
		}
		else if (_secondaryButton.Style == style)
		{
			result = _secondaryButton;
		}
		return result;
	}

	public StyledButton GetMainPromptButton()
	{
		return _primaryButton;
	}

	public StyledButton GetSecondaryPromptButton()
	{
		return _secondaryButton;
	}

	public void ShowButtonNotice(MTGALocalizedString text)
	{
		if (_buttonNotice == null)
		{
			_assetLookupSystem.Blackboard.Clear();
			ButtonNoticePrefab payload = _assetLookupSystem.TreeLoader.LoadTree<ButtonNoticePrefab>().GetPayload(_assetLookupSystem.Blackboard);
			_buttonNotice = AssetLoader.Instantiate(payload.ButtonPrompRef, _canvasManager.GetCanvasRoot(CanvasLayer.ScreenSpace_Default));
		}
		_buttonNotice.ShowPop(text);
	}

	public void HideButtonNotice()
	{
		if (_buttonNotice != null)
		{
			_buttonNotice.Hide();
		}
	}
}
