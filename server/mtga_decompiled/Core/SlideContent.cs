using UnityEngine;

public class SlideContent
{
	private Sprite _sprite;

	private NavContentType _navContentType;

	public Sprite Sprite => _sprite;

	public NavContentType NavContentType => _navContentType;

	public SlideContent(Sprite sprite, NavContentType navContentType)
	{
		_sprite = sprite;
		_navContentType = navContentType;
	}
}
