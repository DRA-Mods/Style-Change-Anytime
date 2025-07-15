using System;
using System.Reflection;
using JetBrains.Annotations;
using Verse;

namespace StyleChangeAnytime;

public static class MethodUtilities
{
    [Pure]
    [ContractAnnotation("method:null => halt; method:notnull => notnull")]
    public static string GetMethodNameWithNamespace(this MethodBase method)
        => (method.DeclaringType?.Namespace).NullOrEmpty() ? method.Name : $"{method.DeclaringType.Name}:{method.Name}";

    [Pure]
    [ContractAnnotation("method:null => halt; method:notnull => notnull")]
    public static MethodBase MethodOf(Delegate method) => method.Method;
}