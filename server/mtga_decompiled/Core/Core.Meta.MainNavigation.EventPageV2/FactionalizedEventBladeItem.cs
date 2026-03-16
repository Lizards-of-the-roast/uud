using AssetLookupTree.Payloads.Event;
using EventPage;
using UnityEngine;
using UnityEngine.UI;
using Wizards.MDN;
using Wizards.Unification.Models.Events;
using Wotc.Mtga.Loc;

namespace Core.Meta.MainNavigation.EventPageV2;

public class FactionalizedEventBladeItem : MonoBehaviour
{
	[SerializeField]
	private Image _image;

	[SerializeField]
	private Animator _animator;

	[SerializeField]
	private Image _activeIcon;

	[SerializeField]
	private Image _inactiveIcon;

	[SerializeField]
	private Localize _text;

	public CustomButton Button;

	private AssetLoader.AssetTracker<Sprite> _bannerAssetTracker;

	private AudioEvent _vo;

	private SharedEventPageClasses _sharedClasses;

	private static readonly int Active = Animator.StringToHash("Active");

	public string FactionName { get; set; }

	private void Start()
	{
		Button.OnClick.AddListener(delegate
		{
			if (_vo != null)
			{
				AudioManager.PlayAudio(_vo.WwiseEventName, base.gameObject, 0.05f);
			}
		});
	}

	public void Init(EventContext context, FactionSealedUXInfo faction, SharedEventPageClasses sharedClasses)
	{
		FactionName = faction.FactionInternalName;
		_sharedClasses = sharedClasses;
		_text.SetText(faction.FactionSelectShortNameLoc, null, faction.FactionInternalName);
		if (FactionalizedEventUtils.TryFetchFactionalizedEvent_BannerPayload(context, FactionName, _sharedClasses.AssetLookupSystem, out var payload))
		{
			SetBanner(payload);
		}
		else
		{
			_sharedClasses.Logger.Error(FactionName + " is missing an event banner!");
		}
	}

	public void SetActive(bool active)
	{
		if (_animator.isActiveAndEnabled)
		{
			_animator.SetBool(Active, active);
		}
	}

	public void SetBanner(BannerPayload payload)
	{
		if (_bannerAssetTracker == null)
		{
			_bannerAssetTracker = new AssetLoader.AssetTracker<Sprite>("FactionalizedEvent_Banner_" + FactionName);
		}
		AssetLoaderUtils.TrySetSprite(_image, _bannerAssetTracker, payload.Reference.RelativePath);
	}

	public void OnDestroy()
	{
		AssetLoaderUtils.CleanupImage(_image, _bannerAssetTracker);
		Button.OnClick.RemoveAllListeners();
		Button.OnMouseover.RemoveAllListeners();
		Button.OnMouseover.RemoveAllListeners();
	}
}
