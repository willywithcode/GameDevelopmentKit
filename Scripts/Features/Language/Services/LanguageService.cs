namespace GameFoundation.Scripts.Features.Language.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using GameFoundation.Scripts.Addressable;
    using GameFoundation.Scripts.Features.Language.Blueprints;
    using GameFoundation.Scripts.Features.Language.LocalDatas;
    using GameFoundation.Scripts.Features.Language.Signals;
    using IGameLogger = GameFoundation.Scripts.Features.Logger.Services.ILogger;
    using LoggerService = GameFoundation.Scripts.Features.Logger.Services.LoggerService;
    using GameFoundation.Scripts.Features.UserExperience.Services;
    using MessagePipe;
    using Newtonsoft.Json;
    using UnityEngine;
    using VContainer.Unity;

    public class LanguageService : IInitializable
    {
        private static readonly StringComparer LanguageComparer = StringComparer.OrdinalIgnoreCase;

        #region Inject

        private readonly LanguageLocalDataService          languageLocalDataService;
        private readonly IPublisher<OnLanguageChange>      languageChangePublisher;
        private readonly UserExperienceService             userExperienceService;
        private readonly LanguageBlueprint                 languageBlueprint;
        private readonly Dictionary<string, LanguageData>  languageDataMap;
        private readonly List<string>                      availableLanguages;
        private readonly string                            defaultLanguageName;
        private readonly Dictionary<string, string>        languageCodeToNameMap;
        private readonly IGameLogger                       logger;

        public LanguageService(
            LanguageLocalDataService    languageLocalDataService,
            IAssetsManager              assetsManager,
            IPublisher<OnLanguageChange> languageChangePublisher,
            UserExperienceService       userExperienceService,
            IGameLogger                 logger = null
        )
        {
            this.languageLocalDataService = languageLocalDataService;
            this.languageChangePublisher  = languageChangePublisher;
            this.userExperienceService    = userExperienceService;
            this.logger                   = logger ?? new LoggerService();
            this.languageBlueprint        = this.LoadLanguageBlueprint(assetsManager);
            this.languageDataMap          = this.LoadLanguageDatas(assetsManager, this.languageBlueprint);

            var languagesFromBlueprint = (this.languageBlueprint.languages ?? new List<LanguageFileReference>())
                .Select(entry => entry?.languageName)
                .Where(name => !string.IsNullOrEmpty(name))
                .Distinct(LanguageComparer)
                .ToList();

            var knownLanguages = new HashSet<string>(languagesFromBlueprint, LanguageComparer);
            foreach (var languageName in this.languageDataMap.Keys)
            {
                if (knownLanguages.Add(languageName))
                {
                    languagesFromBlueprint.Add(languageName);
                }
            }

            this.availableLanguages     = languagesFromBlueprint;
            this.defaultLanguageName    = this.DetermineDefaultLanguageName(this.languageBlueprint, this.languageDataMap);
            this.languageCodeToNameMap  = this.BuildLanguageCodeToNameMap(this.languageBlueprint);
        }

        #endregion

        private LanguageBlueprint LoadLanguageBlueprint(IAssetsManager assetsManager)
        {
            TextAsset jsonAsset = null;
            try
            {
                jsonAsset = assetsManager.LoadAsset<TextAsset>("LanguageBlueprint");
                if (jsonAsset == null)
                {
                    this.logger.Error("Language blueprint JSON asset with key 'LanguageBlueprint' was not found.");
                    return this.CreateFallbackBlueprint();
                }

                var blueprint = JsonConvert.DeserializeObject<LanguageBlueprint>(jsonAsset.text) ?? new LanguageBlueprint();
                this.EnsureBlueprintIntegrity(blueprint);
                return blueprint;
            }
            catch (Exception e)
            {
                this.logger.Error($"Failed to load language blueprint JSON: {e}");
                return this.CreateFallbackBlueprint();
            }
            finally
            {
                if (jsonAsset != null)
                {
                    assetsManager.Release("LanguageBlueprint");
                }
            }
        }

        private LanguageBlueprint CreateFallbackBlueprint()
        {
            var fallbackLanguage = this.languageLocalDataService?.CurrentLanguage ?? string.Empty;
            var fallback = new LanguageBlueprint
            {
                initLanguage     = fallbackLanguage,
                defaultLanguage  = fallbackLanguage,
                languages        = new List<LanguageFileReference>()
            };
            this.EnsureBlueprintIntegrity(fallback);
            return fallback;
        }

        private void EnsureBlueprintIntegrity(LanguageBlueprint blueprint)
        {
            if (blueprint == null)
            {
                return;
            }

            blueprint.languages ??= new List<LanguageFileReference>();

            var uniqueEntries = new List<LanguageFileReference>();
            var seenLanguages = new HashSet<string>(LanguageComparer);

            foreach (var entry in blueprint.languages)
            {
                if (entry == null)
                {
                    continue;
                }

                entry.languageName = entry.languageName?.Trim() ?? string.Empty;
                entry.address      = entry.address?.Trim();

                if (string.IsNullOrEmpty(entry.languageName))
                {
                    continue;
                }

                if (!seenLanguages.Add(entry.languageName))
                {
                    continue;
                }

                uniqueEntries.Add(entry);
            }

            blueprint.languages = uniqueEntries;

            if (string.IsNullOrWhiteSpace(blueprint.initLanguage) && blueprint.languages.Count > 0)
            {
                blueprint.initLanguage = blueprint.languages[0].languageName;
            }

            if (string.IsNullOrWhiteSpace(blueprint.defaultLanguage))
            {
                blueprint.defaultLanguage = blueprint.initLanguage;
            }

            if (!string.IsNullOrEmpty(blueprint.defaultLanguage) && !seenLanguages.Contains(blueprint.defaultLanguage))
            {
                blueprint.defaultLanguage = blueprint.languages.Count > 0 ? blueprint.languages[0].languageName : string.Empty;
            }
        }

        private Dictionary<string, LanguageData> LoadLanguageDatas(IAssetsManager assetsManager, LanguageBlueprint blueprint)
        {
            var languages = new Dictionary<string, LanguageData>(LanguageComparer);
            if (blueprint?.languages == null)
            {
                return languages;
            }

            foreach (var entry in blueprint.languages)
            {
                if (entry == null || string.IsNullOrEmpty(entry.languageName))
                {
                    continue;
                }

                if (string.IsNullOrEmpty(entry.address))
                {
                    this.logger.Warning($"Language '{entry.languageName}' does not have an address configured in LanguageBlueprint.");
                    languages[entry.languageName] = this.EnsureLanguageDataIntegrity(null, entry.languageName);
                    continue;
                }

                TextAsset languageAsset = null;
                try
                {
                    languageAsset = assetsManager.LoadAsset<TextAsset>(entry.address);
                    if (languageAsset == null)
                    {
                        this.logger.Error($"Failed to load language asset for '{entry.languageName}' at address '{entry.address}'.");
                        languages[entry.languageName] = this.EnsureLanguageDataIntegrity(null, entry.languageName);
                        continue;
                    }

                    var languageData = JsonConvert.DeserializeObject<LanguageData>(languageAsset.text);
                    languageData     = this.EnsureLanguageDataIntegrity(languageData, entry.languageName);
                    languages[languageData.languageName] = languageData;
                }
                catch (Exception e)
                {
                    this.logger.Error($"Failed to parse language JSON for '{entry.languageName}' at address '{entry.address}': {e}");
                    languages[entry.languageName] = this.EnsureLanguageDataIntegrity(null, entry.languageName);
                }
                finally
                {
                    if (languageAsset != null)
                    {
                        assetsManager.Release(entry.address);
                    }
                }
            }

            return languages;
        }

        private LanguageData EnsureLanguageDataIntegrity(LanguageData data, string fallbackName)
        {
            data ??= new LanguageData();

            if (string.IsNullOrEmpty(data.languageName))
            {
                data.languageName = fallbackName ?? string.Empty;
            }

            data.translations ??= new Dictionary<string, string>();
            return data;
        }

        private string DetermineDefaultLanguageName(LanguageBlueprint blueprint, Dictionary<string, LanguageData> languages)
        {
            if (blueprint == null || languages == null || languages.Count == 0)
            {
                return string.Empty;
            }

            if (!string.IsNullOrEmpty(blueprint.defaultLanguage) && languages.ContainsKey(blueprint.defaultLanguage))
            {
                return blueprint.defaultLanguage;
            }

            if (!string.IsNullOrEmpty(blueprint.initLanguage) && languages.ContainsKey(blueprint.initLanguage))
            {
                return blueprint.initLanguage;
            }

            if (blueprint.languages != null)
            {
                foreach (var entry in blueprint.languages)
                {
                    if (entry == null || string.IsNullOrEmpty(entry.languageName))
                    {
                        continue;
                    }

                    if (languages.ContainsKey(entry.languageName))
                    {
                        return entry.languageName;
                    }
                }
            }

            return languages.Keys.FirstOrDefault() ?? string.Empty;
        }

        public void Initialize()
        {
            if (this.userExperienceService.GetSessionCount() <= 0)
            {
                string desiredLanguage;

                if (this.languageBlueprint.autoSelectLanguage)
                {
                    desiredLanguage = this.DetectSystemLanguage();
                }
                else
                {
                    desiredLanguage = this.languageBlueprint.initLanguage;
                }

                if (string.IsNullOrEmpty(desiredLanguage) || !this.languageDataMap.ContainsKey(desiredLanguage))
                {
                    desiredLanguage = this.defaultLanguageName;
                }

                if (string.IsNullOrEmpty(desiredLanguage) && this.availableLanguages.Count > 0)
                {
                    desiredLanguage = this.availableLanguages[0];
                }

                if (!string.IsNullOrEmpty(desiredLanguage))
                {
                    this.languageLocalDataService.CurrentLanguage = desiredLanguage;
                }
            }
        }

        public string GetCurrentLanguage()
        {
            return this.languageLocalDataService.CurrentLanguage;
        }

        public void SetLanguage(string language)
        {
            this.languageLocalDataService.CurrentLanguage = language;
            this.languageChangePublisher.Publish(new OnLanguageChange(language));
        }

        public bool TryGetTranslation(string key, out string translation)
        {
            if (this.TryGetLanguageData(this.GetCurrentLanguage(), out var languageData) &&
                languageData.translations.TryGetValue(key, out translation))
            {
                return true;
            }

            if (this.TryGetLanguageData(this.defaultLanguageName, out var defaultLanguageData) &&
                defaultLanguageData.translations.TryGetValue(key, out translation))
            {
                return true;
            }

            translation = null;
            return false;
        }

        public List<string> GetAvailableLanguages()
        {
            return new List<string>(this.availableLanguages);
        }

        private bool TryGetLanguageData(string language, out LanguageData languageData)
        {
            if (string.IsNullOrEmpty(language))
            {
                languageData = null;
                return false;
            }

            return this.languageDataMap.TryGetValue(language, out languageData);
        }

        private Dictionary<string, string> BuildLanguageCodeToNameMap(LanguageBlueprint blueprint)
        {
            var codeToNameMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (blueprint?.languages == null)
            {
                return codeToNameMap;
            }

            foreach (var entry in blueprint.languages)
            {
                if (entry == null || string.IsNullOrEmpty(entry.code) || string.IsNullOrEmpty(entry.languageName))
                {
                    continue;
                }

                if (!codeToNameMap.ContainsKey(entry.code))
                {
                    codeToNameMap[entry.code] = entry.languageName;
                }
            }

            return codeToNameMap;
        }

        private string DetectSystemLanguage()
        {
            try
            {
                var twoLetterCode = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
                if (!string.IsNullOrEmpty(twoLetterCode) &&
                    this.languageCodeToNameMap.TryGetValue(twoLetterCode, out var languageName) &&
                    this.languageDataMap.ContainsKey(languageName))
                {
                    this.logger.Info($"[LanguageService] Auto-detected system language: {twoLetterCode} -> {languageName}");
                    return languageName;
                }

                this.logger.Info($"[LanguageService] System language '{twoLetterCode}' not available, falling back to default: {this.defaultLanguageName}");
            }
            catch (Exception e)
            {
                this.logger.Warning($"[LanguageService] Failed to detect system language: {e.Message}");
            }

            return this.defaultLanguageName;
        }
    }
}
