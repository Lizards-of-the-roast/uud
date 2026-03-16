using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Payloads.Booster;
using Assets.Core.Meta.Utilities;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using Wizards.Mtga;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Wrapper;

public class SetAnnouncementController : PopupBase
{
	public CollationMapping NewSetId;

	[SerializeField]
	private VideoClip _cinematicVideo;

	[SerializeField]
	private bool _sendLearnMoreToStore;

	[SerializeField]
	private string _learnMoreLink;

	[Header("Set Logo")]
	[SerializeField]
	private RawImage _setLogoImage;

	[SerializeField]
	private bool _useHeaderLogoFromPayload;

	private readonly AssetLoader.AssetTracker<Texture> _textureTracker = new AssetLoader.AssetTracker<Texture>("SetAnnouncementTextureTracker");

	[Space]
	[SerializeField]
	private GameObject _closer;

	[SerializeField]
	private BoosterVoucherView _boosterVoucherView;

	public static bool HasSeenNewSet(CollationMapping newSetId)
	{
		AccountInformation accountInformation = Pantry.Get<IAccountClient>()?.AccountInformation;
		if (accountInformation == null)
		{
			return true;
		}
		if (newSetId == CollationMapping.None)
		{
			return true;
		}
		string setAnnouncementsViewed = MDNPlayerPrefs.GetSetAnnouncementsViewed(accountInformation.PersonaID);
		int num = (int)newSetId;
		string text = num.ToString();
		string[] array = setAnnouncementsViewed.Split(',');
		for (num = 0; num < array.Length; num++)
		{
			if (array[num] == text)
			{
				return true;
			}
		}
		return false;
	}

	private void OnEnable()
	{
		if (_accountClient?.AccountInformation == null || NewSetId == CollationMapping.None)
		{
			base.gameObject.SetActive(value: false);
			Debug.LogError("SetAnnouncementController shouldn't be enabled before login.");
			return;
		}
		AssetLookupSystem assetLookupSystem = WrapperController.Instance.AssetLookupSystem;
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.BoosterCollationMapping = NewSetId;
		if (assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<Logo> loadedTree))
		{
			Logo payload = loadedTree.GetPayload(assetLookupSystem.Blackboard);
			if (payload != null)
			{
				string path = (_useHeaderLogoFromPayload ? payload.HeaderLogo.RelativePath : payload.TextureRef.RelativePath);
				_setLogoImage.texture = _textureTracker.Acquire(path);
			}
		}
		PlayVideo();
	}

	public void Start()
	{
		if (_boosterVoucherView != null)
		{
			_boosterVoucherView.SetCollation(NewSetId, new List<CollationMapping> { NewSetId });
		}
	}

	public void PlayVideo()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_submit, base.gameObject);
		if (_cinematicVideo != null)
		{
			_closer.SetActive(value: false);
			SceneLoader.GetSceneLoader().PlayVideo(_cinematicVideo, _closer);
		}
	}

	public void LearnMore()
	{
		if (_sendLearnMoreToStore)
		{
			SceneLoader.GetSceneLoader().GoToStore(StoreTabType.Packs, "Set Announcement Learn More Click");
		}
		else
		{
			UrlOpener.OpenURL(Languages.ActiveLocProvider.GetLocalizedText(_learnMoreLink) ?? _learnMoreLink);
		}
		Hide();
	}

	public static void ClearPersistentFlags()
	{
		AccountInformation accountInformation = Pantry.Get<IAccountClient>()?.AccountInformation;
		if (accountInformation == null)
		{
			Debug.LogError("Cannot clear device flags unless logged in");
		}
		else
		{
			MDNPlayerPrefs.ClearSetAnnouncementsViewed(accountInformation.PersonaID);
		}
	}

	public override void OnEnter()
	{
		if (_closer.activeSelf)
		{
			Hide();
		}
	}

	public override void OnEscape()
	{
		if (_closer.activeSelf)
		{
			Hide();
		}
	}

	public void SetAnnouncmentSeen()
	{
		if (_accountClient?.AccountInformation != null && NewSetId != CollationMapping.None)
		{
			string personaID = _accountClient.AccountInformation.PersonaID;
			string setAnnouncementsViewed = MDNPlayerPrefs.GetSetAnnouncementsViewed(personaID);
			string obj = (string.IsNullOrEmpty(setAnnouncementsViewed) ? "" : ",");
			int newSetId = (int)NewSetId;
			setAnnouncementsViewed = obj + newSetId;
			MDNPlayerPrefs.SetSetAnnouncementsViewed(personaID, setAnnouncementsViewed);
		}
	}

	private void OnDestroy()
	{
		SceneLoader.GetSceneLoader()?.DestroyPopup<CinematicVideo>();
		AssetLoaderUtils.CleanupRawImage(_setLogoImage, _textureTracker);
	}
}
