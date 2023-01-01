using DataObjects;
using Empyrean.Engine_Classes;
using Empyrean.Game.Tiles;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Game.Settings
{
    public enum Setting 
    {
        EnableTileTooltips,
        MovementTurbo,
        VsyncEnabled,
        TileMapLoadDiameter,
        VisibleTileMaps
    }

    public static class SettingsManager
    {
        public static Dictionary<Setting, string> Names = new Dictionary<Setting, string>()
        {
            {Setting.EnableTileTooltips, "EnableTileTooltips" },
            {Setting.MovementTurbo, "MovementTurbo" },
            {Setting.VsyncEnabled, "VsyncEnabled" },
            {Setting.TileMapLoadDiameter, "TileMapLoadDiameter" },
            {Setting.VisibleTileMaps, "VisibleTileMaps" },
        };

        private static Dictionary<string, Setting> _reverseName = new Dictionary<string, Setting>();


        public static readonly string SettingsSearchPath = ":settings.0";

        private static Dictionary<string, object> _settings = new Dictionary<string, object>();

        public static void Initialize()
        {
            foreach (var kvp in Names)
            {
                _reverseName.Add(kvp.Value, kvp.Key);
            }

            FillSettingsDictionary();
        }

        private static void FillSettingsDictionary()
        {
            DataSearchRequest req = new DataSearchRequest(SettingsSearchPath);

            if (req.GetEntry(out var entry)) 
            {
                Dictionary<string, object> settingsDict = entry.Parent;

                if(settingsDict != null)
                {
                    foreach(var kvp in settingsDict)
                    {
                        _settings.Add(kvp.Key, kvp.Value);

                        if(_reverseName.TryGetValue(kvp.Key, out var value))
                        {
                            EvaluateSetting(value);
                        }
                    }
                }
            }
        }

        public static void SetSetting(Setting setting, object value)
        {
            _settings.AddOrSet(Names[setting], value);

            DataSearchRequest req = new DataSearchRequest(SettingsSearchPath + "." + setting);
            req.GetOrCreateEntry(out var entry, value);

            entry.SetValue(value);

            EvaluateSetting(setting);

            DataSourceManager.DataSources["settings"].SaveAllPendingBlocks();
        }

        public static void SetSetting(string setting, object value)
        {
            _settings.AddOrSet(setting, value);

            DataSearchRequest req = new DataSearchRequest(SettingsSearchPath + "." + setting);
            req.GetOrCreateEntry(out var entry, value);

            entry.SetValue(value);

            EvaluateSetting(_reverseName[setting]);

            DataSourceManager.GetSource("settings").SaveAllPendingBlocks();
        }

        public static object GetSetting(string setting)
        {
            _settings.TryGetValue(setting, out var value);

            return value;
        }

        public static T GetSetting<T>(string setting)
        {
            _settings.TryGetValue(setting, out var value);

            if (value == null)
                return default(T);

            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return default(T);
            }
        }

        public static T GetSetting<T>(Setting setting)
        {
            _settings.TryGetValue(Names[setting], out var value);

            if (value == null)
                return default(T);

            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch 
            {
                return default(T);
            }
        }

        private static void EvaluateSetting(Setting setting)
        {
            switch (setting) 
            {
                case Setting.TileMapLoadDiameter:
                    int diameter = GetSetting<int>(Setting.TileMapLoadDiameter);

                    TileMapManager.LOAD_DIAMETER = diameter == 0 ? TileMapManager.LOAD_DIAMETER : diameter;
                    break;
                case Setting.VisibleTileMaps:
                    int tileMaps = GetSetting<int>(Setting.VisibleTileMaps);

                    TileMapManager.VISIBLE_TILE_MAPS = tileMaps == 0 ? TileMapManager.VISIBLE_TILE_MAPS : tileMaps;
                    break;
            }
        }
    }
}
