namespace GameFoundation.Scripts.Blueprints.CSV.Editor.GoogleSheets
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using GameFoundation.Scripts.Blueprints.CSV.Attributes;
    using GameFoundation.Scripts.Blueprints.CSV.Interfaces;
    using GameFoundation.Scripts.Utils;
    using UnityEditor;
    using UnityEditor.AddressableAssets;
    using UnityEngine;

    public enum SyncStatus { Idle, Syncing, Success, Failed }

    public class BlueprintSyncState
    {
        public Type               BlueprintType { get; set; }
        public string             DataPath      { get; set; }
        public CsvBlueprintSource Source        { get; set; }
        public string             SheetUrl      { get; set; }
        public SyncStatus         Status        { get; set; } = SyncStatus.Idle;
        public string             Error         { get; set; }
        public DateTime?          LastSyncedAt  { get; set; }
    }

    public class GoogleSheetsSyncer
    {
        private const string SyncStatePath = "Library/GoogleSheetsSyncState.json";

        private readonly Dictionary<string, DateTime> syncTimestamps = new();

        public GoogleSheetsSyncer()
        {
            this.LoadSyncState();
        }

        public List<BlueprintSyncState> BuildSyncStates()
        {
            var states = new List<BlueprintSyncState>();
            var types  = ReflectionExtensions.GetAllImplementationsOf(typeof(ICsvBlueprintReader));

            foreach (var type in types)
            {
                var csvAttr   = type.GetCustomAttribute<CsvBlueprintAttribute>();
                var sheetAttr = type.GetCustomAttribute<GoogleSheetSourceAttribute>();
                if (csvAttr == null) continue;

                var state = new BlueprintSyncState
                {
                    BlueprintType = type,
                    DataPath      = csvAttr.DataPath,
                    Source        = csvAttr.Source,
                    SheetUrl      = sheetAttr?.BuildExportUrl(),
                };

                if (this.syncTimestamps.TryGetValue(type.FullName, out var ts))
                    state.LastSyncedAt = ts;

                states.Add(state);
            }

            return states;
        }

        public async Task SyncAsync(BlueprintSyncState state, ISheetFetcher fetcher, CancellationToken token = default)
        {
            if (state.SheetUrl == null) return;

            state.Status = SyncStatus.Syncing;
            state.Error  = null;

            var fetchResult = await fetcher.FetchAsync(state.SheetUrl, token);
            if (!fetchResult.Success)
            {
                state.Status = SyncStatus.Failed;
                state.Error  = fetchResult.Error;
                return;
            }

            var savePath = this.ResolveSavePath(state);
            if (savePath == null)
            {
                state.Status = SyncStatus.Failed;
                state.Error  = $"Could not resolve save path for '{state.DataPath}' ({state.Source})";
                return;
            }

            try
            {
                var dir = Path.GetDirectoryName(savePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                File.WriteAllText(savePath, fetchResult.Content);
                AssetDatabase.ImportAsset(savePath);

                state.Status       = SyncStatus.Success;
                state.LastSyncedAt = DateTime.Now;
                this.syncTimestamps[state.BlueprintType.FullName] = DateTime.Now;
                this.SaveSyncState();
            }
            catch (Exception ex)
            {
                state.Status = SyncStatus.Failed;
                state.Error  = ex.Message;
            }
        }

        public async Task SyncAllAsync(List<BlueprintSyncState> states, ISheetFetcher fetcher, IProgress<int> progress = null, CancellationToken token = default)
        {
            var done = 0;
            foreach (var state in states)
            {
                if (state.SheetUrl == null) continue;
                if (token.IsCancellationRequested) break;
                await this.SyncAsync(state, fetcher, token);
                progress?.Report(++done);
            }
        }

        private string ResolveSavePath(BlueprintSyncState state)
        {
            if (state.Source == CsvBlueprintSource.Addressable)
            {
                var settings = AddressableAssetSettingsDefaultObject.Settings;
                if (settings == null) return null;

                foreach (var group in settings.groups)
                {
                    if (group == null) continue;
                    foreach (var entry in group.entries)
                    {
                        if (string.Equals(entry.address, state.DataPath, StringComparison.OrdinalIgnoreCase))
                            return entry.AssetPath;
                    }
                }
                return null;
            }

            var normalized = NormalizeResourcePath(state.DataPath);
            return $"Assets/Resources/{normalized}.csv";
        }

        private void LoadSyncState()
        {
            if (!File.Exists(SyncStatePath)) return;
            try
            {
                var raw = JsonUtility.FromJson<SyncStateJson>(File.ReadAllText(SyncStatePath));
                if (raw?.entries == null) return;
                foreach (var e in raw.entries)
                    if (DateTime.TryParse(e.lastSyncedAt, out var dt))
                        this.syncTimestamps[e.typeName] = dt;
            }
            catch { /* corrupt state — start fresh */ }
        }

        private void SaveSyncState()
        {
            var raw = new SyncStateJson { entries = new List<SyncStateEntry>() };
            foreach (var (typeName, ts) in this.syncTimestamps)
                raw.entries.Add(new SyncStateEntry { typeName = typeName, lastSyncedAt = ts.ToString("O") });
            File.WriteAllText(SyncStatePath, JsonUtility.ToJson(raw, true));
        }

        private static string NormalizeResourcePath(string dataPath)
        {
            var p = dataPath.Trim().Replace('\\', '/').TrimStart('/');
            if (p.EndsWith(".csv", StringComparison.OrdinalIgnoreCase)) p = p[..^4];
            if (!p.StartsWith("Blueprints/CSV", StringComparison.OrdinalIgnoreCase)) p = $"Blueprints/CSV/{p}";
            return p;
        }

        [Serializable] private class SyncStateJson  { public List<SyncStateEntry> entries; }
        [Serializable] private class SyncStateEntry { public string typeName; public string lastSyncedAt; }
    }
}
