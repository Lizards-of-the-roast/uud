using AssetLookupTree;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.Events;
using Wotc.Mtga.Loc;

namespace EventPage.CampaignGraph.ColorMastery;

public class ColorMasteryBannerItem : MonoBehaviour
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

	private readonly AssetLoader.AssetTracker<Sprite> _imageSpriteTracker = new AssetLoader.AssetTracker<Sprite>("MasteryBannerItemSprite");

	private readonly AssetLoader.AssetTracker<Sprite> _activeIconSpriteTracker = new AssetLoader.AssetTracker<Sprite>("MasteryBannerActiveIconSprite");

	private readonly AssetLoader.AssetTracker<Sprite> _inactiveIconSpriteTracker = new AssetLoader.AssetTracker<Sprite>("MasteryBannerInactiveIconSprite");

	private IColorChallengeTrack _event;

	private bool _active;

	private AudioEvent _vo;

	private static readonly int Active = Animator.StringToHash("Active");

	private static readonly int MeterProgress = Animator.StringToHash("Meter_Progress");

	public string EventName => _event.Name;

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

	public void SetTrack(AssetLookupSystem assetLookupSystem, IColorChallengeTrack track)
	{
		_event = track;
		_text.SetText("Events/Event_Campaign_Title_" + track.Name);
		AssetLoaderUtils.TrySetSprite(_image, _imageSpriteTracker, ClientEventDefinitionList.GetBladeImagePath(assetLookupSystem, track.Name));
		string[] manaIconPaths_Banner = ClientEventDefinitionList.GetManaIconPaths_Banner(assetLookupSystem, track.Name);
		_vo = ClientEventDefinitionList.GetAvatarVO(assetLookupSystem, track.Name);
		AssetLoaderUtils.TrySetSprite(_activeIcon, _activeIconSpriteTracker, manaIconPaths_Banner[0]);
		AssetLoaderUtils.TrySetSprite(_inactiveIcon, _inactiveIconSpriteTracker, manaIconPaths_Banner[1]);
		UpdateEvent();
	}

	public void UpdateEvent()
	{
		if (_animator.isActiveAndEnabled)
		{
			_animator.SetInteger(MeterProgress, _event.UnlockedMatchNodeCount);
		}
	}

	private void OnEnable()
	{
		UpdateEvent();
		SetActive(_active);
	}

	public void SetActive(bool active)
	{
		_active = active;
		if (_animator.isActiveAndEnabled)
		{
			_animator.SetBool(Active, active);
		}
	}

	public void OnDestroy()
	{
		AssetLoaderUtils.CleanupImage(_image, _activeIconSpriteTracker);
		AssetLoaderUtils.CleanupImage(_activeIcon, _activeIconSpriteTracker);
		AssetLoaderUtils.CleanupImage(_inactiveIcon, _inactiveIconSpriteTracker);
		Button.OnClick.RemoveAllListeners();
		Button.OnMouseover.RemoveAllListeners();
		Button.OnMouseover.RemoveAllListeners();
	}
}
