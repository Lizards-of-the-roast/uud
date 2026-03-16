using DG.Tweening;
using GreClient.CardData;
using UnityEngine;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;

public class CardBundle : MonoBehaviour
{
	[SerializeField]
	private float _insertAnimationTime = 0.75f;

	[SerializeField]
	private Ease _insertAnimationEase = Ease.InCubic;

	[SerializeField]
	private CDCMetaCardView _cardPrefab;

	private Animator[] _cardAnimators;

	private Transform[] _cardContainers;

	private CardData _cardData;

	private CardDatabase _cardDatabase;

	private CardViewBuilder _cardViewBuilder;

	public int TotalCards { get; private set; }

	private void Awake()
	{
		_cardAnimators = new Animator[base.transform.childCount];
		_cardContainers = new Transform[_cardAnimators.Length];
		for (int i = 0; i < _cardAnimators.Length; i++)
		{
			int num = _cardAnimators.Length - 1 - i;
			_cardAnimators[num] = base.transform.GetChild(i).GetComponent<Animator>();
			_cardContainers[num] = _cardAnimators[num].transform.GetChild(0);
		}
	}

	public void Init(CardData card, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder)
	{
		_cardData = card;
		_cardDatabase = cardDatabase;
		_cardViewBuilder = cardViewBuilder;
		TotalCards = 0;
		for (int i = 0; i < _cardContainers.Length; i++)
		{
			_cardContainers[i].DestroyChildren();
			_cardAnimators[i].gameObject.UpdateActive(active: false);
			Vector3 localPosition = _cardContainers[i].localPosition;
			localPosition.x = 0f;
			_cardContainers[i].localPosition = localPosition;
		}
	}

	public void RevealCard()
	{
		if (TotalCards < _cardContainers.Length)
		{
			int totalCards = TotalCards;
			TotalCards++;
			_cardAnimators[totalCards].gameObject.UpdateActive(active: true);
			CDCMetaCardView cDCMetaCardView = Object.Instantiate(_cardPrefab, _cardContainers[totalCards]);
			cDCMetaCardView.Init(_cardDatabase, _cardViewBuilder);
			AudioManager.PlayAudio("sfx_basicloc_flip_d_loop_speed1_start", base.gameObject);
			AudioManager.PlayAudio("sfx_basicloc_flip_d_loop_speed1_stop", base.gameObject, 0.25f);
			cDCMetaCardView.SetData(_cardData);
		}
	}

	public CardBundleCardAnimation InsertCard(int index, Transform toDeck)
	{
		_cardAnimators[index].SetTrigger("Insert");
		float x = base.transform.InverseTransformPoint(toDeck.position).x;
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_pull_card, base.gameObject);
		DOTween.To(() => _cardContainers[index].localPosition.x, delegate(float x2)
		{
			Vector3 localPosition = _cardContainers[index].localPosition;
			localPosition.x = x2;
			_cardContainers[index].localPosition = localPosition;
		}, x, _insertAnimationTime).SetUpdate(UpdateType.Late).SetEase(_insertAnimationEase);
		return _cardAnimators[index].GetComponent<CardBundleCardAnimation>();
	}
}
