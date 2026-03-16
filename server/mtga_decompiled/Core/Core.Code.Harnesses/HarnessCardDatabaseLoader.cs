using System.Collections;
using Core.Shared.Code.ServiceFactories;
using Cysharp.Threading.Tasks;
using com.wizards.harness;

namespace Core.Code.Harnesses;

public class HarnessCardDatabaseLoader : HarnessInitializer
{
	public override IEnumerator OnInitialize()
	{
		LoadCardDatabaseUniTask loadCardDatabaseUniTask = new LoadCardDatabaseUniTask();
		yield return loadCardDatabaseUniTask.Load().ToCoroutine();
	}

	public override string Status()
	{
		return "Loading card database.";
	}
}
