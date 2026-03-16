using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using GreClient.Rules;
using UnityEngine;

public class View_TurnChanged : MonoBehaviour
{
	[SerializeField]
	private GameObject _turnPromptAnchor;

	private AssetLookupSystem _assetLookupSystem;

	public void Init(AssetLookupSystem assetLookupSystem)
	{
		_assetLookupSystem = assetLookupSystem;
	}

	public void OnTurnChange(MtgPlayer activePlayer)
	{
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.GREPlayerNum = activePlayer?.ClientPlayerEnum ?? GREPlayerNum.Invalid;
		_assetLookupSystem.Blackboard.Player = activePlayer;
		TurnPromptPrefab payload = _assetLookupSystem.TreeLoader.LoadTree<TurnPromptPrefab>().GetPayload(_assetLookupSystem.Blackboard);
		if (payload != null)
		{
			AssetLoader.Instantiate(payload.PrefabPath, _turnPromptAnchor.transform).AddComponent<SelfCleanup>().SetLifetime(4f, SelfCleanup.CleanupType.Destroy);
		}
	}
}
