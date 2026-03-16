using System.Collections.Generic;
using GreClient.CardData;
using UnityEngine;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Client.Models.Mercantile;
using Wotc.Mtga.Extensions;

public class StoreDisplayCardViewBundle : StoreItemDisplay
{
	private const uint _cardBucketSize = 3u;

	[SerializeField]
	private List<StoreCardView> _bundleCardViews = new List<StoreCardView>();

	private Animator companionAnimator;

	private static readonly int InWrapper = Animator.StringToHash("InWrapper");

	private static readonly int WrapperHover = Animator.StringToHash("Wrapper_Hover");

	private static readonly int WrapperClick = Animator.StringToHash("Wrapper_Click");

	private bool showCardSleeve = true;

	private StoreItem _storeItem;

	public List<StoreCardView> BundleCardViews => _bundleCardViews;

	public StoreItem StoreItem => _storeItem;

	private void OnEnable()
	{
		if (GetComponentsInChildren<Animator>().Length >= 2)
		{
			companionAnimator = GetComponentsInChildren<Animator>()[1];
		}
		if (companionAnimator != null && companionAnimator.ContainsParameter(InWrapper))
		{
			companionAnimator.SetBool(InWrapper, value: true);
		}
	}

	public override void Hover(bool on)
	{
		base.Hover(on);
		if (companionAnimator != null && companionAnimator.ContainsParameter(WrapperHover))
		{
			if (on)
			{
				companionAnimator.SetTrigger(WrapperHover);
			}
			else
			{
				companionAnimator.ResetTrigger(WrapperHover);
			}
		}
	}

	public void DisableCardSleeve(bool val)
	{
		showCardSleeve = val;
	}

	public void UpdateRotation()
	{
		if (!showCardSleeve && _bundleCardViews.Count > 0)
		{
			Transform parent = _bundleCardViews[0].transform.parent;
			if (parent != null)
			{
				parent.localPosition = new Vector3(0f, 0f, 0f);
				parent.rotation = new Quaternion(0f, 0f, 0f, 0f);
			}
		}
	}

	public override void SetZoomHandler(ICardRolloverZoom zoomHandler, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder)
	{
		SetZoomHandlers(_bundleCardViews, zoomHandler, cardDatabase, cardViewBuilder);
	}

	public void SetCardViews(List<StoreCardData> cardDefinitions, StoreItem item = null)
	{
		_storeItem = item;
		if (!ApplyCardViews(_bundleCardViews, cardDefinitions, _storeItem, showCardSleeve))
		{
			Debug.LogError(base.gameObject.name + " has null _bundleCardViews.");
		}
	}

	public static bool ApplyCardViews(List<StoreCardView> bundleCardViews, List<StoreCardData> cardDefinitions, StoreItem item = null, bool showCardSleeve = true)
	{
		CardDatabase cardDatabase = WrapperController.Instance.CardDatabase;
		CardViewBuilder cardViewBuilder = WrapperController.Instance.CardViewBuilder;
		List<Sku> list = item?.OwnedBundleItems ?? new List<Sku>();
		for (int i = 0; i < bundleCardViews.Count; i++)
		{
			if (cardDefinitions != null && cardDefinitions.Count > i)
			{
				StoreCardData storeCardData = cardDefinitions[i];
				CardData data;
				if (showCardSleeve && !string.IsNullOrEmpty(storeCardData.SleeveCode))
				{
					data = CardDataExtensions.CreateSkinCard(0u, cardDatabase, storeCardData.CardStyle, storeCardData.SleeveCode, faceDown: true);
				}
				else
				{
					if (storeCardData.GrpID == 0)
					{
						bundleCardViews[i].DestroyCard();
						bundleCardViews[i].gameObject.SetActive(value: false);
						continue;
					}
					if (cardDatabase.CardDataProvider.GetCardPrintingById(storeCardData.GrpID) == null)
					{
						bundleCardViews[i].DestroyCard();
						bundleCardViews[i].gameObject.SetActive(value: false);
						Debug.LogErrorFormat("GrpId {0} doesn't exist in the CardDatabase. Disabling store card display.", storeCardData.GrpID);
						continue;
					}
					data = CardDataExtensions.CreateSkinCard(storeCardData.GrpID, cardDatabase, storeCardData.CardStyle, storeCardData.SleeveCode);
				}
				bundleCardViews[i].gameObject.SetActive(value: true);
				bundleCardViews[i].IsFakeStyleCard = !string.IsNullOrEmpty(storeCardData.CardStyle) && !storeCardData.IsACard;
				bool flag = false;
				foreach (Sku item2 in list)
				{
					if (item2.TreasureItem.TreasureType == TreasureType.ArtStyle && item2.Id.Contains("."))
					{
						flag = StoreDisplayCardViewCore.IsSkuArtStyleForCard(item2.Id, data, cardDatabase);
						if (flag)
						{
							break;
						}
					}
				}
				uint waitFrames = (uint)((long)i / 3L);
				bundleCardViews[i].CreateCard(data, cardDatabase, cardViewBuilder, !string.IsNullOrEmpty(storeCardData.SleeveCode), flag, isClickable: true, waitFrames);
			}
			else
			{
				if (!(bundleCardViews[i] != null))
				{
					return false;
				}
				bundleCardViews[i].DestroyCard();
				bundleCardViews[i].gameObject.SetActive(value: false);
			}
		}
		return true;
	}
}
