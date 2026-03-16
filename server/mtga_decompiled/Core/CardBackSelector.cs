using UnityEngine;
using Wizards.Unification.Models.Mercantile;

public class CardBackSelector : MonoBehaviour
{
	public Animator Animator;

	public CDCMetaCardView CardView;

	public EStoreSection StoreSection;

	public Meta_CDC CDC { get; set; }

	public string ListingId { get; set; }

	public string CardBack { get; set; }

	public bool Collected { get; set; }
}
