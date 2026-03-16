using System;
using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Blackboard;
using AssetLookupTree.Payloads.Browser;
using AssetLookupTree.Payloads.Prefab;
using GreClient.CardData;
using UnityEngine;
using Wizards.Mtga.Platforms;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.DuelScene.Browsers;
using Wotc.Mtga.DuelScene.VFX;
using Wotc.Mtga.Extensions;

public abstract class BrowserBase : IBrowser
{
	protected readonly IDuelSceneBrowserProvider _duelSceneBrowserProvider;

	protected readonly BrowserManager BrowserManager;

	protected readonly GameManager _gameManager;

	protected readonly IVfxProvider _vfxProvider;

	protected readonly ICardMovementController _cardMovementController;

	private readonly CardHolderReference<StackCardHolder> _stack;

	protected AssetLookupSystem _assetLookupSystem;

	protected StyledButton mainButton;

	protected readonly Dictionary<string, DuelSceneBrowserElementData> uiElementData = new Dictionary<string, DuelSceneBrowserElementData>();

	private List<GameObject> hiddenUIElements = new List<GameObject>();

	protected GameObject _scaffold;

	protected BackgroundVFX _backgroundVfx;

	protected BackgroundSFX _backgroundSfx;

	protected GameObject _instantiatedBackgroundVfx;

	public bool IsClosed { get; protected set; }

	public bool IsVisible { get; protected set; }

	public event Action ClosedHandlers;

	public event Action ShownHandlers;

	public event Action HiddenHandlers;

	public event Action<string> ButtonPressedHandlers;

	public BrowserBase(BrowserManager browserManager, IDuelSceneBrowserProvider provider, GameManager gameManager)
	{
		BrowserManager = browserManager;
		_duelSceneBrowserProvider = provider;
		_gameManager = gameManager;
		if (gameManager != null)
		{
			_vfxProvider = gameManager.VfxProvider;
			_assetLookupSystem = gameManager.AssetLookupSystem;
			_stack = CardHolderReference<StackCardHolder>.Stack(gameManager.CardHolderManager);
			_cardMovementController = gameManager.Context.Get<ICardMovementController>() ?? NullCardMovementController.Default;
		}
	}

	public virtual void Init()
	{
		SpawnBrowserElements();
		InitializeUIElements();
		SetVisibility(visible: true);
		_stack.Get()?.LayoutNow();
	}

	public virtual void OnKeyUp(KeyCode keyCode)
	{
	}

	public virtual Transform GetBrowserRoot()
	{
		return BrowserManager.WorkflowBrowserRootScaledCorrectly;
	}

	public virtual void SetVisibility(bool visible)
	{
		if (IsVisible != visible)
		{
			IsVisible = visible;
			ShowElements(IsVisible);
			if (IsVisible)
			{
				this.ShownHandlers?.Invoke();
			}
			else
			{
				this.HiddenHandlers?.Invoke();
			}
		}
		_stack.Get()?.LayoutNow();
	}

	protected void ShowElements(bool show)
	{
		if ((bool)_instantiatedBackgroundVfx)
		{
			_instantiatedBackgroundVfx.UpdateActive(show);
		}
		if (!show)
		{
			if (_backgroundSfx != null)
			{
				AudioManager.PlayAudio(_backgroundSfx.CloseEvent.AudioEvents, GetBrowserRoot().gameObject);
			}
			if (uiElementData == null)
			{
				return;
			}
			{
				foreach (string key in uiElementData.Keys)
				{
					if (uiElementData[key].CanHide && (bool)uiElementData[key].GameObject && uiElementData[key].GameObject.activeSelf)
					{
						uiElementData[key].GameObject.SetActive(value: false);
						hiddenUIElements.Add(uiElementData[key].GameObject);
					}
				}
				return;
			}
		}
		if (_backgroundSfx != null)
		{
			AudioManager.PlayAudio(_backgroundSfx.OpenEvent.AudioEvents, GetBrowserRoot().gameObject);
		}
		foreach (GameObject hiddenUIElement in hiddenUIElements)
		{
			if ((bool)hiddenUIElement)
			{
				hiddenUIElement.SetActive(value: true);
			}
		}
		hiddenUIElements.Clear();
	}

	public virtual void UpdateButtons()
	{
		UpdateButtons(_duelSceneBrowserProvider.GetButtonStateData());
	}

	protected virtual void UpdateButtons(Dictionary<string, ButtonStateData> buttonStateData)
	{
		mainButton = null;
		if (buttonStateData == null)
		{
			return;
		}
		foreach (string buttonKey in buttonStateData.Keys)
		{
			StyledButton component = uiElementData[buttonStateData[buttonKey].BrowserElementKey].GameObject.GetComponent<StyledButton>();
			component.gameObject.SetActive(buttonStateData[buttonKey].IsActive);
			if (buttonStateData[buttonKey].IsActive)
			{
				if (buttonStateData[buttonKey].StyleType == ButtonStyle.StyleType.Main)
				{
					mainButton = component;
				}
				component.SetModel(new PromptButtonData
				{
					ButtonText = buttonStateData[buttonKey].LocalizedString,
					ButtonIcon = buttonStateData[buttonKey].Sprite,
					Style = buttonStateData[buttonKey].StyleType,
					Enabled = buttonStateData[buttonKey].Enabled,
					ButtonCallback = delegate
					{
						OnButtonCallback(buttonKey);
					}
				});
			}
		}
	}

	protected virtual void OnButtonCallback(string buttonKey)
	{
		if (_backgroundSfx != null)
		{
			AudioManager.PlayAudio(_backgroundSfx.SelectionEvent.AudioEvents, GetBrowserRoot().gameObject);
		}
		this.ButtonPressedHandlers?.Invoke(buttonKey);
	}

	public virtual void Close()
	{
		if (!IsClosed)
		{
			SetVisibility(visible: false);
			ReleaseUIElements();
			IsClosed = true;
			_stack.Get()?.LayoutNow();
			_stack.ClearCache();
			this.ClosedHandlers?.Invoke();
		}
	}

	public StyledButton GetMainButton()
	{
		return mainButton;
	}

	protected virtual void InitializeUIElements()
	{
		foreach (KeyValuePair<string, DuelSceneBrowserElementData> uiElementDatum in uiElementData)
		{
			if (uiElementDatum.Key == "ViewBattlefield" && uiElementDatum.Value.GameObject.TryGetComponent<ViewBattlefieldButton>(out var component))
			{
				component.Init(OnClickViewBattlefield, BrowserManager.IsConsecutiveBrowser);
			}
			StyledButton component2 = uiElementDatum.Value.GameObject.GetComponent<StyledButton>();
			if ((bool)component2)
			{
				component2.Init(_assetLookupSystem);
			}
		}
		UpdateButtons();
		if (_gameManager != null)
		{
			AssetLookupTree<BackgroundVFX> assetLookupTree = _assetLookupSystem.TreeLoader.LoadTree<BackgroundVFX>();
			AssetLookupTree<BackgroundSFX> assetLookupTree2 = _assetLookupSystem.TreeLoader.LoadTree<BackgroundSFX>();
			IBlackboard blackboard = _assetLookupSystem.Blackboard;
			blackboard.Clear();
			blackboard.SetCardDataExtensive(_gameManager.ActiveResolutionEffect?.Model);
			blackboard.Ability = _gameManager.ActiveResolutionEffect?.AbilityPrinting;
			blackboard.CardBrowserType = _duelSceneBrowserProvider.GetBrowserType();
			_duelSceneBrowserProvider.SetFxBlackboardData(blackboard);
			_backgroundVfx = assetLookupTree.GetPayload(blackboard);
			_backgroundSfx = assetLookupTree2.GetPayload(blackboard);
			ICardDataAdapter cardData = blackboard.CardData;
			if (!string.IsNullOrWhiteSpace(_backgroundVfx?.PrefabRef?.RelativePath))
			{
				_instantiatedBackgroundVfx = _vfxProvider.PlayVFX(new VfxData
				{
					ActivationType = VfxActivationType.OneShot,
					IgnoreDedupe = false,
					ParentToSpace = true,
					Offset = _backgroundVfx.Offset,
					PrefabData = new VfxPrefabData
					{
						AllPrefabs = { _backgroundVfx.PrefabRef },
						CleanupAfterTime = 0f,
						SkipSelfCleanup = true
					},
					SpaceData = new SpaceData
					{
						Space = RelativeSpace.Local
					}
				}, cardData, cardData?.Instance, GetBrowserRoot());
			}
		}
	}

	public virtual void SpawnBrowserElements()
	{
		Transform browserRoot = GetBrowserRoot();
		DuelSceneBrowserType browserType = _duelSceneBrowserProvider.GetBrowserType();
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.CardBrowserType = browserType;
		BrowserScaffoldPrefab payload = _assetLookupSystem.TreeLoader.LoadTree<BrowserScaffoldPrefab>(returnNewTree: false).GetPayload(_assetLookupSystem.Blackboard);
		if (payload == null || payload.PrefabPath == null)
		{
			Debug.LogErrorFormat("No BrowserScaffoldPrefab found for CardBrowserType {0}", browserType.ToString());
		}
		_scaffold = AssetLoader.Instantiate(payload.PrefabPath, browserRoot).gameObject;
		_scaffold.transform.ZeroOut();
		SpawnBrowserElements(browserType, _scaffold, PlatformUtils.GetCurrentDeviceType(), (float)Screen.width / (float)Screen.height);
	}

	protected void SpawnBrowserElements(DuelSceneBrowserType browserType, GameObject scaffold, DeviceType deviceType, float aspectRatio)
	{
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.CardBrowserType = browserType;
		_assetLookupSystem.Blackboard.DeviceType = deviceType;
		_assetLookupSystem.Blackboard.AspectRatio = aspectRatio;
		if (_gameManager != null)
		{
			_assetLookupSystem.Blackboard.GameState = _gameManager.CurrentGameState;
			_assetLookupSystem.Blackboard.Interaction = _gameManager.CurrentInteraction;
			_assetLookupSystem.Blackboard.ActiveResolution = _gameManager.ActiveResolutionEffect;
		}
		BrowserElementMarker[] componentsInChildren = scaffold.GetComponentsInChildren<BrowserElementMarker>();
		foreach (BrowserElementMarker marker in componentsInChildren)
		{
			SpawnElement(_assetLookupSystem.Blackboard, marker);
		}
	}

	protected void SpawnElement(IBlackboard blackboard, BrowserElementMarker marker)
	{
		blackboard.CardBrowserElementID = marker.ElementPrefabKey;
		string text = string.Empty;
		GameObject gameObject = null;
		BrowserElementPrefab payload = _assetLookupSystem.TreeLoader.LoadTree<BrowserElementPrefab>(returnNewTree: false).GetPayload(blackboard);
		if (payload != null)
		{
			text = payload.PrefabPath;
		}
		gameObject = (string.IsNullOrEmpty(text) ? UnityEngine.Object.Instantiate(marker.gameObject, marker.transform.parent) : AssetLoader.Instantiate(text, marker.transform.parent));
		gameObject.transform.ZeroOut();
		RectTransform component = gameObject.GetComponent<RectTransform>();
		if ((bool)component)
		{
			RectTransform component2 = marker.GetComponent<RectTransform>();
			if (!component2)
			{
				component.pivot = new Vector2(0.5f, 0.5f);
			}
			else
			{
				component.anchorMin = component2.anchorMin;
				component.anchorMax = component2.anchorMax;
				component.pivot = component2.pivot;
				if (marker.ApplyRectSize)
				{
					component.sizeDelta = component2.sizeDelta;
				}
			}
		}
		gameObject.transform.localPosition = marker.transform.localPosition;
		gameObject.transform.localRotation = marker.transform.localRotation;
		gameObject.transform.localScale = marker.transform.localScale;
		int siblingIndex = marker.transform.GetSiblingIndex();
		gameObject.transform.SetSiblingIndex(siblingIndex);
		if (!Application.isPlaying)
		{
			UnityEngine.Object.DestroyImmediate(marker.gameObject);
		}
		else
		{
			UnityEngine.Object.Destroy(marker.gameObject);
		}
		gameObject.name = marker.BrowserLookupKey;
		uiElementData.Add(marker.BrowserLookupKey, new DuelSceneBrowserElementData(gameObject, marker.CanHide, marker.ElementPrefabKey));
	}

	public void ReleaseBrowserElements()
	{
		if ((bool)_instantiatedBackgroundVfx)
		{
			_gameManager.UnityPool.PushObject(_instantiatedBackgroundVfx);
			_instantiatedBackgroundVfx = null;
		}
		foreach (DuelSceneBrowserElementData value in uiElementData.Values)
		{
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(value.GameObject);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(value.GameObject);
			}
		}
		uiElementData.Clear();
		if (_scaffold != null)
		{
			if (!Application.isPlaying)
			{
				UnityEngine.Object.DestroyImmediate(_scaffold.gameObject);
			}
			else
			{
				UnityEngine.Object.Destroy(_scaffold.gameObject);
			}
			_scaffold = null;
		}
	}

	protected virtual void ReleaseUIElements()
	{
		if (uiElementData != null)
		{
			if (uiElementData.TryGetValue("ViewBattlefield", out var value) && value.GameObject.TryGetComponent<ViewBattlefieldButton>(out var component))
			{
				component.Cleanup();
			}
			ReleaseBrowserElements();
		}
	}

	protected virtual void OnClickViewBattlefield()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept, AudioManager.Default);
		SetVisibility(!IsVisible);
	}

	public GameObject GetBrowserElement(string key)
	{
		if (uiElementData.ContainsKey(key))
		{
			return uiElementData[key].GameObject;
		}
		return null;
	}

	protected void AddBrowserElement(string key, GameObject go, bool canHide, string elementKey = null)
	{
		uiElementData.Add(key, new DuelSceneBrowserElementData(go, canHide, elementKey));
	}

	protected static StyledButton UpdateBasicThreeButtonStates(StyledButton singleButton, StyledButton twoButton_Left, StyledButton twoButton_Right, Action<string> OnButtonPressed, Dictionary<string, ButtonStateData> buttonStateData)
	{
		StyledButton result = null;
		int num = 0;
		if (buttonStateData != null)
		{
			num = buttonStateData.Count;
		}
		switch (num)
		{
		case 0:
			singleButton.SetActive(active: false);
			twoButton_Left.SetActive(active: false);
			twoButton_Right.SetActive(active: false);
			break;
		case 1:
		{
			twoButton_Left.SetActive(active: false);
			twoButton_Right.SetActive(active: false);
			ButtonStateData buttonStateData2 = null;
			string buttonKey = string.Empty;
			foreach (string key in buttonStateData.Keys)
			{
				buttonStateData2 = buttonStateData[buttonKey = key];
			}
			singleButton.SetModel(new PromptButtonData
			{
				ButtonText = buttonStateData2.LocalizedString,
				Style = buttonStateData2.StyleType,
				Enabled = buttonStateData2.Enabled,
				ButtonCallback = delegate
				{
					OnButtonPressed?.Invoke(buttonKey);
				}
			});
			if (buttonStateData2.StyleType == ButtonStyle.StyleType.Main)
			{
				result = singleButton;
			}
			break;
		}
		case 2:
			singleButton.SetActive(active: false);
			twoButton_Left.SetModel(new PromptButtonData
			{
				ButtonText = buttonStateData["DoneButton"].LocalizedString,
				Style = buttonStateData["DoneButton"].StyleType,
				Enabled = buttonStateData["DoneButton"].Enabled,
				ButtonCallback = delegate
				{
					OnButtonPressed?.Invoke("DoneButton");
				}
			});
			if (buttonStateData["DoneButton"].StyleType == ButtonStyle.StyleType.Main)
			{
				result = twoButton_Left;
			}
			twoButton_Right.SetModel(new PromptButtonData
			{
				ButtonText = buttonStateData["CancelButton"].LocalizedString,
				Style = buttonStateData["CancelButton"].StyleType,
				Enabled = buttonStateData["CancelButton"].Enabled,
				ButtonCallback = delegate
				{
					OnButtonPressed?.Invoke("CancelButton");
				}
			});
			if (buttonStateData["CancelButton"].StyleType == ButtonStyle.StyleType.Main)
			{
				result = twoButton_Right;
			}
			break;
		}
		return result;
	}

	public DuelSceneBrowserType GetBrowserType()
	{
		return _duelSceneBrowserProvider.GetBrowserType();
	}

	public bool CanShowStack()
	{
		return GetBrowserElement("HideStack") == null;
	}
}
