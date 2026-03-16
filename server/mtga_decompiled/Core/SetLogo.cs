using Core.Meta.MainNavigation.BoosterChamber;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.Extensions;

public class SetLogo : MonoBehaviour
{
	[SerializeField]
	private GameObject _universesBeyondLogo;

	[SerializeField]
	private GameObject _logoShadow;

	private bool _currentSetIsUniversesBeyond;

	private bool _isVisible;

	private RawImageReferenceLoader _setLogoLoader;

	public bool IsVisible
	{
		get
		{
			return _isVisible;
		}
		set
		{
			base.gameObject.SetActive(value);
			_universesBeyondLogo.UpdateActive(_currentSetIsUniversesBeyond && value);
			_logoShadow.UpdateActive(value);
			_isVisible = value;
		}
	}

	public void Init()
	{
		_setLogoLoader = new RawImageReferenceLoader(GetComponent<RawImage>());
	}

	public void SetLogoOut()
	{
		if (IsVisible)
		{
			_isVisible = false;
		}
	}

	public void SetLogoRefresh(IBoosterChamberSetLogoInfo setLogoInfo)
	{
		SetTexture(setLogoInfo);
	}

	public void SetTexture(IBoosterChamberSetLogoInfo setLogoInfo)
	{
		string text = setLogoInfo?.GetHeaderSetLogoTexturePath();
		bool flag = !string.IsNullOrEmpty(text);
		if (flag)
		{
			_setLogoLoader.SetTexture(text);
			_currentSetIsUniversesBeyond = setLogoInfo.IsUniversesBeyond();
			_universesBeyondLogo.UpdateActive(_currentSetIsUniversesBeyond);
		}
		IsVisible = flag;
	}

	public void Cleanup()
	{
		_setLogoLoader?.Cleanup();
	}
}
