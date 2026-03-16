using System;
using AssetLookupTree;
using AssetLookupTree.Payloads.Booster;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.Extensions;

namespace Wotc.Mtga.Wrapper.Draft;

public class DraftBoosterView : MonoBehaviour, IDraftBoosterView
{
	[SerializeField]
	private Animator _animator;

	[SerializeField]
	private Image _boosterImage;

	private AssetLoader.AssetTracker<Sprite> _boosterImageSpriteTracker;

	private int TRNS_Left_TriggerFlag = Animator.StringToHash("TRNS_Left");

	private int TRNS_Right_TriggerFlag = Animator.StringToHash("TRNS_Right");

	private Action<DraftBoosterView> _onFinishedAnimation;

	public CollationMapping CollationId { get; private set; }

	public bool PassDirectionIsLeft { get; private set; }

	public IDraftBoosterView SetBoosterData(CollationMapping collationId)
	{
		if (CollationId == collationId)
		{
			return this;
		}
		CollationId = collationId;
		AssetLookupSystem assetLookupSystem = WrapperController.Instance.AssetLookupSystem;
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.BoosterCollationMapping = collationId;
		if (assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<Icon> loadedTree))
		{
			Icon payload = loadedTree.GetPayload(assetLookupSystem.Blackboard);
			if (payload != null)
			{
				if (_boosterImageSpriteTracker == null)
				{
					_boosterImageSpriteTracker = new AssetLoader.AssetTracker<Sprite>("DraftBoosterImageSprite");
				}
				AssetLoaderUtils.TrySetSprite(_boosterImage, _boosterImageSpriteTracker, payload.SpriteRef.RelativePath);
			}
		}
		return this;
	}

	public IDraftBoosterView PassBooster(bool passDirecitonLeft, Action<IDraftBoosterView> onFinishedAnimation = null)
	{
		_onFinishedAnimation = onFinishedAnimation;
		if (!_animator.isActiveAndEnabled)
		{
			_onFinishedAnimation?.Invoke(this);
			_onFinishedAnimation = null;
		}
		else if (passDirecitonLeft)
		{
			_animator.SetTrigger(TRNS_Left_TriggerFlag);
		}
		else
		{
			_animator.SetTrigger(TRNS_Right_TriggerFlag);
		}
		return this;
	}

	public IDraftBoosterView UpdateActive(bool isActive)
	{
		base.gameObject.UpdateActive(isActive);
		return this;
	}

	private void AnimationEvent_OnFinishedAnimation()
	{
		base.gameObject.SetActive(value: false);
	}

	private void OnDisable()
	{
		_onFinishedAnimation?.Invoke(this);
		_onFinishedAnimation = null;
	}

	private void OnDestroy()
	{
		AssetLoaderUtils.CleanupImage(_boosterImage, _boosterImageSpriteTracker);
	}
}
