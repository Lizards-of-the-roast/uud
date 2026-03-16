using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "ScriptableObject/Deck View Images", fileName = "DeckViewImages")]
public class DeckViewImages : ScriptableObject
{
	public Texture2D DefaultDeckTexture;

	public Sprite WhiteMana;

	public Sprite BlueMana;

	public Sprite BlackMana;

	public Sprite RedMana;

	public Sprite GreenMana;
}
