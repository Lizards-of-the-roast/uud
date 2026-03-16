using UnityEngine;

[CreateAssetMenu(fileName = "FrameSpriteData", menuName = "ScriptableObject/FrameSpriteData", order = -1)]
public class FrameSpriteData : ScriptableObject
{
	public string Name = "Default";

	public string SkinCode = "";

	public Color TitleTextColor = new Color(0f, 0f, 0f, 1f);

	public Sprite Colorless;

	public Sprite Artifact;

	public Sprite White;

	public Sprite Blue;

	public Sprite Black;

	public Sprite Red;

	public Sprite Green;

	public Sprite WhiteBlue;

	public Sprite WhiteBlack;

	public Sprite BlueBlack;

	public Sprite BlueRed;

	public Sprite BlackRed;

	public Sprite BlackGreen;

	public Sprite RedGreen;

	public Sprite RedWhite;

	public Sprite GreenWhite;

	public Sprite GreenBlue;

	public Sprite Multicolor;
}
