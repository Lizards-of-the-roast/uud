using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Wizards.Mtga.BI;

public static class BIEventTracker
{
	private class MockTracker : IEventTracker
	{
		public void InitializeFirstPartyTracking(string playerEmailAddress)
		{
		}

		public Task RequestTrackingAuthorization()
		{
			return Task.CompletedTask;
		}

		public void TrackEvent(EBiEvent biEvent, string playerExternalID)
		{
		}

		public void TrackPurchaseEvent(string productID)
		{
		}
	}

	public const string TrackerObjectName = "BIEventTracker";

	private static List<IEventTracker> trackers = new List<IEventTracker>();

	public static void Initialize()
	{
		if (trackers.Count > 0)
		{
			Debug.LogWarning("Tried to perform double-initialization of BIEventTracker: ignoring second instance.");
			return;
		}
		Global.VersionInfo.IsValidForRelease();
		_ = Application.platform;
		trackers.Add(new MockTracker());
	}

	public static void InitializeFirstPartyTracking(string playerEmailAddress)
	{
		foreach (IEventTracker tracker in trackers)
		{
			tracker.InitializeFirstPartyTracking(playerEmailAddress);
		}
	}

	public static Task RequestTrackingAuthorization()
	{
		return Task.WhenAll(trackers.Select((IEventTracker t) => t.RequestTrackingAuthorization()));
	}

	public static void TrackEvent(EBiEvent biEvent, string playerId = null)
	{
		foreach (IEventTracker tracker in trackers)
		{
			tracker.TrackEvent(biEvent, playerId);
		}
	}

	public static void TrackPurchaseEvent(string productId)
	{
		foreach (IEventTracker tracker in trackers)
		{
			tracker.TrackPurchaseEvent(productId);
		}
	}
}
