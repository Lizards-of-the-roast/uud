using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MetaCardPip : MonoBehaviour
{
	public enum SpriteType
	{
		Infinity,
		Collected_InInventory_Collapsed,
		Collected_InInventory_Expanded,
		Collected_InDeck_Collapsed,
		Collected_InDeck_Expanded,
		NotCollected_InInventory,
		NotCollected_InDeck,
		Collected_Skin,
		NotCollected_Skin
	}

	[SerializeField]
	private TooltipTrigger _toolTipTrigger;

	[SerializeField]
	private Image _targetImage;

	[SerializeField]
	private GameObject _newPip;

	[SerializeField]
	private GameObject _newPipParticles;

	[SerializeField]
	private Image _newPipGlowImage;

	[SerializeField]
	private Sprite _spriteInfinity;

	[SerializeField]
	private Sprite _spriteCollectedInInventoryCollapsed;

	[SerializeField]
	private Sprite _spriteCollectedInInventoryExpanded;

	[SerializeField]
	private Sprite _spriteCollectedInDeckCollapsed;

	[SerializeField]
	private Sprite _spriteCollectedInDeckExpanded;

	[SerializeField]
	private Sprite _spriteNotCollectedInInventory;

	[SerializeField]
	private Sprite _spriteNotCollectedInDeck;

	[SerializeField]
	private Sprite _spriteCollectedSkin;

	[SerializeField]
	private Sprite _spriteNotCollectedSkin;

	private Dictionary<SpriteType, Sprite> _table;

	private void Awake()
	{
		_table = new Dictionary<SpriteType, Sprite>
		{
			{
				SpriteType.Infinity,
				_spriteInfinity
			},
			{
				SpriteType.Collected_InInventory_Collapsed,
				_spriteCollectedInInventoryCollapsed
			},
			{
				SpriteType.Collected_InInventory_Expanded,
				_spriteCollectedInInventoryExpanded
			},
			{
				SpriteType.Collected_InDeck_Collapsed,
				_spriteCollectedInDeckCollapsed
			},
			{
				SpriteType.Collected_InDeck_Expanded,
				_spriteCollectedInDeckExpanded
			},
			{
				SpriteType.NotCollected_InInventory,
				_spriteNotCollectedInInventory
			},
			{
				SpriteType.NotCollected_InDeck,
				_spriteNotCollectedInDeck
			},
			{
				SpriteType.Collected_Skin,
				_spriteCollectedSkin
			},
			{
				SpriteType.NotCollected_Skin,
				_spriteNotCollectedSkin
			}
		};
	}

	public void SetToolTip(string locKey)
	{
		_toolTipTrigger.LocString.mTerm = locKey;
	}

	public void SetSpriteType(SpriteType sprite)
	{
		_targetImage.sprite = _table[sprite];
		_newPipGlowImage.sprite = _table[sprite];
		_newPipParticles.SetActive(sprite == SpriteType.Collected_InDeck_Collapsed);
	}

	public void SetNew(bool isNew)
	{
		_newPip.SetActive(isNew);
	}
}
