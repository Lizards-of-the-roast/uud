using AssetLookupTree;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Unification.Models.Mercantile;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

public class AvatarSelection : MonoBehaviour
{
	public Image BustImage;

	public CustomButton Button;

	private Animator _animator;

	private bool _locked;

	private bool _default;

	private AssetLoader.AssetTracker<Sprite> _bustImageSpriteTracker = new AssetLoader.AssetTracker<Sprite>("AvatarSelectionBustSprite");

	private AssetLookupSystem _assetLookupSystem;

	private bool _awakeCalled;

	private static readonly int _accountDefault = Animator.StringToHash("AccountDefault");

	public string Id { get; private set; }

	public string ListingId { get; private set; }

	public string FullSpritePath { get; private set; }

	public LocalizedString NameString { get; private set; }

	public LocalizedString BioString { get; private set; }

	public AudioEvent VO { get; private set; }

	public EStoreSection StoreSection { get; private set; }

	public void Awake()
	{
		_awakeCalled = true;
	}

	public void Initialize(AssetLookupSystem assetLookupSystem)
	{
		_assetLookupSystem = assetLookupSystem;
	}

	public bool IsLocked()
	{
		return _locked;
	}

	public void SetToggleTrigger(bool on)
	{
		SetTrigger("Toggle", on);
	}

	public void SetLockedTrigger(bool on)
	{
		_locked = on;
		SetTrigger("Locked", on);
	}

	public void SetDefaultTrigger(bool on)
	{
		_default = on;
		SetTrigger("AccountDefault", on);
	}

	private void SetTrigger(string triggerName, bool on)
	{
		if (_animator == null)
		{
			_animator = GetComponent<Animator>();
		}
		if (_animator.isActiveAndEnabled)
		{
			_animator.SetTrigger(triggerName, on);
		}
	}

	public void SetAvatar(string avatarId, bool owned, EStoreSection storeSection, string listingId = null)
	{
		if (!_awakeCalled && !base.gameObject.activeInHierarchy)
		{
			Transform parent = base.transform.parent;
			base.transform.SetParent(null, worldPositionStays: false);
			if (!base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(value: true);
				base.gameObject.SetActive(value: false);
			}
			base.transform.SetParent(parent, worldPositionStays: false);
		}
		ListingId = listingId;
		Id = avatarId;
		FullSpritePath = ProfileUtilities.GetAvatarFullImagePath(_assetLookupSystem, avatarId);
		if (FullSpritePath == null)
		{
			Debug.LogError("Avatar found in the store catalog, but missing art assets: " + avatarId);
		}
		AssetLoaderUtils.TrySetSprite(BustImage, _bustImageSpriteTracker, ProfileUtilities.GetAvatarBustImagePath(_assetLookupSystem, avatarId));
		SetLockedTrigger(!owned);
		StoreSection = storeSection;
		NameString = ProfileUtilities.GetAvatarLocKey(avatarId);
		BioString = ProfileUtilities.GetAvatarBio(avatarId);
		VO = ProfileUtilities.GetAvatarVO(_assetLookupSystem, avatarId);
	}

	public void SetInitialState()
	{
		SetLockedTrigger(_locked);
		SetDefaultTrigger(_default);
	}

	public void OnDestroy()
	{
		AssetLoaderUtils.CleanupImage(BustImage, _bustImageSpriteTracker);
	}
}
