using System;
using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using Core.Code.Input;
using MTGA.KeyboardManager;
using UnityEngine;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Providers;

public class PreGameScene : MonoBehaviour
{
	public const float ANIM_TIMEOUT = 10f;

	[SerializeField]
	private Transform _VSScreenParent;

	private AssetLookupSystem _assetLookupSystem;

	public bool IsComplete { get; private set; }

	public event Action GameCancelled;

	public event Action MatchConfigReceived;

	public event Action MatchServiceConnected;

	public event Action GameFound;

	public event Action PregameSequenceCompleted;

	private void OnEnable()
	{
		IsComplete = false;
	}

	public void Init(AssetLookupSystem assetLookupSystem, KeyboardManager keyboardManager, IActionSystem actionSystem, ICardDatabaseAdapter cardDatabase, CardMaterialBuilder cardMaterialBuilder, MatchManager matchManager, NPEState npeState, CosmeticsProvider cosmeticsProvider)
	{
		_assetLookupSystem = assetLookupSystem;
		_assetLookupSystem.Blackboard.Clear();
		AssetLoader.Instantiate(_assetLookupSystem.TreeLoader.LoadTree<VSScreenPrefab>(returnNewTree: false).GetPayload(_assetLookupSystem.Blackboard).Prefab, _VSScreenParent).Init(this, keyboardManager, actionSystem, _assetLookupSystem, cardDatabase, cardMaterialBuilder, matchManager, npeState, cosmeticsProvider);
	}

	private void OnDisable()
	{
		this.GameCancelled = null;
		this.GameFound = null;
		this.PregameSequenceCompleted = null;
	}

	private void OnDestroy()
	{
		this.MatchConfigReceived = null;
		this.MatchServiceConnected = null;
	}

	public void ReceivingMatchConfig()
	{
		this.MatchConfigReceived?.Invoke();
	}

	public void ConnectedMatchService()
	{
		this.MatchServiceConnected?.Invoke();
	}

	public void CompletePreGame()
	{
		IsComplete = true;
		this.PregameSequenceCompleted?.Invoke();
	}

	public void CancelGame()
	{
		this.GameCancelled?.Invoke();
	}

	public void AnimateGameFound()
	{
		this.GameFound?.Invoke();
	}
}
