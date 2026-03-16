using System.Collections.Generic;
using System.Linq;
using Assets.Core.Meta.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Wotc.Mtga.CustomInput;
using Wotc.Mtga.Loc;

public class LoginLinks : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	public Camera Camera;

	public TMP_Text Text;

	private Color32 _originalColor;

	public Color HoverColor = new Color32(209, 209, 209, byte.MaxValue);

	private Color32 _hoverColor = new Color32(209, 209, 209, byte.MaxValue);

	private bool _firstHover = true;

	private int _hoverLink = -1;

	private void Start()
	{
		if (Camera == null)
		{
			Camera = GetComponentInParent<Canvas>().worldCamera;
		}
		if (Text == null)
		{
			Text = GetComponent<TMP_Text>();
		}
		_hoverColor = HoverColor;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		int num = TMP_TextUtilities.FindIntersectingLink(Text, CustomInputModule.GetPointerPosition(), Camera);
		if (num != -1)
		{
			TMP_LinkInfo tMP_LinkInfo = Text.textInfo.linkInfo[num];
			string linkID = tMP_LinkInfo.GetLinkID();
			string text = linkID switch
			{
				"CoC" => Languages.ActiveLocProvider.GetLocalizedText("MainNav/WebLink/CodeOfConduct"), 
				"PP8" => Languages.ActiveLocProvider.GetLocalizedText("MainNav/WebLink/PrivacyPolicy"), 
				"TC" => Languages.ActiveLocProvider.GetLocalizedText("MainNav/WebLink/TermsAndConditions"), 
				"PREORDER" => Languages.ActiveLocProvider.GetLocalizedText("MainNav/WebLink/PreOrder"), 
				"CustomerSupport" => Languages.ActiveLocProvider.GetLocalizedText("MainNav/WebLink/CustomerSupport"), 
				_ => linkID, 
			};
			if (!string.IsNullOrEmpty(text))
			{
				UrlOpener.OpenURL(text);
			}
		}
	}

	private void Update()
	{
		int num = TMP_TextUtilities.FindIntersectingLink(Text, CustomInputModule.GetPointerPosition(), Camera);
		if (num != -1)
		{
			if (_hoverLink != num && _hoverLink != -1)
			{
				SetLinkToColor(_hoverLink, _originalColor);
			}
			SetLinkToColor(num, _hoverColor);
			_hoverLink = num;
		}
		else if (_hoverLink != -1)
		{
			SetLinkToColor(_hoverLink, _originalColor);
			_hoverLink = -1;
		}
	}

	private List<Color32[]> SetLinkToColor(int linkIndex, Color32 color)
	{
		if (Text == null)
		{
			Text = GetComponent<TMP_Text>();
		}
		TMP_LinkInfo tMP_LinkInfo = Text.textInfo.linkInfo[linkIndex];
		List<Color32[]> list = new List<Color32[]>();
		for (int i = 0; i < tMP_LinkInfo.linkTextLength; i++)
		{
			int num = tMP_LinkInfo.linkTextfirstCharacterIndex + i;
			TMP_CharacterInfo tMP_CharacterInfo = Text.textInfo.characterInfo[num];
			int materialReferenceIndex = tMP_CharacterInfo.materialReferenceIndex;
			int vertexIndex = tMP_CharacterInfo.vertexIndex;
			Color32[] colors = Text.textInfo.meshInfo[materialReferenceIndex].colors32;
			list.Add(colors.ToArray());
			if (_firstHover && colors.Length != 0)
			{
				_firstHover = false;
				_originalColor = colors[vertexIndex];
			}
			if (tMP_CharacterInfo.isVisible)
			{
				colors[vertexIndex] = color;
				colors[vertexIndex + 1] = color;
				colors[vertexIndex + 2] = color;
				colors[vertexIndex + 3] = color;
			}
		}
		Text.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
		return list;
	}
}
