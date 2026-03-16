using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SocialEntityListHeader : MonoBehaviour
{
	[SerializeField]
	private Button _headerButton;

	[SerializeField]
	private TMP_Text _labelItemCount;

	[SerializeField]
	private Image _arrow;

	[SerializeField]
	private GameObject _contentParent;

	private bool _isOpen = true;

	public GameObject ContentParent => _contentParent;

	public Action<bool> IsOpenChanged { get; set; }

	public bool IsOpen
	{
		get
		{
			return _isOpen;
		}
		set
		{
			if (_isOpen != value)
			{
				_isOpen = value;
				_arrow.rectTransform.DOKill();
				_arrow.rectTransform.DOLocalRotate(_isOpen ? Vector3.zero : new Vector3(0f, 0f, 90f), 0.2f);
				IsOpenChanged?.Invoke(_isOpen);
			}
		}
	}

	private void Awake()
	{
		_headerButton.onClick.AddListener(HandleHeaderClick);
	}

	private void HandleHeaderClick()
	{
		IsOpen = !IsOpen;
	}

	public void SetCount(int count)
	{
		base.gameObject.SetActive(count > 0);
		_labelItemCount.text = $"{count}";
	}
}
