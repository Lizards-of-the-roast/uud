using System;
using System.Collections.Generic;
using AssetLookupTree;
using UnityEngine;

namespace Wotc.Mtga.Wrapper.Draft;

public class TableDraftPopupView : MonoBehaviour, ITableDraftPopupView
{
	private const int UPDATE_TIME = 5;

	[SerializeField]
	private DraftBoosterView _boosterViewPrefab;

	[SerializeField]
	private CustomButton _backgroundButton;

	[SerializeField]
	private TableDraftSeatView[] _tableDraftSeatViews;

	private Queue<IDraftBoosterView> _boosterViewPool = new Queue<IDraftBoosterView>();

	private Animator _animator;

	private IDraftPod _draftPod;

	private AssetLookupSystem _assetLookupSystem;

	private float _timer;

	private static readonly int ToLeftHash = Animator.StringToHash("ToLeft");

	private static readonly int PlayersHash = Animator.StringToHash("Players");

	public void InitTable(IDraftPod draftPod, BustVisualData[] bustVisualDatas, AssetLookupSystem assetLookupSystem)
	{
		_timer = 5f;
		_draftPod = draftPod;
		_assetLookupSystem = assetLookupSystem;
		if (_animator == null)
		{
			_animator = GetComponent<Animator>();
		}
		_animator.SetInteger(PlayersHash, bustVisualDatas.Length);
		for (int i = 0; i < bustVisualDatas.Length; i++)
		{
			_tableDraftSeatViews[i].Initialize(_assetLookupSystem);
			if (i < bustVisualDatas.Length)
			{
				_tableDraftSeatViews[i].SetBustVisualData(bustVisualDatas[i]);
			}
		}
	}

	public void UpdateTable(bool passDirectionIsLeft, PlayerBoosterVisualData[] boosterVisualDatas)
	{
		_animator.SetBool(ToLeftHash, passDirectionIsLeft);
		int num = Math.Min(boosterVisualDatas.Length, _tableDraftSeatViews.Length);
		List<CollationMapping[]> list = new List<CollationMapping[]>();
		for (int i = 0; i < num; i++)
		{
			list.Add(TableDraftPopupFunctions.GetBoostersNeededAndReturnUnused(boosterVisualDatas[i].DraftBoosterViews, _tableDraftSeatViews[i].DraftBoosterViews, _boosterViewPool));
		}
		for (int j = 0; j < num; j++)
		{
			IDraftBoosterView[] newDraftBoosterViews = TableDraftPopupFunctions.GetNewDraftBoosterViews(list[j], _boosterViewPool);
			for (int k = 0; k < newDraftBoosterViews.Length; k++)
			{
				if (newDraftBoosterViews[k] == null)
				{
					newDraftBoosterViews[k] = UnityEngine.Object.Instantiate(_boosterViewPrefab);
					newDraftBoosterViews[k].SetBoosterData(list[j][k]);
				}
				newDraftBoosterViews[k].UpdateActive(isActive: true);
			}
			_tableDraftSeatViews[j].AddDraftBoosterViews(newDraftBoosterViews);
		}
	}

	private void Update()
	{
		if (!(_timer > 0f) || _draftPod == null)
		{
			return;
		}
		_timer -= Time.deltaTime;
		if (_timer <= 0f)
		{
			StartCoroutine(_draftPod.GetTableVisualData(delegate(DynamicDraftStateVisualData headerData, BustVisualData[] busts, PlayerBoosterVisualData[] playerBoosters)
			{
				UpdateTable(headerData.PassDirectionIsLeft, playerBoosters);
				_timer = 5f;
			}, null));
		}
	}
}
