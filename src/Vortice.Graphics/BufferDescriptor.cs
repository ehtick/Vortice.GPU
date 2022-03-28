// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

namespace Vortice.Graphics;

/// <summary>
/// Structure that describes the <see cref="Buffer"/>.
/// </summary>
public record struct BufferDescriptor : IEquatable<BufferDescriptor>
{
    public BufferDescriptor(ulong size, BufferUsage usage = BufferUsage.ShaderReadWrite, CpuAccess access = CpuAccess.None, string? label = default)
    {
        Usage = usage;
        Size = size;
        Access = access;
        Label = label;
    }

    /// <summary>
    /// <see cref="BufferUsage"/> of <see cref="Buffer"/>.
    /// </summary>
    public BufferUsage Usage { get; init; }

    /// <summary>
    /// Size in bytes of <see cref="Buffer"/>
    /// </summary>
    public ulong Size { get; init; }

    /// <summary>
    /// CPU access of <see cref="Buffer"/>
    /// </summary>
    public CpuAccess Access { get; init; }

    /// <summary>
    /// Gets or sets the <see cref="Buffer"/> label.
    /// </summary>
    public string? Label { get; init; }
}
