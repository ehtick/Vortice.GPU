// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

namespace Vortice.Graphics;

/// <summary>
/// Defines a Graphics buffer.
/// </summary>
public abstract class Buffer : GraphicsResource
{
    protected Buffer(GraphicsDevice device, in BufferDescriptor descriptor)
        : base(device)
    {
        Usage = descriptor.Usage;
        Size = descriptor.Size;
        Access = descriptor.Access;
        SharedResourceFlags = descriptor.SharedResourceFlags;
        _label = descriptor.Label;
    }

    /// <summary>
    /// Gets the buffer usage (see <see cref="BufferUsage"/>).
    /// </summary>
    public BufferUsage Usage { get; }

    /// <summary>
    /// Gets the buffer size in bytes.
    /// </summary>
    public ulong Size { get; }

    /// <summary>
    /// Gets the buffer CPU access.
    /// </summary>
    public CpuAccess Access { get; }

    /// <summary>
    /// Gets the buffer shared flags.
    /// </summary>
    public SharedResourceFlags SharedResourceFlags { get; set; }
}
