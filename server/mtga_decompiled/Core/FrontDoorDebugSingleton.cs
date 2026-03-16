using Newtonsoft.Json;
using Wizards.Mtga;
using Wizards.Unification.Models.JsonConverters;
using Wizards.Unification.Models.Player;
using Wotc.Mtga.Network.ServiceWrappers;

public static class FrontDoorDebugSingleton
{
	private static readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
	{
		ContractResolver = new FrontDoorConnectionContractResolver(null, null)
	};

	public static void SpoofIncomingFrontDoorMessage(string pushNotificationJson)
	{
		PushNotification pushNotification = JsonConvert.DeserializeObject<PushNotification>(pushNotificationJson, _jsonSerializerSettings);
		Pantry.Get<IFrontDoorConnectionServiceWrapper>().FDCAWS.DebugSpoofIncomingMessage(pushNotification);
	}
}
