// Copyright (c) Amer Koleci and contributors.
// Distributed under the MIT license. See the LICENSE file in the project root for more information.

using Microsoft.Toolkit.Diagnostics;
using Vortice.Direct3D;
using Vortice.Direct3D12;
using Vortice.Direct3D12.Debug;
using Vortice.DXGI;
using static Vortice.Direct3D12.D3D12;

namespace Vortice.GPU.D3D12;

internal unsafe class D3D12GPUDevice : GPUDevice
{
    public D3D12GPUDevice(IDXGIAdapter1 adapter)
    {
        Guard.IsNotNull(adapter, nameof(adapter));

        if (ValidationMode != ValidationMode.Disabled)
        {
            if (D3D12GetDebugInterface(out ID3D12Debug? debugController).Success)
            {
                debugController!.EnableDebugLayer();

                if (ValidationMode == ValidationMode.GPU)
                {
                    ID3D12Debug1? debugController1 = debugController.QueryInterfaceOrNull<ID3D12Debug1>();
                    if (debugController1 != null)
                    {
                        debugController1.SetEnableGPUBasedValidation(true);
                        debugController1.SetEnableSynchronizedCommandQueueValidation(true);
                        debugController1.Dispose();
                    }

                    ID3D12Debug2? debugController2 = debugController.QueryInterfaceOrNull<ID3D12Debug2>();
                    if (debugController1 != null)
                    {
                        debugController2.SetGPUBasedValidationFlags(GpuBasedValidationFlags.None);
                        debugController2.Dispose();
                    }
                }

                debugController.Dispose();
            }
            else
            {
                //Log.Debug("WARNING: Direct3D Debug Device is not available\n");
            }
        }


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
