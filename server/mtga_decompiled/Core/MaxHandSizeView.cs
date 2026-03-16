using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Wotc.Mtga.Loc;

public class MaxHandSizeView : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	[SerializeField]
	private Canvas _canvas;

	[SerializeField]
	private TMP_Text _text;

	[SerializeField]
	private GREPlayerNum _owner;

	private GREPlayerNum _activePlayer;

	private int _currentHandCount;

	private uint _maxHandCount;

	public void Init(Camera cam)
	{
		_canvas.worldCamera = cam;
		_canvas.enabled = false;
	}

	public void OnTurnChange(GREPlayerNum activePlayer)
	{
		_activePlayer = activePlayer;
		UpdateText(_currentHandCount, _maxHandCount);
	}

	public void SetCurrentHandCount(int current)
	{
		_currentHandCount = current;
		UpdateText(_currentHandCount, _maxHandCount);
	}

	public void SetMaxHandCount(uint max)
	{
		_maxHandCount = max;
		UpdateText(_currentHandCount, _maxHandCount);
	}

	private void UpdateText(int current, uint max)
	{
		bool flag = current > max && _owner == _activePlayer;
		if (flag)
		{
			int num = Mathf.Max(current - (int)max, 1);
			string key = ((num > 1) ? "DuelScene/ClientPrompt/Over_Max_HandSize_Discard_Multiple_Text" : "DuelScene/ClientPrompt/Over_Max_HandSize_Discard_Single_Text");
			string localizedText = Languages.ActiveLocProvider.GetLocalizedText(key, ("maxHandSize", max.ToString()), ("number", num.ToString()));
			_text.text = localizedText;
		}
		if (_canvas.enabled != flag)
		{
			_canvas.enabled = flag;
		}
	}

	public void OnLanguageChanged()
	{
		UpdateText(_currentHandCount, _maxHandCount);
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		_canvas.enabled = false;
	}
}
