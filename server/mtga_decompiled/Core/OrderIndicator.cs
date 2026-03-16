using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OrderIndicator : MonoBehaviour
{
	public enum ArrowDirection
	{
		Left,
		Right
	}

	[SerializeField]
	private TextMeshProUGUI leftText;

	[SerializeField]
	private TextMeshProUGUI middleText;

	[SerializeField]
	private TextMeshProUGUI rightText;

	[SerializeField]
	private Image arrows;

	private ArrowDirection currentDirection = ArrowDirection.Right;

	public void SetText(string left, string right)
	{
		leftText.SetText(left);
		rightText.SetText(right);
		middleText.SetText(string.Empty);
	}

	public void SetMiddleText(string middle)
	{
		middleText.SetText(middle);
	}

	public void SetArrowDirection(ArrowDirection direction)
	{
		if (direction != currentDirection)
		{
			Vector3 localScale = arrows.transform.localScale;
			localScale.x *= -1f;
			arrows.transform.localScale = localScale;
			currentDirection = direction;
		}
	}
}
