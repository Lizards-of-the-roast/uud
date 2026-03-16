using TMPro;
using UnityEngine;
using Wotc.Mtga.Client.Models.Catalog;

public class CardSelector : MonoBehaviour
{
	public Transform CardAnchor;

	public TMP_Text CostGemsLabel;

	public GameObject CostGemsParent;

	public TMP_Text CostGoldLabel;

	public GameObject CostGoldParent;

	public Animator Animator;

	[SerializeField]
	private SpriteRenderer _pip;

	[SerializeField]
	private Sprite _collectedSprite;

	[SerializeField]
	private Sprite _notCollectedSprite;

	public CDCMetaCardView CardView;

	private bool _collected;

	public Meta_CDC CDC { get; set; }

	public ArtStyleEntry CardSkin { get; set; }

	public int? GemPrice { get; set; }

	public int? GoldPrice { get; set; }

	public bool Collected
	{
		get
		{
			return _collected;
		}
		set
		{
			_collected = value;
			_pip.sprite = (value ? _collectedSprite : _notCollectedSprite);
		}
	}

	public void ShowPip(bool show)
	{
		_pip.gameObject.SetActive(show);
	}
}
