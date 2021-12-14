// Copyright (c) Amer Koleci and contributors.
// Distributed under the MIT license. See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;
using Microsoft.Toolkit.Diagnostics;

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

    public static GPUDevice Create(in GPUDeviceDescriptor descriptor)
    {
        return GPUDeviceHelper.CreateDevice(descriptor);
    }

    /// <summary>
    /// Get the device information, see <see cref="GPUDeviceInfo"/> for details.
    /// </summary>
    public abstract GPUDeviceInfo Info { get; }

    /// <summary>
    /// Get the GPU adapter information, see <see cref="GPUAdapterInfo"/> for details.
    /// </summary>
    public abstract GPUAdapterInfo AdapterInfo { get; }

    /// <summary>
    /// Wait for GPU to finish pending operations.
    /// </summary>
    public abstract void WaitIdle();

    public Texture CreateTexture(in TextureDescriptor descriptor)
    {
        Guard.IsGreaterThanOrEqualTo(descriptor.Width, 1, nameof(TextureDescriptor.Width));
        Guard.IsGreaterThanOrEqualTo(descriptor.Height, 1, nameof(TextureDescriptor.Height));
        Guard.IsGreaterThanOrEqualTo(descriptor.DepthOrArraySize, 1, nameof(TextureDescriptor.DepthOrArraySize));

        return CreateTextureCore(descriptor);
    }

    protected abstract Texture CreateTextureCore(in TextureDescriptor descriptor);
}
