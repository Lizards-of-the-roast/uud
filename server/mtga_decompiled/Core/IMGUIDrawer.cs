using UnityEngine;

public class IMGUIDrawer : MonoBehaviour
{
	private Rect _windowRect;

	private int _windowId;

	private int _windowWidth;

	private int _windowHeight;

	private GUI.WindowFunction _windowFunction;

	private string _windowLabel;

	private Matrix4x4 _scale;

	public void Init(int windowId, string windowLabel, GUI.WindowFunction windowFunction, int width, int height, Matrix4x4 scale)
	{
		_windowId = windowId;
		_windowLabel = windowLabel;
		_windowFunction = windowFunction;
		_windowWidth = width;
		_windowHeight = height;
		_scale = scale;
	}

	private void OnGUI()
	{
		GUI.matrix = _scale;
		Color backgroundColor = GUI.backgroundColor;
		GUI.backgroundColor = Color.black;
		_windowRect = GUILayout.Window(_windowId, _windowRect, _windowFunction, _windowLabel, GUILayout.Width(_windowWidth), GUILayout.Height(_windowHeight));
		GUI.backgroundColor = backgroundColor;
		GUI.matrix = Matrix4x4.identity;
	}
}
