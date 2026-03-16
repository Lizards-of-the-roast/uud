using System.Collections.Generic;

public interface IButtonScrollListBrowserProvider
{
	Dictionary<string, ButtonStateData> GetScrollListButtonDataByKey();
}
