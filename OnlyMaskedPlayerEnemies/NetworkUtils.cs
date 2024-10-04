using Unity.Netcode;
using UnityEngine;

namespace com.github.zehsteam.OnlyMaskedPlayerEnemies;

internal static class NetworkUtils
{
    public static bool IsServer
    {
        get
        {
            if (NetworkManager.Singleton == null) return false;

            return NetworkManager.Singleton.IsServer;
        }
    }

    public static bool IsHost
    {
        get
        {
            if (NetworkManager.Singleton == null) return false;

            return NetworkManager.Singleton.IsHost;
        }
    }

    public static ulong GetLocalClientId()
    {
        return NetworkManager.Singleton.LocalClientId;
    }

    public static bool IsLocalClientId(ulong clientId)
    {
        return clientId == GetLocalClientId();
    }

    public static bool IsNetworkPrefab(GameObject prefab)
    {
        foreach (var networkPrefab in NetworkManager.Singleton.NetworkConfig.Prefabs.Prefabs)
        {
            if (networkPrefab.Prefab == prefab)
            {
                return true;
            }
        }

        return false;
    }
}
