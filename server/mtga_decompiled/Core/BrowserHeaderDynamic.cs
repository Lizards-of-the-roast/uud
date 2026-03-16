public class BrowserHeaderDynamic : BrowserHeader
{
	private string _headerStr = string.Empty;

	private string _subHeaderStr = string.Empty;

	public override void SetHeaderText(string text)
	{
		_headerStr = text;
		SetSimpleText();
	}

	public override void SetSubheaderText(string text)
	{
		_subHeaderStr = text;
		SetSimpleText();
	}

	private void SetSimpleText()
	{
		if ((bool)header && (bool)subheader)
		{
			ClearText();
			bool flag = !string.IsNullOrEmpty(_headerStr);
			bool flag2 = !string.IsNullOrEmpty(_subHeaderStr);
			if (flag && flag2)
			{
				subheader.text = $"<b>{_headerStr}:</b> <i>{_subHeaderStr}</i>";
			}
			else if (flag)
			{
				header.text = _headerStr;
			}
			else if (flag2)
			{
				subheader.text = $"<i>{_subHeaderStr}</i>";
			}
		}
	}

	private void ClearText()
	{
		SetText(header, string.Empty);
		SetText(subheader, string.Empty);
	}
}
