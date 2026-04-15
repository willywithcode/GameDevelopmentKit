# LeaderBoard Feature

This feature provides a leaderboard with a provider-based architecture. The default `LocalLeaderBoardProvider` generates fake bot players whose scores grow while the app is open (online) and closed (offline). Swap the provider to connect a real backend without changing any service or UI code.

## Architecture

```
ILeaderBoardProvider          ← interface (FetchEntries, SubmitScore, Dispose)
├── LocalLeaderBoardProvider  ← fake bots, offline/online ticks, local persistence
└── (your remote provider)    ← Firebase, PlayFab, custom server, etc.

LeaderBoardService            ← facade consumed by UI and game code
```

`LeaderBoardService` never touches persistence or networking directly — it delegates everything to `ILeaderBoardProvider`.

## What It Stores

- `List<LeaderBoardEntry>` (each entry: `PlayerId`, `DisplayName`, `AvatarIndex`, `Score`, `IsPlayer`)
- `LastUpdatedAtUtc`

Data is persisted by `LeaderBoardLocalDataService` with Anti-Cheat Toolkit encrypted storage.

## Main Classes

- `Services/ILeaderBoardProvider.cs`
  Provider interface. Implement this to add a real backend.
- `Services/LocalLeaderBoardProvider.cs`
  Default local provider with fake bots, online ticking, and offline catch-up.
- `Services/LeaderBoardService.cs`
  Facade API consumed by UI and game code. Delegates to `ILeaderBoardProvider`.
- `LocalData/LeaderBoardLocalData.cs`
  Local persistence model, entry data class, and save helpers.
- `Blueprints/LeaderBoardBlueprint.cs`
  ScriptableObject with all configurable values (bot count, score ranges, tick intervals).
  Also contains `LeaderBoardBlueprintService` for auto-loading via Addressables.
- `Signals/LeaderBoardSignals.cs`
  Signal fired when the leaderboard is updated.
- `DI/LeaderBoardVContainer.cs`
  Registers provider and service in VContainer. Change one line to swap providers.

## Registration

The feature is already registered in:

- `Assets/GDK/Scripts/GameFoundationVContainer.cs`

No extra setup is needed if your scene uses the existing `GameLifeTimeScope`.

## Blueprint Setup

1. In Unity: Right-click in Project > **Create > GameFoundation > Features > LeaderBoard > Blueprint**
2. Save as `LeaderBoardBlueprint` in `Assets/Blueprints/`
3. Mark as **Addressable** with key `"LeaderBoardBlueprint"`

### Blueprint Fields

| Field | Default | Description |
|-------|---------|-------------|
| `BotCount` | 19 | Number of fake bots on the leaderboard |
| `InitialScoreMin` | 50 | Minimum score for a newly generated bot |
| `InitialScoreMax` | 5000 | Maximum score for a newly generated bot |
| `OnlineIntervalSeconds` | 60 | How often bots gain score while app is open (seconds) |
| `OnlineScoreGainMin` | 5 | Min score a bot gains per online tick |
| `OnlineScoreGainMax` | 30 | Max score a bot gains per online tick |
| `OfflineIntervalSeconds` | 60 | How often bots gain score while app is closed (seconds) |
| `OfflineScoreGainMin` | 3 | Min score a bot gains per offline tick |
| `OfflineScoreGainMax` | 20 | Max score a bot gains per offline tick |
| `MaxOfflineHours` | 24 | Maximum offline time counted (prevents extreme gains) |
| `BotNames` | 20 names | Array of fake bot display names |

## Basic Usage

Inject `LeaderBoardService`:

```csharp
using GameFoundation.Scripts.Features.LeaderBoard.Services;

public class ExampleConsumer
{
    private readonly LeaderBoardService leaderBoardService;

    public ExampleConsumer(LeaderBoardService leaderBoardService)
    {
        this.leaderBoardService = leaderBoardService;
    }

    public async void OnGameEnd(int score)
    {
        await this.leaderBoardService.SubmitScore(score);
    }
}
```

Read current values:

```csharp
var entries = leaderBoardService.Entries;        // sorted by score descending
var rank    = leaderBoardService.GetPlayerRank(); // 1-based player rank
```

Force refresh from provider:

```csharp
await leaderBoardService.RefreshEntries();
```

Cleanup on app quit:

```csharp
leaderBoardService.Dispose();
```

## Signals

The feature publishes through `MessagePipe`.

Available signals:

- `OnLeaderBoardUpdated`

Example subscription:

```csharp
using System;
using GameFoundation.Scripts.Features.LeaderBoard.Signals;
using MessagePipe;

public class ExampleLeaderBoardListener : IDisposable
{
    private readonly ISubscriber<OnLeaderBoardUpdated> leaderBoardUpdatedSubscriber;
    private readonly IDisposable                        subscription;

    public ExampleLeaderBoardListener(ISubscriber<OnLeaderBoardUpdated> leaderBoardUpdatedSubscriber)
    {
        this.leaderBoardUpdatedSubscriber = leaderBoardUpdatedSubscriber;
        this.subscription = this.leaderBoardUpdatedSubscriber.Subscribe(this.OnLeaderBoardUpdated);
    }

    private void OnLeaderBoardUpdated(OnLeaderBoardUpdated signal)
    {
        UnityEngine.Debug.Log($"Leaderboard updated: {signal.Entries.Count} entries");
    }

    public void Dispose()
    {
        this.subscription.Dispose();
    }
}
```

## Implementing a Real Backend

1. Create a new provider:

```csharp
public class FirebaseLeaderBoardProvider : ILeaderBoardProvider
{
    public async UniTask<List<LeaderBoardEntry>> FetchEntries()
    {
        // Fetch from Firebase Realtime Database / Firestore
    }

    public async UniTask SubmitScore(string playerId, string displayName, int avatarIndex, int score)
    {
        // Write to Firebase
    }

    public void Dispose() { }
}
```

2. Swap one line in `LeaderBoardVContainer.cs`:

```csharp
// Before (fake bots):
builder.Register<LocalLeaderBoardProvider>(Lifetime.Singleton).As<ILeaderBoardProvider>();

// After (real backend):
builder.Register<FirebaseLeaderBoardProvider>(Lifetime.Singleton).As<ILeaderBoardProvider>();
```

No changes needed in `LeaderBoardService`, UI, or any consumer code.

## Bot Score Progression (LocalLeaderBoardProvider only)

| Mode | When | How |
|------|------|-----|
| **Online** | While app is running | Auto-ticks every `OnlineIntervalSeconds`. Each bot gains `OnlineScoreGainMin`-`OnlineScoreGainMax` per tick. |
| **Offline** | On next app startup | Calculates elapsed time since `LastUpdatedAtUtc`. Each bot gains `OfflineScoreGainMin`-`OfflineScoreGainMax` per `OfflineIntervalSeconds` elapsed. Capped at `MaxOfflineHours`. |

On first launch, the leaderboard is populated with the player entry and `BotCount` bots with random scores in `InitialScoreMin`-`InitialScoreMax`.

## Current Project Integration

This project uses the feature here:

- `Assets/Scripts/UIs/LeaderboardScreenView.cs`
  Displays the leaderboard with entry rows. Listens for `OnLeaderBoardUpdated` to refresh UI.
- `Assets/Scripts/UIs/Navbar/NavbarView.cs`
  Shows/hides the leaderboard screen from the navigation bar.

## UI Prefab Setup

The `LeaderboardScreenView` prefab requires:

- `EntryContainer` — a `Transform` with `VerticalLayoutGroup` (scroll content parent)
- `EntryPrefab` — a `GameObject` row template with:
  - 3 `TMP_Text` children in order: **Rank**, **Name**, **Score**
  - Optional child named `"Avatar"` with `Image` component
  - `Image` on root for background tinting (player row is highlighted blue)

## Notes

- Leaderboard is populated automatically on first use.
- Bot names are picked randomly without duplicates from the blueprint array.
- Player entry is always present and highlighted in the UI.
- Scores are sorted descending (highest first) after every update.
- All local data is encrypted via Anti-Cheat Toolkit.
- `SubmitScore` and `RefreshEntries` are async — works seamlessly for both local and remote providers.
