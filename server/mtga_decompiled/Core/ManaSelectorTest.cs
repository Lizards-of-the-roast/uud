using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using Wotc.Mtgo.Gre.External.Messaging;

public class ManaSelectorTest : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			OpenSelector();
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			OpenSelector();
		}
	}

	private void OpenSelector()
	{
		Object.FindObjectOfType<ManaColorSelector>().OpenSelector(new List<ManaColor>
		{
			ManaColor.White,
			ManaColor.Blue,
			ManaColor.Black,
			ManaColor.Red,
			ManaColor.Green
		}, 4u, SelectionValidationType.ArbitraryRepeats, Input.mousePosition, default(ManaColorSelector.ManaColorSelectorConfig), delegate(IReadOnlyCollection<ManaColor> colorList)
		{
			if (colorList == null)
			{
				Debug.LogWarning("User Canceled Color Selection");
			}
			else
			{
				Debug.LogWarning("User Selected Color(s): " + string.Join(", ", colorList.Select((ManaColor x) => x.ToString()).ToArray()));
			}
		});
	}
}
