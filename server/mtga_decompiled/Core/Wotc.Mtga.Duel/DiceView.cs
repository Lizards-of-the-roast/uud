using System;
using System.Collections;
using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Blackboard;
using AssetLookupTree.Payloads.GeneralEffect;
using GreClient.Rules;
using TMPro;
using UnityEngine;
using Wotc.Mtga.DuelScene.VFX;
using Wotc.Mtga.Extensions;

namespace Wotc.Mtga.Duel;

public class DiceView : MonoBehaviour, IDiceView, IDisposable
{
	public enum HorizontalAlignment
	{
		Center,
		Left,
		Right
	}

	[SerializeField]
	private TMP_Text _dieCountLabel;

	[Header("Prefabs")]
	[SerializeField]
	private ParticleSystem _manyDiceVfxPf;

	[Header("Physical Spacing")]
	[SerializeField]
	private Transform _diceAndTextRoot;

	[SerializeField]
	private HorizontalAlignment _hAlignment;

	[SerializeField]
	private uint _maxIgnoredDiceShown = 3u;

	[SerializeField]
	private float _spaceBetweenDice = 4f;

	[Header("Temporal Spacing")]
	public float _secondsBetweenRolls = 0.5f;

	private GREPlayerNum _controller;

	private IVfxProvider _vfxProvider;

	private AssetLookupSystem _assetLookupSystem;

	private uint _originalDieRollCount;

	private IList<DieRollResultData> _dieRolls;

	private IList<IDieView> _dieViews;

	private uint _rollsInProgressCount;

	private uint _keepAndIgnoresInProgressCount;

	public bool Disposed { get; private set; }

	public event Action<IDiceView> RollsCommencedHandlers;

	public event Action<IDiceView> RollsCompletedHandlers;

	public event Action<IDiceView> KeepAndIgnoresCommencedHandlers;

	public event Action<IDiceView> KeepAndIgnoresCompletedHandlers;

	public void Initialize(GREPlayerNum controller, IVfxProvider vfxProvider, AssetLookupSystem assetLookupSystem)
	{
		_controller = controller;
		_vfxProvider = vfxProvider ?? NullVfxProvider.Default;
		_assetLookupSystem = assetLookupSystem;
	}

	public void Roll(IReadOnlyList<DieRollResultData> dieRolls)
	{
		_originalDieRollCount = (uint)dieRolls.Count;
		_dieCountLabel.gameObject.SetActive(value: false);
		foreach (DieRollResultData dieRoll in dieRolls)
		{
			_dieRolls.Add(dieRoll);
		}
		DieRollResultData item = _dieRolls[0];
		_dieRolls.RemoveAt(0);
		_dieRolls.Shuffle();
		while (_dieRolls.Count > _maxIgnoredDiceShown)
		{
			_dieRolls.RemoveAt(_dieRolls.Count - 1);
		}
		_dieRolls.Add(item);
		_dieRolls.Shuffle();
		_rollsInProgressCount = (uint)_dieRolls.Count;
		StartCoroutine(RollDiceRoutine());
	}

	public void KeepAndIgnore()
	{
		_keepAndIgnoresInProgressCount = (uint)_dieRolls.Count;
		StartCoroutine(KeepAndIgnoreDiceRoutine());
	}

	public void Dispose()
	{
		if (!Disposed && (bool)this && (bool)base.gameObject)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void Awake()
	{
		_dieRolls = new List<DieRollResultData>();
		_dieViews = new List<IDieView>();
	}

	private void OnDestroy()
	{
		if (_dieViews != null)
		{
			foreach (IDieView dieView in _dieViews)
			{
				if (dieView != null)
				{
					dieView.RollCommencedHandlers -= OnRollCommenced;
					dieView.RollCompletedHandlers -= OnRollCompleted;
					dieView.IgnoreCommencedHandlers -= OnIgnoreCommenced;
					dieView.IgnoreCompletedHandlers -= OnIgnoreCompleted;
					dieView.KeepCommencedHandlers -= OnKeepCommenced;
					dieView.KeepCompletedHandlers -= OnKeepCompleted;
					dieView.Dispose();
				}
			}
			_dieViews.Clear();
		}
		_assetLookupSystem = default(AssetLookupSystem);
		_vfxProvider = null;
		Disposed = true;
	}

	private IEnumerator RollDiceRoutine()
	{
		this.RollsCommencedHandlers?.Invoke(this);
		yield return null;
		if (_originalDieRollCount - 1 > _maxIgnoredDiceShown)
		{
			_dieCountLabel.text = $"x{_originalDieRollCount}";
			_dieCountLabel.gameObject.SetActive(value: true);
			InstantiateManyDice();
		}
		int i = 0;
		while (i < _dieRolls.Count)
		{
			DieRollResultData dieRollResultData = _dieRolls[i];
			IDieView dieView = InstantiateDie(dieRollResultData.DieFaces, (uint)i, (uint)_dieRolls.Count);
			dieView.RollCommencedHandlers += OnRollCommenced;
			dieView.RollCompletedHandlers += OnRollCompleted;
			dieView.IgnoreCommencedHandlers += OnIgnoreCommenced;
			dieView.IgnoreCompletedHandlers += OnIgnoreCompleted;
			dieView.KeepCommencedHandlers += OnKeepCommenced;
			dieView.KeepCompletedHandlers += OnKeepCompleted;
			dieView.Roll(dieRollResultData.NaturalResult, dieRollResultData.Result);
			_dieViews.Add(dieView);
			yield return new WaitForSeconds(_secondsBetweenRolls);
			int num = i + 1;
			i = num;
		}
	}

	private IEnumerator KeepAndIgnoreDiceRoutine()
	{
		this.KeepAndIgnoresCommencedHandlers?.Invoke(this);
		yield return null;
		for (int i = 0; i < _dieRolls.Count; i++)
		{
			DieRollResultData dieRollResultData = _dieRolls[i];
			IDieView dieView = _dieViews[i];
			if (dieRollResultData.Ignored)
			{
				dieView.Ignore();
			}
			else
			{
				dieView.Keep();
			}
		}
		_dieCountLabel.gameObject.SetActive(value: false);
	}

	private IDieView InstantiateDie(uint dieFaces, uint dieIndex = 0u, uint dieCount = 1u)
	{
		if (!_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<DieViewPrefab> loadedTree))
		{
			return null;
		}
		IBlackboard blackboard = _assetLookupSystem.Blackboard;
		blackboard.Clear();
		blackboard.DieFaces = dieFaces;
		blackboard.GREPlayerNum = _controller;
		DieViewPrefab payload = loadedTree.GetPayload(blackboard);
		if (payload == null)
		{
			return null;
		}
		DieView dieView = UnityEngine.Object.Instantiate(AssetLoader.GetObjectData(payload.PrefabRef), _diceAndTextRoot, worldPositionStays: false);
		Transform component = dieView.gameObject.GetComponent<Transform>();
		float num = CalculateXOffset(dieIndex, dieCount);
		component.localPosition += component.right * num;
		dieView.Initialize(dieFaces, _vfxProvider, _assetLookupSystem);
		return dieView;
	}

	private void InstantiateManyDice()
	{
		UnityEngine.Object.Instantiate(_manyDiceVfxPf).gameObject.AddOrGetComponent<SelfCleanup>();
	}

	private float CalculateXOffset(uint dieIndex = 0u, uint dieCount = 1u)
	{
		float num = _spaceBetweenDice * (float)(dieCount - 1);
		float num2 = 0f;
		switch (_hAlignment)
		{
		case HorizontalAlignment.Left:
			num2 = num * 0f;
			break;
		case HorizontalAlignment.Center:
			num2 = num * 0.5f;
			break;
		case HorizontalAlignment.Right:
			num2 = num * 1f;
			break;
		}
		return num2 - _spaceBetweenDice * (float)dieIndex;
	}

	private void OnRollCommenced(IDieView dieView)
	{
	}

	private void OnRollCompleted(IDieView dieView)
	{
		_rollsInProgressCount--;
		if (_rollsInProgressCount == 0)
		{
			this.RollsCompletedHandlers?.Invoke(this);
		}
	}

	private void OnIgnoreCommenced(IDieView dieView)
	{
	}

	private void OnIgnoreCompleted(IDieView dieView)
	{
		_keepAndIgnoresInProgressCount--;
		if (_keepAndIgnoresInProgressCount == 0)
		{
			this.KeepAndIgnoresCompletedHandlers?.Invoke(this);
		}
	}

	private void OnKeepCommenced(IDieView dieView)
	{
	}

	private void OnKeepCompleted(IDieView dieView)
	{
		_keepAndIgnoresInProgressCount--;
		if (_keepAndIgnoresInProgressCount == 0)
		{
			this.KeepAndIgnoresCompletedHandlers?.Invoke(this);
		}
	}
}
