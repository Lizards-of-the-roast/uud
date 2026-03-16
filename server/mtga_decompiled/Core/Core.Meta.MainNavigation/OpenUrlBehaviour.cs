using Assets.Core.Meta.Utilities;
using UnityEngine;
using Wizards.Mtga;
using Wotc.Mtga.Loc;

namespace Core.Meta.MainNavigation;

public class OpenUrlBehaviour : MonoBehaviour
{
	[SerializeField]
	private string _URLKey;

	private IClientLocProvider _locProvider;

	private void Awake()
	{
		_locProvider = Pantry.Get<IClientLocProvider>();
	}

	public void OpenURL()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, AudioManager.Default);
		UrlOpener.OpenURL(_locProvider.GetLocalizedText(_URLKey));
	}
}
