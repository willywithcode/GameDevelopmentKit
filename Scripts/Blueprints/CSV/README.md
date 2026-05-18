# CSV Blueprint Guide

This document explains how to use the **CSV Blueprint** system integrated in this project.

## 1. Overview

The CSV Blueprint system lets you:

- Define game data in `.csv` files
- Parse data into C# classes using reflection and type conversion
- Auto-load all registered blueprints during `LoadingEntryPoint`
- Inject blueprint readers directly through VContainer

There are two reader types:

1. **Row-based**: one CSV row = one record (`CsvBlueprintReaderByRow<TKey, TRecord>`)
2. **Column-based**: one CSV row = one key/value pair (`CsvBlueprintReaderByCol`)

---

## 2. Code structure

Location: `Assets\GDK\Scripts\Blueprints\CSV`

**Runtime**
- `Attributes\CsvBlueprintAttribute.cs`
- `Attributes\CsvHeaderKeyAttribute.cs`
- `Attributes\GoogleSheetSourceAttribute.cs`
- `Readers\CsvBlueprintReaderByRow.cs`
- `Readers\CsvBlueprintReaderByCol.cs`
- `Services\CsvBlueprintLoader.cs`
- `DI\CsvBlueprintVContainer.cs`
- `Interfaces\ICsvBlueprintReader.cs`
- `Interfaces\ICsvBlueprintLoader.cs`

**Editor** (`Editor\` — asmdef: `Game.GDK.CSV.Editor`)
- `Editor\Validator\CsvBlueprintValidator.cs`
- `Editor\Validator\CsvBlueprintValidatorWindow.cs`
- `Editor\GoogleSheets\GoogleSheetsSyncer.cs`
- `Editor\GoogleSheets\GoogleSheetsSyncWindow.cs`

Already wired in:

- `Assets\GDK\Scripts\GameFoundationVContainer.cs` via `builder.RegisterCsvBlueprint();`
- `Assets\Scripts\Game\Loading\LoadingEntryPoint.cs` via `LoadAllBlueprints()`

---

## 3. CSV folder convention

### Resource mode (default)

`CsvBlueprintLoader` loads from:

`Assets\Resources\Blueprints\CSV\`

Example file:

`Assets\Resources\Blueprints\CSV\Monster.csv`

Attribute usage:

```csharp
[CsvBlueprint("Monster")]
```

or:

```csharp
[CsvBlueprint("Blueprints/CSV/Monster")]
```

### Addressable mode

Use a `TextAsset` Addressables key:

```csharp
[CsvBlueprint("MonsterCsvKey", CsvBlueprintSource.Addressable)]
```

---

## 4. Create a row-based blueprint (most common)

### 4.1 Create a record type

```csharp
using GameFoundation.Scripts.Blueprints.CSV.Attributes;

[CsvHeaderKey("Id")]
public class MonsterRecord
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int Hp { get; set; }
    public float MoveSpeed { get; set; }
}
```

`CsvHeaderKey("Id")` defines the key column in your CSV.

### 4.2 Create a reader

```csharp
using GameFoundation.Scripts.Blueprints.CSV.Attributes;
using GameFoundation.Scripts.Blueprints.CSV.Readers;

[CsvBlueprint("Monster")]
public class MonsterBlueprint : CsvBlueprintReaderByRow<string, MonsterRecord>
{
}
```

### 4.3 Create the CSV file

`Assets\Resources\Blueprints\CSV\Monster.csv`

```csv
Id,Name,Hp,MoveSpeed
M001,Zombie,100,2.5
M002,Skeleton,80,3.2
```

### 4.4 Use it in a service

```csharp
public class MonsterService
{
    private readonly MonsterBlueprint monsterBlueprint;

    public MonsterService(MonsterBlueprint monsterBlueprint)
    {
        this.monsterBlueprint = monsterBlueprint;
    }

    public MonsterRecord GetMonster(string id) => this.monsterBlueprint.GetDataById(id);
}
```

---

## 5. Create a column-based blueprint

Column-based is useful for global key/value-style configuration:

```csharp
using System.Collections.Generic;
using GameFoundation.Scripts.Blueprints.CSV.Attributes;
using GameFoundation.Scripts.Blueprints.CSV.Readers;
using UnityEngine;

[CsvBlueprint("GameConfig")]
public class GameConfigBlueprint : CsvBlueprintReaderByCol
{
    public int MaxLives { get; set; }
    public float ReviveCooldown { get; set; }
    public List<int> RewardMilestones { get; set; }
    public Vector3 SpawnPoint { get; set; }
}
```

CSV:

```csv
Key,Value
MaxLives,5
ReviveCooldown,30
RewardMilestones,"10,25,50"
SpawnPoint,0|1.5|0
```

---

## 6. Supported type conversion

`CsvTypeConverter` currently supports:

- Primitive types: `string`, `bool`, `int`, `float`, etc.
- `enum`
- `Nullable<T>`
- `Vector2`, `Vector3` (`x|y` or `x|y|z`)
- Collections:
  - `List<T>`, `IList<T>`, `IReadOnlyList<T>`, `ICollection<T>`, `IEnumerable<T>`
  - `T[]`
  - `Dictionary<TKey, TValue>`, `IDictionary<TKey, TValue>`

Collection formats:

- List/Array: `1,2,3` (or `1|2|3`)
- Dictionary: `hp:100,atk:20`
- Bool: supports `true/false` and `1/0`

---

## 7. Loading data

### 7.1 Auto-load all blueprints (already used)

In `LoadingEntryPoint`:

```csharp
await this._csvBlueprintLoader.LoadAllBlueprints();
```

Keep this flow so data is ready before entering the main scene.

### 7.2 Load a single blueprint

```csharp
await csvBlueprintLoader.LoadBlueprint<MonsterBlueprint>();
```

---

## 8. Editor tools

### 8.1 CSV Blueprint Validator

**Menu:** `Tools > QuantumGame > Validate CSV Blueprints`

Scans all `ICsvBlueprintReader` implementations and reports:

| Check | Severity |
|-------|----------|
| CSV file not found at declared path | Error |
| Key column missing from header | Error |
| Duplicate keys | Error |
| TRecord field has no matching column | Warning |
| CSV file exists but no blueprint maps to it (orphan) | Warning |

Optional checkbox **"Validate data types"** — tries to parse every cell using `CsvTypeConverter`. Slower but catches format mismatches (e.g. a string in an `int` column) before runtime.

Click any result row → **Ping** highlights the CSV file in the Project window.

---

### 8.2 Google Sheets Sync

**Menu:** `Tools > QuantumGame > Sync CSV from Google Sheets`

Fetches CSV data directly from a Google Sheet and overwrites the local file. Requires the sheet to be **published** (`File > Share > Publish to web`).

#### Setup — add `[GoogleSheetSource]` to the blueprint

```csharp
[CsvBlueprint("WoolBlockColors", CsvBlueprintSource.Addressable)]
[GoogleSheetSource("1BxiMVs0XRA5nFMdKvBdBZjgmUUqptlbs74OgVE2upms", sheetGid: 0)]
public class WoolBlockColorBlueprint : CsvBlueprintReaderByRow<WoolBlockColor, WoolBlockColorRecord>
```

- `spreadsheetId` — the long ID in the Google Sheets URL
- `sheetGid` — the tab ID (`0` = first sheet); find it in the URL after `#gid=`

The tool builds the export URL automatically:
```
https://docs.google.com/spreadsheets/d/{spreadsheetId}/export?format=csv&gid={sheetGid}
```

#### Save path

| `CsvBlueprintSource` | Save location |
|----------------------|---------------|
| `Addressable` | Resolved via `AddressableAssetSettings` — overwrites the existing registered asset |
| `Resources` | `Assets/Resources/Blueprints/CSV/{DataPath}.csv` |

#### After sync

- `AssetDatabase.ImportAsset` is called automatically
- Validator runs automatically — a dialog appears if any errors are found
- Last-synced timestamps are stored in `Library/GoogleSheetsSyncState.json` (not committed to git)

#### Extending auth

The fetch layer uses `ISheetFetcher`. To add OAuth or Service Account later:

```csharp
// 1. Add to the enum
public enum SheetAuthMode { Public, OAuth }

// 2. Implement the interface
public class OAuthSheetFetcher : ISheetFetcher { ... }

// 3. Add a case to the factory
SheetAuthMode.OAuth => new OAuthSheetFetcher()
```

No existing code changes needed.

---

## 9. Important rules when authoring CSV

1. Header names must match `TRecord` field/property names (case-insensitive).
2. Row-based blueprints must include the key column defined by `CsvHeaderKey`.
3. Duplicate keys throw an exception.
4. Reader classes must have `[CsvBlueprint(...)]`.
5. Resource mode only loads files under `Resources`.

---

## 10. Quick troubleshooting

- **`must have CsvBlueprintAttribute`**  
  The reader class is missing `[CsvBlueprint(...)]`.

- **`does not contain key column`**  
  CSV header does not include the required key column from `CsvHeaderKey`.

- **`Duplicate key`**  
  Your CSV has duplicate key values.

- **`Can not load CSV blueprint from Resources path`**  
  Wrong path/file name, or file is not under `Assets\Resources\Blueprints\CSV`.

- **`FormatException` while parsing**  
  CSV value format does not match the target type (for example `Vector3` must be `x|y|z`).

---

## 11. Quick template

```csharp
using GameFoundation.Scripts.Blueprints.CSV.Attributes;
using GameFoundation.Scripts.Blueprints.CSV.Readers;

[CsvBlueprint("MyData")]
public class MyDataBlueprint : CsvBlueprintReaderByRow<int, MyDataRecord>
{
}

[CsvHeaderKey("Id")]
public class MyDataRecord
{
    public int Id { get; set; }
    public string Name { get; set; }
    public float Value { get; set; }
}
```

CSV:

```csv
Id,Name,Value
1,TestA,10.5
2,TestB,20
```
