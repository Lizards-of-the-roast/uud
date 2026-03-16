using AssetLookupTree;
using Core.Code.PrizeWall;
using Core.Meta.MainNavigation.Store.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

public class PrizeWallCurrency : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI _currencyQuantity;

	[SerializeField]
	private Image _tokenImage;

	[SerializeField]
	private TooltipTrigger _tooltip;

	private AssetLoader.AssetTracker<Sprite> _prizeWallTokenImageSpriteTracker;

	private string _prizeWallTokenId;

	private string _prizeWallTooltipLocKey;

	public void SetCurrency(PrizeWallDataProvider prizeWallDataProvider, Client_PrizeWall prizeWall, AssetLookupSystem assetLookupSystem)
	{
		string currencyCustomTokenId = prizeWall.CurrencyCustomTokenId;
		base.gameObject.UpdateActive(currencyCustomTokenId != null);
		if (currencyCustomTokenId != null)
		{
			if (_prizeWallTokenImageSpriteTracker == null)
			{
				_prizeWallTokenImageSpriteTracker = new AssetLoader.AssetTracker<Sprite>("PrizeWallTokenImageSprite");
			}
			if (_prizeWallTokenId != currencyCustomTokenId)
			{
				AssetLoaderUtils.TrySetSprite(_tokenImage, _prizeWallTokenImageSpriteTracker, PrizeWallUtils.GetTokenImagePath(assetLookupSystem, currencyCustomTokenId));
				_prizeWallTokenId = currencyCustomTokenId;
			}
			_currencyQuantity.text = prizeWallDataProvider.GetPrizeWallCurrencyQuantity(prizeWall.Id).ToString();
		}
	}

	public void UpdateCurrencyCountTooltip(string tokenHeaderLocKey)
	{
		if (!(_tooltip == null) && !string.IsNullOrEmpty(tokenHeaderLocKey) && !(tokenHeaderLocKey == "MainNav/General/Empty_String") && !(_prizeWallTooltipLocKey == tokenHeaderLocKey))
		{
			LocalizedString locString = new LocalizedString
			{
				mTerm = tokenHeaderLocKey + "Plural"
			};
			_prizeWallTooltipLocKey = tokenHeaderLocKey;
			_tooltip.LocString = locString;
		}
	}

	private void OnDestroy()
	{
		AssetLoaderUtils.CleanupImage(_tokenImage, _prizeWallTokenImageSpriteTracker);
	}
}
