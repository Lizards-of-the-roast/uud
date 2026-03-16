using System;
using System.Linq;
using System.Reflection;

namespace Core.Code.Utils;

public class AssemblyUtils
{
	public static Assembly GetAssemblyByName(string name)
	{
		return AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault((Assembly assembly) => assembly.GetName().Name == name);
	}
}
