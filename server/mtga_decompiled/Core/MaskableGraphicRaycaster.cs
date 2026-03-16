using UnityEngine;
using UnityEngine.UI;

public class MaskableGraphicRaycaster : GraphicRaycaster
{
	public void SetLayerMask(LayerMask mask)
	{
		m_BlockingMask = mask;
	}
}
