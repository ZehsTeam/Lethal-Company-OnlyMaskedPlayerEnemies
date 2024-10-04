using GameNetcodeStuff;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.github.zehsteam.OnlyMaskedPlayerEnemies;

internal static class EnemyHelper
{
    public static void SetOnlyEnemyMasked()
    {
        SetOnlyEnemy("Masked");
    }

    public static void SetOnlyEnemy(string enemyName)
    {
        foreach (var level in StartOfRound.Instance.levels)
        {
            SetOnlyEnemyForLevel(level, enemyName, rarity: 10000, maxSpawnCount: Plugin.ConfigManager.MaxSpawnCount.Value);
        }
    }
    
    private static void SetOnlyEnemyForLevel(SelectableLevel level, string enemyName, int rarity, int maxSpawnCount)
    {
        SetOnlyEnemyForList(ref level.Enemies, enemyName, rarity, maxSpawnCount);
        SetOnlyEnemyForList(ref level.OutsideEnemies, enemyName, rarity, maxSpawnCount);
    }

    private static void SetOnlyEnemyForList(ref List<SpawnableEnemyWithRarity> spawnableEnemyWithRarities, string enemyName, int rarity, int maxSpawnCount)
    {
        bool hasEnemy = false;

        foreach (var spawnableEnemyWithRarity in spawnableEnemyWithRarities)
        {
            if (spawnableEnemyWithRarity.enemyType.enemyName == enemyName)
            {
                hasEnemy = true;
                spawnableEnemyWithRarity.rarity = rarity;
                spawnableEnemyWithRarity.enemyType.MaxCount = maxSpawnCount;

                Plugin.logger.LogMessage($"Set enemy \"{enemyName}\" rarity to {rarity}.");
                continue;
            }

            spawnableEnemyWithRarity.rarity = 0;
            Plugin.logger.LogInfo($"Set enemy \"{spawnableEnemyWithRarity.enemyType.enemyName}\" rarity to {spawnableEnemyWithRarity.rarity}.");
        }

        if (!hasEnemy)
        {
            AddEnemyToList(ref spawnableEnemyWithRarities, enemyName, rarity, maxSpawnCount);
        }
    }

    private static void AddEnemyToList(ref List<SpawnableEnemyWithRarity> spawnableEnemyWithRarities, string enemyName, int rarity, int maxSpawnCount)
    {
        EnemyType enemyType = GetEnemyType(enemyName);

        if (enemyType == null)
        {
            Plugin.logger.LogError($"Failed to add enemy \"{enemyName}\" to list.");
            return;
        }

        SpawnableEnemyWithRarity spawnableEnemyWithRarity = new SpawnableEnemyWithRarity();
        spawnableEnemyWithRarity.rarity = rarity;
        spawnableEnemyWithRarity.enemyType = enemyType;
        spawnableEnemyWithRarity.enemyType.MaxCount = maxSpawnCount;

        spawnableEnemyWithRarities.Add(spawnableEnemyWithRarity);

        Plugin.logger.LogInfo($"Added enemy \"{enemyName}\" to list.");
    }

    #region Spawn
    public static void SpawnEnemyOnServer(string enemyName)
    {
        if (!NetworkUtils.IsServer)
        {
            Plugin.logger.LogError("Failed to spawn enemy. Only the host can spawn enemies.");
            return;
        }

        EnemyType enemyType = GetEnemyType(enemyName);

        if (enemyType == null)
        {
            Plugin.logger.LogError($"Failed to spawn \"{enemyName}\" enemy. EnemyType is null.");
            return;
        }

        if (enemyType.enemyPrefab == null)
        {
            Plugin.logger.LogError($"Failed to spawn \"{enemyName}\" enemy. EnemyType.enemyPrefab is null.");
            return;
        }

        Plugin.logger.LogInfo($"Trying to spawn \"{enemyName}\" enemy.");

        Vector3 spawnPosition = GetRandomSpawnPosition();
        float yRot = Random.Range(0f, 360f);
        RoundManager.Instance.SpawnEnemyGameObject(spawnPosition, yRot, -1, enemyType);

        Plugin.logger.LogInfo($"Spawned \"{enemyName}\" enemy at position: (x: {spawnPosition.x}, y: {spawnPosition.y}, z: {spawnPosition.z})");
    }

    private static Vector3 GetRandomSpawnPosition()
    {
        if (RoundManager.Instance == null)
        {
            return Vector3.zero;
        }

        if (Utils.RandomPercent(50f))
        {
            return GetRandomSpawnPositionInside();
        }

        return GetRandomSpawnPositionOutside();
    }

    private static Vector3 GetRandomSpawnPositionInside()
    {
        if (RoundManager.Instance.insideAINodes == null || RoundManager.Instance.insideAINodes.Length == 0)
        {
            return Vector3.zero;
        }

        GameObject[]  nodeObjects = RoundManager.Instance.insideAINodes.Where(_ => _ != null).ToArray();

        if (nodeObjects.Length == 0)
        {
            return Vector3.zero;
        }

        return nodeObjects[Random.Range(0, nodeObjects.Length)].transform.position;
    }

    private static Vector3 GetRandomSpawnPositionOutside()
    {
        if (RoundManager.Instance.outsideAINodes == null || RoundManager.Instance.outsideAINodes.Length == 0)
        {
            return Vector3.zero;
        }

        GameObject[] nodeObjects = RoundManager.Instance.outsideAINodes.Where(_ => _ != null).ToArray();

        if (nodeObjects.Length == 0)
        {
            return Vector3.zero;
        }

        return nodeObjects[Random.Range(0, nodeObjects.Length)].transform.position;
    }
    #endregion

    public static EnemyType GetEnemyType(string enemyName)
    {
        foreach (var enemyType in GetEnemyTypes())
        {
            if (enemyType.enemyName == enemyName)
            {
                return enemyType;
            }
        }

        try
        {
            EnemyType enemyType = Resources.FindObjectsOfTypeAll<EnemyType>().Single((EnemyType x) => x.enemyName == enemyName);

            if (IsValidEnemyType(enemyType) && NetworkUtils.IsNetworkPrefab(enemyType.enemyPrefab))
            {
                Plugin.logger.LogInfo($"Found EnemyType \"{enemyType.enemyName}\" from Resources.");

                return enemyType;
            }
        }
        catch { }

        return null;
    }

    public static List<EnemyType> GetEnemyTypes()
    {
        var enemyTypes = new HashSet<EnemyType>(new EnemyTypeComparer());

        foreach (var level in StartOfRound.Instance.levels)
        {
            var levelEnemyTypes = level.Enemies
                .Concat(level.DaytimeEnemies)
                .Concat(level.OutsideEnemies)
                .Select(e => e.enemyType)
                .Where(IsValidEnemyType);

            foreach (var levelEnemyType in levelEnemyTypes)
            {
                enemyTypes.Add(levelEnemyType);
            }
        }

        return enemyTypes.ToList();
    }

    public static bool IsValidEnemyType(EnemyType enemyType)
    {
        if (enemyType == null) return false;
        if (string.IsNullOrWhiteSpace(enemyType.enemyName)) return false;
        if (enemyType.enemyPrefab == null) return false;

        return true;
    }
}

public class EnemyTypeComparer : IEqualityComparer<EnemyType>
{
    public bool Equals(EnemyType x, EnemyType y)
    {
        if (x == null || y == null) return false;
        return x.enemyName == y.enemyName;
    }

    public int GetHashCode(EnemyType obj)
    {
        return obj.enemyName?.GetHashCode() ?? 0;
    }
}
