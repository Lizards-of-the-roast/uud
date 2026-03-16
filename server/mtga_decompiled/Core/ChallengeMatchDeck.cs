using System;
using System.Linq;
using UnityEngine;

public class ChallengeMatchDeck : PropertyAttribute
{
	public static string GetLocalPath(string moreGlobalPathforP4)
	{
		string[] array = Application.dataPath.Split(new string[1] { "/MDN/" }, StringSplitOptions.None);
		if (array.Count() > 0)
		{
			return array[0] + moreGlobalPathforP4;
		}
		return moreGlobalPathforP4;
	}
}
