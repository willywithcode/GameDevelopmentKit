# GameFoundation MVP System

## Overview
Model-View-Presenter architecture for Unity UI. Uses **VContainer** for DI, **Addressables** for view prefab loading, **UniTask** for async lifecycle, and **DOTween** for animations. Presenters are auto-registered via reflection.

## Features
- Auto-registration of presenters via `[PresenterAttribute]`
- Async view loading via Addressables (`LoadAssetAsync`)
- Bounded object pooling per view type (configurable via `[ViewAttribute]`)
- Four presenter flavors: Screen, Popup, Overlay, Splash
- Async lifecycle with `CancellationToken` for abort-on-close
- Optional animations — every `Open`/`Close` accepts a `bool animate` flag
- Signal publishing on open/hide via MessagePipe (`OpenPresenterSignal`, `HidePresenterSignal`)

## Presenter Types

| Type | View | Presenter | Use For |
|------|------|-----------|---------|
| Screen | `ScreenView` | `ScreenPresenter<TView>` / `<TView,TModel>` | Full-screen views |
| Popup | `PopupView` | `PopupPresenter<TView>` / `<TView,TModel>` | Modal dialogs (scale + fade + dimmer) |
| Overlay | `OverlayView` | `OverlayPresenter<TView>` / `<TView,TModel>` | Non-modal HUD |
| Splash | `SplashView` | `SplashPresenter<TView>` / `<TView,TModel>` | Loading screens with async work |

The `<TView, TModel>` variants inherit from their `<TView>` sibling and add `SetModel(TModel)`.

## Lifecycle

```
OpenAsync(animate)
  └─ create view if null → Ready() (once)
  └─ RebindButtonClickEffect() → Bind()
  └─ publish OpenPresenterSignal
  └─ await OnBeforeShow()
  └─ await view.Show(animate)
  └─ await OnAfterShow()

CloseAsync(animate)
  └─ publish HidePresenterSignal
  └─ await OnBeforeHide()
  └─ await view.Hide(animate)
  └─ await OnAfterHide()

Destroy()
  └─ cancel lifetime → return view to pool
```

Each `OpenAsync`/`CloseAsync` creates a new `CancellationTokenSource`. Access it via `protected CancellationToken LifetimeToken` inside `OnBeforeShow`/`OnAfterShow`/etc. to abort pending awaits when the user closes early. `OperationCanceledException` is swallowed by the base.

## Step-by-Step: Create a Screen

### 1. Define the View

```csharp
using GameFoundation.Scripts.Patterns.MVP.Attribute;
using GameFoundation.Scripts.Patterns.MVP.Implementation;
using UnityEngine;
using UnityEngine.UI;

[ViewAttribute("UI/HomeScreen", maxPoolSize: 2)]
public class HomeScreenView : ScreenView
{
    [SerializeField] private Button playButton;
    [SerializeField] private Text   coinText;

    public Button PlayButton => this.playButton;
    public Text   CoinText   => this.coinText;
}
```

`maxPoolSize` is optional (default `5`). When the pool is full, extra returned views are destroyed and the prefab handle is released back to Addressables.

### 2. Define the Model (optional)

```csharp
public class HomeScreenModel
{
    public int    Coins         { get; set; }
    public Action OnPlayClicked { get; set; }
}
```

### 3. Define the Presenter

```csharp
using GameFoundation.Scripts.Patterns.MVP.Attribute;
using GameFoundation.Scripts.Patterns.MVP.Implementation;
using Cysharp.Threading.Tasks;

[PresenterAttribute(isSingleton: false, autoInit: false)]
public class HomeScreenPresenter : ScreenPresenter<HomeScreenView, HomeScreenModel>
{
    public HomeScreenPresenter(
        IViewFactory                    viewFactory,
        UICanvas                        uiCanvas,
        IPublisher<OpenPresenterSignal> openPresenterPublisher,
        IPublisher<HidePresenterSignal> hidePresenterPublisher,
        IPublisher<OnButtonClickSignal> buttonClickPublisher
    ) : base(viewFactory, uiCanvas, openPresenterPublisher, hidePresenterPublisher, buttonClickPublisher) { }

    protected override void Ready()
    {
        // One-time setup after the view prefab is first instantiated
    }

    protected override void Bind()
    {
        // Runs on every Open() — safe place to read from model
        this.view.CoinText.text = this.model.Coins.ToString();
        this.view.PlayButton.onClick.AddListener(() => this.model.OnPlayClicked?.Invoke());
    }

    protected override async UniTask OnBeforeShow()
    {
        // Example: preload data with LifetimeToken
        // await this.dataService.FetchAsync(this.LifetimeToken);
    }

    protected override UniTask OnAfterHide() => UniTask.CompletedTask;
}
```

> **Button clicks**: The base presenter auto-wires a click-sound publisher on every `Button` in the view (`RebindButtonClickEffect`). It calls `RemoveAllListeners()` first, so add your own listeners in `Bind()` — don't add them in `Ready()` or they'll be wiped.

### 4. Show / Hide via `IScreenManager`

```csharp
private readonly IScreenManager screenManager;

// Fire-and-forget
screenManager.ShowScreen<HomeScreenPresenter, HomeScreenModel>(new HomeScreenModel
{
    Coins         = 100,
    OnPlayClicked = () => Debug.Log("Play!")
});

// Without model
screenManager.ShowScreen<HomeScreenPresenter>();

// Without animation
screenManager.ShowScreen<HomeScreenPresenter>(animate: false);

// Awaitable (wait until show animation finishes)
await screenManager.ShowScreenAsync<HomeScreenPresenter, HomeScreenModel>(model);

// Hide
screenManager.HideScreen<HomeScreenPresenter>();
await screenManager.HideScreenAsync<HomeScreenPresenter>(animate: false);

// Query
bool open    = screenManager.IsScreenOpen<HomeScreenPresenter>();
var  current = screenManager.GetScreen<HomeScreenPresenter>();

// Bulk
screenManager.HideAllScreens();
screenManager.HideAllScreens(PresenterType.Popup);
foreach (var p in screenManager.GetAllScreens(PresenterType.Screen, onlyOpened: true)) { }
```

## Attributes

- `[ViewAttribute(prefabPath, maxPoolSize = 5)]` — on the `BaseView` subclass. Pool bound is per-type.
- `[PresenterAttribute(isSingleton, autoInit)]` — on the presenter subclass.
  - `isSingleton: true` → same instance reused. `false` → new instance per resolve.
  - `autoInit: true` → VContainer builds it at container start.

All presenters with `[PresenterAttribute]` are auto-discovered by `MVPVContainer.RegisterMVP()`. **Do not register them manually.**

## DI Registration

Call once from your root `LifetimeScope`:

```csharp
builder.RegisterMVP();
```

This registers `IScreenManager`, `IViewFactory`, and all `[PresenterAttribute]`-annotated presenters.

## Animated Views (Popup / Splash)

`PopupView` and `SplashView` override `Show(bool)` / `Hide(bool)` as `async UniTask` and run DOTween sequences. Each serialized field (`showDuration`, `hideDuration`, `showEase`, `hideEase`, etc.) is tweakable in the inspector. Passing `animate: false` snaps to the target state without playing the sequence.

### SplashPresenter Extras

```csharp
protected UniTask PlaySplashAsync(
    Func<CancellationToken, UniTask> loadOperation,
    bool animateIn  = true,
    bool animateOut = true);

protected void CancelLoading();
```

`PlaySplashAsync` shows the splash, awaits your load operation with a cancellation-linked token, then hides the splash. Call `CancelLoading()` to abort.

## Do / Don't

- **Do** put one-time setup in `Ready()`, per-open wiring in `Bind()`.
- **Do** use `LifetimeToken` when awaiting inside `OnBeforeShow`/`OnAfterShow`/`OnBeforeHide`/`OnAfterHide`.
- **Do** use `ShowScreenAsync`/`HideScreenAsync` when you need to await animation completion.
- **Don't** call `Open()` and `Close()` directly on a presenter you resolved yourself — go through `IScreenManager` so state is tracked and lookup works.
- **Don't** manually register presenters in a `LifetimeScope` — `RegisterMVP()` handles it.
- **Don't** add `Button.onClick` listeners outside `Bind()` — they will be wiped on the next open.
