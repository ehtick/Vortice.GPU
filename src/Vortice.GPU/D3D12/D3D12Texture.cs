// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using Vortice.Direct3D12;
using static Vortice.Direct3D12.D3D12;

namespace Vortice.GPU.D3D12;

internal unsafe class D3D12Texture : Texture
{
    public D3D12Texture(D3D12GPUDevice device, in TextureDescriptor descriptor)
        : base(device, descriptor)
    {
    }

    public ID3D12Resource Handle { get; }
}
