using System.Collections.Generic;
using Core.Meta.MainNavigation.SocialV2;
using MTGA.Loc;
using TMPro;
using UnityEngine;
using Wizards.GeneralUtilities.Object_Pooling_Scroll_Rect;
using Wotc.Mtga.Loc;

namespace Wizards.Arena.Gathering;

public sealed class GatheringListItem : MonoBehaviour, ICell
{
	[SerializeField]
	private TextMeshProUGUI _gatheringTitle;

	[SerializeField]
	private Localize _eventsTitleLocalization;

	[SerializeField]
	private MTGALocalizable.LocParam _eventsLocalizationParam;

	[SerializeField]
	private Localize _feedChatTitleLocalization;

	[SerializeField]
	private MTGALocalizable.LocParam _feedChatTitleLocalizationParam;

	[SerializeField]
	private Localize _membersTitleLocalization;

	[SerializeField]
	private MTGALocalizable.LocParam _membersLocalizationParam;

	private Core.Meta.MainNavigation.SocialV2.Gathering _gathering;

	private readonly List<MTGALocalizable.LocParam> _reusableParamsList = new List<MTGALocalizable.LocParam>();

	public void SetGathering(Core.Meta.MainNavigation.SocialV2.Gathering gathering)
	{
		_gathering = gathering;
		_gatheringTitle.SetText(_gathering.Name);
		UpdateEventsTitle();
		UpdateFeedChatTitle();
		UpdateMembersTitle();
	}

	private void UpdateEventsTitle()
	{
		_reusableParamsList.Clear();
		_reusableParamsList.Add(_eventsLocalizationParam);
		_eventsLocalizationParam.value = "5";
		_eventsTitleLocalization.SetText(string.Empty, _reusableParamsList, "Events (" + _eventsLocalizationParam.value + ")");
	}

	private void UpdateFeedChatTitle()
	{
		_reusableParamsList.Clear();
		_reusableParamsList.Add(_feedChatTitleLocalizationParam);
		_feedChatTitleLocalizationParam.value = "5";
		_feedChatTitleLocalization.SetText(string.Empty, _reusableParamsList, "Feed/Chat (" + _feedChatTitleLocalizationParam.value + ")");
	}

	private void UpdateMembersTitle()
	{
		_reusableParamsList.Clear();
		_reusableParamsList.Add(_membersLocalizationParam);
		_membersLocalizationParam.key = "count";
		_membersLocalizationParam.value = _gathering.Players.Count.ToString();
		_membersTitleLocalization.SetText(string.Empty, _reusableParamsList, "Members (" + _membersLocalizationParam.value + ")");
	}
}
