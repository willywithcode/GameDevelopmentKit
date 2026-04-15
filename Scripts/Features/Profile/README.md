# Profile Feature

This feature provides a simple local player profile for casual and puzzle games.

## What It Stores

- `PlayerId`
- `DisplayName`
- `AvatarIndex`
- `GamesPlayed`
- `BestScore`
- `HighestUnlockedLevel`
- `CreatedAtUtc`
- `LastUpdatedAtUtc`

Data is persisted by `ProfileLocalDataService` with Easy Save.

## Main Classes

- `Services/ProfileService.cs`
  Main API for reading and updating profile data.
- `LocalData/ProfileLocalData.cs`
  Local persistence model and save helpers.
- `Signals/ProfileSignals.cs`
  Signals fired when profile is created or changed.
- `DI/ProfileVContainer.cs`
  Registers `ProfileService` in VContainer.

## Registration

The feature is already registered in:

- `Assets/GDK/Scripts/GameFoundationVContainer.cs`

No extra setup is needed if your scene uses the existing `GameLifeTimeScope`.

## Basic Usage

Inject `ProfileService`:

```csharp
using GameFoundation.Scripts.Features.Profile.Services;

public class ExampleConsumer
{
    private readonly ProfileService profileService;

    public ExampleConsumer(ProfileService profileService)
    {
        this.profileService = profileService;
    }

    public void RenamePlayer(string newName)
    {
        this.profileService.SetDisplayName(newName);
    }
}
```

Read current values:

```csharp
var playerId = profileService.PlayerId;
var name = profileService.DisplayName;
var avatarIndex = profileService.AvatarIndex;
var bestScore = profileService.BestScore;
```

Update values:

```csharp
profileService.SetDisplayName("Player1234");
profileService.SetAvatarIndex(2);
profileService.SetProfile("Player1234", 2);
profileService.RecordGamePlayed();
profileService.RecordScore(18);
profileService.UnlockLevel(3);
```

## Signals

The feature publishes through `MessagePipe`.

Available signals:

- `OnProfileCreated`
- `OnProfileChanged`

Example subscription:

```csharp
using System;
using GameFoundation.Scripts.Features.Profile.Signals;
using MessagePipe;

public class ExampleProfileListener : IDisposable
{
    private readonly ISubscriber<OnProfileChanged> profileChangedSubscriber;
    private readonly IDisposable                    subscription;

    public ExampleProfileListener(ISubscriber<OnProfileChanged> profileChangedSubscriber)
    {
        this.profileChangedSubscriber = profileChangedSubscriber;
        this.subscription = this.profileChangedSubscriber.Subscribe(this.OnProfileChanged);
    }

    private void OnProfileChanged(OnProfileChanged signal)
    {
        UnityEngine.Debug.Log($"Profile changed: {signal.DisplayName} / avatar {signal.AvatarIndex}");
    }

    public void Dispose()
    {
        this.subscription.Dispose();
    }
}
```

## Avatar Loading

Avatar sprites are loaded through `IAssetsManager`.

Current address keys:

- `ProfileAvatar_0`
- `ProfileAvatar_1`
- `ProfileAvatar_2`
- `ProfileAvatar_3`
- `ProfileAvatar_4`
- `ProfileAvatar_5`
- `ProfileAvatar_6`
- `ProfileAvatar_7`
- `ProfileAvatar_8`
- `ProfileAvatar_9`

Get the key from `ProfileService`:

```csharp
var avatarAddress = profileService.GetAvatarAddress(profileService.AvatarIndex);
var sprite = await assetsManager.LoadAssetAsync<Sprite>(avatarAddress);
```

## Current Project Integration

This project already uses the feature here:

- `Assets/PaperIO/Scripts/Services/PlayerDataService.cs`
  Bridges older game-specific API to `ProfileService`.
- `Assets/Scripts/UIs/HomeScreenView.cs`
  Loads the current avatar with `IAssetsManager`.
- `Assets/Scripts/UIs/ProfilePopupView.cs`
  Loads the current avatar with `IAssetsManager`.
- `Assets/Scripts/Services/GameplayService.cs`
  Records games played and best score.

## Notes

- Default display name is generated as `Player####`.
- Avatar index is clamped to the current supported avatar range.
- Profile data is created automatically on first use.
- `ProfilePopupView.prefab` currently shows the current avatar only.
  Avatar selection UI still needs to be populated if you want a full picker.
