using System;
using System.Linq;
using AssetLookupTree;
using AssetLookupTree.Payloads.Booster;
using Assets.Core.Shared.Code;
using Core.Code.AssetLookupTree.AssetLookup;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Wizards.GeneralUtilities.Object_Pooling_Scroll_Rect;
using Wizards.Mtga;
using Wotc.Mtga.Loc;

namespace Core.Meta.MainNavigation.Achievements;

[DisallowMultipleComponent]
public sealed class AchievementSetItem : MonoBehaviour, ICell
{
	private const string _summaryButtonLocalizationKey = "Achievements/UI/SummaryButtonText";

	private static AchievementSetItem _currentlySelected;

	[SerializeField]
	private BladeSetAnimationController _bladeSetAnimationController;

	[SerializeField]
	private GameObject _achievementHub;

	[SerializeField]
	private TextMeshProUGUI[] _setTitle;

	[SerializeField]
	private RawImage _setLogo;

	[SerializeField]
	private AspectRatioFitter _logoRatio;

	[SerializeField]
	private Texture _defaultLogo;

	[SerializeField]
	[FormerlySerializedAs("_setExpansionSymbol")]
	private Image _setIcon;

	[SerializeField]
	private Sprite _summaryIcon;

	[SerializeField]
	private AchievementEndTimeDisplay _endTimeDisplay;

	private AchievementGroupsController _achievementGroupsController;

	private IClientAchievementSet _clientAchievementSet;

	private AssetLookupSystem _assetLookupSystem;

	private RawImageReferenceLoader _rawImageReferenceLoader;

	private ImageReferenceLoader _imageReferenceLoader;

	private IClientLocProvider _localizationProvider;

	private bool? _showDisplay;

	private static event Action<AchievementSetItem> _AchievementSetSelected;

	private void Awake()
	{
		_AchievementSetSelected += On_AchievementSetSelected;
		_assetLookupSystem = Pantry.Get<AssetLookupManager>().AssetLookupSystem;
		_rawImageReferenceLoader = new RawImageReferenceLoader(_setLogo);
		_imageReferenceLoader = new ImageReferenceLoader(_setIcon);
		_localizationProvider = Pantry.Get<IClientLocProvider>();
	}

	private void OnEnable()
	{
		SetBladeItemStatus();
		_showDisplay = null;
		SetGraphExpirationTime();
	}

	private void OnDestroy()
	{
		_AchievementSetSelected -= On_AchievementSetSelected;
		if (_clientAchievementSet != null)
		{
			_clientAchievementSet.SetChanged -= OnAchievementSetChanged;
		}
	}

	private void On_AchievementSetSelected(AchievementSetItem achievementSetItemSelected)
	{
		if (_bladeSetAnimationController != null)
		{
			_bladeSetAnimationController.SetSelected(achievementSetItemSelected == this);
		}
	}

	public void SelectSet(bool overrideCurrentlySelectedCheck = false)
	{
		if (overrideCurrentlySelectedCheck || !(_currentlySelected == this))
		{
			_currentlySelected = this;
			AchievementSetItem._AchievementSetSelected?.Invoke(this);
			_achievementGroupsController.InitializeWithSet(_clientAchievementSet?.AchievementGroups);
		}
	}

	public AchievementDeeplinkingCalculations SelectSetIfItsTheRightAchievement(IClientAchievement achievement)
	{
		AchievementDeeplinkingCalculations result = new AchievementDeeplinkingCalculations();
		if (_clientAchievementSet != null && achievement.AchievementGroup.AchievementSet.GraphId == _clientAchievementSet.GraphId)
		{
			SelectSet(overrideCurrentlySelectedCheck: true);
			result = _achievementGroupsController.InitializeWithDeeplinkedAchievement(achievement);
		}
		return result;
	}

	public void ConfigureCell(IClientAchievementSet set, AchievementGroupsController achievementGroupsController)
	{
		if (_clientAchievementSet != null)
		{
			_clientAchievementSet.SetChanged -= OnAchievementSetChanged;
		}
		_clientAchievementSet = set;
		_achievementGroupsController = achievementGroupsController;
		if (_clientAchievementSet != null)
		{
			_clientAchievementSet.SetChanged += OnAchievementSetChanged;
		}
		TextMeshProUGUI[] setTitle = _setTitle;
		for (int i = 0; i < setTitle.Length; i++)
		{
			setTitle[i].text = ((_clientAchievementSet == null) ? _localizationProvider.GetLocalizedText("Achievements/UI/SummaryButtonText") : _clientAchievementSet.Title);
		}
		if (_bladeSetAnimationController != null)
		{
			_bladeSetAnimationController.SetLogoLoaded(logoLoaded: false);
		}
		SetBladeItemStatus();
		LoadSetLogoData();
		LoadSetExpansionSymbolData();
		SetGraphExpirationTime();
	}

	private void OnAchievementSetChanged()
	{
		SetBladeItemStatus();
	}

	private void SetBladeItemStatus()
	{
		if (!(_bladeSetAnimationController == null) && _clientAchievementSet != null)
		{
			_bladeSetAnimationController.SetBladeStatus((!_clientAchievementSet.AchievementGroups.Any((IClientAchievementGroup x) => x.ClaimableAchievementCount > 0)) ? BladeSetAnimationController.Status.Default : BladeSetAnimationController.Status.ReadyToClaim);
		}
	}

	private void LoadSetExpansionSymbolData()
	{
		if (_setIcon == null)
		{
			return;
		}
		if (_clientAchievementSet == null)
		{
			_setIcon.sprite = _summaryIcon;
			return;
		}
		_assetLookupSystem.Blackboard.Clear();
		Sprite achievementSetIcon = _clientAchievementSet.GetAchievementSetIcon(_assetLookupSystem);
		if (!(achievementSetIcon == null))
		{
			if (_imageReferenceLoader == null)
			{
				_imageReferenceLoader = new ImageReferenceLoader(_setIcon);
			}
			_imageReferenceLoader.SetSprite(achievementSetIcon);
		}
	}

	private void LoadSetLogoData()
	{
		if (_setLogo == null || _clientAchievementSet == null)
		{
			return;
		}
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.BoosterCollationMapping = _clientAchievementSet.ExpansionCode;
		if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<Logo> loadedTree))
		{
			Logo payload = loadedTree.GetPayload(_assetLookupSystem.Blackboard);
			if (payload != null)
			{
				if (_rawImageReferenceLoader == null)
				{
					_rawImageReferenceLoader = new RawImageReferenceLoader(_setLogo);
				}
				_rawImageReferenceLoader.SetTexture(payload.TextureRef.RelativePath);
			}
			else
			{
				_setLogo.texture = _defaultLogo;
			}
		}
		_logoRatio.aspectRatio = (float)_setLogo.texture.width / (float)_setLogo.texture.height;
		_bladeSetAnimationController?.SetLogoLoaded(logoLoaded: true);
	}

	private void SetGraphExpirationTime()
	{
		if (_clientAchievementSet != null && !(_clientAchievementSet.EndTime == default(DateTime)) && !(_clientAchievementSet.EndRevealTime == default(DateTime)))
		{
			bool flag = _clientAchievementSet.EndRevealTime <= ServerGameTime.GameTime && _clientAchievementSet.EndTime >= ServerGameTime.GameTime;
			_endTimeDisplay.Init(flag, _clientAchievementSet.EndTime, _localizationProvider);
			if (flag != _showDisplay)
			{
				_showDisplay = flag;
				_bladeSetAnimationController.SetTimer(flag);
				_endTimeDisplay.enabled = flag;
			}
		}
	}
}
