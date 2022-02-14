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
    Vertex = 1 << 0,
    Index = 1 << 1,
    Uniform = 1 << 2,
    ShaderRead = 1 << 3,
    ShaderWrite = 1 << 4,
    ShaderReadWrite = ShaderRead | ShaderWrite,
    Indirect = 1 << 5,
}
