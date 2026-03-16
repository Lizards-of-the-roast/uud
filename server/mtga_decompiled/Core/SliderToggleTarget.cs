using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class SliderToggleTarget : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerClickHandler
{
	[SerializeField]
	private string _value;

	[SerializeField]
	private TextMeshProUGUI _labelText;

	[SerializeField]
	private Color _highlightColor = new Color(0.85f, 0.85f, 0.85f, 1f);

	[SerializeField]
	private float _highlightDuration = 0.1f;

	[SerializeField]
	private Ease _highlightEase = Ease.OutCubic;

	private Color _normalColor;

	public string Value => _value;

	public string Text => _labelText.text;

	public Vector3 Position => base.transform.position;

	public int Index { get; set; }

	public SliderToggle ParentToggle { get; set; }

	private void Awake()
	{
		_normalColor = _labelText.color;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		DOTween.Kill(this);
		_labelText.DOColor(_highlightColor, _highlightDuration).SetEase(_highlightEase).SetTarget(this);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		DOTween.Kill(this);
		_labelText.DOColor(_normalColor, _highlightDuration).SetEase(_highlightEase).SetTarget(this);
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		ParentToggle.SetValue(Value);
	}
}
