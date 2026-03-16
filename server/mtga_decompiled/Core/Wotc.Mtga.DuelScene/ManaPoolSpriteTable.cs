using System;
using UnityEngine;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class ManaPoolSpriteTable : ScriptableObject
{
	[Serializable]
	public struct Sprites
	{
		[SerializeField]
		private Sprite _defaultSprite;

		[SerializeField]
		private Sprite _hoverSprite;

		[Space(3f)]
		[SerializeField]
		private Sprite _hotHighlight;

		[SerializeField]
		private Sprite _autoPayHighlight;

		public Sprite Default(HighlightType highlightType)
		{
			switch (highlightType)
			{
			case HighlightType.AutoPay:
				return _autoPayHighlight;
			case HighlightType.Cold:
			case HighlightType.Tepid:
			case HighlightType.Hot:
				return _hotHighlight;
			default:
				return _defaultSprite;
			}
		}

		public Sprite Hover(HighlightType highlightType)
		{
			if ((uint)(highlightType - 1) <= 3u)
			{
				return _hoverSprite;
			}
			return _defaultSprite;
		}
	}

	[SerializeField]
	private Sprites _whiteMana;

	[SerializeField]
	private Sprites _blueMana;

	[SerializeField]
	private Sprites _blackMana;

	[SerializeField]
	private Sprites _redMana;

	[SerializeField]
	private Sprites _greenMana;

	[SerializeField]
	private Sprites _colorlessMana;

	[SerializeField]
	private Sprites _energyCounter;

	[SerializeField]
	private Sprites _poisonCounter;

	[SerializeField]
	private Sprites _experienceCounter;

	public Sprites GetSpritesForColor(ManaColor color)
	{
		return color switch
		{
			ManaColor.White => _whiteMana, 
			ManaColor.Blue => _blueMana, 
			ManaColor.Black => _blackMana, 
			ManaColor.Red => _redMana, 
			ManaColor.Green => _greenMana, 
			ManaColor.Colorless => _colorlessMana, 
			_ => _colorlessMana, 
		};
	}

	public Sprites GetSpritesForCounter(CounterType counterType)
	{
		return counterType switch
		{
			CounterType.Energy => _energyCounter, 
			CounterType.Poison => _poisonCounter, 
			CounterType.Experience => _experienceCounter, 
			_ => _colorlessMana, 
		};
	}
}
