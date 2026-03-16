using AssetLookupTree;
using Core.Code.ClientFeatureToggle;
using Core.Code.PrizeWall;
using Core.Meta.MainNavigation.Store;
using Core.Meta.MainNavigation.Store.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Mtga;
using Wotc.Mtga.Loc;

namespace Core.Meta.MainNavigation.Rewards;

public class PrizeWallTokenRewardDisplay : MonoBehaviour
{
	[SerializeField]
	private Transform _anchor;

	private PrizeWallTokenRewardView _prizeWallTokenInstance;

	private PrizeWallTokenRewardModel _prizeWallTokenRewardModel;

	[SerializeField]
	private TMP_Text _quantityText;

	[SerializeField]
	private Image _prizeWallTokenImage;

	[SerializeField]
	private Localize _buttonText;

	private AssetLoader.AssetTracker<Sprite> _prizeWallTokenImageSpriteTracker;

	private ClientFeatureToggleDataProvider _clientFeatureToggleDataProvider => Pantry.Get<ClientFeatureToggleDataProvider>();

	public void SetToken(PrizeWallTokenRewardModel rewardModel, AssetLookupSystem assetLookupSystem)
	{
		_prizeWallTokenRewardModel = rewardModel;
		_quantityText.text = rewardModel?.Amount.ToString();
		if (rewardModel != null && rewardModel.RewardNavLocKey != null)
		{
			_buttonText.SetText(rewardModel?.RewardNavLocKey);
		}
		_prizeWallTokenImageSpriteTracker = new AssetLoader.AssetTracker<Sprite>("PrizeWallTokenImageSprite");
		AssetLoaderUtils.TrySetSprite(_prizeWallTokenImage, _prizeWallTokenImageSpriteTracker, PrizeWallUtils.GetTokenImagePath(assetLookupSystem, rewardModel.TokenId));
	}

	public void OnVisitPrizeWallButtonPressed()
	{
		foreach (Client_PrizeWall item in Pantry.Get<PrizeWallDataProvider>().GetPrizeWallsByCurrencyId(_prizeWallTokenRewardModel.TokenId))
		{
			if (Pantry.Get<PrizeWallDataProvider>().IsPrizeWallUnlocked(item.Id) || _clientFeatureToggleDataProvider.GetToggleValueById("PrizeWallTixShouldNotify"))
			{
				PrizeWallContext prizeWallContext = new PrizeWallContext(NavContentType.Home);
				SceneLoader.GetSceneLoader().GoToPrizeWall(item.Id, prizeWallContext);
				break;
			}
		}
		if (SceneLoader.GetSceneLoader() != null && SceneLoader.GetSceneLoader().GetRewardsContentController() != null)
		{
			SceneLoader.GetSceneLoader().GetRewardsContentController().Clear();
		}
	}

	public void OnDestroy()
	{
		AssetLoaderUtils.CleanupImage(_prizeWallTokenImage, _prizeWallTokenImageSpriteTracker);
	}
}
