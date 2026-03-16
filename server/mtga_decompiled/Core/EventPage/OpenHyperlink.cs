using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Core.Meta.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Wotc.Mtga.CustomInput;
using Wotc.Mtga.Extensions;

namespace EventPage;

[RequireComponent(typeof(TextMeshProUGUI))]
public class OpenHyperlink : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	[SerializeField]
	private Canvas _canvas;

	[SerializeField]
	private bool _doesColorChangeOnHover = true;

	[SerializeField]
	private Color _hoverColor = new Color(0.23529412f, 0.47058824f, 1f);

	private TextMeshProUGUI _textMeshPro;

	private Camera _camera;

	private int _currentLinkIndex = -1;

	private List<Color32[]> _originalVertexColors = new List<Color32[]>();

	private bool _isLinkHighlighted => _currentLinkIndex != -1;

	protected virtual void Awake()
	{
		_textMeshPro = GetComponent<TextMeshProUGUI>();
		if (_canvas == null)
		{
			_canvas = GetComponentInParent<Canvas>();
		}
		if (_canvas == null)
		{
			_camera = CurrentCamera.Value;
		}
		else if (_canvas.renderMode == RenderMode.ScreenSpaceOverlay)
		{
			_camera = null;
		}
		else
		{
			_camera = _canvas.worldCamera;
		}
	}

	private void LateUpdate()
	{
		Vector3 vector = CustomInputModule.GetPointerPosition();
		int num = (TMP_TextUtilities.IsIntersectingRectTransform(_textMeshPro.rectTransform, vector.ZeroIfInvalidVector(), _camera) ? TMP_TextUtilities.FindIntersectingLink(_textMeshPro, vector, _camera) : (-1));
		if (_currentLinkIndex != -1 && num != _currentLinkIndex)
		{
			SetLinkToColor(_currentLinkIndex, (int linkIdx, int vertIdx) => _originalVertexColors[linkIdx][vertIdx]);
			_originalVertexColors.Clear();
			_currentLinkIndex = -1;
		}
		if (num == -1 || num == _currentLinkIndex)
		{
			return;
		}
		_currentLinkIndex = num;
		if (_doesColorChangeOnHover)
		{
			_originalVertexColors = SetLinkToColor(num, (int _linkIdx, int _vertIdx) => _hoverColor);
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		int num = TMP_TextUtilities.FindIntersectingLink(_textMeshPro, CustomInputModule.GetPointerPosition(), _camera);
		if (num != -1)
		{
			TMP_LinkInfo tMP_LinkInfo = _textMeshPro.textInfo.linkInfo[num];
			UrlOpener.OpenURL(tMP_LinkInfo.GetLinkID());
		}
	}

	private List<Color32[]> SetLinkToColor(int linkIndex, Func<int, int, Color32> colorForLinkAndVert)
	{
		TMP_LinkInfo tMP_LinkInfo = _textMeshPro.textInfo.linkInfo[linkIndex];
		List<Color32[]> list = new List<Color32[]>();
		for (int i = 0; i < tMP_LinkInfo.linkTextLength; i++)
		{
			int num = tMP_LinkInfo.linkTextfirstCharacterIndex + i;
			TMP_CharacterInfo tMP_CharacterInfo = _textMeshPro.textInfo.characterInfo[num];
			int materialReferenceIndex = tMP_CharacterInfo.materialReferenceIndex;
			int vertexIndex = tMP_CharacterInfo.vertexIndex;
			Color32[] colors = _textMeshPro.textInfo.meshInfo[materialReferenceIndex].colors32;
			list.Add(colors.ToArray());
			if (tMP_CharacterInfo.isVisible)
			{
				colors[vertexIndex] = colorForLinkAndVert(i, vertexIndex);
				colors[vertexIndex + 1] = colorForLinkAndVert(i, vertexIndex + 1);
				colors[vertexIndex + 2] = colorForLinkAndVert(i, vertexIndex + 2);
				colors[vertexIndex + 3] = colorForLinkAndVert(i, vertexIndex + 3);
			}
		}
		_textMeshPro.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
		return list;
	}
}
