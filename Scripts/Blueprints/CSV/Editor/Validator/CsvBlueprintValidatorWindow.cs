namespace GameFoundation.Scripts.Blueprints.CSV.Editor.Validator
{
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    public class CsvBlueprintValidatorWindow : EditorWindow
    {
        private bool              validateDataTypes;
        private CsvValidationReport report;
        private Vector2           scrollPos;
        private readonly HashSet<int> expanded = new();

        [MenuItem("Tools/QuantumGame/Validate CSV Blueprints")]
        public static void Open() => GetWindow<CsvBlueprintValidatorWindow>("CSV Validator").Show();

        private void OnGUI()
        {
            DrawToolbar();

            if (this.report == null)
            {
                EditorGUILayout.HelpBox("Press \"Validate All\" to run.", MessageType.None);
                return;
            }

            this.scrollPos = EditorGUILayout.BeginScrollView(this.scrollPos);
            this.DrawBlueprintSection();
            this.DrawOrphanSection();
            EditorGUILayout.EndScrollView();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            this.validateDataTypes = GUILayout.Toggle(
                this.validateDataTypes, "Validate data types (slower)", EditorStyles.toolbarButton);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Validate All", EditorStyles.toolbarButton))
            {
                this.expanded.Clear();
                this.report = CsvBlueprintValidator.ValidateAll(this.validateDataTypes);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawBlueprintSection()
        {
            EditorGUILayout.LabelField("Blueprints", EditorStyles.boldLabel);
            for (var i = 0; i < this.report.BlueprintResults.Count; i++)
                this.DrawBlueprintRow(i, this.report.BlueprintResults[i]);
        }

        private void DrawBlueprintRow(int index, CsvBlueprintValidationResult result)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(StatusIcon(result.Status), GUILayout.Width(18));

            var label = $"{result.BlueprintType.Name}  —  {result.DataPath}  [{result.Source}]";
            if (result.Issues.Count > 0)
            {
                var isExpanded = this.expanded.Contains(index);
                var toggled    = EditorGUILayout.Foldout(isExpanded, label);
                if (toggled != isExpanded)
                {
                    if (toggled) this.expanded.Add(index);
                    else this.expanded.Remove(index);
                }
            }
            else
            {
                EditorGUILayout.LabelField(label);
            }

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Ping", EditorStyles.miniButton, GUILayout.Width(36)))
                PingBlueprint(result);

            EditorGUILayout.EndHorizontal();

            if (!this.expanded.Contains(index)) return;

            EditorGUI.indentLevel++;
            foreach (var issue in result.Issues)
            {
                var prev  = GUI.color;
                GUI.color = issue.Severity == ValidationStatus.Error ? new Color(1f, 0.4f, 0.4f) : new Color(1f, 0.85f, 0.3f);
                var text  = issue.Location != null ? $"[{issue.Location}]  {issue.Message}" : issue.Message;
                EditorGUILayout.LabelField(text, EditorStyles.wordWrappedMiniLabel);
                GUI.color = prev;
            }
            EditorGUI.indentLevel--;
        }

        private void DrawOrphanSection()
        {
            if (this.report.Orphans.Count == 0) return;

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Orphan CSV Files (no blueprint maps to these)", EditorStyles.boldLabel);
            foreach (var orphan in this.report.Orphans)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(StatusIcon(ValidationStatus.Warning), GUILayout.Width(18));
                if (GUILayout.Button(orphan.AssetPath, EditorStyles.linkLabel))
                    PingAsset(orphan.AssetPath);
                EditorGUILayout.EndHorizontal();
            }
        }

        private static string StatusIcon(ValidationStatus status) => status switch
        {
            ValidationStatus.Ok      => "✓",
            ValidationStatus.Warning => "⚠",
            ValidationStatus.Error   => "✗",
            _                        => "?",
        };

        private static void PingAsset(string assetPath)
        {
            var obj = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            if (obj != null) EditorGUIUtility.PingObject(obj);
        }

        private static void PingBlueprint(CsvBlueprintValidationResult result)
        {
            var guids = AssetDatabase.FindAssets($"{result.DataPath} t:TextAsset");
            if (guids.Length > 0) PingAsset(AssetDatabase.GUIDToAssetPath(guids[0]));
        }
    }
}
