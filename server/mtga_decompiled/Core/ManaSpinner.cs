using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtgo.Gre.External.Messaging;

public class ManaSpinner : MonoBehaviour
{
	[SerializeField]
	private Animator _animator;

	[SerializeField]
	private TextMeshProUGUI _countLabel;

	[SerializeField]
	private Button _upButton;

	[SerializeField]
	private Button _downButton;

	public ManaColor Color { get; private set; }

	public uint Count { get; private set; }

	public event Action<ManaSpinner> UpEvent;

	public event Action<ManaSpinner> DownEvent;

	public void Init(ManaColor color, uint count)
	{
		Color = color;
		_animator.SetInteger("Color", ManaColorToAnimationInt(Color));
		SetCount(count);
		_upButton.onClick.AddListener(delegate
		{
			this.UpEvent(this);
		});
		_downButton.onClick.AddListener(delegate
		{
			this.DownEvent(this);
		});
	}

	public void OnEnable()
	{
		if (Color != ManaColor.None)
		{
			_animator.SetInteger("Color", ManaColorToAnimationInt(Color));
		}
	}

	public void Cleanup()
	{
		SetUpArrowInteractable(interactable: true);
		SetDownArrowInteractable(interactable: true);
		_upButton.onClick.RemoveAllListeners();
		_downButton.onClick.RemoveAllListeners();
	}

	private int ManaColorToAnimationInt(ManaColor color)
	{
		return color switch
		{
			ManaColor.White => 0, 
			ManaColor.Blue => 1, 
			ManaColor.Black => 2, 
			ManaColor.Red => 3, 
			ManaColor.Green => 4, 
			ManaColor.Colorless => 5, 
			_ => throw new Exception($"Invalid mana color for spinner of {color.ToString()}"), 
		};
	}

	private void OnUpClick()
	{
		this.UpEvent?.Invoke(this);
	}

	public void SetCount(uint count)
	{
		Count = count;
		_countLabel.SetText(count.ToString());
	}

	public void SetUpArrowInteractable(bool interactable)
	{
		_upButton.interactable = interactable;
	}

	public void SetDownArrowInteractable(bool interactable)
	{
		_downButton.interactable = interactable;
	}
}
