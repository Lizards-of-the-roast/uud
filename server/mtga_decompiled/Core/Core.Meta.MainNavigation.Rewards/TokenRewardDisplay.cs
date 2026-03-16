using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using Core.Meta.MainNavigation.Store;
using UnityEngine;

namespace Core.Meta.MainNavigation.Rewards;

public class TokenRewardDisplay : MonoBehaviour
{
	[SerializeField]
	private Transform _anchor;

	private TokenRewardView _tokenInstance;

	public void SetToken(TokenRewardModel rewardModel, AssetLookupSystem assetLookupSystem)
	{
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.LookupString = rewardModel.LookupString;
		string text = (assetLookupSystem.TreeLoader.LoadTree<RewardTokenPrefab>(returnNewTree: false)?.GetPayload(assetLookupSystem.Blackboard))?.PrefabPath;
		if (text == null)
		{
			SimpleLog.LogError("Could not find prefab for lookup: " + rewardModel.LookupString);
			return;
		}
		_tokenInstance = AssetLoader.Instantiate<TokenRewardView>(text, _anchor);
		_tokenInstance.Refresh(rewardModel.TitleKey, rewardModel.Amount, rewardModel.DescriptionKey);
	}
}
