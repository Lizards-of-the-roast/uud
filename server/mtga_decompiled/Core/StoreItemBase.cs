using System;
using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using Assets.Core.Meta.MainNavigation.Store.Utils;
using Assets.Core.Shared.Code;
using Core.Meta.MainNavigation.Store;
using Core.Meta.MainNavigation.Store.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Wizards.Mtga.Platforms;
using Wizards.Unification.Models.Mercantile;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Client.Models.Mercantile;
using Wotc.Mtga.CustomInput;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

public class StoreItemBase : MonoBehaviour
{
	public class PurchaseButtonSpecification
	{
		public Client_PurchaseCurrencyType CurrencyType;

		public string Text;

		public string CurrencyId;

		public bool Enabled;
	}

	[Serializable]
	public struct OptionalObject
	{
		public GameObject GameObject;

		public Localize Text;
	}

	[Serializable]
	public class WidgetInfo
	{
		public int SiblingIndex;

		public Vector3 LocalPosition;

		public Transform Parent;

		public bool BrowseButtonActive;

		public bool TooltipActive;

		public Vector2 Size;

		public bool DailyDealTrimActive;

		public Action OnRestore { get; set; }
	}

	[HideInInspector]
	public WidgetInfo _previousWidgetInfo;

	[Header("Purchase Options")]
	[SerializeField]
	private PurchaseCostUtils.PurchaseButton BlueButton;

	[SerializeField]
	private PurchaseCostUtils.PurchaseButton OrangeButton;

	[SerializeField]
	private PurchaseCostUtils.PurchaseButton ClearButton;

	[SerializeField]
	private PurchaseCostUtils.PurchaseButton GreenButton;

	[SerializeField]
	private Transform _displayAnchor;

	[SerializeField]
	private Image _customTokenIcon;

	[SerializeField]
	private Button BackgroundButton;

	[Header("Tags")]
	[SerializeField]
	private Localize Tag_Header;

	[SerializeField]
	private Localize Tag_Badge;

	[SerializeField]
	private Localize Tag_Ribbon;

	[SerializeField]
	private Localize Tag_Footer;

	[SerializeField]
	private Localize Tag_PriceSlash;

	[SerializeField]
	private Localize Tag_Timer;

	[FormerlySerializedAs("_header")]
	[SerializeField]
	private OptionalObject TAG_Heat3;

	[SerializeField]
	private OptionalObject _limit;

	[FormerlySerializedAs("_featureCallout")]
	[SerializeField]
	private OptionalObject TAG_Heat2;

	[SerializeField]
	private OptionalObject TAG_Heat1;

	[SerializeField]
	private OptionalObject _browseButton;

	[SerializeField]
	private Animator _animator;

	[Header("Daily Deal")]
	[SerializeField]
	private Color _dailyDealTextGradientOverride;

	[SerializeField]
	private bool _useDailyDealOverride;

	[SerializeField]
	private Color _dailyDealOverride;

	[SerializeField]
	private GameObject _dailyDealTrim;

	[Header("Misc")]
	public OptionalObject _label;

	public OptionalObject _description;

	public TooltipTrigger _tooltipTrigger;

	public bool _forceToolTipDescriptionOnConfirmation;

	[SerializeField]
	private GameObject _spawnedPurchaseItemContainer;

	[SerializeField]
	private Image _backgroundPanel;

	[SerializeField]
	private TMP_Text _warningLabel;

	[SerializeField]
	private GameObject PromoHighlight;

	[SerializeField]
	private GameObject _sparkyHighlight;

	[SerializeField]
	private GameObject _clickShield;

	[SerializeField]
	private bool _allowHover = true;

	[SerializeField]
	private bool _allowInput = true;

	[SerializeField]
	private Vector3 _buttonScale = Vector3.one;

	[SerializeField]
	private Image _mobileItemTextGradient;

	public Transform ManaSymbolParent;

	public StoreItem _storeItem;

	private SceneObjectBeacon mainSparkyBeacon;

	private Action onFakeGotToDeals;

	private StoreItemDisplay _itemDisplay;

	private StoreItemDisplay _confirmationDisplay;

	private bool _useConfirmationDisplay;

	private bool _hideDiscounts;

	private WwiseEvents _rolloverSound;

	private Client_PurchaseCurrencyType _defaultClickCurrencyType;

	private DateTime? _endTime;

	private Color _defaultColor;

	private const string TagParamIndex = "number1";

	private AssetLoader.AssetTracker<Sprite> _prizeWallTokenImageSpriteTracker;

	private const int UPDATEFREQ = 60;

	private float dtTimer;

	private bool _pointerEntered;

	private static readonly int Purchased = Animator.StringToHash("Purchased");

	private static readonly int Disabled = Animator.StringToHash("Disabled");

	private static readonly int AnimHighlight = Animator.StringToHash("Highlight");

	private static readonly int AllowHover = Animator.StringToHash("AllowHover");

	private static readonly int StoreTagHeader = Animator.StringToHash("StoreTag_Header");

	private static readonly int StoreTagPriceSlash = Animator.StringToHash("StoreTag_PriceSlash");

	private static readonly int StoreTagBadge = Animator.StringToHash("StoreTag_Badge");

	private static readonly int StoreTagRibbon = Animator.StringToHash("StoreTag_Ribbon");

	private static readonly int StoreTagFooter = Animator.StringToHash("StoreTag_Footer");

	private static readonly int MouseOver = Animator.StringToHash("MouseOver");

	private static readonly int Up = Animator.StringToHash("Up");

	private StoreItemDisplay VisibleDisplay
	{
		get
		{
			if (!_useConfirmationDisplay || !_confirmationDisplay)
			{
				return _itemDisplay;
			}
			return _confirmationDisplay;
		}
	}

	public StoreItemDisplay ItemDisplay => _itemDisplay;

	public StoreItemDisplay ItemConfirmationDisplay => _confirmationDisplay;

	public event Action<StoreItem, Client_PurchaseCurrencyType> StoreItemBackgroundClicked;

	private event Action<StoreItem, Client_PurchaseCurrencyType> _purchaseOptionClicked;

	public event Action<StoreItem, Client_PurchaseCurrencyType> PurchaseOptionClicked
	{
		add
		{
			this._purchaseOptionClicked = null;
			_purchaseOptionClicked += value;
		}
		remove
		{
			_purchaseOptionClicked -= value;
		}
	}

	private event Action<StoreItem> _browseButtonClicked;

	public event Action<StoreItem> BrowseButtonClicked
	{
		add
		{
			this._browseButtonClicked = null;
			_browseButtonClicked += value;
		}
		remove
		{
			_browseButtonClicked -= value;
		}
	}

	public event Action WhenDestroyed;

	private void Awake()
	{
		_limit.GameObject.UpdateActive(active: false);
		_browseButton.GameObject.UpdateActive(active: false);
		_defaultColor = ((_backgroundPanel != null) ? _backgroundPanel.color : default(Color));
		if (BlueButton.Button != null)
		{
			BlueButton.Button.OnClick.AddListener(delegate
			{
				OnButtonClicked(Client_PurchaseCurrencyType.Gem);
			});
		}
		if (OrangeButton.Button != null)
		{
			OrangeButton.Button.OnClick.AddListener(delegate
			{
				OnButtonClicked(Client_PurchaseCurrencyType.Gold);
			});
		}
		if (ClearButton.Button != null)
		{
			ClearButton.Button.OnClick.AddListener(delegate
			{
				OnButtonClicked(Client_PurchaseCurrencyType.RMT);
			});
		}
		if (BackgroundButton != null)
		{
			BackgroundButton.onClick.AddListener(delegate
			{
				OnBackgroundElementClicked(Client_PurchaseCurrencyType.Gem);
			});
		}
		if (_browseButton.GameObject != null)
		{
			CustomButton component = _browseButton.GameObject.GetComponent<CustomButton>();
			if (component != null)
			{
				component.OnClick.AddListener(UnityEvent_BrowseButtonClicked);
			}
		}
		SetAllowHover(_allowHover);
		SetAllowInput(allowInput: true);
	}

	private void OnDisable()
	{
		if (BlueButton.ButtonContainer != null)
		{
			BlueButton.ButtonContainer.transform.localScale = _buttonScale;
		}
		if (GreenButton.ButtonContainer != null)
		{
			GreenButton.ButtonContainer.transform.localScale = _buttonScale;
		}
		if (OrangeButton.ButtonContainer != null)
		{
			OrangeButton.ButtonContainer.transform.localScale = _buttonScale;
		}
		if (ClearButton.ButtonContainer != null)
		{
			ClearButton.ButtonContainer.transform.localScale = _buttonScale;
		}
	}

	private void OnDestroy()
	{
		if (GreenButton.Button != null)
		{
			GreenButton.Button.OnClick.RemoveAllListeners();
		}
		if (BlueButton.Button != null)
		{
			BlueButton.Button.OnClick.RemoveAllListeners();
		}
		if (OrangeButton.Button != null)
		{
			OrangeButton.Button.OnClick.RemoveAllListeners();
		}
		if (ClearButton.Button != null)
		{
			ClearButton.Button.OnClick.RemoveAllListeners();
		}
		if (BackgroundButton != null)
		{
			BackgroundButton.onClick.RemoveAllListeners();
		}
		this._purchaseOptionClicked = null;
		this._browseButtonClicked = null;
		AssetLoaderUtils.CleanupImage(_customTokenIcon, _prizeWallTokenImageSpriteTracker);
		this.WhenDestroyed?.Invoke();
	}

	private void Update()
	{
		if (_endTime.HasValue)
		{
			dtTimer += Time.deltaTime;
			if (dtTimer > 60f)
			{
				dtTimer %= 60f;
				UpdateTimer();
			}
		}
		if (!Application.isEditor && PlatformUtils.IsHandheld() && _pointerEntered && CustomInputModule.GetTouchCount() == 0)
		{
			PointerExit();
		}
	}

	private void OnButtonClicked(Client_PurchaseCurrencyType currencyType)
	{
		if (_storeItem.HasRemainingPurchases)
		{
			if (currencyType == Client_PurchaseCurrencyType.RMT)
			{
				AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rewards_gem_tap, base.gameObject);
			}
			this._purchaseOptionClicked?.Invoke(_storeItem, currencyType);
		}
	}

	public void OnBackgroundElementClicked(Client_PurchaseCurrencyType currencyType)
	{
		if (this.StoreItemBackgroundClicked != null)
		{
			this.StoreItemBackgroundClicked(_storeItem, currencyType);
		}
	}

	public GameObject GetSpawnedPurchaseItemContainer()
	{
		return _spawnedPurchaseItemContainer;
	}

	public void OnPressedGeneric()
	{
		if (_defaultClickCurrencyType != Client_PurchaseCurrencyType.None && _allowInput)
		{
			OnButtonClicked(_defaultClickCurrencyType);
		}
	}

	public void UnityEvent_BrowseButtonClicked()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, base.gameObject);
		this._browseButtonClicked?.Invoke(_storeItem);
	}

	public void PointerEnter()
	{
		if (_allowHover && _allowInput)
		{
			AudioManager.PlayAudio(_rolloverSound ?? WwiseEvents.sfx_ui_main_rollover, base.gameObject);
			VisibleDisplay.Hover(on: true);
			_animator.SetTrigger(MouseOver);
			_pointerEntered = true;
		}
	}

	public void PointerClick()
	{
		VisibleDisplay.OnClick();
	}

	public void PointerExit()
	{
		if (_allowHover && _allowInput)
		{
			VisibleDisplay.Hover(on: false);
			_animator.SetTrigger(Up);
			_pointerEntered = false;
		}
	}

	public void SetTimer(DateTime endTime)
	{
		if (endTime != DateTime.MinValue)
		{
			_endTime = endTime;
		}
		UpdateTimer();
	}

	private void UpdateTimer()
	{
		if (!_endTime.HasValue || !Tag_Timer || !(_endTime != DateTime.MinValue))
		{
			return;
		}
		TimeSpan timeSpan = _endTime.Value - ServerGameTime.GameTime;
		if (timeSpan.TotalMilliseconds < 0.0)
		{
			base.gameObject.UpdateActive(active: false);
		}
		else if (timeSpan.Days <= 7)
		{
			string text = string.Empty;
			if (timeSpan.Days > 1)
			{
				text = "MainNav/General/Timers/DD_plural";
			}
			else if (timeSpan.Days == 1)
			{
				text = "MainNav/General/Timers/DD_singular";
			}
			else if (timeSpan.Hours > 1)
			{
				text = "MainNav/General/Timers/HH_plural";
			}
			else if (timeSpan.Hours == 1)
			{
				text = "MainNav/General/Timers/HH_singular";
			}
			else if (timeSpan.Minutes > 1)
			{
				text = "MainNav/General/Timers/MM_plural";
			}
			else if (timeSpan.Minutes == 1)
			{
				text = "MainNav/General/Timers/MM_singular";
			}
			MTGALocalizedString mTGALocalizedString = null;
			if (!string.IsNullOrEmpty(text))
			{
				mTGALocalizedString = new MTGALocalizedString
				{
					Key = text,
					Parameters = new Dictionary<string, string>
					{
						{
							"days",
							timeSpan.Days.ToString()
						},
						{
							"hours",
							timeSpan.Hours.ToString()
						},
						{
							"minutes",
							timeSpan.Minutes.ToString()
						}
					}
				};
			}
			Tag_Timer.gameObject.SetActive(mTGALocalizedString != null);
			Tag_Timer.SetText(mTGALocalizedString);
			if (_animator.GetInteger(StoreTagFooter) != 2)
			{
				_animator.SetInteger(StoreTagFooter, 2);
				MTGALocalizedString text2 = new MTGALocalizedString
				{
					Key = "MainNav/Store/Tags/OfferEndsSoon"
				};
				Tag_Footer.SetText(text2);
			}
		}
	}

	public void SetLabelText(MTGALocalizedString text)
	{
		SetOptionalObjectText(_label, text);
	}

	public void SetDescriptionActiveIfNotEmpty(bool isActive)
	{
		if (!string.IsNullOrWhiteSpace(_description.Text.TextTarget?.LocalizedString?.Key) && !string.IsNullOrWhiteSpace(_description.Text.TextTarget?.LocalizedString))
		{
			_description.GameObject.UpdateActive(isActive);
		}
		else
		{
			_description.GameObject.UpdateActive(active: false);
		}
	}

	public void SetDescriptionText(MTGALocalizedString text)
	{
		SetOptionalObjectText(_description, text);
	}

	public void SetTooltipText(LocalizedString localizedString)
	{
		if (_tooltipTrigger != null)
		{
			_tooltipTrigger.gameObject.UpdateActive(!string.IsNullOrEmpty(localizedString.mTerm));
			_tooltipTrigger.LocString = localizedString;
		}
	}

	private void ShowTooltip()
	{
		if (_tooltipTrigger != null)
		{
			_tooltipTrigger.gameObject.SetActive(value: true);
		}
	}

	public void HideTooltip()
	{
		if (_tooltipTrigger != null)
		{
			_tooltipTrigger.gameObject.SetActive(value: false);
		}
	}

	public void SetHeaderText(MTGALocalizedString text)
	{
		SetOptionalObjectText(TAG_Heat3, text);
	}

	public void SetLimitText(MTGALocalizedString text)
	{
		SetOptionalObjectText(_limit, text);
	}

	public void SetFeatureCalloutText(MTGALocalizedString text)
	{
		if (text?.Key == "MainNav/General/Empty_String")
		{
			text = null;
		}
		SetOptionalObjectText(TAG_Heat2, text);
	}

	private void SetOptionalObjectText(OptionalObject optionalObject, MTGALocalizedString text)
	{
		if (!(optionalObject.GameObject == null))
		{
			if (string.IsNullOrEmpty(text))
			{
				optionalObject.GameObject.UpdateActive(active: false);
				return;
			}
			optionalObject.GameObject.UpdateActive(!string.IsNullOrEmpty(text.Key));
			optionalObject.Text.SetText(text);
		}
	}

	private void SetButton(PurchaseCostUtils.PurchaseButton button)
	{
		if (button.Button != null)
		{
			button.Button.gameObject.UpdateActive(active: false);
			if (button.ButtonContainer != null)
			{
				button.ButtonContainer.transform.localScale = _buttonScale;
			}
		}
	}

	public void SetPurchaseButtons(StoreItem storeItem, AssetLookupSystem assetLookupSystem)
	{
		SetButton(BlueButton);
		SetButton(OrangeButton);
		SetButton(ClearButton);
		SetButton(GreenButton);
		if (storeItem == null)
		{
			return;
		}
		if (!storeItem.HasRemainingPurchases)
		{
			_animator.SetBool(Purchased, value: true);
			PromoHighlight.UpdateActive(active: false);
			SetDailyDealOverride();
			return;
		}
		_animator.SetBool(Purchased, value: false);
		List<PurchaseButtonSpecification> list = PurchaseCostUtils.TransformPurchaseOptions(storeItem).ToList();
		foreach (PurchaseButtonSpecification item in list)
		{
			PurchaseCostUtils.SetPurchaseButtons(item, BlueButton, OrangeButton, ClearButton, GreenButton, Disabled);
			_customTokenIcon.gameObject.SetActive(item.CurrencyType == Client_PurchaseCurrencyType.CustomToken);
			if (item.CurrencyType == Client_PurchaseCurrencyType.CustomToken)
			{
				GreenButton.Button.OnClick.RemoveAllListeners();
				GreenButton.Button.OnClick.AddListener(delegate
				{
					OnButtonClicked(Client_PurchaseCurrencyType.CustomToken);
				});
				if (_prizeWallTokenImageSpriteTracker == null)
				{
					_prizeWallTokenImageSpriteTracker = new AssetLoader.AssetTracker<Sprite>("PrizeWallTokenImageSprite");
				}
				AssetLoaderUtils.TrySetSprite(_customTokenIcon, _prizeWallTokenImageSpriteTracker, PrizeWallUtils.GetTokenImagePath(assetLookupSystem, item.CurrencyId));
			}
			else if (item.CurrencyType == Client_PurchaseCurrencyType.None)
			{
				GreenButton.Button.OnClick.RemoveAllListeners();
				GreenButton.Button.OnClick.AddListener(delegate
				{
					OnButtonClicked(Client_PurchaseCurrencyType.None);
				});
			}
		}
		_defaultClickCurrencyType = ((list.Count == 1 && list[0].Enabled) ? list[0].CurrencyType : Client_PurchaseCurrencyType.None);
	}

	public void SetItem(StoreItem item, bool allowStoreTags = true)
	{
		_storeItem = item;
		_hideDiscounts = false;
		DisableTags();
		_animator.SetInteger(AnimHighlight, 0);
		if (item.StoreSection == EStoreSection.Packs)
		{
			base.gameObject.name = $"StoreItem - {item.PackCount} Packs";
			SetSparkyBeacons($"Packs_{item.PackCount}");
		}
		else
		{
			base.gameObject.name = "StoreItem - " + item.Id;
			SetSparkyBeacons(item.PrefabIdentifier ?? "");
		}
		if (allowStoreTags)
		{
			ResetTags();
		}
		if ((object)_backgroundPanel != null)
		{
			if (_useDailyDealOverride)
			{
				_backgroundPanel.color = _dailyDealOverride;
				if (_mobileItemTextGradient != null)
				{
					_mobileItemTextGradient.color = _dailyDealTextGradientOverride;
				}
				if (item.StoreTags != null)
				{
					foreach (StoreTagUXData storeTag in item.StoreTags)
					{
						DoTagWork(storeTag.Tag, storeTag.Payload);
					}
				}
			}
			else
			{
				_backgroundPanel.color = _defaultColor;
			}
		}
		_dailyDealTrim.UpdateActive(_useDailyDealOverride);
	}

	public void SetDailyDealOverride(bool useDailyDealOverride = false)
	{
		_useDailyDealOverride = useDailyDealOverride;
		if ((object)_backgroundPanel != null)
		{
			if (_useDailyDealOverride)
			{
				_backgroundPanel.color = _dailyDealOverride;
				if (_mobileItemTextGradient != null)
				{
					_mobileItemTextGradient.color = _dailyDealTextGradientOverride;
				}
				if (_storeItem.StoreTags != null)
				{
					foreach (StoreTagUXData storeTag in _storeItem.StoreTags)
					{
						DoTagWork(storeTag.Tag, storeTag.Payload);
					}
				}
			}
			else
			{
				_backgroundPanel.color = _defaultColor;
			}
		}
		_dailyDealTrim.UpdateActive(_useDailyDealOverride);
	}

	private void SetSparkyBeacons(string beaconName)
	{
		if (mainSparkyBeacon == null)
		{
			mainSparkyBeacon = ((_displayAnchor != null) ? _displayAnchor.gameObject.AddComponent<SceneObjectBeacon>() : base.gameObject.AddComponent<SceneObjectBeacon>());
		}
		mainSparkyBeacon.BeaconName = "StoreItem_" + beaconName;
		mainSparkyBeacon.InitializeBeacon();
		if (_sparkyHighlight != null)
		{
			SceneObjectBeacon sceneObjectBeacon = _sparkyHighlight.AddComponent<SceneObjectBeacon>();
			sceneObjectBeacon.BeaconName = "StoreItem_" + beaconName + "_Highlight";
			sceneObjectBeacon.InitializeBeacon();
		}
	}

	private void DisableTags()
	{
		_animator.SetInteger(StoreTagHeader, 0);
		_animator.SetInteger(StoreTagBadge, 0);
		_animator.SetInteger(StoreTagRibbon, 0);
		_animator.SetInteger(StoreTagPriceSlash, 0);
		_animator.SetInteger(StoreTagFooter, 0);
	}

	public void ResetTags()
	{
		if (_storeItem.StoreTags == null)
		{
			return;
		}
		foreach (StoreTagUXData storeTag in _storeItem.StoreTags)
		{
			DoTagWork(storeTag.Tag, storeTag.Payload);
		}
	}

	public void Highlight()
	{
		_animator.SetInteger(AnimHighlight, 1);
	}

	private void DoTagWork(string storeTag, string payload)
	{
		if (string.IsNullOrWhiteSpace(storeTag) || storeTag.Count((char s) => s == '_') != 1)
		{
			return;
		}
		string[] array = storeTag.Split('_');
		if (!int.TryParse(array[1], out var result))
		{
			Debug.LogError("[Store] Unsupported store tag: " + storeTag + " ");
			return;
		}
		switch (array[0])
		{
		case "Header":
			_animator.SetInteger(StoreTagHeader, result);
			Tag_Header.SetText(ParseTokenizedString(payload));
			break;
		case "Badge":
			_animator.SetInteger(StoreTagBadge, result);
			Tag_Badge.SetText(ParseTokenizedString(payload));
			break;
		case "Ribbon":
			_animator.SetInteger(StoreTagRibbon, (!_hideDiscounts) ? result : 0);
			Tag_Ribbon.SetText(ParseTokenizedString(payload));
			break;
		case "PriceSlash":
			_animator.SetInteger(StoreTagPriceSlash, (!_hideDiscounts) ? result : 0);
			switch (result)
			{
			case 1:
				Tag_PriceSlash.SetText((MTGALocalizedString)new UnlocalizedMTGAString
				{
					Key = payload
				});
				break;
			case 2:
				Tag_PriceSlash.SetText((MTGALocalizedString)new UnlocalizedMTGAString
				{
					Key = payload
				});
				break;
			}
			if (VisibleDisplay is StoreDisplayPreconDeck)
			{
				HidePriceSlashIfNoDiscount();
			}
			break;
		case "Footer":
			_animator.SetInteger(StoreTagFooter, result);
			Tag_Footer.SetText(ParseTokenizedString(payload));
			break;
		case "Limit":
		{
			if (int.TryParse(payload, out var result2) && _storeItem.LimitRemaining < result2)
			{
				SetLimitText(new MTGALocalizedString
				{
					Key = "MainNav/Store/PurchaseLimitPurchased",
					Parameters = new Dictionary<string, string>
					{
						{ "total", payload },
						{
							"purchased",
							(result2 - _storeItem.LimitRemaining).ToString()
						}
					}
				});
			}
			else
			{
				SetLimitText(new MTGALocalizedString
				{
					Key = "MainNav/Store/PurchaseLimit",
					Parameters = new Dictionary<string, string> { 
					{
						"total",
						_storeItem.LimitRemaining.ToString()
					} }
				});
			}
			break;
		}
		case "HideDiscount":
			_hideDiscounts = true;
			_animator.SetInteger(StoreTagRibbon, 0);
			_animator.SetInteger(StoreTagPriceSlash, 0);
			break;
		default:
			Debug.LogError("[Store] Unsupported store tag: " + storeTag + " ");
			break;
		}
	}

	private void HidePriceSlashIfNoDiscount()
	{
		if (_storeItem.IsProratedBundle && _storeItem.StoreTags.Exists((StoreTagUXData x) => x.Tag == "PriceSlash_1") && !_storeItem.PurchaseOptions.Find((Client_PurchaseOption x) => x.CurrencyType == Client_PurchaseCurrencyType.Gem).HasDiscount())
		{
			_animator.SetInteger(StoreTagPriceSlash, 0);
		}
	}

	private MTGALocalizedString ParseTokenizedString(string tagPayload)
	{
		MTGALocalizedString mTGALocalizedString = new MTGALocalizedString
		{
			Key = "MainNav/General/Empty_String"
		};
		if (string.IsNullOrEmpty(tagPayload) || tagPayload.Count((char s) => s == ';') > 1)
		{
			return mTGALocalizedString;
		}
		if (!tagPayload.Contains(";"))
		{
			mTGALocalizedString.Key = tagPayload;
			return mTGALocalizedString;
		}
		string[] array = tagPayload.Split(';');
		mTGALocalizedString.Key = array[0];
		mTGALocalizedString.Parameters = new Dictionary<string, string> { 
		{
			"number1",
			array[1]
		} };
		return mTGALocalizedString;
	}

	public void SetRolloverSound(WwiseEvents sound)
	{
		_rolloverSound = sound;
	}

	public void SetZoomHandler(ICardRolloverZoom zoomHandler, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder)
	{
		_itemDisplay.SetZoomHandler(zoomHandler, cardDatabase, cardViewBuilder);
	}

	public void AttachItemDisplay(StoreItemDisplay itemDisplay)
	{
		_itemDisplay = itemDisplay;
		_itemDisplay.transform.SetParent(_displayAnchor, worldPositionStays: false);
		if (_itemDisplay.WideBase && _itemDisplay.WideBaseExtraWidth > 0)
		{
			LayoutElement component = GetComponent<LayoutElement>();
			if (component != null)
			{
				component.preferredWidth += _itemDisplay.WideBaseExtraWidth;
			}
		}
	}

	public void AttachConfirmationItemDisplay(StoreItemDisplay confirmationItemDisplay)
	{
		_confirmationDisplay = confirmationItemDisplay;
		_confirmationDisplay.transform.SetParent(_displayAnchor, worldPositionStays: false);
	}

	public void SetBackgroundColor(Color color)
	{
		color.a = _backgroundPanel.color.a;
		_backgroundPanel.color = color;
	}

	public void SetPreviousWidgetInfo()
	{
		bool activeSelf = _tooltipTrigger.gameObject.activeSelf;
		bool browseButtonActive = _browseButton.GameObject != null && _browseButton.GameObject.activeSelf;
		bool useDailyDealOverride = _useDailyDealOverride;
		RectTransform component = GetComponent<RectTransform>();
		Vector2 size = default(Vector2);
		if (component != null)
		{
			Vector2 sizeDelta = component.sizeDelta;
			size = new Vector2(sizeDelta.x, sizeDelta.y);
		}
		Transform transform = base.transform;
		_previousWidgetInfo = new WidgetInfo
		{
			SiblingIndex = transform.GetSiblingIndex(),
			LocalPosition = transform.localPosition,
			Parent = transform.parent,
			BrowseButtonActive = browseButtonActive,
			TooltipActive = activeSelf,
			Size = size,
			DailyDealTrimActive = useDailyDealOverride
		};
		if ((bool)_confirmationDisplay)
		{
			_useConfirmationDisplay = true;
			_confirmationDisplay.gameObject.UpdateActive(active: true);
			_itemDisplay.gameObject.UpdateActive(active: false);
		}
	}

	public virtual void RestorePreviousWidget(bool turnOffStoreItem)
	{
		Transform obj = base.transform;
		obj.SetParent(_previousWidgetInfo.Parent);
		obj.SetSiblingIndex(_previousWidgetInfo.SiblingIndex);
		obj.localPosition = _previousWidgetInfo.LocalPosition;
		if (_previousWidgetInfo.BrowseButtonActive)
		{
			ShowBrowseButton();
		}
		if (_previousWidgetInfo.TooltipActive)
		{
			ShowTooltip();
		}
		if (_useConfirmationDisplay)
		{
			_useConfirmationDisplay = false;
			_confirmationDisplay.gameObject.UpdateActive(active: false);
			_itemDisplay.gameObject.UpdateActive(active: true);
		}
		_previousWidgetInfo.OnRestore?.Invoke();
		if (turnOffStoreItem)
		{
			base.gameObject.UpdateActive(active: false);
		}
	}

	public void SetAllowInput(bool allowInput)
	{
		_allowInput = allowInput;
		if ((bool)_clickShield)
		{
			_clickShield.SetActive(!allowInput);
		}
	}

	public void ShowBrowseButton(string locKey = null)
	{
		if (_browseButton.GameObject != null)
		{
			_browseButton.GameObject.UpdateActive(active: true);
			if (locKey != null)
			{
				_browseButton.Text.SetText(locKey);
			}
		}
	}

	public void HideBrowseButton()
	{
		_browseButton.GameObject.UpdateActive(active: false);
	}

	public void SetDealsButton(string key, AssetLookupSystem assetLookupSystem, Action onclick = null)
	{
		SetPurchaseButtons(null, assetLookupSystem);
		PromoHighlight.UpdateActive(active: true);
		ClearButton.Button.OnClick.RemoveAllListeners();
		ClearButton.Button.SetText(new MTGALocalizedString
		{
			Key = key
		});
		onFakeGotToDeals = onclick;
		if (ClearButton.Button != null)
		{
			ClearButton.Button.gameObject.UpdateActive(active: true);
			ClearButton.Button.Interactable = true;
			ClearButton.Button.OnClick.AddListener(OneTimeFakeGotToDealsListener);
		}
	}

	private void OneTimeFakeGotToDealsListener()
	{
		onFakeGotToDeals?.Invoke();
		ClearButton.Button.OnClick.RemoveListener(OneTimeFakeGotToDealsListener);
	}

	public void SetWarningText(string localizedText)
	{
		if (string.IsNullOrEmpty(localizedText))
		{
			_warningLabel.gameObject.SetActive(value: false);
			return;
		}
		_warningLabel.gameObject.SetActive(value: true);
		_warningLabel.text = localizedText;
	}

	public void SetAllowHover(bool allowHover, bool lockedInHover = false)
	{
		_allowHover = allowHover;
		if ((bool)_animator)
		{
			_animator.SetBool(AllowHover, allowHover);
		}
		if ((bool)VisibleDisplay)
		{
			VisibleDisplay.Hover(lockedInHover);
		}
	}

	public void InvokeUponRestore(Action action)
	{
		_previousWidgetInfo.OnRestore = action;
	}

	public void DetachItemDisplay()
	{
		_itemDisplay = null;
		_confirmationDisplay = null;
	}
}
