using UnityEngine;
using Wizards.Mtga.Decks;
using Wotc.Mtga.Cards.Database;

public class NPEMetaDeckView : MetaDeckView
{
	[Header("NPE Specifics")]
	[SerializeField]
	private NPEObjectivesController _NPEObjectivesController;

	[SerializeField]
	private GameObject _cardsAddedEffect;

	[SerializeField]
	private uint _deckBoxImageGrpID;

	[SerializeField]
	private CustomButton _deckHitbox;

	public override void Init(CardDatabase cardDatabase, CardViewBuilder cardViewBuilder, Client_Deck deck)
	{
		base.Init(cardDatabase, cardViewBuilder, deck);
		SetDeckBoxToCard_BrassMan(_deckBoxImageGrpID);
	}

	public void SetDeckBoxToCard_BrassMan(uint newDeckBoxImageGrpID)
	{
		DeckBoxUtil.SetDeckBoxTexture(newDeckBoxImageGrpID, _cardDatabase.CardDataProvider, _cardViewBuilder.CardMaterialBuilder.TextureLoader, _cardViewBuilder.CardMaterialBuilder.CropDatabase, _meshRendererReferenceLoaders);
	}

	public void UpdateUnlockedCards()
	{
		_NPEObjectivesController.UpdateCardsUnlockedText();
		_cardsAddedEffect.SetActive(value: true);
	}

	public void EnableOpenHitbox()
	{
		_deckHitbox.enabled = true;
	}

	public void DisableOpenHitbox()
	{
		_deckHitbox.enabled = false;
	}

	private void OnEnable()
	{
		if (GetComponent<Animator>() != null)
		{
			GetComponent<Animator>().SetBool("noIntro", value: true);
		}
	}

	private void OnDisable()
	{
	}
}
