using System.Collections;
using Core.Shared.Code.ServiceFactories;
using Cysharp.Threading.Tasks;
using com.wizards.harness;

namespace Core.Code.Harnesses;

public class HarnessLocInitializer : HarnessInitializer
{
	public override IEnumerator OnInitialize()
	{
		yield return new LoadLocDatabaseUniTask().Load().ToCoroutine();
	}

	public override string Status()
	{
		return "Initializing localization.";
	}
}
