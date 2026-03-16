using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Wotc.Mtga.Extensions;

public class FakeGameManager : MonoBehaviour
{
	private const int SIZE = 5;

	[SerializeField]
	public GameObject eventSystem;

	public EmoteOptionsView emoteOptionsView;

	[Header("Local Sticker References")]
	[SerializeField]
	private GameObject[] localStickerPrefabs = new GameObject[5];

	private Transform[] localStickersEmoteViewParents;

	[SerializeField]
	private Transform localStickerTransform;

	private List<CustomButton> localStickerButtons = new List<CustomButton>();

	[SerializeField]
	private EmoteViewPresenter LocalPlayerEmotePresenter;

	private List<EmoteView> visibleEmoteOptions = new List<EmoteView>();

	[Header("Opponent Sticker References")]
	[SerializeField]
	private Button opponentAvatar;

	public GameObject opponentStickerPrefabs;

	[SerializeField]
	private Transform opponentStickerTransform;

	private void OnValidate()
	{
		if (localStickerPrefabs.Length != 5)
		{
			Array.Resize(ref localStickerPrefabs, 5);
		}
	}

	private void Start()
	{
		localStickersEmoteViewParents = emoteOptionsView.returnStickerTransforms();
		createEventSystem();
		SetAllStickersToEquipped();
		if (!opponentStickerPrefabs)
		{
			opponentStickerPrefabs = localStickerPrefabs[0];
		}
		opponentAvatar.onClick.AddListener(delegate
		{
			showOpponentSticker();
		});
	}

	private void SetAllStickersToEquipped()
	{
		InstantiateStickerPrefabs();
		emoteOptionsView.SetStickerEmoteViewSelections(visibleEmoteOptions);
	}

	public void InstantiateStickerPrefabs()
	{
		GameObject[] array = localStickerPrefabs;
		for (int i = 0; i < array.Length; i++)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(array[i], new Vector3(0f, 0f, 0f), Quaternion.identity);
			gameObject.GetComponent<EmoteView>().SetEquipped(isEquipped: true);
			visibleEmoteOptions.Add(gameObject.GetComponent<EmoteView>());
			gameObject.GetComponentInChildren<CustomButton>().OnClick.AddListener(delegate
			{
				showLocalSelectedSticker();
			});
			localStickerButtons.Add(gameObject.GetComponentInChildren<CustomButton>());
		}
	}

	private void createEventSystem()
	{
		if (EventSystem.current == null || !EventSystem.current.gameObject.activeSelf)
		{
			eventSystem.SetActive(value: true);
		}
	}

	public void ShowSelectedSticker(Transform _stickerEmoteViewParent, GameObject _selectedSticker)
	{
		ChangeLayersRecursively(_selectedSticker.transform, "Default");
		GameObject gameObject = UnityEngine.Object.Instantiate(_selectedSticker, new Vector3(0f, 0f, 0f), Quaternion.identity);
		PresentQueuedEmote(gameObject.GetComponent<EmoteView>(), _stickerEmoteViewParent);
	}

	private void _parentEmote(EmoteView emoteView, Transform _stickerEmoteViewParent)
	{
		emoteView.transform.SetParent(_stickerEmoteViewParent);
		emoteView.transform.ZeroOut();
	}

	public void PresentQueuedEmote(EmoteView emoteView, Transform _stickerEmoteViewParent)
	{
		_parentEmote(emoteView, _stickerEmoteViewParent);
		LocalPlayerEmotePresenter.PresentQueuedEmoteNoData(emoteView);
	}

	public void showLocalSelectedSticker()
	{
		GameObject selectedSticker = EventSystem.current.currentSelectedGameObject.transform.parent.gameObject;
		ShowSelectedSticker(localStickerTransform, selectedSticker);
		emoteOptionsView.Close();
	}

	public void showOpponentSticker()
	{
		opponentStickerTransform.transform.localScale = new Vector3(opponentStickerTransform.localScale.x, Math.Abs(opponentStickerTransform.localScale.y), opponentStickerTransform.localScale.z);
		ShowSelectedSticker(opponentStickerTransform, opponentStickerPrefabs);
	}

	public void ChangeLayersRecursively(Transform trans, string name)
	{
		foreach (Transform tran in trans)
		{
			tran.transform.gameObject.layer = LayerMask.NameToLayer(name);
			ChangeLayersRecursively(tran, name);
		}
	}
}
