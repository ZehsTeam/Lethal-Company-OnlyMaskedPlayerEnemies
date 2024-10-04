using BepInEx.Configuration;
using System.Collections.Generic;
using System.Reflection;

namespace com.github.zehsteam.OnlyMaskedPlayerEnemies;

internal class ConfigManager
{
    // Masked Player Enemy Settings
    public ConfigEntry<int> MaxSpawnCount { get; private set; }

    // Hourly Spawn Settings
    public ConfigEntry<bool> Hourly_Enabled { get; private set; }
    public ConfigEntry<int> Hourly_SpawnCount { get; private set; }
    public ConfigEntry<float> Hourly_SpawnChance { get; private set; }

    public ConfigManager()
    {
        BindConfigs();
        SetupChangedEvents();
        ClearUnusedEntries();
    }

    private void BindConfigs()
    {
        ConfigHelper.SkipAutoGen();

        // Masked Player Enemy Settings
        MaxSpawnCount = ConfigHelper.Bind("Masked Player Enemy Settings", "MaxSpawnCount", defaultValue: 100, requiresRestart: false, "The max spawn count for Masked player enemies.");

        // Hourly Spawn Settings
        Hourly_Enabled =     ConfigHelper.Bind("Hourly Spawn Settings", "Enabled",     defaultValue: false, requiresRestart: false, "If enabled, Masked player enemies will force spawn hourly.");
        Hourly_SpawnCount =  ConfigHelper.Bind("Hourly Spawn Settings", "SpawnCount",  defaultValue: 4,     requiresRestart: false, "The amount of Masked player enemies to try and spawn.");
        Hourly_SpawnChance = ConfigHelper.Bind("Hourly Spawn Settings", "SpawnChance", defaultValue: 50f,   requiresRestart: false, "The percent chance a Masked player enemy will spawn hourly.");
    }

    private void SetupChangedEvents()
    {
        // Masked Player Enemy Settings
        MaxSpawnCount.SettingChanged += MaxSpawnCount_SettingChanged;
    }

    private void MaxSpawnCount_SettingChanged(object sender, System.EventArgs e)
    {
        EnemyHelper.SetOnlyEnemyMasked();
    }

    private void ClearUnusedEntries()
    {
        ConfigFile configFile = Plugin.Instance.Config;

        // Normally, old unused config entries don't get removed, so we do it with this piece of code. Credit to Kittenji.
        PropertyInfo orphanedEntriesProp = configFile.GetType().GetProperty("OrphanedEntries", BindingFlags.NonPublic | BindingFlags.Instance);
        var orphanedEntries = (Dictionary<ConfigDefinition, string>)orphanedEntriesProp.GetValue(configFile, null);
        orphanedEntries.Clear(); // Clear orphaned entries (Unbinded/Abandoned entries)
        configFile.Save(); // Save the config file to save these changes
    }
}
