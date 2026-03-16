using Assets.Core.Meta.Utilities;
using UnityEngine;

namespace Wotc.Mtga;

public class ExternalLink : MonoBehaviour
{
	[SerializeField]
	private string _link;

	public void GoToLink()
	{
		UrlOpener.OpenURL(_link);
	}

	public void GoToLink(string link)
	{
		UrlOpener.OpenURL(link);
	}
}
