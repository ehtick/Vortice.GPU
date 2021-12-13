// Copyright (c) Amer Koleci and contributors.
// Distributed under the MIT license. See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace Vortice.GPU;

public abstract class GPUDevice : IDisposable
{
    private volatile int _isDisposed;

    protected GPUDevice()
    {
    }

    /// <summary>
    /// Releases unmanaged resources and performs other cleanup operations.
    /// </summary>
    ~GPUDevice()
    {
        if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) == 0)
        {
            OnDispose();
        }
    }

    // <summary>
    /// Gets whether or not the current instance has already been disposed.
    /// </summary>
    public bool IsDisposed
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            return _isDisposed != 0;
        }
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        OnDispose();
        GC.SuppressFinalize(this);
    }

    protected abstract void OnDispose();

    /// <summary>
    /// Throws an <see cref="ObjectDisposedException" /> if the current instance has been disposed.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void ThrowIfDisposed()
    {
        if (IsDisposed)
        {
            throw new ObjectDisposedException(ToString());
        }
    }

    /// <summary>
    /// Gets or sets the preferred <see cref="GPUBackend"/> to use.
    /// </summary>
    public static GPUBackend PreferredBackend { get; set; } = GPUBackend.Count;

    /// <summary>
    /// Gets or sets the <see cref="GPU.ValidationMode"/> to use.
    /// </summary>
    public static ValidationMode ValidationMode { get; set; } = ValidationMode.Disabled;

    /// <summary>
    /// Gets the best supported <see cref="GPUDevice"/> of the current OS.
    /// </summary>
    public static GPUDevice Default => GPUDeviceHelper.DefaultFactory.Value;

    /// <summary>
    /// Wait for GPU to finish pending operations.
    /// </summary>
    public abstract void WaitIdle();
}
