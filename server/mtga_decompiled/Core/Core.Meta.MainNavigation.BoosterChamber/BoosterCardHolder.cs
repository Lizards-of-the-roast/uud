using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Meta.MainNavigation.BoosterChamber;

public class BoosterCardHolder : MonoBehaviour
{
	private const float AUDIO_DELAY = 0.25f;

	[SerializeField]
	private Transform[] _cardParents;

	[SerializeField]
	private bool _holdsRareCard;

	private CardDataAndRevealStatus _cardDataAndRevealStatus;

	public bool InteractionsAllowed = true;

	private Animator _animator;

	private CustomButton _button;

	private Image _image;

	public List<BoosterMetaCardView> CardViews { get; private set; } = new List<BoosterMetaCardView>();

	public bool HoldsRareCard => _holdsRareCard;

	public bool Hidden { get; set; }

	private Animator Animator
	{
		get
		{
			if (_animator == null)
			{
				_animator = GetComponent<Animator>();
			}
			return _animator;
		}
	}

	private CustomButton Button
	{
		get
		{
			if (_button == null)
			{
				_button = GetComponent<CustomButton>();
				_button.OnMouseover.AddListener(delegate
				{
					AudioManager.SetRTPCValue("booster_card_rollover", 100f);
				});
				_button.OnMouseoff.AddListener(delegate
				{
					AudioManager.SetRTPCValue("booster_card_rollover", 0f);
				});
			}
			return _button;
		}
	}

	private Image Image
	{
		get
		{
			if (_image == null)
			{
				_image = GetComponent<Image>();
			}
			return _image;
		}
	}

	public void AddCard(BoosterMetaCardView cardView)
	{
		int count = CardViews.Count;
		if (count < _cardParents.Length)
		{
			cardView.transform.SetParent(_cardParents[count], worldPositionStays: false);
		}
		CardViews.Add(cardView);
		cardView.ActivateFirstTag(isFirst: false);
	}

	public void SetCardDataAndRevealStatus(CardDataAndRevealStatus cardDataAndRevealStatus)
	{
		_cardDataAndRevealStatus = cardDataAndRevealStatus;
	}

	public void ResetCardViewsAndHiddenStatus()
	{
		foreach (BoosterMetaCardView cardView in CardViews)
		{
			cardView.transform.DOKill();
			cardView.GetComponent<Image>().enabled = true;
			cardView.RemoveParticles();
			cardView.EnableCollider(enabled: true);
			cardView.transform.localEulerAngles = Vector3.zero;
		}
		Hidden = false;
		Button.enabled = false;
		Image.enabled = false;
		Animator.enabled = false;
		_cardDataAndRevealStatus = null;
		CardViews = new List<BoosterMetaCardView>();
	}

	public void HideCard(bool anticipation = true)
	{
		Button.enabled = true;
		Image.enabled = true;
		Hidden = true;
		if (anticipation)
		{
			Animator.enabled = true;
		}
		foreach (BoosterMetaCardView cardView in CardViews)
		{
			cardView.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
			cardView.EnableCollider(enabled: false);
			if (anticipation)
			{
				cardView.StartAnticipationParticles(offsetStart: false);
			}
		}
	}

	public void RevealCard(bool anticipation = true, float speed = 0.4f, AnimationCurve ease = null)
	{
		if (_cardDataAndRevealStatus != null)
		{
			_cardDataAndRevealStatus.Revealed = true;
		}
		Button.OnMouseoff.Invoke();
		Hidden = false;
		Button.enabled = false;
		Image.enabled = false;
		foreach (BoosterMetaCardView cardView in CardViews)
		{
			cardView.GetComponent<Image>().enabled = true;
			cardView.EnableCollider(enabled: true);
			if (ease != null)
			{
				cardView.transform.DOLocalRotate(Vector3.zero, speed).SetEase(ease);
			}
			else
			{
				cardView.transform.DOLocalRotate(Vector3.zero, speed);
			}
			if (anticipation)
			{
				cardView.StopAnticipationParticles();
				cardView.StartRevealParticles();
			}
		}
		if (speed > 0f && base.gameObject.activeInHierarchy)
		{
			StartCoroutine(DelayThenShowTagsYield(speed / 2f));
		}
		else
		{
			DisplayTags();
		}
	}

	public void PlayFlipSound()
	{
		switch (CardViews[0].Card.Rarity)
		{
		case CardRarity.MythicRare:
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_boost_card_flip_mythic_rare, base.gameObject);
			break;
		case CardRarity.Rare:
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_boost_card_flip_rare, base.gameObject);
			break;
		default:
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_boost_card_flip_common, base.gameObject);
			break;
		}
		AudioManager.Instance.PlayAudio_BoosterETB(CardViews[0].CardView, 0.25f);
	}

	public IEnumerator DelayThenShowTagsYield(float delay)
	{
		yield return new WaitForSeconds(delay);
		DisplayTags();
	}

	private void DisplayTags()
	{
		foreach (BoosterMetaCardView cardView in CardViews)
		{
			cardView.ActivateTags(_cardDataAndRevealStatus.Tags);
			if (!string.IsNullOrWhiteSpace(_cardDataAndRevealStatus.factionTag))
			{
				cardView.ActivateFactionTag(isFaction: true, _cardDataAndRevealStatus.factionTag);
			}
		}
	}

	public void OnClick()
	{
		if (Hidden)
		{
			PlayFlipSound();
			RevealCard();
		}
	}
}
