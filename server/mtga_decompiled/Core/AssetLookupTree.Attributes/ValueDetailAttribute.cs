using System;
using System.Diagnostics;

namespace AssetLookupTree.Attributes;

[Conditional("UNITY_EDITOR")]
[AttributeUsage(AttributeTargets.Class)]
public abstract class ValueDetailAttribute : Attribute
{
}
