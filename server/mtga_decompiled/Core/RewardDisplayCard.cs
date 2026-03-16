using System;
using TMPro;
using UnityEngine;

public class RewardDisplayCard : MonoBehaviour
{
	private static readonly int ClickDown = Animator.StringToHash("ClickDown");

	private static readonly int Click = Animator.StringToHash("Click");

	private static readonly int CardType = Animator.StringToHash("CardType");

	private static readonly int ActualCardType = Animator.StringToHash("ActualCardType");

	private static readonly int IsWildcard = Animator.StringToHash("IsWildcard");

	private static readonly int _stateHashClickDown = Animator.StringToHash("ClickDown");

	private static readonly int _stateHashIntro = Animator.StringToHash("Intro");

	private static readonly int _stateHashUnrevealed = Animator.StringToHash("Unrevealed");

	public GameObject CardParent1;

	[NonSerialized]
	public Meta_CDC card;

	[SerializeField]
	private Animator _animator;

	[SerializeField]
	private GameObject _textAnchor;

	[SerializeField]
	private TMP_Text _quantity;

	[SerializeField]
	public GameObject newTagObject;

	private bool _clicked;

	private int _actualCardTypeParam;

	private int _expectedCardTypeParam;

	public bool AutoFlip { get; set; }

	public bool IsFlipped()
	{
		if (_animator != null)
		{
			int shortNameHash = _animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
			return shortNameHash != _stateHashClickDown && shortNameHash != _stateHashIntro && shortNameHash != _stateHashUnrevealed;
		}
		return true;
	}

	public void FlipCard()
	{
		_animator.SetTrigger(ClickDown);
		_animator.SetTrigger(Click);
	}

	public void SetQuantity(uint count)
	{
		_quantity.text = $"x{count}";
		_textAnchor.SetActive(count > 1);
	}

	private void OnEnable()
	{
		_animator.SetInteger(CardType, _expectedCardTypeParam);
		_animator.SetInteger(ActualCardType, _actualCardTypeParam);
		if (AutoFlip)
		{
			_animator.SetBool(IsWildcard, value: true);
		}
	}

	public void SetRarity(CardRarity rarity, CardRarity expectedRarity)
	{
		if (expectedRarity > rarity)
		{
			Debug.LogError("expected rarity of " + expectedRarity.ToString() + " Is greater than real card rarity of " + rarity);
			expectedRarity = rarity;
		}
		switch (rarity)
		{
		case CardRarity.Uncommon:
			_actualCardTypeParam = 1;
			break;
		case CardRarity.Rare:
			_actualCardTypeParam = 2;
			break;
		case CardRarity.MythicRare:
			_actualCardTypeParam = 3;
			break;
		default:
			_actualCardTypeParam = 0;
			break;
		}
		switch (expectedRarity)
		{
		case CardRarity.Uncommon:
			_expectedCardTypeParam = 1;
			break;
		case CardRarity.Rare:
			_expectedCardTypeParam = 2;
			break;
		case CardRarity.MythicRare:
			_expectedCardTypeParam = 3;
			break;
		case CardRarity.Land:
		case CardRarity.Common:
			_expectedCardTypeParam = 0;
			break;
		default:
			_expectedCardTypeParam = _actualCardTypeParam;
			break;
		}
		if (_animator.gameObject.activeInHierarchy)
		{
			_animator.SetInteger(CardType, _expectedCardTypeParam);
			_animator.SetInteger(ActualCardType, _actualCardTypeParam);
		}
	}

	public void PlayFlipAudio()
	{
		if (!_clicked)
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept, base.gameObject);
			AudioManager.PlayAudio(WwiseEvents.sfx_energy_hit, base.gameObject);
			float num = 0.28f;
			switch (card.Model.Rarity)
			{
			case CardRarity.Rare:
				num += 1f;
				break;
			case CardRarity.MythicRare:
				num += 1.75f;
				break;
			}
			AudioManager.Instance.PlayAudio_BoosterETB(card, num);
			_clicked = true;
		}
	}

	public void OnRightClick()
	{
	}
}
