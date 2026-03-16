using System;
using UnityEngine;
using Wotc.Mtga.Loc;

namespace EventPage.Components;

public class PrizeWallComponent : EventComponent
{
	[SerializeField]
	private CustomButton _button;

	public Action OnClicked;

	public PrizeWallCurrency CurrencyPrizeWall;

	public Localize PrizeWallNameLocKey;

	private void Awake()
	{
		_button.OnClick.AddListener(delegate
		{
			OnClicked?.Invoke();
		});
	}
}
