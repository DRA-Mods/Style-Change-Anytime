using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Multiplayer.API;

namespace StyleChangeAnytime.Compat;

public class MpUtils
{
    public static bool CanUseDevMode
    {
        [Pure]
        get => MP.enabled && CanUseDevModeInternal;
    }

    // Do not inline, as `MP.CanUseDevMode` may not exist if a mod was loaded before with outdated MP API.
    // If this got inlined we would end up with errors.
    private static bool CanUseDevModeInternal
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        get => MP.CanUseDevMode;
    }
}