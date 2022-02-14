// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

namespace Vortice.Graphics.Samples;

public abstract class Application : IDisposable
{
    private readonly AppPlatform _platform;

    public event EventHandler<EventArgs>? Disposed;


    protected Application(GPUBackend preferredBackend = GPUBackend.Count, ValidationMode validationMode = ValidationMode.Disabled)
    {
        _platform = AppPlatform.Create(this);
        _platform.Activated += OnPlatformActivated;
        _platform.Deactivated += OnPlatformDeactivated;

        GPUDeviceDescriptor descriptor = new()
        {
            PreferredBackend = preferredBackend,
            ValidationMode = validationMode
        };

        GraphicsDevice = GraphicsDevice.Create(descriptor);
    }

    public bool IsDisposed { get; private set; }
    public Window MainWindow => _platform.MainWindow;

    public bool IsActive => _platform.IsActive;

    /// <summary>
    /// Gets the <see cref="GraphicsDevice"/> used for rendering.
    /// </summary>
    public GraphicsDevice GraphicsDevice { get; }

    public event EventHandler<EventArgs>? Activated;

    public event EventHandler<EventArgs>? Deactivated;

    ~Application()
    {
        Dispose(dispose: false);
    }

    public void Dispose()
    {
        Dispose(dispose: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool dispose)
    {
        if (dispose && !IsDisposed)
        {
            GraphicsDevice?.Dispose();
            Disposed?.Invoke(this, EventArgs.Empty);
            IsDisposed = true;
        }
    }

    public void Run()
    {
        _platform.Run();
    }

    public void Tick()
    {
    }

    internal void InitBeforeRun()
    {
        Initialize();
    }

    protected virtual void Initialize()
    {
    }

    private void OnPlatformActivated(object? sender, EventArgs e)
    {
        Activated?.Invoke(this, EventArgs.Empty);
    }

    private void OnPlatformDeactivated(object? sender, EventArgs e)
    {
        Deactivated?.Invoke(this, EventArgs.Empty);
    }
}
