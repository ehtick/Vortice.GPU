// Copyright (c) Amer Koleci and contributors.
// Distributed under the MIT license. See the LICENSE file in the project root for more information.

using Microsoft.Toolkit.Diagnostics;
using Vortice.Direct3D;
using Vortice.Direct3D12;
using Vortice.DXGI;
using static Vortice.Direct3D12.D3D12;

namespace Vortice.GPU.D3D12;

internal unsafe class D3D12GPUDevice : GPUDevice
{
    public D3D12GPUDevice(IDXGIAdapter1 adapter)
    {
        Guard.IsNotNull(adapter, nameof(adapter));

        Adapter = adapter;
        NativeDevice = D3D12CreateDevice<ID3D12Device2>(adapter, FeatureLevel.Level_11_0);
    }

    public IDXGIAdapter1 Adapter { get; }

    public ID3D12Device2 NativeDevice { get; }

    /// <inheritdoc />
    protected override void OnDispose()
    {
        Adapter.Dispose();
    }
}
