# Global Events

[![Unity 6000.3+](https://img.shields.io/badge/Unity-6000.3%2B-black?logo=unity)](https://unity.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

A low-allocation global event system for Unity.

Global Events lets you connect classes through a lightweight, event-based architecture. Instead of wiring up direct references between systems, components raise and listen to global events, which keeps your code decoupled, easier to test and easier to extend.

## Features

- **Decoupled architecture** – systems communicate through events instead of direct references.
- **Allocation-free synchronous events** – raising and handling synchronous events produces no garbage, keeping the GC quiet during gameplay.
- **Async support** – async events are built on [UniTask](https://github.com/Cysharp/UniTask) to keep overhead low. Async code does allocate, so use synchronous events on hot paths where every allocation counts.
- **Simple API** – minimal setup to start sending and receiving events.

## Requirements

- Unity **6000.3** or newer
- [UniTask](https://github.com/Cysharp/UniTask) **2.5.11** or newer

## Installation

### 1. Install UniTask

Global Events depends on UniTask. Unity does not resolve dependencies of Git-based packages automatically, so install UniTask first.

Open **Window → Package Manager**, click the **+** button, choose **Add package from git URL...** and enter:

```
https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask
```

### 2. Install Global Events

In the Package Manager, click **+ → Add package from git URL...** and enter:

```
https://github.com/Wezzel-NL/GlobalEvents.git
```

### Alternative: edit `manifest.json`

Add both packages to the `dependencies` block of `Packages/manifest.json` in your project:

```json
{
  "dependencies": {
    "com.cysharp.unitask": "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask",
    "com.wezzelnl.globalevents": "https://github.com/Wezzel-NL/GlobalEvents.git"
  }
}
```

To lock to a specific version, append a tag or commit hash, for example `https://github.com/Wezzel-NL/GlobalEvents.git#1.0.0`.

## Usage

<!-- TODO: Add code samples -->

### Defining an event

```csharp
//Normal (synchronous)

public struct MyEvent : IGlobalEvent
{
    public int Parameter { get; }

    public MyEvent(int parameter)
    {
        Paramter = parameter;
    }
}


//Async (UniTask)

public struct MyAsyncEvent : IAsyncGlobalEvent
{
    public int Parameter { get; }

    public MyAsyncEvent(int parameter)
    {
        Paramter = parameter;
    }
}
```

### Listening to an event

```csharp
//Normal (synchronous)
public class Example : MonoBehaviour
{
    void OnEnable()
    {
        GlobalEvent.AddListener<MyEvent>(MyEventListener);
    }

    void OnDisable()
    {
        GlobalEvent.RemoveListener<MyEvent>(MyEventListener);
    }
    
    void MyEventListener(in MyEvent evt)
    {
        Debug.Log($"Event raised! Parameter: {evt.Parameter}");
    }
}

//Async (UniTask)
public class Example : MonoBehaviour
{
    private void OnEnable()
    {
        GlobalEvent.AddAsyncListener<MyAsyncEvent>(MyEventListener);
    }

    private void OnDisable()
    {
        GlobalEvent.RemoveAsyncListener<MyAsyncEvent>(MyEventListener);
    }
    
    private async UniTask MyEventListener(MyAsyncEvent evt, CancellationToken ct)
    {
        Debug.Log($"Event raised! Waiting for {evt.Parameter} seconds!");
        await UniTask.Delay(TimeSpan.FromSeconds(evt.Parameter), cancellationToken: ct);
        Debug.Log("Event completed!");
    }
}
```

### Raising an event

```csharp
//Normal (synchronous)
public class Example : MonoBehaviour
{
    public void InvokeFromSyncMethod()
    {
        var parameter = 1;
        var evt = new MyEvent(parameter);
        GlobalEvent.Invoke(evt);
    }
}

//Async (UniTask)
public class Example : MonoBehaviour
{
    public void InvokeFromSyncMethod()
    {
        var parameter = 1;
        var evt = new MyAsyncEvent(parameter);
        GlobalEvent.InvokeAsync(evt, destroyCancellationToken).Forget();
    }
    
    public async UniTask InvokeFromAsyncMethod(CancellationToken ct)
    {
        var parameter = 1;
        var evt = new MyAsyncEvent(parameter);
        await GlobalEvent.InvokeAsync(evt, ct);
    }
}
```

## Contributing

Issues and pull requests are welcome. If you find a bug or have a feature request, please [open an issue](https://github.com/Wezzel-NL/GlobalEvents/issues).

## License

This project is licensed under the [MIT License](LICENSE).

© WezzelNL