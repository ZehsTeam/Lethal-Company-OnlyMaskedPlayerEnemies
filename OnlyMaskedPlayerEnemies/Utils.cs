using UnityEngine;

namespace com.github.zehsteam.OnlyMaskedPlayerEnemies;

internal static class Utils
{
    public static bool RandomPercent(float percent)
    {
        if (percent <= 0f) return false;
        if (percent >= 100f) return true;
        return Random.value * 100f <= percent;
    }
}
