namespace GameFoundation.Scripts.Blueprints.CSV.Editor.GoogleSheets
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using GameFoundation.Scripts.Blueprints.CSV.Editor.Validator;
    using UnityEditor;
    using UnityEngine;

    public class GoogleSheetsSyncWindow : EditorWindow
    {
        private GoogleSheetsSyncer       syncer;
        private List<BlueprintSyncState> states;
        private SheetAuthMode            authMode = SheetAuthMode.Public;
        private Vector2                  scrollPos;
        private CancellationTokenSource  cts;
        private bool                     isSyncing;

        [MenuItem("Tools/QuantumGame/Sync CSV from Google Sheets")]
        public static void Open() => GetWindow<GoogleSheetsSyncWindow>("Google Sheets Sync").Show();

        private void OnEnable()
        {
            this.syncer = new GoogleSheetsSyncer();
            this.Refresh();
        }

        private void OnDisable()
        {
            this.cts?.Cancel();
            this.cts?.Dispose();
        }

        private void Refresh()
        {
            this.states = this.syncer.BuildSyncStates();
            this.Repaint();
        }

        private void OnGUI()
        {
            this.DrawToolbar();

            if (this.states == null || this.states.Count == 0)
            {
                EditorGUILayout.HelpBox("No CSV blueprints found.", MessageType.None);
                return;
            }

            DrawTableHeader();

            this.scrollPos = EditorGUILayout.BeginScrollView(this.scrollPos);
            foreach (var state in this.states)
                this.DrawRow(state);
            EditorGUILayout.EndScrollView();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUILayout.LabelField("Auth:", GUILayout.Width(36));
            this.authMode = (SheetAuthMode)EditorGUILayout.EnumPopup(
                this.authMode, EditorStyles.toolbarPopup, GUILayout.Width(90));
            GUILayout.FlexibleSpace();
            EditorGUI.BeginDisabledGroup(this.isSyncing);
            if (GUILayout.Button("Refresh",  EditorStyles.toolbarButton)) this.Refresh();
            if (GUILayout.Button("Sync All", EditorStyles.toolbarButton)) this.RunSyncAll();
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }

        private static void DrawTableHeader()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("Blueprint",   EditorStyles.toolbarButton, GUILayout.Width(200));
            GUILayout.Label("Source",      EditorStyles.toolbarButton, GUILayout.Width(90));
            GUILayout.Label("Last Synced", EditorStyles.toolbarButton, GUILayout.Width(130));
            GUILayout.Label("Status",      EditorStyles.toolbarButton, GUILayout.Width(80));
            GUILayout.Label("",            EditorStyles.toolbarButton);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawRow(BlueprintSyncState state)
        {
            var hasSheet = state.SheetUrl != null;
            EditorGUI.BeginDisabledGroup(!hasSheet || this.isSyncing);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(state.BlueprintType.Name,                                                 GUILayout.Width(200));
            GUILayout.Label(state.Source.ToString(),                                                  GUILayout.Width(90));
            GUILayout.Label(state.LastSyncedAt.HasValue
                ? state.LastSyncedAt.Value.ToString("yyyy-MM-dd HH:mm")
                : "Never",                                                                             GUILayout.Width(130));
            GUILayout.Label(StatusLabel(state),                                                       GUILayout.Width(80));

            if (hasSheet)
            {
                if (GUILayout.Button("↓", GUILayout.Width(28))) this.RunSyncOne(state);
            }
            else
            {
                var prev  = GUI.color;
                GUI.color = Color.gray;
                GUILayout.Label("No [GoogleSheetSource]");
                GUI.color = prev;
            }

            EditorGUILayout.EndHorizontal();

            if (state.Status == SyncStatus.Failed && !string.IsNullOrEmpty(state.Error))
            {
                EditorGUI.indentLevel++;
                var prev  = GUI.color;
                GUI.color = new Color(1f, 0.4f, 0.4f);
                EditorGUILayout.LabelField(state.Error, EditorStyles.wordWrappedMiniLabel);
                GUI.color = prev;
                EditorGUI.indentLevel--;
            }

            EditorGUI.EndDisabledGroup();
        }

        private static string StatusLabel(BlueprintSyncState state) => state.Status switch
        {
            SyncStatus.Idle    => state.SheetUrl == null ? "—" : "Idle",
            SyncStatus.Syncing => "Syncing…",
            SyncStatus.Success => "✓ OK",
            SyncStatus.Failed  => "✗ Error",
            _                  => "?",
        };

        private void RunSyncOne(BlueprintSyncState state)
        {
            _ = this.ExecuteSync(async () =>
            {
                var fetcher = GoogleSheetsFetcherFactory.Create(this.authMode);
                await this.syncer.SyncAsync(state, fetcher, this.cts.Token);
            });
        }

        private void RunSyncAll()
        {
            _ = this.ExecuteSync(async () =>
            {
                var fetcher = GoogleSheetsFetcherFactory.Create(this.authMode);
                await this.syncer.SyncAllAsync(this.states, fetcher, null, this.cts.Token);
            });
        }

        private async Task ExecuteSync(Func<Task> action)
        {
            this.cts?.Cancel();
            this.cts?.Dispose();
            this.cts       = new CancellationTokenSource();
            this.isSyncing = true;
            this.Repaint();

            try
            {
                await action();
                this.RunValidatorAfterSync();
            }
            finally
            {
                this.isSyncing = false;
                this.Repaint();
            }
        }

        private void RunValidatorAfterSync()
        {
            var report   = CsvBlueprintValidator.ValidateAll();
            var hasError = false;
            foreach (var r in report.BlueprintResults)
                if (r.Status == ValidationStatus.Error) { hasError = true; break; }

            if (hasError)
                EditorUtility.DisplayDialog(
                    "CSV Sync",
                    "Sync completed, but validation found errors.\nOpen Tools > QuantumGame > Validate CSV Blueprints for details.",
                    "OK");
        }
    }
}
