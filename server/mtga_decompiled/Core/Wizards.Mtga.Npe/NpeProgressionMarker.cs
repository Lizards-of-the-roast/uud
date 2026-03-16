using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Wizards.Mtga.Npe;

[CustomStyle("NpeProgressionMarker")]
public class NpeProgressionMarker : Marker, INotification, INotificationOptionProvider
{
	[SerializeField]
	private NpeProgressionFlag _progressionFlag;

	public NpeProgressionFlag ProgressionFlag => _progressionFlag;

	public PropertyName id => default(PropertyName);

	public NotificationFlags flags => NotificationFlags.Retroactive | NotificationFlags.TriggerOnce;
}
