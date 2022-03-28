// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using Vortice.Direct3D12;
using static Vortice.Direct3D12.D3D12;

namespace Vortice.Graphics.D3D12;

internal class D3D12Buffer : Buffer
{
    public D3D12Buffer(D3D12GraphicsDevice device, in BufferDescriptor descriptor, IntPtr initialData)
        : base(device, descriptor)
    {
    }

    public ID3D12Resource Handle { get; }

    protected override void OnLabelChanged()
    {
        Handle.Name = _label ?? string.Empty;
    }
}
