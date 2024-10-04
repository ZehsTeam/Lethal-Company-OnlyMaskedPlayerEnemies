using HarmonyLib;

namespace com.github.zehsteam.OnlyMaskedPlayerEnemies.Patches;

[HarmonyPatch(typeof(StartOfRound))]
internal static class StartOfRoundPatch
{
    [HarmonyPatch(nameof(StartOfRound.Start))]
    [HarmonyPostfix]
    private static void StartPatch()
    {
        EnemyHelper.SetOnlyEnemyMasked();
    }
}
