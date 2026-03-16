using UnityEngine;

public class ToMouseFromEntityIntention : FromEntityIntentionBase
{
	public override void UpdateArrow()
	{
		if ((bool)CurrentCamera.Value && Physics.Raycast(CurrentCamera.Value.ScreenPointToRay((Input.touchCount > 0) ? ((Vector3)Input.GetTouch(0).position) : Input.mousePosition), out var hitInfo))
		{
			base.ArrowBehavior.SetEnd(hitInfo.point);
			base.ArrowBehavior.Flush();
			Vector3 a = CurrentCamera.Value.WorldToScreenPoint(base.ArrowBehavior.GetStartPosition());
			Vector3 b = CurrentCamera.Value.WorldToScreenPoint(base.ArrowBehavior.GetEndPosition());
			AudioManager.SetRTPCValue("draw_target", Vector3.Distance(a, b) / (float)Screen.width * 100f);
		}
		base.UpdateArrow();
	}
}
