using Assets.Core.Meta.LearnMore;
using UnityEngine;

namespace Wotc.Mtga.LearnMore;

public interface ILearnToPlayContentBuilder
{
	LearnToPlayContents InstantiateContent(string path, string contentName, Transform parent);

	void DestroyContent(LearnToPlayContents contents);
}
