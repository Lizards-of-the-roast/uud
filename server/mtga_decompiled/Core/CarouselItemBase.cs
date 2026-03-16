using System;
using Assets.Core.Shared.Code;
using UnityEngine;

public abstract class CarouselItemBase : ScriptableObject
{
	public Sprite MainSprite;

	public Sprite FrameBreakSprite;

	public Sprite FrameBreakBackgroundSprite;

	public string TitleKey;

	public string DescriptionKey;

	public int Priority;

	[Space(20f)]
	[Tooltip("Only active after this (UTC) dateTime")]
	public UDateTime UtcStartTime = DateTime.MinValue;

	[Tooltip("Only active before this (UTC) dateTime")]
	public UDateTime UtcEndTime = DateTime.MaxValue;

	protected virtual bool OnIsVisibleToPlayer()
	{
		return true;
	}

	public virtual bool IsTimeToShow()
	{
		DateTime gameTime = ServerGameTime.GameTime;
		if (gameTime > UtcStartTime)
		{
			return gameTime < UtcEndTime;
		}
		return false;
	}

	public abstract void OnClick();
}
