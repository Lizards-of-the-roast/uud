using System.Linq;
using UnityEngine;

namespace Wotc.Mtga.AutoPlay;

public class AutoPlayHolder : MonoBehaviour
{
	private GUIStyle _fontStyle = new GUIStyle();

	public AutoPlayManager AutoPlayManager { get; private set; }

	private void Awake()
	{
		Object.DontDestroyOnLoad(base.gameObject);
		_fontStyle.normal.textColor = Color.white;
	}

	public void SetManager(AutoPlayManager autoPlayManager)
	{
		AutoPlayManager = autoPlayManager;
	}

	public void SetFontSize(int fontSize)
	{
		_fontStyle.fontSize = fontSize;
	}

	private void OnGUI()
	{
		if (AutoPlayManager.IsRunning && _fontStyle.fontSize > 1)
		{
			GUILayout.Label("Autoplay step: " + AutoPlayManager.GuiLogs.Last(), _fontStyle);
		}
	}

	private void OnDestroy()
	{
		AutoPlayManager.Dispose();
	}
}
