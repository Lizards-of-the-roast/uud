using AssetLookupTree;
using AssetLookupTree.Payloads.Card;
using UnityEngine;

public class CDCPart_ExpansionSymbol : CDCPart
{
	[SerializeField]
	private SpriteRenderer _spriteRenderer;

	protected override void HandleUpdateInternal()
	{
		base.HandleUpdateInternal();
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.SetCardDataExtensive(_cachedModel);
		_assetLookupSystem.Blackboard.CardHolderType = _cachedCardHolderType;
		if (!_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<ExpansionSymbol> loadedTree))
		{
			return;
		}
		ExpansionSymbol expansionSymbol = loadedTree?.GetPayload(_assetLookupSystem.Blackboard);
		if (expansionSymbol != null)
		{
			AltAssetReference<Sprite> iconRef = expansionSymbol.GetIconRef(_cachedModel.Rarity);
			Sprite sprite = AssetLoader.AcquireAndTrackAsset(_assetTracker, "ExpansionSymbol._spriteRenderer", iconRef);
			if (sprite != null)
			{
				_spriteRenderer.sprite = sprite;
				RectTransform obj = base.transform as RectTransform;
				obj.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, sprite.bounds.size.x);
				obj.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, sprite.bounds.size.y);
			}
		}
	}
}
