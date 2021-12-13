// Copyright (c) Amer Koleci and contributors.
// Distributed under the MIT license. See the LICENSE file in the project root for more information.

using Vortice.Direct3D12;
using static Vortice.Direct3D12.D3D12;

namespace Vortice.GPU.D3D12;

internal unsafe class D3D12Texture : Texture
{
    public D3D12Texture(D3D12GPUDevice device) : base(device)
    {
    }

    public ID3D12Resource Handle { get; }
}
