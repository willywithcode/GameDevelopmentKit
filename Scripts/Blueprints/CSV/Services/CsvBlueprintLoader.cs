namespace GameFoundation.Scripts.Blueprints.CSV.Services
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Addressable;
    using GameFoundation.Scripts.Blueprints.CSV.Attributes;
    using GameFoundation.Scripts.Blueprints.CSV.Interfaces;
    using GameFoundation.Scripts.Utils;
    using UnityEngine;
    using VContainer;

    public class CsvBlueprintLoader : ICsvBlueprintLoader
    {
        private const string DefaultResourceFolder = "Blueprints/CSV";

        private readonly IObjectResolver objectResolver;
        private readonly IAssetsManager  assetsManager;
        private readonly HashSet<Type>   loadedTypes = new();

        public CsvBlueprintLoader(IObjectResolver objectResolver, IAssetsManager assetsManager)
        {
            this.objectResolver = objectResolver;
            this.assetsManager  = assetsManager;
        }

        public bool IsLoaded { get; private set; }

        public async UniTask LoadAllBlueprints()
        {
            var blueprintTypes = ReflectionExtensions.GetAllImplementationsOf(typeof(ICsvBlueprintReader));
            foreach (var blueprintType in blueprintTypes)
            {
                await this.LoadBlueprint(blueprintType);
            }

            this.IsLoaded = true;
        }

        public UniTask LoadBlueprint<TBlueprint>() where TBlueprint : class, ICsvBlueprintReader
        {
            return this.LoadBlueprint(typeof(TBlueprint));
        }

        private async UniTask LoadBlueprint(Type blueprintType)
        {
            if (this.loadedTypes.Contains(blueprintType))
            {
                return;
            }

            var attribute = blueprintType.GetCustomAttribute<CsvBlueprintAttribute>();
            if (attribute == null)
            {
                throw new InvalidOperationException($"CSV blueprint reader '{blueprintType.FullName}' must have CsvBlueprintAttribute.");
            }

            var blueprintReader = this.objectResolver.Resolve(blueprintType) as ICsvBlueprintReader;
            if (blueprintReader == null)
            {
                throw new InvalidOperationException($"Can not resolve CSV blueprint reader '{blueprintType.FullName}'.");
            }

            var rawCsv = this.LoadRawCsv(attribute);
            await blueprintReader.DeserializeFromCsv(rawCsv);
            this.loadedTypes.Add(blueprintType);
        }

        private string LoadRawCsv(CsvBlueprintAttribute attribute)
        {
            if (attribute.Source == CsvBlueprintSource.Resources)
            {
                var resourcePath = this.NormalizeResourcePath(attribute.DataPath);
                var textAsset    = Resources.Load<TextAsset>(resourcePath);
                if (textAsset == null)
                {
                    throw new InvalidOperationException($"Can not load CSV blueprint from Resources path '{resourcePath}'.");
                }

                return textAsset.text;
            }

            var addressableAsset = this.assetsManager.LoadAsset<TextAsset>(attribute.DataPath);
            if (addressableAsset == null)
            {
                throw new InvalidOperationException($"Can not load CSV blueprint from Addressables key '{attribute.DataPath}'.");
            }

            try
            {
                return addressableAsset.text;
            }
            finally
            {
                this.assetsManager.Release(attribute.DataPath);
            }
        }

        private string NormalizeResourcePath(string dataPath)
        {
            var normalizedPath = dataPath
                .Trim()
                .Replace('\\', '/')
                .TrimStart('/');
            if (normalizedPath.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                normalizedPath = normalizedPath.Substring(0, normalizedPath.Length - 4);
            }

            if (normalizedPath.StartsWith(DefaultResourceFolder, StringComparison.OrdinalIgnoreCase))
            {
                return normalizedPath;
            }

            return $"{DefaultResourceFolder}/{normalizedPath}";
        }
    }
}
