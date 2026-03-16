using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using Core.Code.AssetLookupTree.AssetLookup;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Wizards.GeneralUtilities;
using Wizards.Mtga;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

namespace Core.Meta.MainNavigation.Achievements;

public class AchievementGroupDisplay : UIBehaviour
{
	[SerializeField]
	private Localize _titleText;

	[SerializeField]
	private Localize _groupDescriptionText;

	[SerializeField]
	private Image _groupCompletionFillMeter;

	[SerializeField]
	private Image _groupCompletedMark;

	[SerializeField]
	private TextMeshProUGUI _fractionCompletionText;

	[SerializeField]
	[AdditionalInformation("This is the item that should be just above the meta group meter in the hierarchy.")]
	private Transform _olderSiblingOfMetaMeter;

	[SerializeField]
	private GameObject _metaGroupMeter;

	[SerializeField]
	private Transform _achievementCardParent;

	[SerializeField]
	private CustomButton _collectGroupButton;

	[SerializeField]
	private AchievementGroupDisplayAnimationController _groupDisplayAnimationController;

	[SerializeField]
	private Foldout _achievementGroupDisplayFoldout;

	[SerializeField]
	private GameObject _spacer;

	[SerializeField]
	private GameObject _contentSection;

	private List<AchievementCard> _achievementCards;

	private bool _onlyShowClaimableAchievements;

	private string _achievementCardPrefabPath;

	private IClientAchievementGroup _achievementGroup;

	protected override void Awake()
	{
		AssetLookupSystem assetLookupSystem = Pantry.Get<AssetLookupManager>().AssetLookupSystem;
		_achievementCardPrefabPath = AchievementsScreenHelperFunctions.GetPrefabPath(assetLookupSystem, "AchievementScreenCard");
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (_achievementGroup != null)
		{
			_achievementGroup.GroupChanged += UpdateGroupDisplay;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (_achievementGroup != null)
		{
			_achievementGroup.GroupChanged -= UpdateGroupDisplay;
		}
	}

	private void UpdateGroupDisplay()
	{
		_groupCompletionFillMeter.fillAmount = ((_achievementGroup.TotalAchievementCount > 0) ? _achievementGroup.AchievementGroupCompletion : 0f);
		_fractionCompletionText.text = _achievementGroup.ClaimedAchievementCount.ToString("n0") + "/" + _achievementGroup.TotalAchievementCount.ToString("n0");
		_groupDisplayAnimationController.HasReadyToClaimAchievements = _achievementGroup.ClaimableAchievementCount > 0;
	}

	private void SetInitialGroupGraphics()
	{
		_titleText.SetText(_achievementGroup.TitleLocalizationKey, null, _achievementGroup.Title);
		_groupDescriptionText.SetText(_achievementGroup.DescriptionLocalizationKey, null, _achievementGroup.Description);
		if (_achievementGroup.HasGroupRewards)
		{
			_fractionCompletionText.gameObject.SetActive(value: true);
			_groupCompletionFillMeter.gameObject.SetActive(value: true);
			_collectGroupButton?.gameObject.SetActive(_achievementGroup.Achievements.Any((IClientAchievement x) => x.IsCompleted && !x.IsClaimed));
			int siblingIndex = _olderSiblingOfMetaMeter.transform.GetSiblingIndex();
			GroupMetaGoalMeter component = Object.Instantiate(_metaGroupMeter, _olderSiblingOfMetaMeter.transform.parent).GetComponent<GroupMetaGoalMeter>();
			component.transform.SetSiblingIndex(siblingIndex + 1);
			component.AssignAchievementGroup(_achievementGroup);
		}
		else
		{
			_groupCompletionFillMeter.gameObject.SetActive(value: false);
			_fractionCompletionText.gameObject.SetActive(value: false);
			_groupCompletedMark.gameObject.SetActive(value: false);
			_groupCompletionFillMeter.fillAmount = 0f;
		}
		if (_achievementGroup.AchievementSet.AchievementGroups.Count == 1)
		{
			_achievementGroupDisplayFoldout.ToggleFoldout();
		}
		UpdateGroupDisplay();
		_achievementCardParent.DestroyChildren();
		IEnumerable<IClientAchievement> source = _achievementGroup.Achievements;
		if (_onlyShowClaimableAchievements)
		{
			source = source.Where((IClientAchievement x) => x.IsCompleted && !x.IsClaimed);
		}
		_achievementCards = new List<AchievementCard>();
		foreach (IClientAchievement item in source.OrderByDescending((IClientAchievement x) => x.IsClaimable))
		{
			AchievementCard component2 = AssetLoader.Instantiate(_achievementCardPrefabPath, _achievementCardParent).GetComponent<AchievementCard>();
			GameObject gameObject = component2.gameObject;
			gameObject.name = gameObject.name + " (" + item?.Id.GraphId + "." + item?.Id.NodeId + ")";
			component2.SetAchievementData(item);
			_achievementCards.Add(component2);
		}
	}

	internal void AssignAchievementGroup(IClientAchievementGroup achievementGroup, bool onlyShowClaimable = false)
	{
		if (_achievementGroup != null)
		{
			_achievementGroup.GroupChanged -= UpdateGroupDisplay;
		}
		_achievementGroup = achievementGroup;
		_onlyShowClaimableAchievements = onlyShowClaimable;
		if (_achievementGroup != null)
		{
			_achievementGroup.GroupChanged += UpdateGroupDisplay;
		}
		SetInitialGroupGraphics();
	}

	public IClientAchievementGroup GetAchievementGroup()
	{
		return _achievementGroup;
	}

	public AchievementDeeplinkingCalculations ShowDeeplinkedAchievement(IClientAchievement achievement, AchievementDeeplinkingCalculations calculations)
	{
		_achievementGroupDisplayFoldout.ToggleFoldout();
		calculations.CheckIfAnimationDone = _achievementGroupDisplayFoldout.CheckIfFolderFullyOpened;
		foreach (AchievementCard card in _achievementCards)
		{
			if (card.CheckIfCardAndAchievementEqual(achievement))
			{
				calculations.GetYDistanceOfTargetCardInImmediateParent = () => Mathf.Abs(card.GetComponent<RectTransform>().anchoredPosition.y);
				calculations.YdistanceOfTargetCard += _metaGroupMeter.GetComponent<RectTransform>().rect.height;
				calculations.YdistanceOfTargetCard += _spacer.GetComponent<RectTransform>().rect.height;
				calculations.YdistanceOfTargetCard += _contentSection.GetComponent<VerticalLayoutGroup>().padding.top;
				break;
			}
		}
		return calculations;
	}
}
