# GameFoundation MVP System

## Overview
This MVP (Model-View-Presenter) system provides a robust architecture for creating UI screens and game components in Unity. It uses VContainer for dependency injection and Addressables for asset loading.

## Key Features
- 🏭 **Auto-registration** of presenters via reflection
- 📦 **Addressable loading** of view prefabs
- ♻️ **Object pooling** for efficient view reuse
- 🏷️ **Attribute-based** configuration
- 💉 **Dependency injection** via VContainer
- 🔄 **Lifecycle management** (Initialize, Show, Hide)

## Getting Started

### 1. Create a View
Views represent the visual elements of your UI. They should inherit from `BaseView` and use the `ViewAttribute` to specify the prefab path:

```csharp
using GameFoundation.Scripts.Patterns.MVP.Attribute;
using GameFoundation.Scripts.Patterns.MVP.View;
using UnityEngine;
using UnityEngine.UI;

[ViewAttribute("UI/MyView")]
public class MyView : BaseView
{
    [SerializeField] private Button button;
    
    public Button Button => this.button;
    
    public override void Initialize()
    {
        base.Initialize();
        // Additional initialization
    }
}
```

### 2. Create a Model (Optional)
Models contain the data for your UI:

```csharp
public class MyModel
{
    public string Title { get; set; }
    public Action OnButtonClicked { get; set; }
}
```

### 3. Create a Presenter
Presenters contain the logic for your UI. They can be auto-registered using the `PresenterAttribute`:

```csharp
using GameFoundation.Scripts.Patterns.MVP.Attribute;
using GameFoundation.Scripts.Patterns.MVP.Presenter;
using VContainer;

[PresenterAttribute(isSingleton: false, autoInit: false)]
public class MyPresenter : BasePresenter<MyView, MyModel>
{
    [Inject]
    public MyPresenter(IViewFactory viewFactory) : base(viewFactory)
    {
    }
    
    protected override void OnBeforeShow()
    {
        base.OnBeforeShow();
        
        if (this.model != null)
        {
            // Set up the view with model data
            this.view.Button.onClick.AddListener(OnButtonClicked);
        }
    }
    
    protected override void OnBeforeHide()
    {
        base.OnBeforeHide();
        this.view.Button.onClick.RemoveListener(OnButtonClicked);
    }
    
    private void OnButtonClicked()
    {
        this.model?.OnButtonClicked?.Invoke();
    }
}
```

### 4. Setup VContainer Registration
In your VContainer configuration, use the extension method to register all MVP components:

```csharp
public class MyLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // Register AssetsManager for Addressables
        builder.Register<AssetsManager>(Lifetime.Singleton).AsImplementedInterfaces();
        
        // Register all MVP components (including auto-discovered presenters)
        builder.RegisterMVP();
        
        // Register other components
        builder.RegisterComponent(this.gameObject.GetComponent<MyComponent>());
    }
}
```

### 5. Show a Screen
Use the ScreenManager to show and hide screens:

```csharp
// Direct approach
var model = new MyModel
{
    Title = "Hello World",
    OnButtonClicked = () => Debug.Log("Button clicked!")
};

var presenter = resolver.Resolve<MyPresenter>();
presenter.SetModel(model);
screenManager.ShowScreen<MyPresenter>();

// Or using the helper extensions
lifetimeScope.ShowScreen<MyPresenter, MyModel>(model);
```

## Advanced Features

### Presenter Attributes
The `PresenterAttribute` provides additional configuration:

```csharp
[PresenterAttribute(isSingleton: true, autoInit: true)]
```

- **isSingleton**: If true, the presenter will be registered as a singleton
- **autoInit**: If true, the presenter will be automatically initialized at startup

### View Pooling
Views are automatically pooled for reuse. When you're done with a view, use:

```csharp
screenManager.DestroyScreen<MyPresenter>();
```

### Helper Methods
The `MVPHelper` class provides various utility methods for working with the MVP system, including extension methods for LifetimeScope.

## Example Usage
See the `Examples` folder for sample implementations. 