// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

namespace Vortice.Graphics;

/// <summary>
/// A bitmask indicating how a <see cref="Buffer"/> is permitted to be used.
/// </summary>
[Flags]
public enum BufferUsage
{
    None = 0,
    /// <summary>
    /// Supports input assembly access as VertexBuffer.
    /// </summary>
    Vertex = 1 << 0,
    /// <summary>
    /// Supports input assembly access as IndexBuffer.
    /// </summary>
    Index = 1 << 1,
    /// <summary>
    /// Supports constant buffer access.
    /// </summary>
    Constant = 1 << 2,
    ShaderRead = 1 << 3,
    ShaderWrite = 1 << 4,
    ShaderReadWrite = ShaderRead | ShaderWrite,
    Indirect = 1 << 5,
}
