using System;
using TMPro;
using UnityEngine;

public class RewardDisplayCardWithBonus : RewardDisplayCard
{
	[NonSerialized]
	public Meta_CDC cardBonus;

	public GameObject CardParent2;

	[SerializeField]
	private GameObject _bonusTextAnchor;

	[SerializeField]
	private TMP_Text _bonusQuantity;

	public GameObject placeHolderOriginalCard;

	public GameObject placeHolderAlchemyCard;

	private void Awake()
	{
		UnityEngine.Object.Destroy(placeHolderAlchemyCard);
		placeHolderAlchemyCard = null;
		UnityEngine.Object.Destroy(placeHolderOriginalCard);
		placeHolderOriginalCard = null;
	}

	public void SetCardQuantity(uint count)
	{
		_bonusQuantity.text = $"x{count}";
		_bonusTextAnchor.SetActive(count > 1);
		SetQuantity(count);
	}
}
