using System;
using AssetLookupTree;
using AssetLookupTree.Payloads.Ability.Metadata;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.DuelScene.BadgeActivationCalculators;

namespace Wotc.Mtga.DuelScene.CardView.BattlefieldIcons;

[Serializable]
public class IconView : BadgeEntrySubView
{
	[SerializeField]
	private Image _iconImage;

	private readonly AssetLoader.AssetTracker<Sprite> _iconTracker = new AssetLoader.AssetTracker<Sprite>("IconView");

	protected override void Init(IBadgeEntryData badgeEntryData)
	{
		base.Init(badgeEntryData);
		_iconImage.sprite = _iconTracker.Acquire(badgeEntryData.SpriteRef);
	}

	protected override void ActivatorInit(IBadgeActivationCalculator calculator, BadgeActivationCalculatorInput input)
	{
		base.ActivatorInit(calculator, input);
		AltAssetReference<Sprite> reference = ((_badgeEntryData.ActivatedSpriteRef == null || string.IsNullOrEmpty(_badgeEntryData.ActivatedSpriteRef.Guid) || string.IsNullOrEmpty(_badgeEntryData.ActivatedSpriteRef.RelativePath) || !calculator.GetActive(input)) ? _badgeEntryData.SpriteRef : _badgeEntryData.ActivatedSpriteRef);
		_iconImage.sprite = _iconTracker.Acquire(reference);
	}

	public override void Cleanup()
	{
		_iconImage.sprite = null;
		_iconTracker.Cleanup();
	}
}
