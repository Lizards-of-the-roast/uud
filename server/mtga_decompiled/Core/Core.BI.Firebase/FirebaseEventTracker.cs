using System;
using System.Threading.Tasks;
using Core.Code.Utils;
using Firebase.Analytics;
using Wizards.Mtga.BI;

namespace Core.BI.Firebase;

public class FirebaseEventTracker : IEventTracker
{
	public FirebaseEventTracker()
	{
		FirebaseAnalytics.SetAnalyticsCollectionEnabled(enabled: true);
		FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventAppOpen);
	}

	public void InitializeFirstPartyTracking(string playerEmailAddress)
	{
		FirebaseAnalytics.InitiateOnDeviceConversionMeasurementWithHashedEmailAddress(ObfuscatedEmailHasher.Hash(playerEmailAddress));
	}

	public Task RequestTrackingAuthorization()
	{
		return Task.CompletedTask;
	}

	public void TrackEvent(EBiEvent biEvent, string playerExternalID = null)
	{
		switch (biEvent)
		{
		case EBiEvent.CompletedNpe:
			FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventTutorialComplete);
			break;
		case EBiEvent.PlayerLogin:
			FirebaseAnalytics.SetUserId(playerExternalID);
			FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventLogin);
			break;
		case EBiEvent.PlayerRegistration:
			FirebaseAnalytics.SetUserId(playerExternalID);
			FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventSignUp);
			break;
		default:
			throw new ArgumentOutOfRangeException("biEvent", biEvent, null);
		case EBiEvent.PaidEventEntry:
			break;
		}
	}

	public void TrackPurchaseEvent(string productID)
	{
		FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventPurchase);
	}
}
