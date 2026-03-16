using System;
using System.Linq;
using UnityEngine;
using Wizards.GeneralUtilities.ObjectCommunication;

namespace Wizards.DynamicTimelineBinding;

[CreateAssetMenu(menuName = "Beacon/Home Page Event ID Filter", fileName = "Home Page Event ID Filter", order = 20)]
public class HomePageEventIdFilter : BindingFilter
{
	[SerializeField]
	private string _eventId;

	[SerializeField]
	private bool _checkParents;

	public override UnityEngine.Object[] Filter(BeaconIdentifier beaconIdentifier)
	{
		return (from billboard in beaconIdentifier.GetBeaconObject<HomePageBillboard>(_checkParents)
			where string.Equals(_eventId, billboard.EventId, StringComparison.InvariantCulture)
			select billboard).ToArray();
	}
}
