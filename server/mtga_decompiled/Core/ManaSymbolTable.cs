using UnityEngine;
using Wotc.Mtgo.Gre.External.Messaging;

[CreateAssetMenu(fileName = "ManaSymbolTable", menuName = "ScriptableObject/Mana Symbol Table", order = -1)]
public class ManaSymbolTable : ScriptableObject
{
	[SerializeField]
	private Sprite _colorlessSprite;

	[SerializeField]
	private Sprite _whiteSprite;

	[SerializeField]
	private Sprite _blueSprite;

	[SerializeField]
	private Sprite _blackSprite;

	[SerializeField]
	private Sprite _redSprite;

	[SerializeField]
	private Sprite _greenSprite;

	public Sprite GetSprite(Wotc.Mtgo.Gre.External.Messaging.Color color)
	{
		return color switch
		{
			Wotc.Mtgo.Gre.External.Messaging.Color.White => _whiteSprite, 
			Wotc.Mtgo.Gre.External.Messaging.Color.Blue => _blueSprite, 
			Wotc.Mtgo.Gre.External.Messaging.Color.Black => _blackSprite, 
			Wotc.Mtgo.Gre.External.Messaging.Color.Red => _redSprite, 
			Wotc.Mtgo.Gre.External.Messaging.Color.Green => _greenSprite, 
			_ => _colorlessSprite, 
		};
	}

	public Sprite GetSprite(ManaColor color)
	{
		return color switch
		{
			ManaColor.White => _whiteSprite, 
			ManaColor.Blue => _blueSprite, 
			ManaColor.Black => _blackSprite, 
			ManaColor.Red => _redSprite, 
			ManaColor.Green => _greenSprite, 
			_ => _colorlessSprite, 
		};
	}

	public Sprite GetSprite(CardColor color)
	{
		return color switch
		{
			CardColor.White => _whiteSprite, 
			CardColor.Blue => _blueSprite, 
			CardColor.Black => _blackSprite, 
			CardColor.Red => _redSprite, 
			CardColor.Green => _greenSprite, 
			_ => _colorlessSprite, 
		};
	}
}
