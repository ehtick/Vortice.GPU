// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Runtime.CompilerServices;
using Microsoft.Toolkit.Diagnostics;

namespace Vortice.Graphics;

public abstract class GraphicsDevice : IDisposable
{
    private volatile int _isDisposed;

    protected GraphicsDevice()
    {
    }

    /// <summary>
    /// Releases unmanaged resources and performs other cleanup operations.
    /// </summary>
    ~GraphicsDevice()
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

    public static void Shutdown()
    {
        GPUDeviceHelper.Shutdown();
    }

    /// <summary>
    /// Verify whether given backend is supported.
    /// </summary>
    /// <param name="backend">The <see cref="BackendType"/> to check.</param>
    /// <returns>True if supported, false otherwise.</returns>
    public static bool IsBackendSupported(BackendType backend)
    {
        return GPUDeviceHelper.IsBackendSupported(backend);
    }

    /// <summary>
    ///  Create new instance of <see cref="GraphicsDevice"/> with given descriptor.
    /// </summary>
    /// <param name="descriptor">The optional <see cref="GraphicsDeviceDescriptor"/></param>
    /// <returns>New instance of <see cref="GraphicsDevice"/> or throws <see cref="GraphicsException"/></returns>
    public static GraphicsDevice Create(in GraphicsDeviceDescriptor? descriptor = default)
    {
        return GPUDeviceHelper.CreateDevice(descriptor);
    }

    /// <summary>
    /// Gets the <see cref="BackendType"/> of this device.
    /// </summary>
    public abstract BackendType BackendType { get; }

    /// <summary>
    /// Get the GPU adapter information, see <see cref="GraphicsAdapterInfo"/> for details.
    /// </summary>
    public abstract GraphicsAdapterInfo AdapterInfo { get; }

    /// <summary>
    /// Get the supported device features, see <see cref="GraphicsDeviceFeatures"/> for details.
    /// </summary>
    public abstract GraphicsDeviceFeatures Features { get; }

    /// <summary>
    /// Get the supported device limits, see <see cref="GraphicsDeviceLimits"/> for details.
    /// </summary>
    public abstract GraphicsDeviceLimits Limits { get; }

    /// <summary>
    /// Wait for GPU to finish pending operations.
    /// </summary>
    public abstract void WaitIdle();

    public Buffer CreateBuffer(in BufferDescriptor descriptor)
    {
        Guard.IsGreaterThanOrEqualTo(descriptor.Size, 1, nameof(Buffer.Size));

        return CreateBufferCore(descriptor, IntPtr.Zero);
    }

    public unsafe Buffer CreateBuffer<T>(Span<T> data, BufferUsage usage = BufferUsage.ShaderReadWrite) where T : unmanaged
    {
        BufferDescriptor descriptor = new((ulong)(data.Length * sizeof(T)), usage);
        fixed (T* dataPtr = data)
        {
            return CreateBufferCore(descriptor, (IntPtr)dataPtr);
        }
    }

    public Texture CreateTexture(in TextureDescriptor descriptor)
    {
        Guard.IsGreaterThanOrEqualTo(descriptor.Width, 1, nameof(TextureDescriptor.Width));
        Guard.IsGreaterThanOrEqualTo(descriptor.Height, 1, nameof(TextureDescriptor.Height));
        Guard.IsGreaterThanOrEqualTo(descriptor.DepthOrArraySize, 1, nameof(TextureDescriptor.DepthOrArraySize));

        return CreateTextureCore(descriptor);
    }

    protected abstract Buffer CreateBufferCore(in BufferDescriptor descriptor, IntPtr initialData);
    protected abstract Texture CreateTextureCore(in TextureDescriptor descriptor);
}
