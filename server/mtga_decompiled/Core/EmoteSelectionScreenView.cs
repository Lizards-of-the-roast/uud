using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Wotc.Mtga.Extensions;

public class EmoteSelectionScreenView : MonoBehaviour
{
	[SerializeField]
	private CustomButton _confirmButton;

	[SerializeField]
	private CustomButton _closeButton;

	[SerializeField]
	private EmoteView[] _preinstantiatedEmoteViews;

	[Header("Emote View Local Scale Parameters")]
	[SerializeField]
	private Vector3 _phraseEmoteButtonScale = new Vector3(1f, 1f, 1f);

	[SerializeField]
	private Vector3 _stickerEmoteButtonScale = new Vector3(1f, 1f, 1f);

	[SerializeField]
	private Vector3 _phraseEmotePreviewScale = new Vector3(1f, 1f, 1f);

	[SerializeField]
	private Vector3 _stickerEmotePreviewScale = new Vector3(1f, 1f, 1f);

	[Header("Text Parameters")]
	[SerializeField]
	private TMP_Text _stickersText;

	[SerializeField]
	private TMP_Text _phrasesText;

	[SerializeField]
	private TMP_Text _notificationText;

	[SerializeField]
	private Color _defaultTextColor = Color.gray;

	[SerializeField]
	private Color _notificationTextColor = Color.yellow;

	[SerializeField]
	private Color _errorTextColor = Color.red;

	[Header("UI Section Transforms")]
	[SerializeField]
	private Transform _sampleSection;

	[SerializeField]
	private Transform _classicSection;

	[SerializeField]
	private Transform _stickerSection;

	[SerializeField]
	private Transform _expansionSection;

	public event Action OnConfirmCallback;

	public event Action OnCloseCallback;

	private void Awake()
	{
		_confirmButton.OnClick.AddListener(delegate
		{
			this.OnConfirmCallback?.Invoke();
		});
		_closeButton.OnClick.AddListener(delegate
		{
			this.OnCloseCallback?.Invoke();
		});
		EmoteView[] preinstantiatedEmoteViews = _preinstantiatedEmoteViews;
		foreach (EmoteView obj in preinstantiatedEmoteViews)
		{
			obj.SetEquipped(isEquipped: true);
			obj.SetClickable(isClickable: false);
		}
	}

	private void OnDestroy()
	{
		this.OnConfirmCallback = null;
		this.OnCloseCallback = null;
	}

	public void UpdateOwnedEmotes(EmoteSelectionController.EmoteUISection uiSection, List<EmoteView> emoteViews)
	{
		foreach (EmoteView emoteView in emoteViews)
		{
			switch (uiSection)
			{
			case EmoteSelectionController.EmoteUISection.Classic:
				emoteView.transform.SetParent(_classicSection);
				break;
			case EmoteSelectionController.EmoteUISection.Expansion:
				emoteView.transform.SetParent(_expansionSection);
				break;
			case EmoteSelectionController.EmoteUISection.Sticker:
				emoteView.transform.SetParent(_stickerSection);
				break;
			}
			emoteView.transform.ZeroOut();
			if (uiSection == EmoteSelectionController.EmoteUISection.Sticker)
			{
				emoteView.SetScale(_stickerEmoteButtonScale);
			}
			else
			{
				emoteView.SetScale(_phraseEmoteButtonScale);
			}
		}
	}

	public void UpdateSamplePhraseEmotes(List<EmoteView> emoteViews)
	{
		foreach (EmoteView emoteView in emoteViews)
		{
			emoteView.transform.SetParent(_sampleSection);
			emoteView.transform.ZeroOut();
			emoteView.SetScale(_phraseEmotePreviewScale);
		}
	}

	public void UpdateSampleStickerEmote(EmoteView emoteView)
	{
		emoteView.transform.SetParent(_sampleSection);
		emoteView.transform.ZeroOut();
		emoteView.SetScale(_stickerEmotePreviewScale);
	}

	public void UpdateStickersText(string text, bool isDefaultTextColor = true)
	{
		_stickersText.text = text;
		_stickersText.color = (isDefaultTextColor ? _defaultTextColor : _errorTextColor);
	}

	public void UpdatePhrasesText(string text, bool isDefaultTextColor = true)
	{
		_phrasesText.text = text;
		_phrasesText.color = (isDefaultTextColor ? _defaultTextColor : _errorTextColor);
	}

	public void UpdateNotificationText(string text, bool isDefaultTextColor = true)
	{
		_notificationText.text = text;
		_notificationText.color = (isDefaultTextColor ? _notificationTextColor : _errorTextColor);
	}

	public void SetConfirmButtonInteractable(bool isInteractable)
	{
		_confirmButton.Interactable = isInteractable;
	}

	public Dictionary<string, EmoteView> GetPreInstantiatedEmoteViews()
	{
		Dictionary<string, EmoteView> dictionary = new Dictionary<string, EmoteView>();
		EmoteView[] preinstantiatedEmoteViews = _preinstantiatedEmoteViews;
		foreach (EmoteView emoteView in preinstantiatedEmoteViews)
		{
			dictionary.Add(emoteView.Id, emoteView);
		}
		return dictionary;
	}

	public void SetHoverOnBakedEmoteViews(Action<string> onHover)
	{
		EmoteView[] preinstantiatedEmoteViews = _preinstantiatedEmoteViews;
		foreach (EmoteView obj in preinstantiatedEmoteViews)
		{
			obj.SetHoverable(isHoverable: true);
			obj.OnHover += onHover;
		}
	}
}
