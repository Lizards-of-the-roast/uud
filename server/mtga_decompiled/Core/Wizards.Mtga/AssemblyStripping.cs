using Newtonsoft.Json.Utilities;
using UnityEngine.Scripting;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wizards.Mtga;

[Preserve]
internal static class AssemblyStripping
{
	static AssemblyStripping()
	{
		GuardReflectedTypes();
	}

	private static void GuardReflectedTypes()
	{
		AotHelper.EnsureList<int>();
		AotHelper.EnsureList<uint>();
		AotHelper.EnsureList<CardFrameKey>();
		AotHelper.EnsureList<CardRarity>();
		AotHelper.EnsureList<CardType>();
		AotHelper.EnsureList<GameObjectInfo>();
		AotHelper.EnsureList<GameObjectType>();
		AotHelper.EnsureList<ZoneType>();
	}
}
