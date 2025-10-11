# Localization Tutorial

This tutorial covers the JSON-based localization workflow used by the game and walks through setup, authoring language files, and validating changes.

## 1. System Overview
- **Service**: `LanguageService` (registered in `LanguageVContainer`) loads data on startup and exposes `SetLanguage`, `GetAvailableLanguages`, and `TryGetTranslation`.
- **Data source**: `Assets/Data/LanguageBlueprint.json`, referenced through Addressables with key `LanguageBlueprint`.
- **UI bindings**:
  - `Language_TMP_Text` listens to language-change signals and updates a `TMP_Text` automatically.
  - `ChangeLanguageButton` calls `LanguageService.SetLanguage(languageKey)` when pressed.
  - Other scripts can query translations with `languageService.TryGetTranslation("key", out value)`.

## 2. Project Setup Checklist
1. Confirm `Assets/Data/LanguageBlueprint.json` sits in an Addressables group and keeps the address `LanguageBlueprint` (configured through its `.meta` GUID).
2. After editing localization JSON, rebuild Addressables via `Window > Asset Management > Addressables > Build > New Build`.
3. Ensure `Newtonsoft.Json.dll` remains enabled so JSON deserialization works in all build targets.

## 3. JSON File Structure
Localization data is split into a single blueprint file and one language file per locale.

### 3.1 `LanguageBlueprint.json`
Located at `Assets/Data/LanguageBlueprint.json`, this file declares which languages exist and where their JSON lives (Addressables addresses).

```json
{
  "initLanguage": "english",
  "defaultLanguage": "english",
  "languages": [
    {
      "languageName": "english",
      "address": "Localization/english"
    },
    {
      "languageName": "vietnamese",
      "address": "Localization/vietnamese"
    }
  ]
}
```

- `initLanguage`: language applied for new players (if unset, the first listed language is used).
- `defaultLanguage`: fallback language consulted when the active language is missing a key.
- `languages`: array of entries mapping a `languageName` to its Addressables `address`. Names must match what buttons/scripts use (e.g., `english`, `vietnamese`).

### 3.2 Individual language files
Each language has its own JSON under `Assets/Data/Localization/`. Example: `english.json`.

```json
{
  "languageName": "english",
  "translations": {
    "ui.play": "Play",
    "ui.quit": "Quit",
    "msg.welcome": "Welcome!"
  }
}
```

- `languageName`: should align with the blueprint entry.
- `translations`: key/value pairs used by UI components and gameplay scripts.

## 4. Adding or Updating Languages
1. Create a new JSON file in `Assets/Data/Localization/` (e.g., `spanish.json`) following the structure above.
2. Add an entry to `LanguageBlueprint.json` pointing to the new file’s Addressables address (e.g., `"address": "Localization/spanish"`).
3. Maintain consistent translation keys across every language so UI text does not fall back unexpectedly.
4. Save all JSON files as UTF-8 without BOM. Escape special characters (`\uXXXX`) if your editor cannot store them directly.

## 5. Hooking Up UI
- **TMP text**: Add `Language_TMP_Text`, set the `Key` field (`ui.play`, `msg.welcome`, etc.). The component updates itself after language changes.
- **Buttons**: Add `ChangeLanguageButton`, set `languageKey` (for example `english` or `vietnamese`). Clicking it calls `LanguageService.SetLanguage(languageKey)`.
- **Manual lookup example**:
  ```csharp
  if (languageService.TryGetTranslation("ui.play", out var label))
  {
      playButtonLabel.text = label;
  }
  ```

## 6. Testing Changes
1. Enter Play Mode and watch the Console for JSON parse errors.
2. Trigger a language switch via a `ChangeLanguageButton` or by calling `LanguageService.SetLanguage("vietnamese")` from a debug script.
3. Confirm all UI text updates instantly and that the selection persists on restart (stored in `LanguageLocalDataService`).
4. Optionally log available languages: `Debug.Log(string.Join(", ", languageService.GetAvailableLanguages()));`.

## 7. Troubleshooting
- Rebuild Addressables after every JSON edit to avoid shipping stale data.
- Missing strings usually mean a key is absent in the active language or in the default language file referenced by the blueprint.
- If parsing fails, check the Console for exceptions; look for malformed JSON, incorrect escaping, or invalid Unicode sequences.
- Use source control to review diffs; JSON whitespace changes are easy to spot and revert if mistakes slip in.

With the JSON workflow in place you can expand localization by editing the blueprint plus per-language files, while UI bindings and runtime behaviour remain unchanged from the previous system.
