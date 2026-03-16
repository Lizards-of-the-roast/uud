using System.Collections.Generic;
using Core.Code.Promises;
using Core.Meta.MainNavigation.Achievements.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Arena.Promises;
using Wizards.Mtga;
using Wizards.Unification.Models.Graph;
using Wotc.Mtga.Loc;

namespace Core.Meta.MainNavigation.Achievements;

[RequireComponent(typeof(Animator))]
public class AchievementCard : MonoBehaviour
{
	private enum AnimatorCardState
	{
		Disabled,
		InProgress,
		Completed,
		Claimed
	}

	private static readonly int _animatorParameterCardState = Animator.StringToHash("CardState");

	[SerializeField]
	private Localize _title;

	[SerializeField]
	private Localize _description;

	[SerializeField]
	private Image _progressMeter;

	[SerializeField]
	private TextMeshProUGUI _completionAmt;

	[SerializeField]
	private Toggle _favoriteToggle;

	[SerializeField]
	private CustomButton _collectButton;

	[SerializeField]
	private Localize _taglineText;

	[SerializeField]
	private AchievementCardDataView[] _dataViews;

	private Animator _animator;

	private IAchievementManager _achievementManager;

	private IClientAchievement _achievementData;

	private bool _isUpNext;

	private AnimatorCardState CardState
	{
		get
		{
			if (_achievementData.IsCompleted)
			{
				if (!_achievementData.IsClaimed)
				{
					return AnimatorCardState.Completed;
				}
				return AnimatorCardState.Claimed;
			}
			return AnimatorCardState.InProgress;
		}
	}

	public bool CheckIfCardAndAchievementEqual(IClientAchievement achievement)
	{
		return achievement.Id.Equals(_achievementData.Id);
	}

	public void SetAchievementData(IClientAchievement achievementData, bool isUpNext = false)
	{
		_achievementData = achievementData;
		AchievementCardDataView[] dataViews = _dataViews;
		for (int i = 0; i < dataViews.Length; i++)
		{
			dataViews[i]?.AssignAchievementData(_achievementData);
		}
		_isUpNext = isUpNext;
		if (base.isActiveAndEnabled)
		{
			UpdateUI();
		}
	}

	private void Awake()
	{
		_animator = GetComponent<Animator>();
		_achievementManager = Pantry.Get<IAchievementManager>();
	}

	private void Start()
	{
		_favoriteToggle.onValueChanged.AddListener(FavoriteToggled);
		_collectButton.OnClick.AddListener(OnClaimReward);
	}

	private void OnFavoriteAchievementsCollectionUpdated(List<IClientAchievement> favoritedAchievements)
	{
		if (_achievementData != null)
		{
			_favoriteToggle.SetIsOnWithoutNotify(_achievementData.IsFavorite);
		}
	}

	private void FavoriteToggled(bool value)
	{
		if (value != _achievementData.IsFavorite)
		{
			_achievementData.SetFavorite(value);
		}
	}

	private void OnEnable()
	{
		_achievementManager.OnFavoriteAchievementsUpdated += OnFavoriteAchievementsCollectionUpdated;
		if (_achievementData != null)
		{
			UpdateUI();
		}
	}

	private void UpdateUI()
	{
		if (_achievementData != null)
		{
			if (_title != null)
			{
				_title.SetText(_achievementData.TitleLocalizationKey, null, _achievementData.Title);
			}
			if (_description != null)
			{
				_description.SetText(_achievementData.DescriptionLocalizationKey, null, _achievementData.Description ?? "");
			}
			if (_completionAmt != null)
			{
				_completionAmt.SetText($"{_achievementData.CurrentCount}/{_achievementData.MaxCount}");
			}
			if (_favoriteToggle != null)
			{
				_favoriteToggle?.SetIsOnWithoutNotify(_achievementData.IsFavorite);
			}
			_animator.SetInteger(_animatorParameterCardState, (int)CardState);
			if ((bool)_progressMeter)
			{
				_progressMeter.fillAmount = (float)_achievementData.CurrentCount / (float)_achievementData.MaxCount;
			}
			_taglineText.transform.parent.gameObject.SetActive(_isUpNext);
			if (_isUpNext)
			{
				SetTaglineText();
			}
		}
	}

	private void OnClaimReward()
	{
		IClientAchievement achievementData = _achievementData;
		Achievement achievementData2 = achievementData as Achievement;
		if (achievementData2 == null)
		{
			return;
		}
		achievementData2.ClaimAchievement().ThenOnMainThread(delegate(Promise<ClientCampaignGraphState> p)
		{
			if (!p.Successful)
			{
				Debug.LogError($"{achievementData2.Id} reward claim failed: {p.Error}");
			}
			else
			{
				UpdateUI();
			}
		});
	}

	private void SetTaglineText()
	{
		switch (_achievementData.UpNextReason)
		{
		case AchievementUpNextReason.IsFavorited:
			_taglineText.SetText("Achievements/UI/FavoriteFooter");
			break;
		case AchievementUpNextReason.IsOneShot:
			_taglineText.SetText("Achievements/UI/OneshotSuggestion");
			break;
		case AchievementUpNextReason.IsRecentlyProgressed:
			_taglineText.SetText("Achievements/UI/RecentProgress");
			break;
		case AchievementUpNextReason.IsCloseToComplete:
			_taglineText.SetText(((double)((float)_achievementData.CurrentCount / (float)_achievementData.MaxCount) < 0.7) ? "Achievements/UI/GettingThere" : "Achievements/UI/AlmostDone");
			break;
		default:
			_taglineText.transform.parent.gameObject.SetActive(value: false);
			break;
		}
	}

	private void OnDisable()
	{
		_achievementManager.OnFavoriteAchievementsUpdated -= OnFavoriteAchievementsCollectionUpdated;
	}

	private void OnDestroy()
	{
		_favoriteToggle.onValueChanged.RemoveListener(FavoriteToggled);
		_collectButton.OnClick.RemoveListener(OnClaimReward);
	}
}
