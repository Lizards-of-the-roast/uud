using System;

namespace Wotc.Mtga.DuelScene.Interactions;

public class NullChooseXInterface : IChooseXInterface
{
	public static readonly IChooseXInterface Default = new NullChooseXInterface();

	public event Action Submit
	{
		add
		{
		}
		remove
		{
		}
	}

	public event Action<int> ValueModified
	{
		add
		{
		}
		remove
		{
		}
	}

	public void Open()
	{
	}

	public void Close()
	{
	}

	public void SetButtonText(string text)
	{
	}

	public void SetVisualState(NumericInputVisualState visualState)
	{
	}

	public void SetButtonStyle(ButtonStyle.StyleType styleType)
	{
	}
}
