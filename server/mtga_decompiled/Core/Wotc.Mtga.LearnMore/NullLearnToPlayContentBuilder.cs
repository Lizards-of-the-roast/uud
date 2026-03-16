using Assets.Core.Meta.LearnMore;
using UnityEngine;

namespace Wotc.Mtga.LearnMore;

public class NullLearnToPlayContentBuilder : ILearnToPlayContentBuilder
{
	public static readonly ILearnToPlayContentBuilder Default = new NullLearnToPlayContentBuilder();

	public LearnToPlayContents InstantiateContent(string path, string contentName, Transform parent)
	{
		return null;
	}

	public void DestroyContent(LearnToPlayContents contents)
	{
	}
}
