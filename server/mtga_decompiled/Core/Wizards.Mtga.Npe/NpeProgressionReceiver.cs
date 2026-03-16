using UnityEngine;
using UnityEngine.Playables;

namespace Wizards.Mtga.Npe;

[AddComponentMenu("")]
public class NpeProgressionReceiver : MonoBehaviour, INotificationReceiver
{
	public void OnNotify(Playable origin, INotification notification, object context)
	{
		NpeProgressionMarker npeProgressionMarker = notification as NpeProgressionMarker;
		if (!(npeProgressionMarker == null))
		{
			npeProgressionMarker.ProgressionFlag.MarkFlagCompletionStatus();
		}
	}
}
