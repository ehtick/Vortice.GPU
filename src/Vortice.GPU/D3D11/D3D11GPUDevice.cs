// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Toolkit.Diagnostics;
using Vortice.Direct3D;
using static Vortice.GPU.D3DUtils;
using static Vortice.Direct3D11.D3D11;
using Vortice.DXGI;

namespace Vortice.GPU.D3D11;

internal class D3D11GPUDevice : GPUDevice
{
    public D3D11GPUDevice(IDXGIAdapter1 adapter)
    {
        Guard.IsNotNull(adapter, nameof(adapter));

        Adapter = adapter;
        //NativeDevice = D3D12CreateDevice<ID3D12Device2>(adapter, FeatureLevel.Level_11_0);
    }

    public IDXGIAdapter1 Adapter { get; }

    /// <inheritdoc />
    protected override void OnDispose()
    {
        Adapter.Dispose();
    }
}
