using System.Collections.Generic;
using UnityEngine;
using Wotc.Mtgo.Gre.External.Messaging;

public class ZoneCountView : MonoBehaviour
{
	[Header("Animation")]
	[SerializeField]
	private CanvasGroup _canvasGroup;

	[SerializeField]
	private float _fadeInSpeed = 2f;

	[SerializeField]
	private float _fadeOutSpeed = 0.5f;

	[Header("Zone Count")]
	[SerializeField]
	private ZoneCountElement _library;

	[SerializeField]
	private ZoneCountElement _exile;

	[SerializeField]
	private ZoneCountElement _hand;

	[SerializeField]
	private ZoneCountElement _graveyard;

	[Header("Misc")]
	[SerializeField]
	private ZoneCountElement _undergrowth;

	[SerializeField]
	private ZoneCountElement _handSize;

	[SerializeField]
	private ZoneCountElement _collectEvidence;

	private bool _shouldBeVisible;

	private KeyValuePair<ZoneType, ZoneCountElement>[] _elements;

	private void Awake()
	{
		_elements = new KeyValuePair<ZoneType, ZoneCountElement>[7]
		{
			new KeyValuePair<ZoneType, ZoneCountElement>(ZoneType.Library, _library),
			new KeyValuePair<ZoneType, ZoneCountElement>(ZoneType.Exile, _exile),
			new KeyValuePair<ZoneType, ZoneCountElement>(ZoneType.Hand, _hand),
			new KeyValuePair<ZoneType, ZoneCountElement>(ZoneType.Hand, _handSize),
			new KeyValuePair<ZoneType, ZoneCountElement>(ZoneType.Graveyard, _graveyard),
			new KeyValuePair<ZoneType, ZoneCountElement>(ZoneType.Graveyard, _undergrowth),
			new KeyValuePair<ZoneType, ZoneCountElement>(ZoneType.Graveyard, _collectEvidence)
		};
	}

	private void Update()
	{
		if (_shouldBeVisible)
		{
			if (_canvasGroup.alpha < 1f)
			{
				_canvasGroup.alpha += _fadeInSpeed * Time.smoothDeltaTime;
			}
		}
		else if (_canvasGroup.alpha > 0f)
		{
			_canvasGroup.alpha -= _fadeOutSpeed * Time.smoothDeltaTime;
		}
	}

	public void SetVisibility(bool visible)
	{
		_shouldBeVisible = visible;
	}

	public void SetHighlights(ZoneType zoneType)
	{
		KeyValuePair<ZoneType, ZoneCountElement>[] elements = _elements;
		for (int i = 0; i < elements.Length; i++)
		{
			KeyValuePair<ZoneType, ZoneCountElement> keyValuePair = elements[i];
			keyValuePair.Value.SetHighlighted(keyValuePair.Key == zoneType);
		}
	}

	public void SetLibraryCount(int count)
	{
		_library.SetCount(count);
	}

	public void SetExileCount(int count)
	{
		_exile.SetCount(count);
	}

	public void SetHandCount(int count)
	{
		_hand.SetCount(count);
	}

	public void SetHandSize(bool isVisible, int count)
	{
		_handSize.SetVisible(isVisible);
		_handSize.SetCount(count);
	}

	public void SetGraveyardCount(int count)
	{
		_graveyard.SetCount(count);
	}

	public void SetUndergrowth(bool visible, int count)
	{
		_undergrowth.SetVisible(visible && !_handSize.gameObject.activeSelf);
		_undergrowth.SetCount(count);
	}

	public void SetCollectEvidenceCount(bool visible, int count)
	{
		_collectEvidence.SetVisible(visible);
		_collectEvidence.SetCount(count);
	}
}
