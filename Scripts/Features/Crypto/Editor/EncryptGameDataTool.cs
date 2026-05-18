namespace GameFoundation.Scripts.Features.Crypto.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    public class EncryptGameDataTool : EditorWindow
    {
        private const string PrefKeySource = "QuantumGame.EncryptTool.SourceFolder";
        private const string PrefKeyOutput = "QuantumGame.EncryptTool.OutputFolder";

        private string sourceFolder;
        private string outputFolder;
        private Vector2 scrollPos;
        private readonly List<string> log = new();

        [MenuItem("Tools/QuantumGame/Encrypt Game Data")]
        public static void ShowWindow()
        {
            GetWindow<EncryptGameDataTool>("Encrypt Game Data").minSize = new Vector2(480, 320);
        }

        private void OnEnable()
        {
            this.sourceFolder = EditorPrefs.GetString(PrefKeySource, "Assets/SourceData~");
            this.outputFolder = EditorPrefs.GetString(PrefKeyOutput, "Assets/GameAssets");
        }

        private void OnGUI()
        {
            GUILayout.Space(8);
            GUILayout.Label("Game Data Encryption", EditorStyles.boldLabel);
            GUILayout.Space(4);

            EditorGUI.BeginChangeCheck();
            this.sourceFolder = EditorGUILayout.TextField("Source Folder", this.sourceFolder);
            this.outputFolder = EditorGUILayout.TextField("Output Folder", this.outputFolder);
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetString(PrefKeySource, this.sourceFolder);
                EditorPrefs.SetString(PrefKeyOutput, this.outputFolder);
            }

            GUILayout.Space(4);
            EditorGUILayout.HelpBox(
                "Source: plain .json / .csv files (not imported by Unity)\n" +
                "Output: encrypted .bytes files (added to Addressables)",
                MessageType.Info);

            GUILayout.Space(8);
            if (GUILayout.Button("Encrypt All", GUILayout.Height(32)))
            {
                this.RunEncryption();
            }

            GUILayout.Space(8);
            GUILayout.Label("Log", EditorStyles.boldLabel);
            this.scrollPos = EditorGUILayout.BeginScrollView(this.scrollPos, GUILayout.ExpandHeight(true));
            foreach (var line in this.log)
            {
                GUILayout.Label(line, EditorStyles.miniLabel);
            }
            EditorGUILayout.EndScrollView();
        }

        private void RunEncryption()
        {
            this.log.Clear();

            var projectRoot   = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            var sourceAbsolute = Path.GetFullPath(Path.Combine(projectRoot, this.sourceFolder));
            var outputAbsolute = Path.GetFullPath(Path.Combine(projectRoot, this.outputFolder));

            if (!Directory.Exists(sourceAbsolute))
            {
                this.log.Add($"[ERROR] Source folder not found: {sourceAbsolute}");
                this.Repaint();
                return;
            }

            var extensions = new[] { "*.json", "*.csv" };
            var files      = new List<string>();
            foreach (var ext in extensions)
            {
                files.AddRange(Directory.GetFiles(sourceAbsolute, ext, SearchOption.AllDirectories));
            }

            if (files.Count == 0)
            {
                this.log.Add("[WARN] No .json or .csv files found in source folder.");
                this.Repaint();
                return;
            }

            var successCount = 0;
            foreach (var filePath in files)
            {
                try
                {
                    var plaintext    = File.ReadAllText(filePath);
                    var encrypted    = CryptoUtils.Encrypt(plaintext);
                    var relativePath = Path.GetRelativePath(sourceAbsolute, filePath);
                    var outputPath   = Path.Combine(outputAbsolute, Path.ChangeExtension(relativePath, ".bytes"));

                    Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
                    File.WriteAllBytes(outputPath, encrypted);

                    this.log.Add($"[OK] {relativePath}  →  {Path.GetRelativePath(projectRoot, outputPath)}");
                    successCount++;
                }
                catch (Exception ex)
                {
                    this.log.Add($"[ERROR] {Path.GetFileName(filePath)}: {ex.Message}");
                }
            }

            this.log.Add(string.Empty);
            this.log.Add($"Done — {successCount}/{files.Count} files encrypted.");

            AssetDatabase.Refresh();
            this.Repaint();
        }
    }
}
