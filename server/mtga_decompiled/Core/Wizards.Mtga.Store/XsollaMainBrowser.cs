using UnityEngine;
using UnityEngine.UI;
using ZenFulcrum.EmbeddedBrowser;

namespace Wizards.Mtga.Store;

public class XsollaMainBrowser : MonoBehaviour
{
	[SerializeField]
	private GameObject _backer;

	[SerializeField]
	private MonoBehaviour _browser;

	public Browser Browser => (Browser)_browser;

	private void Awake()
	{
		SetBackerActive(isActive: false);
	}

	public void SetBackerActive(bool isActive)
	{
		if (_backer != null)
		{
			_backer.SetActive(isActive);
		}
	}

	public void Init(EmbeddedXsollaConnection parent)
	{
		Browser.Resize((int)Browser.Size.x, (int)Browser.Size.y);
		Browser.GetComponent<LayoutElement>().preferredHeight = (int)Browser.Size.y;
		Browser.SetNewWindowHandler(Browser.NewWindowAction.NewBrowser, parent);
		Browser.gameObject.SetActive(value: true);
		Browser.WhenLoaded(delegate
		{
			SetBackerActive(isActive: true);
		});
	}
}
