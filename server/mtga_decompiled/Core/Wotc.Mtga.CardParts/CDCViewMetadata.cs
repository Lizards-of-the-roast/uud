namespace Wotc.Mtga.CardParts;

public readonly struct CDCViewMetadata
{
	public readonly bool IsMeta;

	public readonly bool IsDimmed;

	public readonly bool IsMouseOver;

	public readonly bool IsHoverCopy;

	public readonly bool IsExaminedCard;

	public CDCViewMetadata(bool isMeta, bool isDimmed, bool isMouseOver, bool isHoverCopy, bool isExaminedCard)
	{
		IsMeta = isMeta;
		IsDimmed = isDimmed;
		IsMouseOver = isMouseOver;
		IsHoverCopy = isHoverCopy;
		IsExaminedCard = isExaminedCard;
	}

	public CDCViewMetadata(BASE_CDC cdc)
	{
		IsMeta = !(cdc is DuelScene_CDC);
		IsDimmed = cdc.IsDimmed;
		IsMouseOver = cdc.IsMousedOver;
		IsHoverCopy = cdc.IsHoverCopy;
		IsExaminedCard = cdc.IsExaminedCard;
	}
}
