using TMPro;
using UnityEngine;
using Wotc.Mtga.Extensions;

public class ViewCounter : MonoBehaviour
{
	public enum NumberFormattingType
	{
		Default,
		RomanNumeral
	}

	[SerializeField]
	protected TMP_Text _label;

	[SerializeField]
	protected NumberFormattingType _formattingType;

	public uint? Count { get; private set; }

	public void Init()
	{
		SetCount(0u);
	}

	public void SetCount(uint? count)
	{
		Count = count;
		_label.text = FormattedLabelText(Count, _formattingType);
	}

	private static string FormattedLabelText(uint? count, NumberFormattingType formatType)
	{
		return formatType switch
		{
			NumberFormattingType.Default => count.ToString(), 
			NumberFormattingType.RomanNumeral => ((int)count.Value).ToRomanNumeral(), 
			_ => string.Empty, 
		};
	}
}
