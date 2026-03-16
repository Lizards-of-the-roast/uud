using System;
using UnityEngine;

[Serializable]
public class RewardTrackData
{
	public string TrackName;

	public Sprite BackgroundImage;

	public string PetHitboxTooltipLocString;

	public GameObject ShowcasePetReward;

	public MTGALocalizedString Level1PopupTitle;

	public MTGALocalizedString Level1PopupDescription;

	public NotificationPopupReward PopupPremium3DObject;

	public GameObject Level1CustomPrefab;
}
