using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TMP_ClickableHyperlinkText : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	private TextMeshProUGUI _text;

	private void Awake()
	{
		_text = GetComponent<TextMeshProUGUI>();
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		Canvas componentInParent = GetComponentInParent<Canvas>();
		Camera camera = ((componentInParent.renderMode == RenderMode.ScreenSpaceOverlay) ? null : componentInParent.worldCamera);
		int num = TMP_TextUtilities.FindIntersectingLink(position: new Vector3(eventData.position.x, eventData.position.y, 0f), text: _text, camera: camera);
		if (num != -1)
		{
			TMP_LinkInfo tMP_LinkInfo = _text.textInfo.linkInfo[num];
			string linkID = tMP_LinkInfo.GetLinkID();
			if (linkID.StartsWith("http://") || linkID.StartsWith("https://"))
			{
				Application.OpenURL(linkID);
			}
		}
	}
}
