// Copyright (c) Amer Koleci and contributors.
// Distributed under the MIT license. See the LICENSE file in the project root for more information.

using Vortice.Direct3D12;
using static Vortice.Direct3D12.D3D12;

namespace Vortice.GPU.Direct3D12;

public class D3D12Device : Device
{
    public ID3D12Device2 NativeDevice { get; }

    protected override void OnDispose() => throw new NotImplementedException();
}
