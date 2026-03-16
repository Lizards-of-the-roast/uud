using GreClient.CardData;
using UnityEngine;
using Wotc.Mtga.Extensions;

public class OrbAndAutoAsseter : MonoBehaviour
{
	public bool UseGRPIDForPremiumCard;

	public uint GRPIDofCardToShow;

	public GameObject AnchorForCard;

	private bool _cardWasMade;

	public bool UseCardback;

	public string NameOfSleeveToShow;

	public GameObject AnchorForSleeve;

	public void Awake()
	{
		if (UseGRPIDForPremiumCard && !_cardWasMade && AnchorForCard != null)
		{
			CardData data = CardDataExtensions.CreateSkinCard(GRPIDofCardToShow, WrapperController.Instance.CardDatabase, "DA");
			Meta_CDC meta_CDC = WrapperController.Instance.CardViewBuilder.CreateMetaCdc(CardDataExtensions.CreateBlank());
			meta_CDC.transform.SetParent(AnchorForCard.transform);
			meta_CDC.transform.ZeroOut();
			meta_CDC.gameObject.SetActive(value: false);
			meta_CDC.SetModel(data);
			meta_CDC.ImmediateUpdate();
			meta_CDC.gameObject.SetActive(value: true);
			_cardWasMade = true;
		}
	}
}
