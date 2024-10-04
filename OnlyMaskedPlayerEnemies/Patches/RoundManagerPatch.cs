using HarmonyLib;

namespace com.github.zehsteam.OnlyMaskedPlayerEnemies.Patches;

[HarmonyPatch(typeof(RoundManager))]
internal static class RoundManagerPatch
{
    [HarmonyPatch(nameof(RoundManager.AdvanceHourAndSpawnNewBatchOfEnemies))]
    [HarmonyPostfix]
    private static void AdvanceHourAndSpawnNewBatchOfEnemiesPatch()
    {
        if (!NetworkUtils.IsServer) return;
        if (!Plugin.ConfigManager.Hourly_Enabled.Value) return;

        Plugin.logger.LogInfo("Trying to force spawn Masked player enemies hourly.");

        EnemyType enemyType = EnemyHelper.GetEnemyType("Masked");

        if (enemyType == null)
        {
            Plugin.logger.LogError("Failed to force spawn Masked player enemies hourly. EnemyType is null.");
            return;
        }

        int spawnCount = Plugin.ConfigManager.Hourly_SpawnCount.Value;
        int maxSpawnCount = Plugin.ConfigManager.MaxSpawnCount.Value;

        if (enemyType.numberSpawned >= maxSpawnCount)
        {
            Plugin.logger.LogWarning($"Failed to force spawn Masked player enemies hourly. Max spawn count of {maxSpawnCount} has been reached.");
            return;
        }

        for (int i = 0; i < spawnCount; i++)
        {
            if (enemyType.numberSpawned >= maxSpawnCount)
            {
                Plugin.logger.LogWarning($"Failed to force spawn Masked player enemies hourly. Max spawn count of {maxSpawnCount} has been reached.");
                return;
            }

            if (Utils.RandomPercent(Plugin.ConfigManager.Hourly_SpawnChance.Value))
            {
                EnemyHelper.SpawnEnemyOnServer("Masked");
            }
        }
    }
}
