using System.Collections.Generic;
using AssetLookupTree;
using MTGA.Loc;
using UnityEngine;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga.Wrapper.Draft;

public class TableDraftQueueView : MonoBehaviour
{
	public enum SeatType
	{
		Filled,
		FilledMe,
		Open
	}

	[SerializeField]
	private Transform _openSeats;

	[SerializeField]
	private Transform _openSeatsBaseBacker;

	[SerializeField]
	private Transform _filledSeats;

	[SerializeField]
	private Transform _loadingIndicator;

	[Header("Seat Prefabs")]
	[SerializeField]
	private GameObject _filledSeatMe;

	[SerializeField]
	private GameObject _filledSeat;

	[SerializeField]
	private GameObject _openSeat;

	[Header("Extended Pod Capacity")]
	[SerializeField]
	private GameObject _waitingMessage;

	[SerializeField]
	private Localize _waitingText;

	[SerializeField]
	private int _maxSeats = 8;

	private SeatTileView _localSeatView;

	private readonly List<SeatTileView> _seatViews = new List<SeatTileView>();

	private readonly List<GameObject> _openSeatViews = new List<GameObject>();

	private PlayerInSeat _localSeat;

	private AnonymousSeatVisualData[] _anonymousSeatVisualDataArray;

	private AssetLookupSystem _assetLookupSystem;

	private bool seatsInitialized;

	public void InitTable(string displayName, string avatarId, AssetLookupSystem assetLookupSystem)
	{
		_showLoadingIndicator(show: true);
		_localSeat = new PlayerInSeat(displayName, avatarId, isReady: false);
		_assetLookupSystem = assetLookupSystem;
		UpdateLocalSeat(TableDraftQueueFunctions.GetKnownSeatVisualData(_localSeat));
		if (!seatsInitialized)
		{
			for (int i = 0; i < _maxSeats - 1; i++)
			{
				GameObject gameObject = InitSeatPrefab(SeatType.Open);
				gameObject.SetActive(value: false);
				_openSeatViews.Add(gameObject);
				GameObject gameObject2 = InitSeatPrefab(SeatType.Filled);
				gameObject2.SetActive(value: false);
				_seatViews.Add(gameObject2.GetComponent<SeatTileView>());
			}
			seatsInitialized = true;
		}
	}

	public void InitSeats(int podCapacity)
	{
		int num = Mathf.Min(podCapacity, _maxSeats);
		for (int i = 0; i < num - 1; i++)
		{
			_openSeatViews[i].SetActive(value: true);
		}
	}

	public void UpdateWaitingMessage(int numInPod, int podCapacity)
	{
		bool flag = podCapacity > _maxSeats;
		if (flag)
		{
			List<MTGALocalizable.LocParam> parameters = new List<MTGALocalizable.LocParam>
			{
				new MTGALocalizable.LocParam
				{
					key = "numInPod",
					value = numInPod.ToString()
				},
				new MTGALocalizable.LocParam
				{
					key = "podCapacity",
					value = podCapacity.ToString()
				}
			};
			_waitingText.SetText("Draft/Waiting_For_Players", parameters);
			foreach (SeatTileView seatView in _seatViews)
			{
				seatView.gameObject.SetActive(value: false);
			}
		}
		_waitingMessage.gameObject.UpdateActive(flag);
		_showLoadingIndicator(flag);
	}

	private void _showLoadingIndicator(bool show)
	{
		_openSeats.gameObject.UpdateActive(!show);
		_openSeatsBaseBacker.gameObject.UpdateActive(!show);
		_loadingIndicator.gameObject.UpdateActive(show);
	}

	public void ResetTable()
	{
		_showLoadingIndicator(show: true);
		_anonymousSeatVisualDataArray = null;
		_localSeatView.UpdateKnownSeat(TableDraftQueueFunctions.GetKnownSeatVisualData(_localSeat), _assetLookupSystem);
		foreach (SeatTileView seatView in _seatViews)
		{
			seatView.ResetSeat();
			seatView.gameObject.SetActive(value: false);
		}
		foreach (GameObject openSeatView in _openSeatViews)
		{
			openSeatView.SetActive(value: false);
		}
	}

	private GameObject InitSeatPrefab(SeatType seatType)
	{
		GameObject result = null;
		switch (seatType)
		{
		case SeatType.FilledMe:
			result = Object.Instantiate(_filledSeatMe, _filledSeats);
			break;
		case SeatType.Filled:
			result = Object.Instantiate(_filledSeat, _filledSeats);
			break;
		case SeatType.Open:
			result = Object.Instantiate(_openSeat, _openSeats);
			break;
		default:
			SimpleLog.LogError("TableDraftQueueView.InitSeatPrefab() SeatType not implemented: " + seatType);
			break;
		}
		return result;
	}

	public void UpdateLocalSeat(KnownSeatVisualData seatVisualData)
	{
		if (_localSeatView == null)
		{
			GameObject gameObject = InitSeatPrefab(SeatType.FilledMe);
			_localSeatView = gameObject.GetComponent<SeatTileView>();
		}
		_localSeatView.UpdateKnownSeat(seatVisualData, _assetLookupSystem);
	}

	public AnonymousSeatVisualData[] UpdateFilledSeats(int numberOfSeatsFilled, int capacity = 8)
	{
		_showLoadingIndicator(show: false);
		_anonymousSeatVisualDataArray = TableDraftQueueFunctions.GetAnonymousSeatVisualDataArray(numberOfSeatsFilled - 1, _seatViews.Count);
		if (seatsInitialized)
		{
			for (int i = 0; i < _anonymousSeatVisualDataArray.Length; i++)
			{
				_seatViews[i].UpdateAnonymousSeat(_anonymousSeatVisualDataArray[i]);
			}
		}
		return _anonymousSeatVisualDataArray;
	}

	public AnonymousSeatVisualData[] UpdateReadySeats(int numberOfReadySeats, bool isLocalPlayerReady)
	{
		_showLoadingIndicator(show: false);
		_anonymousSeatVisualDataArray = TableDraftQueueFunctions.GetReadySeatVisualDataArray(_anonymousSeatVisualDataArray ?? new AnonymousSeatVisualData[0], numberOfReadySeats, isLocalPlayerReady);
		int num = Mathf.Min(_seatViews.Count, _anonymousSeatVisualDataArray.Length);
		for (int i = 0; i < num; i++)
		{
			_seatViews[i].UpdateReadySeat(_anonymousSeatVisualDataArray[i]);
		}
		return _anonymousSeatVisualDataArray;
	}

	public void RevealAllSeatIdentities(List<KnownSeatVisualData> seatVisualDataArray)
	{
		if (seatsInitialized)
		{
			int num = Mathf.Min(_seatViews.Count, seatVisualDataArray.Count);
			for (int i = 0; i < num; i++)
			{
				_seatViews[i].UpdateKnownSeat(seatVisualDataArray[i], _assetLookupSystem);
			}
		}
	}
}
